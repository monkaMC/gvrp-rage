using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Threading.Tasks;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Module.Players.Db;
using GVRP.Module.Players;
using GVRP.Module.Spawners;
using GVRP.Module.Teams;
using GVRP.Module.Configurations;
using GVRP.Module.Houses;
using GVRP.Module.Doors;

namespace GVRP.Module.Players.JumpPoints
{
    public class JumpPoint
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public float Heading { get; set; }
        public uint Dimension { get; set; }
        public DimensionType DimensionType { get; set; }
        public int DestinationId { get; set; }
        public HashSet<Team> Teams { get; set; }
        public bool Locked { get; set; }
        public bool InsideVehicle { get; set; }
        public float Range { get; set; }
        public GTANetworkAPI.Object Object { get; set; }
        public ColShape ColShape { get; set; }
        public DateTime LastBreak { get; set; }
        public bool Unbreakable { get; set; }
        public bool AdminUnbreakable { get; set; }
        public JumpPoint Destination { get; set; }
        public bool EnterOnColShape { get; set; }
        public bool HideInfos { get; set; }
        public bool Disabled { get; set; }

        public bool DisableInfos { get; set; }
        public HashSet<uint> Houses { get; }

        public int RangRestriction { get; set; }


        public List<LastUsedFrom> LastUseds { get; set; }

        public JumpPoint(MySqlDataReader reader)
        {
            Id = reader.GetInt32("id");
            Name = reader.GetString("name");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            Heading = reader.GetFloat("heading");
            Dimension = (uint)reader.GetInt32("dimension");
            DimensionType = (DimensionType)reader.GetInt32("dimension_type");
            DestinationId = reader.GetInt32("destionation");
            RangRestriction = reader.GetInt32("rangrestriction");
            Disabled = reader.GetInt32("disabled") == 1;
            HideInfos = reader.GetInt32("hide_infos") == 1;
            var teamsString = reader.GetString("teams");
            var teams = new HashSet<Team>();
            LastUseds = new List<LastUsedFrom>();
            if (!string.IsNullOrEmpty(teamsString))
            {
                var splittedTeams = teamsString.Split(',');
                foreach (var splittedTeam in splittedTeams)
                {
                    if (!uint.TryParse(splittedTeam, out var teamId) || teams.Contains(TeamModule.Instance.Get(teamId))) return;
                    teams.Add(TeamModule.Instance.Get(teamId));
                }
            }

            var housestring = reader.GetString("houses");
            Houses = new HashSet<uint>();
            if (!string.IsNullOrEmpty(housestring))
            {
                var splittedHouses = housestring.Split(',');
                foreach (var houseIdString in splittedHouses)
                {
                    if (!uint.TryParse(houseIdString, out var houseid)) continue;
                    Houses.Add(houseid);
                }
            }

            LastBreak = DateTime.Now.Add(new TimeSpan(0, -5, 0)); // set lastbreak for load now -5 min

            Teams = teams;
            InsideVehicle = reader.GetInt32("inside_vehicle") == 1;
            Range = reader.GetFloat("range");
            Locked = reader.GetBoolean("locked");
            ColShape = ColShapes.Create(Position, Range, Dimension);
            ColShape.SetData("jumpPointId", Id);
            Unbreakable = reader.GetInt32("unbreakable") == 1;
            AdminUnbreakable = reader.GetInt32("unbreakable") == 2;
            EnterOnColShape = reader.GetInt32("colshape") == 1;
        }

        public JumpPoint()
        {

        }

        public bool TravelThrough(DbPlayer player)
        {
            if (Locked || Disabled) return false;

            if (Id == 216 || Id == 215)
            {
                if (player.hasPerso[0] == 0) return false; // Only Use with perso
            }

            if (!InsideVehicle || !player.Player.IsInVehicle)
            {
                player.Player.Dimension = Destination.Dimension;
                player.DimensionType[0] = Destination.DimensionType;

                if(Destination.Dimension != 0)
                {
                    if ((int)Destination.DimensionType == 10)
                    {
                        int boilerState = 2;
                        int tableState = 1;
                        int securityState = 1;

                        player.Player.TriggerEvent("loadMethInterior", tableState, boilerState, securityState);
                    }
                    Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                    {
                        if (Laboratories.MethlaboratoryModule.Instance.GetAll().Where(lab => lab.Value.JumpPointAusgang.Id.Equals(DestinationId)).Count() > 0)
                        {
                            player.SetData("inMethLaboraty", true);

                        }

                        Vector3 PortPosition = new Vector3(Destination.Position.X, Destination.Position.Y, Destination.Position.Z - 15.0f);

                        player.SetData("lastPosition", player.Player.Position);
                        player.Player.TriggerEvent("freezePlayer", true);
                        await Task.Delay(500);
                        player.Player.SetPosition(PortPosition);
                        player.Player.SetRotation(Destination.Heading);
                        await Task.Delay(2000);
                        player.Player.SetPosition(Destination.Position);
                        player.Player.SetRotation(Destination.Heading);
                        player.Player.TriggerEvent("freezePlayer", false);
                    }));
                }
                else
                {
                    Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
                    {
                        if (Laboratories.MethlaboratoryModule.Instance.GetAll().Where(lab => lab.Value.JumpPointAusgang.Id.Equals(DestinationId)).Count() > 0 && player.HasData("inMethLaboraty"))
                        {
                            player.ResetData("inMethLaboraty");
                        }

                        player.Player.SetPosition(Destination.Position);
                        player.Player.SetRotation(Destination.Heading);
                        player.ResetData("lastPosition");
                    }));
                }
            }
            else
            {
                var vehicle = player.Player.Vehicle;
                vehicle.Dimension = Destination.Dimension;

                foreach (var occupant in vehicle.Occupants)
                {
                    occupant.Dimension = Destination.Dimension;
                }

                if (Destination.Dimension != 0)
                {
                    Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                    {
                        player.SetData("lastPosition", player.Player.Position);
                        player.Player.TriggerEvent("freezePlayer", true);
                        vehicle.Rotation = new Vector3(0, 0, Destination.Heading);
                        vehicle.Position = Destination.Position;
                        await Task.Delay(1000);
                        vehicle.Rotation = new Vector3(0, 0, Destination.Heading);
                        vehicle.Position = Destination.Position;
                        await Task.Delay(1500);
                        player.Player.TriggerEvent("freezePlayer", false);
                    }));
                }
                else
                {
                    Main.m_AsyncThread.AddToAsyncThread(new Task(async() =>
                    {
                        player.Player.TriggerEvent("freezePlayer", true);
                        vehicle.Rotation = new Vector3(0, 0, Destination.Heading);
                        vehicle.Position = Destination.Position;
                        await Task.Delay(1000);
                        vehicle.Rotation = new Vector3(0, 0, Destination.Heading);
                        vehicle.Position = Destination.Position;
                        await Task.Delay(1500);
                        player.Player.TriggerEvent("freezePlayer", false);
                        player.ResetData("lastPosition");
                    }));
                }
            }
            return true;
        }

        public bool CanOpen(DbPlayer dbPlayer)
        {
            if (Houses.Contains(HouseModule.Instance.Get(dbPlayer.ownHouse[0]).Id)) return true;
            foreach (uint houseId in Houses)
            {
                if (dbPlayer.IsTenant() && dbPlayer.GetTenant().HouseId == houseId) return true;
                if (dbPlayer.HouseKeys.Contains(houseId)) return true;
            }

            return false;
        }

        public bool CanInteract(DbPlayer dbPlayer)
        {
            if (Disabled) return false;
            if ((Teams.Contains(dbPlayer.Team) && dbPlayer.TeamRank >= RangRestriction) ||
                dbPlayer.Rank.CanAccessFeature("enter_all") ||
                (Houses.Count > 0 && CanOpen(dbPlayer))) return true;
            return false;
        }

        public bool ToggleLock(DbPlayer player)
        {
            if (!CanInteract(player)) return false;

            if (LastBreak.AddMinutes(5) > DateTime.Now) return false; // Bei einem Break, kann 5 min nicht interagiert werden

            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                Locked = !Locked;
                Destination.Locked = !Destination.Locked;
                if (Locked)
                {
                    player.SendNewNotification("Tuer abgeschlossen", title: "Tür", notificationType: PlayerNotification.NotificationType.ERROR);
                }
                else
                {
                    player.SendNewNotification("Tuer aufgeschlossen", title: "Tür", notificationType: PlayerNotification.NotificationType.SUCCESS);
                }

                // Add
                LastUseds.Add(new LastUsedFrom() { Name = player.GetName(), DateTime = DateTime.Now, Opened = !Locked });
            }));

            return true;
        }
    }
}
using System;
using System.Collections.Generic;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Module.Houses;
using GVRP.Module.Players.Db;
using GVRP.Module.Spawners;
using GVRP.Module.Teams;

namespace GVRP.Module.Doors
{
    public class LastUsedFrom
    {
        public string Name { get; set; }
        public DateTime DateTime { get; set; }
        public bool Opened { get; set; }
    }

    public class Door : Loadable<uint>
    {
        public uint Id { get; }
        public Vector3 Position { get; }
        public long Model { get; }
        public HashSet<Team> Teams { get; }
        public HashSet<uint> Houses { get; }
        public bool Locked { get; set; }
        public string Name { get; }
        public uint Pair { get; }
        public float Range { get; }
        public ColShape ColShape { get; set; }
        public DateTime LastBreak { get; set; }
        public bool OpenWithWelding { get; }
        public bool OpenWithHacking { get; }

        public bool LessSecurity { get; set; }
        public DateTime LessSecurityChanged { get; set; }
        public List<LastUsedFrom> LastUseds { get; set; }

        public int RangRestriction { get; }
        public int Group { get; set; }
        public bool AdminUnbreakable { get; }
        public Door(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            Model = reader.GetInt64("model");
            Group = reader.GetInt32("group");
            var teamString = reader.GetString("team");
            RangRestriction = reader.GetInt32("rangrestriction");

            LessSecurity = false;
            LessSecurityChanged = DateTime.Now.AddMinutes(-5);

            LastUseds = new List<LastUsedFrom>();
            Teams = new HashSet<Team>();
            if (!string.IsNullOrEmpty(teamString))
            {
                var splittedTeams = teamString.Split(',');
                foreach (var teamIdString in splittedTeams)
                {
                    if (!uint.TryParse(teamIdString, out var teamId) || teamId == 0|| Teams.Contains(TeamModule.Instance[teamId])) continue;
                    Teams.Add(TeamModule.Instance[teamId]);
                }
            }

            LastBreak = DateTime.Now.Add(new TimeSpan(0, -5, 0)); // set lastbreak for load now -5 min

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
            
            Locked = reader.GetBoolean("locked");
            Name = reader.GetString("name");
            Pair = reader.GetUInt32("pair");
            Range = reader.GetFloat("range");
            OpenWithWelding = reader.GetInt32("unbreakable") == 1;
            OpenWithHacking = reader.GetInt32("unbreakable") == 2;
            AdminUnbreakable = reader.GetInt32("unbreakable") == 3;

            CreateColshape();
        }

        public void Break()
        {
            SetLocked(false);
            LastBreak = DateTime.Now;
            if (Pair != 0)
            {
                if (DoorModule.Instance.Get(Pair) != null)
                {
                    DoorModule.Instance.Get(Pair).LastBreak = DateTime.Now;
                }
            }
        }

        private void CreateColshape()
        {
            ColShape = ColShapes.Create(Position, Range);
            ColShape.SetData("doorId", Id);
        }

        public override uint GetIdentifier()
        {
            return Id;
        }

        public void SetLocked(bool locked = true)
        {
            Locked = locked;
            if (Pair > 0)
            {
                var pair = DoorModule.Instance.Get(Pair);
                if (pair != null)
                {
                    pair.Locked = locked;
                    pair.Refresh();
                }
            }
            Refresh();
            return;
        }

        public void Refresh()
        {
            foreach(DbPlayer player in Players.Players.Instance.GetValidPlayers())
            {
                if(player.Player.Position.DistanceTo(Position) < Range*3) // Update for players in Range * 3
                {
                    //TriggerEvent does not accept int64 -> Workaround. Range between -2147483648 and 4294967295 allowed.
                    if (Model >= 0)
                        player.Player.TriggerEvent("setStateOfClosestDoorOfType", (uint)Model, Position.X, Position.Y, Position.Z, Locked, 0, false);
                    else
                        player.Player.TriggerEvent("setStateOfClosestDoorOfType", (int)Model, Position.X, Position.Y, Position.Z, Locked, 0, false);
                }
            }
        }
    }
}
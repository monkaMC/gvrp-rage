using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Handler;
using GVRP.Module.Configurations;
using GVRP.Module.Gangwar;
using GVRP.Module.GTAN;
using GVRP.Module.Helper;
using GVRP.Module.Houses;
using GVRP.Module.Logging;
using GVRP.Module.NpcSpawner;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Spawners;
using GVRP.Module.Vehicles.Data;

namespace GVRP.Module.Vehicles.Garages
{
    public class Garage : Loadable<uint>
    {
        public uint Id { get; }
        public Vector3 Position { get; }
        public float Heading { get; }
        public string Name { get; }

        public HashSet<uint> Teams { get; }
        
        public GarageType Type { get; }
        public List<GarageSpawn> Spawns { get; }
        public int Rang { get; }
        public bool Marker { get; }
        public uint HouseId { get; }
        public int Slots { get; }
        public PedHash Npc { get; }
        public HashSet<uint> Classifications { get; }
        public Blip Blip { get; set; }
        public int GangwarTownId { get; set; }
        public int dimension { get; set; }
        public int Radius { get; set; }
        public bool PlanningGarage { get; set; }
        public int PublicTeamRestriction { get; set; }

        public bool DisableInfos { get; set; }

        public bool DisableAutomaticCarInsertion { get; set; }

        public Garage(MySqlDataReader reader) : base(reader)
        {
            if (!Enum.TryParse(reader.GetInt32(6).ToString(), out GarageType type))
            Logger.Crash(new Exception($"Unknown garage type {reader.GetInt32(6)}"));

            Id = reader.GetUInt32("id");
            Position = new Vector3(reader.GetFloat("npc_pos_x"), reader.GetFloat("npc_pos_y"), reader.GetFloat("npc_pos_z"));
            Heading = reader.GetFloat("npc_heading");
            Name = reader.GetString("name");
            
            var teamString = reader.GetString("team_id");
            Teams = new HashSet<uint>();
            if (!string.IsNullOrEmpty(teamString))
            {
                var splittedTeams = teamString.Split(',');
                foreach (var teamIdString in splittedTeams)
                {
                    if (!uint.TryParse(teamIdString, out var teamId) || teamId == 0 || Teams.Contains(teamId)) continue;
                    Teams.Add(teamId);
                }
            }

            Type = type;
            Spawns = new List<GarageSpawn>();

            if (Position.X != 0 && Position.Y != 0)
            {
                Npc = Enum.TryParse(reader.GetString("ped_hash"), true, out PedHash skin) ? skin : PedHash.Autoshop02SMM;
            }

            Rang = reader.GetInt32("rang");
            Marker = reader.GetInt32("marker") == 1;
            HouseId = reader.GetUInt32("house_id");
            Slots = reader.GetInt32("slots");
            GangwarTownId = reader.GetInt32("gangwar_towns_id");
            DisableInfos = reader.GetInt32("hide_infos") == 1;
            dimension = reader.GetInt32("dimension");
            PublicTeamRestriction = reader.GetInt32("public_team_restriction");
            Radius = reader.GetInt32("radius");
            if (Radius < 5 || Radius > 100)
                Radius = 25;
            PlanningGarage = reader.GetInt32("planning_garage") == 1;
            DisableAutomaticCarInsertion = reader.GetInt32("park_script_stop") == 1;
            Classifications = new HashSet<uint>();
            var classificationsString = reader.GetString("classifications");
            if (!string.IsNullOrEmpty(classificationsString))
            {
                var splittedClassifications = classificationsString.Split(',');
                foreach (var classificationString in splittedClassifications)
                {
                    if (!uint.TryParse(classificationString, out var classificationId)) continue;
                    if (VehicleClassificationModule.Instance[classificationId] != null)
                    {
                        Classifications.Add(classificationId);
                    }
                    else
                    {
                        Logger.Print($"Invalid garage classificatin id {classificationId}");
                    }
                }
            }

            if (HouseId == 0)
            {
                var colShape = ColShapes.Create(Position, 7f);
                colShape.SetData("garageId", Id);
                // Register NPC if public Garage
                if(!DisableInfos && Position.X != 0 && Position.Y != 0) new Npc(Npc, Position, Heading, 0);
                //NAPI.TextLabel.CreateTextLabel(Name, Position.Add(new Vector3(0, 0, 1.1d)), 18, 1.5f, 0, new Color(230, 123, 0), true, (uint)dimension);
            }
        }

        public override uint GetIdentifier()
        {
            return Id;
        }

        public bool IsTeamGarage()
        {
            return Teams.Count() > 0 || GangwarTownId > 0;
        }

        public bool IsPlanningGarage()
        {
            return PlanningGarage;
        }

        public bool CanVehiclePutIntoHouseGarage()
        {
            if(HouseId > 0)
            {
                House house = HouseModule.Instance.Get(HouseId);
                if(house != null)
                {
                    var count = 0;
                    var query = $"SELECT COUNT(*) FROM `vehicles` WHERE garage_id = '{Id}'";
                    using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                    using (var cmd = conn.CreateCommand())
                    {
                        conn.Open();
                        cmd.CommandText = @query;
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.HasRows)
                            {
                                while (reader.Read())
                                {
                                    count = reader.GetInt32(0);
                                }
                            }
                        }
                    }

                    return count + 1 <= house.GetGarageSize();
                }
            }
            return false;
        }

        public List<Main.GarageVehicle> GetAvailableVehicles(DbPlayer iPlayer, int radius = 25)
        {
            var vehicles = new List<Main.GarageVehicle>();
            if (HouseId > 0 && (iPlayer.ownHouse[0] != HouseId)) return vehicles;

            // Fraktionsgarage
            if (IsTeamGarage())
            {
                if (IsPlanningGarage())
                {
                    if (!Teams.Contains(iPlayer.TeamId)) return vehicles;

                    foreach (var vehicle in VehicleHandler.Instance.GetTeamPlanningVehicles(iPlayer.TeamId))
                    {
                        if (vehicle == null) continue;
                        if (vehicle.databaseId == 0) continue;
                        if (!iPlayer.CanControl(vehicle)) continue;

                        if (iPlayer.Player.Position.DistanceTo(vehicle.entity.Position) <= radius)
                        {
                            if (vehicle.Data.modded_car == 1)
                                vehicles.Add(new Main.GarageVehicle(vehicle.databaseId, vehicle.fuel, vehicle.Data.mod_car_name, ""));
                            else
                                vehicles.Add(new Main.GarageVehicle(vehicle.databaseId, vehicle.fuel, vehicle.Data.Model, ""));
                        }
                    }
                }
                else
                {
                    if (!Teams.Contains(iPlayer.TeamId)) return vehicles;

                    foreach (var vehicle in VehicleHandler.Instance.GetTeamVehicles(iPlayer.TeamId))
                    {
                        if (vehicle == null) continue;
                        if (vehicle.databaseId == 0) continue;
                        if (!iPlayer.CanControl(vehicle)) continue;

                        if (iPlayer.Player.Position.DistanceTo(vehicle.entity.Position) <= radius)
                        {
                            if (vehicle.Data.modded_car == 1)
                                vehicles.Add(new Main.GarageVehicle(vehicle.databaseId, vehicle.fuel, vehicle.Data.mod_car_name, ""));
                            else
                                vehicles.Add(new Main.GarageVehicle(vehicle.databaseId, vehicle.fuel, vehicle.Data.Model, ""));
                        }
                    }
                }
            }
            else
            {
                foreach (var vehicle in VehicleHandler.Instance.GetAllVehicles())
                {
                    if (vehicle == null) continue;
                    if (vehicle.databaseId == 0) continue;
                    if (!iPlayer.CanControl(vehicle)) continue;

                    if (iPlayer.Player.Position.DistanceTo(vehicle.entity.Position) <= radius)
                    {
                        if (vehicle.Data.modded_car == 1)
                            vehicles.Add(new Main.GarageVehicle(vehicle.databaseId, vehicle.fuel, vehicle.Data.mod_car_name, ""));
                        else
                            vehicles.Add(new Main.GarageVehicle(vehicle.databaseId, vehicle.fuel, vehicle.Data.Model,""));
                    }
                }
            }

            return vehicles;
        }

        public GarageSpawn GetFreeSpawnPosition()
        {
            foreach (var spawnPoint in Spawns)
            {
                var found = false;
                foreach (var vehicle in VehicleHandler.Instance.GetAllVehicles())
                {
                    if (vehicle?.entity.Position.DistanceTo(spawnPoint.Position) <= 2.0f)
                    {
                        found = true;
                    }
                }

                if (!found)
                {
                    return spawnPoint;
                }
            }

            return null;
        }
    }
}
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using GVRP.Module.Computer.Apps.FahrzeuguebersichtApp;
using GVRP.Module.Configurations;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Vehicles.Garages;
using static GVRP.Module.Computer.Apps.FahrzeuguebersichtApp.Apps.FahrzeugUebersichtApp;

namespace GVRP.Module.Computer.Apps.FahrzeugUebersichtApp
{
    public class FahrzeugUebersichtFunctions
    {
        public static List<OverviewVehicle> GetOverviewVehiclesForPlayerByCategory(DbPlayer dbPlayer, OverviewCategory overviewCategory)
        {
            List<OverviewVehicle> overviewVehicles = new List<OverviewVehicle>();

            if(dbPlayer.LastQueryBreak.AddSeconds(5) > DateTime.Now)
            {
                dbPlayer.SendNewNotification("Antispam: Bitte 5 Sekunden warten!");
                return overviewVehicles;
            }
            //82227
            switch (overviewCategory)
            {
                case OverviewCategory.OWN:
                    overviewVehicles = GetOverviewVehiclesFromDb($"SELECT vehicles.id, color1, color2, fuel, inGarage, km, vehicles.garage_id, vehiclehash, gps_tracker, garages_spawns.pos_x, garages_spawns.pos_y, garages_spawns.pos_z FROM vehicles LEFT JOIN garages_spawns ON vehicles.garage_id = garages_spawns.garage_id WHERE vehicles.owner = {dbPlayer.Id} GROUP BY vehicles.id;");
                    break;
                case OverviewCategory.KEY:
                    overviewVehicles = GetOverviewVehiclesFromDb($"SELECT vehicles.id, color1, color2, fuel, inGarage, km, vehicles.garage_id, vehiclehash, gps_tracker, garages_spawns.pos_x, garages_spawns.pos_y, garages_spawns.pos_z FROM vehicles LEFT JOIN player_to_vehicle ON player_to_vehicle.vehicleId = vehicles.id LEFT JOIN garages_spawns ON vehicles.garage_id = garages_spawns.garage_id WHERE player_to_vehicle.playerId = { dbPlayer.Id } GROUP BY vehicles.id;");
                    break;
                case OverviewCategory.RENT:
                    overviewVehicles = GetOverviewVehiclesFromDb($"SELECT vehicles.id, color1, color2, fuel, inGarage, km, vehicles.garage_id, vehiclehash, gps_tracker, garages_spawns.pos_x, garages_spawns.pos_y, garages_spawns.pos_z FROM vehicles LEFT JOIN player_vehicle_rent ON player_vehicle_rent.vehicle_id = vehicles.id LEFT JOIN garages_spawns ON vehicles.garage_id = garages_spawns.garage_id WHERE player_vehicle_rent.player_id = { dbPlayer.Id } GROUP BY vehicles.id;");
                    break;
                case OverviewCategory.BUSINESS:
                    if (dbPlayer.ActiveBusiness != null)
                    {
                        overviewVehicles = GetOverviewVehiclesFromDb($"SELECT vehicles.id, color1, color2, fuel, inGarage, km, vehicles.garage_id, vehiclehash, gps_tracker, garages_spawns.pos_x, garages_spawns.pos_y, garages_spawns.pos_z FROM vehicles INNER JOIN business_vehicles ON business_vehicles.vehicle_id = vehicles.id LEFT JOIN garages_spawns ON vehicles.garage_id = garages_spawns.garage_id WHERE business_vehicles.business_id = { dbPlayer.ActiveBusiness.Id} GROUP BY vehicles.id;");
                    }
                    break;
            }

            dbPlayer.LastQueryBreak = DateTime.Now;

            return overviewVehicles;
        }

        private static List<OverviewVehicle> GetOverviewVehiclesFromDb(string statement)
        {
            List<OverviewVehicle> ownVehicles = new List<OverviewVehicle>();
            
            using (var keyConn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var keyCmd = keyConn.CreateCommand())
            {
                keyConn.Open();
                keyCmd.CommandText = statement;
                using (var reader = keyCmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            CarCoorinate ccor = new CarCoorinate
                            {
                                position_x = 0,
                                position_y = 0,
                                position_z =0
                            };

                            OverviewVehicle vehicle = new OverviewVehicle
                            {
                                Id = reader.GetUInt32("id"),
                                Color1 = reader.GetUInt32("color1"),
                                Color2 = reader.GetUInt32("color2"),
                                Fuel = reader.GetDouble("fuel"),
                                InGarage = reader.GetInt32("inGarage") == 1,
                                Km = reader.GetFloat("km"),
                                GarageName = reader.GetInt32("gps_tracker") == 1 && GarageModule.Instance.Contains(reader.GetUInt32("garage_id")) ? GarageModule.Instance.Get(reader.GetUInt32("garage_id")).Name : "kein GPS Signal...",
                                Vehiclehash = reader.GetString("vehiclehash"),
                                Besitzer = "",
                                CarCor = ccor
                            };

                            if(vehicle != null) ownVehicles.Add(vehicle);
                        }
                    }
                }
                keyConn.Close();
            }

            return ownVehicles;
        }
    }
}

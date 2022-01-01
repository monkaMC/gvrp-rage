using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using GVRP.Handler;
using GVRP.Module.Business;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.ClientUI.Windows;
using GVRP.Module.Configurations;
using GVRP.Module.Government;
using GVRP.Module.Logging;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.VehicleRent;
using GVRP.Module.Vehicles.Data;
using GVRP.Module.Vehicles.Garages;
using GVRP.Module.Vehicles.Windows;

namespace GVRP.Module.Tasks
{
    public class GarageVehiclesTask : SqlResultTask
    {
        private readonly Garage garage;
        private readonly DbPlayer dbPlayer;

        public GarageVehiclesTask(DbPlayer dbPlayer, Garage garage)
        {
            this.dbPlayer = dbPlayer;
            this.garage = garage;
        }

        public override bool CanExecute()
        {
            if (garage.IsTeamGarage())
            {
                return garage.Teams.Contains(dbPlayer.TeamId) && dbPlayer.TeamRank >= garage.Rang;
            }

            return true;
        }

        public override string GetQuery()
        {
            if (garage.IsTeamGarage() && !garage.IsPlanningGarage())
            {
                return $"SELECT * FROM `fvehicles` WHERE `team` = '{dbPlayer.TeamId}' AND `inGarage` = 1 AND `impound_release` <= NOW() ORDER BY vehiclehash ASC;";
            }
            else if (garage.IsTeamGarage() && garage.IsPlanningGarage())
            {
                return $"SELECT * FROM `fvehicles` WHERE `team` = '{dbPlayer.TeamId}' AND `inGarage` = 1 AND `planning_vehicle` = 1 AND `impound_release` <= NOW() ORDER BY vehiclehash ASC;";
            }
            else
            {
                return (dbPlayer.TeamId == (int)teams.TEAM_LSC)
                    ? $"SELECT * FROM `vehicles` WHERE (`owner` = '{dbPlayer.Id}' OR `TuningState` = 1) AND `inGarage` = 1 AND `garage_id` = '{garage.Id}'  ORDER BY vehiclehash ASC;"
                    : $"SELECT * FROM `vehicles` WHERE `owner` = '{dbPlayer.Id}' AND `inGarage` = 1 AND `garage_id` = '{garage.Id}' ORDER BY vehiclehash ASC;";
            }
        }

        public override void OnFinished(MySqlDataReader reader)
        {
            var vehList = new List<Main.GarageVehicle>();

            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    var data = VehicleDataModule.Instance.GetDataById(reader.GetUInt32("model"));
                    if (data == null) continue;
                    var vehicleId = reader.GetUInt32("id");
                    var fuel = reader.GetFloat("fuel");

                    if (garage.IsTeamGarage() && reader.GetInt32("defcon_level") > 0 && reader.GetInt32("defcon_level") < GovernmentModule.Defcon.Level)
                    {
                        dbPlayer.SendNewNotification("Die aktuelle Defcon Stufe für dieses Fahrzeug ist nicht erreicht!");
                        continue;
                    }

                    if (data.modded_car == 1)
                        vehList.Add(new Main.GarageVehicle(vehicleId, fuel, data.mod_car_name, garage.IsTeamGarage() ? "" : reader.GetString("note")));
                    else
                        vehList.Add(new Main.GarageVehicle(vehicleId, fuel, data.Model, garage.IsTeamGarage() ? "" : reader.GetString("note")));
                }
            }

            var keys = dbPlayer.VehicleKeys;

            if(dbPlayer.IsMemberOfBusiness())
            {
                foreach(var key in dbPlayer.ActiveBusiness.VehicleKeys)
                {
                    if (!keys.ContainsKey(key.Key) && vehList.Find(g => g.Id == key.Key) == null) keys.Add(key.Key, key.Value);
                }
            }

            // VehicleRentModule
            foreach (PlayerVehicleRentKey playerVehicleRent in VehicleRentModule.PlayerVehicleRentKeys.ToList().Where(k => k.PlayerId == dbPlayer.Id))
            {
                if(!keys.ContainsKey(playerVehicleRent.VehicleId) && vehList.Find(g => g.Id == playerVehicleRent.VehicleId) == null)
                {
                    keys.Add(playerVehicleRent.VehicleId, "Mietfahrzeug");
                }
            }

            foreach (var key in keys)
            {
                var vehicle = GetGarageVehicleByKey(key.Key);
                if (vehicle != null)
                { 
                    if(!CheckById(vehList,vehicle))
                    vehList.Add(vehicle);
                }
            }

            vehList = vehList.Distinct().ToList();
            var vehicleJson = JsonConvert.SerializeObject(vehList);
            ComponentManager.Get<GarageWindow>().TriggerEvent(dbPlayer.Player, "responseVehicleList", vehicleJson);
        }

        private bool CheckById(List<Main.GarageVehicle> vehList, Main.GarageVehicle vehicle)
        {
            foreach(var veh in vehList)
            {
                if (veh.Id == vehicle.Id) return true;
            }
            return false;
        }

        private Main.GarageVehicle GetGarageVehicleByKey(uint key)
        {
            if (VehicleHandler.Instance.GetByVehicleDatabaseId(key) != null) return null;
            try
            {
                using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = $"SELECT * FROM `vehicles` WHERE `id` = '{key}' AND `inGarage` = '1' AND `garage_id` = {garage.Id} AND `impound_release` <= NOW();";
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                var data = VehicleDataModule.Instance.GetDataById(reader.GetUInt32("model"));
                                if (data == null) continue;
                                if (data.modded_car == 1)
                                    return new Main.GarageVehicle(reader.GetUInt32("id"), reader.GetFloat("fuel"), data.mod_car_name, reader.GetString("note"));
                                else
                                    return new Main.GarageVehicle(reader.GetUInt32("id"), reader.GetFloat("fuel"), data.Model, reader.GetString("note"));
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Crash(e);
                return null;
            }

            return null;
        }
    }
}
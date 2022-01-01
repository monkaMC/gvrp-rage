using MySql.Data.MySqlClient;
using System;
using GVRP.Module.Configurations;

namespace GVRP.Module.Business
{
    public class BusinessVehicle
    {
        public static BusinessVehicle Instance { get; } = new BusinessVehicle();

        private BusinessVehicle()
        {
        }

        public void DeleteAllVehicleKeys(uint vehicleId)
        {
            foreach (var biz in BusinessModule.Instance.GetAll().Values)
            {
                if (biz?.VehicleKeys == null) continue;
                if (biz.VehicleKeys.ContainsKey(vehicleId))
                {
                    biz.VehicleKeys.Remove(vehicleId);
                }
            }
            MySQLHandler.ExecuteAsync($"DELETE FROM `business_vehicles` WHERE `vehicle_id` = '{vehicleId}';");
        }
    }
    public static class BusinessVehicleExtension
    {
        public static void DeleteVehicleKey(this Business biz, int vehicleId)
        {
            MySQLHandler.ExecuteAsync(
                    $"DELETE FROM `business_vehicles` WHERE `business_id` = '{biz.Id}' AND `vehicle_id` = '{vehicleId}';");
        }

        public static void AddVehicleKey(this Business biz, uint vehicleId, String vehicleName)
        {
            if (vehicleId == 0) return;
            if (biz.VehicleKeys.ContainsKey(vehicleId)) return;
            biz.VehicleKeys.Add(vehicleId, vehicleName);
            MySQLHandler.ExecuteAsync(
                $"INSERT INTO `business_vehicles` (`business_id`, `vehicle_id`) VALUES ('{biz.Id}', '{vehicleId}');");
        }

        public static void LoadVehicleKeys(this Business biz)
        {
            biz.VehicleKeys.Clear();

            using (var keyConn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var keyCmd = keyConn.CreateCommand())
            {
                keyConn.Open();
                keyCmd.CommandText =
                    $"SELECT vehicle_id, vehiclehash FROM `business_vehicles` INNER JOIN vehicles ON business_vehicles.vehicle_id = vehicles.id WHERE business_id = '{biz.Id}';";
                using (var keyReader = keyCmd.ExecuteReader())
                {
                    if (keyReader.HasRows)
                    {
                        while (keyReader.Read())
                        {
                            var keyId = keyReader.GetUInt32(0);
                            var keyName = keyReader.GetString(1);
                            if (!biz.VehicleKeys.ContainsKey(keyId))
                            {
                                biz.VehicleKeys.Add(keyId, keyName);
                            }
                        }
                    }
                }
                keyConn.Close();
            }
        }
    }
}
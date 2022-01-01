using MySql.Data.MySqlClient;
using System.Collections.Generic;
using GVRP.Module.Configurations;
using GVRP.Module.Vehicles.Garages;

namespace GVRP.Module.Computer.Apps.SupportVehicleApp
{
    public class SupportVehicleFunctions
    {
        public enum VehicleCategory
        {
            ID = 0,
            ALL = 1
        }

        public static List<VehicleData> GetVehicleData(VehicleCategory category, int id)
        {
            List<VehicleData> vehicleData = new List<VehicleData>();

            using (MySqlConnection conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                conn.Open();

                string statement;

                if(category == VehicleCategory.ID)
                {
                    statement = $"SELECT id, owner, inGarage, garage_id, vehiclehash FROM vehicles WHERE id = '{ id }'";
                }
                else
                {
                    statement = $"SELECT id, owner, inGarage, garage_id, vehiclehash FROM vehicles WHERE owner = '{ id }'";
                }

                cmd.CommandText = statement;
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            VehicleData data = new VehicleData
                            {
                                Id = reader.GetInt32("id"),
                                InGarage = reader.GetInt32("inGarage"),
                                Garage = reader.GetInt32("garage_id") > 0 && GarageModule.Instance.Contains(reader.GetUInt32("garage_id")) ? GarageModule.Instance.Get(reader.GetUInt32("garage_id")).Name : "Unbekannte Garage!" ,
                                Vehiclehash = reader.GetString("vehiclehash")
                            };

                            vehicleData.Add(data);
                        }

                        reader.Close();
                    }
                }

                conn.Close();
            }

            return vehicleData;
        }
    }
}

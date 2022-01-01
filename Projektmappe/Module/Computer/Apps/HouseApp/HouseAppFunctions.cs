using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using GVRP.Module.Computer.Apps.VehicleTaxApp;
using GVRP.Module.Configurations;
using GVRP.Module.Houses;
using GVRP.Module.PlayerName;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Computer.Apps.HouseApp
{
    public class HouseAppFunctions
    {
        public static List<Tenant> GetTenantsForHouseByPlayer(DbPlayer dbPlayer)
        {
            try
            {
                List<Tenant> tenants = new List<Tenant>();

                foreach (HouseRent houseRent in HouseRentModule.Instance.houseRents.ToList().Where(hr => hr.HouseId == dbPlayer.ownHouse[0]))
                {
                    if (houseRent == null || houseRent.PlayerId == null || houseRent.SlotId == null) continue;
                    tenants.Add(new Tenant()
                    {
                        SlotId = houseRent.SlotId,
                        PlayerId = houseRent.PlayerId,
                        Name = houseRent.PlayerId != 0 ? PlayerNameModule.Instance.Get(houseRent.PlayerId).Name : "Freier Mietplatz",
                        RentPrice = houseRent.RentPrice,
                    });
                }

                return tenants;
            }
            catch(Exception e)
            {
                Logging.Logger.Crash(e);
            }
            return new List<Tenant>();
        }

        public static List<HouseVehicle> GetVehiclesForHouseByPlayer(DbPlayer dbPlayer, House house)
        {
            List<HouseVehicle> vehicles = new List<HouseVehicle>();
            using (MySqlConnection conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                conn.Open();

                cmd.CommandText = $"SELECT id, vehiclehash, owner FROM vehicles WHERE garage_id = @garageId";
                cmd.Parameters.AddWithValue("@garageId", $"{house.GarageId}");
                cmd.Prepare();

                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            string name = "";
                            // Get PlayerName
                            if (PlayerNameModule.Instance.Contains(reader.GetUInt32("owner"))) name = PlayerNameModule.Instance.Get(reader.GetUInt32("owner")).Name;

                            HouseVehicle houseVehicle = new HouseVehicle()
                            {
                                Id = reader.GetUInt32("id"),
                                Name = reader.GetString("vehiclehash"),
                                Owner = name
                            };
                            vehicles.Add(houseVehicle);
                        }
                    }
                }
                conn.Close();
            }
            return vehicles;
        }
    }
}

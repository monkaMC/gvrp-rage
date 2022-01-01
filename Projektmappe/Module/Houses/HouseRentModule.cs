using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GVRP.Module.Configurations;
using GVRP.Module.Menu;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Houses
{
    public class HouseRentModule : Module<HouseRentModule>
    {
        public static uint ItemHouseRentContract = 956;
        public List<HouseRent> houseRents = new List<HouseRent>();

        protected override bool OnLoad()
        {
            houseRents.Clear();

            var query = $"SELECT * FROM `house_rents`";
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
                            houseRents.Add(new HouseRent(reader));
                        }
                    }
                }
            }
            
            MenuManager.Instance.AddBuilder(new HouseRentContractMenuBuilder());
            return base.OnLoad();
        }
    }

    public static class HouseRentFunctions
    {
        public static HouseRent GetTenant(this DbPlayer dbPlayer)
        {
            return HouseRentModule.Instance.houseRents.ToList().Where(ht => ht.PlayerId == dbPlayer.Id).FirstOrDefault();
        }
        
        public static bool IsTenant(this DbPlayer dbPlayer)
        {
            return HouseRentModule.Instance.houseRents.ToList().Where(ht => ht.PlayerId == dbPlayer.Id).FirstOrDefault() != null;
        }

        public static bool IsTenant(this House house, DbPlayer dbPlayer)
        {
            return HouseRentModule.Instance.houseRents.ToList().Where(ht => ht.PlayerId == dbPlayer.Id && ht.HouseId == house.Id).FirstOrDefault() != null;
        }

        public static int GetTenantAmountUsed(this House house)
        {
            return HouseRentModule.Instance.houseRents.ToList().Where(ht => ht.HouseId == house.Id && ht.PlayerId != 0).Count();
        }

        public static int GetFreeRents(this House house)
        {
            return HouseRentModule.Instance.houseRents.ToList().Where(ht => ht.HouseId == house.Id && ht.PlayerId == 0).Count();
        }

        public static void AddTenant(this House house, DbPlayer tenant, int slot, int price)
        {
            // Find Tenant Slot on House
            HouseRent houseRent = HouseRentModule.Instance.houseRents.ToList().Where(ht => ht.HouseId == house.Id && ht.SlotId == slot).FirstOrDefault();

            // Add if not exists
            if(houseRent == null)
            {
                houseRent = new HouseRent(house.Id, tenant.Id, slot, price);
                HouseRentModule.Instance.houseRents.Add(houseRent);
                houseRent.Insert();
            }
        }

        public static void AddCleanTenant(this House house, int slot, int price)
        {
            // Find Tenant Slot on House
            HouseRent houseRent = HouseRentModule.Instance.houseRents.ToList().Where(ht => ht.HouseId == house.Id && ht.SlotId == slot).FirstOrDefault();

            // Add if not exists
            if (houseRent == null)
            {
                houseRent = new HouseRent(house.Id, 0, slot, price);
                HouseRentModule.Instance.houseRents.Add(houseRent);
                houseRent.Insert();
            }
        }

        public static void ChangeTenant(this House house, DbPlayer tenant, int slot, int price)
        {
            // Find Tenant Slot on House
            HouseRent houseRent = HouseRentModule.Instance.houseRents.Where(ht => ht.HouseId == house.Id && ht.SlotId == slot).FirstOrDefault();

            if (houseRent == null) return;
            houseRent.PlayerId = tenant.Id;
            houseRent.HouseId = house.Id;
            houseRent.RentPrice = price;
            houseRent.Save();
        }
        
        public static void ClearTenant(this House house, int slot)
        {
            // Find Tenant Slot on House
            HouseRent houseRent = HouseRentModule.Instance.houseRents.Where(ht => ht.HouseId == house.Id && ht.SlotId == slot).FirstOrDefault();

            if (houseRent == null) return;
            houseRent.PlayerId = 0;
            houseRent.RentPrice = 0;
            houseRent.Save();
        }

        public static void RemoveTenant(this DbPlayer dbPlayer)
        {
            HouseRent houseRent = HouseRentModule.Instance.houseRents.Where(ht => ht.PlayerId == dbPlayer.Id).FirstOrDefault();

            if (houseRent == null) return;

            House house = HouseModule.Instance.Get(houseRent.HouseId);

            if (house != null) {

                // alle Fahrzeuge in Pillbox beim ausmieten.
                MySQLHandler.ExecuteAsync($"UPDATE vehicles SET inGarage = '1' WHERE inGarage = '{house.GarageId}' AND owner = '{houseRent.PlayerId}'");
            }

            houseRent.PlayerId = 0;
            houseRent.RentPrice = 0;
            houseRent.Save();
        }

        public static void Clear(this HouseRent houseRent)
        {
            if (houseRent == null) return;
            houseRent.PlayerId = 0;
            houseRent.RentPrice = 0;
            houseRent.Save();
        }

        public static void Insert(this HouseRent houseRent)
        {
            MySQLHandler.ExecuteAsync($"INSERT INTO `house_rents` (house_id, player_id, slot_id, rent_price) VALUES ('{houseRent.HouseId}', '{houseRent.PlayerId}', '{houseRent.SlotId}', '{houseRent.RentPrice}');");
        }

        public static void Save(this HouseRent houseRent)
        {
            MySQLHandler.ExecuteAsync($"UPDATE `house_rents` SET player_id = '{houseRent.PlayerId}', rent_price = '{houseRent.RentPrice}' WHERE house_id = '{houseRent.HouseId}' AND slot_id = '{houseRent.SlotId}';");
        }
    }
}

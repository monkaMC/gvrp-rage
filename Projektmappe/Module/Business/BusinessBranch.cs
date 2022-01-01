using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GVRP.Module.Configurations;

namespace GVRP.Module.Business
{
    public class BusinessBranch
    {
        public uint BusinessId { get; set; }
        public uint FuelstationId { get; set; }
        public uint RaffinerieId { get; set; }
        public uint NightClubId { get; set; }
    }

    public static class BusinessBranchExtension
    {
        public static bool CanBuyBranch(this BusinessBranch businessBranch)
        {
            int count = 0;
            if (businessBranch.FuelstationId != 0) count++;
            if (businessBranch.RaffinerieId != 0) count++;
            if (businessBranch.NightClubId != 0) count++;

            return count <= 3;
        }

        public static bool hasFuelstation(this BusinessBranch businessBranch)
        {
            return businessBranch.FuelstationId != 0;
        }

        public static bool hasRaffinerie(this BusinessBranch businessBranch)
        {
            return businessBranch.RaffinerieId != 0;
        }

        public static bool hasNightClub(this BusinessBranch businessBranch)
        {
            return businessBranch.NightClubId != 0;
        }

        public static void SetFuelstation(this BusinessBranch businessBranch, uint fuelstationId)
        {
            businessBranch.FuelstationId = fuelstationId;
            MySQLHandler.ExecuteAsync($"UPDATE `business_branches` SET `fuelstation_id` = '{fuelstationId}' WHERE business_id = '{businessBranch.BusinessId}'");
        }

        public static void RemoveFuelstation(this BusinessBranch businessBranch)
        {
            businessBranch.SetFuelstation(0);
        }

        public static void SetNightClub(this BusinessBranch businessBranch, uint nightClubId)
        {
            businessBranch.NightClubId = nightClubId;
            MySQLHandler.ExecuteAsync($"UPDATE `business_branches` SET `nightclub_id` = '{nightClubId}' WHERE business_id = '{businessBranch.BusinessId}'");
        }

        public static void RemoveNightClub(this BusinessBranch businessBranch)
        {
            businessBranch.SetNightClub(0);
        }

        public static void SetRaffinerie(this BusinessBranch businessBranch, uint raffinerieId)
        {
            businessBranch.RaffinerieId = raffinerieId;
            MySQLHandler.ExecuteAsync($"UPDATE `business_branches` SET `raffinery_id` = '{raffinerieId}' WHERE business_id = '{businessBranch.BusinessId}'");
        }

        public static void RemoveRaffinerie(this BusinessBranch businessBranch)
        {
            businessBranch.SetRaffinerie(0);
        }

        public static void LoadBusinessBranch(this Business business)
        {
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"SELECT * FROM `business_branches` WHERE business_id = '{business.Id}';";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            business.BusinessBranch = new BusinessBranch();
                            business.BusinessBranch.BusinessId = business.Id;
                            business.BusinessBranch.FuelstationId = reader.GetUInt32("fuelstation_id");
                            business.BusinessBranch.RaffinerieId = reader.GetUInt32("raffinery_id");
                            business.BusinessBranch.NightClubId = reader.GetUInt32("nightclub_id");
                        }
                    }
                    else
                    {
                        CreateBusinessBranch(business);
                    }
                }
                conn.Close();
            }
        }

        private static void CreateBusinessBranch(this Business business)
        {
            MySQLHandler.ExecuteAsync($"INSERT INTO `business_branches` (`business_id`, `raffinery_id`, `fuelstation_id`, `nightclub_id`) VALUES ('{business.Id}', '0', '0', '0')");

            business.BusinessBranch = new BusinessBranch();
            business.BusinessBranch.BusinessId = business.Id;
            business.BusinessBranch.FuelstationId = 0;
            business.BusinessBranch.RaffinerieId = 0;
            business.BusinessBranch.NightClubId = 0;
        }
    }
}

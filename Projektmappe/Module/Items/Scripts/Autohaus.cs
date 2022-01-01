using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.Configurations;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Vehicles.Data;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool UseCarsell(DbPlayer iPlayer, ItemModel ItemData, Item item)
        {
            string itemScript = ItemData.Script;

            if (!uint.TryParse(itemScript.Split('_')[1], out uint AutohausId))
            {
                return false;
            }

            uint teamId = (uint)teams.TEAM_CARSELL1;
            switch(AutohausId)
            {
                case 1:
                    teamId = (uint)teams.TEAM_CARSELL1;
                    break;
                case 2:
                    teamId = (uint)teams.TEAM_CARSELL2;
                    break;
                case 3:
                    teamId = (uint)teams.TEAM_CARSELL3;
                    break;
                default:
                    break;
            }
            
            if(item.Data == null || !item.Data.ContainsKey("orderId") || !item.Data.ContainsKey("vehName") || !item.Data.ContainsKey("customerName"))
            {
                return false;
            }

            if(iPlayer.TeamId == teamId) // Read Only
            {
                

                string vehName = item.Data["vehName"];
                string contractString = item.Data["orderId"];
                if(!Int32.TryParse(contractString, out int contractId))
                {
                    return false;
                }
                string customerName = item.Data["customerName"];
                iPlayer.SendNewNotification($"({contractId}) Lieferschein {vehName} | Kunde {customerName}");
                return false;
            }
            else
            {
                string contractString = item.Data["orderId"];
                if (!Int32.TryParse(contractString, out int contractId))
                {
                    return false;
                }
                if (contractId == 0)
                {
                    iPlayer.SendNewNotification($"Fehler, bitte im Support melden!");
                    return false;
                }

                using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = $"SELECT * FROM jobfaction_carsell_orders WHERE status = 2 AND id = '{contractId}';";
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if(iPlayer.Id != reader.GetUInt32("player_id"))
                                {
                                    iPlayer.SendNewNotification($"Dieser Lieferschein kann nur vom Besitzer eingelöst werden!");
                                    return false;
                                }

                                VehicleData vehData = VehicleDataModule.Instance.GetDataById(reader.GetUInt32("vehicle_data_id"));
                                if (vehData == null) return false;
                                
                                // INSERT VEHICLE
                                MySQLHandler.ExecuteAsync($"INSERT INTO `vehicles` (`team_id`, `owner`, `color1`, `color2`, `tuning`, `inGarage`, `garage_id`, `model`, `vehiclehash`) " +
                                    $"VALUES ('0', '{reader.GetInt32("player_id")}', '{reader.GetInt32("color1")}', '{reader.GetInt32("color2")}', '23:{reader.GetInt32("wheel")}', '1', '1', '{vehData.Id}', '{vehData.Model}');");

                                iPlayer.SendNewNotification($"Ihr Fahrzeug wurde erfolgreich ausgeliefert (Pillbox Garage)!");
                                return true;
                            }
                        }
                    }
                    conn.Close();
                    
                }
            }
            iPlayer.SendNewNotification($"Fehler, bitte im Support melden!");
            return false;
        }
    }
}

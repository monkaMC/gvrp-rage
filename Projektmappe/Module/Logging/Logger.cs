using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Module.Chat;
using GVRP.Module.Configurations;
using GVRP.Module.Farming;
using GVRP.Module.Items;
using GVRP.Module.Kasino;
using GVRP.Module.Laboratories;
using GVRP.Module.Meth;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Schwarzgeld;

namespace GVRP.Module.Logging
{
    public class LoggerEvent : Script
    {
        [RemoteEvent]
        public void logServerNiggablablalllbllalalallallla(Client client, string log)
        {
            //Logger.Debug(log);
        }
    }

    public class LoggingModule : Module<LoggingModule>
    {
        private static string LoggingQuerys = "";

        public void AddToQueryLog(string query)
        {
            LoggingQuerys += query;
        }
        
        public override void OnFiveMinuteUpdate()
        {
            string query = "";

            foreach (var test in FarmingModule.Instance.FarmAmount.ToList())
            {
                query += $"INSERT INTO `log_farm` (`farmspot_id`, `amount`) VALUES ('{test.Key.Id}', '{test.Value}');";
            }
            FarmingModule.Instance.FarmAmount.Clear();
            foreach (var test in FarmingModule.Instance.ProcessAmount.ToList())
            {
                query += $"INSERT INTO `log_farmprocess` (`farmprocess_id`, `amount`) VALUES ('{test.Key.Id}', '{test.Value}');";

            }

            if (ItemModelModule.Instance.LogItems.Count > 0)
            {
                foreach (KeyValuePair<int, Dictionary<int, int>> temp in ItemModelModule.Instance.LogItems.ToList())
                {
                    foreach (KeyValuePair<int, int> kvp in temp.Value)
                    {
                        query += $"INSERT INTO `log_used_items` (`item_id`, `team_id`, `amount`) VALUES ('{temp.Key}', '{kvp.Key}', '{kvp.Value}');";
                    }
                    temp.Value.Clear();
                }
            }

            if (MethModule.Instance.ephi != 0) query += $"INSERT INTO `log_methcook` (`ephi_amount`, `meth_amount`, `cooker_amount`) VALUES ('{MethModule.Instance.ephi}', '{MethModule.Instance.meth}', '{MethModule.Instance.cooker}');";
            MethModule.Instance.ResetLogVariables();
            if (!query.Equals(""))MySQLHandler.ExecuteAsync(query);

            MySQLHandler.ExecuteAsync(LoggingQuerys);
            LoggingQuerys = "";
        }
    }

    public static class Logger
    {
        public static void Debug(string message)
        {
            if (Configuration.Instance.DevLog)
            {
                Print($"{DateTime.Now} {message}");
            }
        }

        public static void GlobalDebug(string message)
        {
            Chats.SendGlobalMessage(message, Chats.COLOR.RED, Chats.ICON.DEV, duration:20000);
        }
        
        public static void SaveToFbankLog(uint p_TeamID, int p_Amount, uint p_PlayerID, string p_PlayerName, bool p_Deposit)
        {
            int l_Deposit = p_Deposit ? 1 : 0;
            var query = $"INSERT INTO `log_fbank` (`team_id`,`amount`, `player_id`, `player_name`, `deposit`) VALUES ('{p_TeamID}', '{p_Amount}', '{p_PlayerID}', '{p_PlayerName}', '{l_Deposit}');";
            LoggingModule.Instance.AddToQueryLog(query);
        }

        public static void SaveToItemLog(uint playerId, string playerName, uint itemId, int itemAmount, string affectedInventoryType, int affectedInventoryId)
        {
            var query = $"INSERT INTO `log_item` (`player_id`,`player_name`, `inventory_id`, `inventory_type`, `item_id`, `item_amount`) VALUES ('{playerId}', '{playerName}', '{affectedInventoryId}', '{affectedInventoryType}', '{itemId}', '{itemAmount}');";
            LoggingModule.Instance.AddToQueryLog(query);
        }

        public static void LiveinvaderLog(uint playerId, string playerName, string message)
        {
            var query = $"INSERT INTO `log_liveinvader` (`player_id`,`player_name`, `message`) VALUES ('{playerId}', '{playerName}', '{message}');";
            LoggingModule.Instance.AddToQueryLog(query);
        }

        internal static void AddVehiclePlateLog(uint id, uint vehicleId, string plate)
        {
            var query = $"INSERT INTO `log_vehicleplate` (`player_id`, `vehicle_id`, `plate`) VALUES ('{id}', '{vehicleId}', '{plate}');";
            LoggingModule.Instance.AddToQueryLog(query);
        }

        internal static void AddToFuelStationInsertLog(uint fuelstationId, uint playerId, int amount)
        {
            var query = $"INSERT INTO `log_fuelstation_filled` (`fuelstation_id` ,`player_id`, `amount`) VALUES ('{fuelstationId}', '{playerId}', '{amount}');";
            LoggingModule.Instance.AddToQueryLog(query);
        }

        internal static void AddToRaffineryLog(uint raffineryId, uint playerId)
        {
            var query = $"INSERT INTO `log_raffinery` (`raffinery_id` ,`player_id`) VALUES ('{raffineryId}', '{playerId}');";
            LoggingModule.Instance.AddToQueryLog(query);
        }

        internal static void AddToVehicleDestroyLog(uint vehicleId, uint playerId, int returnMoney)
        {
            var query = $"INSERT INTO `log_vehicle_destroyed` (`vehicle_id` ,`player_id`, `return_money`) VALUES ('{vehicleId}', '{playerId}', '{returnMoney}');";
            LoggingModule.Instance.AddToQueryLog(query);
        }

        public static void SaveToDbLog(string argument)
        {
            if (argument.Contains("'")) argument.Replace("'", "");
            var query = $"INSERT INTO `log_serverdump` (`argument`) VALUES ('{argument}');";
            LoggingModule.Instance.AddToQueryLog(query);
        }
        
        public static void SaveClothesShopBuyAction(uint shopid, int amount)
        {
            var query = $"INSERT INTO log_clothes_shops (shop_id, amount, buy_actions) VALUES ({shopid}, {amount}, 1) ON DUPLICATE KEY UPDATE amount=amount+{amount}, buy_actions = buy_actions +1;";
            LoggingModule.Instance.AddToQueryLog(query);
        }

        public static void SaveToCustomLog(string key, int val1, int val2)
        {
            var query = $"INSERT INTO log_customdata (`key`, `val_1`, `val_2`) VALUES ('{key}', '{val1}', '{val2}') ON DUPLICATE KEY UPDATE val_1=val_1+{val1}, val_2=val_2+{val2};";
            LoggingModule.Instance.AddToQueryLog(query);
        }

        public static void SaveToFriskHouseLog(uint player_id, int houseid, string playerName)
        {
            var query = $"INSERT INTO `log_friskhouse` (`player_id`,`houseid`, `player_name`) VALUES ('{player_id}', '{houseid}', '{playerName}');";
            LoggingModule.Instance.AddToQueryLog(query);
        }

        public static void SaveToFriskVehLog(uint player_id, int vehicleId, string playerName)
        {
            var query = $"INSERT INTO `log_friskveh` (`player_id`,`vehicle_id`, `player_name`) VALUES ('{player_id}', '{vehicleId}', '{playerName}');";
            LoggingModule.Instance.AddToQueryLog(query);
        }

        public static void SaveToStaatsKasse(int p_Amount, uint p_PlayerID, string p_PlayerName, bool p_Deposit)
        {
            int l_Deposit = p_Deposit ? 1 : 0;
            var query = $"INSERT INTO `log_staatskasse` (`amount`, `player_id`, `player_name`, `deposit`) VALUES ('{p_Amount}', '{p_PlayerID}', '{p_PlayerName}', '{l_Deposit}');";
            LoggingModule.Instance.AddToQueryLog(query);
        }

        public static void SaveToEventKasse(int p_Amount, uint p_PlayerID, string p_PlayerName, bool p_Deposit)
        {
            int l_Deposit = p_Deposit ? 1 : 0;
            var query = $"INSERT INTO `log_eventkasse` (`amount`, `player_id`, `player_name`, `deposit`) VALUES ('{p_Amount}', '{p_PlayerID}', '{p_PlayerName}', '{l_Deposit}');";
            LoggingModule.Instance.AddToQueryLog(query);
        }

        public static void SaveToBusinessBank(uint p_BizId, int p_Amount, uint p_PlayerID, string p_PlayerName, bool p_Deposit)
        {
            int l_Deposit = p_Deposit ? 1 : 0;
            var query = $"INSERT INTO `log_bizbank` (`biz_id` ,`amount`, `player_id`, `player_name` , `deposit`) VALUES ('{p_BizId}', '{p_Amount}', '{p_PlayerID}', '{p_PlayerName}', '{l_Deposit}');";
            LoggingModule.Instance.AddToQueryLog(query);
        }
        public static void AddFuelToDB(float amount, int fraktionId)
        {
            var query = $"INSERT INTO `log_fuel` (`amount` ,`fraktionId`) VALUES ('{amount}', '{fraktionId}');";
            LoggingModule.Instance.AddToQueryLog(query);
        }

        public static void AddGangwarSellToDB(uint id, uint item_id, int amount, int playerPrice)
        {
            var query = $"INSERT INTO `log_gangwar_sell` (`player_id` ,`item_id`, `amount`, `price`) VALUES ('{id}', '{item_id}', '{amount}', '{playerPrice}');";
            LoggingModule.Instance.AddToQueryLog(query);
        }

        public static void AddActionLogg(uint playerId, int type)
        {
            var query = $"INSERT INTO `log_actions` (`player_id` ,`type`) VALUES ('{playerId}', '{type}');";
            LoggingModule.Instance.AddToQueryLog(query);
        }

        public static void AddToAltkleiderLog(uint playerId, string desc)
        {
            var query = $"INSERT INTO `log_altkleider` (`player_id` ,`wardrobe_desc`) VALUES ('{playerId}', '{desc}');";
            LoggingModule.Instance.AddToQueryLog(query);
        }

        public static void AddToEinreiseLog(uint playerId, uint destp, bool success)
        {
            var query = $"INSERT INTO `log_einreise` (`player_id` ,`dest_id`, `success`) VALUES ('{playerId}', '{destp}', '{(success ? "angenommen" : "abgelehnt")}');";
            LoggingModule.Instance.AddToQueryLog(query);
        }

        public static void AddWeaponFactoryLog(uint playerId, int itemId)
        {
            var query = $"INSERT INTO `log_weapon` (`player_id` ,`item_id`) VALUES ('{playerId}', '{itemId}');";
            LoggingModule.Instance.AddToQueryLog(query);
        }

        public static void AddFindLog(uint playerId, uint findplayerId)
        {
            var query = $"INSERT INTO `log_finds` (`playerid` ,`findplayerid`) VALUES ('{playerId}', '{findplayerId}');";
            LoggingModule.Instance.AddToQueryLog(query);
        }

        public static void SaveLoginAttempt(uint p_PlayerID, string p_SCName, string p_IP, uint p_Success)
        {
            var l_Query = $"INSERT INTO `log_login` (`account_id`, `sc_name`, `ip`, `success`) VALUES ('{p_PlayerID.ToString()}', '{p_SCName}', '{p_IP}', '{p_Success.ToString()}');";
            LoggingModule.Instance.AddToQueryLog(l_Query);
        }

        public static void AddSupportLog(uint playerId, uint supporterId, string reason, DateTime created)
        {
            var l_Query = $"INSERT INTO `log_support` (`player_id`, `supporter_id`, `reason`, `created`) VALUES ('{playerId}', '{supporterId}', '{reason}', '{created.ToString("yyyy-MM-dd HH:mm:ss")}');";
            LoggingModule.Instance.AddToQueryLog(l_Query);
        }

        public static void AddTattoShopLog(uint tattoShopId, uint playerId, int amount, bool deposit )
        {
            var l_Query = $"INSERT INTO `log_tattoshop` (`tattoshop_id`, `player_id`, `amount`, `deposit`) VALUES ('{tattoShopId}', '{playerId}', '{amount}', '{(deposit == true ? "1" : "0")}');";
            LoggingModule.Instance.AddToQueryLog(l_Query);
        }

        public static void AddSpendeLog(uint playerId, int amount)
        {
            var l_Query = $"INSERT INTO `log_spende` (`player_id`, `amount`) VALUES ('{playerId}', '{amount}');";
            LoggingModule.Instance.AddToQueryLog(l_Query);
        }

        public static void AddReportLog(uint playerId, String playerIds, String reason)
        {
            var l_Query = $"INSERT INTO `log_report` (`player_id`, `players`, `reason`) VALUES ('{playerId}', '{playerIds}', '{reason}');";
            LoggingModule.Instance.AddToQueryLog(l_Query);
        }
        public static void AddFlimitReport(uint playerId, String reportstr)
        {
            var l_Query = $"INSERT INTO `log_flimitreport` (`player_id`, `reportdata`) VALUES ('{playerId}', '{reportstr}');";
            LoggingModule.Instance.AddToQueryLog(l_Query);
        }

        public static void AddShopBuyLog(uint playerId, int item_id, int amount, int price)
        {
            var l_Query = $"INSERT INTO `log_shop_buy` (`player_id`, `item_id`, `item_amount`, `price`) VALUES ('{playerId}', '{item_id}', '{amount}', '{price}');";
            LoggingModule.Instance.AddToQueryLog(l_Query);
        }
        public static void AddRobLog(uint playerId, string type, int progress, bool finished)
        {
            var l_Query = $"INSERT INTO `log_rob` (`player_id`, `type`, `progress`, `finished`) VALUES ('{playerId}', '{type}', '{progress}', '{(finished ? "1" : "0")}');";
            LoggingModule.Instance.AddToQueryLog(l_Query);
        }
        public static void AddPackgunLog(uint playerId, uint itemId, int itemAmount)
        {
            var l_Query = $"INSERT INTO `log_packgun` (`player_id`, `item_id`, `amount`) VALUES ('{playerId}', '{itemId}', '{itemAmount}');";
            LoggingModule.Instance.AddToQueryLog(l_Query);
        }

        public static void AddSlotMachineGameToDbLog(DbPlayer dbPlayer, SlotMachineGame slotMachineGame)
        {
            var l_Query = $"INSERT INTO `kasino_games` (`player_id`, `einsatz`, `slot1`, `slot2`, `slot3`, `winsum`, `status`, `multiple`) VALUES ('{dbPlayer.Id}','{slotMachineGame.Einsatz}', '{slotMachineGame.Slot1}', '{slotMachineGame.Slot2}', '{slotMachineGame.Slot3}', '{slotMachineGame.WinSum}', '{slotMachineGame.Status}', '{slotMachineGame.Multiple}');";
            LoggingModule.Instance.AddToQueryLog(l_Query);
        }

        public static void AddNameChangeLog(uint playerId, int playerLevel, string oldName, string newName, bool marriage)
        {
            var l_Query = $"INSERT INTO `log_namechange` (`player_id`, `player_level`, `old_name`, `new_name`, `marriage`) VALUES ('{playerId}', '{playerLevel}', '{oldName}', '{newName}','{(marriage ? "1" : "0")}');";
            LoggingModule.Instance.AddToQueryLog(l_Query);
        }
        public static void AddDivorceLog(uint playerId, int playerLevel, uint targetId)
        {
            var l_Query = $"INSERT INTO `log_divorce` (`player_id`, `player_level`, `target_id`) VALUES ('{playerId}', '{playerLevel}', '{targetId}');";
            LoggingModule.Instance.AddToQueryLog(l_Query);
        }
        public static void AddDiceGameToDbLog(uint playerId, int einsatz, int memberAmount, int einsatzSum)
        {
            var l_Query = $"INSERT INTO `kasino_dice_games` (`einsatz`, `member_amount`, `einsatz_sum`, `winner_id`) VALUES ('{einsatz}', '{memberAmount}', '{einsatzSum}', '{playerId}');";
            LoggingModule.Instance.AddToQueryLog(l_Query);
        }
        public static void AddVehiclePurchaseLog(uint playerId, int carshopId, uint modelId, int price, uint teamId)
        {
            var l_Query = $"INSERT INTO `log_carshop_purchases` (`player_id`, `carshop_id`, `model`, `price`, `team_id`) VALUES ('{playerId}', '{carshopId}', '{modelId}', '{price}', '{teamId}');";
            LoggingModule.Instance.AddToQueryLog(l_Query);
        }
        public static void AddPlaytimeLog(uint playerId, int playTime, DateTime date)
        {
            var l_Query = $"INSERT INTO `log_playtime` (`player_id`, `minutes`, `loginDate`) VALUES ('{playerId}', '{playTime}', '{date.ToString("yyyy-MM-dd HH:mm:ss")}');";
            LoggingModule.Instance.AddToQueryLog(l_Query);
        }

        public static void AddSchwarzgeldLog(uint playerId, int amount, SGLog type)
        {
            var l_Query = $"INSERT INTO `log_schwarzgeld` (`player_id`, `amount`, `type`) VALUES ('{playerId}', '{amount}', '{(int)type}');";
            LoggingModule.Instance.AddToQueryLog(l_Query);
        }
        public static void AddVehicleClawLog(uint playerId, uint vehicleId, string reason, bool remove)
        {
            var l_Query = $"INSERT INTO `log_vehicleclaw` (`player_id`, `vehicle_id`, `reason`, `status`) VALUES ('{playerId}', '{vehicleId}', '{reason}', '{(remove ? 0 : 1)}');";
            LoggingModule.Instance.AddToQueryLog(l_Query);
        }

        public static void Print(string message)
        {
            NAPI.Util.ConsoleOutput(message);
        }

        public static void Crash(Exception ex)
        {
            Console.WriteLine(ex.ToString());
            SaveToDbLog(MySqlHelper.EscapeString(ex.ToString()));
        }
    }
}
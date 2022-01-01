using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.Configurations;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Stadthalle
{
    public class StadthalleModule : Module<StadthalleModule>
    {
        public static int PhoneNumberChangingMonths = 4;

        public void SavePlayerLastPhoneNumberChange(DbPlayer dbPlayer)
        {
            MySQLHandler.ExecuteAsync("UPDATE player SET `lasthandychange` = '" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "' WHERE id = '" + dbPlayer.Id + "';");
        }

        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {
            dbPlayer.LastPhoneNumberChange = reader.GetDateTime("lasthandychange");
        }
        

        public bool IsPhoneNumberAvailable(int number)
        {
            using (MySqlConnection conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"SELECT id FROM player WHERE handy = '{number}' LIMIT 1";
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            return false;
                        }
                    }
                    conn.Close();
                }
            }

            DbPlayer searchPlayer = Players.Players.Instance.GetPlayerByPhoneNumber((uint)number);
            if (searchPlayer != null) return false;

            return true;
        }
    }
    public class StadthalleEvents : Script
    {

        [RemoteEvent]
        public void changePhoneNumberRandom(Client player, string returnString)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            
            if(returnString.Length < 0 || returnString.Length > 20 || returnString.ToLower() != "kaufen")
            {
                return;
            }

            int money = 10000 * dbPlayer.Level;

            if (!dbPlayer.TakeBankMoney(money, "Telefonnummer Änderung"))
            {
                dbPlayer.SendNewNotification(MSG.Money.NotEnoughMoney(money));
                return;
            }

            Random rnd = new Random();

            int number = 0;
            while (number == 0)
            {
                number = rnd.Next(10000, 9999999);
                if (!StadthalleModule.Instance.IsPhoneNumberAvailable(number))
                {
                    number = 0;
                }
            }

            uint oldnumber = dbPlayer.handy[0];

            dbPlayer.handy[0] = Convert.ToUInt32(number);
            dbPlayer.Save();
            dbPlayer.SendNewNotification("Deine Nummer wurde geändert! (Neue Nummer: " + number + ")");

            MySQLHandler.ExecuteAsync($"INSERT INTO `log_phonenumberchange` (`player_id`, `handy_old`, `handy_new`) VALUES ('{dbPlayer.Id}', '{oldnumber}', '{number}');");

            dbPlayer.LastPhoneNumberChange = DateTime.Now;
            StadthalleModule.Instance.SavePlayerLastPhoneNumberChange(dbPlayer);
            return;
        }

        [RemoteEvent]
        public void changePhoneNumber(Client player, string returnString)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if(!UInt32.TryParse(returnString, out uint phoneNumber) || phoneNumber < 1000 || phoneNumber > 9999999)
            {
                dbPlayer.SendNewNotification("Die angegebene Telefonnummer ist ungültig!");
                return;
            }

            if (!StadthalleModule.Instance.IsPhoneNumberAvailable((int)phoneNumber))
            {
                dbPlayer.SendNewNotification("Die angegebene Telefonnummer ist bereits vergeben!");
                return;
            }

            int price = 0;
            if (phoneNumber > 1000 && phoneNumber < 9999) price = 200000 * dbPlayer.Level;
            else price = 25000 * dbPlayer.Level;

            if(!dbPlayer.TakeBankMoney(price, "Telefonnummer Änderung"))
            {
                dbPlayer.SendNewNotification(MSG.Money.NotEnoughMoney(price));
                return;
            }

            uint oldnumber = dbPlayer.handy[0];

            dbPlayer.handy[0] = Convert.ToUInt32(phoneNumber);
            dbPlayer.Save();
            dbPlayer.SendNewNotification("Deine Nummer wurde geändert! (Neue Nummer: " + phoneNumber + ", Kosten: $" + price + ")");

            MySQLHandler.ExecuteAsync($"INSERT INTO `log_phonenumberchange` (`player_id`, `handy_old`, `handy_new`) VALUES ('{dbPlayer.Id}', '{oldnumber}', '{phoneNumber}');");

            dbPlayer.LastPhoneNumberChange = DateTime.Now;
            StadthalleModule.Instance.SavePlayerLastPhoneNumberChange(dbPlayer);
        }
    }
}

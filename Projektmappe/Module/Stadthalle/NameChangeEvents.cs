using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using GTANetworkAPI;
using GTANetworkMethods;
using MySql.Data.MySqlClient;
using GVRP.Module.Configurations;
using GVRP.Module.Logging;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Stadthalle
{
    public class NameChangeEvents : Script
    {

        [RemoteEvent]
        public void NameChange(Client player, string returnString)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            int count = returnString.Count(f => f == '_');
            if (!Regex.IsMatch(returnString, @"^[a-zA-Z_-]+$") || count != 1)
            {
                dbPlayer.SendNewNotification("Bitte gib einen Namen in dem Format Max_Mustermann an.");
                return;
            }

            if (returnString.Length > 40 || returnString.Length < 7)
            {
                dbPlayer.SendNewNotification("Der Name ist zu lang oder zu kurz!");
                return;
            }
            using (MySqlConnection conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (MySqlCommand cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"SELECT name FROM player WHERE LOWER(name) = '{returnString.ToLower()}' LIMIT 1";
                using (MySqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        if (reader.HasRows)
                        {
                            dbPlayer.SendNewNotification("Dieser Name ist bereits vergeben.");
                            conn.Close();
                            return;
                        }
                    }
                    conn.Close();
                }
            }
            NameChangeFunctions.DoNameChange(dbPlayer, returnString, false);
        }


        [RemoteEvent]
        public void NameChangeMarryConfirm(Client p_Player, string p_InvitingPersonName, string p_CasinoDeviceId)
        {
            DbPlayer dbPlayer = p_Player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }

            if (dbPlayer.married[0] != 0)
            {
                string marryName = "";
                using (MySqlConnection conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = $"SELECT name FROM player WHERE id = '{dbPlayer.married[0]}' LIMIT 1";
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (reader.HasRows)
                            {
                                marryName = reader.GetString("name").Split("_")[1];
                            }
                        }
                        conn.Close();
                    }
                }

                if (!marryName.Equals(""))
                {
                    string newName = dbPlayer.Player.Name.Split("_")[0] + $"_{marryName}";

                    using (MySqlConnection conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                    using (MySqlCommand cmd = conn.CreateCommand())
                    {
                        conn.Open();
                        cmd.CommandText = $"SELECT name FROM player WHERE LOWER(name) = '{newName.ToLower()}' LIMIT 1";
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                if (reader.HasRows)
                                {
                                    dbPlayer.SendNewNotification("Dieser Name ist bereits vergeben.");
                                    conn.Close();
                                    return;
                                }
                            }
                            conn.Close();
                        }
                    }


                    NameChangeFunctions.DoNameChange(dbPlayer, newName, true);
                }
                return;
            }
            else
            {
                dbPlayer.SendNewNotification("Du bist nicht verheiratet. Komm bitte ohne Eheurkunde wieder.");
                return;
            }




        }


        [RemoteEvent]
        public void DivorceConfirm(Client p_Player, string p_InvitingPersonName, string p_CasinoDeviceId)
        {
            DbPlayer dbPlayer = p_Player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }


            if (dbPlayer.married[0] != 0)
            {
                string marryName = "";
                int marryLevel = 0;
                using (MySqlConnection conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = $"SELECT name, level FROM player WHERE id = '{dbPlayer.married[0]}' LIMIT 1";
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            if (reader.HasRows)
                            {
                                marryName = reader.GetString("name");
                                marryLevel = reader.GetInt32("level");
                            }
                        }
                        conn.Close();
                    }
                }
                if (marryName != "")
                {
                    if (!dbPlayer.TakeBankMoney(40000 * (dbPlayer.Level + marryLevel) / 2, $"Scheidung von - {marryName}"))
                    {
                        dbPlayer.SendNewNotification($"Die Scheidung würde {40000 * (dbPlayer.Level + marryLevel) / 2} $ kosten. Diese Summe hast du nicht auf dem Konto");
                        return;
                    }

                    dbPlayer.SendNewNotification($"Du hast dich erfolgreich von {marryName} scheiden lassen.");
                    
                    var findPlayer = Players.Players.Instance.FindPlayer(dbPlayer.married[0]);
                    if (findPlayer == null || !findPlayer.IsValid())
                    {
                        MySQLHandler.ExecuteAsync($"UPDATE player SET married = 0 WHERE id = '{dbPlayer.married[0]}'");
                    }
                    else
                    {
                        findPlayer.married[0] = 0;
                        findPlayer.SendNewNotification($"{dbPlayer.Player.Name} hat sich von dir scheiden lassen.");
                    }
                    
                    Logger.AddDivorceLog(dbPlayer.Id, dbPlayer.Level, dbPlayer.married[0]);
                    dbPlayer.married[0] = 0;

                    return;
                }



                return;
            }

            dbPlayer.SendNewNotification("Du bist nicht verheiratet... Wovon willst du dich scheiden?");
            return;




        }
    }
}

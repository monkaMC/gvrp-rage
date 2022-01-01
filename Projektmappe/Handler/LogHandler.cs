using System;
using MySql.Data.MySqlClient;

namespace GVRP
{
    public class LogHandler
    {
        public static void LogDeath(string u1, string u2, string weapon)
        {
            u1 = u1 ?? "undefined";
            u2 = u2 ?? "undefined";
            weapon = weapon ?? "undefined";

            String Query = $"INSERT INTO `log_death` (`user`, `killer`, `weapon`) VALUES('{MySqlHelper.EscapeString(u1)}', '{MySqlHelper.EscapeString(u2)}', '{MySqlHelper.EscapeString(weapon)}')";

            MySQLHandler.ExecuteAsync(Query);
        }

        public static void LogKilled(string u1, string u2, string weapon)
        {
            u1 = u1 ?? "undefined";
            u2 = u2 ?? "undefined";
            weapon = weapon ?? "undefined";
            if (u2 == "") u2 = "None";
            if (weapon == "") weapon = "None";

            String Query = $"INSERT INTO `log_killed` (`user`, `killer`, `weapon`) VALUES('{MySqlHelper.EscapeString(u1)}', '{MySqlHelper.EscapeString(u2)}', '{MySqlHelper.EscapeString(weapon)}')";

            MySQLHandler.ExecuteAsync(Query);
        }

        public static void LogAsi(string username, string asi)
        {
            username = username ?? "undefined";
            asi = asi ?? "undefined";

            MySQLHandler.ExecuteAsync($"INSERT INTO `log_asi` (`name`, `asi`) VALUES('{MySqlHelper.EscapeString(username)}', '{MySqlHelper.EscapeString(asi)}')");
        }

        public static void LogFakename(uint p_PlayerID, string p_PlayerName, string p_FakeName)
        {
            string p_ID = p_PlayerID.ToString() ?? "undefined";
            p_PlayerName = p_PlayerName ?? "undefined";
            p_FakeName = p_FakeName ?? "undefined";
            
            string l_Query =
                $"INSERT INTO `log_fakename` (`player_id`, `player_name`, `fake_name`) VALUES ('{MySqlHelper.EscapeString(p_ID)}', '{MySqlHelper.EscapeString(p_PlayerName)}', '{MySqlHelper.EscapeString(p_FakeName)}');";

            MySQLHandler.ExecuteAsync(l_Query);
        }
    }
}
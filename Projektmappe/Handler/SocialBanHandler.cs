using MySql.Data.MySqlClient;
using System;
using GTANetworkAPI;
using GVRP.Module.Configurations;
using GVRP.Module.Players.Db;
using GVRP.Module.Players;

namespace GVRP
{
    public sealed class SocialBanHandler
    {
        public static SocialBanHandler Instance { get; } = new SocialBanHandler();

        private SocialBanHandler()
        {
        }

        public void AddEntry(Client player)
        {
            MySQLHandler.ExecuteAsync(
                $"INSERT INTO socialbans (Name) VALUES ('{player.SocialClubName}');");
        }

        public bool IsPlayerSocialBanned(Client player)
        {
            if (player == null || player.SocialClubName == "" || player.SocialClubName == null) return false;

            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"SELECT * FROM socialbans WHERE Name = '{MySqlHelper.EscapeString(player.SocialClubName)}';";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        return true;
                    }
                }
                conn.Close();
            }

            return false;
        }

        public bool IsPlayerWhitelisted(Client player)
        {
            if (player == null || player.Address == "" || player.Address == null) return false;

            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"SELECT * FROM ipwhitelist WHERE ip = '{MySqlHelper.EscapeString(player.Address)}';";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        return true;
                    }
                }
                conn.Close();
            }

            return false;
        }

        public void DeleteEntry(Client player)
        {
            var query =
                $"DELETE FROM socialbans WHERE Name = '{player.SocialClubName}';";
            MySQLHandler.ExecuteAsync(query);
        }
    }
}
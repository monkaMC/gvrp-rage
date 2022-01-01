using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GVRP.Module.Configurations;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Players
{
    public class CustomData
    {
        public uint Id { get; }

        public uint PlayerId { get; }
        public string Address { get; set; }
        public string Membership { get; set; }
        public string Phone { get; set; }
        public string Info { get; set; }

        // Optional non DB
        public bool CanAktenView { get; set; }

        public CustomData(uint playerId, string address, string membership, string phone, string info, bool canAktenView)
        {
            PlayerId = playerId;
            Address = address;
            Membership = membership;
            Phone = phone;
            Info = info;
            CanAktenView = canAktenView;
        }
    }

    public static class CustomDataPlayerExtension
    {
        public static CustomData LoadCustomData(this DbPlayer dbPlayer)
        {
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"SELECT * FROM `player_customdata` WHERE player_id = '{dbPlayer.Id}' LIMIT 1;";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows && reader.Read())
                    {
                        return new CustomData(reader.GetUInt32("player_id"), reader.GetString("address"), reader.GetString("membership"), reader.GetString("phone"), reader.GetString("info"), false);
                    }
                    else
                    {
                        MySQLHandler.ExecuteAsync($"INSERT INTO player_customdata (`player_id`, `address`, `membership`, `phone`, `info`) VALUES ('{dbPlayer.Id}', '', '', '', '')");
                        return new CustomData(dbPlayer.Id, "", "", "", "", false);
                    }
                }
            }
        }

        public static void UpdateCustomData(this DbPlayer dbPlayer, string address, string membership, string phone, string info)
        {
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            dbPlayer.CustomData.Address = address;
            dbPlayer.CustomData.Membership = membership;
            dbPlayer.CustomData.Phone = phone;
            dbPlayer.CustomData.Info = info;

            MySQLHandler.ExecuteAsync($"UPDATE player_customdata SET `address` = '{dbPlayer.CustomData.Address}', `membership` = '{dbPlayer.CustomData.Membership}', `phone` = '{dbPlayer.CustomData.Phone}', `info` = '{dbPlayer.CustomData.Info}' WHERE player_id = '{dbPlayer.Id}'");
        }
    }
}
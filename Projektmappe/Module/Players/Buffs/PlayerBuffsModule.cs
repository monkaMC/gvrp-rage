using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkMethods;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using GVRP.Module.Configurations;
using GVRP.Module.Items;
using GVRP.Module.Logging;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Players.Buffs
{
    public class PlayerBuffsModule : Module<PlayerBuffsModule>
    {
        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader Oldreader)
        {
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"SELECT data FROM `player_buffs` WHERE player_id = '{dbPlayer.Id}' LIMIT 1;";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows && reader.Read())
                    {
                        try
                        {
                            var playerBuffs = JsonConvert.DeserializeObject<PlayerBuffs>(reader.GetString(0)) ??
                                              new PlayerBuffs();
                            dbPlayer.Buffs = playerBuffs;
                        }
                        catch (Exception exception)
                        {
                            // Wenns nicht geht create kappa
                            dbPlayer.ResetBuffs();

                            Logger.Crash(exception);
                        }
                    }
                    else
                    {
                        dbPlayer.CreateBuffs();
                    }
                }
            }
        }

        public override void OnPlayerMinuteUpdate(DbPlayer dbPlayer)
        {
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (dbPlayer.HasPlayerBuffToWarp())
            {
                // Drug Infection
                if (dbPlayer.Buffs.DrugsInfected > 0)
                {
                    dbPlayer.Buffs.DrugsInfected = dbPlayer.Buffs.DrugsInfected - 1;
                }

                // Joint
                if (dbPlayer.Buffs.JointBuff > 0)
                {
                    dbPlayer.Buffs.JointBuff -= 1;

                    int playerArmor = dbPlayer.Player.Armor;

                    if (playerArmor < 80)
                    {
                        playerArmor = playerArmor + 10 > 80 ? 80 : playerArmor + 10;
                        dbPlayer.SetArmor(playerArmor);
                    }
                }

                // Meth & Weed Drugs
                if (dbPlayer.Buffs.DrugBuff > 0)
                {
                    CustomDrugModule.Instance.SetCustomDrugEffect(dbPlayer);
                    if(dbPlayer.Buffs.DrugBuff == 1)
                    {
                        CustomDrugModule.Instance.RemoveEffect(dbPlayer);
                    }
                    dbPlayer.Buffs.DrugBuff--;
                }

                if (dbPlayer.Buffs.GCocain > 0) dbPlayer.Buffs.GCocain--;
                if (dbPlayer.Buffs.Cocain > 0) dbPlayer.Buffs.Cocain--;

                dbPlayer.SaveBuffs();
            }
        }
    }

    public static class PlayerBuffsPlayerExtension
    {
        public static void SaveCustomDrugsCreation(this DbPlayer dbPlayer)
        {
            MySQLHandler.ExecuteAsync("UPDATE player SET `drugcreatelast` = '" + dbPlayer.DrugCreateLast.Date.ToString("yyyy-MM-dd HH:mm:ss") + "' WHERE id = '" + dbPlayer.Id + "';");
        }

        public static void CreateBuffs(this DbPlayer iPlayer)
        {
            iPlayer.Buffs = new PlayerBuffs();
            try
            {
                var playerDataString = JsonConvert.SerializeObject(iPlayer.Buffs) ?? "";
                var query =
                    $"INSERT INTO `player_buffs` (data, player_id) VALUES ('{playerDataString}', '{iPlayer.Id}');";
                MySQLHandler.ExecuteAsync(query);
            }
            catch (Exception exception)
            {
                Logger.Crash(exception);
            }
        }
        
        public static bool HasPlayerBuffToWarp(this DbPlayer dbPlayer)
        {
            return (dbPlayer.Buffs.JointBuff > 0 || dbPlayer.Buffs.DrugsInfected > 0 || dbPlayer.Buffs.Cocain > 0 || dbPlayer.Buffs.GCocain > 0 || dbPlayer.Buffs.DrugBuff > 0);
        }
        
        public static void IncreasePlayerDrugInfection(this DbPlayer dbPlayer)
        {
            var rnd = new Random();
            dbPlayer.Buffs.DrugsInfected += rnd.Next(3, 7);
            if (dbPlayer.Buffs.DrugsInfected > 60) dbPlayer.Buffs.DrugsInfected = 60;
            dbPlayer.SaveBuffs();
        }
        
        public static bool Drugtest(this DbPlayer dbPlayer)
        {
            return dbPlayer.Buffs.DrugsInfected > 0 || dbPlayer.Buffs.DrugBuff > 0 || dbPlayer.Buffs.Cocain > 0 || dbPlayer.Buffs.GCocain > 0;
        }

        public static bool HasCustomDrugBuff(this DbPlayer dbPlayer)
        {
            return dbPlayer.Buffs.DrugBuff > 0;
        }

        public static void SaveBuffs(this DbPlayer dbPlayer)
        {
            var playerDataString = JsonConvert.SerializeObject(dbPlayer.Buffs) ?? "";

            MySQLHandler.ExecuteAsync($"UPDATE `player_buffs` SET data = '{playerDataString}' WHERE `player_id` = '{dbPlayer.Id}';");
        }
        
        public static void ResetBuffs(this DbPlayer dbPlayer)
        {
            dbPlayer.Buffs = new PlayerBuffs();
            var playerDataString = JsonConvert.SerializeObject(dbPlayer.Buffs) ?? "";

            MySQLHandler.ExecuteAsync($"UPDATE `player_buffs` SET data = '{playerDataString}' WHERE `player_id` = '{dbPlayer.Id}';");
        }
    }
}

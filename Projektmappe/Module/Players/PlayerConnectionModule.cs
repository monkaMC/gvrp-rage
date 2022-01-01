using System;
using System.Collections.Generic;
using GVRP.Module.Configurations;
using GVRP.Module.Logging;
using GVRP.Module.Players.Db;
using GVRP.Module.Time;

namespace GVRP.Module.Players
{
    // This module has to use the smallest order to execute as last 
    //TODO: to prevent an never happening disconnect event cleanup none removed players in sync thread and call on player disconnected
    // This maks sure that an player only gets setted offline after his full data, inclusive the inventory is fully saved before he connects to an different or same server 
    //TODO: log execution order to make sure this module is always called at last
    public class PlayerConnectionModule : Module<PlayerConnectionModule>
    {

        public Dictionary<DbPlayer, DateTime> Players = new Dictionary<DbPlayer, DateTime>();


        public override int GetOrder()
        {
            return -1;
        }

        public override void OnPlayerConnected(DbPlayer dbPlayer)
        {
            if (!ServerFeatures.IsActive("playtime")) return;
            Players.Add(dbPlayer, DateTime.Now);
        }

        //TODO: disconnect is currently on main thread, but should be an task, none threaded stuff should happen in main event everything else in disconnect task
        public override void OnPlayerDisconnected(DbPlayer dbPlayer, string reason)
        {
            if(dbPlayer == null || !dbPlayer.IsValid())
            {
                return;
            }
            //Set offline
            var query =
                $"UPDATE `player` SET `Online` = '{0}' WHERE `id` = '{dbPlayer.Id}';";

            MySQLHandler.ExecuteAsync(query);

            if (!ServerFeatures.IsActive("playtime")) return;

            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (Players.ContainsKey(dbPlayer))
            {
                if (Players.TryGetValue(dbPlayer, out DateTime date))
                {
                    int minutes = (int) DateTime.Now.Subtract(date).TotalMinutes;
                    if (minutes == 0) return;
                    Logger.AddPlaytimeLog(dbPlayer.Id, minutes, date);
                    Players.Remove(dbPlayer);
                }
            }
        }
    }
}
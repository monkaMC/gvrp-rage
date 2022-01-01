using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GVRP.Handler;
using GVRP.Module.Chat;
using GVRP.Module.Doors;
using GVRP.Module.GTAN;
using GVRP.Module.Injury;
using GVRP.Module.Laboratories;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.JumpPoints;
using GVRP.Module.Players.PlayerAnimations;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> KisteMethamphetamin(DbPlayer dbPlayer, ItemModel ItemData)
        {
            int time = 5000;
            Chats.sendProgressBar(dbPlayer, time);
            Item item = null;
            MethlaboratoryModule.EndProductItemIds.ForEach(id =>
            {
                if (item == null)
                {
                    item = dbPlayer.Container.GetItemById((int)id);
                }
            });
            if (item == null)
            {
                dbPlayer.SendNewNotification("Nichts zum Analysieren gefunden.");
                return false;
            }

            dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "missheistdockssetup1ig_3@talk", "oh_hey_vin_dockworker");
            dbPlayer.Player.TriggerEvent("freezePlayer", true);
            dbPlayer.SetData("userCannotInterrupt", true);

            await Task.Delay(time);
            if (dbPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(dbPlayer.Player) || !dbPlayer.Player.Exists) return false;
            dbPlayer.ResetData("userCannotInterrupt");
            if (dbPlayer.IsCuffed || dbPlayer.IsTied || dbPlayer.isInjured()) return false;

            dbPlayer.Player.TriggerEvent("freezePlayer", false);

            if (item == null)
            {
                dbPlayer.SendNewNotification("Nichts zum Wiegen gefunden.");
                return false;
            }
            foreach (KeyValuePair<string, dynamic> keyValuePair in item.Data)
            {
                if (keyValuePair.Key == "amount")
                {
                    string value = Convert.ToString(keyValuePair.Value);
                    dbPlayer.SendNewNotification($"In dieser Kiste befinden sich {value} Kristalle");
                }
            }
            NAPI.Player.StopPlayerAnimation(dbPlayer.Player);
            return true;
        }
    }
}

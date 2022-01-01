using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GTANetworkAPI;
using GVRP.Module.Chat;
using GVRP.Module.Configurations;
using GVRP.Module.Injury;
using GVRP.Module.Laboratories;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.PlayerAnimations;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> Zentrifuge(DbPlayer dbPlayer, ItemModel itemModel)
        {
            if (dbPlayer.DimensionType[0] == DimensionType.Methlaboratory)
            {
                if (dbPlayer.Player.Position.DistanceTo(Coordinates.MethlaboratoryAnalyzePosition) > 2.0f)
                {
                    dbPlayer.SendNewNotification("Die Zentrifuge muss am richtigen Ort verwendet werden (blauer Kreis)");
                    return false;
                }
                Chats.sendProgressBar(dbPlayer, LaboratoryModule.TimeToAnalyze);
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

                await Task.Delay(LaboratoryModule.TimeToAnalyze);
                if (dbPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(dbPlayer.Player) || !dbPlayer.Player.Exists) return false;
                dbPlayer.ResetData("userCannotInterrupt");

                if (dbPlayer.IsCuffed || dbPlayer.IsTied || dbPlayer.isInjured()) return true;

                dbPlayer.Player.TriggerEvent("freezePlayer", false);

                if (item == null)
                {
                    dbPlayer.SendNewNotification("Nichts zum Analysieren gefunden.");
                    return false;
                }
                string message = "Die Analyse hat folgendes ergeben:  ";
                if (!item.Data.TryGetValue("quality", out dynamic quality) || !item.Data.TryGetValue("amount", out dynamic amount)) return false;

                message += $"Reinheitsgrad ({quality * 100}%), Menge ({amount} Kristalle)";
                dbPlayer.SendNewNotification(message, duration: 30000);
                NAPI.Player.StopPlayerAnimation(dbPlayer.Player);
                return true;
            }
            else if (dbPlayer.DimensionType[0] == DimensionType.Cannabislaboratory)
            {
                if (dbPlayer.Player.Position.DistanceTo(Coordinates.CannabislaboratoryComputerPosition) > 2.0f)
                {
                    dbPlayer.SendNewNotification("Die Zentrifuge muss am richtigen Ort verwendet werden (Computer)");
                    return false;
                }
                Chats.sendProgressBar(dbPlayer, LaboratoryModule.TimeToAnalyze);
                Item item = null;
                CannabislaboratoryModule.EndProductItemIds.ForEach(id =>
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

                await Task.Delay(LaboratoryModule.TimeToAnalyze);
                if (dbPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(dbPlayer.Player) || !dbPlayer.Player.Exists) return false;
                dbPlayer.ResetData("userCannotInterrupt");

                if (dbPlayer.IsCuffed || dbPlayer.IsTied || dbPlayer.isInjured()) return true;

                dbPlayer.Player.TriggerEvent("freezePlayer", false);
                
                string message = "Die Analyse hat folgendes ergeben:  ";
                if (!item.Data.TryGetValue("quality", out dynamic quality) || !item.Data.TryGetValue("amount", out dynamic amount)) return false;

                message += $"Reinheitsgrad ({quality * 100}%), Menge ({amount} Kristalle)";
                dbPlayer.SendNewNotification(message, duration: 30000);
                NAPI.Player.StopPlayerAnimation(dbPlayer.Player);
                return true;
            }
            return false;
        }
    }
}
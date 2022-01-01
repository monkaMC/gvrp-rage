using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.Crime;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Teams;

namespace GVRP.Module.NSA
{
    public static class NSAPlayerExtension
    {
        public static bool HasMoneyTransferWantedStatus(this DbPlayer dbPlayer)
        {
            return CrimeModule.Instance.CalcJailTime(dbPlayer.Crimes) > 30;
        }

        public static bool IsMasked(this DbPlayer iPlayer)
        {
            return (iPlayer != null && iPlayer.IsValid() && iPlayer.Character != null && iPlayer.Character.ActiveClothes != null && iPlayer.Character.ActiveClothes.ContainsKey(1) && iPlayer.Character.ActiveClothes[1]);
        }

        public static void AddTransferHistory(string Description, Vector3 position)
        {
            NSAModule.TransactionHistory.Add(new TransactionHistoryObject() { Description = Description, Added = DateTime.Now, Position = position, TransactionType = TransactionType.MONEY });
        }
        
        public static void AddEnergyHistory(string Description, Vector3 position)
        {
            NSAModule.TransactionHistory.Add(new TransactionHistoryObject() { Description = Description, Added = DateTime.Now, Position = position, TransactionType = TransactionType.ENERGY });
        }
        public static void SetGovLevel(this DbPlayer dbPlayer, string Level)
        {
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            MySQLHandler.ExecuteAsync($"UPDATE player SET gov_level = '{Level}' WHERE id = '{dbPlayer.Id}';");

            dbPlayer.GovLevel = Level;
            return;
        }


        public static void RemoveGovLevel(this DbPlayer dbPlayer)
        {
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            MySQLHandler.ExecuteAsync($"UPDATE player SET gov_level = '' WHERE id = '{dbPlayer.Id}'");

            dbPlayer.GovLevel = "";
            return;
        }
    }
}

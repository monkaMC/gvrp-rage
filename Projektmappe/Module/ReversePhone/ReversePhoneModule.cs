using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Handler;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Items;
using GVRP.Module.Logging;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.ReversePhone
{
    public class PhoneHistoryEntry
    {
        public int Number { get; set; }
        public int Dauer { get; set; }
        public DateTime Time { get; set; }

        public PhoneHistoryEntry(int number, int dauer)
        {
            Number = number;
            Dauer = dauer;
            Time = DateTime.Now;
        }
    }

    public sealed class ReversePhoneModule : Module<ReversePhoneModule>
    {
        public Dictionary<uint, List<PhoneHistoryEntry>> phoneHistory = new Dictionary<uint, List<PhoneHistoryEntry>>();

        public override bool Load(bool reload = false)
        {
            phoneHistory = new Dictionary<uint, List<PhoneHistoryEntry>>();
            return true;
        }

        public void AddPhoneHistory(DbPlayer iPlayer, int number, int dauer)
        {
            if (!phoneHistory.ContainsKey(iPlayer.Id)) phoneHistory.Add(iPlayer.Id, new List<PhoneHistoryEntry>());

            phoneHistory[iPlayer.Id].Add(new PhoneHistoryEntry(number, dauer));
        }
    }
}
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GVRP.Handler;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Menu;
using GVRP.Module.NSA.Observation;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Windows;
using GVRP.Module.Telefon.App;

namespace GVRP.Module.NSA.Menu
{
    public class NSAListCallsMenuBuilder : MenuBuilder
    {
        public NSAListCallsMenuBuilder() : base(PlayerMenu.NSACallListMenu)
        {

        }

        public override Module.Menu.Menu Build(DbPlayer p_DbPlayer)
        {
            var l_Menu = new Module.Menu.Menu(Menu, "NSA Offene Anrufe");
            l_Menu.Add($"Schließen");

            List<uint> tmpList = new List<uint>();
            foreach (DbPlayer xPlayer in Players.Players.Instance.GetValidPlayers())
            {
                if (!xPlayer.HasData("current_caller")) continue;
                if (tmpList.Contains(xPlayer.handy[0]) || tmpList.Contains((uint)xPlayer.GetData("current_caller"))) continue;

                DbPlayer ConPlayer = TelefonInputApp.GetPlayerByPhoneNumber(xPlayer.GetData("current_caller"));
                if (ConPlayer == null || !ConPlayer.IsValid()) continue;

                if (NSAObservationModule.ObservationList.Where(o => o.Value.PlayerId == ConPlayer.Id || o.Value.PlayerId == xPlayer.Id).Count() == 0) continue;

                tmpList.Add(xPlayer.handy[0]);
                tmpList.Add((uint)xPlayer.GetData("current_caller"));

                l_Menu.Add($"{xPlayer.GetName()} ({xPlayer.handy[0]}) == {ConPlayer.GetName()} ({ConPlayer.handy[0]})");
            }

            return l_Menu;
        }

        public override IMenuEventHandler GetEventHandler()
        {
            return new EventHandler();
        }

        private class EventHandler : IMenuEventHandler
        {
            public bool OnSelect(int index, DbPlayer iPlayer)
            {
                MenuManager.DismissCurrent(iPlayer);
                return true;
            }
        }
    }
}

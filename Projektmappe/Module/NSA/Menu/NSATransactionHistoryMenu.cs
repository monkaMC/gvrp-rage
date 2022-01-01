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
    public class NSATransactionHistoryMenuBuilder : MenuBuilder
    {
        public NSATransactionHistoryMenuBuilder() : base(PlayerMenu.NSATransactionHistory)
        {

        }

        public override Module.Menu.Menu Build(DbPlayer p_DbPlayer)
        {
            var l_Menu = new Module.Menu.Menu(Menu, "IAA Transfer History");
            l_Menu.Add($"Schließen");

            foreach (TransactionHistoryObject transactionHistoryObject in NSAModule.TransactionHistory.ToList().Where(t => t.TransactionType == TransactionType.MONEY))
            {
                l_Menu.Add($"{transactionHistoryObject.Description} - {transactionHistoryObject.Added.ToShortTimeString()}");
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
                if (index == 0)
                {
                    MenuManager.DismissCurrent(iPlayer);
                    return true;
                }
                else
                {
                    int idx = 1;
                    foreach (TransactionHistoryObject transactionHistoryObject in NSAModule.TransactionHistory.ToList().Where(t => t.TransactionType == TransactionType.MONEY))
                    {
                        if (idx == index)
                        {
                            iPlayer.Player.TriggerEvent("setPlayerGpsMarker", transactionHistoryObject.Position.X, transactionHistoryObject.Position.Y);
                            return false;
                        }
                        idx++;
                    }
                }
                MenuManager.DismissCurrent(iPlayer);
                return true;
            }
        }
    }
}

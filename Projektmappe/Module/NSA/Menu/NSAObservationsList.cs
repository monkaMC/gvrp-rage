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
    public class NSAObservationsListMenuBuilder : MenuBuilder
    {
        public NSAObservationsListMenuBuilder() : base(PlayerMenu.NSAObservationsList)
        {

        }

        public override Module.Menu.Menu Build(DbPlayer p_DbPlayer)
        {
            var l_Menu = new Module.Menu.Menu(Menu, "NSA Observationen");
            l_Menu.Add($"Schließen");

            foreach (NSAObservation nSAObservation in NSAObservationModule.ObservationList.Values.ToList())
            {
                DbPlayer targetOne = Players.Players.Instance.FindPlayerById(nSAObservation.PlayerId);
                if (targetOne == null || !targetOne.IsValid()) continue;

                l_Menu.Add($"{targetOne.Id} {targetOne.GetName()}");
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
                    foreach (NSAObservation nSAObservation in NSAObservationModule.ObservationList.Values.ToList())
                    {
                        DbPlayer targetOne = Players.Players.Instance.FindPlayerById(nSAObservation.PlayerId);
                        if (targetOne == null || !targetOne.IsValid()) continue;

                        if (idx == index)
                        {
                            // Targetplayer Submenu...
                            iPlayer.SetData("nsa_target_player_id", targetOne.Id);
                            Module.Menu.MenuManager.Instance.Build(GVRP.Module.Menu.PlayerMenu.NSAObservationsSubMenu, iPlayer).Show(iPlayer);
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

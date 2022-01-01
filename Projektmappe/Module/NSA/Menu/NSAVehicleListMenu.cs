using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Handler;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Windows;

namespace GVRP.Module.NSA.Menu
{
    public class NSAVehicleListMenuBuilder : MenuBuilder
    {
        public NSAVehicleListMenuBuilder() : base(PlayerMenu.NSAVehicleListMenu)
        {

        }

        public override Module.Menu.Menu Build(DbPlayer p_DbPlayer)
        {
            var l_Menu = new Module.Menu.Menu(Menu, "NSA Fahrzeugverwaltung");
            l_Menu.Add($"Schließen");
            
            foreach(SxVehicle sxVehicle in VehicleHandler.Instance.GetClosestVehiclesFromTeam(p_DbPlayer.Player.Position, (int)teams.TEAM_FIB, 10.0f))
            {
                l_Menu.Add($"{sxVehicle.GetName()} ({sxVehicle.databaseId})");
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
                if(index >= 1)
                {
                    int idx = 1;
                    foreach (SxVehicle sxVehicle in VehicleHandler.Instance.GetClosestVehiclesFromTeam(iPlayer.Player.Position, (int)teams.TEAM_FIB, 10.0f))
                    {
                        if(index == idx)
                        {
                            MenuManager.DismissCurrent(iPlayer);

                            iPlayer.SetData("nsa_work_vehicle", sxVehicle.databaseId);

                            MenuManager.DismissCurrent(iPlayer);

                            Module.Menu.MenuManager.Instance.Build(GVRP.Module.Menu.PlayerMenu.NSAVehicleModifyMenu, iPlayer).Show(iPlayer);
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

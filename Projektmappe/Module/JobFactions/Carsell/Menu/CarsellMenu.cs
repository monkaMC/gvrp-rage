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
using GVRP.Module.Vehicles.Data;

namespace GVRP.Module.Carsell.Menu
{
    public class CarsellMenuBuilder : MenuBuilder
    {
        public CarsellMenuBuilder() : base(PlayerMenu.CarsellMenu)
        {

        }

        public override Module.Menu.Menu Build(DbPlayer p_DbPlayer)
        {
            var l_Menu = new Module.Menu.Menu(Menu, "Verwaltung");

            l_Menu.Add($"Fahrzeug bestellen");
            l_Menu.Add($"Fahrzeug entfernen");
            l_Menu.Add($"Lieferfahrzeuge");

            l_Menu.Add($"Schließen");
            
            return l_Menu;
        }

        public override IMenuEventHandler GetEventHandler()
        {
            return new EventHandler();
        }

        private class EventHandler : IMenuEventHandler
        {
            public bool OnSelect(int index, DbPlayer dbPlayer)
            {
                if(index == 0)
                {
                    Module.Menu.MenuManager.Instance.Build(GVRP.Module.Menu.PlayerMenu.CarsellBuyMenu, dbPlayer).Show(dbPlayer);
                    return false;
                }
                else if (index == 1)
                {
                    Module.Menu.MenuManager.Instance.Build(GVRP.Module.Menu.PlayerMenu.CarsellDeleteMenu, dbPlayer).Show(dbPlayer);
                    return false;
                }
                else if (index == 2)
                {
                    Module.Menu.MenuManager.Instance.Build(GVRP.Module.Menu.PlayerMenu.CarsellDeliverCustomerMenu, dbPlayer).Show(dbPlayer);
                    return false;
                }
                MenuManager.DismissCurrent(dbPlayer);
                return true;
            }
        }
    }
}

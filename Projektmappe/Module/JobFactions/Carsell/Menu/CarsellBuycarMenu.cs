using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GVRP.Handler;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.JobFactions.Carsell;
using GVRP.Module.Menu;
using GVRP.Module.NSA.Observation;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Windows;
using GVRP.Module.Teams.Shelter;
using GVRP.Module.Telefon.App;
using GVRP.Module.Vehicles.Data;

namespace GVRP.Module.Carsell.Menu
{
    public class CarsellBuycarMenuBuilder : MenuBuilder
    {
        public CarsellBuycarMenuBuilder() : base(PlayerMenu.CarsellBuyMenu)
        {

        }

        public override Module.Menu.Menu Build(DbPlayer p_DbPlayer)
        {
            var l_Menu = new Module.Menu.Menu(Menu, "Fahrzeug bestellen");
            l_Menu.Add($"Schließen");

            foreach(VehicleCarsellCategory vehicleCarsellCategory in VehicleCarsellCategoryModule.Instance.GetAll().Values)
            {
                l_Menu.Add($"{vehicleCarsellCategory.Name}");
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
                if(index == 0)
                {
                    MenuManager.DismissCurrent(iPlayer);
                    return true;
                }

                int idx = 1;

                foreach (VehicleCarsellCategory vehicleCarsellCategory in VehicleCarsellCategoryModule.Instance.GetAll().Values)
                {
                    if(idx == index)
                    {
                        iPlayer.SetData("carsellCat", vehicleCarsellCategory.Id);
                        Module.Menu.MenuManager.Instance.Build(GVRP.Module.Menu.PlayerMenu.CarsellBuySubMenu, iPlayer).Show(iPlayer);
                        return false;
                    }
                    idx++;
                }

                MenuManager.DismissCurrent(iPlayer);
                return true;
            }
        }
    }
}

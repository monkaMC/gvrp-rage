using System;
using System.Collections.Generic;
using System.Linq;
using GVRP.Module.Assets.Tattoo;
using GVRP.Module.Business;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.GTAN;
using GVRP.Module.Houses;
using GVRP.Module.Items;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Windows;
using GVRP.Module.Tattoo;

namespace GVRP.Module.Warehouse
{
    public class WarehouseMenuBuilder : MenuBuilder
    {
        public WarehouseMenuBuilder() : base(PlayerMenu.WarehouseMenu)
        {
        }

        public override Menu.Menu Build(DbPlayer iPlayer)
        {
            Warehouse warehouse = WarehouseModule.Instance.GetThis(iPlayer.Player.Position);
            if (warehouse == null) return null;

            var menu = new Menu.Menu(Menu, "Warenlager");

            menu.Add($"Schließen");
            menu.Add($"Waren Ankauf");
            menu.Add($"Waren verkauf");
            return menu;
        }

        public override IMenuEventHandler GetEventHandler()
        {
            return new EventHandler();
        }

        private class EventHandler : IMenuEventHandler
        {
            public bool OnSelect(int index, DbPlayer iPlayer)
            {
                Warehouse warehouse = WarehouseModule.Instance.GetThis(iPlayer.Player.Position);
                if (warehouse == null) return false;
                if (index == 0)
                {
                    MenuManager.DismissCurrent(iPlayer);
                    return false;
                }
                else if(index == 1) // ankauf Menu
                {
                    MenuManager.Instance.Build(PlayerMenu.WarehouseBuyMenu, iPlayer).Show(iPlayer);
                    return false;
                }
                else // VK Menu
                {
                    MenuManager.Instance.Build(PlayerMenu.WarehouseSellMenu, iPlayer).Show(iPlayer);
                    return false;
                }
            }
        }
    }
}
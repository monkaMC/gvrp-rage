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
    public class WarehouseBuyMenuBuilder : MenuBuilder
    {
        public WarehouseBuyMenuBuilder() : base(PlayerMenu.WarehouseBuyMenu)
        {
        }

        public override Menu.Menu Build(DbPlayer iPlayer)
        {
            Warehouse warehouse = WarehouseModule.Instance.GetThis(iPlayer.Player.Position);
            if (warehouse == null) return null;

            var menu = new Menu.Menu(Menu, "Warenlager Ankauf");

            menu.Add($"Schließen");

            foreach(WarehouseItem warehouseItem in warehouse.WarehouseItems)
            {
                menu.Add($"{ItemModelModule.Instance.Get(warehouseItem.RequiredItemId).Name} ${warehouseItem.RequiredItemPrice}");
            }

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
                else
                {
                    int idx = 1;
                    foreach (WarehouseItem warehouseItem in warehouse.WarehouseItems)
                    {
                        if (idx == index)
                        {
                            // Get Item Amount Player Has
                            int amount = iPlayer.Container.GetItemAmount(warehouseItem.RequiredItemId);
                            int buyamount = (amount / warehouseItem.RequiredToResultItemAmount); // zb nur 5er stacks... x.x

                            if(buyamount > 0 && amount >= warehouseItem.RequiredToResultItemAmount)
                            {
                                int realamount = buyamount * warehouseItem.RequiredToResultItemAmount;
                                int playerResultPrice = realamount * warehouseItem.RequiredItemPrice;

                                // Remove Players Items...
                                iPlayer.Container.RemoveItem(warehouseItem.RequiredItemId, realamount);

                                // Add To bestand..
                                warehouseItem.ResultItemBestand += buyamount;
                                warehouseItem.UpdateBestand();

                                iPlayer.GiveMoney(playerResultPrice);
                                iPlayer.SendNewNotification($"Sie haben {realamount} {ItemModelModule.Instance.Get(warehouseItem.RequiredItemId).Name} für ${playerResultPrice} verkauft!");
                            }
                            return true;
                        }
                        idx++;
                    }
                    return true;
                }
            }
        }
    }
}
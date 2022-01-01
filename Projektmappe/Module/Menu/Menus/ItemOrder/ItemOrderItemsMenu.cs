using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GVRP.Module.Chat;
using GVRP.Module.Gangwar;
using GVRP.Module.Items;
using GVRP.Module.Logging;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Schwarzgeld;

namespace GVRP
{
    public class ItemOrderItemsMenuBuilder : MenuBuilder
    {
        public ItemOrderItemsMenuBuilder() : base(PlayerMenu.ItemOrderItemsMenu)
        {
        }

        public override Menu Build(DbPlayer iPlayer)
        {
            ItemOrderNpc itemOrderNpc = ItemOrderNpcModule.Instance.GetByPlayerPosition(iPlayer);
            if (itemOrderNpc == null) return null;

            var menu = new Menu(Menu, "Verarbeitung");

            if (itemOrderNpc.RequiredTeams.Count == 0 || itemOrderNpc.RequiredTeams.Contains((int)iPlayer.TeamId))
            {
                foreach (ItemOrderNpcItem npcItem in itemOrderNpc.NpcItems.Where(i => iPlayer.TeamRank >= i.RangRestricted))
                {
                    menu.Add($"{npcItem.RewardItemAmount} {npcItem.RewardItem.Name} {npcItem.Hours}h");
                }
            }

            menu.Add(MSG.General.Close());
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
                ItemOrderNpc itemOrderNpc = ItemOrderNpcModule.Instance.GetByPlayerPosition(iPlayer);
                if (itemOrderNpc == null) return false;
                int idx = 0;
                foreach (ItemOrderNpcItem npcItem in itemOrderNpc.NpcItems.Where(i => iPlayer.TeamRank >= i.RangRestricted))
                {
                    if (index == idx)
                    {
                        if (itemOrderNpc.RequiredTeams.Count > 0 && !itemOrderNpc.RequiredTeams.Contains((int)iPlayer.TeamId)) return false;
                        //if (!iPlayer.CanInteractAntiFlood()) return false;
                        if(npcItem.RangRestricted > iPlayer.TeamRank)
                        {
                            iPlayer.SendNewNotification($"Sie benötigen mindestens Rang {iPlayer.TeamRank}!");
                            return false;
                        }
                        if(npcItem.Limited != 0 && ItemOrderModule.Instance.GetItemOrderCountByItem(iPlayer, npcItem) >= npcItem.Limited)
                        {
                            iPlayer.SendNewNotification($"Maximal {npcItem.Limited} {npcItem.RewardItem.Name} herstellbar!");
                            return true;
                        }

                        // Discount 4 Gangwar Gebiet

                        int totalhours = npcItem.Hours;
                        int totalPrice = npcItem.RequiredMoney;
                        if (iPlayer.Team != null && iPlayer.Team.IsGangsters())
                        {
                            int owned = GangwarTownModule.Instance.GetAll().Where(gt => gt.Value.OwnerTeam == iPlayer.Team).Count();

                            if (owned > 0)
                            {
                                totalPrice -= (totalPrice / 10); // 10% nachlass bei GW Gebiet
                                if (totalhours > 4)
                                {
                                    totalhours = totalhours - 1; // Stunden bei Besitz von Gebieten -1h
                                }
                            }
                        }

                        // Check Required Items
                        foreach (KeyValuePair<ItemModel, int> kvp in npcItem.RequiredItems)
                        {
                            if (iPlayer.Container.GetItemAmount(kvp.Key) < kvp.Value)
                            {
                                iPlayer.SendNewNotification($"Sie benoetigen {kvp.Value} {kvp.Key.Name}!");
                                return true;
                            }
                        }

                        // Check if user got schwarzgeld in inventory
                        if(iPlayer.Container.GetItemAmount(SchwarzgeldModule.SchwarzgeldId) <= totalPrice)
                        {
                            if (!iPlayer.TakeMoney(totalPrice))
                            {
                                iPlayer.SendNewNotification($"Sie benoetigen {totalPrice}$!");
                                return true;
                            }
                        }
                        else
                        {
                            iPlayer.Container.RemoveItem(SchwarzgeldModule.SchwarzgeldId, totalPrice);
                        }

                        //RemoveRequiredItems
                        foreach (KeyValuePair<ItemModel, int> kvp in npcItem.RequiredItems)
                        {
                            iPlayer.Container.RemoveItem(kvp.Key, kvp.Value);
                        }
                        
                        ItemOrderModule.Instance.AddDbOrder(npcItem.RewardItemId, npcItem.RewardItemAmount, (int)iPlayer.Id, totalhours, (int)itemOrderNpc.Id);

                        Task.Run(async () =>
                        {
                            iPlayer.SetData("Itemorderflood", true);
                            Chats.sendProgressBar(iPlayer, 2000);
                            iPlayer.Player.TriggerEvent("freezePlayer", true);
                            await Task.Delay(2000);
                            iPlayer.Player.TriggerEvent("freezePlayer", false);
                            if (iPlayer.HasData("Itemorderflood"))
                            {
                                iPlayer.ResetData("Itemorderflood");
                            }
                            iPlayer.SendNewNotification($"Herstellung von {npcItem.RewardItem.Name} begonnen, Dauer {totalhours}h!");
                            if (itemOrderNpc.Id == 12)
                            {
                                Logger.AddWeaponFactoryLog(iPlayer.Id, npcItem.RewardItemId);
                            }
                        });

                        return true;
                    }
                    idx++;
                }
                MenuManager.DismissCurrent(iPlayer);
                return false;
            }
        }
    }
}
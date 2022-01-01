using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using GVRP.Module.ClientUI.Windows;
using GVRP.Module.Logging;
using GVRP.Module.Players.Db;
using GVRP.Module.Players;
using GVRP.Module.Items;
using GVRP.Module.Voice;
using GVRP.Module.Chat;
using GVRP.Module.Business.NightClubs;
using System.Threading.Tasks;
using GVRP.Module.Schwarzgeld;
using GVRP.Module.NSA;
using GVRP.Module.Teams;

namespace GVRP.Module.Shops.Windows
{
    public class ShopItemX
    {
        [JsonProperty(PropertyName = "id")]
        public uint Id { get; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; }
        [JsonProperty(PropertyName = "price")]
        public int Price { get; }

        public ShopItemX(uint id, string name, int price)
        {
            Id = id;
            Name = name;
            Price = price;
        }
    }

    public class ShopWindow : Window<Func<DbPlayer, string, int, List<ShopItemX>, bool>>
    {
        private class ShowEvent : Event
        {
            [JsonProperty(PropertyName = "title")] private string Title { get; }
            [JsonProperty(PropertyName = "id")] private int ShopId { get; }
            [JsonProperty(PropertyName = "items")] private List<ShopItemX> Items { get; }

            public ShowEvent(DbPlayer dbPlayer, string title, int id, List<ShopItemX> items) : base(dbPlayer)
            {
                Title = title;
                ShopId = id;
                Items = items;
            }
        }

        public ShopWindow() : base("Shop")
        {
        }

        public override Func<DbPlayer, string, int, List<ShopItemX>, bool> Show()
        {
            return (player, title, shopId, items) => OnShow(new ShowEvent(player, title, shopId, items));
        }

        [RemoteEvent]
        public void shopBuy(Client client, string shopBuyEventJson)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                var shopBuyEvent = JsonConvert.DeserializeObject<ShopBuyEvent>(shopBuyEventJson);

                var iPlayer = client.GetPlayer();
                if (iPlayer == null || !iPlayer.IsValid()) return;

                var accessedShop = ShopModule.Instance.Get((uint)shopBuyEvent.shopId);
                int price = 0;
                float weight = 0.0f;
                int requiredSlots = 0;

                NightClub nightClub = null;

                if (iPlayer.HasData("nightClubShopId"))
                {
                    nightClub = NightClubModule.Instance.Get(iPlayer.GetData("nightClubShopId"));
                    iPlayer.ResetData("nightClubShopId");
                    if (nightClub == null) return;
                }

                // Check Weight Items and Calculate Price
                foreach (ShopBuyEvent.BasketItem item in shopBuyEvent.basket)
                {
                    if (item == null) continue;
                    ItemModel itemData = ItemModelModule.Instance.Get((uint)item.itemId);

                    if (item.count < 0)
                    {
                        iPlayer.SendNewNotification("Du kannst keine negative Anzahl kaufen!");
                        return;
                    }

                    if (nightClub != null)
                    {
                        if (nightClub.Container.GetItemAmount(itemData.Id) < item.count)
                        {
                            iPlayer.SendNewNotification($"{itemData.Name} aktuell nicht vorraetig!");
                            return;
                        }

                        // Add to Price
                        price += nightClub.NightClubShopItems.First(x => x.ItemId == item.itemId).Price * item.count; ;
                    }
                    else
                    {
                        // Stored Items Checks..
                        ShopItem shopItem = accessedShop.ShopItems.Where(x => x.ItemId == item.itemId).FirstOrDefault();
                        if (shopItem == null) return; // BUG?!

                        if (shopItem.IsStoredItem && shopItem.Stored < item.count)
                        {
                            iPlayer.SendNewNotification($"{itemData.Name} aktuell nicht vorraetig!");
                            return;
                        }

                        price += accessedShop.ShopItems.First(x => x.ItemId == item.itemId).Price * item.count;
                    }
                    weight += itemData.Weight * item.count;

                    //Item ist ein Rucksack
                    if (item.itemId >= 3 && item.itemId <= 5)
                    {
                        foreach (var backpackConfiguration in Module.Items.Scripts.BackpackList.backpackList)
                        {
                            if (iPlayer.Container.GetItemAmount(backpackConfiguration.ItemModel) >= 1)
                            {
                                iPlayer.SendNewNotification("Du besitzt bereits einen Rucksack! Einkauf Fehlgeschlagen");
                                return;
                            }
                        }

                    }

                    var similarStack = ContainerManager.GetSlotOfSimilairSingleItemsToStack(iPlayer.Container, itemData);
                    var stackRequiredSlots = 99;

                    if (similarStack == -1)
                    { //Es gibt bisher keinen Stack mit diesem Itemtyp
                        stackRequiredSlots = item.count / itemData.MaximumStacksize < 1 ? 1 : (int)Math.Ceiling((decimal)item.count / (decimal)itemData.MaximumStacksize);
                    }
                    else
                    { //Es wurde ein Stack mit dem Itemtyp gefunden
                        var spaceLeftOnSlot = itemData.MaximumStacksize - ContainerManager.GetAmountOfItemsOnSlot(iPlayer.Container, similarStack);
                        stackRequiredSlots = item.count <= spaceLeftOnSlot ? 0 : (int)Math.Ceiling((decimal)(item.count - spaceLeftOnSlot) / (decimal)itemData.MaximumStacksize);
                    }

                    requiredSlots += itemData.MaximumStacksize > 1 ? stackRequiredSlots : 1;
                }

                if (iPlayer.Container.GetInventoryUsedSpace() + weight > iPlayer.Container.MaxWeight)
                {
                    iPlayer.SendNewNotification("So viel kannst du nicht tragen");
                    return;
                }

                if (price <= 0)
                {
                    iPlayer.SendNewNotification("You should not read this.");
                    return;
                }

                if (iPlayer.Container.GetUsedSlots() + requiredSlots > iPlayer.Container.MaxSlots)
                {
                    iPlayer.SendNewNotification("Dein Inventar hat zu wenige Slots!");
                    return;
                }

                if (accessedShop.SchwarzgeldUse)
                {
                    if (!iPlayer.TakeBlackMoney(price))
                    {
                        iPlayer.SendNewNotification(MSG.Money.NotEnoughSWMoney(price));
                        return;
                    }
                }
                else
                {
                    if (!iPlayer.TakeMoney(price))
                    {
                        iPlayer.SendNewNotification(MSG.Money.NotEnoughMoney(price));
                        return;
                    }
                }

                // Nightclub Module Give Money To NightClub
                if (nightClub != null && nightClub.IsOwnedByBusines())
                {
                    nightClub.GetOwnedBusiness().GiveMoney(price);
                }

                // Add Items & Remove if Nightclub
                foreach (ShopBuyEvent.BasketItem item in shopBuyEvent.basket)
                {
                    if (item == null) continue;
                    ItemModel itemData = ItemModelModule.Instance.Get((uint)item.itemId);

                    // Remove From NightClub
                    if (nightClub != null) nightClub.Container.RemoveItem(itemData.Id, item.count);
                    else
                    {
                        ShopItem shopItem = accessedShop.ShopItems.Where(x => x.ItemId == item.itemId).FirstOrDefault();
                        if (shopItem == null) return; // BUG?!
                        if (shopItem.IsStoredItem)
                        {
                            shopItem.Stored -= item.count;
                            shopItem.SaveStoreds();
                        }
                        Logger.AddShopBuyLog(iPlayer.Id, item.itemId, item.count, shopItem.Price*item.count );
                    }

                    if (!iPlayer.Container.AddItem(itemData.Id, item.count))
                    {
                        iPlayer.SendNewNotification("Deine Taschen sind nicht groß genug!");
                    }

                    iPlayer.SendNewNotification($"{item.count} {itemData.Name} gekauft!");

                    if(iPlayer.HasMoneyTransferWantedStatus() && !iPlayer.IsMasked())
                    {
                        TeamModule.Instance.SendMessageToTeam($"Finanz-Detection: Die Gesuchte Person {iPlayer.GetName()} hat einen Einkauf getätigt! (Standort: {accessedShop.Name})", teams.TEAM_FIB, 10000, 3);
                        NSAPlayerExtension.AddTransferHistory($"{iPlayer.GetName()} Shop {accessedShop.Name}", accessedShop.Position);
                    }
                }
            }));
        }
    }
}

using GVRP.Module.GTAN;
using System;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using GTANetworkAPI;
using GVRP.Handler;
using GVRP.Module.Business;
using GVRP.Module.Chat;
using GVRP.Module.Clothes;
using GVRP.Module.Clothes.Character;
using GVRP.Module.Clothes.Props;
using GVRP.Module.Clothes.Shops;
using GVRP.Module.Configurations;
using GVRP.Module.Doors;
using GVRP.Module.Export;
using GVRP.Module.Houses;
using GVRP.Module.Jobs;
using GVRP.Module.Jobs.Bus;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.PlayerTask;
using GVRP.Module.Robbery;
using GVRP.Module.Tasks;
using GVRP.Module.Teams;
using GVRP.Module.Teams.Permission;
using GVRP.Module.Vehicles;
using GVRP.Module.Vehicles.Data;
using GVRP.Module.Vehicles.Garages;
using GVRP.Module.Vehicles.Shops;
using GVRP.Module.Players.Events;
using GVRP.Module.Items;
using GVRP.Module.Shops;
using GVRP.Module.Farming;
using GVRP.Module.Crime;
using GVRP.Module.Logging;
using GVRP.Module.Helper;
using GVRP.Module.Teams.Shelter;
using System.Linq;
using GVRP.Module.Gangwar;
using GVRP.Module.Warrants;
using GVRP.Module.Staatskasse;
using GVRP.Module.Vehicles.RegistrationOffice;
using GVRP.Module.Dealer;
using GVRP.Module.Injury;
using GVRP.Module.Guenther;
using GVRP.Module.Zone;
using GVRP.Module.NSA;

namespace GVRP
{
    public class MenuCallbackEventHandler : Script
    {
        public List<DbPlayer> Users = Players.Instance.GetValidPlayers();

        [RemoteEvent]
        public void m(Client player, int index)
        {
            try
            {
                var isClosing = index < 0;
                var iPlayer = player.GetPlayer();
                if (iPlayer == null || !iPlayer.IsValid()) return;

                // Menu is closed, reset menu id
                if (isClosing)
                {
                    /*if (iPlayer.Freezed == false)
                    {
                        player.FreezePosition = false;
                    }*/

                    if (-index == iPlayer.WatchMenu)
                    {
                        iPlayer.WatchMenu = 0;
                    }

                    return;
                }

                var menuid = iPlayer.WatchMenu;

                if (Enum.IsDefined(typeof(PlayerMenu), menuid) && Enum.TryParse(menuid.ToString(), out PlayerMenu menu))
                {
                    MenuManager.Instance.OnSelect(menu, index, iPlayer);
                    return;
                }

                if (menuid == Dialogs.menu_fmembers)
                {
                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_fmembers);
                }
                else if (menuid == Dialogs.menu_fmanager)
                {
                    DbPlayer targetPlayer = iPlayer.GetData("temp_indx");
                    if (!targetPlayer.IsValid()) return;
                    switch (index)
                    {
                        case 0:
                            if (targetPlayer.TeamId == iPlayer.TeamId &&
                                iPlayer.TeamRankPermission.Manage >= 1)
                            {
                                if (targetPlayer.TeamRankPermission.Manage == 2)
                                {
                                    iPlayer.SendNewNotification(
                                        
                                        "Sie können keinen Mainleader uninviten!", title: "", notificationType: PlayerNotification.NotificationType.ERROR);
                                    return;
                                }

                                targetPlayer.RemoveParamedicLicense();

                                targetPlayer.SetTeam(0);
                                targetPlayer.SendNewNotification(
                                    iPlayer.GetName() + " hat Sie aus der Fraktion entlassen!");
                                iPlayer.SendNewNotification($"Sie haben {targetPlayer.GetName()} entlassen!");
                                
                                PlayerSpawn.OnPlayerSpawn(targetPlayer.Player);
                                targetPlayer.SetTeamRankPermission(false, 0, false, "");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_fmanager);
                                return;
                            }

                            break;
                        case 1:
                            if (targetPlayer.TeamId == iPlayer.TeamId &&
                                iPlayer.TeamRankPermission.Manage >= 1)
                            {
                                if (targetPlayer.TeamRank + 1 >= 12)
                                {
                                    iPlayer.SendNewNotification(
                                         "maximaler Rang erreicht");
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_fmanager);
                                    return;
                                }

                                targetPlayer.TeamRank = targetPlayer.TeamRank + 1;
                                targetPlayer.SendNewNotification(
                                    iPlayer.GetName() + " hat Sie befoerdert!", title: "Fraktion", notificationType: PlayerNotification.NotificationType.SUCCESS);
                                iPlayer.SendNewNotification(

                                    $"Sie haben {targetPlayer.GetName()} befoerdert!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_fmanager);
                                return;
                            }

                            break;
                        case 2:
                            if (targetPlayer.TeamId == iPlayer.TeamId &&
                                iPlayer.TeamRankPermission.Manage >= 1)
                            {
                                if (targetPlayer.TeamRankPermission.Manage == 2)
                                {
                                    iPlayer.SendNewNotification(
                                        
                                        "Sie können keinen Mainleader degradieren!");
                                    return;
                                }

                                if (targetPlayer.TeamRank == 0)
                                {
                                    iPlayer.SendNewNotification(
                                         "minimaler Rang erreicht!");
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_fmanager);
                                    return;
                                }

                                targetPlayer.TeamRank = targetPlayer.TeamRank - 1;
                                targetPlayer.SendNewNotification(
                                    iPlayer.GetName() + " hat Sie degradiert!");
                                iPlayer.SendNewNotification(

                                    $"Sie haben {targetPlayer.GetName()} degradiert!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_fmanager);
                                return;
                            }

                            break;
                        case 3:
                            if (targetPlayer.TeamId == iPlayer.TeamId &&
                                iPlayer.TeamRankPermission.Manage >= 1)
                            {
                                if (targetPlayer.TeamRankPermission.Bank)
                                {
                                    iPlayer.SendNewNotification(
                                         "Spieler hat bereits Rechte zur Bank!");
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_fmanager);
                                    return;
                                }

                                targetPlayer.SetTeamRankPermission(true, targetPlayer.TeamRankPermission.Manage,
                                    targetPlayer.TeamRankPermission.Inventory, targetPlayer.TeamRankPermission.Title);
                                targetPlayer.SendNewNotification(
                                    iPlayer.GetName() + " hat Ihnen Rechte zur Fraktionsbank gegeben!");
                                iPlayer.SendNewNotification(

                                    $"Sie haben {targetPlayer.GetName()} Rechte zur Fraktionsbank gegeben!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_fmanager);
                                return;
                            }

                            break;
                        case 4:
                            if (targetPlayer.TeamId == iPlayer.TeamId &&
                                iPlayer.TeamRankPermission.Manage >= 1)
                            {
                                if (targetPlayer.TeamRankPermission.Inventory)
                                {
                                    iPlayer.SendNewNotification(
                                        
                                        "Spieler hat bereits Rechte zum Inventar!");
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_fmanager);
                                    return;
                                }

                                targetPlayer.SetTeamRankPermission(targetPlayer.TeamRankPermission.Bank,
                                    targetPlayer.TeamRankPermission.Manage, true, targetPlayer.TeamRankPermission.Title);
                                targetPlayer.SendNewNotification(
                                    iPlayer.GetName() + " hat Ihnen Rechte zum Fraktionsinventar gegeben!");
                                iPlayer.SendNewNotification(

                                    $"Sie haben {targetPlayer.GetName()} Rechte zum Fraktionsinventar gegeben!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_fmanager);
                                return;
                            }

                            break;
                        case 5:
                            if (targetPlayer.TeamId == iPlayer.TeamId &&
                                iPlayer.TeamRankPermission.Manage >= 1)
                            {
                                if (targetPlayer.TeamRankPermission.Manage == 1)
                                {
                                    iPlayer.SendNewNotification(
                                        
                                        "Spieler hat bereits Rechte zur Fraktionsverwaltung!");
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_fmanager);
                                    return;
                                }

                                targetPlayer.SetTeamRankPermission(targetPlayer.TeamRankPermission.Bank, 1,
                                    targetPlayer.TeamRankPermission.Inventory, targetPlayer.TeamRankPermission.Title);
                                targetPlayer.SendNewNotification(
                                    iPlayer.GetName() + " hat Ihnen Rechte zur Fraktionsverwaltung gegeben!");
                                iPlayer.SendNewNotification(

                                    $"Sie haben {targetPlayer.GetName()} Rechte zur Fraktionsverwaltung gegeben!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_fmanager);
                                return;
                            }

                            break;
                        case 6:
                            if (targetPlayer.TeamId == iPlayer.TeamId &&
                                iPlayer.TeamRankPermission.Manage >= 1)
                            {
                                if (targetPlayer.TeamRankPermission.Manage == 2)
                                {
                                    iPlayer.SendNewNotification(
                                        
                                        "Sie können keinem Mainleader die Rechte entziehen!");
                                    return;
                                }

                                targetPlayer.SetTeamRankPermission(false, 0, false, "");
                                targetPlayer.SendNewNotification(
                                    iPlayer.GetName() + " hat Ihnen alle Fraktionsrechte entzogen!");
                                iPlayer.SendNewNotification(

                                    $"Sie haben {targetPlayer.GetName()} alle Fraktionsrechte entzogen!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_fmanager);
                                return;
                            }

                            break;
                        default:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_fmanager);
                            break;
                    }
                }
                else if (menuid == Dialogs.menu_player)
                {
                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_player);
                }
                else if (menuid == Dialogs.menu_vehinventory)
                {
                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_vehinventory);
                }
                else if (menuid == Dialogs.menu_shop_clothes)
                {
                    int idx = 0;
                    uint shopId = 0;

                    if (iPlayer.HasData("clothShopId"))
                    {
                        shopId = iPlayer.GetData("clothShopId");
                    }

                    ClothesShop clothesShop = ClothesShopModule.Instance.GetShopById(shopId);
                    if (clothesShop == null) return;

                    if (index == 0)
                    {
                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_clothes);
                        return;
                    }
                    // Buy
                    else if (index == 1)
                    {
                        int price = ClothesShopModule.Instance.GetActualClothesPrice(iPlayer);


                        iPlayer.SendNewNotification(
                             "Sie haben diese Kleidung fuer $" + price +
                            " erworben!");
                        if (price > 0)
                        {
                            int couponPercent = 0;
                            uint whatCoupon = 0;

                            if (clothesShop.CouponUsable)
                            {
                                foreach (KeyValuePair<int, Item> kvp in iPlayer.Container.Slots)
                                {
                                    if (kvp.Value.Model == null) continue;
                                    if (kvp.Value.Model.Script == null) continue;
                                    if (kvp.Value.Model.Script.Contains("discount_cloth_"))
                                    {
                                        try
                                        {
                                            couponPercent = Int32.Parse(kvp.Value.Model.Script.Replace("discount_cloth_", ""));
                                            double temp = couponPercent / 100.0d;
                                            price -= (int)(price * temp);
                                            whatCoupon = kvp.Value.Id;
                                        }
                                        catch (Exception)
                                        {
                                            iPlayer.SendNewNotification("Ein Fehler ist aufgetreten...");
                                            return;
                                        }
                                        break;
                                    }
                                }
                            }

                            if (!iPlayer.TakeMoney(price))
                            {
                                iPlayer.SendNewNotification(MSG.Money.NotEnoughMoney(price));
                                return;
                            }
                            if (whatCoupon != 0 && clothesShop.CouponUsable)
                            {
                                iPlayer.SendNewNotification("- " + couponPercent + " % Rabatt", title: "", notificationType: PlayerNotification.NotificationType.SUCCESS);
                                iPlayer.Container.RemoveItem(whatCoupon);
                            }

                            shopId = 0;
                            if (iPlayer.HasData("clothShopId"))
                            {
                                shopId = iPlayer.GetData("clothShopId");
                            }
                            if (shopId != 0)
                            {
                                Logger.SaveClothesShopBuyAction(shopId, price);
                            }
                        }
                        
                        ClothesShopModule.Instance.Buy(iPlayer);

                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_clothes);
                        return;
                    }

                    var character = iPlayer.Character;

                    var clothesSlots = clothesShop.GetClothesSlotsForPlayer(iPlayer);

                    var propsSlots = clothesShop.GetPropsSlotsForPlayer(iPlayer);

                    foreach (KeyValuePair<int, string> kvp in clothesSlots)
                    {
                        // found
                        if (idx == index - 2)
                        {
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_clothes, true);

                            DialogMigrator.CreateMenu(player, Dialogs.menu_shop_clothes_selection, kvp.Value, "");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_clothes_selection, "Zurueck", "");

                            foreach (Cloth cloth in clothesShop.GetClothesBySlotForPlayer(kvp.Key, iPlayer))
                            {
                                DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_clothes_selection, cloth.Name + " $" + cloth.Price, "");
                            }

                            iPlayer.ResetData("propsActualSlot");
                            iPlayer.SetData("clothesActualSlot", kvp.Key);
                            DialogMigrator.OpenUserMenu(iPlayer, Dialogs.menu_shop_clothes_selection);
                            return;
                        }

                        idx++;
                    }

                    foreach (KeyValuePair<int, string> kvp in propsSlots)
                    {
                        // found
                        if (idx == index - 2)
                        {
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_clothes, true);

                            DialogMigrator.CreateMenu(player, Dialogs.menu_shop_clothes_selection, kvp.Value, "");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_clothes_selection, "Zurueck", "");

                            foreach (Prop prop in clothesShop.GetPropsBySlotForPlayer(kvp.Key, iPlayer))
                            {
                                DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_clothes_selection, prop.Name + " $" + prop.Price, "");
                            }

                            iPlayer.ResetData("clothesActualSlot");
                            iPlayer.SetData("propsActualSlot", kvp.Key);
                            DialogMigrator.OpenUserMenu(iPlayer, Dialogs.menu_shop_clothes_selection);
                            return;
                        }

                        idx++;
                    }
                }
                else if (menuid == Dialogs.menu_shop_clothes_selection)
                {
                    uint shopId = 0;
                    if (iPlayer.HasData("clothShopId"))
                    {
                        shopId = iPlayer.GetData("clothShopId");
                    }
                    else
                    {
                        return;
                    }

                    var shop = ClothesShopModule.Instance.GetShopById(shopId);

                    if (shop == null) return;

                    // Zurueck
                    if (index == 0)
                    {
                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_clothes_selection, true);

                        DialogMigrator.CreateMenu(player, Dialogs.menu_shop_clothes, "Kleiderladen", "");

                        DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_clothes, MSG.General.Close(), "");
                        DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_clothes, "Kaufen: $" + ClothesShopModule.Instance.GetActualClothesPrice(iPlayer), "");

                        var clothesSlots = shop.GetClothesSlotsForPlayer(iPlayer);

                        var propsSlots = shop.GetPropsSlotsForPlayer(iPlayer);

                        foreach (KeyValuePair<int, string> kvp in clothesSlots)
                        {
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_clothes, kvp.Value, kvp.Value);
                        }

                        foreach (KeyValuePair<int, string> kvp in propsSlots)
                        {
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_clothes, kvp.Value, kvp.Value);
                        }

                        DialogMigrator.OpenUserMenu(iPlayer, Dialogs.menu_shop_clothes);
                        return;
                    }

                    if (iPlayer.HasData("clothesActualSlot") || iPlayer.HasData("propsActualSlot"))
                    {
                        var character = iPlayer.Character;
                        int slot = -1;
                        if (iPlayer.HasData("clothesActualSlot"))
                        {
                            slot = (int)iPlayer.GetData("clothesActualSlot");
                            List<Cloth> clothesList = shop.GetClothesBySlotForPlayer(slot, iPlayer);

                            if (index - 1 < 0) return;
                            if (index - 1 >= clothesList.Count) return;

                            var selectedCloth = clothesList[index - 1];

                            if (selectedCloth != null)
                            {
                                //player.TriggerEvent("triggerPlayerClothes", selectedCloth.Slot,
                                //    selectedCloth.Variation, selectedCloth.Texture);

                                iPlayer.SetClothes(clothesList[index - 1].Slot, clothesList[index - 1].Variation, clothesList[index - 1].Texture);

                                iPlayer.SetData("clothesActualItem-" + slot,
                                    selectedCloth.Id);
                                return;
                            }

                            return;
                        }
                        else
                        {
                            slot = (int)iPlayer.GetData("propsActualSlot");
                            List<Prop> propsList = shop.GetPropsBySlotForPlayer(slot, iPlayer);

                            int newIndex = index - 1;
                            if (propsList.Count > newIndex && newIndex > 0)
                            {
                                var selectedProp = propsList[index - 1];

                                if (selectedProp != null)
                                {
                                    ClothModule.Instance.SetPlayerAccessories(iPlayer, propsList[index - 1].Slot, propsList[index - 1].Variation, propsList[index - 1].Texture);

                                    iPlayer.SetData("propsActualItem-" + slot,
                                        selectedProp.Id);
                                    return;
                                }
                            }
                        }

                        return;
                    }
                    else
                    {
                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_clothes_selection);
                        return;
                    }
                }
                else if (menuid == Dialogs.menu_wardrobe)
                {
                    int idx = 3;

                    if (index == 0)
                    {
                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_wardrobe);
                        return;
                    }
                    if (index == 1)
                    {
                        MenuManager.Instance.Build(PlayerMenu.OutfitsMenu, iPlayer).Show(iPlayer);
                        return;
                    }

                    if (index == 2) // Altkleider
                    {
                        if (iPlayer.HasData("teamWardrobe")) return;
                        MenuManager.Instance.Build(PlayerMenu.Altkleider, iPlayer).Show(iPlayer);
                        return;
                    }

                    var character = iPlayer.Character;

                    foreach (KeyValuePair<int, string> kvp in ClothesShopModule.Instance.GetSlots())
                    {
                        // found
                        if (idx == index)
                        {
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_wardrobe, true);

                            DialogMigrator.CreateMenu(player, Dialogs.menu_wardrobe_selection, kvp.Value, "");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_wardrobe_selection, "Zurück", "");
                            if (iPlayer.HasData("teamWardrobe"))
                            {
                                foreach (Cloth cloth in ClothModule.Instance.GetTeamWarerobe(iPlayer, kvp.Key))
                                {
                                    DialogMigrator.AddMenuItem(player, Dialogs.menu_wardrobe_selection, cloth.Name,
                                        cloth.Name);
                                }
                            }
                            else
                            {
                                foreach (Cloth cloth in ClothModule.Instance.GetWardrobeBySlot(iPlayer, character,
                                    kvp.Key))
                                {
                                    DialogMigrator.AddMenuItem(player, Dialogs.menu_wardrobe_selection, cloth.Name,
                                        cloth.Name);
                                }
                            }

                            iPlayer.SetData("clothesWardrobeActualSlot", kvp.Key);
                            iPlayer.ResetData("propsWardrobeActualSlot");
                            DialogMigrator.OpenUserMenu(iPlayer, Dialogs.menu_wardrobe_selection);
                            return;
                        }

                        idx++;
                    }

                    foreach (KeyValuePair<int, string> kvp in ClothesShopModule.Instance.GetPropsSlots())
                    {
                        // found
                        if (idx == index - 1)
                        {
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_wardrobe, true);

                            DialogMigrator.CreateMenu(player, Dialogs.menu_wardrobe_selection, kvp.Value, "");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_wardrobe_selection, "Zurück", "");
                            if (iPlayer.HasData("teamWardrobe"))
                            {
                                foreach (Prop prop in PropModule.Instance.GetTeamWarerobe(iPlayer, kvp.Key))
                                {
                                    DialogMigrator.AddMenuItem(player, Dialogs.menu_wardrobe_selection, prop.Name,
                                        prop.Name);
                                }
                            }
                            else
                            {
                                foreach (Prop prop in PropModule.Instance.GetWardrobeBySlot(iPlayer, kvp.Key))
                                {
                                    DialogMigrator.AddMenuItem(player, Dialogs.menu_wardrobe_selection, prop.Name,
                                        prop.Name);
                                }
                            }

                            iPlayer.SetData("propsWardrobeActualSlot", kvp.Key);
                            iPlayer.ResetData("clothesWardrobeActualSlot");
                            DialogMigrator.OpenUserMenu(iPlayer, Dialogs.menu_wardrobe_selection);
                            return;
                        }

                        idx++;
                    }
                }
                else if (menuid == Dialogs.menu_wardrobe_selection)
                {
                    if (index == 0)
                    {
                        Character playerCharacter = iPlayer.Character;
                        var playerWardrobe = playerCharacter.Wardrobe;
                        DialogMigrator.CreateMenu(player, Dialogs.menu_wardrobe, "Kleiderschrank", "");
                        DialogMigrator.AddMenuItem(player, Dialogs.menu_wardrobe, MSG.General.Close(), "");
                        DialogMigrator.AddMenuItem(player, Dialogs.menu_wardrobe, "Outfits", "");
                        DialogMigrator.AddMenuItem(player, Dialogs.menu_wardrobe, "Altkleider packen", "");

                        foreach (KeyValuePair<int, string> kvp in ClothesShopModule.Instance.GetSlots())
                        {
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_wardrobe, kvp.Value, kvp.Value);
                        }

                        foreach (KeyValuePair<int, string> kvp in ClothesShopModule.Instance.GetPropsSlots())
                        {
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_wardrobe, kvp.Value, kvp.Value);
                        }

                        DialogMigrator.OpenUserMenu(iPlayer, Dialogs.menu_wardrobe);

                        return;
                    }

                    if (iPlayer.HasData("clothesWardrobeActualSlot"))
                    {
                        Character playerCharacter = iPlayer.Character;

                        int slot = iPlayer.GetData("clothesWardrobeActualSlot");
                        List<Cloth> wardrobeClothesForSlot;
                        if (iPlayer.HasData("teamWardrobe"))
                        {
                            wardrobeClothesForSlot =
                                ClothModule.Instance.GetTeamWarerobe(iPlayer, slot);
                        }
                        else
                        {
                            wardrobeClothesForSlot =
                                ClothModule.Instance.GetWardrobeBySlot(iPlayer, playerCharacter, slot);
                        }

                        if (wardrobeClothesForSlot.Count > index - 1)
                        {
                            var cloth = wardrobeClothesForSlot[index - 1];

                            playerCharacter.Clothes[cloth.Slot] = cloth.Id;

                            ClothModule.Instance.RefreshPlayerClothes(iPlayer);
                            ClothModule.SaveCharacter(iPlayer);
                        }
                    }

                    if (iPlayer.HasData("propsWardrobeActualSlot"))
                    {
                        Character playerCharacter = iPlayer.Character;

                        int slot = iPlayer.GetData("propsWardrobeActualSlot");
                        List<Prop> wardrobeClothesForSlot;
                        if (iPlayer.HasData("teamWardrobe"))
                        {
                            wardrobeClothesForSlot =
                                PropModule.Instance.GetTeamWarerobe(iPlayer, slot);
                        }
                        else
                        {
                            wardrobeClothesForSlot =
                                PropModule.Instance.GetWardrobeBySlot(iPlayer, slot);
                        }

                        if (wardrobeClothesForSlot.Count > index - 1)
                        {
                            var cloth = wardrobeClothesForSlot[index - 1];
                            playerCharacter.EquipedProps[cloth.Slot] = cloth.Id;

                            ClothModule.Instance.RefreshPlayerClothes(iPlayer);
                            ClothModule.SaveCharacter(iPlayer);
                        }
                    }
                }
                else if (menuid == Dialogs.menu_shop_mechanic)
                {
                    // Close
                    if (index == 0)
                    {
                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_mechanic);
                        return;
                    }

                    if (index == 1)
                    {
                        if (!iPlayer.Container.CanInventoryItemAdded(26, 1))
                        {
                            iPlayer.SendNewNotification("Nicht genug Platz im Inventar!");
                            return;
                        }

                        if (!iPlayer.TakeMoney(300))
                        {
                            iPlayer.SendNewNotification("Du hast nicht genug Geld!" + MSG.Money.NotEnoughMoney(300));
                            return;
                        }

                        iPlayer.Container.AddItem(26, 1);
                        return;
                    }

                    if (index == 2)
                    {
                        if (!iPlayer.Container.CanInventoryItemAdded(297, 1))
                        {
                            iPlayer.SendNewNotification("Nicht genug Platz im Inventar!");
                            return;
                        }

                        if (!iPlayer.TakeMoney(1000))
                        {
                            iPlayer.SendNewNotification("Du hast nicht genug Geld!" + MSG.Money.NotEnoughMoney(1000));
                            return;
                        }

                        iPlayer.Container.AddItem(297, 1);
                        return;
                    }
                    if (index == 3)
                    {
                        if (!iPlayer.Container.CanInventoryItemAdded(70, 1))
                        {
                            iPlayer.SendNewNotification("Nicht genug Platz im Inventar!");
                            return;
                        }

                        if (!iPlayer.TakeMoney(4000))
                        {
                            iPlayer.SendNewNotification("Du hast nicht genug Geld!" + MSG.Money.NotEnoughMoney(4000));
                            return;
                        }

                        iPlayer.Container.AddItem(70, 1);
                        return;
                    }
                    if (index == 4)
                    {
                        if (!iPlayer.Container.CanInventoryItemAdded(245, 1))
                        {
                            iPlayer.SendNewNotification("Nicht genug Platz im Inventar!");
                            return;
                        }

                        if (!iPlayer.TakeMoney(2000))
                        {
                            iPlayer.SendNewNotification("Du hast nicht genug Geld!" + MSG.Money.NotEnoughMoney(2000));
                            return;
                        }

                        iPlayer.Container.AddItem(245, 1);
                        return;
                    }

                    //TODO

                    /*
                    if (idx == index)
                    {
                        if (iPlayer.Container.CanInventoryItemAdded()
                        {
                            iPlayer.SendNewNotification(
                                 "Inventar ist voll!");
                            return;
                        }

                        if (!iPlayer.TakeMoney(item.BuyPrice))
                        {
                            iPlayer.SendNewNotification(
                                
                                MSG.Money.NotEnoughMoney(item.BuyPrice));
                            return;
                        }

                        iPlayer.Container.AddItem(item, 1);
                        iPlayer.SendNewNotification(
                                "Sie haben " + item.Name + " fuer $" +
                            item.BuyPrice + " gekauft!");
                    }*/
                }
                else if (menuid == Dialogs.menu_adminObject)
                {
                    if (iPlayer.adminObject == null)
                    {
                        DialogMigrator.CloseUserDialog(player, Dialogs.menu_adminObject);
                        return;
                    }

                    switch (index)
                    {
                        case 0:
                            iPlayer.adminObject.MovePosition(
                                new Vector3(iPlayer.adminObject.Position.X + iPlayer.adminObjectSpeed,
                                    iPlayer.adminObject.Position.Y, iPlayer.adminObject.Position.Z), 1);
                            break;
                        case 1:
                            iPlayer.adminObject.MovePosition(
                                new Vector3(iPlayer.adminObject.Position.X - iPlayer.adminObjectSpeed,
                                    iPlayer.adminObject.Position.Y, iPlayer.adminObject.Position.Z), 1);
                            break;
                        case 2:
                            iPlayer.adminObject.MovePosition(
                                new Vector3(iPlayer.adminObject.Position.X,
                                    iPlayer.adminObject.Position.Y + iPlayer.adminObjectSpeed,
                                    iPlayer.adminObject.Position.Z), 1);
                            break;
                        case 3:
                            iPlayer.adminObject.MovePosition(
                                new Vector3(iPlayer.adminObject.Position.X,
                                    iPlayer.adminObject.Position.Y - iPlayer.adminObjectSpeed,
                                    iPlayer.adminObject.Position.Z), 1);
                            break;
                        case 4:
                            iPlayer.adminObject.MovePosition(
                                new Vector3(iPlayer.adminObject.Position.X, iPlayer.adminObject.Position.Y,
                                    iPlayer.adminObject.Position.Z + iPlayer.adminObjectSpeed), 1);
                            break;
                        case 5:
                            iPlayer.adminObject.MovePosition(
                                new Vector3(iPlayer.adminObject.Position.X, iPlayer.adminObject.Position.Y,
                                    iPlayer.adminObject.Position.Z - iPlayer.adminObjectSpeed), 1);
                            break;
                        case 6:
                            iPlayer.adminObject.MovePosition(
                                new Vector3(iPlayer.adminObject.Rotation.X, iPlayer.adminObject.Rotation.Y,
                                    iPlayer.adminObject.Rotation.Z + iPlayer.adminObjectSpeed * 8), 1);
                            break;
                        case 7:
                            iPlayer.adminObject.MovePosition(
                                new Vector3(iPlayer.adminObject.Rotation.X, iPlayer.adminObject.Rotation.Y,
                                    iPlayer.adminObject.Rotation.Z - iPlayer.adminObjectSpeed * 8), 1);
                            break;
                        case 8:
                            iPlayer.adminObject.MovePosition(
                                new Vector3(iPlayer.adminObject.Rotation.X + iPlayer.adminObjectSpeed * 8,
                                    iPlayer.adminObject.Rotation.Y,
                                    iPlayer.adminObject.Rotation.Z), 1);
                            break;
                        case 9:
                            iPlayer.adminObject.MovePosition(
                                new Vector3(iPlayer.adminObject.Rotation.X - iPlayer.adminObjectSpeed * 8,
                                    iPlayer.adminObject.Rotation.Y,
                                    iPlayer.adminObject.Rotation.Z), 1);
                            break;
                        case 10:
                            iPlayer.adminObject.MovePosition(
                                new Vector3(iPlayer.adminObject.Rotation.X,
                                    iPlayer.adminObject.Rotation.Y + iPlayer.adminObjectSpeed * 8,
                                    iPlayer.adminObject.Rotation.Z), 1);
                            break;
                        case 11:
                            iPlayer.adminObject.MovePosition(
                                new Vector3(iPlayer.adminObject.Rotation.X,
                                    iPlayer.adminObject.Rotation.Y - iPlayer.adminObjectSpeed * 8,
                                    iPlayer.adminObject.Rotation.Z), 1);
                            break;
                        case 12:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_adminObject);
                            break;
                    }
                }
                else if (menuid == Dialogs.menu_house_main)
                {
                    if (!iPlayer.HasData("houseId")) return;
                    uint houseId = iPlayer.GetData("houseId");
                    House iHouse = HouseModule.Instance[houseId];
                    if (iHouse == null) return;
                    switch (index)
                    {
                        case 0:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_main);
                            break;
                        case 1:
                            HouseModule.Instance.PlayerEnterHouse(iPlayer, iHouse);
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_main);
                            break;
                        case 2:
                            if (iHouse.Locked == true)
                            {
                                iPlayer.SendNewNotification("Bitte schließe dein Haus zunaechst auf 'L'", title: "Haus", notificationType: PlayerNotification.NotificationType.HOUSE);
                                return;
                            }

                            // Keller
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_main, true);

                            DialogMigrator.CreateMenu(player, Dialogs.menu_house_keller, "Hauskeller", "");

                            DialogMigrator.AddMenuItem(player, Dialogs.menu_house_keller, MSG.General.Close(),
                                "");

                            DialogMigrator.AddMenuItem(player, Dialogs.menu_house_keller, "Keller betreten",
                                "Betretet den Keller");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_house_keller, "Geldwaesche betreten",
                                "Betretet den Keller");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_house_keller, "Keller ausbauen",
                                "Baut oder upgraded einen Keller");

                            if (iHouse.Type == 3)
                            {
                                DialogMigrator.AddMenuItem(player, Dialogs.menu_house_keller, "Geldwaesche ausbauen",
                                    "Baut oder upgraded einen Geldwaesche Keller");
                            }

                            DialogMigrator.OpenUserMenu(iPlayer, Dialogs.menu_house_keller);
                            break;
                        case 3:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_main, true);

                            if (iPlayer.ownHouse[0] != iHouse.Id)
                            {
                                iPlayer.SendNewNotification(

                                    "Sie müssen der Besitzer des Hauses sein!");
                                return;
                            }
                            
                            if(iHouse.ShowPhoneNumber.Length > 0)
                            {
                                iPlayer.SendNewNotification($"Die Telefonnummer wird nun nicht mehr angezeigt!");
                                iHouse.ShowPhoneNumber = "";
                                iHouse.SaveShowPhoneNumber();
                            }
                            else
                            {
                                iPlayer.SendNewNotification($"Die Telefonnummer wird nun angezeigt!");
                                iHouse.ShowPhoneNumber = "" + iPlayer.handy[0];
                                iHouse.SaveShowPhoneNumber();
                            }

                            break;
                        case 4: // Garage
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_main, true);

                            // Wenn Garage
                            if (iHouse.GarageId != 0)
                            {
                                Garage garage = GarageModule.Instance.GetHouseGarage(iHouse.Id);
                                if (garage == null) return;
                                if (garage.IsTeamGarage()) return;
                                DialogMigrator.CreateMenu(player, Dialogs.menu_garage_overlay, "Fahrzeug-Garage", "");
                                DialogMigrator.AddMenuItem(player, Dialogs.menu_garage_overlay, MSG.General.Close(), "");
                                DialogMigrator.AddMenuItem(player, Dialogs.menu_garage_overlay, "Fahrzeug entnehmen", "");
                                DialogMigrator.AddMenuItem(player, Dialogs.menu_garage_overlay, "Fahrzeug einlagern", "");
                                

                                DialogMigrator.OpenUserMenu(iPlayer, Dialogs.menu_garage_overlay);
                                iPlayer.SetData("GarageId", garage.Id);
                                return;
                            }
                            else // Garage Ausbau
                            {
                                int cost = iHouse.Price / 4;

                                if (!iHouse.CanGarageBuild())
                                {
                                    iPlayer.SendNewNotification(
                                        $"Fuer den Ausbau benötigen Sie mindestens 50 {ItemModelModule.Instance.Get(312).Name} und 100 {ItemModelModule.Instance.Get(310).Name}!");
                                    return;
                                }
                                else
                                {
                                    if (iPlayer.ownHouse[0] != iHouse.Id)
                                    {
                                        iPlayer.SendNewNotification(
                                            
                                            "Um den Keller auszubauen muessen Sie der Besitzer des Hauses sein!");
                                        return;
                                    }

                                    if (!iPlayer.TakeMoney(cost))
                                    {
                                        iPlayer.SendNewNotification(
                                             MSG.Money.NotEnoughMoney(cost));
                                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_main);
                                        return;
                                    }

                                    iHouse.BuildGarage();

                                    iPlayer.SendNewNotification(
                                         "Garage fuer $" + cost +
                                        " ausgebaut!");
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_main);
                                    iHouse.SaveGarage();
                                    return;
                                }
                            }
                            
                        default:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_main);
                            break;
                    }
                }
                else if (menuid == Dialogs.menu_house_keller)
                {
                    if (!iPlayer.HasData("houseId")) return;
                    uint houseId = iPlayer.GetData("houseId");
                    House iHouse = HouseModule.Instance[houseId];
                    if (iHouse == null) return;
                    switch (index)
                    {
                        case 0:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_keller);
                            break;
                        case 1:
                            if (iHouse.Keller > 0)
                            {
                                //iPlayer.Player.FreezePosition = true;
                                
                                player.SetPosition(new Vector3(1138.25f, -3198.88f, -39.6657f));
                                player.SetRotation(357.87f);
                                
                                if (iHouse.Keller == 2)
                                {
                                    iPlayer.DimensionType[0] = DimensionType.Labor;
                                }
                                else
                                {
                                    iPlayer.DimensionType[0] = DimensionType.Basement;
                                }

                                iPlayer.SetData("inHouse", iHouse.Id);
                                player.Dimension = iHouse.Id;

                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_keller);
                                break;
                            }
                            else
                            {
                                iPlayer.SendNewNotification(
                                    
                                    "Sie haben keinen Keller!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_keller);
                                return;
                            }
                        case 2:
                            if (iHouse.MoneyKeller > 0)
                            {
                                if (iHouse.Type != 3)
                                {
                                    iPlayer.SendNewNotification("Sie haben keinen Geldwäsche Keller!");
                                    return;
                                }

                                //iPlayer.Player.FreezePosition = true;

                                player.SetPosition(new Vector3(1138.25f, -3198.88f, -39.6657f));
                                player.SetRotation(357.87f);
                                iPlayer.DimensionType[0] = DimensionType.MoneyKeller;
                                iPlayer.Player.TriggerEvent("loadblackmoneyInterior");
                                iPlayer.SetData("inHouse", iHouse.Id);

                                player.Dimension = iHouse.Id;

                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_keller);
                                break;
                            }
                            else
                            {
                                iPlayer.SendNewNotification(

                                    "Sie haben keinen Geldwäsche Keller!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_keller);
                                return;
                            }
                        case 3:
                            if (iHouse.Keller == 1) // Ausbau auf Labor
                            {
                                int cost = 100000;

                                if (iPlayer.CheckTaskExists(PlayerTaskTypeId.KellerAusbau) ||
                                    iPlayer.CheckTaskExists(PlayerTaskTypeId.LaborAusbau) ||
                                    iPlayer.CheckTaskExists(PlayerTaskTypeId.MoneyKellerAusbau))
                                {
                                    iPlayer.SendNewNotification("Ein Ausbau ist bereits in Arbeit!");
                                    return;
                                }

                                if (!iHouse.CanKellerUpgraded())
                                {
                                    iPlayer.SendNewNotification(
                                        $"Fuer den Ausbau benötigen Sie mindestens 40 {ItemModelModule.Instance.Get(312).Name} und 100 {ItemModelModule.Instance.Get(310).Name}!");
                                    return;
                                }
                                else
                                {
                                    if (iPlayer.ownHouse[0] != iHouse.Id)
                                    {
                                        iPlayer.SendNewNotification(
                                            
                                            "Um den Keller auszubauen muessen Sie der Besitzer des Hauses sein!");
                                        return;
                                    }

                                    if (!iPlayer.TakeMoney(cost))
                                    {
                                        iPlayer.SendNewNotification(
                                             MSG.Money.NotEnoughMoney(cost));
                                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_keller);
                                        return;
                                    }

                                    iHouse.UpgradeKeller(iPlayer);

                                    iPlayer.SendNewNotification(
                                         "Ausbauvorgang fuer $" + cost +
                                        " gestartet!");
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_keller);
                                    return;
                                }
                            }
                            else if (iHouse.Keller == 0) // Normaler ausbau
                            {
                                int cost = 50000;

                                if (iPlayer.CheckTaskExists(PlayerTaskTypeId.KellerAusbau) ||
                                    iPlayer.CheckTaskExists(PlayerTaskTypeId.LaborAusbau) ||
                                    iPlayer.CheckTaskExists(PlayerTaskTypeId.MoneyKellerAusbau))
                                {
                                    iPlayer.SendNewNotification("Ein Ausbau ist bereits in Arbeit!");
                                    return;
                                }

                                if (!iHouse.CanKellerBuild())
                                {
                                    iPlayer.SendNewNotification(
                                        $"Fuer den Ausbau benötigen Sie mindestens 25 {ItemModelModule.Instance.Get(312).Name} und 60 {ItemModelModule.Instance.Get(310).Name}!");
                                    return;
                                }
                                else
                                {
                                    if (iPlayer.ownHouse[0] != iHouse.Id)
                                    {
                                        iPlayer.SendNewNotification(
                                            
                                            "Um den Keller auszubauen muessen Sie der Besitzer des Hauses sein!");
                                        return;
                                    }

                                    if (!iPlayer.TakeMoney(cost))
                                    {
                                        iPlayer.SendNewNotification(
                                             MSG.Money.NotEnoughMoney(cost));
                                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_keller);
                                        return;
                                    }

                                    iHouse.BuildKeller(iPlayer);

                                    iPlayer.SendNewNotification(
                                         "Ausbauvorgang fuer $" + cost +
                                        " gestartet!");
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_keller);
                                    return;
                                }
                            }
                            break;
                        case 4:
                            if (iHouse.MoneyKeller != 1) // Ausbau auf Geldwäsche
                            {
                                if(iHouse.Type != 3)
                                {
                                    iPlayer.SendNewNotification("Bei diesem Typ ist ein Ausbau nicht möglich!");
                                    return;
                                }

                                int cost = 250000;

                                if (iPlayer.CheckTaskExists(PlayerTaskTypeId.KellerAusbau) ||
                                    iPlayer.CheckTaskExists(PlayerTaskTypeId.LaborAusbau) ||
                                    iPlayer.CheckTaskExists(PlayerTaskTypeId.MoneyKellerAusbau))
                                {
                                    iPlayer.SendNewNotification("Ein Ausbau ist bereits in Arbeit!");
                                    return;
                                }

                                if (!iHouse.CanMoneyKellerBuild())
                                {
                                    iPlayer.SendNewNotification(
                                        $"Fuer den Ausbau benötigen Sie mindestens 10 {ItemModelModule.Instance.Get(312).Name} und 30 {ItemModelModule.Instance.Get(310).Name}!");
                                    return;
                                }
                                else
                                {
                                    if (iPlayer.ownHouse[0] != iHouse.Id)
                                    {
                                        iPlayer.SendNewNotification(

                                            "Um den Geldwäsche Keller auszubauen muessen Sie der Besitzer des Hauses sein!");
                                        return;
                                    }

                                    if (!iPlayer.TakeMoney(cost))
                                    {
                                        iPlayer.SendNewNotification(
                                             MSG.Money.NotEnoughMoney(cost));
                                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_keller);
                                        return;
                                    }

                                    iHouse.BuildMoneyKeller(iPlayer);

                                    iPlayer.SendNewNotification(
                                         "Geldwäsche Keller fuer $" + cost +
                                        " gestartet!");
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_keller);
                                    return;
                                }
                            }
                            break;
                        default:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_house_keller);
                            break;
                    }
                }
                else if (menuid == Dialogs.menu_shop_changecar)
                {
                    switch (index)
                    {
                        case 0:
                            if (!player.IsInVehicle) return;
                            SxVehicle playerVeh = player.Vehicle.GetVehicle();
                            if (playerVeh == null || !iPlayer.IsOwner(playerVeh) || !playerVeh.IsValid()) return;
                            VehicleHash model = playerVeh.entity.GetModel();
                            int price = (VehicleShopModule.Instance.GetVehiclePriceFromHash(playerVeh.Data) / 100) * 20;

                            string newmodel = "";

                            if (model == VehicleHash.Banshee)
                                newmodel = Convert.ToString(VehicleHash.Banshee2);
                            else if (model == VehicleHash.Chino)
                                newmodel = Convert.ToString(VehicleHash.Chino2);
                            else if (model == VehicleHash.Diablous)
                                newmodel = Convert.ToString(VehicleHash.Diablous2);
                            else if (model == VehicleHash.Comet2)
                                newmodel = Convert.ToString(VehicleHash.Comet3);
                            else if (model == VehicleHash.Elegy2)
                                newmodel = Convert.ToString(VehicleHash.Elegy);
                            else if (model == VehicleHash.Faction)
                                newmodel = Convert.ToString(VehicleHash.Faction2);
                            else if (model == VehicleHash.Faction2)
                                newmodel = Convert.ToString(VehicleHash.Faction3);
                            else if (model == VehicleHash.Minivan)
                                newmodel = Convert.ToString(VehicleHash.Minivan2);
                            else if (model == VehicleHash.Moonbeam)
                                newmodel = Convert.ToString(VehicleHash.Moonbeam2);
                            else if (model == VehicleHash.Nero)
                                newmodel = Convert.ToString(VehicleHash.Nero2);
                            else if (model == VehicleHash.Primo)
                                newmodel = Convert.ToString(VehicleHash.Primo2);
                            else if (model == VehicleHash.SabreGT)
                                newmodel = Convert.ToString(VehicleHash.SabreGT2);
                            else if (model == VehicleHash.SlamVan)
                                newmodel = Convert.ToString(VehicleHash.SlamVan3);
                            else if (model == VehicleHash.Specter)
                                newmodel = Convert.ToString(VehicleHash.Specter2);
                            else if (model == VehicleHash.Sultan)
                                newmodel = Convert.ToString(VehicleHash.SultanRS);
                            else if (model == VehicleHash.Tornado)
                                newmodel = Convert.ToString(VehicleHash.Tornado5);
                            else if (model == VehicleHash.Virgo)
                                newmodel = Convert.ToString(VehicleHash.Virgo2);
                            else if (model == VehicleHash.Voodoo2)
                                newmodel = Convert.ToString(VehicleHash.Voodoo);
                            else if (model == VehicleHash.Buffalo)
                                newmodel = Convert.ToString(VehicleHash.Buffalo2);
                            else if (model == VehicleHash.RapidGT)
                                newmodel = Convert.ToString(VehicleHash.RapidGT2);
                            else if (model == VehicleHash.Schafter2)
                                newmodel = Convert.ToString(VehicleHash.Schafter4);
                            else if (model == VehicleHash.Sentinel)
                                newmodel = Convert.ToString(VehicleHash.Sentinel2);
                            else if (model == VehicleHash.Rebel)
                                newmodel = Convert.ToString(VehicleHash.Rebel2);
                            else if (model == VehicleHash.Ninef)
                                newmodel = Convert.ToString(VehicleHash.Ninef2);
                            else if (model == VehicleHash.Banshee2)
                                newmodel = Convert.ToString(VehicleHash.Banshee);
                            else if (model == VehicleHash.Chino2)
                                newmodel = Convert.ToString(VehicleHash.Chino);
                            else if (model == VehicleHash.Diablous2)
                                newmodel = Convert.ToString(VehicleHash.Diablous);
                            else if (model == VehicleHash.Comet3)
                                newmodel = Convert.ToString(VehicleHash.Comet2);
                            else if (model == VehicleHash.Elegy)
                                newmodel = Convert.ToString(VehicleHash.Elegy2);
                            else if (model == VehicleHash.Faction3)
                                newmodel = Convert.ToString(VehicleHash.Faction);
                            else if (model == VehicleHash.Minivan2)
                                newmodel = Convert.ToString(VehicleHash.Minivan);
                            else if (model == VehicleHash.Moonbeam2)
                                newmodel = Convert.ToString(VehicleHash.Moonbeam);
                            else if (model == VehicleHash.Nero2)
                                newmodel = Convert.ToString(VehicleHash.Nero);
                            else if (model == VehicleHash.Primo2)
                                newmodel = Convert.ToString(VehicleHash.Primo);
                            else if (model == VehicleHash.SabreGT2)
                                newmodel = Convert.ToString(VehicleHash.SabreGT);
                            else if (model == VehicleHash.SlamVan3)
                                newmodel = Convert.ToString(VehicleHash.SlamVan);
                            else if (model == VehicleHash.Specter2)
                                newmodel = Convert.ToString(VehicleHash.Specter);
                            else if (model == VehicleHash.SultanRS)
                                newmodel = Convert.ToString(VehicleHash.Sultan);
                            else if (model == VehicleHash.Tornado5)
                                newmodel = Convert.ToString(VehicleHash.Tornado);
                            else if (model == VehicleHash.Virgo2)
                                newmodel = Convert.ToString(VehicleHash.Virgo);
                            else if (model == VehicleHash.Voodoo)
                                newmodel = Convert.ToString(VehicleHash.Voodoo2);
                            else if (model == VehicleHash.Buffalo2)
                                newmodel = Convert.ToString(VehicleHash.Buffalo);
                            else if (model == VehicleHash.RapidGT2)
                                newmodel = Convert.ToString(VehicleHash.RapidGT);
                            else if (model == VehicleHash.Schafter4)
                                newmodel = Convert.ToString(VehicleHash.Schafter2);
                            else if (model == VehicleHash.Sentinel2)
                                newmodel = Convert.ToString(VehicleHash.Sentinel);
                            else if (model == VehicleHash.Rebel2)
                                newmodel = Convert.ToString(VehicleHash.Rebel);
                            else if (model == VehicleHash.Ninef2)
                                newmodel = Convert.ToString(VehicleHash.Ninef);
                            else if (model == VehicleHash.Tornado)
                                newmodel = Convert.ToString(VehicleHash.Tornado5);
                            else if (model == VehicleHash.Tornado2)
                                newmodel = Convert.ToString(VehicleHash.Tornado5);
                            else if (model == VehicleHash.Tornado3)
                                newmodel = Convert.ToString(VehicleHash.Tornado5);
                            else if (model == VehicleHash.Tornado4)
                                newmodel = Convert.ToString(VehicleHash.Tornado5);
                            //else if (model == VehicleHash.Fcr2)
                            //    newmodel = Convert.ToString(VehicleHash.Fcr);
                            else
                            {
                                iPlayer.SendNewNotification(
                                    
                                    "Leider koennen wir dein Auto nicht aufruesten. Komm bitte mit einem Modell wieder, das wir umbauen koennen.");
                                return;
                            }

                            if (newmodel != "")
                            {
                                if (!iPlayer.TakeMoney(price))
                                {
                                    iPlayer.SendNewNotification(
                                         MSG.Money.NotEnoughMoney(price));
                                    return;
                                }

                                //UpdateDB
                                string x = player.Position.X.ToString().Replace(",", ".");
                                string y = player.Position.Y.ToString().Replace(",", ".");
                                string z = player.Position.Z.ToString().Replace(",", ".");
                                string heading = player.Rotation.Z.ToString().Replace(",", ".");

                                if (!Enum.TryParse(newmodel, true, out VehicleHash newModelHash)) return;

                                if (VehicleDataModule.Instance.GetData((uint)newModelHash) != null) return;

                                string query = String.Format(
                                    "UPDATE `vehicles` SET `pos_x` = '{0}', `pos_y` = '{1}', `pos_z` = '{2}', `heading` = '{3}', model = '{4}', tuning = '' WHERE id = '{5}';",
                                    x, y, z, heading, VehicleDataModule.Instance.GetData((uint)newModelHash).Id.ToString(),
                                    playerVeh.databaseId);

                                MySQLHandler.Execute(query);

                                // set Car New
                                // ResetMods && clear
                                uint ownerid = playerVeh.databaseId;

                                // Spawn new Vehicle XX
                                VehicleHandler.Instance.DeleteVehicleByEntity(playerVeh.entity);

                                try
                                {
                                    query = string.Format(
                                        "SELECT * FROM `vehicles` WHERE id = '{0}' ORDER BY id", ownerid);
                                    using (var conn =
                                        new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                                    using (var cmd = conn.CreateCommand())
                                    {
                                        conn.Open();
                                        cmd.CommandText = @query;
                                        using (var reader = cmd.ExecuteReader())
                                        {
                                            if (reader.HasRows)
                                            {
                                                while (reader.Read())
                                                {
                                                    SxVehicle xVeh = VehicleHandler.Instance.CreateServerVehicle(
                                                        reader.GetUInt32("model"), reader.GetInt32("registered") == 1,
                                                        new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"),
                                                            (reader.GetFloat("pos_z") + 0.3f)),
                                                        reader.GetFloat("heading"), reader.GetInt32("color1"),
                                                        reader.GetInt32("color2"), 0, reader.GetUInt32("gps_tracker") == 1, true, true, 0,
                                                        "",
                                                        reader.GetUInt32("id"), 0, reader.GetUInt32("owner"),
                                                        reader.GetInt32("fuel"), reader.GetInt32("zustand"), 
                                                        reader.GetString("tuning"), reader.GetString("neon"),
                                                        reader.GetFloat("km"), null, "", false, reader.GetInt32("TuningState") == 1, WheelClamp:reader.GetInt32("WheelClamp"), AlarmSystem: reader.GetInt32("alarm_system") == 1);
                                                    xVeh.entity.NumberPlate = reader.GetString("plate");

                                                    Main.WarpPlayerIntoVehicle(player, xVeh.entity, -1);
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception)
                                {
                                }

                                iPlayer.SendNewNotification(

                                    "Ihr Fahrzeug wurde erfolgreich entwickelt, bitte parke es erneut!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_changecar);
                                return;
                            }


                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_changecar);
                            break;
                        default:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_changecar);
                            break;
                    }
                }
                else if (menuid == Dialogs.menu_shop_stores)
                {
                    /*Shop shop = Shops.Instance.GetThisShop(player.Position);
                    if (shop == null) return;

                    int idx = 0;
                    if (index == 1)
                    {
                        // Guthaben
                        if (iPlayer.guthaben[0] >= 900)
                        {
                            iPlayer.SendNewNotification(
                                
                                "Sie haben das maximale Limit an Guthaben erreicht!");
                            return;
                        }

                        if (!iPlayer.TakeMoney(100))
                        {
                            iPlayer.SendNewNotification(
                                 MSG.Money.NotEnoughMoney(100));
                            return;
                        }

                        iPlayer.guthaben[0] = iPlayer.guthaben[0] + 100;
                        iPlayer.SendNewNotification(
                             "Sie haben $100 Guthaben gekauft!");
                        return;
                    }

                    if (index == 2)
                    {
                        if (iPlayer.Container.CanInventoryItemAdded(12))
                        {
                            iPlayer.SendNewNotification( "Inventar ist voll!");
                            return;
                        }

                        if (!iPlayer.TakeMoney(12).BuyPrice))
                        {
                            iPlayer.SendNewNotification(
                                
                                MSG.Money.NotEnoughMoney(12).BuyPrice));
                            return;
                        }

                        iPlayer.Container.AddItem(12), 1);
                        iPlayer.SendNewNotification(
                             "Sie haben ein " +
                            12).Name +
                            " fuer $" + 12).BuyPrice + " gekauft!");
                        return;
                    }

                    if (index > 2)
                    {
                        List<ItemData> shopitems =
                            ItemHandler.Instance.GetItemsByMenu((int) Dialogs.menu_shop_stores);
                        foreach (ItemData kvp in shopitems)
                        {
                            if (kvp == null) continue;
                            if (idx == index - 3)
                            {
                                iPlayer.SetData("sBuyItem", kvp.Id);
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_stores);

                                player.CreateUserDialog(Dialogs.menu_shop_input, "input");
                                return;
                            }

                            idx++;
                        }
                    }

                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_stores);*/
                }
                else if (menuid == Dialogs.menu_shop_rebel_weapons)
                {
                    /*if (iPlayer.IsInventoryMax())
                    {
                        iPlayer.SendNewNotification( "Inventar ist voll!");
                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_rebel_weapons);
                        return;
                    }

                    if (!iPlayer.IsAGangster()) return;

                    int idx = 0;

                    if (index > 0)
                    {
                        List<ItemData> shopitems =
                            ItemHandler.Instance.GetItemsByMenu(Convert.ToInt32(Dialogs.menu_shop_rebel_weapons));
                        foreach (ItemData kvp in shopitems)
                        {
                            if (kvp == null) continue;
                            if (idx == index - 1)
                            {
                                if (iPlayer.IsInventoryMax() ||
                                    !iPlayer.Container.CanInventoryItemAdded(kvp))
                                {
                                    iPlayer.SendNewNotification(
                                         "Inventar ist voll!");
                                    return;
                                }

                                int price = kvp.BuyPrice;
                                if (!iPlayer.TakeMoney(price))
                                {
                                    iPlayer.SendNewNotification(
                                         MSG.Money.NotEnoughMoney(price));
                                    return;
                                }

                                iPlayer.Container.AddItem(kvp, 1);
                                iPlayer.SendNewNotification(
                                     "Sie haben sich " + kvp.Name +
                                    " fuer $" +
                                    price + " gekauft!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_rebel_weapons);
                                return;
                            }

                            idx++;
                        }
                    }

                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_rebel_weapons);*/
                }
                else if (menuid == Dialogs.menu_garage_overlay)
                {
                    if (!iPlayer.HasData("GarageId")) return;
                    Garage garage = GarageModule.Instance[iPlayer.GetData("GarageId")];
                    switch (index)
                    {
                        case 1: // Getlist
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_garage_overlay, true);
                            DialogMigrator.CreateMenu(player, Dialogs.menu_garage_getlist, "Fahrzeug-Garage", "");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_garage_getlist, MSG.General.Close(), "");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_garage_getlist, MSG.General.Back(), "");

                            if (garage != null)
                            {
                                // Fraktionsgarage
                                if (garage.IsTeamGarage() && garage.Teams.Contains(iPlayer.TeamId))
                                {
                                    // Exclude GWD
                                    if (iPlayer.Team.Id == (int)teams.TEAM_ARMY && iPlayer.TeamRank == 0) return;

                                    iPlayer.SetData("garage_getlist", Main.getTeamGarageVehicleList(
                                        iPlayer.TeamId, garage));

                                    // List Fahrzeuge
                                    foreach (KeyValuePair<uint, string> kvp in iPlayer.GetData("garage_getlist"))
                                    {
                                        DialogMigrator.AddMenuItem(player, Dialogs.menu_garage_getlist, kvp.Value,
                                            Convert.ToString(kvp.Key));
                                    }
                                }
                                else
                                {
                                    iPlayer.SetData("garage_getlist", Main.getPlayerGarageVehicleList(
                                        iPlayer, garage));

                                    // List Fahrzeuge
                                    foreach (KeyValuePair<uint, string> kvp in iPlayer.GetData("garage_getlist"))
                                    {
                                        DialogMigrator.AddMenuItem(player, Dialogs.menu_garage_getlist, kvp.Value + " - " + Convert.ToString(kvp.Key), "");
                                    }
                                }
                            }

                            DialogMigrator.OpenUserMenu(iPlayer, Dialogs.menu_garage_getlist);
                            break;
                        case 2: // setList

                            // Verwahrplatz kein SET
                            if (garage.Type == GarageType.VehicleCollection) return;
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_garage_overlay, true);
                            DialogMigrator.CreateMenu(player, Dialogs.menu_garage_setlist, "Fahrzeug-Garage", "");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_garage_setlist, MSG.General.Close(), "");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_garage_setlist, MSG.General.Back(), "");

                            if (garage != null)
                            {
                                if (garage.HouseId > 0 &&
                                    (iPlayer.ownHouse[0] != garage.HouseId && !iPlayer.HouseKeys.Contains(garage.HouseId) &&
                                    (!iPlayer.IsTenant() || iPlayer.GetTenant().HouseId != garage.HouseId))) return;

                                // Fraktionsgarage
                                if (garage.IsTeamGarage() && garage.Teams.Contains(iPlayer.TeamId))
                                {
                                    // List Fahrzeuge
                                    foreach (SxVehicle Vehicle in VehicleHandler.Instance.GetAllVehicles())
                                    {
                                        if (Vehicle == null) continue;

                                        if (Vehicle.teamid == iPlayer.TeamId &&
                                            Utils.IsPointNearPoint(25.0f, player.Position,
                                                Vehicle.entity.Position))
                                        {
                                            string l_Name = "";
                                            if (Vehicle.Data.modded_car == 1)
                                                l_Name = Vehicle.Data.mod_car_name;
                                            else
                                                l_Name = Vehicle.Data.Model;
                                            DialogMigrator.AddMenuItem(player, Dialogs.menu_garage_setlist,
                                                l_Name,
                                                "");
                                        }
                                    }
                                }
                                else
                                {
                                    // List Fahrzeuge
                                    foreach (SxVehicle Vehicle in VehicleHandler.Instance.GetAllVehicles())
                                    {
                                        if (Vehicle == null) continue;
                                        if (Vehicle.databaseId == 0) continue;
                                        if (iPlayer.CanControl(Vehicle) &&
                                            Utils.IsPointNearPoint(25.0f, player.Position,
                                                Vehicle.entity.Position))
                                        {
                                            string l_Name = "";
                                            if (Vehicle.Data.modded_car == 1)
                                                l_Name = Vehicle.Data.mod_car_name;
                                            else
                                                l_Name = Vehicle.Data.Model;
                                            if (garage.Classifications.Contains(Vehicle.Data.ClassificationId))
                                            {
                                                if (!Vehicle.IsTeamVehicle())
                                                {
                                                    DialogMigrator.AddMenuItem(player, Dialogs.menu_garage_setlist, l_Name + " - " + Vehicle.databaseId, "");
                                                }
                                            }
                                        }
                                    }
                                }
                            }

                            DialogMigrator.OpenUserMenu(iPlayer, Dialogs.menu_garage_setlist);
                            break;
                        
                        default:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_garage_overlay);
                            break;
                    }
                }
                else if (menuid == Dialogs.menu_garage_setlist)
                {
                    if (!iPlayer.HasData("GarageId")) return;
                    Garage garage = GarageModule.Instance[iPlayer.GetData("GarageId")];
                    if (garage == null) return;
                    if (index == 0) // Close
                    {
                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_garage_setlist);
                        return;
                    }
                    else if (index == 1) // Return
                    {
                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_garage_setlist, true);
                        CreateUserMenuFahrzeugGarage(player, iPlayer, garage);
                    }
                    else
                    {
                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_garage_setlist);
                        
                        if (garage != null)
                        {
                            // Fraktionsgarage
                            /*if (garage.IsTeamGarage() && garage.TeamId == iPlayer.TeamId)
                            {
                                int idx = 0;
                                foreach (SxVehicle Vehicle in VehicleHandler.Instance.GetAllVehicles())
                                {
                                    if (Vehicle == null) continue;

                                    if (garage.Classifications.Contains(Vehicle.Data.ClassificationId) &&
                                        Vehicle.teamid == iPlayer.TeamId &&
                                        player.Position.DistanceTo(Vehicle.entity.Position) <= 25.0f)
                                    {
                                        if (idx == index - 2)
                                        {
                                            Vehicle.SetTeamCarGarage(true);
                                            iPlayer.SendNewNotification(

                                                "Fahrzeug wurde in die Garage eingelagert!");
                                            return;
                                        }

                                        idx++;
                                    }
                                }
                            }*/
                            if (!garage.IsTeamGarage())
                            {
                                if(garage.HouseId > 0 && !garage.CanVehiclePutIntoHouseGarage())
                                {
                                    iPlayer.SendNewNotification("Hausgarage ist voll!");
                                    return;
                                }

                                int idx = 0;
                                foreach (SxVehicle Vehicle in VehicleHandler.Instance.GetAllVehicles())
                                {
                                    if (Vehicle == null) continue;
                                    if (Vehicle.databaseId == 0) continue;
                                    if (Vehicle.IsTeamVehicle()) continue;
                                    if (!garage.Classifications.Contains(Vehicle.Data.ClassificationId)) continue;
                                    if (garage.Classifications.Contains(Vehicle.Data.ClassificationId) &&
                                        iPlayer.CanControl(Vehicle) &&
                                        player.Position.DistanceTo(Vehicle.entity.Position) <= 25.0f)
                                    {
                                            if (idx == index - 2)
                                            {
                                                Vehicle.SetPrivateCarGarage(1, garage.Id);
                                                iPlayer.SendNewNotification("Fahrzeug wurde in die Garage eingelagert!");
                                                return;
                                            }
                                        idx++;
                                    }
                                }
                                return;
                            }
                            else
                            {
                                iPlayer.SendNewNotification("derzeit deaktiviert!");
                                return;
                            }
                        }
                        return;
                    }
                }
                else if (menuid == Dialogs.menu_garage_getlist)
                {
                    if (!iPlayer.HasData("GarageId")) return;
                    Garage garage = GarageModule.Instance[iPlayer.GetData("GarageId")];
                    if (garage == null) return;
                    if (index == 0) // Close
                    {
                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_garage_getlist);
                        return;
                    }
                    else if (index == 1) // Return
                    {
                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_garage_getlist, true);
                        CreateUserMenuFahrzeugGarage(player, iPlayer, garage);
                    }
                    else
                    {
                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_garage_getlist);

                        if (garage.Type == GarageType.VehicleCollection)
                        {
                            if (!iPlayer.TakeMoney(2500))
                            {
                                iPlayer.SendNewNotification(
                                    
                                    "Um ein Fahrzeug freizukaufen benötigst du mindestens $2500 fuer eine Kaution!");
                            }

                            return;
                        }

                        if (garage.Id == 0)
                        {
                            if (!iPlayer.TakeMoney(500))
                            {
                                iPlayer.SendNewNotification(
                                    
                                    "Dein Fahrzeug wurde zerstört um es zu reparieren benötigst du mindestens 500$!");
                                return;
                            }
                        }

                        if (garage.Rang > 0 && iPlayer.TeamRank < garage.Rang)
                        {
                            iPlayer.SendNewNotification(
                                 "Sie haben nicht den benötigten Rang!");
                            return;
                        }

                        var spawnPos = garage.GetFreeSpawnPosition();

                        if (spawnPos == null)
                        {
                            iPlayer.SendNewNotification(

                                "Jemand anderes hat gerade sein Fahrzeug ausgeparkt, bitte warte kurz!");
                            return;
                        }

                        if (garage != null)
                        {
                            // Fraktionsgarage
                            if (garage.IsTeamGarage() && garage.Teams.Contains(iPlayer.TeamId))
                            {
                                int idx = index - 2;

                                NetHandle xveh = Main.LoadTeamVehicle(iPlayer.TeamId, idx, garage, spawnPos);
                                if (xveh != null)
                                {
                                    iPlayer.SendNewNotification(

                                        "Sie haben Ihr Fraktions Fahrzeug erfolgreich aus der Garage entnommen!");
                                    return;
                                }

                                return;
                            }
                            else
                            {

                                Dictionary<uint, string> VehicleList = iPlayer.GetData("garage_getlist");

                                int idx = 0;
                                foreach (KeyValuePair<uint, string> kvp in VehicleList)
                                {
                                    if (idx == index - 2)
                                    {
                                        SynchronizedTaskManager.Instance.Add(
                                            new GaragePlayerVehicleTakeOutTask(garage, kvp.Key, iPlayer, spawnPos));
                                        iPlayer.SendNewNotification(

                                            $"Ihr Fahrzeug {kvp.Value} ({kvp.Key}) wurde aus der Garage entnommen!");
                                        return;
                                    }

                                    idx++;
                                }

                                return;
                            }
                        }
                    }
                }
                else if (menuid == Dialogs.menu_shop_ammunation_main)
                {
                    switch (index)
                    {
                        case 0: // weapons

                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_ammunation_main, true);
                            //Waffenshop
                            DialogMigrator.CreateMenu(player, Dialogs.menu_shop_ammunation, "Ammunation", "");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_ammunation, MSG.General.Close(), "");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_ammunation, MSG.General.Back(), "");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_ammunation, "Pistole (12000$)", "");
                            //DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_ammunation, "Pistole 50 (8000$)", "");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_ammunation, "Schwere Pistole (17000$)", "");
                            
                            DialogMigrator.OpenUserMenu(iPlayer, Dialogs.menu_shop_ammunation);
                            break;
                        case 1: // ammo

                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_ammunation_main, true);
                            //ammo
                            DialogMigrator.CreateMenu(player, Dialogs.menu_shop_ammunation_ammo, "Ammunation", "");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_ammunation_ammo, MSG.General.Close(), "");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_ammunation_ammo, MSG.General.Back(), "");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_ammunation_ammo, "Pistole Ammo (150$)", "");
                           // DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_ammunation_ammo, "Pistole 50 Ammo (500$)", "");
                            DialogMigrator.AddMenuItem(player, Dialogs.menu_shop_ammunation_ammo, "Schwere Pistole Ammo (300$)", "");
                            
                            DialogMigrator.OpenUserMenu(iPlayer, Dialogs.menu_shop_ammunation_ammo);
                            break;
                        default:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_ammunation_main);
                            break;
                    }
                }
                else if (menuid == Dialogs.menu_shop_ammunation)
                {
                    switch (index)
                    {
                        case 0:
                            {
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_ammunation);
                                break;
                            }
                        case 1:
                            {
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_ammunation);
                                break;
                            }
                        case 2:
                            {
                                if (!iPlayer.Container.CanInventoryItemAdded(59, 1))
                                {
                                    iPlayer.SendNewNotification("Du hast keinen Platz fuer dieses Item!");
                                    return;
                                }

                                if (!iPlayer.TakeMoney(4000))
                                {
                                    iPlayer.SendNewNotification("Du hast nicht genug Geld dabei!");
                                    return;
                                }

                                iPlayer.Container.AddItem(59, 1);
                                break;
                            }
                        case 3:
                            {
                                if (!iPlayer.Container.CanInventoryItemAdded(63, 1))
                                {
                                    iPlayer.SendNewNotification("Du hast keinen Platz fuer dieses Item!");
                                    return;
                                }

                                if (!iPlayer.TakeMoney(7000))
                                {
                                    iPlayer.SendNewNotification("Du hast nicht genug Geld dabei!");
                                    return;
                                }

                                iPlayer.Container.AddItem(63, 1);
                                break;
                            }
                        default:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_ammunation);
                            break;
                    }
                }
                else if (menuid == Dialogs.menu_weapondealer)
                {
                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_weapondealer);
                }
                else if (menuid == Dialogs.menu_shop_ammunation_ammo)
                {
                    switch (index)
                    {
                        case 0:
                            {
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_ammunation);
                                break;
                            }
                        case 1:
                            {
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_ammunation);
                                break;
                            }
                        case 2:
                            {
                                if (!iPlayer.Container.CanInventoryItemAdded(200, 1))
                                {
                                    iPlayer.SendNewNotification("Du hast keinen Platz fuer dieses Item!");
                                    return;
                                }

                                if (!iPlayer.TakeMoney(150))
                                {
                                    iPlayer.SendNewNotification("Du hast nicht genug Geld dabei!");
                                    return;
                                }

                                iPlayer.Container.AddItem(200, 1);
                                break;
                            }
                        case 3:
                            {
                                if (!iPlayer.Container.CanInventoryItemAdded(202, 1))
                                {
                                    iPlayer.SendNewNotification("Du hast keinen Platz fuer dieses Item!");
                                    return;
                                }

                                if (!iPlayer.TakeMoney(300))
                                {
                                    iPlayer.SendNewNotification("Du hast nicht genug Geld dabei!");
                                    return;
                                }

                                iPlayer.Container.AddItem(202, 1);
                                break;
                            }
                        case 4:
                            {
                                if (!iPlayer.Container.CanInventoryItemAdded(204, 1))
                                {
                                    iPlayer.SendNewNotification("Du hast keinen Platz fuer dieses Item!");
                                    return;
                                }

                                if (!iPlayer.TakeMoney(250))
                                {
                                    iPlayer.SendNewNotification("Du hast nicht genug Geld dabei!");
                                    return;
                                }

                                iPlayer.Container.AddItem(204, 1);
                                break;
                            }
                        default:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_ammunation);
                            break;
                    }
                }
                else if (menuid == Dialogs.menu_taxi)
                {
                    switch (index)
                    {
                        case 0: // Taxi Lic
                            if (iPlayer.Lic_Taxi[0] == 1)
                            {
                                iPlayer.SendNewNotification(
                                    
                                    "Sie besitzen bereits eine Taxilizenz!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_taxi);
                                break;
                            }

                            if (!iPlayer.TakeMoney(4300))
                            {
                                iPlayer.SendNewNotification(
                                     MSG.Money.NotEnoughMoney(4300));
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_taxi);
                                break;
                            }

                            iPlayer.Lic_Taxi[0] = 1;
                            iPlayer.SendNewNotification(

                                "Sie haben eine Taxi Lizenz fuer $4300 erworben!");
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_taxi);
                            break;
                        case 1: // Aus Dienst
                            if (iPlayer.HasData("taxi"))
                            {
                                iPlayer.ResetData("taxi");
                                iPlayer.SendNewNotification(

                                    "Sie haben nun den Dienst verlassen und sind fuer keine Kunden mehr sichtbar!");
                                iPlayer.SendNewNotification(

                                    "Um erneut sichtbar fuer Kunden zu werden muessen Sie die Fahrtkosten setzen! (/taxi ($200-$999))");
                                break;
                            }
                            else
                            {
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_taxi);
                                break;
                            }
                        default:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_taxi);
                            break;
                    }
                }
                else if (menuid == Dialogs.menu_givelicenses)
                {
                    if (!iPlayer.HasData("giveLic")) return;
                    Client xplayer = iPlayer.GetData("giveLic");

                    DbPlayer xPlayer = xplayer.GetPlayer();
                    if (xPlayer == null || !xPlayer.IsValid()) return;

                    iPlayer.ResetData("giveLic");
                    if (xPlayer == null) return;

                    int bonus = 0;
                    var licence = "";

                    switch (index)
                    {
                        case 0: // Fuehrerschein
                            if (xPlayer.Lic_Car[0] < 0)
                            {
                                iPlayer.SendNewNotification(
                                    
                                    "Spieler hat eine Sperre fuer diese Lizenz!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                break;
                            }

                            if (xPlayer.Lic_Car[0] == 1)
                            {
                                iPlayer.SendNewNotification(
                                    
                                    MSG.License.PlayerAlreadyOwnLic(Content.License.Car));
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                break;
                            }

                            if (!xPlayer.TakeMoney(Price.License.Car))
                            {
                                iPlayer.SendNewNotification(
                                    
                                    MSG.Money.PlayerNotEnoughMoney(Price.License.Car));
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                return;
                            }

                            xPlayer.Lic_Car[0] = 1;
                            xPlayer.SendNewNotification(
                                 MSG.License.HasGiveYouLicense(
                                    iPlayer.GetName(),
                                    Price.License.Car, Content.License.Car));
                            xPlayer.SendNewNotification(
                                MSG.License.HaveGetLicense(Content.License.Car));
                            iPlayer.SendNewNotification(
                                 MSG.License.YouHaveGiveLicense(
                                    xPlayer.GetName(),
                                    Content.License.Car));
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);

                            bonus = Convert.ToInt32(Price.License.Car / 10) + 200;
                            iPlayer.GiveMoney(bonus);
                            iPlayer.SendNewNotification(
                                 "Bonus durch Scheinvergabe: $" + bonus);
                            licence = Content.License.Car;
                            break;
                        case 1: // LKW
                            if (xPlayer.Lic_LKW[0] < 0)
                            {
                                iPlayer.SendNewNotification(
                                    
                                    "Spieler hat eine Sperre fuer diese Lizenz!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                break;
                            }
                            if (xplayer.GetPlayer().IsHomeless())
                            {
                                iPlayer.SendNewNotification(

                                    "Bürger hat keinen Wohnsitz und kann die Lizenz nicht erhalten");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                break;
                            }

                            if (xPlayer.Lic_LKW[0] == 1)
                            {
                                iPlayer.SendNewNotification(
                                    
                                    MSG.License.PlayerAlreadyOwnLic(Content.License.Lkw));
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                break;
                            }

                            if (!xPlayer.TakeMoney(Price.License.Lkw))
                            {
                                iPlayer.SendNewNotification(
                                    
                                    MSG.Money.PlayerNotEnoughMoney(Price.License.Lkw));
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                return;
                            }

                            xPlayer.Lic_LKW[0] = 1;
                            xPlayer.SendNewNotification(
                                 MSG.License.HasGiveYouLicense(
                                    iPlayer.GetName(),
                                    Price.License.Lkw, Content.License.Lkw));
                            xPlayer.SendNewNotification(
                                MSG.License.HaveGetLicense(Content.License.Lkw));
                            iPlayer.SendNewNotification(
                                 MSG.License.YouHaveGiveLicense(
                                    xPlayer.GetName(),
                                    Content.License.Lkw));
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);

                            bonus = Convert.ToInt32(Price.License.Lkw / 10) + 200;
                            iPlayer.GiveMoney(bonus);
                            iPlayer.SendNewNotification(
                                 "Bonus durch Scheinvergabe: $" + bonus);
                            licence = Content.License.Lkw;
                            break;
                        case 2: // Motorrad
                            if (xPlayer.Lic_Bike[0] < 0)
                            {
                                iPlayer.SendNewNotification(
                                    
                                    "Spieler hat eine Sperre fuer diese Lizenz!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                break;
                            }
                            if (xplayer.GetPlayer().IsHomeless())
                            {
                                iPlayer.SendNewNotification(

                                    "Bürger hat keinen Wohnsitz und kann die Lizenz nicht erhalten");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                break;
                            }

                            if (xPlayer.Lic_Bike[0] == 1)
                            {
                                iPlayer.SendNewNotification(
                                    
                                    MSG.License.PlayerAlreadyOwnLic(Content.License.Bike));
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                break;
                            }

                            if (!xPlayer.TakeMoney(Price.License.Bike))
                            {
                                iPlayer.SendNewNotification(
                                    
                                    MSG.Money.PlayerNotEnoughMoney(Price.License.Bike));
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                return;
                            }

                            xPlayer.Lic_Bike[0] = 1;
                            xPlayer.SendNewNotification(
                                 MSG.License.HasGiveYouLicense(
                                    iPlayer.GetName(),
                                    Price.License.Bike, Content.License.Bike));
                            xPlayer.SendNewNotification(
                                MSG.License.HaveGetLicense(Content.License.Bike));
                            iPlayer.SendNewNotification(
                                 MSG.License.YouHaveGiveLicense(
                                    xPlayer.GetName(),
                                    Content.License.Bike));
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);

                            bonus = Convert.ToInt32(Price.License.Bike / 10) + 200;
                            iPlayer.GiveMoney(bonus);
                            iPlayer.SendNewNotification(
                                 "Bonus durch Scheinvergabe: $" + bonus);
                            licence = Content.License.Bike;
                            break;
                        case 3: // Bootsschein
                            if (xPlayer.Lic_Boot[0] < 0)
                            {
                                iPlayer.SendNewNotification(
                                    
                                    "Spieler hat eine Sperre fuer diese Lizenz!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                break;
                            }
                            if (xplayer.GetPlayer().IsHomeless())
                            {
                                iPlayer.SendNewNotification(

                                    "Bürger hat keinen Wohnsitz und kann die Lizenz nicht erhalten");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                break;
                            }

                            if (xPlayer.Lic_Boot[0] == 1)
                            {
                                iPlayer.SendNewNotification(
                                    
                                    MSG.License.PlayerAlreadyOwnLic(Content.License.Boot));
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                break;
                            }

                            if (!xPlayer.TakeMoney(Price.License.Boot))
                            {
                                iPlayer.SendNewNotification(
                                    
                                    MSG.Money.PlayerNotEnoughMoney(Price.License.Boot));
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                return;
                            }

                            xPlayer.Lic_Boot[0] = 1;
                            xPlayer.SendNewNotification(
                                 MSG.License.HasGiveYouLicense(
                                    iPlayer.GetName(),
                                    Price.License.Boot, Content.License.Boot));
                            xPlayer.SendNewNotification(
                                MSG.License.HaveGetLicense(Content.License.Boot));
                            iPlayer.SendNewNotification(
                                 MSG.License.YouHaveGiveLicense(
                                    xPlayer.GetName(),
                                    Content.License.Boot));
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);

                            bonus = Convert.ToInt32(Price.License.Boot / 10) + 200;
                            iPlayer.GiveMoney(bonus);
                            iPlayer.SendNewNotification(
                                 "Bonus durch Scheinvergabe: $" + bonus);
                            licence = Content.License.Boot;
                            break;
                        case 4: // Flugschein A
                            if (xPlayer.Lic_PlaneA[0] < 0)
                            {
                                iPlayer.SendNewNotification(
                                    
                                    "Spieler hat eine Sperre fuer diese Lizenz!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                break;
                            }
                            if (xplayer.GetPlayer().IsHomeless())
                            {
                                iPlayer.SendNewNotification(

                                    "Bürger hat keinen Wohnsitz und kann die Lizenz nicht erhalten");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                break;
                            }

                            if (xPlayer.Lic_PlaneA[0] == 1)
                            {
                                iPlayer.SendNewNotification(
                                    
                                    MSG.License.PlayerAlreadyOwnLic(Content.License.PlaneA));
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                break;
                            }

                            if (!xPlayer.TakeMoney(Price.License.PlaneA))
                            {
                                iPlayer.SendNewNotification(
                                    
                                    MSG.Money.PlayerNotEnoughMoney(Price.License.PlaneA));
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                return;
                            }

                            xPlayer.Lic_PlaneA[0] = 1;
                            xPlayer.SendNewNotification(
                                 MSG.License.HasGiveYouLicense(
                                    iPlayer.GetName(),
                                    Price.License.PlaneA, Content.License.PlaneA));
                            xPlayer.SendNewNotification(
                                MSG.License.HaveGetLicense(Content.License.PlaneA));
                            iPlayer.SendNewNotification(
                                 MSG.License.YouHaveGiveLicense(
                                    xPlayer.GetName(),
                                    Content.License.PlaneA));
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);

                            bonus = Convert.ToInt32(Price.License.PlaneA / 10) + 200;
                            iPlayer.GiveMoney(bonus);
                            iPlayer.SendNewNotification(
                                 "Bonus durch Scheinvergabe: $" + bonus);
                            licence = Content.License.PlaneA;
                            break;
                        case 5: // Flugschein B
                            if (xPlayer.Lic_PlaneB[0] < 0)
                            {
                                iPlayer.SendNewNotification(
                                    
                                    "Spieler hat eine Sperre fuer diese Lizenz!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                break;
                            }
                            if (xplayer.GetPlayer().IsHomeless())
                            {
                                iPlayer.SendNewNotification(

                                    "Bürger hat keinen Wohnsitz und kann die Lizenz nicht erhalten");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                break;
                            }

                            if (xPlayer.Lic_PlaneB[0] == 1)
                            {
                                iPlayer.SendNewNotification(
                                    
                                    MSG.License.PlayerAlreadyOwnLic(Content.License.PlaneB));
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                break;
                            }

                            if (!xPlayer.TakeMoney(Price.License.PlaneB))
                            {
                                iPlayer.SendNewNotification(
                                    
                                    MSG.Money.PlayerNotEnoughMoney(Price.License.PlaneB));
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                return;
                            }

                            xPlayer.Lic_PlaneB[0] = 1;
                            xPlayer.SendNewNotification(
                                 MSG.License.HasGiveYouLicense(
                                    iPlayer.GetName(),
                                    Price.License.PlaneB, Content.License.PlaneB));
                            xPlayer.SendNewNotification(
                                MSG.License.HaveGetLicense(Content.License.PlaneB));
                            iPlayer.SendNewNotification(
                                 MSG.License.YouHaveGiveLicense(
                                    xPlayer.GetName(),
                                    Content.License.PlaneB));
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);

                            bonus = Convert.ToInt32(Price.License.PlaneB / 10) + 200;
                            iPlayer.GiveMoney(bonus);
                            iPlayer.SendNewNotification(
                                 "Bonus durch Scheinvergabe: $" + bonus);
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                            licence = Content.License.PlaneB;
                            break;
                        case 6: // Transport
                            if (xPlayer.Lic_Transfer[0] < 0)
                            {
                                iPlayer.SendNewNotification(
                                    
                                    "Spieler hat eine Sperre fuer diese Lizenz!");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                break;
                            }
                            if (xplayer.GetPlayer().IsHomeless())
                            {
                                iPlayer.SendNewNotification(

                                    "Bürger hat keinen Wohnsitz und kann die Lizenz nicht erhalten");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                break;
                            }

                            if (xPlayer.Lic_Transfer[0] == 1)
                            {
                                iPlayer.SendNewNotification(
                                    
                                    MSG.License.PlayerAlreadyOwnLic(Content.License.Transfer));
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                break;
                            }

                            if (!xPlayer.TakeMoney(Price.License.Transfer))
                            {
                                iPlayer.SendNewNotification(
                                    
                                    MSG.Money.PlayerNotEnoughMoney(Price.License.Transfer));
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                                return;
                            }

                            xPlayer.Lic_Transfer[0] = 1;
                            xPlayer.SendNewNotification(
                                 MSG.License.HasGiveYouLicense(
                                    iPlayer.GetName(),
                                    Price.License.Transfer, Content.License.Transfer));
                            xPlayer.SendNewNotification(
                                MSG.License.HaveGetLicense(Content.License.Transfer));
                            iPlayer.SendNewNotification(
                                 MSG.License.YouHaveGiveLicense(
                                    xPlayer.GetName(),
                                    Content.License.Transfer));
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);

                            bonus = Convert.ToInt32(Price.License.Transfer / 10) + 200;
                            iPlayer.GiveMoney(bonus);
                            iPlayer.SendNewNotification(
                                 "Bonus durch Scheinvergabe: $" + bonus);
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                            licence = Content.License.Transfer;
                            break;
                        default:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_givelicenses);
                            break;
                    }

                    if (licence != "")
                    {
                        TeamModule.Instance.SendMessageToTeam($"{iPlayer.Player.Name} hat einen {licence} an {xPlayer.Player.Name} vergeben", teams.TEAM_DRIVINGSCHOOL);
                    }

                    return;
                }
                else if (menuid == Dialogs.menu_job_createlicenses)
                {
                    if (!iPlayer.HasData("fakeLic")) return;
                    DbPlayer xPlayer = iPlayer.GetData("fakeLic");
                    iPlayer.ResetData("fakeLic");
                    if (xPlayer == null) return;

                    if (iPlayer.job[0] != (int)jobs.JOB_PLAGIAT)
                    {
                        return;
                    }

                    switch (index)
                    {
                        case 0: // Fuehrerschein
                            if (iPlayer.Container.GetItemAmount(24) >=
                                JobContent.Plagiat.Materials.Car)
                            {
                                if (iPlayer.jobskill[0] < JobContent.Plagiat.Requiredskill.Car)
                                {
                                    iPlayer.SendNewNotification(
                                        
                                        MSG.Job.NotEnoughSkill(JobContent.Plagiat.Requiredskill.Car));
                                    return;
                                }

                                if (xPlayer.Lic_Car[0] == 1 || xPlayer.Lic_Car[0] == 2)
                                {
                                    iPlayer.SendNewNotification(
                                        
                                        MSG.License.PlayerAlreadyOwnLic(Content.License.Car));
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                    break;
                                }

                                xPlayer.Lic_Car[0] = 2;
                                iPlayer.Container.RemoveItem(24,
                                    JobContent.Plagiat.Materials.Car);
                             //   iPlayer.JobSkillsIncrease();
                                xPlayer.SendNewNotification(
                                     MSG.License.HasCreateYouLicense(
                                        iPlayer.GetName(),
                                        Content.License.Car));
                                xPlayer.SendNewNotification(
                                    MSG.License.HaveGetLicense(Content.License.Car));
                                iPlayer.SendNewNotification(
                                     MSG.License.YouHaveGiveLicense(
                                        xPlayer.GetName(),
                                        Content.License.Car));
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                break;
                            }
                            else
                            {
                                iPlayer.SendNewNotification(
                                    
                                    "Sie haben nicht genuegend Materialien, benoetigt (" +
                                    JobContent.Plagiat.Materials.Car + ")");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                break;
                            }
                        case 1: // LKW
                            if (iPlayer.Container.GetItemAmount(24) >=
                                JobContent.Plagiat.Materials.Lkw)
                            {
                                if (iPlayer.jobskill[0] < JobContent.Plagiat.Requiredskill.Lkw)
                                {
                                    iPlayer.SendNewNotification(
                                        
                                        MSG.Job.NotEnoughSkill(JobContent.Plagiat.Requiredskill.Lkw));
                                    return;
                                }

                                if (xPlayer.Lic_LKW[0] == 1 || xPlayer.Lic_LKW[0] == 2)
                                {
                                    iPlayer.SendNewNotification(
                                        
                                        MSG.License.PlayerAlreadyOwnLic(Content.License.Lkw));
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                    break;
                                }

                                xPlayer.Lic_LKW[0] = 2;
                                iPlayer.Container.RemoveItem(24,
                                    JobContent.Plagiat.Materials.Lkw);
                              //  iPlayer.JobSkillsIncrease();
                                xPlayer.SendNewNotification(
                                     MSG.License.HasCreateYouLicense(
                                        iPlayer.GetName(),
                                        Content.License.Lkw));
                                xPlayer.SendNewNotification(
                                    MSG.License.HaveGetLicense(Content.License.Lkw));
                                iPlayer.SendNewNotification(
                                     MSG.License.YouHaveGiveLicense(
                                        xPlayer.GetName(),
                                        Content.License.Lkw));
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                break;
                            }
                            else
                            {
                                iPlayer.SendNewNotification(
                                    
                                    "Sie haben nicht genuegend Materialien, benoetigt (" +
                                    JobContent.Plagiat.Materials.Lkw + ")");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                break;
                            }
                        case 2: // Motorrad
                            if (iPlayer.Container.GetItemAmount(24) >=
                                JobContent.Plagiat.Materials.Bike)
                            {
                                if (iPlayer.jobskill[0] < JobContent.Plagiat.Requiredskill.Bike)
                                {
                                    iPlayer.SendNewNotification(
                                        
                                        MSG.Job.NotEnoughSkill(JobContent.Plagiat.Requiredskill.Bike));
                                    return;
                                }

                                if (xPlayer.Lic_Bike[0] == 1 || xPlayer.Lic_Bike[0] == 2)
                                {
                                    iPlayer.SendNewNotification(
                                        
                                        MSG.License.PlayerAlreadyOwnLic(Content.License.Bike));
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                    break;
                                }

                                xPlayer.Lic_Bike[0] = 2;
                                iPlayer.Container.RemoveItem(24,
                                    JobContent.Plagiat.Materials.Bike);
                       //         iPlayer.JobSkillsIncrease();
                                xPlayer.SendNewNotification(
                                     MSG.License.HasCreateYouLicense(
                                        iPlayer.GetName(),
                                        Content.License.Bike));
                                xPlayer.SendNewNotification(
                                    MSG.License.HaveGetLicense(Content.License.Bike));
                                iPlayer.SendNewNotification(
                                     MSG.License.YouHaveGiveLicense(
                                        xPlayer.GetName(),
                                        Content.License.Bike));
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                break;
                            }
                            else
                            {
                                iPlayer.SendNewNotification(
                                    
                                    "Sie haben nicht genuegend Materialien, benoetigt (" +
                                    JobContent.Plagiat.Materials.Bike + ")");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                break;
                            }
                        case 3: // Boot
                            if (iPlayer.Container.GetItemAmount(24) >=
                                JobContent.Plagiat.Materials.Boot)
                            {
                                if (iPlayer.jobskill[0] < JobContent.Plagiat.Requiredskill.Boot)
                                {
                                    iPlayer.SendNewNotification(
                                        
                                        MSG.Job.NotEnoughSkill(JobContent.Plagiat.Requiredskill.Boot));
                                    return;
                                }

                                if (xPlayer.Lic_Boot[0] == 1 || xPlayer.Lic_Boot[0] == 2)
                                {
                                    iPlayer.SendNewNotification(
                                        
                                        MSG.License.PlayerAlreadyOwnLic(Content.License.Boot));
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                    break;
                                }

                                xPlayer.Lic_Boot[0] = 2;
                                iPlayer.Container.RemoveItem(24,
                                    JobContent.Plagiat.Materials.Boot);
                            //    iPlayer.JobSkillsIncrease();
                                xPlayer.SendNewNotification(
                                     MSG.License.HasCreateYouLicense(
                                        iPlayer.GetName(),
                                        Content.License.Boot));
                                xPlayer.SendNewNotification(
                                    MSG.License.HaveGetLicense(Content.License.Boot));
                                iPlayer.SendNewNotification(
                                     MSG.License.YouHaveGiveLicense(
                                        xPlayer.GetName(),
                                        Content.License.Boot));
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                break;
                            }
                            else
                            {
                                iPlayer.SendNewNotification(
                                    
                                    "Sie haben nicht genuegend Materialien, benoetigt (" +
                                    JobContent.Plagiat.Materials.Boot + ")");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                break;
                            }
                        case 4: // PlaneA
                            if (iPlayer.Container.GetItemAmount(24) >=
                                JobContent.Plagiat.Materials.PlaneA)
                            {
                                if (iPlayer.jobskill[0] < JobContent.Plagiat.Requiredskill.PlaneA)
                                {
                                    iPlayer.SendNewNotification(
                                        
                                        MSG.Job.NotEnoughSkill(JobContent.Plagiat.Requiredskill.PlaneA));
                                    return;
                                }

                                if (xPlayer.Lic_PlaneA[0] == 1 || xPlayer.Lic_PlaneA[0] == 2)
                                {
                                    iPlayer.SendNewNotification(
                                        
                                        MSG.License.PlayerAlreadyOwnLic(Content.License.PlaneA));
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                    break;
                                }

                                xPlayer.Lic_PlaneA[0] = 2;
                                iPlayer.Container.RemoveItem(24,
                                    JobContent.Plagiat.Materials.PlaneA);
                            //    iPlayer.JobSkillsIncrease();
                                xPlayer.SendNewNotification(
                                     MSG.License.HasCreateYouLicense(
                                        iPlayer.GetName(),
                                        Content.License.PlaneA));
                                xPlayer.SendNewNotification(
                                    MSG.License.HaveGetLicense(Content.License.PlaneA));
                                iPlayer.SendNewNotification(
                                     MSG.License.YouHaveGiveLicense(
                                        xPlayer.GetName(),
                                        Content.License.PlaneA));
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                break;
                            }
                            else
                            {
                                iPlayer.SendNewNotification(
                                    
                                    "Sie haben nicht genuegend Materialien, benoetigt (" +
                                    JobContent.Plagiat.Materials.PlaneA + ")");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                break;
                            }
                        case 5: // PlaneB
                            if (iPlayer.Container.GetItemAmount(24) >=
                                JobContent.Plagiat.Materials.PlaneB)
                            {
                                if (iPlayer.jobskill[0] < JobContent.Plagiat.Requiredskill.PlaneB)
                                {
                                    iPlayer.SendNewNotification(
                                        
                                        MSG.Job.NotEnoughSkill(JobContent.Plagiat.Requiredskill.PlaneB));
                                    return;
                                }

                                if (xPlayer.Lic_PlaneB[0] == 1 || xPlayer.Lic_PlaneB[0] == 2)
                                {
                                    iPlayer.SendNewNotification(
                                        
                                        MSG.License.PlayerAlreadyOwnLic(Content.License.PlaneB));
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                    break;
                                }

                                xPlayer.Lic_PlaneB[0] = 2;
                                iPlayer.Container.RemoveItem(24,
                                    JobContent.Plagiat.Materials.PlaneB);
                           //     iPlayer.JobSkillsIncrease();
                                xPlayer.SendNewNotification(
                                     MSG.License.HasCreateYouLicense(
                                        iPlayer.GetName(),
                                        Content.License.PlaneB));
                                xPlayer.SendNewNotification(
                                    MSG.License.HaveGetLicense(Content.License.PlaneB));
                                iPlayer.SendNewNotification(
                                     MSG.License.YouHaveGiveLicense(
                                        xPlayer.GetName(),
                                        Content.License.PlaneB));
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                break;
                            }
                            else
                            {
                                iPlayer.SendNewNotification(
                                    
                                    "Sie haben nicht genuegend Materialien, benoetigt (" +
                                    JobContent.Plagiat.Materials.PlaneB + ")");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                break;
                            }
                        case 6: // Biz
                            if (iPlayer.Container.GetItemAmount(24) >=
                                JobContent.Plagiat.Materials.Biz)
                            {
                                if (iPlayer.jobskill[0] <= JobContent.Plagiat.Requiredskill.Biz)
                                {
                                    iPlayer.SendNewNotification(
                                        
                                        MSG.Job.NotEnoughSkill(JobContent.Plagiat.Requiredskill.Biz));
                                    return;
                                }

                                if (xPlayer.Lic_Biz[0] == 1 || xPlayer.Lic_Biz[0] == 2)
                                {
                                    iPlayer.SendNewNotification(
                                        
                                        MSG.License.PlayerAlreadyOwnLic(Content.License.Biz));
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                    break;
                                }

                                xPlayer.Lic_Biz[0] = 2;
                                iPlayer.Container.RemoveItem(24,
                                    JobContent.Plagiat.Materials.Biz);
                      //          iPlayer.JobSkillsIncrease();
                                xPlayer.SendNewNotification(
                                     MSG.License.HasCreateYouLicense(
                                        iPlayer.GetName(),
                                        Content.License.Biz));
                                xPlayer.SendNewNotification(
                                    MSG.License.HaveGetLicense(Content.License.Biz));
                                iPlayer.SendNewNotification(
                                     MSG.License.YouHaveGiveLicense(
                                        xPlayer.GetName(),
                                        Content.License.Biz));
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                break;
                            }
                            else
                            {
                                iPlayer.SendNewNotification(
                                    
                                    "Sie haben nicht genuegend Materialien, benoetigt (" +
                                    JobContent.Plagiat.Materials.Biz + ")");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                break;
                            }
                        case 7: // Gun
                            if (iPlayer.Container.GetItemAmount(24) >=
                                JobContent.Plagiat.Materials.Gun)
                            {
                                if (iPlayer.jobskill[0] < JobContent.Plagiat.Requiredskill.Gun)
                                {
                                    iPlayer.SendNewNotification(
                                        
                                        MSG.Job.NotEnoughSkill(JobContent.Plagiat.Requiredskill.Gun));
                                    return;
                                }

                                if (xPlayer.Lic_Gun[0] == 1 || xPlayer.Lic_Gun[0] == 2)
                                {
                                    iPlayer.SendNewNotification(
                                        
                                        MSG.License.PlayerAlreadyOwnLic(Content.License.Gun));
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                    break;
                                }

                                xPlayer.Lic_Gun[0] = 2;
                                iPlayer.Container.RemoveItem(24,
                                    JobContent.Plagiat.Materials.Gun);
                            //    iPlayer.JobSkillsIncrease();
                                xPlayer.SendNewNotification(
                                     MSG.License.HasCreateYouLicense(
                                        iPlayer.GetName(),
                                        Content.License.Gun));
                                xPlayer.SendNewNotification(
                                    MSG.License.HaveGetLicense(Content.License.Gun));
                                iPlayer.SendNewNotification(
                                     MSG.License.YouHaveGiveLicense(
                                        xPlayer.GetName(),
                                        Content.License.Gun));
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                break;
                            }
                            else
                            {
                                iPlayer.SendNewNotification(
                                    
                                    "Sie haben nicht genuegend Materialien, benoetigt (" +
                                    JobContent.Plagiat.Materials.Gun + ")");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                break;
                            }
                        case 8: // Transfer
                            if (iPlayer.Container.GetItemAmount(24) >=
                                JobContent.Plagiat.Materials.Transfer)
                            {
                                if (iPlayer.jobskill[0] < JobContent.Plagiat.Requiredskill.Transfer)
                                {
                                    iPlayer.SendNewNotification(
                                        
                                        MSG.Job.NotEnoughSkill(JobContent.Plagiat.Requiredskill.Transfer));
                                    return;
                                }

                                if (xPlayer.Lic_Transfer[0] == 1 || xPlayer.Lic_Transfer[0] == 2)
                                {
                                    iPlayer.SendNewNotification(
                                        
                                        MSG.License.PlayerAlreadyOwnLic(Content.License.Transfer));
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                    break;
                                }

                                xPlayer.Lic_Transfer[0] = 2;
                                iPlayer.Container.RemoveItem(24,
                                    JobContent.Plagiat.Materials.Transfer);
                             //   iPlayer.JobSkillsIncrease();
                                xPlayer.SendNewNotification(
                                     MSG.License.HasCreateYouLicense(
                                        iPlayer.GetName(),
                                        Content.License.Transfer));
                                xPlayer.SendNewNotification(
                                    MSG.License.HaveGetLicense(Content.License.Transfer));
                                iPlayer.SendNewNotification(
                                     MSG.License.YouHaveGiveLicense(
                                        xPlayer.GetName(),
                                        Content.License.Transfer));
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                break;
                            }
                            else
                            {
                                iPlayer.SendNewNotification(
                                    
                                    "Sie haben nicht genuegend Materialien, benoetigt (" +
                                    JobContent.Plagiat.Materials.Transfer + ")");
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                                break;
                            }
                        default:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_job_createlicenses);
                            break;
                    }
                }
                else if (menuid == Dialogs.menu_takelic)
                {
                    if (!iPlayer.HasData("takeLic")) return;
                    DbPlayer xPlayer = iPlayer.GetData("takeLic");

                    iPlayer.ResetData("takeLic");

                    if (xPlayer == null || xPlayer.Player.Position.DistanceTo(player.Position) > 5.0f)
                    {
                        iPlayer.SendNewNotification(
                            
                            "Spieler muss in Ihrer Naehe sein");
                        return;
                    }

                    // Fuehrerscheinsperre
                    int sperre = -720;
                    switch (index)
                    {
                        case 0: // Fuehrerschein
                            xPlayer.Lic_Car[0] = sperre;
                            TeamModule.Instance.SendChatMessageToDepartments(iPlayer,
                                iPlayer.GetName() + " hat " + xPlayer.GetName() + " den " +
                                Content.License.Car + " entzogen!");
                            xPlayer.SendNewNotification(
                                 "Ein Beamter hat Ihnen den " +
                                Content.License.Car + " entzogen!");
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_takelic);
                            break;
                        case 1: // LKW
                            xPlayer.Lic_LKW[0] = sperre;
                            TeamModule.Instance.SendChatMessageToDepartments(iPlayer,
                                iPlayer.GetName() + " hat " + xPlayer.GetName() + " den " +
                                Content.License.Lkw + " entzogen!");
                            xPlayer.SendNewNotification(
                                 "Ein Beamter hat Ihnen den " +
                                Content.License.Lkw + " entzogen!");
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_takelic);
                            break;
                        case 2: // Motorrad
                            xPlayer.Lic_Bike[0] = sperre;
                            TeamModule.Instance.SendChatMessageToDepartments(iPlayer,
                                iPlayer.GetName() + " hat " + xPlayer.GetName() + " den " +
                                Content.License.Bike + " entzogen!");
                            xPlayer.SendNewNotification(
                                 "Ein Beamter hat Ihnen den " +
                                Content.License.Bike + " entzogen!");
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_takelic);
                            break;
                        case 3: // Boot
                            xPlayer.Lic_Boot[0] = sperre;
                            TeamModule.Instance.SendChatMessageToDepartments(iPlayer,
                                iPlayer.GetName() + " hat " + xPlayer.GetName() + " den " +
                                Content.License.Boot + " entzogen!");
                            xPlayer.SendNewNotification(
                                 "Ein Beamter hat Ihnen den " +
                                Content.License.Boot + " entzogen!");
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_takelic);
                            break;
                        case 4: // PlaneA
                            xPlayer.Lic_PlaneA[0] = sperre;
                            TeamModule.Instance.SendChatMessageToDepartments(iPlayer,
                                iPlayer.GetName() + " hat " + xPlayer.GetName() + " den " +
                                Content.License.PlaneA + " entzogen!");
                            xPlayer.SendNewNotification(
                                 "Ein Beamter hat Ihnen den " +
                                Content.License.PlaneA + " entzogen!");
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_takelic);
                            break;
                        case 5: // PlaneB
                            xPlayer.Lic_PlaneB[0] = sperre;
                            TeamModule.Instance.SendChatMessageToDepartments(iPlayer,
                                iPlayer.GetName() + " hat " + xPlayer.GetName() + " den " +
                                Content.License.PlaneB + " entzogen!");
                            xPlayer.SendNewNotification(
                                 "Ein Beamter hat Ihnen den " +
                                Content.License.PlaneB + " entzogen!");
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_takelic);
                            break;
                        case 6: // Biz
                            xPlayer.Lic_Biz[0] = sperre;
                            TeamModule.Instance.SendChatMessageToDepartments(iPlayer,
                                iPlayer.GetName() + " hat " + xPlayer.GetName() + " den " +
                                Content.License.Biz + " entzogen!");
                            xPlayer.SendNewNotification(
                                 "Ein Beamter hat Ihnen den " +
                                Content.License.Biz + " entzogen!");
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_takelic);
                            break;
                        case 7: // Gun
                            xPlayer.Lic_Gun[0] = sperre;
                            TeamModule.Instance.SendChatMessageToDepartments(iPlayer,
                                iPlayer.GetName() + " hat " + xPlayer.GetName() + " den " +
                                Content.License.Gun + " entzogen!");
                            xPlayer.SendNewNotification(
                                 "Ein Beamter hat Ihnen den " +
                                Content.License.Gun + " entzogen!");
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_takelic);
                            break;
                        case 8: // Transfer
                            xPlayer.Lic_Transfer[0] = sperre;
                            TeamModule.Instance.SendChatMessageToDepartments(iPlayer,
                                iPlayer.GetName() + " hat " + xPlayer.GetName() + " den " +
                                Content.License.Transfer + " entzogen!");
                            xPlayer.SendNewNotification(
                                 "Ein Beamter hat Ihnen den " +
                                Content.License.Transfer + " entzogen!");
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_takelic);
                            break;
                        default:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_takelic);
                            break;
                    }

                    xPlayer.Save();
                }
                else if (menuid == Dialogs.menu_bizacceptinvite)
                {
                    if (!iPlayer.HasData("bizinv"))
                    {
                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_bizacceptinvite);
                        return;
                    }

                    uint bizid = iPlayer.GetData("bizinv");
                    switch (index)
                    {
                        case 0: // Accept
                            iPlayer.SendNewNotification("Einladung angenommen!");

                            iPlayer.AddBusinessMembership(BusinessModule.Instance.GetById(bizid));

                            iPlayer.ActiveBusiness?.SendMessageToMembers(
                                $"{iPlayer.GetName()} ist {iPlayer.ActiveBusiness?.Name} beigetreten!");
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_bizacceptinvite);
                            break;
                        case 1: // Cancel
                            iPlayer.SendNewNotification("Einladung abgelehnt!");
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_bizacceptinvite);
                            break;
                        default:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_bizacceptinvite);
                            break;
                    }
                }
                else if (menuid == Dialogs.menu_invited)
                {
                    uint teamInviteId;
                    switch (index)
                    {
                        case 0:
                            if (iPlayer.TryData("teamInvite", out teamInviteId))
                            {
                                var team = TeamModule.Instance[teamInviteId];
                                if (team != null)
                                {
                                    iPlayer.SetTeamRankPermission(false, 0, false, "");
                                    iPlayer.SetTeam(teamInviteId);
                                    iPlayer.TeamRank = 0;
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_invited);
                                    iPlayer.SendNewNotification(
                                        "Sie wurden erfolgreich in die Fraktion eingeladen!", title: $"{iPlayer.Team.Name}", notificationType: PlayerNotification.NotificationType.FRAKTION);

                                    if (iPlayer.TeamId != 0)
                                    {
                                        iPlayer.Team.AddMember(iPlayer);
                                    }

                                    iPlayer.ResetData("teamInvite");

                                    PlayerSpawn.OnPlayerSpawn(player);
                                }

                                break;
                            }

                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_invited);
                            break;
                        case 1:
                            if (iPlayer.TryData("teamInvite", out teamInviteId))
                            {
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_invited);
                                iPlayer.SendNewNotification(
                                    "Sie haben den Fraktions-Invite abgelehnt!");
                                iPlayer.ResetData("teamInvite");
                                PlayerSpawn.OnPlayerSpawn(player);
                                break;
                            }

                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_invited);
                            break;
                        default:
                            if (iPlayer.HasData("teamInvite"))
                            {
                                iPlayer.ResetData("teamInvite");
                            }

                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_invited);
                            break;
                    }
                }
                else if (menuid == Dialogs.menu_show_wanteds)
                {
                    if(index == 0)
                    {
                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_show_wanteds);
                        return;
                    }
                    else
                    {
                        if (iPlayer.TeamId != (int)teams.TEAM_FIB || !iPlayer.IsInDuty())
                        {
                            iPlayer.SendNewNotification(MSG.Error.NoPermissions());
                            return;
                        }
                        int idx = 1;
                        foreach (DbPlayer xPlayer in Players.Instance.GetValidPlayers())
                        {
                            if (CrimeModule.Instance.CalcJailTime(xPlayer.Crimes) > 0)
                            {
                                if (index == idx)
                                {
                                    if (xPlayer.IsOrtable(iPlayer))
                                    {
                                        NSAModule.Instance.HandleFind(iPlayer, xPlayer);

                                        xPlayer.SetData("isOrted_" + iPlayer.TeamId, DateTime.Now.AddMinutes(1));

                                        iPlayer.SendNewNotification("Gesuchte Person " + xPlayer.GetName() + " wurde geortet!");
                                        iPlayer.Team.SendNotification($"{iPlayer.GetName()} hat die Person {xPlayer.GetName()} geortet!", 5000, 10);

                                        if (iPlayer.IsNSA || (iPlayer.TeamId == (int)teams.TEAM_FIB && iPlayer.TeamRank >= 6))
                                        {
                                            iPlayer.SetData("nsaOrtung", xPlayer.Id);
                                        }

                                        Logger.AddFindLog(iPlayer.Id, xPlayer.Id);
                                        return;
                                    }
                                    else
                                    {
                                        iPlayer.SendNewNotification("Smartphone konnte nicht geortet werden... ");
                                        return;
                                    }
                                }
                                idx++;
                            }
                        }
                    }
                    return;
                }
                else if (menuid == Dialogs.menu_academic)
                {
                    switch (index)
                    {
                        case 0:

                            if (iPlayer.uni_points[0] == 0)
                            {
                                iPlayer.SendNewNotification(
                                     "Keine Academie Punkte verfuegbar!");
                                return;
                            }

                            if (iPlayer.uni_business[0] >= 10)
                            {
                                iPlayer.SendNewNotification(
                                     "Maximales Academiclevel erreicht!");
                                return;
                            }

                            iPlayer.uni_points[0] = iPlayer.uni_points[0] - 1;
                            iPlayer.uni_business[0] = iPlayer.uni_business[0] + 1;

                            iPlayer.SendNewNotification("Sie sind nun Geschaeftsmann Stufe " +
                                iPlayer.uni_business[0] + ". Preisnachlass: " +
                                (iPlayer.uni_business[0] * 2) + "%");
                            iPlayer.SendNewNotification(
                                "Sie sind nun Geschaeftsmann Stufe " + iPlayer.uni_business[0], title: "", notificationType: PlayerNotification.NotificationType.SUCCESS);
                            break;
                        case 1:

                            if (iPlayer.uni_points[0] == 0)
                            {
                                iPlayer.SendNewNotification(
                                     "Keine Academie Punkte verfuegbar!");
                                return;
                            }

                            if (iPlayer.uni_economy[0] >= 10)
                            {
                                iPlayer.SendNewNotification(
                                     "Maximales Academiclevel erreicht!");
                                return;
                            }

                            iPlayer.uni_points[0] = iPlayer.uni_points[0] - 1;
                            iPlayer.uni_economy[0] = iPlayer.uni_economy[0] + 1;

                            iPlayer.SendNewNotification( "Sie sind nun Sparfuchs Stufe " +
                                iPlayer.uni_economy[0] + ". Sparrate: " + (iPlayer.uni_economy[0] * 2) +
                                "%");
                            iPlayer.SendNewNotification(
                                "Sie sind nun Sparfuchs Stufe " + iPlayer.uni_economy[0], title: "", notificationType: PlayerNotification.NotificationType.SUCCESS);
                            break;
                        case 2:

                            if (iPlayer.uni_points[0] == 0)
                            {
                                iPlayer.SendNewNotification(
                                     "Keine Academie Punkte verfuegbar!");
                                return;
                            }

                            if (iPlayer.uni_workaholic[0] >= 10)
                            {
                                iPlayer.SendNewNotification(
                                     "Maximales Academiclevel erreicht!");
                                return;
                            }

                            iPlayer.uni_points[0] = iPlayer.uni_points[0] - 1;
                            iPlayer.uni_workaholic[0] = iPlayer.uni_workaholic[0] + 1;

                            iPlayer.SendNewNotification("Sie sind nun Workaholic Stufe " +
                                iPlayer.uni_workaholic[0] + ". Job Erfahrung: +" +
                                (iPlayer.uni_workaholic[0] * 2) + "%");
                            iPlayer.SendNewNotification(
                                "Sie sind nun Workaholic Stufe " + iPlayer.uni_workaholic[0], title: "", notificationType: PlayerNotification.NotificationType.SUCCESS);
                            break;
                        case 3:

                            if (iPlayer.Level == 0) return;

                            int academicpoints = (iPlayer.Level - 1);

                            if (!iPlayer.TakeMoney(5000 * academicpoints))
                            {
                                iPlayer.SendNewNotification(
                                    
                                    MSG.Money.NotEnoughMoney(5000 * academicpoints));
                                return;
                            }

                            iPlayer.uni_economy[0] = 0;
                            iPlayer.uni_business[0] = 0;
                            iPlayer.uni_workaholic[0] = 0;
                            iPlayer.SendNewNotification(
                                "Sie haben Ihre academic Punkte erfolgreich resettet! (" + academicpoints +
                                " verfuegbar)");
                            iPlayer.uni_points[0] = academicpoints;
                            break;
                        default:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_academic);
                            break;
                    }
                }
                else if (menuid == Dialogs.menu_taxilist)
                {
                    // close
                    if (index == 0)
                    {
                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_taxilist);
                        return;
                    }
                    else
                    {
                        int idx = 0;
                        for (int ix = 0; ix < Users.Count; ix++)
                        {
                            if (!Users[ix].IsValid()) continue;
                            if (Users[ix].HasData("taxi") &&
                                Users[ix].Lic_Taxi[0] == 1)
                            {
                                if (idx == index - 1)
                                {
                                    string ort = iPlayer.GetData("taxi_ort");
                                    // taxifahrer gefunden yay
                                    Users[ix].SendNewNotification(
                                         "Sie haben eine Taxianfrage von " +
                                        iPlayer.GetName() + " (" + iPlayer.ForumId + ") Ort: " + ort +
                                        ", benutzen Sie /acceptservice um diese anzunehmen!");
                                    iPlayer.SendNewNotification(

                                        "Anfrage an den Taxifahrer wurde gestellt!");
                                    iPlayer.SetData("taxi_request", Users[ix].GetName());
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_taxilist);
                                    return;
                                }

                                idx++;
                            }
                        }
                    }
                }
                else if (menuid == Dialogs.menu_servicelist)
                {
                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_servicelist);
                }
                else if (menuid == Dialogs.menu_plain)
                {
                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_plain);
                }
                else if (menuid == Dialogs.menu_pd_su)
                {
                    switch (index)
                    {
                        case 0: // Waffenschein
                            int price = Price.License.Gun;
                            string Lic = Content.License.Gun;
                            if (iPlayer.Lic_Gun[0] == 0)
                            {
                                if (!iPlayer.TakeMoney(price))
                                {
                                    iPlayer.SendNewNotification(
                                         MSG.Money.NotEnoughMoney(price));
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_pd_su);
                                    return;
                                }
                                if (iPlayer.IsHomeless())
                                {
                                    iPlayer.SendNewNotification("Ohne einen Wohnsitz können Sie keinen Waffenschein erwerben!");
                                    return;
                                }

                                iPlayer.Lic_Gun[0] = 1;
                                iPlayer.SendNewNotification(MSG.License.HaveGetLicense(Lic), title: "Info", notificationType: PlayerNotification.NotificationType.INFO);
                                KassenModule.Instance.ChangeMoney(KassenModule.Kasse.STAATSKASSE, price);
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_pd_su);
                                return;
                            }

                            iPlayer.SendNewNotification(
                                 MSG.License.AlreadyOwnLic(Lic));
                            break;
                        case 1: // Ticket
                            int l_Price = 0;

                            if (iPlayer.Crimes.Count > 0)
                            {
                                if (CrimeModule.Instance.CalcJailCosts(iPlayer.Crimes) > 0)
                                {
                                    l_Price = CrimeModule.Instance.CalcJailCosts(iPlayer.Crimes);


                                    if (!iPlayer.TakeMoney(l_Price))
                                    {
                                        iPlayer.SendNewNotification(
                                            
                                            MSG.Money.NotEnoughMoney(l_Price));
                                        return;
                                    }
                                    KassenModule.Instance.ChangeMoney(KassenModule.Kasse.STAATSKASSE, l_Price);

                                    iPlayer.RemoveAllCrimes();
                                    iPlayer.SendNewNotification(
                                    "Sie haben ihr Ticket bezahlt, Ihre Delikte sind erloschen.");
                                }
                            }
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_pd_su);
                            break;
                        default:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_pd_su);
                            break;
                    }
                }

                if (menuid == Dialogs.menu_ressourcemap)
                {
                    if (index == 0)
                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_ressourcemap);

                    int idx = 1;

                    foreach (var xFarm in FarmSpotModule.Instance.GetAll())
                    {
                        if (xFarm.Value.RessourceName != "")
                        {
                            if (idx == index)
                            {
                                iPlayer.Player.TriggerEvent("setPlayerGpsMarker", xFarm.Value.Positions[0].X, xFarm.Value.Positions[0].Y);
                                iPlayer.SendNewNotification(
                                    "GPS fuer Ressource: " + xFarm.Value.RessourceName +
                                    " wurde gesetzt.", title: "Gps", notificationType: PlayerNotification.NotificationType.SUCCESS);
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_ressourcemap);
                                return;
                            }

                            idx++;
                        }
                    }

                    foreach (var farmProcess in FarmProcessModule.Instance.GetAll())
                    {
                        if (farmProcess.Value.ProcessName != "")
                        {
                            if (idx == index)
                            {
                                iPlayer.Player.TriggerEvent("setPlayerGpsMarker", farmProcess.Value.NpcPosition.X, farmProcess.Value.NpcPosition.Y);
                                iPlayer.SendNewNotification(
                                    "GPS fuer " + farmProcess.Value.ProcessName + " wurde gesetzt.", title: "Gps", notificationType: PlayerNotification.NotificationType.SUCCESS);
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_ressourcemap);
                                return;
                            }

                            idx++;
                        }
                    }
                }
                else if (menuid == Dialogs.menu_weapondealergps)
                {
                    switch (index)
                    {
                        case 0: // 
                            iPlayer.SetWaypoint(582.3424f, -2723.283f);
                            iPlayer.SendNewNotification("GPS fuer Fabrik 1 wurde gesetzt.", title: "Gps", notificationType: PlayerNotification.NotificationType.SUCCESS);
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_weapondealergps);
                            break;
                        case 1: // 
                            iPlayer.SetWaypoint(32.56189f, -627.6917f);
                            iPlayer.SendNewNotification("GPS fuer Fabrik 2 wurde gesetzt.", title: "Gps", notificationType: PlayerNotification.NotificationType.SUCCESS);
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_weapondealergps);
                            break;
                        case 2: // 
                            iPlayer.SetWaypoint(2709.886f, 4316.729f);
                            iPlayer.SendNewNotification("GPS fuer Fabrik 3 wurde gesetzt.", title: "Gps", notificationType: PlayerNotification.NotificationType.SUCCESS);
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_weapondealergps);
                            break;
                        case 3: // 
                            iPlayer.SetWaypoint(-121.5611f, 6204.626f);
                            iPlayer.SendNewNotification("GPS fuer Fabrik 4 wurde gesetzt.", title: "Gps", notificationType: PlayerNotification.NotificationType.SUCCESS);
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_weapondealergps);
                            break;
                    }
                }
                else if (menuid == Dialogs.menu_findrob)
                {
                    int interval = 0;
                    foreach (Rob rob in RobberyModule.Instance.GetActiveRobs(true))
                    {
                        if (index == interval)
                        {
                            Zone zone = ZoneModule.Instance.GetZone(rob.Player.Player.Position);
                            iPlayer.SendNewNotification($"Der Raubüberfall wurde in {zone.Name} gemeldet!");
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_findrob);
                            return;
                        }

                        interval++;
                    }
                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_findrob);
                }
                else if (menuid == Dialogs.menu_dealerhint)
                {
                    int interval = 0;
                    foreach (Dealer dealer in DealerModule.Instance.GetAll().Values.Where(dealer => dealer.Alert == true))
                    {
                        if ((index-1) == interval)
                        {
                            Vector3 position = dealer.Position;
                            position = Utils.GenerateRandomPosition(position, 400);
                            string message = "";
                            iPlayer.SendNewNotification("Der ungefähre Standort des Drogendeals wurde im GPS markiert!", duration: 10000);
                            iPlayer.SetWaypoint(position.X, position.Y);
                            iPlayer.Team.SendNotification($"Der Dealertipp wurde dem Agenten {iPlayer.GetName()} mitgeteilt.", 10000, 3);
                            if (dealer.LastSeller != null)
                            {
                                bool found = false;
                                if(dealer.LastSeller.IsFemale())
                                {
                                    message = "Die Verkäuferin trug folgende Kleidung: ";
                                }
                                else
                                {
                                    message = "Der Verkäufer trug folgende Kleidung: ";
                                }
                                foreach (uint clothId in dealer.LastSeller.Character.Clothes.Values)
                                {
                                    if (ClothModule.Instance.Get(clothId).Name == "Leer")
                                        continue;
                                    if (Utils.RandomNumber(1, 10) <= 4)
                                    {
                                        message += ClothModule.Instance.Get(clothId).Name + ", ";
                                        found = true;
                                    }
                                }
                                if (found)
                                {
                                    message = message.Substring(0, message.Length - 2);
                                    iPlayer.SendNewNotification(message, duration: 30000);
                                }
                                else
                                {
                                    iPlayer.SendNewNotification("Leider gibt es keine Beschreibung der Personen.");
                                }
                            }
                            dealer.Alert = false;
                            string messageToDB = "FindDealer: " + message;
                            MySQLHandler.ExecuteAsync($"INSERT INTO `log_bucket` (`player_id`, `message`) VALUES ('{iPlayer.Id}', '{messageToDB}');");
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_dealerhint);
                            return;
                        }
                        interval++;
                    }
                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_dealerhint);
                }
                else if (menuid == Dialogs.menu_quitjob)
                {
                    switch (index)
                    {
                        case 0: // job quit

                            iPlayer.SendNewNotification(

                                "Sie haben Ihren Job erfolgreich gekuendigt!");
                            iPlayer.job[0] = 0;
                            iPlayer.jobskill[0] = 0;
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_quitjob);
                            break;
                        default:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_quitjob);
                            break;
                    }
                }
                else if (menuid == Dialogs.menu_jobaccept)
                {
                    switch (index)
                    {
                        case 0: // job annehmen
                            Job xJob;
                            if ((xJob = JobsModule.Instance.GetThisJob(player.Position)) != null)
                            {
                                iPlayer.job[0] = xJob.Id;
                                iPlayer.SetPlayerCurrentJobSkill();
                                iPlayer.SendNewNotification(
                                    "Sie sind nun " + xJob.Name + ", /help fuer weitere Hilfe.", title: "Info", notificationType: PlayerNotification.NotificationType.INFO);
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_jobaccept);
                                break;
                            }

                            break;
                        default:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_jobaccept);
                            break;
                    }
                }
                else if (menuid == Dialogs.menu_mdc)
                {
                    if (!iPlayer.HasData("db")) return;
                    DbPlayer xPlayer = iPlayer.GetData("db");
                    iPlayer.ResetData("db");
                    if (xPlayer == null) return;

                    switch (index)
                    {
                        case 0: // Player Infos
                            iPlayer.SendNewNotification(
                                 "Person " + xPlayer.GetName() + " | Level " +
                                xPlayer.Level);
                            if (xPlayer.wanteds[0] > 0)
                                iPlayer.SendNewNotification(
                                     "Gesucht mit " + xPlayer.wanteds[0]);
                            iPlayer.SendNewNotification(
                                 "Wanteds " + xPlayer.wanteds[0]);
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_mdc);
                            break;
                        case 1: // Car Lic
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_mdc);
                            if (xPlayer.Lic_Car[0] == 1)
                            {
                                iPlayer.SendNewNotification(MSG.MDC_LicCheck(true));
                            }
                            else
                            {
                                iPlayer.SendNewNotification(MSG.MDC_LicCheck(false));
                            }

                            break;
                        case 2: // LKW Lic
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_mdc);
                            if (xPlayer.Lic_LKW[0] == 1)
                            {
                                iPlayer.SendNewNotification(MSG.MDC_LicCheck(true));
                            }
                            else
                            {
                                iPlayer.SendNewNotification(MSG.MDC_LicCheck(false));
                            }

                            break;
                        case 3: // Bike Lic
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_mdc);
                            if (xPlayer.Lic_Bike[0] == 1)
                            {
                                iPlayer.SendNewNotification(MSG.MDC_LicCheck(true));
                            }
                            else
                            {
                                iPlayer.SendNewNotification(MSG.MDC_LicCheck(false));
                            }

                            break;
                        case 4: // FlyA Lic
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_mdc);
                            if (xPlayer.Lic_PlaneA[0] == 1)
                            {
                                iPlayer.SendNewNotification(MSG.MDC_LicCheck(true));
                            }
                            else
                            {
                                iPlayer.SendNewNotification(MSG.MDC_LicCheck(false));
                            }

                            break;
                        case 5: // FlyB Lic
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_mdc);
                            if (xPlayer.Lic_PlaneB[0] == 1)
                            {
                                iPlayer.SendNewNotification(MSG.MDC_LicCheck(true));
                            }
                            else
                            {
                                iPlayer.SendNewNotification(MSG.MDC_LicCheck(false));
                            }

                            break;
                        case 6: // Boot Lic
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_mdc);
                            if (xPlayer.Lic_Boot[0] == 1)
                            {
                                iPlayer.SendNewNotification(MSG.MDC_LicCheck(true));
                            }
                            else
                            {
                                iPlayer.SendNewNotification(MSG.MDC_LicCheck(false));
                            }

                            break;
                        case 7: // Gun Lic
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_mdc);
                            if (xPlayer.Lic_Gun[0] == 1)
                            {
                                iPlayer.SendNewNotification(MSG.MDC_LicCheck(true));
                            }
                            else
                            {
                                iPlayer.SendNewNotification(MSG.MDC_LicCheck(false));
                            }

                            break;
                        case 8: // Biz Lic
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_mdc);
                            if (xPlayer.Lic_Biz[0] == 1)
                            {
                                iPlayer.SendNewNotification(MSG.MDC_LicCheck(true));
                            }
                            else
                            {
                                iPlayer.SendNewNotification(MSG.MDC_LicCheck(false));
                            }

                            break;
                        case 9: // Transfer
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_mdc);
                            if (xPlayer.Lic_Transfer[0] == 1)
                            {
                                iPlayer.SendNewNotification(MSG.MDC_LicCheck(true));
                            }
                            else
                            {
                                iPlayer.SendNewNotification(MSG.MDC_LicCheck(false));
                            }

                            break;
                        default:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_mdc);
                            break;
                    }
                }
                else if (menuid == Dialogs.menu_help)
                {
                    switch (index)
                    {
                        case 0: // Allgemeine Hilfe
                            iPlayer.SendNewNotification(

                                "Allgemeine Befehle: /fishing (LS-Pier) /s (schreien)", title: "Info", notificationType: PlayerNotification.NotificationType.INFO);
                            iPlayer.SendNewNotification(

                                "Allgemeine Befehle: /support /tognews /service", title: "Info", notificationType: PlayerNotification.NotificationType.INFO);
                            iPlayer.SendNewNotification(

                                "Allgemeine Befehle: /cc (Fahrzeugchat) /me /ooc (Out of Character)", title: "Info", notificationType: PlayerNotification.NotificationType.INFO);
                            iPlayer.SendNewNotification(
                                 "Allgemeine Befehle: /dropguns /spende", title: "Info", notificationType: PlayerNotification.NotificationType.INFO);
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_help);
                            break;
                        case 1: // Fraktions Hilfe

                            if (iPlayer.TeamId == (int)teams.TEAM_CIVILIAN)
                            {
                                iPlayer.SendNewNotification( MSG.Error.NoTeam());
                                DialogMigrator.CloseUserMenu(player, Dialogs.menu_help);
                                break;
                            }

                            switch (iPlayer.TeamId)
                            {
                                case (int)teams.TEAM_POLICE:
                                    iPlayer.SendNewNotification(
                                         MSG.HelpPolice());
                                    iPlayer.SendNewNotification(
                                         MSG.HelpPolice2());
                                    iPlayer.SendNewNotification(
                                         MSG.HelpPolice3());
                                    iPlayer.SendNewNotification(
                                         MSG.HelpPolice4());
                                    if (iPlayer.TeamRankPermission.Manage >= 1)
                                        iPlayer.SendNewNotification(
                                             MSG.HelpLeader());
                                    break;
                                case (int)teams.TEAM_ARMY:
                                    iPlayer.SendNewNotification(
                                         MSG.HelpPolice());
                                    iPlayer.SendNewNotification(
                                         MSG.HelpPolice2());
                                    iPlayer.SendNewNotification(
                                         MSG.HelpPolice3());
                                    iPlayer.SendNewNotification(
                                         MSG.HelpPolice4());
                                    iPlayer.SendNewNotification(MSG.HelpArmy());
                                    if (iPlayer.TeamRankPermission.Manage >= 1)
                                        iPlayer.SendNewNotification(
                                             MSG.HelpLeader());
                                    break;
                                case (int)teams.TEAM_DPOS:
                                    iPlayer.SendNewNotification(

                                        "/vehpark /m(egaphone) /r(adio) /d(epartment)");
                                    if (iPlayer.TeamRankPermission.Manage >= 1)
                                        iPlayer.SendNewNotification(
                                             MSG.HelpLeader());
                                    break;
                                case (int)teams.TEAM_GOV:
                                    iPlayer.SendNewNotification("/gov");
                                    if (iPlayer.TeamRankPermission.Manage >= 1)
                                        iPlayer.SendNewNotification(
                                             MSG.HelpLeader());
                                    break;
                                case (int)teams.TEAM_FIB:
                                    iPlayer.SendNewNotification(
                                         MSG.HelpPolice());
                                    iPlayer.SendNewNotification(
                                         MSG.HelpPolice2());
                                    iPlayer.SendNewNotification(
                                         MSG.HelpPolice3());
                                    iPlayer.SendNewNotification(
                                         MSG.HelpPolice4());
                                    iPlayer.SendNewNotification(

                                        "FIB Befehle: /find (Person) /undercover (Am Equip)");
                                    if (iPlayer.TeamRankPermission.Manage >= 1)
                                        iPlayer.SendNewNotification(
                                             MSG.HelpLeader());
                                    break;
                                case (int)teams.TEAM_BALLAS:
                                    iPlayer.SendNewNotification(MSG.HelpGang());
                                    if (iPlayer.TeamRankPermission.Manage >= 1)
                                        iPlayer.SendNewNotification(
                                             MSG.HelpLeader());
                                    break;
                                case (int)teams.TEAM_AOD:
                                    iPlayer.SendNewNotification(MSG.HelpGang());
                                    if (iPlayer.TeamRankPermission.Manage >= 1)
                                        iPlayer.SendNewNotification(
                                             MSG.HelpLeader());
                                    break;
                                case (int)teams.TEAM_LOST:
                                    iPlayer.SendNewNotification(MSG.HelpGang());
                                    if (iPlayer.TeamRankPermission.Manage >= 1)
                                        iPlayer.SendNewNotification(
                                             MSG.HelpLeader());
                                    break;
                                case (int)teams.TEAM_NEWS:
                                    iPlayer.SendNewNotification(MSG.HelpNews());
                                    if (iPlayer.TeamRankPermission.Manage >= 1)
                                        iPlayer.SendNewNotification(
                                             MSG.HelpLeader());
                                    break;
                                case (int)teams.TEAM_DRIVINGSCHOOL:
                                    iPlayer.SendNewNotification(
                                         MSG.HelpDrivingSchool());
                                    if (iPlayer.TeamRankPermission.Manage >= 1)
                                        iPlayer.SendNewNotification(
                                             MSG.HelpLeader());
                                    break;
                                case (int)teams.TEAM_MEDIC:
                                    iPlayer.SendNewNotification(MSG.HelpMedic());
                                    iPlayer.SendNewNotification(
                                         MSG.HelpMedic2());
                                    if (iPlayer.TeamRankPermission.Manage >= 1)
                                        iPlayer.SendNewNotification(
                                             MSG.HelpLeader());
                                    break;
                                case (int)teams.TEAM_VAGOS:
                                    iPlayer.SendNewNotification(MSG.HelpGang());
                                    if (iPlayer.TeamRankPermission.Manage >= 1)
                                        iPlayer.SendNewNotification(
                                             MSG.HelpLeader());
                                    break;
                                case (int)teams.TEAM_LCN:
                                    iPlayer.SendNewNotification(MSG.HelpGang());
                                    if (iPlayer.TeamRankPermission.Manage >= 1)
                                        iPlayer.SendNewNotification(
                                             MSG.HelpLeader());
                                    break;
                                case (int)teams.TEAM_YAKUZA:
                                    iPlayer.SendNewNotification(MSG.HelpGang());
                                    if (iPlayer.TeamRankPermission.Manage >= 1)
                                        iPlayer.SendNewNotification(
                                             MSG.HelpLeader());
                                    break;
                                case (int)teams.TEAM_HUSTLER:
                                    iPlayer.SendNewNotification(MSG.HelpGang());
                                    if (iPlayer.TeamRankPermission.Manage >= 1)
                                        iPlayer.SendNewNotification(
                                             MSG.HelpLeader());
                                    break;
                                case (int)teams.TEAM_BRATWA:
                                    iPlayer.SendNewNotification(MSG.HelpGang());
                                    if (iPlayer.TeamRankPermission.Manage >= 1)
                                        iPlayer.SendNewNotification(
                                             MSG.HelpLeader());
                                    break;
                                case (int)teams.TEAM_NNM:
                                    iPlayer.Player.SendNotification(MSG.HelpGang());
                                    if (iPlayer.TeamRankPermission.Manage >= 1)
                                        iPlayer.Player.SendNotification(
                                             MSG.HelpLeader());
                                    break;
                                case (int)teams.TEAM_GROVE:
                                    iPlayer.SendNewNotification(MSG.HelpGang());
                                    if (iPlayer.TeamRankPermission.Manage >= 1)
                                        iPlayer.SendNewNotification(
                                             MSG.HelpLeader());
                                    break;
                                case (int)teams.TEAM_ICA:
                                    iPlayer.SendNewNotification(MSG.HelpGang());
                                    if (iPlayer.TeamRankPermission.Manage >= 1)
                                        iPlayer.SendNewNotification(
                                             MSG.HelpLeader());
                                    break;
                                default:
                                    iPlayer.SendNewNotification(
                                         MSG.Error.NoTeam());
                                    break;
                            }

                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_help);
                            break;
                        case 2: // Job Hilfe
                            if (iPlayer.job[0] > 0)
                            {
                                Job xJob;
                                xJob = JobsModule.Instance.GetJob(iPlayer.job[0]);
                                if (xJob == null) return;
                                iPlayer.SendNewNotification(
                                     xJob.Name + " Befehle: " + xJob.Helps);
                            }
                            else
                            {
                                iPlayer.SendNewNotification( MSG.Error.NoJob());
                            }

                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_help);
                            break;
                        case 3: // Haus Hilfe
                            iPlayer.SendNewNotification(

                                "Haus Befehle: /house (Infos zum Haus | Hausverwaltung) /buyhouse /rent /unrent");
                            iPlayer.SendNewNotification(

                                "Haus Befehle: /buyinterior (Interior kaufen)");
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_help);
                            break;
                        case 4: // Fahrzeug Hilfe
                            iPlayer.SendNewNotification(
                                 "Fahrzeug Befehle: /eject [Name]");
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_help);
                            break;
                        case 5: // Inventar Hilfe
                            iPlayer.SendNewNotification(

                                "Inventar Befehle: /giveitem [Name] [ItemData] [Amount] /packgun");
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_help);
                            break;
                        case 6:
                            /*// Biz Hilfe
                            iPlayer.SendNewNotification(

                                MSG.HelpBusiness());
                                */
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_help);
                            break;
                        case 7: // Admin Hilfe
                            if (iPlayer.RankId > 0)
                            {
                                var commands = "";
                                int curr = 0;
                                foreach (var command in iPlayer.Rank.Commands)
                                {
                                    commands += " /" + command + " ";
                                    if (curr >= 5)
                                    {
                                        iPlayer.SendNewNotification(commands);
                                        commands = "";
                                        curr = 0;
                                    }

                                    curr++;
                                }

                                if (!string.IsNullOrEmpty(commands))
                                {
                                    iPlayer.SendNewNotification(commands);
                                }
                            }

                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_help);
                            break;
                        default:
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_help);
                            break;
                    }
                }
                else if (menuid == Dialogs.menu_shop_interior)
                {
                    switch (index)
                    {
                        case 0: //Schließen
                            House xHouse = HouseModule.Instance[iPlayer.ownHouse[0]];
                            if (xHouse == null) return;
                            player.SetPosition(xHouse.Position);
                            player.Dimension = 0;
                            iPlayer.DimensionType[0] = DimensionType.World;
                            iPlayer.ResetData("tempInt");
                            iPlayer.ResetData("lastPosition");
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_interior);
                            break;

                        case 1: //Kaufen
                            if (iPlayer.HasData("tempInt"))
                            {
                                uint i = iPlayer.GetData("tempInt");
                                if (i > 0)
                                {
                                    var interior = InteriorModule.Instance[i];
                                    if (interior == null) return;
                                    int price = interior.Price;
                                    if (!iPlayer.TakeMoney(price))
                                    {
                                        iPlayer.SendNewNotification(
                                            
                                            MSG.Money.NotEnoughMoney(price));
                                        return;
                                    }

                                    iPlayer.SendNewNotification(
                                         "Interior gekauft $" + price);
                                    House mHouse = HouseModule.Instance[iPlayer.ownHouse[0]];

                                    mHouse.Interior = interior;
                                    player.SetPosition(mHouse.Position);
                                    player.Dimension = 0;
                                    iPlayer.DimensionType[0] = DimensionType.World;
                                    iPlayer.ResetData("tempInt");
                                    iPlayer.ResetData("lastPosition");
                                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_shop_interior);
                                    mHouse.SaveInterior();
                                }

                                return;
                            }

                            break;
                        default:
                            int idx = 0;
                            House iHouse;
                            if ((iHouse = HouseModule.Instance[iPlayer.ownHouse[0]]) != null)
                            {
                                foreach (KeyValuePair<uint, Interior> kvp in InteriorModule.Instance.GetAll())
                                {
                                    if (kvp.Value.Type == iHouse.Type)
                                    {
                                        if (index - 2 == idx)
                                        {
                                            iPlayer.SetData("lastPosition", iPlayer.Player.Position);
                                            player.SetPosition(kvp.Value.Position);
                                            iPlayer.DimensionType[0] = DimensionType.House_Shop_Interior;
                                            iPlayer.SetData("tempInt", kvp.Key);
                                            break;
                                        }
                                        else
                                        {
                                            idx++;
                                        }
                                    }
                                }
                            }
                            break;
                    }
                }
                else if (menuid == Dialogs.menu_carshop)
                {
                    VehicleShop cShop = VehicleShopModule.Instance.GetThisShop(player.Position);
                    if (cShop == null)
                    {
                        DialogMigrator.CloseUserMenu(player, Dialogs.menu_carshop);
                        return;
                    }
                    if (iPlayer == null || !iPlayer.IsValid()) return;
                    int idx = 0;
                    List<ShopVehicle> VehsOfShop = VehicleShopModule.Instance.GetVehsFromCarShop(cShop.Id);
                    foreach (ShopVehicle Vehicle in VehsOfShop)
                    {
                        if (idx == index)
                        {
                            var price = Vehicle.Price;
                            var discount = 0;

                            
                            
                            if (price < 0) return;

                            if(!Vehicle.CanPurchased())
                            {
                                iPlayer.SendNewNotification("Dieses Fahrzeug ist limitiert und derzeit nicht erhältlich!");
                                return;
                            }

                            if(!cShop.TeamCarShop)
                            {
                                int couponPercent = 0;
                                uint whatCoupon = 0;

                                if(cShop.Id == 1001 && Vehicle.IsSpecialCar)
                                {
                                    if(cShop.PlayerIds.Contains((int)iPlayer.Id))
                                    {
                                        iPlayer.SendNewNotification("Du hast bei mir bereits ein Fahrzeug gekauft.");
                                        return;
                                    }
                                    if(!GuentherModule.DbPlayers.Contains(iPlayer))
                                    {
                                        iPlayer.SendNewNotification("Du bist scheinbar nicht richtig hier reingekommen!");
                                        MySQLHandler.ExecuteAsync($"INSERT INTO `log_guentherclub` (`player_id`, `info`) VALUES ('{iPlayer.Id}', 'Versuchter Kauf: {Vehicle.Name} ({Vehicle.Price} $)');");
                                        return;
                                    }
                                }

                                if (!Vehicle.IsSpecialCar)
                                {
                                    foreach (KeyValuePair<int, Item> kvp in iPlayer.Container.Slots)
                                    {
                                        if (kvp.Value.Model == null) continue;
                                        if (kvp.Value.Model.Script == null) continue;
                                        if (kvp.Value.Model.Script.Contains("discount_car_"))
                                        {
                                            try
                                            {
                                                couponPercent = Int32.Parse(kvp.Value.Model.Script.Replace("discount_car_", ""));
                                                double temp = couponPercent / 100.0d;
                                                price -= (int)(price * temp);
                                                whatCoupon = kvp.Value.Id;
                                            }
                                            catch (Exception)
                                            {
                                                iPlayer.SendNewNotification("Ein Fehler ist aufgetreten...");
                                                return;
                                            }
                                            break;
                                        }
                                    }

                                    if (iPlayer.uni_business[0] > 0 && couponPercent == 0)
                                    {
                                        discount = 100 - (iPlayer.uni_business[0] * 2);
                                        price = Convert.ToInt32((discount / 100.0) * price);
                                    }
                                }
                                if (price > 50000 && iPlayer.IsHomeless())
                                {
                                    iPlayer.SendNewNotification("Ohne einen Wohnsitz können Sie dieses Fahrzeug nicht erwerben!");
                                    return;
                                }
                                if (!iPlayer.TakeMoney(price))
                                {
                                    iPlayer.SendNewNotification(
                                         "Sie haben nicht genuegend Geld!");
                                    DialogMigrator.OpenUserMenu(iPlayer, Dialogs.menu_carshop);
                                    return;
                                }

                                if (whatCoupon != 0)
                                {
                                    iPlayer.SendNewNotification("- " + couponPercent + " % Rabatt", title: "", notificationType: PlayerNotification.NotificationType.SUCCESS);
                                    iPlayer.Container.RemoveItem(whatCoupon);
                                }

                                if (cShop.Id == 1001 && Vehicle.IsSpecialCar)
                                {
                                    cShop.PlayerIds.Add((int)iPlayer.Id);
                                    string PlayerIds = "";
                                    cShop.PlayerIds.ForEach(id => PlayerIds += id + ",");
                                    if (PlayerIds.Length > 0)
                                        PlayerIds = PlayerIds.Substring(0, PlayerIds.Length - 1);
                                    MySQLHandler.ExecuteAsync($"UPDATE `carshop_shops` SET `player_ids` = '{PlayerIds}' WHERE `id` = '{cShop.Id}'");
                                }

                                iPlayer.SendNewNotification("Sie haben einen " + Vehicle.Name +
                                " fuer $" + price +
                                " gekauft!");
                                
                            }
                            else
                            {
                                if (iPlayer.TeamId == 0 || !cShop.RestrictedTeams.Contains((int)iPlayer.TeamId))
                                {
                                    iPlayer.SendNewNotification("Dieser Shop ist nicht fuer Ihr Team geeignet!");
                                    DialogMigrator.OpenUserMenu(iPlayer, Dialogs.menu_carshop);
                                    return;
                                }

                                if (!Vehicle.IsSpecialCar)
                                {
                                    // Gangwar Discount
                                    discount = 100 - (GangwarTownModule.Instance.GetCarShopDiscount(iPlayer.Team));
                                    price = Convert.ToInt32((discount / 100.0) * price);
                                }

                                // Check FBank & Rights
                                if (iPlayer.TeamRankPermission == null || iPlayer.TeamRankPermission.Manage == 0)
                                {
                                    iPlayer.SendNewNotification("Keine Berechtigung!");
                                    DialogMigrator.OpenUserMenu(iPlayer, Dialogs.menu_carshop);
                                    return;
                                }

                                var teamShelter = TeamShelterModule.Instance.GetAll().FirstOrDefault(s => s.Value.Team.Id == iPlayer.Team.Id).Value;
                                if (teamShelter == null || teamShelter.Team == null || iPlayer.TeamId != teamShelter.Team.Id) return;
                                if (teamShelter.Money < price)
                                {
                                    iPlayer.SendNewNotification($"Kosten {price}$ nicht in der FBank vorhanden!");
                                    DialogMigrator.OpenUserMenu(iPlayer, Dialogs.menu_carshop);
                                    return;
                                }

                                teamShelter.TakeMoney(price);

                            }
                            if (discount > 0)
                            {
                                iPlayer.SendNewNotification(
                                    "Sie haben fuer dieses Fahrzeug nur " + discount +
                                    "% des Kaufpreises bezahlt!");
                            }

                            // Team Autohaus
                            if (cShop.TeamCarShop)
                            {
                                string query = String.Format(
                                    "INSERT INTO `fvehicles` (`vehiclehash`, `team`, `color1`, `color2`, `inGarage`, `model`, `fuel`) VALUES ('{0}', '{1}', '{2}', '{3}', '1', '{4}', '100');",
                                    Vehicle.Data.modded_car == 1 ? Vehicle.Data.mod_car_name : Vehicle.Data.Model, iPlayer.TeamId, iPlayer.Team.ColorId, iPlayer.Team.ColorId, 
                                    Vehicle.Data.Id);
                                MySQLHandler.Execute(query);
                                Logger.AddVehiclePurchaseLog(iPlayer.Id, cShop.Id, Vehicle.Data.Id, price, iPlayer.TeamId);


                                query = string.Format(
                                    "SELECT * FROM `fvehicles` WHERE `team` = '{0}' AND `model` LIKE '{1}' ORDER BY id DESC LIMIT 1;",
                                    iPlayer.Team.Id,
                                    Vehicle.Data.Id);

                                uint id = 0;

                                using (var conn =
                                    new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                                using (var cmd = conn.CreateCommand())
                                {
                                    conn.Open();
                                    cmd.CommandText = @query;
                                    using (var reader = cmd.ExecuteReader())
                                    {
                                        if (reader.HasRows)
                                        {
                                            while (reader.Read())
                                            {
                                                id = reader.GetUInt32("id");
                                                break;
                                            }
                                        }
                                    }
                                    conn.Close();
                                }

                                iPlayer.SendNewNotification(
                                    $"Sie haben {Vehicle.Data.Model} fuer Ihre Fraktion gekauft!");
                                
                                SxVehicle sxVehicle = new SxVehicle()
                                {
                                    Data = Vehicle.Data,
                                    databaseId = 0
                                };
                                RegistrationOfficeFunctions.GiveVehicleContract(iPlayer, sxVehicle, "Fahrzeugshop " + cShop.Description);
                            }
                            else
                            {
                                string x = cShop.SpawnPosition.X.ToString().Replace(",", ".");
                                string y = cShop.SpawnPosition.Y.ToString().Replace(",", ".");
                                string z = cShop.SpawnPosition.Z.ToString().Replace(",", ".");
                                string heading2 = cShop.SpawnHeading.ToString().Replace(",", ".");

                                var crumbs = iPlayer.GetName().Split('_');

                                var firstLetter = "";
                                var secondLetter = "";

                                if (crumbs.Length == 2)
                                {
                                    firstLetter = crumbs[0][0].ToString();
                                    secondLetter = crumbs[1][0].ToString();
                                }

                                int defaultgarage = 1;
                                if (Vehicle.Data.ClassificationId == (int)VehicleClassificationTypes.Boot) defaultgarage = 34;
                                else if (Vehicle.Data.ClassificationId == (int)VehicleClassificationTypes.Flugzeug) defaultgarage = 35;
                                else if (Vehicle.Data.ClassificationId == (int)VehicleClassificationTypes.Helikopter) defaultgarage = 466;

                                var registered = 0;
                                bool GpsSender = false;
                                var plate = "";

                                //register veh if noob shoh
                                //Noobshop, damit die Noobs direkt einen GPS Sender eingebaut haben
                                if (cShop.Id == 12)
                                {
                                    GpsSender = true;
                                    registered = 1;
                                    plate = RegistrationOfficeFunctions.GetRandomPlate(true);
                                    
                                }
                                
                                string query = String.Format(
                                        "INSERT INTO `vehicles` (`owner`, `pos_x`, `pos_y`, `pos_z`, `heading`, `color1`, `color2`, `plate`, `model`, `vehiclehash`, `garage_id`,`gps_tracker`,`registered`) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}');",
                                        iPlayer.Id, x, y, z, heading2,
                                        Vehicle.PrimaryColor,
                                        Vehicle.SecondaryColor,
                                        plate, Vehicle.Data.Id, Vehicle.Data.modded_car == 1 ? Vehicle.Data.mod_car_name : Vehicle.Data.Model, defaultgarage, GpsSender ? 1 : 0, registered);
                                    MySQLHandler.Execute(query);
                                    Logger.AddVehiclePurchaseLog(iPlayer.Id, cShop.Id, Vehicle.Data.Id, price, 0);




                                query = string.Format(
                                    "SELECT * FROM `vehicles` WHERE `owner` = '{0}' AND `model` LIKE '{1}' ORDER BY id DESC LIMIT 1;",
                                    iPlayer.Id,
                                    Vehicle.Data.Id);

                                // Update Special Cars Stuff
                                if (Vehicle.IsSpecialCar) Vehicle.IncreaseLimitedAmount();


                                uint id = 0;

                                using (var conn =
                                    new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                                using (var cmd = conn.CreateCommand())
                                {
                                    conn.Open();
                                    cmd.CommandText = @query;
                                    using (var reader = cmd.ExecuteReader())
                                    {
                                        if (reader.HasRows)
                                        {
                                            while (reader.Read())
                                            {
                                                id = reader.GetUInt32(0);
                                                break;
                                            }
                                        }
                                    }
                                    conn.Close();
                                }

                                SxVehicle sxVehicle = VehicleHandler.Instance.CreateServerVehicle(Vehicle.Data.Id, false,
                                    cShop.SpawnPosition, cShop.SpawnHeading, Vehicle.PrimaryColor,
                                    Vehicle.SecondaryColor, 0, GpsSender, true,
                                    false, 0,
                                    iPlayer.GetName(), id, 0, iPlayer.Id, 100, VehicleHandler.MaxVehicleHealth, 
                                    "", "", 0, ContainerManager.LoadContainer(id, ContainerTypes.VEHICLE, Vehicle.Data.InventorySize, Vehicle.Data.InventoryWeight), plate);
                                iPlayer.OwnVehicles.Add(id, Vehicle.Data.modded_car == 1 ? Vehicle.Data.mod_car_name : Vehicle.Data.Model);
                                    if (cShop.Id != 12)
                                    {
                                        RegistrationOfficeFunctions.GiveVehicleContract(iPlayer, sxVehicle, "Fahrzeugshop " + cShop.Description);
                                    }
                                    else
                                    {
                                        RegistrationOfficeFunctions.UpdateVehicleRegistrationToDb(sxVehicle, iPlayer, iPlayer, plate, true);
                                    }
                            }
                            DialogMigrator.CloseUserMenu(player, Dialogs.menu_carshop);
                            iPlayer.Save();
                            return;
                        }

                        idx++;
                    }

                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_carshop);
                }
                else if (menuid == Dialogs.menu_jailinhabits)
                {
                    DialogMigrator.CloseUserMenu(player, Dialogs.menu_jailinhabits);
                }
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        private static void CreateUserMenuFahrzeugGarage(Client player, DbPlayer iPlayer, Garage garage)
        {
            DialogMigrator.CreateMenu(player, Dialogs.menu_garage_overlay, "Fahrzeug-Garage", "");
            DialogMigrator.AddMenuItem(player, Dialogs.menu_garage_overlay, MSG.General.Close(), "");
            DialogMigrator.AddMenuItem(player, Dialogs.menu_garage_overlay, "Fahrzeug entnehmen", "");
            DialogMigrator.AddMenuItem(player, Dialogs.menu_garage_overlay, "Fahrzeug einlagern", "");
         
            DialogMigrator.OpenUserMenu(iPlayer, Dialogs.menu_garage_overlay);
            iPlayer.SetData("GarageId", garage.Id);
            return;
        }
    }
}
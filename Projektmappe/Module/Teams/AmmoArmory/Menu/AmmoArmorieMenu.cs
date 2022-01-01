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
using GVRP.Module.Teams.AmmoPackageOrder;
using GVRP.Module.Teams.Shelter;

namespace GVRP.Module.Teams.AmmoArmory
{
    public class AmmoArmorieMenuBuilder : MenuBuilder
    {
        public AmmoArmorieMenuBuilder() : base(PlayerMenu.AmmoArmorieMenu)
        {
        }

        public override Menu.Menu Build(DbPlayer iPlayer)
        {
            AmmoArmorie ammoArmorie = AmmoArmoryModule.Instance.GetByPosition(iPlayer.Player.Position);
            if (ammoArmorie == null || !iPlayer.Team.IsGangsters() || iPlayer.Team.Id != ammoArmorie.TeamId) return null;

            var menu = new Menu.Menu(Menu, "Munitionskammer (" + ammoArmorie.Packets + "P)");

            menu.Add($"Schließen");

            menu.Add("Pakete abgeben");
            menu.Add("Preise einstellen");

            foreach (AmmoArmorieItem ammoArmorieItem in ammoArmorie.ArmorieItems)
            {
                menu.Add(ItemModelModule.Instance.Get(ammoArmorieItem.ItemId).Name + " $" + ammoArmorieItem.TeamPrice, "(P:" + ammoArmorieItem.GetRequiredPacketsForTeam(iPlayer.Team) + ")");
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
                if (index == 0)
                {
                    MenuManager.DismissCurrent(iPlayer);
                    return false;
                }
                else if (index == 1) // Packete abgeben
                {
                    AmmoArmorie ammoArmorie = AmmoArmoryModule.Instance.GetByPosition(iPlayer.Player.Position);
                    if (ammoArmorie == null || !iPlayer.Team.IsGangsters() || iPlayer.Team.Id != ammoArmorie.TeamId) return false;

                    int hasChests = iPlayer.Container.GetItemAmount(AmmoArmoryModule.AmmoChestItem);
                    if (hasChests > 0)
                    {
                        if(ammoArmorie.Packets+(hasChests * AmmoPackageOrderModule.AmmoChestToPackageMultipliert) >= AmmoArmoryModule.MaxLagerBestand)
                        {
                            iPlayer.SendNewNotification("Maximale Anzahl erreicht! (" + AmmoArmoryModule.MaxLagerBestand + ")");
                            return false;
                        }
                        else
                        {
                            ammoArmorie.ChangePackets(hasChests * AmmoPackageOrderModule.AmmoChestToPackageMultipliert);
                            iPlayer.Container.RemoveItem(AmmoArmoryModule.AmmoChestItem, hasChests);
                            iPlayer.SendNewNotification($"Sie haben {hasChests} Kisten ({hasChests* AmmoPackageOrderModule.AmmoChestToPackageMultipliert} Pakete) eingelagert!");
                            return true;
                        }
                    }

                    MenuManager.DismissCurrent(iPlayer);
                    return false;
                }
                else if (index == 2) // Preis einstellen
                {
                    AmmoArmorie ammoArmorie = AmmoArmoryModule.Instance.GetByPosition(iPlayer.Player.Position);
                    if (ammoArmorie == null || !iPlayer.Team.IsGangsters() || iPlayer.Team.Id != ammoArmorie.TeamId) return false;

                    if (iPlayer.TeamRank < 11) return false;
                    
                    MenuManager.Instance.Build(PlayerMenu.AmmoArmoriePriceMenu, iPlayer).Show(iPlayer);
                    return false;
                }
                else // choose x.x
                {
                    AmmoArmorie ammoArmorie = AmmoArmoryModule.Instance.GetByPosition(iPlayer.Player.Position);
                    if (ammoArmorie == null || !iPlayer.Team.IsGangsters() || iPlayer.Team.Id != ammoArmorie.TeamId) return false;

                    int idx = 3;
                    foreach (AmmoArmorieItem ammoArmorieItem in ammoArmorie.ArmorieItems)
                    {
                        if (idx == index)
                        {
                            int RequiredPackets = ammoArmorieItem.GetRequiredPacketsForTeam(iPlayer.Team);

                            if(ammoArmorie.Packets < RequiredPackets)
                            {
                                iPlayer.SendNewNotification("Nicht genug Waffenpakete!");
                                return false;
                            }

                            ItemModel resultItem = ItemModelModule.Instance.Get(ammoArmorieItem.ItemId);
                            if (resultItem == null) return false;

                            if(iPlayer.Container.CanInventoryItemAdded(resultItem))
                            {
                                if (ammoArmorieItem.TeamPrice > 0)
                                {
                                    if (!iPlayer.TakeBankMoney(ammoArmorieItem.TeamPrice))
                                    {
                                        iPlayer.SendNewNotification("Nicht genug Geld (Bank)!");
                                        return false;
                                    }

                                    TeamShelter teamShelter = TeamShelterModule.Instance.GetByTeam(iPlayer.Team.Id);
                                    if (teamShelter == null) return false;

                                    teamShelter.GiveMoney(ammoArmorieItem.TeamPrice);
                                }

                                ammoArmorie.ChangePackets(-RequiredPackets);
                                iPlayer.Container.AddItem(resultItem);
                                iPlayer.SendNewNotification($"Sie haben {resultItem.Name} für ${ammoArmorieItem.TeamPrice} (P: {RequiredPackets}) entnommen!");
                                
                                return false;
                            }
                            else
                            {
                                iPlayer.SendNewNotification("Zu wenig Platz!");
                                return false;
                            }
                        }
                        else idx++;
                    }
                    return true;
                }
            }
        }
    }
}
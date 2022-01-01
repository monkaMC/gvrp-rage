using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using GVRP.Module.Armory;
using GVRP.Module.GTAN;
using GVRP.Module.Items;
using GVRP.Module.Logging;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Staatskasse;
using GVRP.Module.Weapons.Component;
using GVRP.Module.Weapons.Data;

namespace GVRP
{
    public class ArmoryWeaponMenuBuilder : MenuBuilder
    {
        public ArmoryWeaponMenuBuilder() : base(PlayerMenu.ArmoryWeapons)
        {
        }

        public override Menu Build(DbPlayer iPlayer)
        {
            var menu = new Menu(Menu, "Armory Waffen");

            menu.Add(MSG.General.Close(), "");
            
            menu.Add("Zurueck", "");
            
            if (!iPlayer.HasData("ArmoryId")) return menu;
            var ArmoryId = iPlayer.GetData("ArmoryId");
            Armory Armory = ArmoryModule.Instance.Get(ArmoryId);
            if (Armory == null) return menu;
            
            foreach (var ArmoryWeapon in Armory.ArmoryWeapons)
            {
                menu.Add("R: " + ArmoryWeapon.GetDefconRequiredRang() + " " + ArmoryWeapon.WeaponName + (ArmoryWeapon.Price > 0 ? (" ($" + ArmoryWeapon.Price + ") ") : ""));
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
                if (!iPlayer.HasData("ArmoryId")) return false;
                var ArmoryId = iPlayer.GetData("ArmoryId");
                Armory Armory = ArmoryModule.Instance.Get(ArmoryId);
                if (Armory == null) return false;

                if (index == 0)
                {
                    MenuManager.DismissMenu(iPlayer.Player, (int)PlayerMenu.ArmoryWeapons);
                    return false;
                }
                else if (index == 1)
                {
                    MenuManager.DismissMenu(iPlayer.Player, (int)PlayerMenu.Armory);
                    return false;
                }
                else {
                    int actualIndex = 0;
                    foreach (ArmoryWeapon ArmoryWeapon in Armory.ArmoryWeapons)
                    {
                        if (actualIndex == index - 2)
                        {
                            // Rang check
                            if (iPlayer.TeamRank < ArmoryWeapon.GetDefconRequiredRang())
                            {
                                iPlayer.SendNewNotification(
                                    "Sie haben nicht den benötigten Rang fuer diese Waffe!");
                                return false;
                            }

                            if (!iPlayer.IsInDuty())
                            {
                                iPlayer.SendNewNotification(
                                    "Sie muessen dafuer im Dienst sein!");
                                return false;
                            }

                            // Check Armor
                            if (Armory.GetPackets() < ArmoryWeapon.Packets)
                            {
                                iPlayer.SendNewNotification(
                                    $"Die Waffenkammer hat nicht mehr genuegend Waffenkisten! (Benötigt: {ArmoryWeapon.Packets} )");
                                return false;
                            }                                                                                  

                            if (ArmoryWeapon.Price > 0 && !iPlayer.TakeBankMoney(ArmoryWeapon.Price))
                            {
                                iPlayer.SendNewNotification(
                                    $"Diese Waffe kostet {ArmoryWeapon.Price}$ (Bank)!");
                                return false;
                            }

                            // Beamter Remove...
                            if(iPlayer.IsACop())
                            {
                                WeaponData weaponData = WeaponDataModule.Instance.GetAll().Values.Where(wd => (WeaponHash)wd.Hash == ArmoryWeapon.Weapon).FirstOrDefault();

                                if (weaponData != null)
                                {
                                    // Spieler besitzt aktuelle Waffe
                                    if(iPlayer.Weapons.Where(w => w.WeaponDataId == weaponData.Id).Count() > 0)
                                    {
                                        iPlayer.RemoveWeapon(ArmoryWeapon.Weapon);
                                        iPlayer.GiveBankMoney(ArmoryWeapon.Price);
                                        iPlayer.SendNewNotification($"${ArmoryWeapon.Price} als Rückzahlung für {ArmoryWeapon.WeaponName} erhalten!");
                                    }
                                }
                            }

                            // Find Item..
                            var weapon = ItemModelModule.Instance.GetByScript($"bw_{ArmoryWeapon.Weapon.ToString()}");
                            if (weapon == null) return false;

                            if (!iPlayer.Container.CanInventoryItemAdded(weapon))
                            {
                                iPlayer.SendNewNotification("Sie haben nicht genug Platz!");
                                return false;
                            }
                            iPlayer.Container.AddItem(weapon);

                            if (ArmoryWeapon.Price > 0)
                            {
                                iPlayer.SendNewNotification($"{ArmoryWeapon.WeaponName} für ${ArmoryWeapon.Price} entnommen!");
                                KassenModule.Instance.ChangeMoney(KassenModule.Kasse.STAATSKASSE, -ArmoryWeapon.Price);
                            }


                            Armory.RemovePackets(ArmoryWeapon.Packets);
                            return false;
                        }

                        actualIndex++;
                    }
                }

                return false;
            }
        }
    }
}

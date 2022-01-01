using GTANetworkAPI;
using System;
using System.Linq;
using GVRP.Module.Armory;
using GVRP.Module.GTAN;
using GVRP.Module.Logging;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Staatskasse;
using GVRP.Module.Weapons.Component;
using GVRP.Module.Weapons.Data;

namespace GVRP
{
    public class ArmoryArmorMenuBuilder : MenuBuilder
    {
        public ArmoryArmorMenuBuilder() : base(PlayerMenu.ArmoryArmorMenu)
        {
        }

        public override Menu Build(DbPlayer iPlayer)
        {
            var menu = new Menu(Menu, "Armory Rüstungen");

            menu.Add(MSG.General.Close(), "");

            if (!iPlayer.HasData("ArmoryId")) return menu;
            var ArmoryId = iPlayer.GetData("ArmoryId");
            Armory Armory = ArmoryModule.Instance.Get(ArmoryId);
            if (Armory == null) return menu;

            foreach (var ArmoryWeapon in Armory.ArmoryArmors)
            {
                menu.Add("R: " + ArmoryWeapon.RestrictedRang + " " + ArmoryWeapon.Name);
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
                else
                {
                    int actualIndex = 0;
                    foreach (ArmoryArmor armoryArmor in Armory.ArmoryArmors)
                    {
                        if (actualIndex == index - 1)
                        {
                            // Rang check
                            if (iPlayer.TeamRank < armoryArmor.RestrictedRang)
                            {
                                iPlayer.SendNewNotification(
                                    "Sie haben nicht den benötigten Rang für diese Schutzweste!");
                                return false;
                            }

                            if (!iPlayer.IsInDuty())
                            {
                                iPlayer.SendNewNotification(
                                    "Sie müssen dafür im Dienst sein!");
                                return false;
                            }

                            if (armoryArmor.VisibleArmorType != -1)
                            {
                                iPlayer.VisibleArmorType = (uint)armoryArmor.VisibleArmorType;
                                iPlayer.SaveArmorType((uint)armoryArmor.VisibleArmorType);
                                iPlayer.SetArmor(armoryArmor.ArmorValue, true);
                            }
                            else
                            {
                                iPlayer.SetArmor(armoryArmor.ArmorValue, false);
                            }
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

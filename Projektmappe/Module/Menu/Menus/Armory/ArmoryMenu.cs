using GTANetworkAPI;
using System.Linq;
using GVRP.Module.Armory;
using GVRP.Module.GTAN;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Swat;
using GVRP.Module.Weapons;
using GVRP.Module.Weapons.Data;

namespace GVRP
{
    public class ArmoryMenuBuilder : MenuBuilder
    {
        public ArmoryMenuBuilder() : base(PlayerMenu.Armory)
        {
        }

        public override Menu Build(DbPlayer iPlayer)
        {
            var menu = new Menu(Menu, $"Armory");

            menu.Add(MSG.General.Close(), "");
            
            if (!iPlayer.HasData("ArmoryId")) return menu;
            var ArmoryId = iPlayer.GetData("ArmoryId");
            Armory Armory = ArmoryModule.Instance.Get(ArmoryId);
            if (Armory == null) return menu;
            
            menu.Description = $"{Armory.Packets} Pakete";

            menu.Add("Dienst verlassen", "Waffen, Munition und Schutzweste wird entfernt");
            menu.Add("Dienst betreten", "Sie registrieren sich fuer den Dienst");

            if (Armory.ArmoryItems.Count > 0 || Armory.ArmoryWeapons.Count > 0 || Armory.ArmoryArmors.Count > 0)
            {
                menu.Add("Schutzwesten", "Schutzwestenschrank öffnen");
                menu.Add("Waffen", "Waffenarsenal öffnen");
                menu.Add("Items", "Items öffnen");
                menu.Add("Munition", "Munitionsauswahl öffnen");
            }
            if (iPlayer.HasSwatRights()) menu.Add("Swat-Dienst verlassen", "");
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

                switch (index)
                {
                    case 1:
                        // Out of Duty
                        iPlayer.SendNewNotification("Sie haben sich vom Dienst abgemeldet!", title:"Dienstende", notificationType:PlayerNotification.NotificationType.ERROR);
                        iPlayer.SetArmor(0);
                        iPlayer.SetDuty(false);

                        int back = 0;
                        foreach(WeaponDetail wdetail in iPlayer.Weapons)
                        {
                            var WeaponData = WeaponDataModule.Instance.Get(wdetail.WeaponDataId);

                            // Weapon is in Armory
                            ArmoryWeapon armoryWeapon = Armory.ArmoryWeapons.Where(aw => aw.Weapon == (WeaponHash)WeaponData.Hash).FirstOrDefault();
                            if(armoryWeapon != null)
                            {
                                // Gebe 50% an Geld zurück
                                back += armoryWeapon.Price;
                            }
                        }
                        if(back > 0)
                        {
                            iPlayer.SendNewNotification($"Sie haben ${back} als Rückzahlung für Ihr Equipment erhalten!");
                            iPlayer.GiveBankMoney(back);
                            back = 0;
                        }

                        iPlayer.RemoveWeapons();
                        iPlayer.Team.SendNotification(Lang.rang[0] + iPlayer.TeamRank + " | " + $"{iPlayer.GetName()} meldet sich vom Dienst ab.");
                        MenuManager.DismissMenu(iPlayer.Player, (int)PlayerMenu.Armory);
                        break;
                    case 2:
                        if(iPlayer.Suspension && (iPlayer.IsACop() || iPlayer.TeamId == (int)teams.TEAM_DPOS || iPlayer.TeamId == (int)teams.TEAM_DRIVINGSCHOOL || iPlayer.TeamId == (int)teams.TEAM_MEDIC))
                        {
                            iPlayer.SendNewNotification("Sie koennen nicht in Dienst gehen, wenn sie suspendiert sind!");
                            return false;
                        }

                        // in Duty
                        iPlayer.SendNewNotification("Sie haben sich zum Dienst gemeldet!", title: "Dienstbereit", notificationType:PlayerNotification.NotificationType.SUCCESS);
                        iPlayer.SetDuty(true);
                        iPlayer.SetHealth(100);
                        iPlayer.Team.SendNotification(Lang.rang[0] + iPlayer.TeamRank + " | " + $"{iPlayer.GetName()} meldet sich zum Dienst an.");
                        MenuManager.DismissMenu(iPlayer.Player, (int)PlayerMenu.Armory);
                        break;
                    case 3:
                        if(Armory.ArmoryArmors.Count <= 0)
                        {
                            iPlayer.SendNewNotification("Ihr Team hat keine Schutzwesten verfügbar!");
                            MenuManager.DismissMenu(iPlayer.Player, (int)PlayerMenu.Armory);
                            return true;
                        }
                        // Westen
                        MenuManager.Instance.Build(PlayerMenu.ArmoryArmorMenu, iPlayer).Show(iPlayer);
                        break;
                    case 4:
                        if (Armory.ArmoryWeapons.Count <= 0)
                        {
                            iPlayer.SendNewNotification("Ihr Team hat keine Waffen verfügbar!");
                            MenuManager.DismissMenu(iPlayer.Player, (int)PlayerMenu.Armory);
                            return true;
                        }
                        // Waffen
                        MenuManager.Instance.Build(PlayerMenu.ArmoryWeapons, iPlayer).Show(iPlayer);
                        break;
                    case 5:
                        if (Armory.ArmoryItems.Count <= 0)
                        {
                            iPlayer.SendNewNotification("Ihr Team hat keine Gegenstände verfügbar!");
                            MenuManager.DismissMenu(iPlayer.Player, (int)PlayerMenu.Armory);
                            return true;
                        }
                        // Items
                        MenuManager.Instance.Build(PlayerMenu.ArmoryItems, iPlayer).Show(iPlayer);
                        break;
                    case 6:
                        if (Armory.ArmoryWeapons.Count <= 0)
                        {
                            iPlayer.SendNewNotification("Ihr Team hat keine Munition verfügbar!");
                            MenuManager.DismissMenu(iPlayer.Player, (int)PlayerMenu.Armory);
                            return true;
                        }
                        // Ammo
                        MenuManager.Instance.Build(PlayerMenu.ArmoryAmmo, iPlayer).Show(iPlayer);
                        break;
                    case 7:
                        if (iPlayer.HasSwatRights() && iPlayer.IsSwatDuty())
                        {
                            iPlayer.SetSwatDuty(false);
                            break;
                        }
                        MenuManager.DismissMenu(iPlayer.Player, (int)PlayerMenu.Armory);
                        break;
                     default:
                        MenuManager.DismissMenu(iPlayer.Player, (int) PlayerMenu.Armory);
                        break;
                }

                return false;
            }
        }
    }
}
 
using GTANetworkAPI;
using System.Collections.Generic;
using System.Linq;
using GVRP.Handler;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Farming;
using GVRP.Module.Items;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Windows;
using GVRP.Module.Vehicles;

namespace GVRP.Module.Houses
{
    public class LaborArmorMenuBuilder : MenuBuilder
    {
        public LaborArmorMenuBuilder() : base(PlayerMenu.LaborArmorMenu)
        {
        }

        public override Module.Menu.Menu Build(DbPlayer iPlayer)
        {
            var menu = new Module.Menu.Menu(Menu, "Herstellung");

            menu.Add($"Schließen");
            menu.Add($"Schutzweste ($2000/SG $1500)");
            menu.Add($"Underarmor ($1800/SG $1200)");
            return menu;
        }

        public override IMenuEventHandler GetEventHandler()
        {
            return new EventHandler();
        }

        private class EventHandler : IMenuEventHandler
        {
            public bool OnSelect(int index, DbPlayer dbPlayer)
            {
                if (!dbPlayer.CanInteract())
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }
                if (index == 0)
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }
                else if (index == 1)
                {
                    if (dbPlayer.Player.Position.DistanceTo(new Vector3(1129.7, -3194.27, -40.3972)) <= 2.0f)
                    {
                        int kevlar = 9;
                        // Schutzweste herstellen
                        if (dbPlayer.Container.GetItemAmount(592) >= kevlar)
                        {
                            if (!dbPlayer.TakeBlackMoney(1500))
                            {
                                if (!dbPlayer.TakeMoney(2000))
                                {
                                    dbPlayer.SendNewNotification("Sie benötigen mindestens 2000$ Herstellungskosten (oder $1500 Schwargeld)!");
                                    return true;
                                }
                            }
                            dbPlayer.SendNewNotification($"Sie haben aus {kevlar} {ItemModelModule.Instance.Get(592).Name} eine Schutzweste hergestellt!");
                            dbPlayer.Container.RemoveItem(592, kevlar);
                            dbPlayer.Container.AddItem(40, 1);
                            return true;
                        }
                        else
                        {
                            dbPlayer.SendNewNotification($"Zum Herstellen einer Schutzweste benötigen Sie {kevlar} {ItemModelModule.Instance.Get(592).Name}!");
                            return true;
                        }
                    }
                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }
                else if (index == 2)
                {
                    if (dbPlayer.Player.Position.DistanceTo(new Vector3(1129.7, -3194.27, -40.3972)) <= 2.0f)
                    {
                        int kevlar = 6;
                        // Schutzweste herstellen
                        if (dbPlayer.Container.GetItemAmount(592) >= kevlar)
                        {
                            if (!dbPlayer.TakeBlackMoney(1200))
                            {
                                if (!dbPlayer.TakeMoney(1800))
                                {
                                    dbPlayer.SendNewNotification("Sie benötigen mindestens 1800$ Herstellungskosten (oder $1200 Schwargeld)!");
                                    return true;
                                }
                            }
                            dbPlayer.SendNewNotification($"Sie haben aus {kevlar} {ItemModelModule.Instance.Get(592).Name} eine Underarmor Schutzweste hergestellt!");
                            dbPlayer.Container.RemoveItem(592, kevlar);
                            dbPlayer.Container.AddItem(975, 1);
                            return true;
                        }
                        else
                        {
                            dbPlayer.SendNewNotification($"Zum Herstellen einer Underarmor Schutzweste benötigen Sie {kevlar} {ItemModelModule.Instance.Get(592).Name}!");
                            return true;
                        }
                    }
                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }

                MenuManager.DismissCurrent(dbPlayer);
                return true;
            }
        }
    }
}
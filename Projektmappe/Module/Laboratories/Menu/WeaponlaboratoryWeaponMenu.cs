using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GVRP.Module.Chat;
using GVRP.Module.Items;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Laboratories.Menu
{
    public class WeaponlaboratoryWeaponMenu : MenuBuilder
    {
        public WeaponlaboratoryWeaponMenu() : base(PlayerMenu.LaboratoryWeaponMenu)
        {
        }

        public override Module.Menu.Menu Build(DbPlayer iPlayer)
        {
            if (iPlayer.Container.GetItemAmount(976) <= 0)
            {
                iPlayer.SendNewNotification("Sie benötigen ein Waffenset!");
                return null;
            }

            var menu = new Module.Menu.Menu(Menu, "Herstellung");

            menu.Add($"Schließen");
            foreach(KeyValuePair<uint, int> kvp in WeaponlaboratoryModule.Instance.WeaponHerstellungList)
            {
                menu.Add($"{kvp.Value} - {ItemModelModule.Instance.Get(kvp.Key).Name}");
            }
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
                if (index == 0)
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return false;
                }
                else
                {
                    int idx = 1;
                    foreach (KeyValuePair<uint, int> kvp in WeaponlaboratoryModule.Instance.WeaponHerstellungList)
                    {
                        if(index == idx)
                        {
                            if (dbPlayer.Container.GetItemAmount(976) <= 0)
                            {
                                dbPlayer.SendNewNotification("Sie benötigen ein Waffenset!");
                                return false;
                            }

                            if (!dbPlayer.Container.CanInventoryItemAdded(kvp.Key))
                            {
                                dbPlayer.SendNewNotification(MSG.Inventory.NotEnoughSpace());
                                return false;
                            }

                            if (!dbPlayer.TakeBlackMoney(kvp.Value))
                            {
                                dbPlayer.SendNewNotification(MSG.Money.NotEnoughSWMoney(kvp.Value));
                                return false;
                            }

                            // remove Waffenset
                            dbPlayer.Container.RemoveItem(976);

                            Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                            {
                                int time = 15000; // 15 sek
                                Chats.sendProgressBar(dbPlayer, time);
                                
                                dbPlayer.Player.TriggerEvent("freezePlayer", true);
                                dbPlayer.SetData("userCannotInterrupt", true);

                                await Task.Delay(time);
                                if (dbPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(dbPlayer.Player) || !dbPlayer.Player.Exists) return;

                                dbPlayer.SetData("userCannotInterrupt", false);
                                dbPlayer.Player.TriggerEvent("freezePlayer", false);
                                
                                dbPlayer.Container.AddItem(kvp.Key, 1);
                                dbPlayer.SendNewNotification($"Sie haben {ItemModelModule.Instance.Get(kvp.Key).Name} für {kvp.Value} hergestellt!");

                            }));
                            return true;
                        }
                        idx++;
                    }

                    MenuManager.DismissCurrent(dbPlayer);
                    return true;
                }
                
            }
        }
    }
}

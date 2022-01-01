using System.Collections.Generic;
using GVRP.Handler;
using GVRP.Module.Chat;
using GVRP.Module.GTAN;
using GVRP.Module.Houses;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP
{
    public class AccountHouseKeyMenuBuilder : MenuBuilder
    {
        public AccountHouseKeyMenuBuilder() : base(PlayerMenu.AccountHouseKeys)
        {
        }

        public override Menu Build(DbPlayer iPlayer)
        {
            var menu = new Menu(Menu, "Schluessel");
            menu.Add(MSG.General.Close(), "");


            if (iPlayer.ownHouse[0] > 0)
            {
                menu.Add(
                    "~g~Hausschluessel " + iPlayer.ownHouse[0],
                    "Klicken um Schluessel zu vergeben");
            }

            var keys = iPlayer.HouseKeys;
            if (keys.Count > 0)
            {
                foreach (var key in keys)
                {
                    if (key != 0)
                    {
                        menu.Add(
                            "Hausschluessel " + key,
                            "~r~Klicken um Schluessel fallen zu lassen!");
                    }
                }
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
                if (index == 0) // General Close
                {
                    return true;
                }

                if (index == 1)
                {
                    if (iPlayer.ownHouse[0] > 0)
                    {
                        iPlayer.SetData("sKeyId", (uint) iPlayer.ownHouse[0]);

                        // Chose Menu
                        MenuManager.DismissMenu(iPlayer.Player, (int) PlayerMenu.AccountHouseKeys);
                        iPlayer.Player.CreateUserDialog(Dialogs.menu_housekeys_input, "inputtext");
                    }

                    return false;
                }

                int idx = 2;
                var keys = iPlayer.HouseKeys;
                if (keys.Count > 0)
                {
                    foreach (uint key in keys)
                    {
                        if (key != 0)
                        {
                            if (idx == index)
                            {
                                HouseKeyHandler.Instance.DeleteHouseKey(iPlayer, HouseModule.Instance.Get(key));
                                iPlayer.SendNewNotification(

                                    "Sie haben den Schluessel fuer das Haus " +
                                    key + " fallen gelassen!");
                                MenuManager.DismissMenu(iPlayer.Player, (int) PlayerMenu.AccountHouseKeys);
                                return true;
                            }

                            idx++;
                        }
                    }
                }

                return true;
            }
        }
    }
}
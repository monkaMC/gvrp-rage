using GVRP.Handler;
using GVRP.Module.Chat;
using GVRP.Module.GTAN;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Menu.Menus.Account
{
    public class AccountVehicleKeyMenuBuilder : MenuBuilder
    {
        public AccountVehicleKeyMenuBuilder() : base(PlayerMenu.AccountVehicleKeys)
        {
        }

        public override Menu Build(DbPlayer iPlayer)
        {
            var menu = new Menu(Menu, "Schluessel");
            menu.Add(MSG.General.Close(), "");

            // List Fahrzeuge
            foreach (SxVehicle Vehicle in VehicleHandler.Instance.GetPlayerVehicles(iPlayer.Id))
            {
                if (Vehicle == null) continue;
                
                if (Vehicle.databaseId == 0) continue;

                if (iPlayer.IsOwner(Vehicle))
                {
                    menu.Add(
                        "~g~" + Vehicle.Data.Model +
                        " Schluessel " + Vehicle.databaseId,
                        "Klicken um Schluessel zu vergeben");
                }
            }

            var keys = iPlayer.VehicleKeys;
            if (keys.Count > 0)
            {
                foreach (var key in keys)
                {
                    if (key.Key != 0)
                    {
                        menu.Add(
                            VehicleHandler.Instance.GetPlayerVehicleNameByDatabaseId(key.Key) + " Schluessel " + key.Key,
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
                var player = iPlayer.Player;
                if (index == 0) // General Close
                {
                    MenuManager.DismissMenu(player, (int) PlayerMenu.AccountVehicleKeys);
                    return false;
                }

                int idx = 1;
                foreach (SxVehicle Vehicle in VehicleHandler.Instance.GetPlayerVehicles(iPlayer.Id))
                {
                    if (Vehicle == null) continue;
                    {
                        if (Vehicle.databaseId == 0) continue;
                        if (iPlayer.IsOwner(Vehicle))
                        {
                            if (idx == index)
                            {
                                iPlayer.SetData("sKeyId", Vehicle.databaseId);

                                // Chose Menu
                                MenuManager.DismissMenu(player, (int) PlayerMenu.AccountVehicleKeys);
                                MenuManager.Instance.Build(PlayerMenu.AccountKeyChooseMenu, iPlayer).Show(iPlayer);
                                return false;
                            }

                            idx++;
                        }
                    }
                }

                var keys = iPlayer.VehicleKeys;
                if (keys.Count > 0)
                {
                    foreach (var key in keys)
                    {
                        if (key.Key != 0)
                        {
                            if (idx == index)
                            {
                                VehicleKeyHandler.Instance.DeletePlayerKey(iPlayer, key.Key);
                                iPlayer.SendNewNotification(

                                    "Sie haben den Schluessel fuer das Fahrzeug " +
                                    VehicleHandler.Instance.GetPlayerVehicleNameByDatabaseId(key.Key) +
                                    " (" + key +
                                    ") fallen gelassen!");
                                MenuManager.DismissMenu(player, (int) PlayerMenu.AccountVehicleKeys);
                                return false;
                            }

                            idx++;
                        }
                    }
                }
                return false;
            }
        }
    }
}
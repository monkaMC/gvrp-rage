using GVRP.Handler;
using GVRP.Module.Business;
using GVRP.Module.GTAN;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Menu.Menus.Account
{
    public class AccountVehicleKeyChooseMenuBuilder : MenuBuilder
    {
        public AccountVehicleKeyChooseMenuBuilder() : base(PlayerMenu.AccountKeyChooseMenu)
        {
        }

        public override Menu Build(DbPlayer iPlayer)
        {
            var menu = new Menu(Menu, "Schluessel");
            menu.Add(MSG.General.Close(), "");

            menu.Add("Spieler geben", "Schluessel an Spieler geben");

            if (iPlayer.IsMemberOfBusiness())
            {
                if (iPlayer.ActiveBusiness != null)
                {
                    menu.Add("~b~" + iPlayer.ActiveBusiness.Name, "Schluessel im Business hinterlegen");
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
                    MenuManager.DismissMenu(iPlayer.Player, (int) PlayerMenu.AccountKeyChooseMenu);
                    return false;
                }
                else if (index == 1) // Spieler
                {
                    MenuManager.DismissMenu(iPlayer.Player, (int) PlayerMenu.AccountKeyChooseMenu);
                    iPlayer.Player.CreateUserDialog(Dialogs.menu_keys_input, "inputtext");
                }
                else if (index == 2) // Business
                {
                    if (iPlayer.IsMemberOfBusiness())
                    {
                        if (iPlayer.ActiveBusiness != null)
                        {
                            //(uint) iPlayer.Player.GetData("sKeyId")
                            iPlayer.ActiveBusiness.AddVehicleKey((uint) iPlayer.GetData("sKeyId"), VehicleHandler.Instance.GetPlayerVehicleNameByDatabaseId((uint)iPlayer.GetData("sKeyId")));
                            iPlayer.SendNewNotification(
                                $"Schluessel {iPlayer.GetData("sKeyId")} fuer Business {iPlayer.ActiveBusiness.Name} hinterlegt!");
                        }
                    }

                    MenuManager.DismissMenu(iPlayer.Player, (int) PlayerMenu.AccountKeyChooseMenu);
                    return false;
                }
                return false;
            }
        }
    }
}
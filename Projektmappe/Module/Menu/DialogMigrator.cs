using System;
using GTANetworkAPI;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Menu
{

    public static class DialogMigrator
    {
        public static void OpenUserMenu(DbPlayer iPlayer, uint MenuID, bool nofreeze = false)
        {
            if (iPlayer.WatchMenu > 0)
            {
                //CloseUserMenu(iPlayer.Player, iPlayer.WatchMenu);
            }
            
            ShowMenu(iPlayer.Player, MenuID);
            iPlayer.WatchMenu = MenuID;
        }

        public static void CloseUserMenu(Client player, uint MenuID, bool noHide = false)
        {
            DbPlayer iPlayer = player.GetPlayer();
            if (iPlayer == null) return;
            if(!noHide) HideMenu(player, MenuID);
            /*if (iPlayer.Freezed == false)
            {
                player.FreezePosition = false;
            }*/

            iPlayer.WatchMenu = 0;
        }

        public static void CreateMenu(Client player, uint menuid, string name = "", string description = "")
        {
            player.TriggerEvent("componentServerEvent", "NativeMenu", "createMenu", name, description);
        }

        public static void AddMenuItem(Client player, uint menuid, string label, string description)
        {
            player.TriggerEvent("componentServerEvent", "NativeMenu", "addItem", label, description);
        }

        public static void ShowMenu(Client player, uint menuid)
        {
            player.TriggerEvent("componentServerEvent", "NativeMenu", "show", menuid);
        }

        private static void HideMenu(Client player, uint menuid)
        {
            player.TriggerEvent("componentServerEvent", "NativeMenu", "hide");
        }

        public static void CloseUserDialog(Client player, uint dialogid)
        {
            var iPlayer = player.GetPlayer();
            iPlayer.watchDialog = 0;
            player.TriggerEvent("deleteDialog");
            iPlayer.Player.TriggerEvent("freezePlayer", false);
            //player.Freeze(false);
        }

    }
}

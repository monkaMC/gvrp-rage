using System;
using GTANetworkAPI;
using GVRP.Module.Chat;
using GVRP.Module.Farming;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool RessourceMap(DbPlayer iPlayer, ItemModel ItemData)
        {
            DialogMigrator.CreateMenu(iPlayer.Player, Dialogs.menu_ressourcemap, "Ressourcen Karte", "");
            DialogMigrator.AddMenuItem(iPlayer.Player, Dialogs.menu_ressourcemap, MSG.General.Close(), "");

            foreach (var xFarm in FarmSpotModule.Instance.GetAll())
            {
                if (xFarm.Value.RessourceName != "")
                {
                    DialogMigrator.AddMenuItem(iPlayer.Player, Dialogs.menu_ressourcemap,
                        xFarm.Value.RessourceName,
                        "Ressourcenpunkt fuer " + xFarm.Value.RessourceName);
                }
            }

            foreach (var farmProcess in FarmProcessModule.Instance.GetAll())
            {
                if (farmProcess.Value.ProcessName != "")
                {
                    DialogMigrator.AddMenuItem(iPlayer.Player, Dialogs.menu_ressourcemap,
                        farmProcess.Value.ProcessName,
                        "Hersteller fuer " + ItemModelModule.Instance.Get(farmProcess.Value.RewardItemId).Name);
                }
            }

            iPlayer.Player.TriggerEvent("hideInventory");
            iPlayer.watchDialog = 0;
            iPlayer.ResetData("invType");

            DialogMigrator.OpenUserMenu(iPlayer, Dialogs.menu_ressourcemap);

            return true;
        }
    }
}

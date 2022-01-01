using System;
using System.Threading.Tasks;
using GTANetworkAPI;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.PlayerAnimations;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool Present(DbPlayer iPlayer, ItemModel ItemData)
        {
            if (iPlayer.Container.GetInventoryFreeSpace() < 10000 || iPlayer.Container.MaxSlots - iPlayer.Container.GetUsedSlots() < 2)
            {
                iPlayer.SendNewNotification("Du benoetigst mehr Platz in den Taschen! (30kg & 2 Plätze)");
                return false;
            }

            if(ItemData.Id == 550) // großes Geschenk
            {
                iPlayer.Container.AddItem(504); // 25% KFZ
                iPlayer.Container.AddItem(552); // Teddy
                iPlayer.GiveMoney(iPlayer.Level * 10000);

                iPlayer.SendNewNotification($"Du hast {iPlayer.Level * 10000}$ erhalten!");
            }
            else
            {
                iPlayer.SendNewNotification("Etwas ist gewaltig schief gelaufen...");
                return false;
            }
            iPlayer.SendNewNotification("Du hast ein " + ItemData.Name + " geoeffnet");
            // RefreshInventory
            return true;
        }
    }
}
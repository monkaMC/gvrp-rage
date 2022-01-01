using GVRP.Module.Chat;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.PlayerAnimations;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool paper(DbPlayer iPlayer, ItemModel ItemData)
        {
            if (iPlayer.Container.GetItemAmount(8) < 1)
            {
                iPlayer.SendNewNotification(
                    
                    "Ohne Grindedweed, kannst du keinen Joint bauen!");
                return false;
            }

            iPlayer.Container.RemoveItem(8);
            iPlayer.Container.AddItem(159);
            iPlayer.SendNewNotification(
                
                "Du hast etwas Grindedweed in einem Paper zu einem Joint gedreht!");

            // RefreshInventory
            return true;
        }
    }
}
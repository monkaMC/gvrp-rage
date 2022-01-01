using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using GVRP.Module.Chat;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.PlayerAnimations;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool vehiclerent(DbPlayer iPlayer, ItemModel ItemData)
        {
            if (!iPlayer.CanInteract())
            {
                return false;
            }
            
            MenuManager.Instance.Build(PlayerMenu.VehicleRentMenu, iPlayer).Show(iPlayer);
            return false;
        }

        public static bool vehiclerentview(DbPlayer iPlayer, ItemModel ItemData, Item item)
        {
            if (!iPlayer.CanInteract())
            {
                return false;
            }

            if (item.Data != null && item.Data.ContainsKey("info"))
            {
                iPlayer.SendNewNotification($"KFZ-Mietvertrag: {item.Data["info"]}", PlayerNotification.NotificationType.STANDARD, "", 12000);
            }
            return false;
        }
    }
}
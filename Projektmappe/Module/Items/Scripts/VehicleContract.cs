using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool VehicleContract(DbPlayer iPlayer, Item item)
        {
            if (item.Data == null) return false;
            if (!item.Data.ContainsKey("Info")) return false;
            string info = (string)item.Data["Info"];

            iPlayer.SendNewNotification(info, PlayerNotification.NotificationType.INFO, "Fahrzeugkaufvertrag", 15000);

            return false;
        } 
    }
}
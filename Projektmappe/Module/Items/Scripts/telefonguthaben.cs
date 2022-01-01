using GVRP.Module.Chat;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool telefonguthaben(DbPlayer iPlayer, ItemModel ItemData)
        {
            // Guthaben
            if (iPlayer.guthaben[0] >= 900)
            {
                iPlayer.SendNewNotification(
                    
                    "Sie haben das maximale Limit an Guthaben erreicht!");
                return false;
            }

            iPlayer.guthaben[0] = iPlayer.guthaben[0] + 100;
            iPlayer.SendNewNotification("Sie haben $100 Guthaben verwendet!");
            return true;
        }
    }
}
using GVRP.Module.Clothes;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool Divebottle(DbPlayer iPlayer, ItemModel itemModel)
        {
            int texture = int.Parse(itemModel.Script.Split("_")[1]);
            iPlayer.SetClothes(8, 123, texture);

            iPlayer.SendNewNotification("Taucherflasche angezogen");

            return true;
        }
    }
}
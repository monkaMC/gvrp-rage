using System;
using System.Globalization;
using System.Threading.Tasks;
using GTANetworkAPI;
using GVRP.Module.Chat;

using GVRP.Module.Players.Db;
using GVRP.Module.Players.PlayerAnimations;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool Bargeldkoffer(DbPlayer iPlayer, ItemModel ItemData, Item Item)
        {
            if(Item.Data.ContainsKey("DateTime") && Item.Data.ContainsKey("Mins"))
            {
                DateTime dateTime = DateTime.ParseExact(Item.Data["DateTime"], "ddMMyyyy", CultureInfo.InvariantCulture);
                int min = Convert.ToInt32(Item.Data["Mins"]);
            }
            return false;
        }
    }
}
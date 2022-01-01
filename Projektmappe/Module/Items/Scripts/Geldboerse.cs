using System;
using System.Globalization;
using System.Threading.Tasks;
using GTANetworkAPI;
using GVRP.Module.Chat;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.PlayerAnimations;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool Geldboerse(DbPlayer iPlayer, ItemModel ItemData)
        {
            int price = new Random().Next(100, 1100);
            iPlayer.GiveMoney(price);
            iPlayer.SendNewNotification("Geldboerse geoeffnet! $" + price);
            return true;
        }
    }
}
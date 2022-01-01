using System.Threading.Tasks;
using GTANetworkAPI;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.PlayerAnimations;
using GVRP.Module.Players.Windows;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool Digging(DbPlayer iPlayer)
        {
            if (iPlayer.Player.IsInVehicle) return false;
            return false;

            ComponentManager.Get<TextInputBoxWindow>().Show()(iPlayer, new TextInputBoxWindowObject() { Title = "Gold-Digger", Callback = "DigTo", Message = "Gib einen Namen zudem du drich graben willst ein" });
            return true;
        }
    }
}
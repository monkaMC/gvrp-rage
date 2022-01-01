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
        public static async Task<bool> Food(DbPlayer iPlayer, ItemModel ItemData)
        {
            if (!iPlayer.CanInteract() || iPlayer.Player.IsInVehicle) return false;

            iPlayer.PlayAnimation((int)(AnimationFlags.AllowPlayerControl | AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody), "mp_player_inteat@burger", "mp_player_int_eat_burger");
            await Task.Delay(5000);
            iPlayer.StopAnimation();
            return true;
        }
    }
}
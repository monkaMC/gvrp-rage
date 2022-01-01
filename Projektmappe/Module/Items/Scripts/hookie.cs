using System.Threading.Tasks;
using GTANetworkAPI;
using GVRP.Module.Chat;

using GVRP.Module.Players.Db;
using GVRP.Module.Players.PlayerAnimations;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> hookie(DbPlayer iPlayer, ItemModel ItemData)
        {
            
                iPlayer.PlayAnimation( (int) (AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["rock"].Split()[0], Main.AnimationList["rock"].Split()[1]); 
                iPlayer.Player.TriggerEvent("freezePlayer", true); 
                await Task.Delay(3000); 
                iPlayer.Player.TriggerEvent("freezePlayer", false); 
                NAPI.Player.StopPlayerAnimation(iPlayer.Player); 
            
            return true;
        }
    }
}
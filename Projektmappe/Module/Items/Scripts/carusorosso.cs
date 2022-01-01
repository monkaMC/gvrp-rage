using System.Threading.Tasks;
using GTANetworkAPI;
using GVRP.Module.Chat;

using GVRP.Module.Players.Db;
using GVRP.Module.Players.PlayerAnimations;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> carusorosso(DbPlayer iPlayer, ItemModel ItemData)
        {
            
                iPlayer.PlayAnimation( 
                    (int) (AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["bro"].Split()[0], Main.AnimationList["bro"].Split()[1]); 
                iPlayer.Player.TriggerEvent("freezePlayer", true); 
                await Task.Delay(3000); 
             if (iPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(iPlayer.Player) || !iPlayer.Player.Exists) return false;
                iPlayer.Player.TriggerEvent("freezePlayer", false); 
                NAPI.Player.StopPlayerAnimation(iPlayer.Player); 
            
            return true;
        }
    }
}
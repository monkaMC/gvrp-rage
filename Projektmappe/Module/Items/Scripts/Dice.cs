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
        public static async Task<bool> Dice(DbPlayer iPlayer, ItemModel itemModel)
        {
            if (iPlayer.Player.IsInVehicle) return false;
            
            if (!int.TryParse(itemModel.Script.Split("_")[1], out int max)) return false;

            var trig = Main.rnd.Next(1, max);
            
                
                iPlayer.PlayAnimation((int)(AnimationFlags.AllowPlayerControl | AnimationFlags.Loop), "mp_player_int_upperwank", "mp_player_int_wank_01");
                await Task.Delay(2000);
                iPlayer.StopAnimation();
            
            
            var surroundingUsers = NAPI.Player.GetPlayersInRadiusOfPlayer(20.0f, iPlayer.Player);

            foreach (var user in surroundingUsers)
            {
                if (user.Dimension == iPlayer.Player.Dimension)
                {
                    var dbPlayer = user.GetPlayer();
                    if (dbPlayer == null || !dbPlayer.IsValid())continue;
                    
                    dbPlayer.SendNewNotification( "* " + iPlayer.GetName() + " rollt die Wuerfel und bekommt eine " + trig + ".");
                }
            }
            return false;
        }
    }
}
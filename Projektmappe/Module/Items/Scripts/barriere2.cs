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
        public static async Task<bool> barriere2(DbPlayer iPlayer, ItemModel ItemData)
        {
            if (iPlayer.IsCuffed || iPlayer.IsTied ||
                (!iPlayer.Team.IsCops() && !iPlayer.Team.IsDpos() && !iPlayer.Team.IsMedics()) ||
                iPlayer.Player.IsInVehicle)
            {
                return false;
            }

            if (PoliceObjectModule.Instance.IsMaxReached())
            {
                iPlayer.SendNewNotification(
                    
                    "Maximale Anzahl an Polizeiabsperrungen erreicht!");
                return false;
            }

            PoliceObjectModule.Instance.Add(-143315610, iPlayer.Player, ItemData, false);
                iPlayer.SendNewNotification(
                     ItemData.Name +
                    " erfolgreich platziert!");
                iPlayer.PlayAnimation(
                    (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["fixing"].Split()[0], Main.AnimationList["fixing"].Split()[1]);
                iPlayer.Player.TriggerEvent("freezePlayer", true);
                await Task.Delay(4000);
            iPlayer.Player.TriggerEvent("freezePlayer", false);
                NAPI.Player.StopPlayerAnimation(iPlayer.Player);
            

            return true;
        }
    }
}
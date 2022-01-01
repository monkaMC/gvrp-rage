using System.Threading.Tasks;
using GTANetworkAPI;
using GVRP.Module.Blitzer;
using GVRP.Module.Chat;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.PlayerAnimations;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> Blitzer70(DbPlayer iPlayer, ItemModel ItemData)
        {
            if (iPlayer.IsCuffed || iPlayer.IsTied ||
                iPlayer.Team.Id != (uint)teams.TEAM_POLICE ||
                iPlayer.Player.IsInVehicle)
            {
                return false;
            }

            if(BlitzerModule.Instance.aufgestellt >= 4)
            {
                iPlayer.SendNewNotification( "Maximale Anzahl an Blitzern erreicht!");
                return false;
            }

            if (PoliceObjectModule.Instance.IsMaxReached())
            {
                iPlayer.SendNewNotification( "Maximale Anzahl an Polizeiabsperrungen erreicht!");
                return false;
            }

            PoliceObjectModule.Instance.Add(1382242693, iPlayer.Player, ItemData, false);

            Vector3 pos = iPlayer.Player.Position;
            pos.Z = pos.Z - 5.0f;
            BlitzerModule.Instance.AddBlitzer(pos, iPlayer.GetName(), (int)iPlayer.TeamId, 70);

            iPlayer.SendNewNotification( ItemData.Name + " erfolgreich platziert!");
            
                iPlayer.PlayAnimation(
                    (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["fixing"].Split()[0], Main.AnimationList["fixing"].Split()[1]);
                iPlayer.Player.TriggerEvent("freezePlayer", true);
                await Task.Delay(4000);
                iPlayer.Player.TriggerEvent("freezePlayer", false);
                NAPI.Player.StopPlayerAnimation(iPlayer.Player);
            
            return true;
        }

        public static async Task<bool> Blitzer120(DbPlayer iPlayer, ItemModel ItemData)
        {
            if (iPlayer.IsCuffed || iPlayer.IsTied ||
                iPlayer.Team.Id != (uint)teams.TEAM_POLICE ||
                iPlayer.Player.IsInVehicle)
            {
                return false;
            }

            if (BlitzerModule.Instance.aufgestellt >= 4)
            {
                iPlayer.SendNewNotification( "Maximale Anzahl an Blitzern erreicht!");
                return false;
            }

            if (PoliceObjectModule.Instance.IsMaxReached())
            {
                iPlayer.SendNewNotification( "Maximale Anzahl an Polizeiabsperrungen erreicht!");
                return false;
            }

            PoliceObjectModule.Instance.Add(1382242693, iPlayer.Player, ItemData, false);

            BlitzerModule.Instance.AddBlitzer(iPlayer.Player.Position, iPlayer.GetName(), (int)iPlayer.TeamId, 120);

            iPlayer.SendNewNotification( ItemData.Name + " erfolgreich platziert!");
            
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
using System.Threading.Tasks;
using GTANetworkAPI;
using GVRP.Module.Chat;
using GVRP.Module.Gangwar;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> medikit(DbPlayer iPlayer, ItemModel ItemData)
        {
            if (iPlayer.Player.Health > 99)
            {
                return false;
            }

            if (iPlayer.Player.IsInVehicle)
            {
                iPlayer.SendNewNotification("Du kannst waehrend der Fahrt keinen Verbandskasten benutzen");
                return false;
            }

                Chats.sendProgressBar(iPlayer, 4000);
                iPlayer.PlayAnimation(
                    (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["revive"].Split()[0], Main.AnimationList["revive"].Split()[1]);
                iPlayer.Player.TriggerEvent("freezePlayer", true);
                await Task.Delay(4000);
            if (iPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(iPlayer.Player) || !iPlayer.Player.Exists) return false;
            iPlayer.Player.TriggerEvent("freezePlayer", false);
                NAPI.Player.StopPlayerAnimation(iPlayer.Player);
                iPlayer.SetHealth(100);

            return true;
        }

        public static async Task<bool> FMedikit(DbPlayer iPlayer, ItemModel ItemData)

        {

            if (iPlayer.Player.IsInVehicle || iPlayer.Player.Health > 99) return false;
            //if (!iPlayer.Team.IsInTeamfight()) return false;
            if (!GangwarTownModule.Instance.IsTeamInGangwar(iPlayer.Team)) return false;

                Chats.sendProgressBar(iPlayer, 4000);
                iPlayer.PlayAnimation(
                    (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["revive"].Split()[0], Main.AnimationList["revive"].Split()[1]);
                iPlayer.Player.TriggerEvent("freezePlayer", true);
                await Task.Delay(4000);
               if (iPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(iPlayer.Player) || !iPlayer.Player.Exists) return false;
            iPlayer.Player.TriggerEvent("freezePlayer", false);
                NAPI.Player.StopPlayerAnimation(iPlayer.Player);
                iPlayer.SetHealth(100);

            return true;

        }

    }
}
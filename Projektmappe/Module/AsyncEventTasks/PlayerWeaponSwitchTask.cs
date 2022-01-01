using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GVRP.Module.Armory;
using GVRP.Module.Banks;
using GVRP.Module.Clothes;
using GVRP.Module.Houses;
using GVRP.Module.Logging;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Teams;
using GVRP.Module.Vehicles.Garages;

namespace GVRP.Module.AsyncEventTasks
{
    public static partial class AsyncEventTasks
    {
        public static void PlayerWeaponSwitchTask(Client player, WeaponHash oldgun, WeaponHash newWeapon)
        {
            
                DbPlayer iPlayer = player.GetPlayer();

                if (!iPlayer.IsValid()) return;

                Modules.Instance.OnPlayerWeaponSwitch(iPlayer, oldgun, newWeapon);

                if (iPlayer.IsCuffed)
                {
                    iPlayer.Player.PlayAnimation("mp_arresting", iPlayer.Player.IsInVehicle ? "sit" : "idle", 0);
                }

                if (iPlayer.IsTied)
                {
                    if (iPlayer.Player.IsInVehicle) iPlayer.Player.PlayAnimation("mp_arresting", "sit", 0);
                    else iPlayer.Player.PlayAnimation("anim@move_m@prisoner_cuffed_rc", "aim_low_loop", 0);
                }

                if (iPlayer.Lic_Gun[0] <= 0 && iPlayer.Level < 3)
                {
                    iPlayer.RemoveWeapons();
                }
            
        }
    }
}

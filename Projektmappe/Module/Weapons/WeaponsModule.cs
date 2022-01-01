using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using GVRP.Handler;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Vehicles;
using GVRP.Module.Vehicles.Data;
using GVRP.Module.Weapons.Data;

namespace GVRP.Module.Weapons
{
    public sealed class WeaponsModule : Module<WeaponsModule>
    {
        public override void OnPlayerEnterVehicle(DbPlayer dbPlayer, Vehicle vehicle, sbyte seat)
        {
            CheckPlayerDisableDrivebyWeapons(dbPlayer);
        }

        public override void OnPlayerWeaponSwitch(DbPlayer dbPlayer, WeaponHash oldgun, WeaponHash newgun)
        {
            if (dbPlayer.Player.Dimension != (uint)9999)
            {
                GVRP.Anticheat.Anticheat.CheckForbiddenWeapons(dbPlayer);
                var l_WeaponDatas = WeaponDataModule.Instance.GetAll();

                int l_OldGun = 0;
                int l_NewGun = 0;

                foreach (var l_Data in l_WeaponDatas)
                {
                    if (l_OldGun != 0 && l_NewGun != 0)
                        break;

                    if (l_Data.Value.Hash != (int)oldgun && l_Data.Value.Hash != (int)newgun)
                        continue;

                    if (l_Data.Value.Hash == (int)oldgun)
                    {
                        if (dbPlayer.Weapons.Exists(detail => detail.WeaponDataId == l_Data.Key))
                        {
                            l_OldGun = l_Data.Value.Id;
                            dbPlayer.Player.TriggerEvent("getWeaponAmmo", l_OldGun);
                        }
                        else
                        {
                            if (oldgun != WeaponHash.Unarmed)
                                dbPlayer.Player.RemoveWeapon(oldgun);
                            continue;
                        }
                    }
                    else if (l_Data.Value.Hash == (int)newgun)
                    {
                        if (!dbPlayer.Weapons.Exists(detail => detail.WeaponDataId == l_Data.Value.Id))
                        {
                            if (newgun != WeaponHash.Unarmed)
                                dbPlayer.Player.RemoveWeapon(newgun);
                            continue;
                        }

                        l_NewGun = l_Data.Value.Id;
                        dbPlayer.Player.TriggerEvent("setCurrentWeapon", l_NewGun);
                    }
                }

                // WeaponDisabling Driveby
                if (dbPlayer.Player.IsInVehicle)
                {
                    CheckPlayerDisableDrivebyWeapons(dbPlayer);
                }
            }
        }

        public async void CheckPlayerDisableDrivebyWeapons(DbPlayer iPlayer)
        {
            if (iPlayer == null || !iPlayer.IsValid()) return;

            // WeaponDisabling Driveby
            if (iPlayer.Player.IsInVehicle && iPlayer.Player.Vehicle != null)
            {
                SxVehicle sxVehicle = iPlayer.Player.Vehicle.GetVehicle();
                if (sxVehicle == null || !sxVehicle.IsValid()) return;

                if (sxVehicle.teamid == 0 || sxVehicle.teamid != iPlayer.TeamId)
                {
                    WeaponHash currWeapon = NAPI.Player.GetPlayerCurrentWeapon(iPlayer.Player);
                    if (currWeapon == WeaponHash.MicroSMG || currWeapon == WeaponHash.MiniSMG || currWeapon == WeaponHash.MachinePistol)
                    {
                        NAPI.Player.SetPlayerCurrentWeapon(iPlayer.Player, WeaponHash.Unarmed);
                        return;
                    }
                }
            }
        }
    }
}

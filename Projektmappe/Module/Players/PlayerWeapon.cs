using System;
using GTANetworkAPI;
using GVRP.Handler;
using GVRP.Module.Logging;
using GVRP.Module.Players.Db;
using GVRP.Module.Anticheat;
using System.Collections.Generic;
using GVRP.Module.Weapons.Data;
using GVRP.Module.Weapons;
using GVRP.Module.Weapons.Component;
using System.Linq;

namespace GVRP.Module.Players
{
    public static class PlayerWeapon
    {
        public static int MaxPlayerWeaponWeight = 20;

        public static void GiveWeapon(this DbPlayer iPlayer, WeaponHash weaponHash, int ammo, bool equipNow = false,
            bool loaded = false)
        {
            iPlayer.Player.GiveWeapon(weaponHash, ammo);
            iPlayer.Player.SetWeaponAmmo(weaponHash, ammo);
            
            var l_WeaponData = Weapons.Data.WeaponDataModule.Instance.GetAll().ToList().Where(wp => wp.Value.Hash == (int)weaponHash).FirstOrDefault();
            if(l_WeaponData.Value != null)
            { 
                Predicate<WeaponDetail> l_Detail = (WeaponDetail d) => { return d.WeaponDataId == l_WeaponData.Key; };
                if (iPlayer.Weapons.Exists(l_Detail))
                    return;

                WeaponDetail l_Details = new WeaponDetail();
                l_Details.Ammo = ammo;
                l_Details.WeaponDataId = l_WeaponData.Key;
                l_Details.Components = new List<int>();

                iPlayer.Weapons.Add(l_Details);
                iPlayer.Player.TriggerEvent("fillWeaponAmmo", l_Details.WeaponDataId, l_Details.Ammo);
                iPlayer.Player.TriggerEvent("setCurrentWeapon", l_Details.WeaponDataId);
            }
        }

        public static bool CanWeaponAdded(this DbPlayer iPlayer, WeaponHash weaponHash)
        {
            var l_WeaponData = Weapons.Data.WeaponDataModule.Instance.GetAll().ToList().Where(wp => wp.Value.Hash == (int)weaponHash).FirstOrDefault();
            if (l_WeaponData.Value != null)
            {
                int weaponWeight = l_WeaponData.Value.Weight;
                if (l_WeaponData.Value.Id == 81 && (!iPlayer.IsACop() || !iPlayer.IsInDuty())) // SMG und KEIN Cop
                {
                    weaponWeight = 9; // PDW
                }

                if (iPlayer.GetPlayerWeaponsWeight() + weaponWeight > MaxPlayerWeaponWeight)
                {
                    iPlayer.SendNewNotification("Du kannst diese Waffe nicht tragen!", PlayerNotification.NotificationType.ERROR);
                    return false;
                }
                else return true;
            }
            iPlayer.SendNewNotification("Du kannst diese Waffe nicht tragen!", PlayerNotification.NotificationType.ERROR);
            return false;
        }

        public static int GetPlayerWeaponsWeight(this DbPlayer iPlayer)
        {
            int weight = 0;
            foreach( WeaponDetail weaponDetail in iPlayer.Weapons)
            {
                WeaponData l_Data = WeaponDataModule.Instance[weaponDetail.WeaponDataId];
                if (l_Data == null) continue;

                int weaponWeight = l_Data.Weight;

                if (l_Data.Id == 81 && (!iPlayer.IsACop() || !iPlayer.IsInDuty())) // SMG und KEIN Cop
                {
                    weaponWeight = 9; // PDW
                }

                weight += weaponWeight;
            }
            return weight;
        }

        public static void LoadPlayerWeapons(this DbPlayer iPlayer)
        {
            iPlayer.Player.RemoveAllWeapons();
            if (iPlayer.Weapons.Count != 0)
            {
                if (iPlayer.Weapons == null) return;
                foreach (var l_Detail in iPlayer.Weapons.ToList()) // ToList makes it ThreadSafe (Else it's causing a crash due to edit while iterating)
                {

                    if (l_Detail == null) return;
                    WeaponData l_Data = WeaponDataModule.Instance[l_Detail.WeaponDataId];
                    if (l_Data == null) return;
                    iPlayer.GiveWeapon((WeaponHash)l_Data.Hash, l_Detail.Ammo);

                    iPlayer.Player.TriggerEvent("fillWeaponAmmo", l_Data.Id, l_Detail.Ammo);

                    if (l_Detail.Components.Count > 0)
                    {

                        if (l_Detail.Components == null) return;
                        foreach (int l_CompID in l_Detail.Components)
                        {
                            GVRP.Module.Weapons.Component.WeaponComponent l_Comp = WeaponComponentModule.Instance[l_CompID];

                            if (l_Comp == null) return;

                            if (!int.TryParse(l_Comp.Hash, out int l_Hash))
                                continue;

                            iPlayer.Player.SetWeaponComponent((WeaponHash)l_Data.Hash, (GTANetworkAPI.WeaponComponent)l_Hash);
                        }
                    }
                }
            }
            NAPI.Player.SetPlayerCurrentWeapon(iPlayer.Player, WeaponHash.Unarmed);
        }

        public static void RemoveWeapons(this DbPlayer iPlayer)
        {
            iPlayer.Player.RemoveAllWeapons();
            iPlayer.Weapons.Clear();
            iPlayer.Player.TriggerEvent("emptyWeaponAmmo", 0);
        }

        public static void RemoveWeapon(this DbPlayer iPlayer, WeaponHash weapon)
        {
            var l_WeaponID = 0;
            var l_CurrentWeapon = 0;

            var l_WeaponDatas = WeaponDataModule.Instance.GetAll();
            foreach (var l_Weapon in l_WeaponDatas)
            {
                if (l_Weapon.Value.Hash != (int)weapon)
                    continue;

                l_WeaponID = l_Weapon.Key;
                break;
            }

            foreach (var l_Weapon in l_WeaponDatas)
            {
                if (l_Weapon.Value.Hash != (int)iPlayer.Player.CurrentWeapon)
                    continue;

                l_CurrentWeapon = l_Weapon.Key;
                break;
            }

            iPlayer.Player.TriggerEvent("setCurrentWeapon", l_CurrentWeapon);

            WeaponData l_Data = WeaponDataModule.Instance.Get(l_WeaponID);
            if (l_Data == null)
                return;

            foreach (var l_Detail in iPlayer.Weapons)
            {
                if (l_Detail.WeaponDataId != l_WeaponID)
                    continue;

                iPlayer.Weapons.Remove(l_Detail);
                break;
            }

            iPlayer.Player.RemoveWeapon(weapon);

            iPlayer.Player.TriggerEvent("emptyWeaponAmmo");

            foreach (var l_Detail in iPlayer.Weapons)
            {
                iPlayer.Player.TriggerEvent("fillWeaponAmmo", l_Detail.WeaponDataId, l_Detail.Ammo);
            }
        }

        public static void SetWeaponAmmo(this DbPlayer iPlayer, WeaponHash Weapon, int ammo)
        {
            var xammo = iPlayer.Player.GetWeaponAmmo(Weapon);

            var l_WeaponDatas = WeaponDataModule.Instance.GetAll();
            int l_WeaponID = 0;
            foreach (var l_Weapon in l_WeaponDatas)
            {
                if (l_Weapon.Value.Hash != (int)Weapon)
                    continue;

                l_WeaponID = l_Weapon.Key;
                break;
            }

            iPlayer.Player.SetWeaponAmmo(Weapon, xammo + ammo);
            var weaponPlayer = iPlayer.Weapons.FirstOrDefault(w => w.WeaponDataId == l_WeaponID);
            if(weaponPlayer != null)
            {
                weaponPlayer.Ammo = ammo;
            }
        }

        public static int GetWeaponAmmo(this DbPlayer iPlayer, WeaponHash Weapon)
        {
            return NAPI.Player.GetPlayerWeaponAmmo(iPlayer.Player, Weapon);
        }
    }
}
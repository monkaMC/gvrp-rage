using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using GVRP.Module.Chat;
using GVRP.Module.Events.Halloween;
using GVRP.Module.Players;

using GVRP.Module.Players.Db;
using GVRP.Module.Weapons.Data;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool WeaponUnpack(DbPlayer iPlayer, ItemModel ItemData, Item item)
        {
            string weaponstring = ItemData.Script.ToLower().Replace("w_", "");
            WeaponData weaponData = WeaponDataModule.Instance.GetAll().Where(d => d.Value.Name.ToLower().Equals(weaponstring)).FirstOrDefault().Value;

            if (weaponData == null) return false;

            WeaponHash weapon = (WeaponHash)weaponData.Hash;

            if (!iPlayer.CanWeaponAdded(weapon)) return false;

            if (!iPlayer.Team.CanWeaponEquipedForTeam(weapon))
            {
                iPlayer.SendNewNotification("Diese Waffe können Sie nicht ausrüsten!");
                return false;
            }
            
            iPlayer.SendNewNotification("Sie haben Ihre Waffe ausgeruestet!");

            int defaultammo = 0;
            if (weapon == WeaponHash.Molotov || weapon == WeaponHash.Grenade ||
                weapon == WeaponHash.Flare)
            {
                defaultammo = 1;
            }

            if (weapon == WeaponHash.Snowball)
            {
                defaultammo = 10;
            }

            iPlayer.GiveWeapon(weapon, defaultammo);
            return true;


        }

        public static bool WeaponUnpackCop(DbPlayer iPlayer, ItemModel ItemData, Item item)
        {
            string weaponstring = ItemData.Script.ToLower().Replace("bw_", "");

         //   if (!iPlayer.IsCopPackGun()) return false;

            WeaponData weaponData = WeaponDataModule.Instance.GetAll().Where(d => d.Value.Name.ToLower().Equals(weaponstring)).FirstOrDefault().Value;

            if (weaponData == null) return false;

            WeaponHash weapon = (WeaponHash)weaponData.Hash;

            if (!iPlayer.CanWeaponAdded(weapon)) return false;

            if (!iPlayer.Team.CanWeaponEquipedForTeam(weapon))
            {
                iPlayer.SendNewNotification("Diese Waffe können Sie nicht ausrüsten!");
                return false;
            }

            iPlayer.SendNewNotification("Sie haben Ihre Waffe ausgeruestet!");

            int defaultammo = 0;
            if (weapon == WeaponHash.Molotov || weapon == WeaponHash.Grenade ||
                weapon == WeaponHash.Flare)
            {
                defaultammo = 1;
            }

            if (weapon == WeaponHash.Snowball)
            {
                defaultammo = 10;
            }

            iPlayer.GiveWeapon(weapon, defaultammo);

            return true;


        }

        public static async Task<bool> ZerlegteWaffeUnpack(DbPlayer iPlayer, ItemModel ItemData, Item item)
        {
            string weaponstring = ItemData.Script.ToLower().Replace("zw_", "");

            if (!iPlayer.IsAGangster()) return false;

            if (weaponstring.Length <= 0 || !uint.TryParse(weaponstring, out uint WeaponItemId))
            {
                return false;
            }
            int time = 2500; // 2,5 sek
            Chats.sendProgressBar(iPlayer, time);
            iPlayer.Player.TriggerEvent("freezePlayer", true);
            iPlayer.SetData("userCannotInterrupt", true);

            iPlayer.PlayAnimation(
                (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@prop_human_parking_meter@male@base", "base");

            await Task.Delay(time);
            if (iPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(iPlayer.Player) || !iPlayer.Player.Exists) return false;

            iPlayer.Container.RemoveItem(ItemData, 1);
            iPlayer.Container.AddItem(WeaponItemId, 1, new Dictionary<string, dynamic>() { { "fingerprint" , iPlayer.GetName() } });

            iPlayer.Player.TriggerEvent("freezePlayer", false);
            iPlayer.SetData("userCannotInterrupt", false);
            NAPI.Player.StopPlayerAnimation(iPlayer.Player);
            return true;
        }
    }
}

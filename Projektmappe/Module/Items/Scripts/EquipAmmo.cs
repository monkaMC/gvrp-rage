using System;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using GVRP.Module.Chat;
using GVRP.Module.Logging;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Weapons.Data;
//Possible problem. Removed on use, but not possible to add without weapon. Readd item?
namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> EquipAmo(DbPlayer iPlayer, ItemModel ItemData, int Amount)
        {
            try
            {
                if (iPlayer.Player.IsInVehicle || !iPlayer.CanInteract()) return false;

                string[] parts = ItemData.Script.ToLower().Replace("ammo_", "").Split('_');
                string weaponstring = parts[0];

                WeaponData weaponData = WeaponDataModule.Instance.GetAll().Where(d => d.Value.Name.ToLower().Equals(weaponstring)).FirstOrDefault().Value;

                if (weaponData == null) return false;

                if (iPlayer.Weapons.Count == 0 || !iPlayer.Weapons.Exists(detail => detail.WeaponDataId == weaponData.Id))
                {
                    iPlayer.SendNewNotification(
                        "Sie können kein Magazin fuer eine nicht vorhandene Waffe ausruesten!");
                    return false;
                }

                var l_Details = iPlayer.Weapons.FirstOrDefault(detail => detail.WeaponDataId == weaponData.Id);
                iPlayer.SetData("no-packgun", true);
                iPlayer.SetCannotInteract(true);
                Chats.sendProgressBar(iPlayer, 800 * Amount);
                iPlayer.SetCannotInteract(false);
                iPlayer.Player.TriggerEvent("freezePlayer", true);

                iPlayer.Container.RemoveItem(ItemData, Amount);

                for (int i = 0; i < Amount; i++)
                {
                    iPlayer.PlayAnimation(
                        (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "anim@weapons@pistol@machine_str", "reload_aim");

                    await Task.Delay(800);
                    NAPI.Player.StopPlayerAnimation(iPlayer.Player);
                    var l_Ammo = l_Details.Ammo + 32;
                    iPlayer.Player.TriggerEvent("updateWeaponAmmo", l_Details.WeaponDataId, l_Ammo);
                    iPlayer.SetWeaponAmmo((WeaponHash)weaponData.Hash, l_Ammo);
                }
                iPlayer.Player.TriggerEvent("freezePlayer", false);
                iPlayer.SetData("no-packgun", false);
                iPlayer.SendNewNotification(
                    "Sie haben ein Magazin fuer Ihre Waffe ausgeruestet!");
                return true;
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }

            return false;
        }

        public static async Task<bool> EquipAmoCop(DbPlayer iPlayer, ItemModel ItemData, int Amount)
        {
            try
            {
                if (iPlayer.Player.IsInVehicle || !iPlayer.CanInteract()) return false;
                string weaponstring = ItemData.Script.ToLower().Replace("bammo_", "");

                string[] parts = ItemData.Script.ToLower().Replace("bammo_", "").Split('_');
                string weaponstringe = parts[0];

                if (!iPlayer.IsCopPackGun()) return false;

                WeaponData weaponData = WeaponDataModule.Instance.GetAll().Where(d => d.Value.Name.ToLower().Equals(weaponstring)).FirstOrDefault().Value;

                if (weaponData == null) return false;

                if (iPlayer.Weapons.Count == 0 || !iPlayer.Weapons.Exists(detail => detail.WeaponDataId == weaponData.Id))
                {
                    iPlayer.SendNewNotification(
                        "Sie können kein Magazin fuer eine nicht vorhandene Waffe ausruesten!");
                    return false;
                }

                var l_Details = iPlayer.Weapons.FirstOrDefault(detail => detail.WeaponDataId == weaponData.Id);
                iPlayer.SetData("no-packgun", true);
                iPlayer.SetCannotInteract(true);
                Chats.sendProgressBar(iPlayer, 1500 * Amount);
                iPlayer.SetCannotInteract(false);
                iPlayer.Player.TriggerEvent("freezePlayer", true);

                iPlayer.Container.RemoveItem(ItemData, Amount);
                for (int i = 0; i < Amount; i++)
                {
                    iPlayer.PlayAnimation(
                        (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "anim@weapons@pistol@machine_str", "reload_aim");

                    await Task.Delay(1500);
                    NAPI.Player.StopPlayerAnimation(iPlayer.Player);
                    var l_Ammo = l_Details.Ammo + 32;
                    iPlayer.Player.TriggerEvent("updateWeaponAmmo", l_Details.WeaponDataId, l_Ammo);
                    iPlayer.SetWeaponAmmo((WeaponHash)weaponData.Hash, l_Ammo);
                }
                iPlayer.Player.TriggerEvent("freezePlayer", false);
                iPlayer.SetData("no-packgun", false);
                iPlayer.SendNewNotification(
                    "Sie haben ein Magazin fuer Ihre Waffe ausgeruestet!");
                return true;
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }

            return false;
        }
    }
}

using System;
using System.Threading.Tasks;
using GTANetworkAPI;
using GVRP.Module.Players.Buffs;
using GVRP.Module.Players.Db;
using GVRP.Module.Items;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        private const int JointBuff = 20;
        private const int MaxArmor = 80;

        public static async Task<bool> joint(DbPlayer iPlayer, ItemModel ItemData)
        {
            if (iPlayer.Player.IsInVehicle)
            {
                return false;
            }

            bool antiinterrupt = false;
            if (iPlayer.HasData("disableAnim"))
            {
                antiinterrupt = Convert.ToBoolean(iPlayer.GetData("disableAnim"));
            }

            if (antiinterrupt)
            {
                return false;
            }
            
            iPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["joint_start"].Split()[0], Main.AnimationList["joint_start"].Split()[1]);
            iPlayer.Player.TriggerEvent("freezePlayer", true);
            await Task.Delay(8000);
            iPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["joint_end"].Split()[0], Main.AnimationList["joint_end"].Split()[1]);
            await Task.Delay(2500);
            iPlayer.Player.TriggerEvent("freezePlayer", false);
            NAPI.Player.StopPlayerAnimation(iPlayer.Player);
            int currentArmor = NAPI.Player.GetPlayerArmor(iPlayer.Player);
            int newArmor = 0;

            if (currentArmor >= MaxArmor)
                newArmor = currentArmor;
            else if (currentArmor >= MaxArmor - JointBuff)
                newArmor = MaxArmor;
            else
                newArmor = currentArmor + JointBuff;
            if (iPlayer.Container.GetItemAmount(ItemData.Id) <= 0)
                return false;
            NAPI.Player.SetPlayerArmor(iPlayer.Player, newArmor);
            iPlayer.Armor[0] = newArmor;

            ItemModelModule.Instance.LogItem((int)ItemData.Id, (int)iPlayer.TeamId, 1);
            

            return true;
        }
    }
}
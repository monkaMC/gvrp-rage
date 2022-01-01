using System;
using System.Threading.Tasks;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Drunk;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> Alk(DbPlayer iPlayer, ItemModel ItemData)
        {
            if (!iPlayer.CanInteract()) return false;

            iPlayer.PlayAnimation((int)(AnimationFlags.AllowPlayerControl | AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody),
                    "amb@world_human_drinking@coffee@male@idle_a",
                    "idle_a");

            iPlayer.SetCannotInteract(true);
            await Task.Delay(5000);
            iPlayer.SetCannotInteract(false);
            var level = Convert.ToInt32(ItemData.Script.Split("_")[1]);
            DrunkModule.Instance.IncreasePlayerAlkLevel(iPlayer, level);
            iPlayer.Player.StopAnimation();

            return true;
        }
    }
}

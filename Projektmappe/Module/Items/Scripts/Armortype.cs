using System.Threading.Tasks;
using GTANetworkAPI;
using GTANetworkMethods;
using GVRP.Module.Chat;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.PlayerAnimations;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> Armortype(DbPlayer dbPlayer, ItemModel itemModel)
        {
            if (dbPlayer.Player.IsInVehicle || !dbPlayer.CanInteract()) return false;

            if (!uint.TryParse(itemModel.Script.Split("_")[1], out uint type)) return false;

            if (!dbPlayer.IsACop() && type > 0)// && dbPlayer.Container.GetItemAmount(40) < 1)
            {
                //dbPlayer.SendNewNotification("Hierfür benötige ich noch eine weitere Schutzweste...");
                return false;
            }

            dbPlayer.IsInTask = true;
            Chats.sendProgressBar(dbPlayer, 4000);
            dbPlayer.PlayAnimation(
                    (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["fixing"].Split()[0], Main.AnimationList["fixing"].Split()[1]);
            dbPlayer.Player.TriggerEvent("freezePlayer", true);
            dbPlayer.SetCannotInteract(true);
            await System.Threading.Tasks.Task.Delay(4000);
            dbPlayer.SetCannotInteract(false);
            dbPlayer.Player.TriggerEvent("freezePlayer", false);
            NAPI.Player.StopPlayerAnimation(dbPlayer.Player);
            if (type != 1)
            {
                if (dbPlayer.VisibleArmorType != type)
                    dbPlayer.SaveArmorType(type);
                dbPlayer.VisibleArmorType = type;
            }
            dbPlayer.SetArmor(100, true);

            return true;
        }
    }
}

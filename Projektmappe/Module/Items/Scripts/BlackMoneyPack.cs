using System;
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
        public static async Task<bool> BlackMoneyPack(DbPlayer iPlayer, ItemModel ItemData, Item item, int slot)
        {
            if (iPlayer.Player.IsInVehicle || !iPlayer.CanInteract()) return false;

            int amount = item.Amount;

            Chats.sendProgressBar(iPlayer, 3000);

            // Remove
            iPlayer.Container.RemoveAllFromSlot(slot);

            iPlayer.Player.TriggerEvent("freezePlayer", true);
            
            iPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@prop_human_parking_meter@male@base", "base");

            iPlayer.SetData("userCannotInterrupt", true);

            await Task.Delay(3000);

            iPlayer.SetData("userCannotInterrupt", false);
            NAPI.Player.StopPlayerAnimation(iPlayer.Player);
           
            iPlayer.Player.TriggerEvent("freezePlayer", false);

            iPlayer.GiveBlackMoney(amount);
            iPlayer.SendNewNotification($"Sie haben ${amount} Schwarzgeld entpackt!");
            return true;
        }
    }
}

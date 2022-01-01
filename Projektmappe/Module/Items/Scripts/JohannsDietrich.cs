using GTANetworkAPI;
using System;
using System.Threading.Tasks;
using GVRP.Handler;
using GVRP.Module.Chat;
using GVRP.Module.Doors;
using GVRP.Module.GTAN;
using GVRP.Module.Houses;
using GVRP.Module.Injury;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.JumpPoints;
using GVRP.Module.Players.PlayerAnimations;
using GVRP.Module.Teams;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> JohannsDietrich(DbPlayer iPlayer, ItemModel ItemData)
        {
            // Check Door
            if (iPlayer.TryData("houseId", out uint houseId))
            {
                House house = HouseModule.Instance.Get(houseId);
                if (house != null)
                {
                    if (house.LastBreak.AddMinutes(10) > DateTime.Now) return false; // Bei einem Break, kann 5 min nicht interagiert werden
                    int breakTime = 300000;
                    if (Configurations.Configuration.Instance.DevMode)
                        breakTime = 5000;
                    Chats.sendProgressBar(iPlayer, breakTime);

                    DbPlayer owner = Players.Players.Instance.FindPlayer(house.OwnerId);
                    if(owner != null && owner.IsValid() && house.Price >= 100000)
                    {
                        owner.SendNewNotification($"Hookers House Guardian: Ein stiller Alarm an deinem Haus wurde ausgelöst.", title:"Hookers House Guardian", duration:30000);
                    }
                    if(house.Price >= 2500000)
                    {
                        if(owner != null)
                            TeamModule.Instance.SendChatMessageToDepartments($"Hookers House Guardian: Ein stiller Alarm am Haus {house.Id} von {owner.GetName()} wurde ausgelöst.");
                        else
                            TeamModule.Instance.SendChatMessageToDepartments($"Hookers House Guardian: Ein stiller Alarm am Haus {house.Id} wurde ausgelöst.");
                    }

                    iPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "missheistdockssetup1ig_3@talk", "oh_hey_vin_dockworker");
                    iPlayer.Player.TriggerEvent("freezePlayer", true);
                    iPlayer.SetData("userCannotInterrupt", true);

                    await Task.Delay(breakTime);
                    if (iPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(iPlayer.Player) || !iPlayer.Player.Exists) return false;
                    iPlayer.ResetData("userCannotInterrupt");

                    if (iPlayer.IsCuffed || iPlayer.IsTied || iPlayer.isInjured()) return true;

                    iPlayer.Player.TriggerEvent("freezePlayer", false);
                    house.Break();

                    iPlayer.SendNewNotification("Haus aufgebrochen!", notificationType: PlayerNotification.NotificationType.SUCCESS);
                    NAPI.Player.StopPlayerAnimation(iPlayer.Player);
                    return true;
                }
            }
            bool callBack = await Schweissgereat(iPlayer, ItemData);
            if (!callBack)
                iPlayer.SendNewNotification("Besonderer Dietrich zum Knacken von Schlössern.");
            return callBack;
        }
    }
}

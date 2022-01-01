using System;
using GTANetworkAPI;
using Newtonsoft.Json;
using GVRP.Module.Chat;
using GVRP.Module.Players.Db;
using GVRP.Module.ClientUI.Windows;
using GVRP.Module.RemoteEvents;
using GVRP.Module.Kasino;
using GVRP.Module.Logging;

namespace GVRP.Module.Players.Windows
{
    public class SlotMachineWindow : Window<Func<DbPlayer, KasinoDevice, bool>>
    {
        //Tables: SlotMachines, SlotMachineGames (mit state active)
        private readonly Random random = new Random();
        private class ShowEvent : Event
        {
            [JsonProperty(PropertyName = "machineId")] private uint Id { get; }
            [JsonProperty(PropertyName = "price")] private int Price { get; }
            [JsonProperty(PropertyName = "minprice")] private int MinPrice { get; }
            [JsonProperty(PropertyName = "maxprice")] private int MaxPrice { get; }
            [JsonProperty(PropertyName = "pricestep")] private int PriceStep { get; }
            [JsonProperty(PropertyName = "maxmultiple")] private int MaxMultiple { get; }

            public ShowEvent(DbPlayer dbPlayer, KasinoDevice kasinoDevice) : base(dbPlayer)
            {
                Id = kasinoDevice.Id;
                Price = kasinoDevice.Price;
                MinPrice = kasinoDevice.MinPrice;
                MaxPrice = kasinoDevice.MaxPrice;
                PriceStep = kasinoDevice.PriceStep;
                MaxMultiple = kasinoDevice.MaxMultiple;
            }
        }

        public SlotMachineWindow() : base("SlotMachine")
        {
        }

        public override Func<DbPlayer, KasinoDevice, bool> Show()
        {
            return (player, kasinoDevice) => OnShow(new ShowEvent(player, kasinoDevice));
        }


        [RemoteEvent]
        public void requestSlotInfo(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            TriggerEvent(dbPlayer.Player, "responseSlotInfo", JsonConvert.SerializeObject(KasinoModule.Instance.Factors));
        }


        [RemoteEvent]
        public void newSlotRoll(Client player, int moneyUsed)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (KasinoModule.Instance.CasinoGuests.Contains(dbPlayer) || dbPlayer.Rank.CanAccessFeature("casino"))
            {
                if (dbPlayer.TakeMoney(moneyUsed))
                {
                    SlotMachineGame slotMachineGame = KasinoModule.Instance.GenerateSlotMachineGame(dbPlayer, moneyUsed);
                    // dbPlayer.SendNewNotification(JsonConvert.SerializeObject(slotMachineGame), duration:9000);
                    SendGameResultToPlayer(dbPlayer, slotMachineGame);
                    return;
                }

                dbPlayer.SendNewNotification("Du hast nicht genug Geld dafür!");
            }
            else
            {
                dbPlayer.SendNewNotification("Das Casino hat geschlossen. Die Automaten wurden deaktiviert.");
            }


        }

        [RemoteEvent]
        public void leaveSlotMachine(Client player, int deviceId)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            KasinoDevice kasinoDevice = KasinoModule.Instance.Get((uint)deviceId);
            if (kasinoDevice == null) return;
            kasinoDevice.IsInUse = false;
            //dbPlayer.Freeze(false);
        }

        [RemoteEvent]
        public void cashoutSlotRoll(Client player, int id)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (KasinoModule.Instance.SlotMachineGames.TryGetValue(id, out SlotMachineGame slotMachineGame))
            {
                if (slotMachineGame.Status == Status.LOSE) return;
                slotMachineGame.WinSum = slotMachineGame.WinSum * slotMachineGame.Multiple;
                dbPlayer.GiveMoney(slotMachineGame.WinSum);
                dbPlayer.SendNewNotification($"{id} | Du hast gewonnen! | {slotMachineGame.WinSum}");
                Logger.AddSlotMachineGameToDbLog(dbPlayer, slotMachineGame);
                KasinoModule.Instance.SlotMachineGames.Remove(id);
            }   
        }

        [RemoteEvent]
        public void risikoCard(Client player, int number, int id)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            int rndnumber = random.Next(1, 3);
            if (KasinoModule.Instance.SlotMachineGames.TryGetValue(id, out SlotMachineGame slotMachineGame))
            {
                if (rndnumber == number)
                {
                    slotMachineGame.Multiple++;
                    TriggerEvent(player, "responseRisiko", rndnumber, 1);
                    return;
                }
                else
                {
                    slotMachineGame.Multiple = 0;
                    slotMachineGame.WinSum = 0;
                    slotMachineGame.Status = Status.LOSE;
                    Logger.AddSlotMachineGameToDbLog(dbPlayer, slotMachineGame);
                    TriggerEvent(player, "responseRisiko", rndnumber, 0);
                }
            }
        }

        public void SendGameResultToPlayer(DbPlayer dbPlayer, SlotMachineGame slotMachineGame)
        {
            TriggerEvent(dbPlayer.Player, "rollSlot", JsonConvert.SerializeObject(slotMachineGame));
        }
    }
}
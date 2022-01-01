using System;
using System.Collections.Generic;
using System.Linq;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Kasino;
using GVRP.Module.Logging;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Windows;

namespace GVRP.Module.Kasino
{
    public class KasinoModule : SqlModule<KasinoModule, KasinoDevice, uint>
    {
        private readonly Random random = new Random();

        private int GameId = 0;

        public float[,] Factors = new float[,]
        {
            {1.5f,  3,  250},
            {1,  1.5f,   30},
            {1,  1.5f,   15},
            {0,  1.5f,   10},
            {0,  1,       5},
            {0,  1,       2},
            {0,  0,       1},
            {0,  0,       0},
        };

        public Dictionary<int, SlotMachineGame> SlotMachineGames = new Dictionary<int, SlotMachineGame>();

        public List<DbPlayer> CasinoGuests = new List<DbPlayer>();

        protected override string GetQuery()
        {
            return "SELECT * FROM `kasino_devices`";
        }

        protected override void OnItemLoaded(KasinoDevice kasinoDevice)
        {
            //Temporär für die Testphase deaktiviert.

            PlayerNotifications.Instance.Add(kasinoDevice.Position, kasinoDevice.Type.ToString(), "Benutze E um zu Spielen");
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key != Key.E) return false;

            KasinoDevice kasinoDevice = GetClosest(dbPlayer);
            if (kasinoDevice == null) return false;

            if (!dbPlayer.Rank.CanAccessFeature("casino") && !CasinoGuests.Contains(dbPlayer)) return true;



            switch (kasinoDevice.Type)
            {
                case MachineType.SlotMachine:
                    ShowSlotMachine(dbPlayer, kasinoDevice);
                    break;
                case MachineType.Roulett:
                    break;
                case MachineType.Wheel:
                    break;
                default:
                    break;
            }

            return true;
        }

        private void ShowSlotMachine(DbPlayer dbPlayer, KasinoDevice kasinoDevice)
        {
            if (kasinoDevice.IsInUse)
            {
                dbPlayer.SendNewNotification("Diese Slot Machine wird bereits benutzt");
            }
            else
            {
                if(dbPlayer.money[0] < kasinoDevice.MinPrice*5)
                {
                    dbPlayer.SendNewNotification($"Sie benötigen mindestens ${kasinoDevice.MinPrice * 5} um hier zu spielen!");
                }
                ComponentManager.Get<SlotMachineWindow>().Show()(dbPlayer, kasinoDevice);
                kasinoDevice.IsInUse = true;
                //dbPlayer.Freeze(true);
            }
        }

        public SlotMachineGame GenerateSlotMachineGame(DbPlayer dbPlayer, int moneyUsed)
        {
            int multiple = 1;
            int slot1 = random.Next(1, 9);
            int slot2 = random.Next(1, 101);
            if (slot2 <= 27)
            {
                slot2 = slot1;
            }
            else
            {
                slot2 = random.Next(1, 9);
                while (slot2 == slot1)
                {
                    slot2 = random.Next(1, 9);
                }
            }
            int slot3 = random.Next(1, 101);
            if (slot3 <= 2)
            {
                slot3 = slot2;
            }
            else
            {
                slot3 = random.Next(1, 9);
                while (slot3 == slot2)
                {
                    slot3 = random.Next(1, 9);
                }
            }

            Status status = Status.LOSE;
            float winSum = calculateProfit(slot1, slot2, slot3, moneyUsed);

            if (winSum != 0.0f)
            {
                status = Status.WIN;
            }

            SlotMachineGame slotMachineGame = new SlotMachineGame()
            {
                Id = GameId,
                Einsatz = moneyUsed,
                KasinoDeviceId = 1,
                Slot1 = slot1,
                Slot2 = slot2,
                Slot3 = slot3,
                Status = status,
                WinSum = (int)winSum,
                Multiple = multiple
            };

            SlotMachineGames.Add(GameId, slotMachineGame);
            if (status == Status.LOSE)
            {
                Logger.AddSlotMachineGameToDbLog(dbPlayer, slotMachineGame);
            }
            GameId++;

            return slotMachineGame;
        }

        private float calculateProfit(int slot1, int slot2, int slot3, int einsatz)
        {
            float profit = 0f;
            float profitFactor = 0f;


            if (slot1 == slot2)
            {
                if (slot1 == slot3)
                {
                    profitFactor = Factors[slot1 - 1, 2];
                }
                else
                {
                    profitFactor = Factors[slot1 - 1, 1];
                }
            }
            else
            {
                profitFactor = Factors[slot1 - 1, 0];
            }
            profit = profitFactor * einsatz;
            return profit;
        }
        public KasinoDevice GetClosest(DbPlayer dbPlayer)
        {
            return GetAll().FirstOrDefault(kasinoDevice => kasinoDevice.Value.Position.DistanceTo(dbPlayer.Player.Position) < kasinoDevice.Value.Radius).Value;
        }
    }
}

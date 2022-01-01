using GTANetworkAPI;
using System;
using System.Linq;
using System.Threading.Tasks;
using GVRP.Module.Chat;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Houses;
using GVRP.Module.Items;
using GVRP.Module.NSA;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Windows;
using GVRP.Module.Robbery;
using GVRP.Module.Teams;

namespace GVRP.Module.Schwarzgeld
{
    public class SchwarzgeldModule : Module<SchwarzgeldModule>
    {
        public static uint SchwarzgeldId = 15;
        public static uint BankNotes = 880;
        public static uint Batteries = 44; // 45 stacksize

        public static Vector3 BlackMoneyInvPosition = new Vector3(1136.29, -3197.38, -39.6657);
        public static Vector3 BlackMoneyCodeContainer = new Vector3(1131.22, -3197.28, -39.6657);
        public static Vector3 BlackMoneyEndPoint = new Vector3(1126.07, -3197.11, -39.6657);
        public static Vector3 BlackMoneyBatteriePoint = new Vector3(1139, -3198.83, -39.6657);

        public static Vector3 MarkedDollarWorkbench = new Vector3(1117.24, -3196.7, -40.3974);

        public static int PercentBlackmoneyHouse = 85;

        public void SchwarzgeldContainerCheck()
        {
            if (!Configurations.Configuration.Instance.BlackMoneyEnabled)
                return;

            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                foreach (House house in HouseModule.Instance.GetAll().Values.Where(h => h.MoneyKeller == 1 && h.BlackMoneyInvContainer != null && h.BlackMoneyBatterieContainer != null && h.BlackMoneyCodeContainer != null))
                {
                    // Nur mit Codes möglich...
                    if (house.BlackMoneyCodeContainer.GetItemAmount(SchwarzgeldModule.BankNotes) <= 0) continue;

                    // Check BankNotes
                    if (house.BlackMoneyInvContainer.GetItemAmount(SchwarzgeldId) > 0)
                    {
                        //125.000 weil alle 5min gewaschen wird (30000)
                        var l_Amount = house.BlackMoneyInvContainer.GetItemAmount(SchwarzgeldId);
                        if (l_Amount > 30000)
                            l_Amount = 30000;

                        house.BlackMoneyInvContainer.RemoveItem(SchwarzgeldId, l_Amount);

                        int realmoney = (l_Amount / 100) * PercentBlackmoneyHouse; // Only % max from 100%

                        house.BLAmount += realmoney;
                        house.SaveBlackMoney();

                        // Intervall tick
                        if (!house.BlackMoneyBatterieContainer.IsEmpty() && house.BlackMoneyBatterieContainer.GetItemAmount(Batteries) >= 10) // 10 Batteries pro tick
                        {
                            house.BlackMoneyBatterieContainer.RemoveItem(Batteries, 10);
                            if (house.BlackMoneyTick > 0) house.BlackMoneyTick--;
                        }
                        else house.BlackMoneyTick++;

                        Random rnd = new Random();
                        int chance = rnd.Next(1, 1000);
                        if (chance > (995 - (house.BlackMoneyTick*50))) // 0,5% + 5% * tick
                        {
                            HousesVoltage housesVoltage = HousesVoltageModule.Instance.GetClosestFromPosition(house.Position);
                            if (housesVoltage != null)
                            {
                                // Add to Voltage for futureing stuff -- only show once
                                if (!housesVoltage.DetectedHouses.Contains(house.Id))
                                {
                                    housesVoltage.DetectedHouses.Add(house.Id);
                                    TeamModule.Instance.SendMessageToTeam($"Energie-Detection: Es wurde ein erhöhter Stromverbrauch gemeldet!", teams.TEAM_FIB, 10000, 3);
                                    NSAPlayerExtension.AddEnergyHistory($"Energieverbrauch Meldung (Stromkasten)", housesVoltage.Position);
                                }
                            }
                        }
                    }
                    else
                    {
                        // Wenn nix los, dann interval tick runter
                        if (house.BlackMoneyTick > 0) house.BlackMoneyTick--;
                    }
                }
            }));
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key != Key.E || dbPlayer.Player.IsInVehicle || dbPlayer.Player.Dimension == 0) return false;

            if (!Configurations.Configuration.Instance.BlackMoneyEnabled)
                return false;

            if (dbPlayer.DimensionType[0] == DimensionType.MoneyKeller)
            {
                House xHouse = HouseModule.Instance.Get((uint)dbPlayer.Player.Dimension);
                if (xHouse != null && (dbPlayer.HouseKeys.Contains(xHouse.Id) || dbPlayer.ownHouse[0] == xHouse.Id))
                {
                    // Blackmoney (Geldwäsche)
                    if (dbPlayer.Player.Position.DistanceTo(SchwarzgeldModule.BlackMoneyEndPoint) < 1.5f)
                    {
                        ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Gelddruckmaschine", Callback = "BlackmoneyWithdraw", Message = $"Aktueller Betrag {xHouse.BLAmount}$, wie viel möchten Sie entnehmen:" });
                        return true;
                    }

                    // Bank to normal geld
                    if (dbPlayer.Player.Position.DistanceTo(SchwarzgeldModule.MarkedDollarWorkbench) < 1.5f)
                    {
                        if(dbPlayer.Container.GetItemAmount(RobberyModule.MarkierteScheineID) <= 0)
                        {
                            return true;
                        }
                        
                        Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                        {
                            Chats.sendProgressBar(dbPlayer, 60000);

                            dbPlayer.Player.TriggerEvent("freezePlayer", true);
                            dbPlayer.SetData("userCannotInterrupt", true);
                            dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@prop_human_parking_meter@male@base", "base");

                            await Task.Delay(60000);

                            dbPlayer.SetData("userCannotInterrupt", false);
                            dbPlayer.Player.TriggerEvent("freezePlayer", false);
                            dbPlayer.ResetData("userCannotInterrupt");
                            NAPI.Player.StopPlayerAnimation(dbPlayer.Player);

                            int amount = dbPlayer.Container.GetItemAmount(RobberyModule.MarkierteScheineID);

                            dbPlayer.Container.RemoveItemAll(RobberyModule.MarkierteScheineID);
                            dbPlayer.GiveMoney(amount);

                            dbPlayer.SendNewNotification($"Du hast die Markierung der Scheine entfernt!");
                        }));
                        return true;
                    }
                }
            }
            
            return false;
        }
    }
    
    public enum SGLog
    {
        GIVEPLAYER, //Schwarzgeld to player
        TAKEPLAYER, //Schwarzgeld from player
        EXCHANGESG, //Schwarzgeld to echtgeld
        EXCHANGEEG //Echtgeld to Schwarzgeld
    }
}

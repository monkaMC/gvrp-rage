using System;
using System.Collections.Generic;
using GVRP.Module.Players.Db;
using Newtonsoft.Json;
using GTANetworkAPI;
using GVRP.Module.Players;
using GVRP.Module.Business;
using GVRP.Module.ClientUI.Windows;
using GVRP.Module.Teams.Shelter;
using GVRP.Module.GTAN;
using GVRP.Module.Tattoo;
using GVRP.Module.Gangwar;
using GVRP.Module.Players.Windows;
using GVRP.Module.Logging;
using GVRP.Module.NSA;
using GVRP.Module.Teams;

namespace GVRP.Module.Banks.Windows
{
    public class BankWindow : Window<Func<DbPlayer, string, string, int, int, int, List<BankHistory.BankHistory>, bool>>
    {
        private class ShowEvent : Event
        {
            [JsonProperty(PropertyName = "title")] private string Title { get; }

            [JsonProperty(PropertyName = "balance")]
            private int Balance { get; }

            [JsonProperty(PropertyName = "history")]
            private List<BankHistory.BankHistory> History { get; }

            [JsonProperty(PropertyName = "playername")]
            private string Playername { get; }

            [JsonProperty(PropertyName = "money")]
            private int Money { get; }

            [JsonProperty(PropertyName = "type")]
            private int Type { get; }

            public ShowEvent(DbPlayer dbPlayer, string title, string playername, int money, int balance, int type, List<BankHistory.BankHistory> history) :
                base(dbPlayer)
            {
                Title = title;
                Playername = playername;
                Money = money;
                Balance = balance;
                Type = type;
                History = history;
            }
        }

        public BankWindow() : base("Bank")
        {
        }

        public override Func<DbPlayer, string, string, int, int, int, List<BankHistory.BankHistory>, bool> Show()
        {
            return (player, title, playername, money, balance, type, history) => OnShow(new ShowEvent(player, title, playername, money, balance, type, history));
        }

        [RemoteEvent]
        public void bankPayout(Client player, int balance)
        {
            BankTransaction(player, 0, balance);
        }

        [RemoteEvent]
        public void bankDeposit(Client player, int balance)
        {
            BankTransaction(player, balance, 0);
        }

        [RemoteEvent]
        public void bankTransfer(Client player, int amount, string target)
        {
            var iPlayer = player.GetPlayer();
            if (iPlayer == null || !iPlayer.IsValid()) return;

            // BusinessBank
            switch (iPlayer.DimensionType[0])
            {
                case DimensionType.Business:
                    iPlayer.SendNewNotification("Funktion nicht moeglich!");
                    break;
                default:
                    // Tattostudio
                    if (iPlayer.TryData("tattooShopId", out uint tattooShopId))
                    {
                        var tattooShop = TattooShopModule.Instance.Get(tattooShopId);
                        if (tattooShop != null)
                        {
                            iPlayer.SendNewNotification("Funktion nicht moeglich!");
                            return;
                        }
                    }

                    // TeamShelter
                    if (iPlayer.TryData("teamShelterMenuId", out uint teamshelterId))
                    {
                        var shelter = TeamShelterModule.Instance.Get(teamshelterId);
                        if (shelter != null)
                        {
                            iPlayer.SendNewNotification("Funktion nicht moeglich!");
                            return;
                        }
                    }

                    // Gangwar Town
                    GangwarTown gangwarTown = GangwarTownModule.Instance.Get(iPlayer.Player.Dimension);
                    if (gangwarTown != null && gangwarTown.OwnerTeam.Id == iPlayer.TeamId && iPlayer.Player.Position.DistanceTo(GangwarTownModule.BankPosition) < 1.5f)
                    {
                        iPlayer.SendNewNotification("Funktion nicht moeglich!");
                        return;
                    }

                    DbPlayer targetPlayer = Players.Players.Instance.FindPlayer(target);

                    if (targetPlayer != null && targetPlayer.IsValid() && targetPlayer != iPlayer)
                    {
                        if (amount <= 0) return;
                        if (iPlayer.TakeBankMoney(amount))
                        {
                            targetPlayer.GiveBankMoney(amount);
                            iPlayer.SendNewNotification(
                                "Sie haben " + amount + "$ an " + targetPlayer.GetName() + " ueberwiesen.");
                            targetPlayer.SendNewNotification(
                                iPlayer.GetName() + " hat ihnen " + amount + "$ ueberwiesen.");

                            // Bankhistory
                            targetPlayer.AddPlayerBankHistory(amount, "Ueberweisung von " + iPlayer.Player.Name);
                            iPlayer.AddPlayerBankHistory(-amount, "Ueberweisung an " + targetPlayer.Player.Name);
                            GiveMoneyWindow.SaveToPayLog(iPlayer.Id.ToString(), targetPlayer.Id.ToString(), amount, TransferType.ÜBERWEISUNG);
                            return;
                        }
                    }
                    else
                    {

                        iPlayer.SendNewNotification("Spieler nicht gefunden!");
                        return;
                    }
                    break;
            }
        }

        public void BankTransaction(Client player, int einzahlen, int auszahlen)
        {
            var iPlayer = player.GetPlayer();
            if (iPlayer == null || !iPlayer.IsValid()) return;


            // Get Type of Bank

            // BusinessBank
            switch (iPlayer.DimensionType[0])
            {
                case DimensionType.Business:
                    var biz = BusinessModule.Instance.GetById((uint)iPlayer.Player.Dimension);
                    if (biz == null || iPlayer.ActiveBusiness?.Id != biz.Id) return;
                    if (auszahlen > 0 && auszahlen <= biz.Money)
                    {
                        if (iPlayer.GetActiveBusinessMember() == null ||
                            iPlayer.GetActiveBusinessMember()?.Money == false) return;
                        biz.Disburse(iPlayer, auszahlen);

                        Logging.Logger.SaveToBusinessBank(biz.Id, auszahlen, iPlayer.Id, iPlayer.GetName(), false);
                        biz.AddBankHistory(-auszahlen, $"Auszahlung von {iPlayer.GetName()}");
                    }

                    if (einzahlen > 0 && einzahlen <= iPlayer.money[0])
                    {
                        biz.Deposite(iPlayer, einzahlen);
                        Logging.Logger.SaveToBusinessBank(biz.Id, einzahlen, iPlayer.Id, iPlayer.GetName(), true);
                        biz.AddBankHistory(einzahlen, $"Einzahlung von {iPlayer.GetName()}");
                    }

                    break;
                default:

                    // TeamShelter
                    if (iPlayer.TryData("teamShelterMenuId", out uint teamshelterId))
                    {
                        var teamShelter = TeamShelterModule.Instance.Get(teamshelterId);
                        if (teamShelter != null)
                        {
                            if (teamShelter == null || iPlayer.TeamId != teamShelter.Team.Id) return;

                            if (iPlayer.HasData("swbank"))
                            {
                                if (auszahlen > 0 && auszahlen <= iPlayer.blackmoneybank[0])
                                {
                                    iPlayer.TakeBlackMoneyBank(auszahlen);
                                    iPlayer.GiveBlackMoney(auszahlen);

                                    iPlayer.SendNewNotification(
                                        "Sie haben " + auszahlen + "$ von Ihrem Schwarzgeldkonto abgehoben.", title: "Schwarzgeldkonto", notificationType: PlayerNotification.NotificationType.ERROR);
                                }

                                if (einzahlen > 0 && einzahlen <= iPlayer.blackmoney[0])
                                {
                                    if (iPlayer.blackmoneybank[0] >= 3000000)
                                    {
                                        iPlayer.SendNewNotification("Ihr Schwarzgeldkonto kann maximal 3 Millionen fassen!");
                                        return;
                                    }

                                    int diff = 3000000 - iPlayer.blackmoneybank[0];
                                    if (einzahlen > diff) einzahlen = diff;

                                    iPlayer.GiveBlackMoneyBank(einzahlen);
                                    iPlayer.TakeBlackMoney(einzahlen);

                                    iPlayer.SendNewNotification(
                                        "Sie haben " + einzahlen + "$ auf Ihr Schwarzgeldkonto eingezahlt.", title: "Schwarzgeldkonto", notificationType: PlayerNotification.NotificationType.SUCCESS);
                                }
                                return;
                            }
                            else
                            {
                                if (auszahlen > 0 && auszahlen <= teamShelter.Money)
                                {
                                    if (!iPlayer.TeamRankPermission.Bank)
                                    {
                                        return;
                                    }

                                    teamShelter.Disburse(iPlayer, auszahlen);
                                    iPlayer.SendNewNotification(
                                        "Sie haben " + auszahlen + "$ von Ihrem Fraktionskonto abgehoben.", title: "Fraktionskasse", notificationType: PlayerNotification.NotificationType.ERROR);
                                    Logging.Logger.SaveToFbankLog(iPlayer.TeamId, auszahlen, iPlayer.Id, iPlayer.GetName(), false);
                                }

                                if (einzahlen > 0 && einzahlen <= iPlayer.money[0])
                                {
                                    teamShelter.Deposit(iPlayer, einzahlen);
                                    iPlayer.SendNewNotification(
                                        "Sie haben " + einzahlen + "$ auf Ihr Fraktionskonto eingezahlt.", title: "Fraktionskasse", notificationType: PlayerNotification.NotificationType.SUCCESS);
                                    Logging.Logger.SaveToFbankLog(iPlayer.TeamId, einzahlen, iPlayer.Id, iPlayer.GetName(), true);
                                }
                                return;
                            }
                        }
                    }

                    // Tattostudio
                    if (iPlayer.TryData("tattooShopId", out uint tattooShopId))
                    {
                        var tattooShop = TattooShopModule.Instance.Get(tattooShopId);
                        if (tattooShop != null)
                        {
                            if (!iPlayer.IsMemberOfBusiness() || !iPlayer.GetActiveBusinessMember().Manage || iPlayer.GetActiveBusinessMember().BusinessId != tattooShop.BusinessId) return;

                            if (einzahlen > 0)
                            {
                                iPlayer.SendNewNotification("Nur Auszahlungen moeglich!");
                                return;
                            }

                            if (tattooShop.Bank < auszahlen)
                            {
                                iPlayer.SendNewNotification("Konto nicht genügend gedeckt!");
                                return;
                            }

                            tattooShop.MinusBank(auszahlen);
                            Logger.AddTattoShopLog(tattooShop.Id, iPlayer.Id, auszahlen, false);
                            iPlayer.GiveMoney(auszahlen);

                            biz = BusinessModule.Instance.GetById((uint)tattooShop.BusinessId);
                            if (biz == null || iPlayer.ActiveBusiness?.Id != biz.Id) return;

                            biz.AddBankHistory(-auszahlen, $"Auszahlung von {iPlayer.GetName()}");

                            return;
                        }
                    }

                    // Gangwar Town
                    GangwarTown gangwarTown = GangwarTownModule.Instance.Get(iPlayer.Player.Dimension);
                    if (gangwarTown != null && gangwarTown.OwnerTeam.Id == iPlayer.TeamId && iPlayer.Player.Position.DistanceTo(GangwarTownModule.BankPosition) < 1.5f)
                    {
                        if (auszahlen > 0 && auszahlen <= gangwarTown.Cash)
                        {
                            if (!iPlayer.TeamRankPermission.Bank)
                            {
                                return;
                            }

                            gangwarTown.SetCash(gangwarTown.Cash - auszahlen);
                            iPlayer.GiveMoney(auszahlen);
                            iPlayer.SendNewNotification(
                                "Sie haben " + auszahlen + "$ von Ihrer Gangwarkasse abgehoben.", title: "Gangwarkasse", notificationType: PlayerNotification.NotificationType.SUCCESS);
                            Logging.Logger.SaveToFbankLog(iPlayer.TeamId, auszahlen, iPlayer.Id, iPlayer.GetName() + " gw", false);

                            return;
                        }

                        if (einzahlen > 0)
                        {
                            iPlayer.SendNewNotification("Nur Auszahlungen moeglich!", title: "Gangwarkasse", notificationType: PlayerNotification.NotificationType.SERVER);
                            return;
                        }
                    }
                    if (iPlayer.TryData("bankId", out uint bankId))
                    {

                        Bank bank = BankModule.Instance.Get(bankId);

                        if (!bank.CanMoneyWithdrawn(auszahlen))
                        {
                            iPlayer.SendNewNotification("Dieser Bankautomat verfügt aktuell leider nicht über diese Geldsumme.", title: "Bankautomat");
                            return;
                        }

                        if (auszahlen > 0 && auszahlen <= iPlayer.bank_money[0])
                        {
                            if (iPlayer.TakeBankMoney(auszahlen))
                            {
                                iPlayer.GiveMoney(auszahlen);
                                iPlayer.AddPlayerBankHistory(-auszahlen, $"Geldtransfer ({bank.Name}) - Auszahlung");
                                bank.WithdrawMoney(auszahlen);
                                bank.SaveActMoneyToDb();
                                iPlayer.SendNewNotification(
                                    "Sie haben " + auszahlen + "$ von Ihrem Konto abgehoben.", title: "Konto", notificationType: PlayerNotification.NotificationType.SUCCESS);

                                if (iPlayer.HasMoneyTransferWantedStatus())
                                {
                                    TeamModule.Instance.SendMessageToTeam($"Finanz-Detection: Die Gesuchte Person {iPlayer.GetName()} hat eine Auszahlung von ${auszahlen} getätigt! (Standort: {bank.Name})", teams.TEAM_FIB, 10000, 3);
                                    NSAPlayerExtension.AddTransferHistory($"{iPlayer.GetName()} Auszahlung {bank.Name}", bank.Position);
                                }
                            }
                        }

                        if (!bank.CanMoneyDeposited(einzahlen))
                        {
                            iPlayer.SendNewNotification("Der Speicher dieses Bankautomaten kann diese Geldsumme leider nicht mehr aufnehmen.", title: "Bankautomat");
                            return;
                        }

                        if (einzahlen > 0 && einzahlen <= iPlayer.money[0])
                        {
                            if (iPlayer.GiveBankMoney(einzahlen))
                            {
                                iPlayer.TakeMoney(einzahlen);
                                iPlayer.AddPlayerBankHistory(einzahlen, $"Geldtransfer ({bank.Name}) - Einzahlung");
                                bank.DepositMoney(einzahlen);
                                bank.SaveActMoneyToDb();
                                iPlayer.SendNewNotification(
                                    "Sie haben " + einzahlen + "$ auf Ihr Konto eingezahlt.", title: "Konto", notificationType: PlayerNotification.NotificationType.SUCCESS);

                                if (iPlayer.HasMoneyTransferWantedStatus())
                                {
                                    TeamModule.Instance.SendMessageToTeam($"Finanz-Detection: Die Gesuchte Person {iPlayer.GetName()} hat eine Einzahlung von ${einzahlen} getätigt! (Standort: {bank.Name})", teams.TEAM_FIB, 10000, 3);
                                    NSAPlayerExtension.AddTransferHistory($"{iPlayer.GetName()} Einzahlung {bank.Name}", bank.Position);
                                }
                            }
                        }
                    }
                    break;
            }
            return;
        }
    }
}
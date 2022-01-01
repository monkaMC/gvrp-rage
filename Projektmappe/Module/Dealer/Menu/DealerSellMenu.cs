using System;
using System.Collections.Generic;
using GVRP.Module.Configurations;
using GVRP.Module.Items;
using GVRP.Module.Logging;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Teams;
using GVRP.Module.Teams.Shelter;

namespace GVRP.Module.Dealer.Menu
{
    public class DealerSellMenu : MenuBuilder
    {
        public const int MaxGangwarTownBonus = 4;

        public DealerSellMenu() : base(PlayerMenu.DealerSellMenu)
        {
        }

        public override Module.Menu.Menu Build(DbPlayer iPlayer)
        {
            if (!iPlayer.HasData("current_dealer")) return null;

            Dealer l_Dealer = DealerModule.Instance.Get(iPlayer.GetData("current_dealer"));
            if (l_Dealer == null)
                return null;

            var l_Menu = new Module.Menu.Menu(Menu, "Dealer", l_Dealer.Note);
            l_Menu.Add("Schließen", "");
            l_Menu.Add($"V | Meth ({l_Dealer.MethResource.Price.ToString()} $ pro Kristall)", "");
            l_Menu.Add($"V | Kiste Meth (~ {l_Dealer.MethResource.PurePrice.ToString()} $ pro Kristall)", "");
            l_Menu.Add($"V | Waffenset (~ {(l_Dealer.WeaponResource.Price * 50).ToString()} $)", "");
            l_Menu.Add($"V | Paket Cannabis (~ {l_Dealer.CannabisResource.Price.ToString()} $ pro Gramm)", "");
            l_Menu.Add($"V | Goldbarren ({l_Dealer.GoldResource.Price.ToString()} $)", "");
            l_Menu.Add($"V | Juwelen ({l_Dealer.DiamondResource.Price.ToString()} $)", "");
            l_Menu.Add($"K | Wegfahrsperre ({l_Dealer.VehicleClawPrice.ToString()} $)", "");

            return l_Menu;
        }

        public override IMenuEventHandler GetEventHandler()
        {
            return new EventHandler();
        }

        private class EventHandler : IMenuEventHandler
        {
            public bool OnSelect(int index, DbPlayer dbPlayer)
            {
                if (!dbPlayer.HasData("current_dealer")) return false;

                Dealer l_Dealer = DealerModule.Instance.Get(dbPlayer.GetData("current_dealer"));
                if (l_Dealer == null) return false;

                bool soldSmth = false;

                switch (index)
                {
                    case 0:
                        break;
                    case 1: // Meth
                        uint l_PricePerMeth = l_Dealer.MethResource.Price;
                        uint l_MethAmount = (uint)dbPlayer.Container.GetItemAmount(DealerModule.Instance.MethItemId);

                        if (l_MethAmount <= 0)
                        {
                            dbPlayer.SendNewNotification("Du hast kein Meth dabei, welches du mir verkaufen könntest!");
                            break;
                        }

                        if (l_Dealer.MethCristallResource.IsFull())
                        {
                            dbPlayer.SendNewNotification($"Ich nimm erstmal nix an von dem Zeug. Komm später wieder.");
                            break;
                        }

                        uint LeftToSellAmount = l_Dealer.MethCristallResource.GetAvailableAmountToSell();
                        if (LeftToSellAmount < l_MethAmount)
                        {
                            dbPlayer.SendNewNotification($"Alter... Das is zu viel. Ich kann nur noch {l_Dealer.MethCristallResource.GetAvailableAmountToSell().ToString()} annehmen...");
                            break;
                        }

                        int l_MethPrice         = Convert.ToInt32(l_MethAmount * l_PricePerMeth);
                        int l_MethFBank         = Convert.ToInt32(l_MethPrice * 0.05f);
                        int l_PlayerMethPrice   = Convert.ToInt32(l_MethPrice * 0.95f);

                        dbPlayer.Container.RemoveItem(DealerModule.Instance.MethItemId, (int)l_MethAmount);

                        dbPlayer.GiveBlackMoney(l_PlayerMethPrice);
                        TeamShelterModule.Instance.Get(dbPlayer.Team.Id).GiveMoney(l_MethFBank);
                        dbPlayer.SendNewNotification($"Du hast {l_MethAmount.ToString()} Meth für {l_MethPrice.ToString()}$ verkauft." + $"Es gingen 5% an die Fraktion. ({l_MethFBank.ToString()}$)");
                        l_Dealer.MethCristallResource.Sold += l_MethAmount;
                        soldSmth = true;
                        Logger.AddGangwarSellToDB(dbPlayer.Id, DealerModule.Instance.MethItemId, (int)l_MethAmount, l_PlayerMethPrice);

                        if (l_Dealer.MethCristallResource.IsFull())
                            l_Dealer.MethCristallResource.TimeSinceFull = DateTime.Now;

                        break;
                    case 2: // Kiste Meth
                        uint l_PricePerPureMeth = l_Dealer.MethResource.PurePrice;
                        Item item = dbPlayer.Container.GetItemById(726);
                        if (item == null)
                            item = dbPlayer.Container.GetItemById(727);
                        if (item == null)
                            item = dbPlayer.Container.GetItemById(728);
                        if (item == null)
                            item = dbPlayer.Container.GetItemById(729);

                        if (item == null)
                        {
                            dbPlayer.SendNewNotification("Du hast keine Kiste mit Methamphetamin dabei!");
                            break;
                        }

                        if (l_Dealer.MethResource.IsFull())
                        {
                            dbPlayer.SendNewNotification($"Ich nimm erstmal nix an von dem Zeug. Komm später wieder.");
                            break;
                        }

                        uint amount = 0;
                        double quality = 1;
                        foreach (KeyValuePair<string, dynamic> keyValuePair in item.Data)
                        {
                            if (keyValuePair.Key == "amount")
                            {
                                string amount_str = Convert.ToString(keyValuePair.Value);
                                amount = Convert.ToUInt32(amount_str);
                            }
                           /* else if (keyValuePair.Key == "quality")
                            {
                                string quality_str = Convert.ToString(keyValuePair.Value);
                                quality = Convert.ToDouble(quality_str);*/
                            //}

                        }

                       if (amount == 0)// || quality == 0)
                        {
                            dbPlayer.SendNewNotification("Mit dieser Kiste scheint etwas nicht korrekt zu sein.");
                            break;
                        }
                      
                        if (l_Dealer.MethResource.GetAvailableAmountToSell() < amount)
                        {
                            dbPlayer.SendNewNotification($"Alter... Das is zu viel. Ich kann nur noch {l_Dealer.MethResource.GetAvailableAmountToSell().ToString()} annehmen...");
                            break;
                        }

                        double offset = (1 - 0.90) * 100;
                        l_PricePerPureMeth += (uint)offset;
                        
                        int l_PureMethPrice = Convert.ToInt32(amount * l_PricePerPureMeth);
                        int l_PureMethFBank = Convert.ToInt32(l_PureMethPrice * 0.05f);
                        int l_PlayerPureMethPrice = Convert.ToInt32(l_PureMethPrice * 0.95f);

                        dbPlayer.Container.RemoveItem(item.Model);

                        dbPlayer.GiveBlackMoney(l_PlayerPureMethPrice);
                        TeamShelterModule.Instance.Get(dbPlayer.Team.Id).GiveMoney(l_PureMethFBank);
                        dbPlayer.SendNewNotification($"Du hast {amount.ToString()} reines Meth für {l_PureMethPrice.ToString()}$ verkauft." + $"Es gingen 5% an die Fraktion. ({l_PureMethFBank.ToString()}$)");
                        l_Dealer.MethResource.Sold += amount;
                        soldSmth = true;
                        Logger.AddGangwarSellToDB(dbPlayer.Id, item.Id, (int)amount, l_PlayerPureMethPrice);

                        if (l_Dealer.MethResource.IsFull())
                            l_Dealer.MethResource.TimeSinceFull = DateTime.Now;

                        break;
                    case 3: // Waffenset
                        uint priceWeaponSet = l_Dealer.WeaponResource.Price * 50;

                        item = dbPlayer.Container.GetItemById(976);
                        if (item == null)
                        {
                            dbPlayer.SendNewNotification("Du hast keine Waffensets dabei!");
                            break;
                        }

                        if (l_Dealer.WeaponResource.IsFull())
                        {
                            dbPlayer.SendNewNotification($"Ich nimm erstmal nix an von dem Zeug. Komm später wieder.");
                            break;
                        }

                        int fBankRevard = Convert.ToInt32(priceWeaponSet * 0.05);

                        dbPlayer.Container.RemoveItem(item.Model);

                        dbPlayer.GiveBlackMoney((int)priceWeaponSet);
                        TeamShelterModule.Instance.Get(dbPlayer.Team.Id).GiveMoney(fBankRevard);
                        dbPlayer.SendNewNotification($"Du hast ein Waffenset für {priceWeaponSet}$ verkauft." + $"Es gingen 5% an die Fraktion. ({fBankRevard}$)");
                        l_Dealer.WeaponResource.Sold++;
                        soldSmth = true;
                        Logger.AddGangwarSellToDB(dbPlayer.Id, item.Id, 1, (int)priceWeaponSet);

                        if (l_Dealer.WeaponResource.IsFull())
                            l_Dealer.WeaponResource.TimeSinceFull = DateTime.Now;

                        break;
                    case 4: // Paket Cannabis
                        l_PricePerPureMeth = l_Dealer.CannabisResource.Price;
                        Item xitem = dbPlayer.Container.GetItemById(983);
                        if (xitem == null)
                            xitem = dbPlayer.Container.GetItemById(982);
                        if (xitem == null)
                            xitem = dbPlayer.Container.GetItemById(981);
                        if (xitem == null)
                            xitem = dbPlayer.Container.GetItemById(980);

                        if (xitem == null)
                        {
                            dbPlayer.SendNewNotification("Du hast keine Paket mit Cannabis dabei!");
                            break;
                        }

                        if (l_Dealer.CannabisResource.IsFull())
                        {
                            dbPlayer.SendNewNotification($"Ich nimm erstmal nix an von dem Zeug. Komm später wieder.");
                            break;
                        }

                        amount = 0;
                        quality = 1;

                        foreach (KeyValuePair<string, dynamic> keyValuePair in xitem.Data)
                        {
                            if (keyValuePair.Key == "amount")
                            {
                                string amount_str = Convert.ToString(keyValuePair.Value);
                                amount = Convert.ToUInt32   (amount_str);
                            }
                           /* else if (keyValuePair.Key == "quality")
                            {
                                string quality_str = Convert.ToString(keyValuePair.Value);
                                quality = Convert.ToDouble(quality_str);
                            }*/

                        }

                        if (amount == 0)// || quality == 0)
                        {
                            dbPlayer.SendNewNotification("Mit diesem Paket scheint etwas nicht korrekt zu sein.");
                            break;
                        }

                        if (l_Dealer.CannabisResource.GetAvailableAmountToSell() < amount)
                        {
                            dbPlayer.SendNewNotification($"Alter... Das is zu viel. Ich kann nur noch {l_Dealer.CannabisResource.GetAvailableAmountToSell().ToString()} annehmen...");
                            break;
                        }

                        offset = (1 - 0.90) * 100;
                        l_PricePerPureMeth += (uint)offset;

                        l_PureMethPrice = Convert.ToInt32(amount * l_PricePerPureMeth);
                        l_PureMethFBank = Convert.ToInt32(l_PureMethPrice * 0.05f);
                        l_PlayerPureMethPrice = Convert.ToInt32(l_PureMethPrice * 0.95f);

                        dbPlayer.Container.RemoveItem(xitem.Model);

                        dbPlayer.GiveBlackMoney(l_PlayerPureMethPrice);
                        TeamShelterModule.Instance.Get(dbPlayer.Team.Id).GiveMoney(l_PureMethFBank);
                        dbPlayer.SendNewNotification($"Du hast {amount.ToString()} Cannabis für {l_PureMethPrice.ToString()}$ verkauft." + $"Es gingen 5% an die Fraktion. ({l_PureMethFBank.ToString()}$)");
                        l_Dealer.CannabisResource.Sold += amount;
                        soldSmth = true;
                        Logger.AddGangwarSellToDB(dbPlayer.Id, xitem.Id, (int)amount, l_PlayerPureMethPrice);

                        if (l_Dealer.CannabisResource.IsFull())
                            l_Dealer.CannabisResource.TimeSinceFull = DateTime.Now;

                        break;
                    case 5: //Gold
                        uint l_PricePerGold = l_Dealer.GoldResource.Price;
                        uint l_GoldAmount = (uint)dbPlayer.Container.GetItemAmount(DealerModule.Instance.GoldBarrenItemId);

                        if (l_GoldAmount <= 0)
                        {
                            dbPlayer.SendNewNotification("Du hast keine Goldbarren dabei, welche du mir verkaufen könntest!");
                            break;
                        }

                        if (l_Dealer.GoldResource.IsFull())
                        {
                            dbPlayer.SendNewNotification($"Ich nimm erstmal nix an von dem Zeug. Komm später wieder.");
                            break;
                        }

                        if (l_Dealer.GoldResource.GetAvailableAmountToSell() < l_GoldAmount)
                        {
                            dbPlayer.SendNewNotification($"Alter... Das is zu viel. Ich kann nur noch {l_Dealer.GoldResource.GetAvailableAmountToSell().ToString()} annehmen...");
                            break;
                        }

                        int l_GoldPrice = Convert.ToInt32(l_GoldAmount * l_PricePerGold);
                        int l_GoldFBank = Convert.ToInt32(l_GoldPrice * 0.05f);
                        int l_PlayerGoldPrice = Convert.ToInt32(l_GoldPrice * 0.95f);

                        dbPlayer.Container.RemoveItem(DealerModule.Instance.GoldBarrenItemId, (int)l_GoldAmount);

                        dbPlayer.GiveBlackMoney(l_PlayerGoldPrice);
                        TeamShelterModule.Instance.Get(dbPlayer.Team.Id).GiveMoney(l_GoldFBank);
                        dbPlayer.SendNewNotification($"Du hast {l_GoldAmount.ToString()} Goldbarren für {l_GoldPrice.ToString()}$ verkauft. Es gingen 5% an die Fraktion. ({l_GoldFBank.ToString()}$)");
                        l_Dealer.GoldResource.Sold += l_GoldAmount;
                        soldSmth = true;
                        Logger.AddGangwarSellToDB(dbPlayer.Id, DealerModule.Instance.GoldBarrenItemId, (int)l_GoldAmount, l_PlayerGoldPrice);

                        if (l_Dealer.GoldResource.IsFull())
                            l_Dealer.GoldResource.TimeSinceFull = DateTime.Now;

                        break;
                    case 6: // Juwelen
                        uint l_PricePerDiamond = l_Dealer.DiamondResource.Price;
                        uint l_DiamondAmount = (uint)dbPlayer.Container.GetItemAmount(DealerModule.Instance.DiamondItemId);

                        if (l_DiamondAmount <= 0)
                        {
                            dbPlayer.SendNewNotification("Du hast keine Diamanten dabei, welche du mir verkaufen könntest!");
                            break;
                        }

                        if (l_Dealer.DiamondResource.IsFull())
                        {
                            dbPlayer.SendNewNotification($"Ich nimm erstmal nix an von dem Zeug. Komm später wieder.");
                            break;
                        }

                        if (l_Dealer.DiamondResource.GetAvailableAmountToSell() < l_DiamondAmount)
                        {
                            dbPlayer.SendNewNotification($"Alter... Das is zu viel. Ich kann nur noch {l_Dealer.DiamondResource.GetAvailableAmountToSell().ToString()} annehmen...");
                            break;
                        }

                        int l_DiamondPrice = Convert.ToInt32(l_DiamondAmount * l_PricePerDiamond);
                        int l_DiamondFBank = Convert.ToInt32(l_DiamondPrice * 0.05f);
                        int l_PlayerDiamondPrice = Convert.ToInt32(l_DiamondPrice * 0.95f);

                        dbPlayer.Container.RemoveItem(DealerModule.Instance.DiamondItemId, (int)l_DiamondAmount);

                        dbPlayer.GiveBlackMoney(l_PlayerDiamondPrice);
                        TeamShelterModule.Instance.Get(dbPlayer.Team.Id).GiveMoney(l_DiamondFBank);
                        dbPlayer.SendNewNotification($"Du hast {l_DiamondAmount.ToString()} Juwelen für {l_DiamondPrice.ToString()}$ verkauft. Es gingen 5% an die Fraktion. ({l_DiamondFBank.ToString()}$)");
                        l_Dealer.DiamondResource.Sold += l_DiamondAmount;
                        soldSmth = true;
                        Logger.AddGangwarSellToDB(dbPlayer.Id, DealerModule.Instance.DiamondItemId, (int)l_DiamondAmount, l_PlayerDiamondPrice);

                        if (l_Dealer.DiamondResource.IsFull())
                            l_Dealer.DiamondResource.TimeSinceFull = DateTime.Now;

                        break;
                    case 7: //Wegfahrsperre

                        if (l_Dealer.VehicleClaw)
                        {
                            if (l_Dealer.VehicleClawBought < DealerModule.Instance.MaxVehicleClawAmount)
                            {
                                if (dbPlayer.TakeMoney(100000))
                                {
                                    dbPlayer.Container.AddItem(732, 1);
                                    dbPlayer.SendNewNotification("Du hast eine Wegfahrsperre gekauft.. Lass dich nicht erwischen!");
                                    l_Dealer.VehicleClawBought++;
                                    break;
                                }
                                else
                                {
                                    dbPlayer.SendNewNotification("Du hast nicht genug Geld dabei.");
                                    break;
                                }
                            }
                            else
                            {
                                dbPlayer.SendNewNotification("Ich habe bereits alle verkauft...");
                                break;
                            }
                        }
                        else
                        {
                            dbPlayer.SendNewNotification("Ich habe leider aktuell keine auf Vorrat...");
                        }
                        break;

                }

                if (soldSmth)
                {
                    double rnd = Utils.RandomDoubleNumber(0, 100);
                    if (rnd <= DealerModule.Instance.MaulwurfAlarmChance || Configuration.Instance.DevMode)
                    {
                        if (!l_Dealer.Alert)
                            TeamModule.Instance.SendMessageToTeam("Ein neuer Tipp von einem Maulwurf ist eingegangen... (/finddealer)", teams.TEAM_FIB, 10000);
                        string messageToDB = $"FindDealer: Neuer Tipp - Id - {l_Dealer.Id}, Methpreis - {l_Dealer.MethResource.Price}";
                        MySQLHandler.ExecuteAsync($"INSERT INTO `log_bucket` (`player_id`, `message`) VALUES ('{dbPlayer.Id}', '{messageToDB}');");
                        l_Dealer.Alert = true;
                        l_Dealer.LastSeller = dbPlayer;
                    }
                }

                MenuManager.DismissCurrent(dbPlayer);
                return true;
            }
        }
    }
}

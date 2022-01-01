using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using GVRP.Module.Banks.BankHistory;
using GVRP.Module.Banks.Windows;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Commands;
using GVRP.Module.Dealer;
using GVRP.Module.Gangwar;
using GVRP.Module.Injury;
using GVRP.Module.Items;
using GVRP.Module.Laboratories;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Buffs;
using GVRP.Module.Players.Db;
using GVRP.Module.Teamfight;

namespace GVRP.Module.Teams.Shelter
{
    public class ShelterMenuBuilder : MenuBuilder
    {
        public ShelterMenuBuilder() : base(PlayerMenu.ShelterMenu)
        {
        }

        public override Menu.Menu Build(DbPlayer iPlayer)
        {
            if (!iPlayer.HasData("teamShelterMenuId")) return null;
            var menu = new Menu.Menu(Menu, "Fraktionslager");

            menu.Add($"Schließen");
            menu.Add($"Fraktionsbank");
            if (!iPlayer.Team.IsBusinessTeam)
            {
                menu.Add("Equip kaufen (6.000$)");
                menu.Add("Dealer suchen (25.000$)");
                menu.Add("Gangwar beitreten");
                menu.Add("Schwarzgeldbank");
                menu.Add("Fraktionsdroge herstellen");

                if (iPlayer.Team.IsMethTeam()) menu.Add("Methlabor suchen");
                else if (iPlayer.Team.IsWeaponTeam()) menu.Add("Waffenfabrik suchen");
                else if (iPlayer.Team.IsWeedTeam()) menu.Add("Cannabislabor suchen");

                int FingerPrintedWeaponCount = iPlayer.Container.GetItemsByDataKey("fingerprint").Count;
                if (FingerPrintedWeaponCount > 0)
                {
                    int price = FingerPrintedWeaponCount * 25000;
                    menu.Add($"{FingerPrintedWeaponCount} Fingerabdrücke für ${price} entfernen");
                }
            }
            
            return menu;
        }

        public override IMenuEventHandler GetEventHandler()
        {
            return new EventHandler();
        }

        private class EventHandler : IMenuEventHandler
        {
            public bool OnSelect(int index, DbPlayer dbPlayer)
            {
                if (!dbPlayer.HasData("teamShelterMenuId")) return true;
                var teamShelter = TeamShelterModule.Instance.Get(dbPlayer.GetData("teamShelterMenuId"));
                if (teamShelter == null || teamShelter.Team.Id != dbPlayer.TeamId) return true;

                // Close menu
                if (index == 0)
                {
                    return true;
                }
                else if (index == 1)
                {
                    //Open FBank

                    MenuManager.DismissCurrent(dbPlayer);
                    ComponentManager.Get<BankWindow>().Show()(dbPlayer, "Fraktionskonto", dbPlayer.Team.Name, dbPlayer.money[0], teamShelter.Money, 0, dbPlayer.Team.BankHistory);
                }

                else if (dbPlayer.Team.IsGangster)
                {
                    if (index == 2)
                    {
                        //Equip

                        if (dbPlayer.CanWeaponAdded(WeaponHash.HeavyPistol))
                        {
                            if (!dbPlayer.TakeMoney(6000))
                            {
                                dbPlayer.SendNewNotification(
                                    MSG.Money.NotEnoughMoney(6000));
                                return true;
                            }

                            if (dbPlayer.CanWeaponAdded(WeaponHash.GolfClub))
                            {
                                switch (dbPlayer.TeamId)
                                {
                                    case (int)teams.TEAM_LCN:
                                        dbPlayer.GiveWeapon(WeaponHash.HeavyPistol, 500);
                                        dbPlayer.GiveWeapon(WeaponHash.GolfClub, 0);
                                        break;
                                    case (int)teams.TEAM_VAGOS:
                                        dbPlayer.GiveWeapon(WeaponHash.HeavyPistol, 500);
                                        dbPlayer.GiveWeapon(WeaponHash.Machete, 0);
                                        break;
                                    case (int)teams.TEAM_BRATWA:
                                        dbPlayer.GiveWeapon(WeaponHash.HeavyPistol, 500);
                                        dbPlayer.GiveWeapon(WeaponHash.Dagger, 0);
                                        break;
                                    case (int)teams.TEAM_NNM:
                                        dbPlayer.GiveWeapon(WeaponHash.HeavyPistol, 500);
                                        dbPlayer.GiveWeapon(WeaponHash.Dagger, 0);
                                        break;
                                    case (int)teams.TEAM_LOST:
                                    case (int)teams.TEAM_AOD:
                                        dbPlayer.GiveWeapon(WeaponHash.HeavyPistol, 500);
                                        dbPlayer.GiveWeapon(WeaponHash.BattleAxe, 0);
                                        break;
                                    case (int)teams.TEAM_BALLAS:
                                        dbPlayer.GiveWeapon(WeaponHash.HeavyPistol, 500);
                                        dbPlayer.GiveWeapon(WeaponHash.KnuckleDuster, 0);
                                        break;
                                    case (int)teams.TEAM_YAKUZA:
                                        dbPlayer.GiveWeapon(WeaponHash.HeavyPistol, 500);
                                        dbPlayer.GiveWeapon(WeaponHash.SwitchBlade, 0);
                                        break;
                                    case (int)teams.TEAM_GROVE:
                                        dbPlayer.GiveWeapon(WeaponHash.HeavyPistol, 500);
                                        dbPlayer.GiveWeapon(WeaponHash.Bat, 0);
                                        break;
                                    case (int)teams.TEAM_TRIADEN:
                                        dbPlayer.GiveWeapon(WeaponHash.HeavyPistol, 500);
                                        dbPlayer.GiveWeapon(WeaponHash.PoolCue, 0);
                                        break;
                                    case (int)teams.TEAM_MIDNIGHT:
                                        dbPlayer.GiveWeapon(WeaponHash.HeavyPistol, 500);
                                        dbPlayer.GiveWeapon(WeaponHash.Knife, 0);
                                        break;
                                    case (int)teams.TEAM_MARABUNTA:
                                        dbPlayer.GiveWeapon(WeaponHash.HeavyPistol, 500);
                                        dbPlayer.GiveWeapon(WeaponHash.Hatchet, 0);
                                        break;
                                    case (int)teams.TEAM_REDNECKS:
                                        dbPlayer.GiveWeapon(WeaponHash.HeavyPistol, 500);
                                        dbPlayer.GiveWeapon(WeaponHash.Bottle, 0);
                                        break;
                                    case (int)teams.TEAM_HOH:
                                        dbPlayer.GiveWeapon(WeaponHash.HeavyPistol, 500);
                                        dbPlayer.GiveWeapon(WeaponHash.Hammer, 0);
                                        break;
                                    case (int)teams.TEAM_HUSTLER:
                                        dbPlayer.GiveWeapon(WeaponHash.HeavyPistol, 500);
                                        dbPlayer.GiveWeapon(WeaponHash.Bat, 0);
                                        break;
                                    case (int)teams.TEAM_ICA:
                                        dbPlayer.GiveWeapon(WeaponHash.HeavyPistol, 500);
                                        dbPlayer.GiveWeapon(WeaponHash.Dagger, 0);
                                        break;
                                    default:
                                        dbPlayer.GiveWeapon(WeaponHash.HeavyPistol, 500);
                                        dbPlayer.GiveWeapon(WeaponHash.Bat, 0);
                                        break;
                                }

                                dbPlayer.SendNewNotification(
                                    "Sie haben ihre Waffen aus dem Arsenal genommen! (6000$ Kosten)", title: "Fraktion", notificationType: PlayerNotification.NotificationType.FRAKTION);
                                teamShelter.GiveMoney(3000);
                            }
                        }
                    }
                    else if (index == 2)
                    {
                        //Open FBank

                        if (dbPlayer.HasData("swbank")) dbPlayer.ResetData("swbank");
                        MenuManager.DismissCurrent(dbPlayer);
                        ComponentManager.Get<BankWindow>().Show()(dbPlayer, "Fraktionskonto", dbPlayer.Team.Name, dbPlayer.money[0], teamShelter.Money, 0, dbPlayer.Team.BankHistory);
                    }
                    else if (index == 3)
                    {
                        //Show random dealer

                        if (teamShelter.DealerPosition == null)
                        {
                            if (dbPlayer.TeamRankPermission.Manage < 1)
                            {
                                dbPlayer.SendNewNotification("Du musst Leader deiner Fraktion sein!");
                                return true;
                            }

                            var dealer = DealerModule.Instance.GetRandomDealer();
                            teamShelter.DealerPosition = Utils.GenerateRandomPosition(dealer.Position);
                            teamShelter.TakeMoney(25000);
                        }
                        float X = teamShelter.DealerPosition.X;
                        float Y = teamShelter.DealerPosition.Y;
                        dbPlayer.SetWaypoint(X, Y);
                        dbPlayer.SendNewNotification("Hier irgendwo...");
                    }
                    else if (index == 4)
                    {
                        //Join Gangwar

                        if (GangwarTownModule.Instance.IsTeamInGangwar(dbPlayer.Team))
                        {
                            if (CanPlayerJoinGangwar(dbPlayer))
                            {
                                Vector3 pos = teamShelter.MenuPosition;
                                dbPlayer.Player.SetPosition(dbPlayer.Team.TeamSpawns.FirstOrDefault().Value.Position);
                                TeamfightFunctions.SetToGangware(dbPlayer);
                            }
                            else
                            {
                                dbPlayer.SendNewNotification("Es sind bereits 20 Mitglieder im Gangwar");
                            }
                        }
                        else
                        {
                            dbPlayer.SendNewNotification("Kein Gangwar aktiv!");
                        }
                    }
                    else if (index == 5)
                    {
                        //Open Blackmoneybank

                        dbPlayer.SetData("swbank", 1);
                        MenuManager.DismissCurrent(dbPlayer);
                        ComponentManager.Get<BankWindow>().Show()(dbPlayer, "Schwarzgeldkonto", "Schwarzgeldkonto", dbPlayer.blackmoney[0], dbPlayer.blackmoneybank[0], 0, new List<Banks.BankHistory.BankHistory>());
                    }
                    else if (index == 6)
                    {
                        DateTime actualDate = DateTime.Now;
                        if (dbPlayer.DrugCreateLast.Day < actualDate.Day || dbPlayer.DrugCreateLast.Month < actualDate.Month)
                        {
                            if (dbPlayer.Team.IsWeedTeam())
                            {
                                uint RequiredItemId = 980;
                                int ResultAmount = 30;

                                if (dbPlayer.Container.GetItemAmount(RequiredItemId) > 0)
                                {
                                    uint resultItemId = 0;

                                    // Add Fraktion item
                                    switch (dbPlayer.TeamId)
                                    {
                                        case (int)teams.TEAM_VAGOS:
                                            resultItemId = 998;
                                            break;
                                        case (int)teams.TEAM_BALLAS:
                                            resultItemId = 999;
                                            break;
                                        case (int)teams.TEAM_GROVE:
                                            resultItemId = 1000;
                                            break;
                                        case (int)teams.TEAM_MARABUNTA:
                                            resultItemId = 1001;
                                            break;
                                        case (int)teams.TEAM_MIDNIGHT:
                                            resultItemId = 1002;
                                            break;
                                        case (int)teams.TEAM_LOST:
                                            resultItemId = 1003;
                                            break;
                                        case (int)teams.TEAM_REDNECKS:
                                            resultItemId = 1004;
                                            break;
                                        default:
                                            resultItemId = 998;
                                            break;
                                    }

                                    if (!dbPlayer.Container.CanInventoryItemAdded(resultItemId, ResultAmount))
                                    {
                                        dbPlayer.SendNewNotification(MSG.Inventory.NotEnoughSpace());
                                        return true;
                                    }

                                    dbPlayer.Container.RemoveItem(RequiredItemId, 1);
                                    dbPlayer.Container.AddItem(resultItemId, ResultAmount);
                                    dbPlayer.SendNewNotification($"Du hast aus {ItemModelModule.Instance.Get(RequiredItemId).Name}  {ResultAmount} {ItemModelModule.Instance.Get(RequiredItemId).Name} hergestellt.");

                                    dbPlayer.DrugCreateLast = DateTime.Now;
                                    dbPlayer.SaveCustomDrugsCreation();
                                    return true;
                                }
                            }
                            else if (dbPlayer.Team.IsMethTeam())
                            {
                                uint RequiredItemId = 729;
                                int ResultAmount = 15;

                                if (dbPlayer.Container.GetItemAmount(RequiredItemId) > 0)
                                {
                                    uint resultItemId = 0;

                                    // Add Fraktion item
                                    switch (dbPlayer.TeamId)
                                    {
                                        case (int)teams.TEAM_TRIADEN:
                                            resultItemId = 1005;
                                            break;
                                        case (int)teams.TEAM_YAKUZA:
                                            resultItemId = 1006;
                                            break;
                                        case (int)teams.TEAM_LCN:
                                            resultItemId = 1007;
                                            break;
                                        default:
                                            resultItemId = 1005;
                                            break;
                                    }

                                    if (!dbPlayer.Container.CanInventoryItemAdded(resultItemId, ResultAmount))
                                    {
                                        dbPlayer.SendNewNotification(MSG.Inventory.NotEnoughSpace());
                                        return true;
                                    }

                                    dbPlayer.Container.RemoveItem(RequiredItemId, 1);
                                    dbPlayer.Container.AddItem(resultItemId, ResultAmount);
                                    dbPlayer.SendNewNotification($"Du hast aus {ItemModelModule.Instance.Get(RequiredItemId).Name}  {ResultAmount} {ItemModelModule.Instance.Get(RequiredItemId).Name} hergestellt.");

                                    dbPlayer.DrugCreateLast = DateTime.Now;
                                    dbPlayer.SaveCustomDrugsCreation();
                                    return true;
                                }
                            }
                            else
                            {
                                dbPlayer.SendNewNotification("Das können Sie nicht!");
                                return true;
                            }
                        }
                        else
                        {
                            dbPlayer.SendNewNotification("Du hast heute bereits deine Fraktionsdroge hergestellt!");
                            return true;
                        }
                    }

                    else if (index == 7)
                    {
                        //Methlabor suchen
                        if (teamShelter.LoboratoryPosition == null)
                        {
                            if (dbPlayer.TeamRankPermission.Manage < 1)
                            {
                                dbPlayer.SendNewNotification("Du musst Leader deiner Fraktion sein!");
                                return true;
                            }

                            if (dbPlayer.Team.IsMethTeam())
                            {
                                var methlaboratory = MethlaboratoryModule.Instance.GetLaboratoryByTeamId(dbPlayer.TeamId);
                                teamShelter.LoboratoryPosition = methlaboratory.JumpPointEingang.Position;
                            }
                            else if (dbPlayer.Team.IsWeaponTeam())
                            {
                                var weaponlab = WeaponlaboratoryModule.Instance.GetLaboratoryByTeamId(dbPlayer.TeamId);
                                teamShelter.LoboratoryPosition = weaponlab.JumpPointEingang.Position;
                            }
                            else if (dbPlayer.Team.IsWeedTeam())
                            {
                                var cannabislab = CannabislaboratoryModule.Instance.GetLaboratoryByTeamId(dbPlayer.TeamId);
                                teamShelter.LoboratoryPosition = cannabislab.JumpPointEingang.Position;
                            }
                        }

                        float X = teamShelter.LoboratoryPosition.X;
                        float Y = teamShelter.LoboratoryPosition.Y;
                        dbPlayer.SetWaypoint(X, Y);
                        dbPlayer.SendNewNotification("Hier ist das Labor!");
                    }
                    else if (index == 8)
                    {
                        //Fingerprint
                        int FingerPrintedWeaponCount = dbPlayer.Container.GetItemsByDataKey("fingerprint").Count;
                        if (FingerPrintedWeaponCount > 0)
                        {
                            int price = FingerPrintedWeaponCount * 25000;
                            if (!dbPlayer.TakeBlackMoney(price))
                            {
                                dbPlayer.SendNewNotification(MSG.Money.NotEnoughSWMoney(price));
                                return true;
                            }

                            foreach (Item item in dbPlayer.Container.GetItemsByDataKey("fingerprint"))
                            {
                                item.Data = new Dictionary<string, dynamic>();
                            }
                            dbPlayer.Container.SaveAll();

                            dbPlayer.SendNewNotification($"Sie haben {FingerPrintedWeaponCount} für ${price} entfernen lassen!");
                            return true;
                        }
                    }
                    return true;
                }
                return false;
            }
            private bool CanPlayerJoinGangwar(DbPlayer dbPlayer)
            {
                return dbPlayer.Team.Members.Values.ToList().Where(p => p.Player.Dimension == GangwarModule.Instance.DefaultDimension).Count() < 20;
            }
        }
    }
}

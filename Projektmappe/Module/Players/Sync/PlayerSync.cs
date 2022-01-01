using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GVRP.Module.Banks.BankHistory;
using GVRP.Module.Business;
using GVRP.Module.Business.Tasks;
using GVRP.Module.Configurations;
using GVRP.Module.Crime;
using GVRP.Module.Customization;
using GVRP.Module.Houses;
using GVRP.Module.Injury;
using GVRP.Module.Jails;
using GVRP.Module.Players.Buffs;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Events;
using GVRP.Module.Staatskasse;
using GVRP.Module.Storage;
using GVRP.Module.Tasks;
using GVRP.Module.Teams.Shelter;
using GVRP.Module.UHaft;

namespace GVRP.Module.Players.Sync
{
    public sealed class PlayerSyncModule : Module<PlayerSyncModule>
    {
        private const int RpMultiplikator = 4;

        public static void CheckSalary(DbPlayer iPlayer)
        {
            // Wenn DutyPaycheck dann prüfe auf onduty
            if (!iPlayer.Team.HasDuty || !iPlayer.IsInDuty()) return;

            if(iPlayer.Team.Salary[(int)iPlayer.TeamRank] > 0) 
            {
                // Nachts doppeltes Gehalt
                if (DateTime.Now.Hour >= 0 && DateTime.Now.Hour < 10 && iPlayer.Team.GetsExtraNightPayday())
                    iPlayer.GiveEarning((iPlayer.Team.Salary[(int)iPlayer.TeamRank] / 60) + 66); //4k nightbonus
                else
                    iPlayer.GiveEarning(iPlayer.Team.Salary[(int)iPlayer.TeamRank] / 60);
            }
            else
            {
                iPlayer.GiveEarning(iPlayer.fgehalt[0] / 60);
            }
        }
        
        public static void PlayerPayday(DbPlayer iPlayer)
        {
            // Start Bankhistories
            var bankHistories = new List<Banks.BankHistory.BankHistory>();

            // Level System
            if (iPlayer.rp[0] >= iPlayer.Level * RpMultiplikator)
            {
                iPlayer.Level++;
                iPlayer.uni_points[0]++;
                iPlayer.rp[0] = 0;
                iPlayer.SendNewNotification($"Glueckwunsch, Sie haben nun Level {iPlayer.Level} erreicht!", title: "Level aufgestiegen!", notificationType:PlayerNotification.NotificationType.SERVER);
            }
            else
            {
                iPlayer.rp[0]++;
            }

            //Stuff to do on Payday
            iPlayer.payday[0] = 1;
            Main.lowerPlayerJobSkill(iPlayer);

            // Money Money Money
            var total = 0;
            
            int salary = 0;

            // Gebe Server Gehalt für nicht Duty Fraktionen & staatlichen Gehalt
            if (!iPlayer.Team.HasDuty && iPlayer.Team.Salary[(int)iPlayer.TeamRank] > 0)
            {
                salary += iPlayer.Team.Salary[(int)iPlayer.TeamRank];
            }

            // Gebe verdienst durch jbos gehalt etc
            if (iPlayer.paycheck[0] > 0)
            {
                if(iPlayer.Team.IsBusinessTeam)
                {
                    var teamShelter = TeamShelterModule.Instance.GetByTeam(iPlayer.TeamId);
                    if (teamShelter != null)
                    {
                        if (teamShelter.Money - (iPlayer.fgehalt[0] / 60) > 0)
                        {
                            salary = salary + iPlayer.paycheck[0];
                            iPlayer.Team.AddBankHistory(-iPlayer.paycheck[0], $"Gehaltszahlung an {iPlayer.GetName()}");
                        }
                    }
                } 
                else
                {
                    salary = salary + iPlayer.paycheck[0];
                }
            }

            // Staatskasse abrechnung
            if (salary != 0 && iPlayer.Team.IsStaatsfraktion())
            {
                KassenModule.Instance.StaatsKassenPaycheckAmountAll += salary;
            }

            //Reset Gehaltsanrechnung
            iPlayer.paycheck[0] = 0;

            if (iPlayer.IsMemberOfBusiness())
            {
                var business = iPlayer.ActiveBusiness;

                var member = iPlayer.GetActiveBusinessMember();

                if (business != null && member != null)
                {
                    if (business.Money > member.Salary)
                    {
                        SynchronizedTaskManager.Instance.Add(
                            new BusinessSalaryTask(business, iPlayer, member.Salary));

                        bankHistories.Add(new Banks.BankHistory.BankHistory
                        {
                            Name = "Business Lohn",
                            Value = member.Salary
                        });
                    }
                }
            }
            
            //Gebe FGehalt für alle NICHT Duty Fraktionen (Gangs)
            if (iPlayer.Team.Id != (int)teams.TEAM_CIVILIAN && iPlayer.fgehalt[0] > 0 && !iPlayer.Team.HasDuty)
            {
                var teamShelter = TeamShelterModule.Instance.GetByTeam(iPlayer.TeamId);
                if (teamShelter != null)
                {
                    var usersalary = iPlayer.fgehalt[0];
                    if (teamShelter.Money - usersalary > 0)
                    {
                        teamShelter.Money -= usersalary;

                        if (iPlayer.IsAGangster())
                        {
                            total += usersalary;

                            bankHistories.Add(new Banks.BankHistory.BankHistory
                            {
                                Name = "Fraktion - Entlohnung",
                                Value = usersalary
                            });
                        }
                        else
                        {
                            salary += usersalary;
                        }
                    }
                }
            }

            total += salary;
            bankHistories.Add(new Banks.BankHistory.BankHistory { Name = "Einkommen", Value = salary });

            if (iPlayer.Rank.Salary > 0)
            {
                bankHistories.Add(new Banks.BankHistory.BankHistory
                {
                    Name = "Nexus Bonus",
                    Value = iPlayer.Rank.Salary
                });
                total += iPlayer.Rank.Salary;
            }

            total -= 75;
            bankHistories.Add(new Banks.BankHistory.BankHistory
            {
                Name = "Krankenversicherung",
                Value = -75
            });
            

            if (iPlayer.married[0] > 0 && iPlayer.Team.IsStaatsfraktion())
            {
                var steuern = Convert.ToInt32(salary * 0.01);
                total -= steuern;
                bankHistories.Add(new Banks.BankHistory.BankHistory
                {
                    Name = "Steuern (Klasse 4 | 1%)",
                    Value = -steuern
                });
            }
            else if (iPlayer.married[0] > 0)
            {
                var steuern = Convert.ToInt32(salary * 0.03);
                total -= steuern;
                bankHistories.Add(new Banks.BankHistory.BankHistory
                {
                    Name = "Steuern (Klasse 3 | 3%)",
                    Value = -steuern
                });
            }
            else if (iPlayer.Team.IsStaatsfraktion())
            {
                var steuern = Convert.ToInt32(salary * 0.03);
                total -= steuern;
                bankHistories.Add(new Banks.BankHistory.BankHistory
                {
                    Name = "Steuern (Klasse 2 | 3%)",
                    Value = -steuern
                });
            }
            else
            {
                var steuern = Convert.ToInt32(salary * 0.15);
                total -= steuern;
                bankHistories.Add(new Banks.BankHistory.BankHistory
                {
                    Name = "Steuern (Klasse 1 | 15%)",
                    Value = -steuern
                });
            }

            int storageTax = 0;
            foreach(KeyValuePair<uint, StorageRoom> kvp in iPlayer.GetStoragesOwned()) {
                storageTax += StorageRoomAusbaustufenModule.Instance.Get((uint)kvp.Value.Ausbaustufe).Tax;
            }

            if(storageTax != 0)
            {
                total -= storageTax;
                bankHistories.Add(new Banks.BankHistory.BankHistory
                {
                    Name = "Lagerraum Steuer",
                    Value = -storageTax
                });
            }

            //KFZ Steuer
            var steuer = iPlayer.VehicleTaxSum;
            if (steuer > 0)
            {
                total -= steuer;
                bankHistories.Add(new Banks.BankHistory.BankHistory
                {
                    Name = "KFZ Steuer",
                    Value = -steuer
                });
            }
            iPlayer.VehicleTaxSum = 0;

            var newsShelter = TeamShelterModule.Instance.GetByTeam((uint)teams.TEAM_NEWS);
            if (newsShelter != null)
            {
                newsShelter.GiveMoney(50);
            }

            total -= 50;
            bankHistories.Add(
                new Banks.BankHistory.BankHistory { Name = "Rundfunkbeitrag", Value = -50 });

            // HausSteuer
            if (iPlayer.ownHouse[0] > 0)
            {
                var tax = 0;
                var wasser = 22;
                var strom = 68;
                House iHouse;
                if ((iHouse = HouseModule.Instance.Get(iPlayer.ownHouse[0])) != null)
                {

                    float taxRate = 0.003f;

                    switch(iHouse.Type)
                    {
                        case 1:
                            taxRate = 0.0005f;
                            break;
                        case 2:
                            taxRate = 0.0003f;
                            break;
                        case 3:
                            taxRate = 0.0008f;
                            break;
                        case 4:
                            taxRate = 0.0005f;
                            break;
                    }

                    tax = tax + Convert.ToInt32(iHouse.Price * taxRate);



                    int activeTenants = iHouse.GetTenantAmountUsed();
                    wasser = wasser * activeTenants;
                    strom = strom * activeTenants;
                }
                
                total -= tax;
                bankHistories.Add(new Banks.BankHistory.BankHistory
                {
                    Name = "Haussteuern",
                    Value = -tax
                });

                total -= strom + wasser;
                bankHistories.Add(new Banks.BankHistory.BankHistory
                {
                    Name = "Nebenkosten",
                    Value = -(strom + wasser)
                });
                
                KassenModule.Instance.ChangeMoney(KassenModule.Kasse.STAATSKASSE, (strom + wasser + steuer + tax));
            }
            else if (iPlayer.IsTenant())
            {
                House iHouse;
                HouseRent tenant = iPlayer.GetTenant();
                if ((iHouse = HouseModule.Instance.Get(tenant.HouseId)) != null)
                {

                    if (iHouse.OwnerId == 0)
                    {
                        iPlayer.RemoveTenant();
                    }                    
                    else if (iHouse.OwnerId == iPlayer.married[0])
                    {
                        bankHistories.Add(new Banks.BankHistory.BankHistory
                        {
                            Name = "Miete",
                            Value = 0
                        });
                    }
                    else
                    {
                        if (iPlayer.bank_money[0] < -500000)
                        {
                            // No Money to Rent??!
                            iPlayer.RemoveTenant();
                            iPlayer.SendNewNotification(
                                "Aufgrund nicht gezahlter Miete, wurde ihre Mietwohnung gekuendigt!");
                        }
                        else
                        {
                            var wasser = 25;
                            var strom = 12;

                            wasser = wasser * iHouse.Maxrents / 2;
                            strom = strom * iHouse.Maxrents / 2;

                            if (iPlayer.uni_economy[0] > 0)
                            {
                                var nachlass = 2 * iPlayer.uni_economy[0];
                                strom = Convert.ToInt32((strom * (100 - nachlass)) / 100);
                                wasser = Convert.ToInt32((wasser * (100 - nachlass)) / 100);
                            }

                            total -= tenant.RentPrice;
                            bankHistories.Add(new Banks.BankHistory.BankHistory
                            {
                                Name = "Miete",
                                Value = -tenant.RentPrice
                            });

                            total -= strom + wasser;
                            bankHistories.Add(new Banks.BankHistory.BankHistory
                            {
                                Name = "Nebenkosten",
                                Value = -(strom + wasser)
                            });

                            iHouse.InventoryCash += tenant.RentPrice;
                            iHouse.SaveHouseBank();
                        }
                    }
                }
            }
            // Levelbonus
            if (!iPlayer.IsHomeless())
            {
                var bonus = 150 * (iPlayer.Level - 1);
                total += bonus;
                bankHistories.Add(new Banks.BankHistory.BankHistory { Name = "Sozialbonus", Value = bonus });
            }

            iPlayer.SendNewNotification("Sie haben ihren Payday erhalten, schauen Sie auf Ihrem Konto fuer mehr Informationen!", title: "Kontoveraenderung", notificationType:PlayerNotification.NotificationType.SUCCESS);

            iPlayer.AddPlayerBankHistories(bankHistories);

            iPlayer.TakeOrGiveBankMoney(total, true);
        }


        public static void CheckPayDay(DbPlayer iPlayer)
        {
            //Bei Neuling keinen PayDay hochzählen
            if (iPlayer.hasPerso[0] == 0) return;

            //Payday System
            if (iPlayer.payday[0] < 60 && iPlayer.jailtime[0] == 0)
            {
                if (!iPlayer.IsInAdminDuty())
                {
                    iPlayer.payday[0]++;
                }
            }
            //The Payday
            else if (iPlayer.payday[0] >= 60)
            {
                PlayerPayday(iPlayer);
                iPlayer.Save();
            }
        }

        public override void OnPlayerMinuteUpdate(DbPlayer iPlayer)
        {
            // Staatskassenshit
            KassenModule.Instance.StaatsKassenPaycheckAmountAll = 0;

            if (iPlayer == null || !iPlayer.IsValid())
                return;

            if (iPlayer.AccountStatus != AccountStatus.LoggedIn) return;

            // Führerschein Sperre
            if (iPlayer.Lic_Bike[0] < 0) iPlayer.Lic_Bike[0]++;
            if (iPlayer.Lic_Biz[0] < 0) iPlayer.Lic_Biz[0]++;
            if (iPlayer.Lic_Boot[0] < 0) iPlayer.Lic_Boot[0]++;
            if (iPlayer.Lic_Taxi[0] < 0) iPlayer.Lic_Taxi[0]++;
            if (iPlayer.Lic_Car[0] < 0) iPlayer.Lic_Car[0]++;
            if (iPlayer.Lic_FirstAID[0] < 0) iPlayer.Lic_FirstAID[0]++;
            if (iPlayer.Lic_Gun[0] < 0) iPlayer.Lic_Gun[0]++;
            if (iPlayer.Lic_PlaneA[0] < 0) iPlayer.Lic_PlaneA[0]++;
            if (iPlayer.Lic_PlaneB[0] < 0) iPlayer.Lic_PlaneB[0]++;
            if (iPlayer.Lic_LKW[0] < 0) iPlayer.Lic_LKW[0]++;
            if (iPlayer.Lic_Transfer[0] < 0) iPlayer.Lic_Transfer[0]++;
                        
            //Jailtime
            if (iPlayer.jailtime[0] == 1)
            {
                // Erneute Wanteds?
                if(CrimeModule.Instance.CalcJailTime(iPlayer.Crimes) > 0)
                {
                    iPlayer.jailtime[0] += CrimeModule.Instance.CalcJailTime(iPlayer.Crimes);
                    iPlayer.SendNewNotification($"Durch erneute Verbrechen, haben Sie eine Haftzeitverlängerung von {CrimeModule.Instance.CalcJailTime(iPlayer.Crimes)} Minuten!");
                    iPlayer.RemoveAllCrimes();
                    
                }
                else
                {
                    //ReleasePlayerFromJail
                    iPlayer.SendNewNotification("Sie wurden aus dem Gefaengnis entlassen.");

                    if (iPlayer.HasData("inJailGroup") || iPlayer.Player.Position.DistanceTo(JailModule.PrisonZone) <= JailModule.Range)
                    {
                        iPlayer.jailtime[0] = 0;

                        // SG
                        if (iPlayer.Player.Position.DistanceTo(JailModule.PrisonZone) <= JailModule.Range)
                        {
                            iPlayer.Player.SetPosition(JailModule.PrisonSpawn);
                            iPlayer.Player.SetRotation(266.862f);
                            iPlayer.Player.Dimension = 0;
                            iPlayer.Dimension[0] = 0;
                            iPlayer.DimensionType[0] = DimensionType.World;
                            iPlayer.SetCuffed(false);
                            iPlayer.ApplyCharacter();
                        }
                        // In Zellen
                        else if(iPlayer.HasData("inJailGroup"))
                        {
                            // Get JailSpawn
                            JailSpawn JailSpawn = JailSpawnModule.Instance.GetAll().Values.Where(js => js.Group == iPlayer.GetData("inJailGroup")).FirstOrDefault();
                            if (JailSpawn != null)
                            {
                                iPlayer.Player.SetPosition(JailSpawn.Position);
                                iPlayer.Player.SetRotation(JailSpawn.Heading);
                                iPlayer.Player.Dimension = 0;
                                iPlayer.Dimension[0] = 0;
                                iPlayer.DimensionType[0] = DimensionType.World;
                                iPlayer.SetCuffed(false);
                                iPlayer.ApplyCharacter();
                            }
                        }

                        iPlayer.SendNewNotification("Sie haben ihre Haftzeit nun vollständig abgesessen!");
                    }
                    else // Check Manually
                    {

                        JailCell JailCell = JailCellModule.Instance.GetAll().Values.Where(js => js.Position.DistanceTo(iPlayer.Player.Position) < js.Range+2).FirstOrDefault();

                        if(JailCell != null)
                        {
                            // Get JailSpawn
                            JailSpawn JailSpawn = JailSpawnModule.Instance.GetAll().Values.Where(js => js.Group == JailCell.Group).FirstOrDefault();
                            if (JailSpawn != null)
                            {
                                iPlayer.Player.SetPosition(JailSpawn.Position);
                                iPlayer.Player.SetRotation(JailSpawn.Heading);
                                iPlayer.Player.Dimension = 0;
                                iPlayer.Dimension[0] = 0;
                                iPlayer.DimensionType[0] = DimensionType.World;
                                iPlayer.SetCuffed(false);
                                iPlayer.ApplyCharacter();
                            }
                        }

                        iPlayer.jailtime[0] = 0;
                        iPlayer.SendNewNotification("Sie haben ihre Haftzeit nun vollständig abgesessen!");
                    }
                }
            }
            else if (iPlayer.jailtime[0] > 1 && !iPlayer.isInjured())
            {
                iPlayer.jailtime[0]--;
            }

            CheckSalary(iPlayer);
            CheckPayDay(iPlayer);
            
            // Minus Staatskasse
            KassenModule.Instance.ChangeMoney(KassenModule.Kasse.STAATSKASSE, -KassenModule.Instance.StaatsKassenPaycheckAmountAll);
            KassenModule.Instance.StaatsKassenPaycheckAmountAll = 0;
        }
    }
}

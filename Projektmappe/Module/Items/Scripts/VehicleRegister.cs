using System;
using System.Collections.Generic;
using GTANetworkMethods;
using GVRP.Handler;
using GVRP.Module.Chat;
using GVRP.Module.Logging;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Vehicles;
using GVRP.Module.Vehicles.RegistrationOffice;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool VehicleRegister(DbPlayer iPlayer, ItemModel ItemData)
        {
            //check if worker is from DPOS
            if (!iPlayer.Team.CanRegisterVehicles())
            {
                iPlayer.SendNewNotification("Dieser Vorgang ist nur fuer geschultes Personal vom DPOS und DMV!");
                return false;
            }
            Logger.Print("dpos1");
            if (!iPlayer.IsInDuty())
            {
                iPlayer.SendNewNotification("Sie müssen im Dienst sein um Fahrzeuge anzumelden.");
                return false;
            }
            Logger.Print("dpos2");

            if (iPlayer.TeamRank < 3)
            {
                iPlayer.SendNewNotification("Sie müssen mindestens Rang 3 sein um Fahrzeuge anmelden zu können.");
                return false;
            }

            Logger.Print("dpos4");

            SxVehicle sxVehicle = VehicleHandler.Instance.GetClosestVehicle(iPlayer.Player.Position);
            if (sxVehicle == null)
            {
                iPlayer.SendNewNotification("Kein Fahrzeug in der Nähe!");
                return false;
            }
            Logger.Print("dpos5");
            if (sxVehicle.databaseId == 0) return false;
            Logger.Print("dpos6");
            //check if vehicle has driver
            if (sxVehicle.Occupants.ContainsKey(-1))
            {
                Logger.Print("dpos7");
                //driver is available
                DbPlayer driver = sxVehicle.Occupants.GetValueOrDefault(-1);
                if (driver == null || !driver.IsValid()) return false;
                Logger.Print("dpos8");
                //check if driver is owner
                if (sxVehicle.ownerId == driver.Id || (sxVehicle.IsTeamVehicle() && sxVehicle.teamid == driver.TeamId))
                {
                    Logger.Print("dpos9");
                    //yees driver is owner

                    if (sxVehicle.Team.IsStaatsfraktion())
                    {
                        Logger.Print("dpos10");
                        if (driver.TeamRank < 9)
                        {
                            Logger.Print("dpos11");
                            iPlayer.SendNewNotification("Der Bürger muss mindestens Rang 9 seiner Organisation zu sein um das Fahrzeug anzumelden.");
                            return false;
                        }
                    }

                    Item numberplate = iPlayer.Container.GetItemById(12227);
                    if (numberplate == null)
                    {
                        Logger.Print("dpos12");
                        numberplate = iPlayer.Container.GetItemById(12256);
                        if (numberplate == null)
                        {
                            Logger.Print("dpos13");
                            iPlayer.SendNewNotification("Sie benötigen ein Kennzeichen");
                            return false;
                        }
                    }
                    Logger.Print("dpos14");
                    String plateString = "";
                    if (numberplate.Data.ContainsKey("Plate"))
                    {
                        Logger.Print("dpos15");
                        plateString = numberplate.Data.ContainsKey("Plate") ? numberplate.Data.GetValueOrDefault("Plate") : RegistrationOfficeFunctions.GetRandomPlate(sxVehicle.teamid == 0 ? true : false); ;
                    }
                    else
                    {
                        Logger.Print("dpos16");
                        plateString = RegistrationOfficeFunctions.GetRandomPlate(sxVehicle.teamid == 0 ? true : false);
                    }
                    Logger.Print("dpos16.5");
                    bool successfullyRegistered = RegistrationOfficeFunctions.registerVehicle(sxVehicle, driver, iPlayer, plateString, numberplate.Data.ContainsKey("Plate") ? true : false);
                    Logger.Print("dpos17");
                    if (successfullyRegistered)
                    {
                        Logger.Print("dpos18");
                        if (numberplate != null)
                        {
                            Logger.Print("dpos19");
                            iPlayer.Container.RemoveItem(numberplate.Id);
                        }
                    }
                    Logger.Print("dpos20");
                    return successfullyRegistered;
                }
            }
            Logger.Print("dpos21");
            iPlayer.SendNewNotification("Der Besitzer des Fahrzeugs muss auf dem Fahrersitz sein");
            return false;
        }
    }
}
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
        public static bool VehicleUnregister(DbPlayer iPlayer, ItemModel ItemData)
        {
            //check if worker is from DPOS
            if (!iPlayer.Team.CanRegisterVehicles())
            {
                iPlayer.SendNewNotification("Dieser Vorgang ist nur fuer geschultes Personal vom DPOS und DMV!");
                return false;
            }

            if (!iPlayer.IsInDuty())
            {
                iPlayer.SendNewNotification("Sie müssen im Dienst sein um Fahrzeuge anzumelden.");
                return false;
            }

            if (iPlayer.TeamRank < 3)
            {
                iPlayer.SendNewNotification("Sie müssen mindestens Rang 3 sein um Fahrzeuge abmelden zu können.");
                return false;
            }

            bool canUseEverywhere = bool.Parse(ItemData.Script.Split("_")[1]);
            if (!canUseEverywhere && iPlayer.Player.Position.DistanceTo(new GTANetworkAPI.Vector3(386.223, -1621.51, 29.292)) > RegistrationOfficeFunctions.RegistrationRadius)
            {
                iPlayer.SendNewNotification("Sie müssen am Zulassungsplatz sein.");
                return false;
            }

            SxVehicle sxVehicle = VehicleHandler.Instance.GetClosestVehicle(iPlayer.Player.Position);
            if (sxVehicle == null)
            {
                iPlayer.SendNewNotification("Kein Fahrzeug in der Nähe!");
                return false;
            }

            if (sxVehicle.databaseId == 0) return false;

            if (!RegistrationOfficeFunctions.IsVehicleRegistered(sxVehicle.databaseId))
            {
                iPlayer.SendNewNotification("Dieses Fahrzeug ist nicht mit einem Kennzeichen angemeldet.");
                return false;
            }

            //check if vehicle has driver
            if (sxVehicle.Occupants.ContainsKey(-1))
            {
                //driver is available
                DbPlayer driver = sxVehicle.Occupants.GetValueOrDefault(-1);
                if (driver == null || !driver.IsValid()) return false;

                //check if driver is owner
                if (sxVehicle.ownerId == driver.Id || (sxVehicle.IsTeamVehicle() && sxVehicle.teamid == driver.TeamId))
                {
                    //yees driver is owner

                    if (sxVehicle.Team.IsStaatsfraktion())
                    {
                        if (driver.TeamRank < 9)
                        {
                            iPlayer.SendNewNotification("Der Bürger muss mindestens Rang 9 seiner Organisation zu sein um das Fahrzeug abzumelden.");
                            return false;
                        }
                    }

                    if (driver.TakeBankMoney(10000, "Fahrzeug abgemeldet " + sxVehicle.databaseId))
                    {
                        sxVehicle.Registered = false;
                        RegistrationOfficeFunctions.UpdateVehicleRegistrationToDb(sxVehicle, driver, iPlayer, sxVehicle.plate, false);
                        sxVehicle.plate = "";
                        driver.SendNewNotification("Ihr Fahrzeug wurde erfolgreich abgemeldet");
                        iPlayer.SendNewNotification("Sie haben das Fahrzeug erfolgreich abgemeldet.");
                        return true;
                    }
                    else
                    {
                        iPlayer.SendNewNotification("Ihr Konto ist nicht gedeckt 10.000$");
                        return false;
                    }
                }
            }

            iPlayer.SendNewNotification("Der Besitzer des Fahrzeugs muss auf dem Fahrersitz sein");
            return false;
        }
    }
}
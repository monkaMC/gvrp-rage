using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Handler;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Windows;
using GVRP.Module.Teams.Shelter;
using GVRP.Module.Vehicles;
using GVRP.Module.Vehicles.Data;

namespace GVRP.Module.JobFactions.Carsell
{
    public class JobCarsellFactionEvents : Script
    {
        [RemoteEvent]
        public void JobCarsellSetCarColor(Client player, string returnstring)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            
            if (returnstring.Length < 2 || !returnstring.Contains(" ")) return;

            string[] splittedReturn = returnstring.Split(" ");
            if (splittedReturn.Length != 2) return;

            if (!Int32.TryParse(splittedReturn[0], out int color1)) return;
            if (!Int32.TryParse(splittedReturn[1], out int color2)) return;

            if (!JobCarsellFactionModule.WhitelistedColors.Contains(color1) || !JobCarsellFactionModule.WhitelistedColors.Contains(color2))
            {
                dbPlayer.SendNewNotification("Diese Farbe ist nicht verfügbar!");
                return;
            }

            if (!dbPlayer.Player.IsInVehicle) return;

            SxVehicle sxVehicle = dbPlayer.Player.Vehicle.GetVehicle();

            if (sxVehicle == null || !sxVehicle.IsValid() || sxVehicle.teamid != dbPlayer.TeamId) return;

            sxVehicle.color1 = color1;
            sxVehicle.color2 = color2;

            sxVehicle.entity.PrimaryColor = color1;
            sxVehicle.entity.SecondaryColor = color2;

            dbPlayer.SetData("carsellTuneColor1", color1);
            dbPlayer.SetData("carsellTuneColor2", color2);

            dbPlayer.SendNewNotification($"Fahrzeugfarbe auf {color1} {color2} geändert!");
            return;
        }

        [RemoteEvent]
        public void JobCarsellCreateOrder(Client player, string returnstring)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (returnstring.Length < 2) return;
            
            if (!dbPlayer.Player.IsInVehicle) return;

            DbPlayer customer = Players.Players.Instance.FindPlayer(returnstring);
            if (customer == null || !customer.IsValid()) return;

            if(customer.TeamId == dbPlayer.TeamId)
            {
                dbPlayer.SendNewNotification($"Mitarbeiter können hier nichts kaufen...!");
                return;
            }

            if(customer.Player.Position.DistanceTo(dbPlayer.Player.Position) > 10.0f)
            {
                dbPlayer.SendNewNotification($"Kunde muss in der Nähe sein!");
                return;
            }

            dbPlayer.SetData("carsellCustomer", customer.Id);

            ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Rechnung erstellen", Callback = "JobCarsellFinishOrder", Message = $"Kunde: {customer.GetName()} | Geben Sie den Preis an:" });
            return;
        }

        [RemoteEvent]
        public void JobCarsellFinishOrder(Client player, string returnstring)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (returnstring.Length < 2) return;

            if (!dbPlayer.Player.IsInVehicle) return;
            
            if(!Int32.TryParse(returnstring, out int price))
            {
                return;
            }

            DbPlayer customer = Players.Players.Instance.GetByDbId(dbPlayer.GetData("carsellCustomer"));
            if (customer == null || !customer.IsValid()) return;

            SxVehicle sxVehicle = dbPlayer.Player.Vehicle.GetVehicle();
            if (sxVehicle == null || !sxVehicle.IsValid() || sxVehicle.teamid != dbPlayer.TeamId) return;

            // Validate Price

            if (!dbPlayer.HasData("carsellTuneWheelId") || !dbPlayer.HasData("carsellTuneColor1") || !dbPlayer.HasData("carsellTuneColor2"))
            {
                dbPlayer.SendNewNotification($"Sie müssen zuerst Felgen und Farbe definieren!");
                return;
            }

            int vehiclePriceMin = Convert.ToInt32(sxVehicle.Data.Price * 0.8);
            int vehiclePriceMax = Convert.ToInt32(sxVehicle.Data.Price * 1.1);

            if(price < vehiclePriceMin || price > vehiclePriceMax)
            {
                dbPlayer.SendNewNotification($"Der Preis muss zwischen ${vehiclePriceMin} und ${vehiclePriceMax} liegen!");
                return;
            }

            if (customer.bank_money[0] < price)
            {
                dbPlayer.SendNewNotification($"Kunde hat nicht genug Geld auf dem Konto!");
                return;
            }
            else {

                ComponentManager.Get<TextInputBoxWindow>().Show()(customer, new TextInputBoxWindowObject() { Title = "Kauf Bestätigen", Callback = "JobCarsellCustomerBuy", Message = $"{(sxVehicle.Data.mod_car_name.Length <= 0 ? sxVehicle.Data.Model : sxVehicle.Data.mod_car_name)} für ${price} - Geben sie [KAUFEN] zum annehmen oder [ABLEHNEN] zum ablehnen ein:" });

                customer.SetData("carshop_sellerId", dbPlayer.Id);
                customer.SetData("carshop_vehicleDataId", sxVehicle.Data.Id);
                customer.SetData("carshop_wheelId", dbPlayer.GetData("carsellTuneWheelId"));
                customer.SetData("carshop_color1", dbPlayer.GetData("carsellTuneColor1"));
                customer.SetData("carshop_color2", dbPlayer.GetData("carsellTuneColor2"));
                customer.SetData("carshop_price", price);

                
                dbPlayer.SendNewNotification($"Kaufauftrag für {(sxVehicle.Data.mod_car_name.Length <= 0 ? sxVehicle.Data.Model : sxVehicle.Data.mod_car_name)} wurde dem Kunden zur Bestätigung übergeben!");
                return;
            }
        }

        [RemoteEvent]
        public void JobCarsellSetPriceAttach(Client player, string returnstring)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (returnstring.Length < 2) return;

            if (!dbPlayer.Player.IsInVehicle) return;

            if (!Int32.TryParse(returnstring, out int price))
            {
                return;
            }
            
            SxVehicle sxVehicle = dbPlayer.Player.Vehicle.GetVehicle();
            if (sxVehicle == null || !sxVehicle.IsValid() || sxVehicle.teamid != dbPlayer.TeamId) return;

            if(price < sxVehicle.Data.Price*0.8 || price > sxVehicle.Data.Price*1.1)
            {
                dbPlayer.SendNewNotification($"Der Preis muss sich in der verfügbaren Preisspanne befinden!");
                return;
            }

            sxVehicle.CarsellPrice = price;
            dbPlayer.SendNewNotification($"Das Preisschild von ${price} wurde angeheftet!");

            MySQLHandler.ExecuteAsync($"UPDATE `fvehicles` SET carsell_price = '{price}' WHERE id = '{sxVehicle.databaseId}';");
            return;
        }

        [RemoteEvent]
        public void JobCarsellCustomerBuy(Client player, string returnstring)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            
            if(returnstring.Trim().ToLower() != "kaufen")
            {
                dbPlayer.ResetData("carshop_sellerId");
                dbPlayer.ResetData("carshop_vehicleDataId");
                dbPlayer.ResetData("carshop_wheelId");
                dbPlayer.ResetData("carshop_color1");
                dbPlayer.ResetData("carshop_color2");
                dbPlayer.ResetData("carshop_price");
                return;
            }

            if (!dbPlayer.HasData("carshop_sellerId") || !dbPlayer.HasData("carshop_vehicleDataId") || !dbPlayer.HasData("carshop_wheelId") ||
                !dbPlayer.HasData("carshop_color1") || !dbPlayer.HasData("carshop_color2") || !dbPlayer.HasData("carshop_price"))
            {
                dbPlayer.SendNewNotification($"Kein Auftrag vorhanden!");
                return;
            }

            DbPlayer seller = Players.Players.Instance.GetByDbId(dbPlayer.GetData("carshop_sellerId"));
            if (seller == null || !seller.IsValid()) return;

            VehicleData Data = VehicleDataModule.Instance.GetDataById(dbPlayer.GetData("carshop_vehicleDataId"));
            int price = dbPlayer.GetData("carshop_price");
            
            if (!dbPlayer.TakeBankMoney(price, $"Fahrzeugkauf {(Data.mod_car_name.Length <= 0 ? Data.Model : Data.mod_car_name)}"))
            {
                dbPlayer.SendNewNotification(MSG.Money.NotEnoughMoney(price));
                return;
            }
            else
            {
                dbPlayer.SendNewNotification($"Sie haben einen {(Data.mod_car_name.Length <= 0 ? Data.Model : Data.mod_car_name)} für ${price} bestellt!");

                int diff = price - Convert.ToInt32(Data.Price*0.8);

                // steuern
                int tax = Convert.ToInt32(diff * 0.20);
                if (tax < 0) tax = 0;

                TeamShelter teamShelter = TeamShelterModule.Instance.GetByTeam(seller.TeamId);
                if (teamShelter == null) return;

                // diff = gewinn
                if (diff > 0)
                {
                    teamShelter.GiveMoney(diff-tax);
                    seller.SendNewNotification($"Sie haben einen {(Data.mod_car_name.Length <= 0 ? Data.Model : Data.mod_car_name)} für ${price} verkauft! (Gewinn ${diff} (- ${tax} Steuern) wurde auf die Fbank überwiesen)");
                }

                JobCarsellFactionModule.Instance.InsertCustomerOrder(seller.Id, dbPlayer.Id, seller.TeamId, (int)Data.Id, dbPlayer.GetData("carshop_wheelId"), dbPlayer.GetData("carshop_color1"), dbPlayer.GetData("carshop_color2"));

                seller.SendNewNotification($"Farzeug {(Data.mod_car_name.Length <= 0 ? Data.Model : Data.mod_car_name)} für Kunde: {dbPlayer.GetName()} bestellt!");

                dbPlayer.ResetData("carshop_sellerId");
                dbPlayer.ResetData("carshop_vehicleDataId");
                dbPlayer.ResetData("carshop_wheelId");
                dbPlayer.ResetData("carshop_color1");
                dbPlayer.ResetData("carshop_color2");
                dbPlayer.ResetData("carshop_price");
                return;
            }
        }
    }
}

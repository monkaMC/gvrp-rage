using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GVRP.Handler;
using GVRP.Module.Business.FuelStations;
using GVRP.Module.Chat;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.GTAN;
using GVRP.Module.Items;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Windows;
using GVRP.Module.Spawners;
using GVRP.Module.Vehicles;

namespace GVRP.Module.Business.FuelStations
{
    public class FuelStationModule : SqlModule<FuelStationModule, FuelStation, uint>
    {
        public static uint BenzinModelId = 537;
        public static uint RohoelModelId = 536;
        
        public override Type[] RequiredModules()
        {
            return new[] { typeof(FuelStationGasModule), typeof(BusinessModule) };
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `business_fuelstations`;";
        }

        protected override void OnLoaded()
        {
            MenuManager.Instance.AddBuilder(new FuelStationMenuBuilder());
            MenuManager.Instance.AddBuilder(new FuelStationFillMenuBuilder());
        }

        protected override void OnItemLoaded(FuelStation fuelStation)
        {
            // Get Zapfsäulen
            fuelStation.Gas = FuelStationGasModule.Instance.GetAll().Values.Where(fg => fg.FuelStationId == fuelStation.Id).ToList();

            // Load Inventory
            fuelStation.Container = ContainerManager.LoadContainer(fuelStation.Id, ContainerTypes.FUELSTATION);

            // Set Marker lul
            Main.ServerBlips.Add(Blips.Create(fuelStation.Position, fuelStation.Name, 361, 1.0f, true, 64));
            return;
        }

        public FuelStation GetThis(Vector3 position)
        {
            return Instance.GetAll().Values.FirstOrDefault(fs => fs.Position.DistanceTo(position) < 3.0f);
        }

        public FuelStation GetStaionByGas(Vector3 position)
        {
            FuelStationGas fuelStationGas = FuelStationGasModule.Instance.GetAll().Values.FirstOrDefault(gs => gs.Position.DistanceTo(position) < 4.0f);
            if (fuelStationGas != null)
            {
                return Instance.Get(fuelStationGas.FuelStationId);
            }

            return null;
        }
        
        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            uint fuelStationId = 0;

            if (colShape.TryData("fuelstationInfoId", out fuelStationId))
            {
                FuelStation fuelStation = FuelStationModule.Instance.Get(fuelStationId);
                if (fuelStation == null) return false;
                Dictionary<String, String> temp = new Dictionary<string, string>();
                switch (colShapeState)
                {
                    
                    case ColShapeState.Enter:
                        if (fuelStation.IsOwnedByBusines())
                        {   
                            temp.Add("Preis", fuelStation.Price + "$/L");
                            temp.Add("Verfuegbare Liter", fuelStation.Container.GetInventoryUsedSpace()/1000 + " L");
                        }
                        else
                        {
                            temp.Add("Kein Eigentümer", $"Zum Verkauf (${fuelStation.BuyPrice})");
                        }
                        dbPlayer.Player.TriggerEvent("sendInfocard", fuelStation.Name, "red", "fuelstation_erwin.jpg", 20000, JsonConvert.SerializeObject(temp));
                        return true;
                }


            }
            else
            {
                if (!colShape.TryData("fuelstationId", out fuelStationId)) return false;
                FuelStation fuelStation = FuelStationModule.Instance.Get(fuelStationId);
                if (fuelStation == null) return false;
                switch (colShapeState)
                {
                    case ColShapeState.Enter:
                        dbPlayer.SetData("fuelstationId", fuelStationId);
                        if (fuelStation.IsOwnedByBusines())
                        {
                            Business business = fuelStation.GetOwnedBusiness();
                            dbPlayer.SendNewNotification($"${fuelStation.Price}/Liter Besitzer: {business.Name}", title:$"{fuelStation.Name}");
                        }
                        else
                        {
                            dbPlayer.SendNewNotification($"${fuelStation.Price}/Liter Zum Verkauf (${fuelStation.BuyPrice})", title: $"{fuelStation.Name}");
                        }
                        return true;
                    case ColShapeState.Exit:
                        if (dbPlayer.HasData("fuelstationId")) dbPlayer.ResetData("fuelstationId");
                        return true;
                    default:
                        return false;
                }

            }

            return false;


        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if(!dbPlayer.Player.IsInVehicle)
            {
                if(dbPlayer.HasData("fuelstationId"))
                {
                    FuelStation fuelStation = FuelStationModule.Instance.Get(dbPlayer.GetData("fuelstationId"));
                    if(fuelStation != null && fuelStation.Position.DistanceTo(dbPlayer.Player.Position) < 4.0f)
                    {
                        MenuManager.Instance.Build(PlayerMenu.FuelStationMenu, dbPlayer).Show(dbPlayer);
                    }
                }
                // check for gas stations
                var fuel = FuelStationModule.Instance.GetStaionByGas(dbPlayer.Player.Position);
                if(fuel == null)
                {
                    fuel = FuelStationModule.Instance.GetThis(dbPlayer.Player.Position);
                }
                if (fuel != null)
                {
                    if(dbPlayer.Container.GetItemAmount(180) > 0) // Kanister tanken
                    {
                        if (fuel.IsOwnedByBusines() && fuel.Container.GetItemAmount(FuelStationModule.BenzinModelId) < 20)
                        {
                            dbPlayer.SendNewNotification(
                                 "Diese Tankstelle hat nicht genug Benzin!");
                            return false;
                        }

                        int price = fuel.Price * 20;
                        if(!dbPlayer.TakeMoney(price))
                        {
                            dbPlayer.SendNewNotification("Kanister kann nicht betankt werden, Geld benoetigt ($" + price + ")");
                            return false; ;
                        }
                        else
                        {
                            dbPlayer.Container.RemoveItem(180, 1);
                            dbPlayer.Container.AddItem(20, 1);
                            dbPlayer.SendNewNotification("Kanister fuer $" + price + " betankt!");
                            fuel.Container.RemoveItem(FuelStationModule.BenzinModelId, 20);

                            if (fuel.IsOwnedByBusines())
                            {
                                fuel.GetOwnedBusiness().GiveMoney(price);
                            }
                            return true;
                        }
                    }
                }
            }
            else
            {
                // check for gas stations
                var fuel = FuelStationModule.Instance.GetStaionByGas(dbPlayer.Player.Position);
                if(fuel == null)
                {
                    fuel = FuelStationModule.Instance.GetThis(dbPlayer.Player.Position);
                }
                if(fuel != null)
                {
                    SxVehicle sxVehicle = dbPlayer.Player.Vehicle.GetVehicle();

                    if(sxVehicle != null && sxVehicle.IsValid() && sxVehicle.CanInteract && dbPlayer.CanInteract())
                    {
                        if (VehicleHandler.Instance.GetClosestVehiclesPlayerCanControl(dbPlayer, 11.0f).Where(cp => cp.Container.GetItemAmount(BenzinModelId) > 0).Count() > 0)
                        {
                            ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = fuel.Name, Callback = "fillfuelstation", Message = $"Wie viel moechten sie abliefern? Platz verfuegbar : {fuel.Container.GetInventoryFreeSpace()/1000}L" });
                            return true;
                        }
                    }

                }
            }
            return false;
        }
    }
}

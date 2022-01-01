using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GVRP.Handler;
using GVRP.Module.Assets.Tattoo;
using GVRP.Module.Business;
using GVRP.Module.Business.Raffinery;
using GVRP.Module.Chat;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Farming;
using GVRP.Module.GTAN;
using GVRP.Module.Houses;
using GVRP.Module.Items;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Windows;
using GVRP.Module.Tattoo;
using GVRP.Module.Vehicles;

namespace GVRP.Module.Business.FuelStations
{
    public class FuelStationFillMenuBuilder : MenuBuilder
    {
        public FuelStationFillMenuBuilder() : base(PlayerMenu.FuelStationFillMenu)
        {
        }

        public override Menu.Menu Build(DbPlayer iPlayer)
        {
            var menu = new Menu.Menu(Menu, "Benzin liefern");

            menu.Add($"Schließen");

            List<SxVehicle> sxVehicles = VehicleHandler.Instance.GetClosestVehiclesPlayerCanControl(iPlayer, 11.0f);
            if (sxVehicles != null && sxVehicles.Count() > 0)
            {
                foreach (SxVehicle sxVehicle in sxVehicles)
                {
                    menu.Add($"Aus Fahrzeug {sxVehicle.GetName()} ({sxVehicle.databaseId}) abgeben");
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
            public bool OnSelect(int index, DbPlayer iPlayer)
            {
                if (index == 0)
                {
                    MenuManager.DismissCurrent(iPlayer);
                    return false;
                }
                
                if (!iPlayer.Player.IsInVehicle || !iPlayer.HasData("fillLiter")) return false;

                List<SxVehicle> sxVehicles = VehicleHandler.Instance.GetClosestVehiclesPlayerCanControl(iPlayer, 11.0f);
                if (sxVehicles != null && sxVehicles.Count() > 0)
                {
                    int count = 1;
                    foreach (SxVehicle sxVehicle in sxVehicles)
                    {
                        if (index == count)
                        {
                            int liter = iPlayer.GetData("fillLiter");
                            if (liter < 1 || liter > 2000)
                            {
                                iPlayer.SendNewNotification("Falsche Menge!");
                                return false;
                            }

                            // check for gas stations
                            var fuel = FuelStationModule.Instance.GetStaionByGas(iPlayer.Player.Position);
                            if(fuel == null)
                            {
                                fuel = FuelStationModule.Instance.GetThis(iPlayer.Player.Position);
                            }
                            if (fuel != null)
                            {
                                if (sxVehicle != null && sxVehicle.IsValid())
                                {
                                    // Get Amount FUEL can added
                                    int amountFuelCanTake = fuel.Container.GetMaxItemAddedAmount(FuelStationModule.BenzinModelId);

                                    if(sxVehicle.Container.GetItemAmount(FuelStationModule.BenzinModelId) < amountFuelCanTake)
                                    {
                                        amountFuelCanTake = sxVehicle.Container.GetItemAmount(FuelStationModule.BenzinModelId);
                                    }

                                    if (liter > amountFuelCanTake)
                                    {
                                        iPlayer.SendNewNotification($"Maximal {amountFuelCanTake} einladbar!");
                                        return false;
                                    }

                                    Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                                    {
                                        Chats.sendProgressBar(iPlayer, (100 * liter));

                                        iPlayer.Player.TriggerEvent("freezePlayer", true);
                                        iPlayer.SetData("userCannotInterrupt", true);
                                        sxVehicle.CanInteract = false;

                                        await Task.Delay(100 * liter);

                                        iPlayer.Player.TriggerEvent("freezePlayer", false);
                                        iPlayer.ResetData("userCannotInterrupt");

                                        // reset fuel fill counter
                                        if (liter > sxVehicle.Container.GetItemAmount(FuelStationModule.BenzinModelId) || liter > amountFuelCanTake)
                                        {
                                            iPlayer.SendNewNotification($"Maximal {amountFuelCanTake} einladbar!");
                                            return;
                                        }

                                        sxVehicle.CanInteract = true;

                                        // Subvention
                                        int subvention = fuel.IsOwnedByBusines() ? liter * 10 : liter * 30;

                                        sxVehicle.Container.MoveIntoAnother(fuel.Container, FuelStationModule.BenzinModelId, liter);
                                        iPlayer.SendNewNotification($"Sie haben {liter} Liter ausgeladen. Staatliche Subvention (${subvention})");
                                        iPlayer.GiveMoney(subvention);

                                        Logging.Logger.AddToFuelStationInsertLog(fuel.Id, iPlayer.Id, liter);
                                    }));
                                }
                            }
                            MenuManager.DismissCurrent(iPlayer);
                            return true;
                        }
                        else count++;
                    }
                }

                MenuManager.DismissCurrent(iPlayer);
                return true;
            }
        }
    }
}
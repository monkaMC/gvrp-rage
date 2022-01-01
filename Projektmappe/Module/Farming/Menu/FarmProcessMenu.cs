using System;
using System.Collections.Generic;
using System.Linq;
using GVRP.Handler;
using GVRP.Module.Assets.Tattoo;
using GVRP.Module.Business;
using GVRP.Module.Business.Raffinery;
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

namespace GVRP.Module.Business.Raffinery
{
    public class FarmProcessMenuBuilder : MenuBuilder
    {
        public FarmProcessMenuBuilder() : base(PlayerMenu.FarmProcessMenu)
        {
        }

        public override Menu.Menu Build(DbPlayer iPlayer)
        {
            var menu = new Menu.Menu(Menu, "Verarbeitung");

            menu.Add($"Schließen");

            List<SxVehicle> sxVehicles = VehicleHandler.Instance.GetClosestVehiclesPlayerCanControl(iPlayer, 20.0f);
            if(sxVehicles != null && sxVehicles.Count() > 0)
            {
                foreach(SxVehicle sxVehicle in sxVehicles)
                {
                    menu.Add($"{sxVehicle.GetName()} ({sxVehicle.databaseId}) verarbeiten");
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

                var farmProcess = FarmProcessModule.Instance.GetByPosition(iPlayer.Player.Position);
                if (farmProcess == null) return false;

                if (iPlayer.Player.IsInVehicle) return false;

                List<SxVehicle> sxVehicles = VehicleHandler.Instance.GetClosestVehiclesPlayerCanControl(iPlayer, 20.0f);
                if (sxVehicles != null && sxVehicles.Count() > 0)
                {
                    int count = 1;
                    foreach (SxVehicle sxVehicle in sxVehicles)
                    {
                        if (index == count)
                        {
                            // Fahrzeug verarbeiten
                            if (sxVehicle == null || !sxVehicle.IsValid()) return false;
                            if (!sxVehicle.CanInteract) return false;

                            // Motor muss aus sein
                            if (sxVehicle.SyncExtension.EngineOn)
                            {
                                iPlayer.SendNewNotification("Motor muss ausgeschaltet sein!");
                                return false;
                            }
                            // zugeschlossen
                            if (!sxVehicle.SyncExtension.Locked)
                            {
                                iPlayer.SendNewNotification("Fahrzeug muss zugeschlossen sein!");
                                return false;
                            }
                            if (sxVehicle.entity.HasData("Door_KRaum") && sxVehicle.entity.GetData("Door_KRaum") == 1)
                            {
                                iPlayer.SendNewNotification("Der Kofferaum muss zugeschlossen sein!");
                                return false;
                            }
                            if (iPlayer.Player.VehicleSeat != -1)
                            {
                                iPlayer.SendNewNotification("Sie müssen Fahrer sein!");
                                return false;
                            }

                            FarmProcessModule.Instance.FarmProcessAction(farmProcess, iPlayer, sxVehicle.Container, sxVehicle);
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
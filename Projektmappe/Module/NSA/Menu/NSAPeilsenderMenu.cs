using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GVRP.Handler;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Menu;
using GVRP.Module.NSA.Observation;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Windows;
using GVRP.Module.Telefon.App;
using GVRP.Module.Vehicles;

namespace GVRP.Module.NSA.Menu
{
    public class NSAPeilsenderMenuBuilder : MenuBuilder
    {
        public NSAPeilsenderMenuBuilder() : base(PlayerMenu.NSAPeilsenderMenu)
        {

        }

        public override Module.Menu.Menu Build(DbPlayer p_DbPlayer)
        {
            var l_Menu = new Module.Menu.Menu(Menu, "NSA Aktive Peilsender");
            l_Menu.Add($"Schließen");

            foreach(NSAPeilsender nSAPeilsender in NSAObservationModule.NSAPeilsenders)
            {
                l_Menu.Add($"{nSAPeilsender.Name}");
            }

            return l_Menu;
        }

        public override IMenuEventHandler GetEventHandler()
        {
            return new EventHandler();
        }

        private class EventHandler : IMenuEventHandler
        {
            public bool OnSelect(int index, DbPlayer iPlayer)
            {
                int i = 1;

                foreach (NSAPeilsender nSAPeilsender in NSAObservationModule.NSAPeilsenders)
                {
                    if(i == index)
                    {
                        if(nSAPeilsender.VehicleId != 0)
                        {
                            SxVehicle sxVeh = VehicleHandler.Instance.GetByVehicleDatabaseId(nSAPeilsender.VehicleId);
                            if (sxVeh == null || !sxVeh.IsValid()) return true;

                            // Orten
                            iPlayer.Player.TriggerEvent("setPlayerGpsMarker", sxVeh.entity.Position.X, sxVeh.entity.Position.Y);
                            iPlayer.SendNewNotification("Peilsender geortet!");
                            return true;
                        }
                        
                        return true;
                    }
                    i++;
                }

                MenuManager.DismissCurrent(iPlayer);
                return true;
            }
        }
    }
}

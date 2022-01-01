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
using GVRP.Module.Vehicles.Data;

namespace GVRP.Module.Carsell.Menu
{
    public class CarsellCustomerMenuBuilder : MenuBuilder
    {
        public CarsellCustomerMenuBuilder() : base(PlayerMenu.CarsellCustomerMenu)
        {

        }

        public override Module.Menu.Menu Build(DbPlayer p_DbPlayer)
        {
            if (!p_DbPlayer.Player.IsInVehicle) return null;

            var l_Menu = new Module.Menu.Menu(Menu, "Fahrzeug Bereitstellen");

            l_Menu.Add($"Schließen");

            l_Menu.Add($"Kundenauftrag erstellen");
            l_Menu.Add($"Farbe lackieren");
            l_Menu.Add($"Reifenfelgen wechseln");
            l_Menu.Add($"Preisschild ändern");

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
                if (!dbPlayer.Player.IsInVehicle) return true;

                SxVehicle sxVehicle = dbPlayer.Player.Vehicle.GetVehicle();
                if (sxVehicle == null || !sxVehicle.IsValid()) return true;

                if (index == 0)
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    return true;
                }
                else if (index == 1) // Kundenauftrag Erstellen
                {
                    ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Auftrag erstellen", Callback = "JobCarsellCreateOrder", Message = "Geben Sie den Namen des Kunden an:" });
                    return true;
                }
                else if (index == 2) // Farbe lackieren
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Fahrzeugfarbe ändern", Callback = "JobCarsellSetCarColor", Message = "Geben Sie die Farben an BSP: (101 1):" });
                    return true;
                }
                else if (index == 3) // Reifenfelgen wechseln
                {
                    Module.Menu.MenuManager.Instance.Build(GVRP.Module.Menu.PlayerMenu.CarsellTuneWheelMenu, dbPlayer).Show(dbPlayer);
                    return false;
                }
                else if (index == 4) // Preisschild
                {
                    MenuManager.DismissCurrent(dbPlayer);
                    ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Preisschild ändern", Callback = "JobCarsellSetPriceAttach", Message = $"Geben Sie einen Pries zwischen ${Convert.ToInt32(sxVehicle.Data.Price*0.8)} und {Convert.ToInt32(sxVehicle.Data.Price * 1.1)}:" });
                    return true;
                }
                return true;
            }
        }
    }
}

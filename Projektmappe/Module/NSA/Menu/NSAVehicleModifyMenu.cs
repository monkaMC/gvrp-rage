using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Handler;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Windows;

namespace GVRP.Module.NSA.Menu
{
    public class NSAVehicleModifyMenuBuilder : MenuBuilder
    {
        public NSAVehicleModifyMenuBuilder() : base(PlayerMenu.NSAVehicleModifyMenu)
        {

        }

        public override Module.Menu.Menu Build(DbPlayer p_DbPlayer)
        {
            if (!p_DbPlayer.HasData("nsa_work_vehicle")) return null;
            var l_Menu = new Module.Menu.Menu(Menu, "NSA Fahrzeugverwaltung");
            l_Menu.Add($"Schließen");
            l_Menu.Add($"Nummernschild ändern");
            l_Menu.Add($"Farbe ändern");
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
                if (!iPlayer.HasData("nsa_work_vehicle")) return true;
                
                switch (index)
                {
                    case 0:
                        MenuManager.DismissCurrent(iPlayer);
                        break;
                    case 1:
                        MenuManager.DismissCurrent(iPlayer);
                        ComponentManager.Get<TextInputBoxWindow>().Show()(iPlayer, new TextInputBoxWindowObject() { Title = "Nummernschild ändern", Callback = "SetCarPlateNSA", Message = "Geben Sie ein Nummernschild ein:" });
                        return true;
                    case 2:
                        MenuManager.DismissCurrent(iPlayer);
                        ComponentManager.Get<TextInputBoxWindow>().Show()(iPlayer, new TextInputBoxWindowObject() { Title = "Fahrzeugfarbe ändern", Callback = "SetCarColorNSA", Message = "Geben Sie die Farben an BSP: (101 1):" });
                        return true;
                    default:
                        break;
                }

                MenuManager.DismissCurrent(iPlayer);
                return true;
            }
        }
    }
}

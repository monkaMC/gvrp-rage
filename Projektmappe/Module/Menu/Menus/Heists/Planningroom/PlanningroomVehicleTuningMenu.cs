using GVRP.Module.ClientUI.Components;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Windows;

namespace GVRP.Module.Menu.Menus.Heists.Planningroom
{
    public class PlanningroomVehicleTuningMenuBuilder : MenuBuilder
    {
        public PlanningroomVehicleTuningMenuBuilder() : base(PlayerMenu.PlanningroomVehicleTuningMenu)
        {
        }

        public override Menu Build(DbPlayer iPlayer)
        {
            if (!iPlayer.HasData("planningroom_vehicle_tuning")) return null;

            var menu = new Menu(Menu, "Planningroom", "Fahrzeuge modifizieren");

            menu.Add($"Schließen");
            menu.Add($"Nummernschild ändern");
            menu.Add($"Farbe ändern");

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
                if (!iPlayer.HasData("planningroom_vehicle_tuning")) return true;

                switch (index)
                {
                    case 0:
                        MenuManager.DismissCurrent(iPlayer);
                        break;
                    case 1:
                        MenuManager.DismissCurrent(iPlayer);
                        ComponentManager.Get<TextInputBoxWindow>().Show()(iPlayer, new TextInputBoxWindowObject() { Title = "Nummernschild ändern", Callback = "PlanningroomSetVehiclePlate", Message = "Geben Sie ein Nummernschild ein:" });
                        return true;
                    case 2:
                        MenuManager.DismissCurrent(iPlayer);
                        ComponentManager.Get<TextInputBoxWindow>().Show()(iPlayer, new TextInputBoxWindowObject() { Title = "Fahrzeugfarbe ändern", Callback = "PlanningroomSetVehicleColor", Message = "Geben Sie die Farben an BSP: (101 1):" });
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

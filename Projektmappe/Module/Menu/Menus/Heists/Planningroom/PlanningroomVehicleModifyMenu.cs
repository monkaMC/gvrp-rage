using System;
using GVRP.Handler;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Menu.Menus.Heists.Planningroom
{
    public class PlanningroomVehicleModifyMenuBuilder : MenuBuilder
    {
        public PlanningroomVehicleModifyMenuBuilder() : base(PlayerMenu.PlanningroomVehicleModifyMenu)
        {
        }

        public override Menu Build(DbPlayer iPlayer)
        {
            var menu = new Menu(Menu, "Planningroom", "Fahrzeugverwaltung");

            menu.Add($"Schließen");

            foreach (SxVehicle sxVehicle in VehicleHandler.Instance.GetClosestPlanningVehiclesFromTeam(iPlayer.Player.Position, Convert.ToInt32(iPlayer.Team.Id), 10.0f))
            {
                menu.Add($"{sxVehicle.GetName()} ({sxVehicle.databaseId})");
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
                else if(index >= 1)
                {
                    int idx = 1;

                    foreach (SxVehicle sxVehicle in VehicleHandler.Instance.GetClosestPlanningVehiclesFromTeam(iPlayer.Player.Position, Convert.ToInt32(iPlayer.Team.Id), 10.0f))
                    {
                        if (index == idx)
                        {
                            iPlayer.SetData("planningroom_vehicle_tuning", sxVehicle.databaseId);
                            MenuManager.DismissCurrent(iPlayer);
                            MenuManager.Instance.Build(PlayerMenu.PlanningroomVehicleTuningMenu, iPlayer).Show(iPlayer);
                            return false;
                        }

                        idx++;
                    }
                }

                MenuManager.DismissCurrent(iPlayer);
                return true;
            }
        }
    }
}

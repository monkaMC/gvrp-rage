using GVRP.Module.Heist.Planning;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Menu.Menus.Heists.Planningroom
{
    public class PlanningroomUpgradeMenuBuilder : MenuBuilder
    {
        public PlanningroomUpgradeMenuBuilder() : base(PlayerMenu.PlanningroomUpgradeMenu)
        {
        }

        public override Menu Build(DbPlayer iPlayer)
        {
            PlanningRoom room = PlanningModule.Instance.GetPlanningRoomByTeamId(iPlayer.Team.Id);

            var menu = new Menu(Menu, "Upgrade Menu");

            menu.Add($"Schließen");

            if (room.Bought && room.MainFloor > 0)
            {
                menu.Add($"Grundraum upgraden");
                if (room.MainFloor > 1)
                {
                    menu.Add($"Spiegeldecke upgraden");
                    menu.Add($"Einrichtungsstyle upgraden");
                    menu.Add($"Inneneinrichtung upgraden");
                    menu.Add($"Spielautomaten upgraden");
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
                PlanningRoom room = PlanningModule.Instance.GetPlanningRoomByTeamId(iPlayer.Team.Id);

                // Close menu
                if (index == 0)
                {
                    iPlayer.ResetData("planningRoomUpgradeSelection");
                    MenuManager.DismissCurrent(iPlayer);
                    return false;
                }
                // Planningroom action
                else
                {
                    iPlayer.SetData("planningRoomUpgradeSelection", index);
                    MenuManager.DismissCurrent(iPlayer);
                    MenuManager.Instance.Build(PlayerMenu.PlanningroomUpgradeSelectionMenu, iPlayer).Show(iPlayer);
                    return false;
                }
            }
        }
    }
}

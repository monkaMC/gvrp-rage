using GVRP.Module.Heist.Planning;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Menu.Menus.Heists.Planningroom
{
    public class PlanningroomKellerUpgradeMenu : MenuBuilder
    {
        public PlanningroomKellerUpgradeMenu() : base(PlayerMenu.PlanningroomKellerUpgradeMenu)
        {
        }

        public override Menu Build(DbPlayer iPlayer)
        {
            PlanningRoom room = PlanningModule.Instance.GetPlanningRoomByTeamId(iPlayer.Team.Id);

            var menu = new Menu(Menu, "Keller Upgrade Menu");

            menu.Add($"Schließen");

            if (room.Bought && room.MainFloor > 1 && room.BasementLevel > 1)
            {
                menu.Add($"Mechaniker Upgrade");
                menu.Add($"Hacker Upgrade");
                menu.Add($"Waffen Upgrade");
                menu.Add($"Umkleide Upgrade");
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

                if (index == 0)
                {
                    MenuManager.DismissCurrent(iPlayer);
                    return false;
                }
                else if (index == 1)
                {
                    room.UpgradePlanningRoom(iPlayer, 7, 1);
                    return false;
                }
                else if (index == 2)
                {
                    room.UpgradePlanningRoom(iPlayer, 8, 1);
                    return false;
                }
                else if (index == 3)
                {
                    room.UpgradePlanningRoom(iPlayer, 9, 1);
                    return false;
                }
                else if (index == 4)
                {
                    room.UpgradePlanningRoom(iPlayer, 10, 1);
                    return false;
                }

                return true;
            }
        }
    }
}

using GVRP.Module.Heist.Planning;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Menu.Menus.Heists.Planningroom
{
    public class ExchangeTrashMenu : MenuBuilder
    {
        public ExchangeTrashMenu() : base(PlayerMenu.ExchangeTrashMenu)
        {
        }

        public override Menu Build(DbPlayer iPlayer)
        {
            var menu = new Menu(Menu, "Schrottplatz Mitarbeiter");

            menu.Add($"Schließen");
            menu.Add($"Muell entsorgen");

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
                    if (room.Bought && room.MainFloor == 0 && room.MainFloorCleanup != 0)
                    {
                        room.DeliverPlanningroomTrash(iPlayer);
                        return false;
                    }
                    else if (room.Bought && room.BasementLevel == 0 && room.BasementCleanUp != 0 && room.MainFloor > 1)
                    {
                        room.RecyclePlanningRoomTrash(iPlayer);
                        return false;
                    }

                    return true;
                }

                return true;
            }
        }
    }
}

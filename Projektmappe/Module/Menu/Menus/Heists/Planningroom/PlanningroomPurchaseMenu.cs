using GVRP.Module.Heist.Planning;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Menu.Menus.Heists.Planningroom
{
    public class PlanningroomPurchaseMenBuilder : MenuBuilder
    {
        public PlanningroomPurchaseMenBuilder() : base(PlayerMenu.PlanningroomPurchaseMenu)
        {
        }

        public override Menu Build(DbPlayer iPlayer)
        {
            PlanningRoom room = PlanningModule.Instance.GetPlanningRoomByTeamId(iPlayer.Team.Id);

            var menu = new Menu(Menu, "Anonymer Kontakt");

            menu.Add($"Schließen");

            if(!room.Bought)
            {
                menu.Add($"Planungsraum erwerben (2.000.000$)");
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
                    MenuManager.DismissCurrent(iPlayer);
                    return false;
                }
                else if (index == 1)
                {
                    room.PurchasePlanningRoom(iPlayer);

                    MenuManager.DismissCurrent(iPlayer);
                    return false;
                }

                return true;
            }
        }
    }
}

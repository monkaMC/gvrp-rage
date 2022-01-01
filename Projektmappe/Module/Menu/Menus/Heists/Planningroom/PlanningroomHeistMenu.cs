using GVRP.Module.Heist.Planning;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Menu.Menus.Heists.Planningroom
{
    public class PlanningroomHeistMenu : MenuBuilder
    {
        public PlanningroomHeistMenu() : base(PlayerMenu.PlanningroomHeistMenu)
        {
        }

        public override Menu Build(DbPlayer iPlayer)
        {
            var menu = new Menu(Menu, "Heists");

            menu.Add($"Schließen");
            menu.Add($"Casino");

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
                else if (index == 1)
                {
                    PlanningModule.Instance.StartHeist(iPlayer, index);
                    return true;
                }

                return true;
            }
        }
    }
}

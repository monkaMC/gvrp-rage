using GVRP.Module.Players.Db;

namespace GVRP.Module.Menu.Menus.Heists.Planningroom
{
    public class PlanningroomPreQuestMenu : MenuBuilder
    {
        public PlanningroomPreQuestMenu() : base(PlayerMenu.PlanningroomPreQuestMenu)
        {
        }

        public override Menu Build(DbPlayer iPlayer)
        {
            var menu = new Menu(Menu, "Vorbereitungen");

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
                    iPlayer.SetData("planningroom_pre_quest", index);
                    MenuManager.DismissCurrent(iPlayer);
                    MenuManager.Instance.Build(PlayerMenu.PlanningroomPreQuestSelectionMenu, iPlayer).Show(iPlayer);
                    return false;
                }

                return true;
            }
        }
    }
}

using GVRP.Module.Heist.Planning;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Menu.Menus.Heists.Planningroom
{
    public class PlanningroomPreQuestSelectionMenu : MenuBuilder
    {
        public PlanningroomPreQuestSelectionMenu() : base(PlayerMenu.PlanningroomPreQuestSelectionMenu)
        {
        }

        public override Menu Build(DbPlayer iPlayer)
        {
            var menu = new Menu(Menu, "Vorbereitungen");

            menu.Add($"Schließen");

            menu.Add($"Fahrzeug besorgen");
            menu.Add($"Outfit besorgen");
            menu.Add($"Extra besorgen");

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
                int heistId = iPlayer.GetData("planningroom_pre_quest");

                if (index == 0)
                {
                    MenuManager.DismissCurrent(iPlayer);
                    return false;
                }
                else if (index == 1)
                {
                    PlanningModule.Instance.StartVehiclePreQuest(iPlayer, heistId);
                    return true;
                }
                else if(index == 2)
                {
                    PlanningModule.Instance.StartOutfitPreQuest(iPlayer, heistId);
                    return true;
                }
                else if (index == 3)
                {
                    PlanningModule.Instance.StartExtraPreQuest(iPlayer, heistId);
                    return true;
                }

                return true;
            }
        }
    }
}

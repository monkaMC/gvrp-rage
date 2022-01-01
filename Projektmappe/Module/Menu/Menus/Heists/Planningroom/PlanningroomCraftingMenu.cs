using GVRP.Module.Heist.Planning;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Menu.Menus.Heists.Planningroom
{
    public class PlanningroomCraftingMenu : MenuBuilder
    {
        public PlanningroomCraftingMenu() : base(PlayerMenu.PlanningroomCraftingMenu)
        {
        }

        public override Menu Build(DbPlayer iPlayer)
        {
            var menu = new Menu(Menu, "Schmiede Mitarbeiter");

            menu.Add($"Schließen");
            menu.Add($"Tresortür anfertigen");
            menu.Add($"Tresortür abholen");

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
                    if (room.CasinoPlanLevel == 1)
                    {
                        room.CraftCasinoDoor(iPlayer);
                        return true;
                    }

                    return true;
                }
                else if(index == 2)
                {
                    if (room.CasinoPlanLevel == 1)
                    {
                        room.DeliverCasinoDoor(iPlayer);
                        return true;
                    }

                    return true;
                }

                return true;
            }
        }
    }
}

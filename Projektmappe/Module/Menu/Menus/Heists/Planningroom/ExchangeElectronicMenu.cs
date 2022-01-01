using GVRP.Module.Heist.Planning;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Menu.Menus.Heists.Planningroom
{
    public class ExchangeElectronicMenu : MenuBuilder
    {
        public ExchangeElectronicMenu() : base(PlayerMenu.ExchangeElectronicMenu)
        {
        }

        public override Menu Build(DbPlayer iPlayer)
        {
            var menu = new Menu(Menu, "Eletronik Umwandlung");

            menu.Add($"Schließen");
            menu.Add($"Materialien umwandeln");

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
                    PlanningModule.Instance.ExchangeElectronicItem(iPlayer);
                    return false;
                }

                return true;
            }
        }
    }
}

using GVRP.Module.Business;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Menu.Menus.Business
{
    public class BusinessSafeMenuBuilder : MenuBuilder
    {
        public BusinessSafeMenuBuilder() : base(PlayerMenu.BusinessSafe)
        {
        }

        public override Menu Build(DbPlayer iPlayer)
        {
            var biz = BusinessModule.Instance.GetById((uint) -iPlayer.Player.Dimension);
            if (biz == null) return null;

            var menu = new Menu(Menu, "Business Tower");

            menu.Add("Geldtresor", "Geld abheben/einzahlen");

            menu.Add(MSG.General.Close(), "");
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
                switch (index)
                {
                    case 0:
                    {
                            // Menue später fuer Tresor    
                            break;
                    }
                    default:
                        break;
                }

                return true;
            }
        }
    }
}
using GVRP.Module.Players.Db;

namespace GVRP.Module.Menu.Menus.Asservatenkammer
{
    class AsservatenkammerDeliverMenuBuilder :MenuBuilder
    {
        public AsservatenkammerDeliverMenuBuilder() : base(PlayerMenu.AsservatenkammerDeliverMenu)
        {
        }

        public override Menu Build(DbPlayer iPlayer)
        {
            var menu = new Menu(Menu, "Asservatenkammer");
            menu.Add(MSG.General.Close(), "");
            menu.Add("Abgabe", "Ware abgeben");
            menu.Add("Informationen", "Bestand prüfen");

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
                if (index == 1)
                {
                    // abgabe
                }
                else if(index == 2)
                {
                    // Informationen
                }

                return true;
            }
        }
    }
}

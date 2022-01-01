using GTANetworkAPI;
using GVRP.Module.Guenther;
using GVRP.Module.Houses;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Menu.Menus.Guenther
{
    public class GuentherAusgangMenuBuilder : MenuBuilder
    {
        public GuentherAusgangMenuBuilder() : base(PlayerMenu.GuentherAusgangMenu)
        {
        }

        public override Menu Build(DbPlayer iPlayer)
        {
            var menu = new Menu(Menu, "Fahrstuhl");
            menu.Add("Eingangstür");
            menu.Add("Garage");
            menu.Add(MSG.General.Close());
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
                if (iPlayer == null || !iPlayer.IsValid()) return false;
                switch (index)
                {
                    case 0:
                        GuentherModule.Instance.FahrstuhlTeleport(iPlayer, GuentherModule.Outside, 34.4379f);
                        break;
                    case 1:
                        GuentherModule.Instance.FahrstuhlTeleport(iPlayer, new Vector3(-1535.7f, -580.245f, 25.7078f), 36.0609f);
                        break;
                    default:
                        break;
                }
                MenuManager.DismissCurrent(iPlayer);
                return false;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Crime
{
    public class CrimeArrestMenuBuilder : MenuBuilder
    {
        public CrimeArrestMenuBuilder() : base(PlayerMenu.CrimeArrestMenu)
        {
        }

        public override Menu.Menu Build(DbPlayer iPlayer)
        {
            var menu = new Menu.Menu(Menu, "Haftrichter");

            menu.Add($"TV war kooperativ");
            menu.Add($"TV war NICHT kooperativ");

            menu.Add($"Schließen");
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
                if (!iPlayer.HasData("arrestPlayer")) return false;
                DbPlayer findPlayer = Players.Players.Instance.FindPlayer(iPlayer.GetData("arrestPlayer"));
                if (findPlayer == null || !findPlayer.IsValid()) return false;

                switch (index)
                {
                    case 0:
                        findPlayer.ArrestPlayer(iPlayer, true);
                        MenuManager.DismissCurrent(iPlayer);
                        return true;
                    case 1:
                        findPlayer.ArrestPlayer(iPlayer, false);
                        MenuManager.DismissCurrent(iPlayer);
                        return true;
                }
                MenuManager.DismissCurrent(iPlayer);
                return false;
            }
        }
    }
}

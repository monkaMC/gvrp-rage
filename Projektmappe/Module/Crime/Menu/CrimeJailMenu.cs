using System;
using System.Collections.Generic;
using System.Linq;
using GVRP.Module.Assets.Tattoo;
using GVRP.Module.Business;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.GTAN;
using GVRP.Module.Houses;
using GVRP.Module.Items;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Windows;
using GVRP.Module.Tattoo;

namespace GVRP.Module.Crime
{
    public class CrimeJailMenuBuilder : MenuBuilder
    {
        public CrimeJailMenuBuilder() : base(PlayerMenu.CrimeJailMenu)
        {
        }

        public override Menu.Menu Build(DbPlayer iPlayer)
        {
            if (!iPlayer.IsACop() || !iPlayer.Duty) return null;
            
            var menu = new Menu.Menu(Menu, "Gefaengnisuebersicht");

            menu.Add($"Schließen");

            foreach (DbPlayer jailPlayer in Players.Players.Instance.GetValidPlayers().Where(x => x.jailtime[0] > 0).ToList())
            {
                menu.Add($"{jailPlayer.GetName()} | {jailPlayer.jailtime[0]} Hafteinheiten");
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
                MenuManager.DismissCurrent(iPlayer);
                return false;
            }
        }
    }
}
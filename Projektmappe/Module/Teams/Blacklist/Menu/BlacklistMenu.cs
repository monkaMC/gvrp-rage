using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GVRP.Handler;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Menu;
using GVRP.Module.NSA.Observation;
using GVRP.Module.PlayerName;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Windows;
using GVRP.Module.Telefon.App;

namespace GVRP.Module.Teams.Blacklist.Menu
{
    public class BlacklistMenuBuilder : MenuBuilder
    {
        public BlacklistMenuBuilder() : base(PlayerMenu.BlacklistMenu)
        {

        }

        public override Module.Menu.Menu Build(DbPlayer p_DbPlayer)
        {
            if (p_DbPlayer == null || !p_DbPlayer.IsValid() || !p_DbPlayer.IsAGangster()) return null;

            var l_Menu = new Module.Menu.Menu(Menu, "Blacklist Einträge");
            l_Menu.Add($"Schließen");

            foreach(BlacklistEntry entry in p_DbPlayer.Team.blacklistEntries)
            {
                if (!BlacklistModule.Instance.blacklistTypes.ContainsKey(entry.TypeId)) continue;
                DbPlayer target = Players.Players.Instance.FindPlayerById((uint)entry.BlacklistPlayerId);
                if (target == null || !target.IsValid()) continue;
                l_Menu.Add($"{PlayerNameModule.Instance.Get((uint)entry.BlacklistPlayerId).Name} | ${BlacklistModule.Instance.blacklistTypes[entry.TypeId].Costs} | {BlacklistModule.Instance.blacklistTypes[entry.TypeId].ShortDesc}");
            }

            return l_Menu;
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
                return true;
            }
        }
    }
}

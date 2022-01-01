using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GVRP.Handler;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Menu;
using GVRP.Module.NSA.Observation;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Windows;
using GVRP.Module.Telefon.App;

namespace GVRP.Module.NSA.Menu
{
    public class NSAKeyMenu : MenuBuilder
    {
        public NSAKeyMenu() : base(PlayerMenu.NSAKeyMenu)
        {

        }

        public override Module.Menu.Menu Build(DbPlayer p_DbPlayer)
        {
            if (!p_DbPlayer.HasData("nsa_target_player_id")) return null;

            DbPlayer l_Target = Players.Players.Instance.FindPlayerById(p_DbPlayer.GetData("nsa_target_player_id"));
            if (l_Target == null || !l_Target.IsValid()) return null;

            var l_Menu = new Module.Menu.Menu(Menu, "NSA Schlüssel (" + l_Target.GetName() + ")");
            l_Menu.Add($"Schließen");
            
            foreach (KeyValuePair<uint, string> kvp in l_Target.VehicleKeys)
            {
                l_Menu.Add($"{kvp.Key} ({kvp.Value}) [Schlüssel]");
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
                if (!iPlayer.HasData("nsa_target_player_id"))
                    return false;

                MenuManager.DismissCurrent(iPlayer);
                return true;
            }
        }
    }
}

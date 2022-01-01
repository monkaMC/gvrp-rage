using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Handler;
using GVRP.Module.Menu;
using GVRP.Module.Players.Db;

namespace GVRP.Module.LSCustoms.Menu
{
    public class LSCTuningSelectionBuilder : MenuBuilder
    {
        public LSCTuningSelectionBuilder() : base(PlayerMenu.LSCTuningMenu)
        {

        }

        public override Module.Menu.Menu Build(DbPlayer p_DbPlayer)
        {
            var l_Menu = new Module.Menu.Menu(Menu, "LSC Tuning Menü");
            l_Menu.Add($"Schließen");

            var l_Tunings = Helper.Helper.m_Mods;
            foreach (var l_Tuning in l_Tunings)
            {
                if (l_Tuning.Value.ID >= 90)
                    continue;
                l_Menu.Add($"{l_Tuning.Value.Name}");
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
                if (Helper.Helper.m_Mods.ContainsKey(index))
                {
                    var l_Tuning = Helper.Helper.m_Mods[index];
                    iPlayer.SetData("tuneSlot", (int)l_Tuning.ID);

                    MenuManager.DismissCurrent(iPlayer);
                    MenuManager.Instance.Build(PlayerMenu.MechanicTune, iPlayer).Show(iPlayer);
                    return false;
                }

                MenuManager.DismissCurrent(iPlayer);
                return true;
            }
        }
    }
}

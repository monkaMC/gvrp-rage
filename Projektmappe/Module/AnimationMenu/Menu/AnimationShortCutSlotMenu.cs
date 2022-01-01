using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.Menu;
using GVRP.Module.Players.Db;

namespace GVRP.Module.AnimationMenu
{
    public class AnimationShortCutSlotMenuBuilder : MenuBuilder
    {
        public AnimationShortCutSlotMenuBuilder() : base(PlayerMenu.AnimationShortCutSlotMenu)
        {
        }

        public override Menu.Menu Build(DbPlayer iPlayer)
        {
            var menu = new Menu.Menu(Menu, "Slot auswählen");

            menu.Add($"Schließen");

            foreach(KeyValuePair<uint, uint> kvp in iPlayer.AnimationShortcuts)
            {
                if (kvp.Key == 0 || kvp.Key == 1) continue;// system keys 

                menu.Add($"Slot {kvp.Key}");
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
                if (index == 0)
                {
                    MenuManager.DismissCurrent(iPlayer);
                    return false;
                }

                int idx = 1;
                foreach (KeyValuePair<uint, uint> kvp in iPlayer.AnimationShortcuts)
                {
                    if (kvp.Key == 0 || kvp.Key == 1) continue;// system keys 

                    if(index == idx)
                    {
                        // Open Secound Menu
                        iPlayer.SetData("animSlot", kvp.Key);
                        MenuManager.Instance.Build(PlayerMenu.AnimationShortCutMenu, iPlayer).Show(iPlayer);
                        return false;
                    }
                    idx++;
                }

                MenuManager.DismissCurrent(iPlayer);
                return true;
            }
        }
    }
}

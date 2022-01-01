using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.AnimationMenu
{
    public class AnimationShortCutMenuBuilder : MenuBuilder
    {
        public AnimationShortCutMenuBuilder() : base(PlayerMenu.AnimationShortCutMenu)
        {
        }

        public override Menu.Menu Build(DbPlayer iPlayer)
        {
            if (!iPlayer.HasData("animSlot")) return null;

            var menu = new Menu.Menu(Menu, $"Animation für Slot {iPlayer.GetData("animSlot")} auswählen");

            menu.Add($"Schließen");

            foreach (AnimationItem animationItem in AnimationItemModule.Instance.GetAll().Values)
            {
                menu.Add($"{animationItem.Name}");
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
                if (!iPlayer.HasData("animSlot")) return false;
                if (index == 0)
                {
                    MenuManager.DismissCurrent(iPlayer);
                    return false;
                }

                int idx = 1;
                foreach (AnimationItem animationItem in AnimationItemModule.Instance.GetAll().Values)
                {
                    if (index == idx)
                    {
                        // Open Secound Menu
                        if (!iPlayer.AnimationShortcuts.ContainsKey(iPlayer.GetData("animSlot"))) return false;

                        iPlayer.AnimationShortcuts[iPlayer.GetData("animSlot")] = animationItem.Id;
                        iPlayer.SendNewNotification($"Animationsslot {iPlayer.GetData("animSlot")} mit {animationItem.Name} belegt!");
                        iPlayer.SaveAnimationShortcuts();
                        iPlayer.UpdateAnimationShortcuts();
                        return true;
                    }
                    idx++;
                }

                MenuManager.DismissCurrent(iPlayer);
                return true;
            }
        }
    }
}

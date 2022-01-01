using System.Collections.Generic;
using System.Linq;
using GVRP.Module.AnimationMenu;
using GVRP.Module.Helper;
using GVRP.Module.Houses;
using GVRP.Module.Injury;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP
{
    public class AnimationItemMenuBuilder : MenuBuilder
    {
        public AnimationItemMenuBuilder() : base(PlayerMenu.AnimationMenuIn)
        {
        }

        public override Menu Build(DbPlayer iPlayer)
        {
            if (!iPlayer.HasData("animCat")) return null;
            int catId = iPlayer.GetData("animCat");

            var menu = new Menu(Menu, AnimationCategoryModule.Instance.Get((uint)catId).Name);
            menu.Add("Schließen", "");
            foreach (KeyValuePair<uint, AnimationItem> kvp in AnimationItemModule.Instance.GetAll().Where(i => i.Value.CategoryId == catId))
                menu.Add(kvp.Value.Name);
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
                if (iPlayer.Player.IsInVehicle || !iPlayer.CanInteract()) return false;
                if (!iPlayer.HasData("animCat")) return false;
                int catId = iPlayer.GetData("animCat");

                if(index == 0) // Close
                {
                    MenuManager.DismissCurrent(iPlayer);
                    return true;
                }

                int idx = 1;
                foreach (KeyValuePair<uint, AnimationItem> kvp in AnimationItemModule.Instance.GetAll().Where(i => i.Value.CategoryId == catId))
                {
                    if(index == idx)
                    {
                        return AnimationExtension.StartAnimation(iPlayer, kvp.Value);
                    }
                    idx++;
                }
                return false;
            }
        }
    }
}
using System.Collections.Generic;
using GVRP.Module.AnimationMenu;
using GVRP.Module.Farming;
using GVRP.Module.Helper;
using GVRP.Module.Houses;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP
{
    public class AnimationMenuBuilder : MenuBuilder
    {
        public AnimationMenuBuilder() : base(PlayerMenu.AnimationMenuOv)
        {
        }

        public override Menu Build(DbPlayer iPlayer)
        {
            var menu = new Menu(Menu, "Animation Menu");
            menu.Add("Schließen", "");
            menu.Add("Animation beenden", "");
            foreach (KeyValuePair<uint, AnimationCategory> kvp in AnimationCategoryModule.Instance.GetAll())
            {
                if (kvp.Value == null) continue;
                menu.Add($"{kvp.Value.Name}", "");
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
                var player = iPlayer.Player;
                if (index == 0)
                {
                    return true;
                }
                else if (index == 1)
                {
                    if (!iPlayer.CanInteract()) return false;
                    player.StopAnimation();
                    MenuManager.DismissCurrent(iPlayer);
                    iPlayer.Player.TriggerEvent("freezePlayer", false);
                    return true;
                }
                else
                {
                    int idx = 0;

                    foreach (KeyValuePair<uint, AnimationCategory> kvp in AnimationCategoryModule.Instance.GetAll())
                    {
                        if (kvp.Value == null) continue;
                        if(index-2 == idx)
                        {
                            // Animation Category Clicked
                            iPlayer.SetData("animCat", (int)kvp.Value.Id);
                            break;
                        }
                        idx++;
                    }
                    
                    MenuManager.Instance.Build(PlayerMenu.AnimationMenuIn, iPlayer).Show(iPlayer);
                    return false;
                }
            }
        }
    }

    public static class AnimationExtension
    {
        public static bool StartAnimation(DbPlayer iPlayer, AnimationItem animationItem)
        {
            var player = iPlayer.Player;
            if (animationItem == null)
            {
                return true;
            }
            
            iPlayer.PlayAnimation(AnimationMenuModule.Instance.animFlagDic[(uint)animationItem.AnimFlag], animationItem.AnimDic, animationItem.AnimName);
            return false;
        }
    }
}
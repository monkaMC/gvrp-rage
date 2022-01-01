using GVRP.Module.Clothes;
using GVRP.Module.Clothes.Shops;
using GVRP.Module.GTAN;
using GVRP.Module.Menu;
using GVRP.Module.Players.Db;

namespace GVRP
{
    public class TeamWardrobePropsMenu : MenuBuilder
    {
        public TeamWardrobePropsMenu() : base(PlayerMenu.TeamWardrobeProps)
        {
        }

        public override Menu Build(DbPlayer iPlayer)
        {
            var menu = new Menu(Menu, "Accessoires");
            menu.Add(MSG.General.Close());
            foreach (var kvp in ClothesShopModule.Instance.GetPropsSlots())
            {
                menu.Add(kvp.Value, kvp.Value);
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
                    MenuManager.DismissMenu(iPlayer.Player, (uint) PlayerMenu.TeamWardrobeProps);
                    ClothModule.SaveCharacter(iPlayer);
                    return false;
                }

                index--;
                var slots = ClothesShopModule.Instance.GetPropsSlots();
                var count = index;
                var currKey = -1;
                foreach (var currSlot in slots)
                {
                    if (count == 0)
                    {
                        currKey = currSlot.Key;
                        break;
                    }

                    count--;
                }

                if (currKey < 0)
                {
                    return false;
                }

                iPlayer.SetData("teamWardrobePropsSlot", currKey);
                var menu = MenuManager.Instance.Build(PlayerMenu.TeamWardrobePropsSelection, iPlayer);
                menu?.Show(iPlayer);
                return false;
            }
        }
    }
}
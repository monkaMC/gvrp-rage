using System.Collections.Generic;
using System.Linq;
using GVRP.Module.Clothes;
using GVRP.Module.Clothes.Props;
using GVRP.Module.Clothes.Shops;
using GVRP.Module.Clothes.Team;
using GVRP.Module.GTAN;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP
{
    public class TeamWardrobePropsSelectionMenu : MenuBuilder
    {
        public TeamWardrobePropsSelectionMenu() : base(PlayerMenu.TeamWardrobePropsSelection)
        {
        }

        public override Menu Build(DbPlayer iPlayer)
        {
            if (!iPlayer.HasData("teamWardrobePropsSlot"))
            {
                return null;
            }

            int slotId = iPlayer.GetData("teamWardrobePropsSlot");
            var slots = ClothesShopModule.Instance.GetPropsSlots();
            if (!slots.ContainsKey(slotId))
            {
                return null;
            }

            var slot = slots[slotId];
            var menu = new Menu(Menu, slot);
            menu.Add(MSG.General.Close());
            menu.Add("Leer");

            if (iPlayer.IsFreeMode())
            {
                List<Prop> propsForSlot = PropModule.Instance.GetTeamWarerobe(iPlayer, slotId);
                if (propsForSlot != null && propsForSlot.Count > 0)
                {
                    foreach (var prop in propsForSlot)
                    {
                        menu.Add(prop.Name);
                    }
                }
            }
            else
            {
                var teamSkin = TeamSkinModule.Instance.GetTeamSkin(iPlayer);
                if (teamSkin == null)
                {
                    return null;
                }

                var clothes = teamSkin.Props.Where(cloth => cloth.Slot == slotId).ToList();
                if (clothes.Count == 0)
                {
                    return null;
                }

                foreach (var cloth in clothes)
                {
                    menu.Add(cloth.Name);
                }
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
                    MenuManager.DismissMenu(iPlayer.Player, (uint) PlayerMenu.TeamWardrobePropsSelection);
                    ClothModule.SaveCharacter(iPlayer);
                    return false;
                }

                if (!iPlayer.HasData("teamWardrobePropsSlot"))
                {
                    return false;
                }

                int slotId = iPlayer.GetData("teamWardrobePropsSlot");
                if (index == 1)
                {
                    iPlayer.Player.ClearAccessory(slotId);
                    iPlayer.Character.EquipedProps.Remove(slotId);
                    ClothModule.SaveCharacter(iPlayer);
                    return false;
                }

                index -= 2;
                if (iPlayer.IsFreeMode())
                {
                    List<Prop> clothesForSlot = PropModule.Instance.GetTeamWarerobe(iPlayer, slotId);
                    if (index >= clothesForSlot.Count)
                    {
                        return false;
                    }

                    var cloth = clothesForSlot[index];
                    if (iPlayer.Character.EquipedProps.ContainsKey(cloth.Slot))
                    {
                        iPlayer.Character.EquipedProps[cloth.Slot] = cloth.Id;
                    }
                    else
                    {
                        iPlayer.Character.EquipedProps.Add(cloth.Slot, cloth.Id);
                    }
                }
                else
                {
                    var teamSkin = TeamSkinModule.Instance.GetTeamSkin(iPlayer);
                    if (teamSkin == null)
                    {
                        return false;
                    }

                    var clothes = teamSkin.Props.Where(cloth => cloth.Slot == slotId).ToList();
                    if (clothes.Count == 0 || index >= clothes.Count)
                    {
                        return false;
                    }

                    var currCloth = clothes[index];
                    if (iPlayer.Character.EquipedProps.ContainsKey(currCloth.Slot))
                    {
                        iPlayer.Character.EquipedProps[currCloth.Slot] = currCloth.Id;
                    }
                    else
                    {
                        iPlayer.Character.EquipedProps.Add(currCloth.Slot, currCloth.Id);
                    }
                }

                ClothModule.Instance.RefreshPlayerClothes(iPlayer);
                ClothModule.SaveCharacter(iPlayer);
                return false;
            }
        }
    }
}
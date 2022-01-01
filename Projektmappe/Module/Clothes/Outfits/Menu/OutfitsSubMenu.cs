using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Clothes.Props;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Windows;

namespace GVRP.Module.Clothes.Outfits
{
    public class OutfitsSubMenuBuilder : MenuBuilder
    {
        public OutfitsSubMenuBuilder() : base(PlayerMenu.OutfitsSubMenu)
        {
        }

        public override Menu.Menu Build(DbPlayer iPlayer)
        {
            if (!iPlayer.HasData("outfit")) return null;
            var menu = new Menu.Menu(Menu, "Outfits");
            menu.Add(MSG.General.Close());
            menu.Add("Anlegen");
            menu.Add("Löschen");
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
                if (!iPlayer.HasData("outfit")) return false;
                if (index == 1)
                {
                    Outfit outfit = null;
                    try {
                        outfit = iPlayer.GetData("outfit");
                    }
                    catch(Exception e)
                    {
                        return false;
                    }
                    if (outfit == null) return false;

                    // Resettall
                    iPlayer.Character.Clothes.Clear();
                    iPlayer.Character.EquipedProps.Clear();

                    // Clothes
                    foreach (KeyValuePair<int, uint> kvp in outfit.Clothes.ToList())
                    {
                        
                        Cloth cloth = ClothModule.Instance.Get(kvp.Value);
                        if (cloth == null) continue;

                        if (cloth.TeamId != 0 && cloth.TeamId != iPlayer.TeamId) continue;

                        if (!iPlayer.Character.Wardrobe.Contains(kvp.Value) && cloth.TeamId == 0)
                        {
                            iPlayer.SendNewNotification("Dieses Kleidungsstück befindet sich nicht mehr in deinem Kleiderschrank!");
                            continue;
                        }

                        // Put on this ...
                        if (iPlayer.Character.Clothes.ContainsKey(kvp.Key))
                        {
                            iPlayer.Character.Clothes[kvp.Key] = kvp.Value;
                        }
                        else
                        {
                            iPlayer.Character.Clothes.Add(kvp.Key, kvp.Value);
                        }
                    }

                    // Props
                    foreach (KeyValuePair<int, uint> kvp in outfit.Props.ToList())
                    {
                        Prop prop = PropModule.Instance.Get(kvp.Value);
                        if (prop == null) continue;

                        if (prop.TeamId != 0 && prop.TeamId != iPlayer.TeamId) continue;

                        if (!iPlayer.Character.Props.Contains(kvp.Value) && prop.TeamId == 0)
                        {
                            iPlayer.SendNewNotification("Dieses Kleidungsstück befindet sich nicht mehr in deinem Kleiderschrank!");
                            continue;
                        }

                        // Put on this ...
                        if (iPlayer.Character.EquipedProps.ContainsKey(prop.Slot))
                        {
                            iPlayer.Character.EquipedProps[prop.Slot] = prop.Id;
                        }
                        else
                        {
                            iPlayer.Character.EquipedProps.Add(prop.Slot, prop.Id);
                        }
                    }


                    ClothModule.Instance.RefreshPlayerClothes(iPlayer);
                    ClothModule.SaveCharacter(iPlayer);

                    iPlayer.SendNewNotification($"Outfit {outfit.Name} angelegt!");
                    return true;
                }
                else if (index == 2)
                {
                    Outfit outfit = null;
                    try
                    {
                        outfit = iPlayer.GetData("outfit");
                    }
                    catch (Exception e)
                    {
                        return false;
                    }
                    if (outfit == null) return false;

                    iPlayer.SendNewNotification($"Outfit {outfit.Name} gelöscht!");
                    OutfitsModule.Instance.DeleteOutfit(iPlayer, outfit);
                    return true;
                }
                else
                {
                    MenuManager.DismissMenu(iPlayer.Player, (uint)PlayerMenu.OutfitsMenu);
                    return false;
                }
            }
        }
    }
}
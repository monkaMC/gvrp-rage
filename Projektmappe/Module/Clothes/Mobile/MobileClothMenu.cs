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

namespace GVRP.Module.Clothes.Mobile
{
    public class MobileClothMenuBuilder : MenuBuilder
    {
        public MobileClothMenuBuilder() : base(PlayerMenu.MobileClothMenu)
        {
        }

        public override Menu.Menu Build(DbPlayer iPlayer)
        {
            if (iPlayer == null || !iPlayer.IsValid()) return null;
            var menu = new Menu.Menu(Menu, "Kleidung");
            menu.Add(MSG.General.Close());
            if (iPlayer.Character == null || iPlayer.Character.Clothes == null || iPlayer.Character.ActiveClothes == null || iPlayer.Character.Clothes.Count <= 0) return null;

            foreach(KeyValuePair<int,uint> kvp in iPlayer.Character.Clothes)
            {
                if (!iPlayer.Character.ActiveClothes.ContainsKey(kvp.Key))
                {
                    if(kvp.Key == 1)
                        iPlayer.Character.ActiveClothes.Add(kvp.Key, false);
                    else
                        iPlayer.Character.ActiveClothes.Add(kvp.Key, true);
                }
            }
            foreach (KeyValuePair<int, uint> kvp in iPlayer.Character.EquipedProps)
            {
                if (!iPlayer.Character.ActiveProps.ContainsKey(kvp.Key))
                {
                        iPlayer.Character.ActiveProps.Add(kvp.Key, true);
                }
            }
            menu.Add("Maskierung", ""); //ComponentID 1
            menu.Add("Oberkörper", ""); //ComponentID 3
            menu.Add("Hose", ""); //ComponentID 4
            menu.Add("Schuhe", ""); //ComponentID 6
            menu.Add("Hut", ""); // AccesoireID 0
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
                if (iPlayer == null || !iPlayer.IsValid()) return false;
                if (index == 0)
                {
                    MenuManager.DismissMenu(iPlayer.Player, (uint)PlayerMenu.OutfitsMenu);
                    return false;
                }
                int accsoire = -1;
                int choice = -1;
                switch(index)
                {
                    case 1:
                        choice = 1; //Maskierung
                        break;
                    case 2:
                        choice = 11; //Oberkörper
                        break;
                    case 3:
                        choice = 4; //Hose
                        break;
                    case 4:
                        choice = 6; //Schuhe
                        break;
                    case 5:
                        accsoire = 0; //Hut
                        break;
                }
                if (choice != -1)
                {
                    if (!iPlayer.Character.Clothes.ContainsKey(choice)) return false;
                    if (!iPlayer.Character.ActiveClothes.ContainsKey(choice)) return false;
                    uint clothId = iPlayer.Character.Clothes[choice];
                    Cloth cloth = ClothModule.Instance[clothId];
                    if (cloth == null) return false;

                    if (iPlayer.Character.ActiveClothes[choice]) //Spieler hat das Kleidungsstück an
                    {
                        if (index == 1) //Maske
                        {
                            iPlayer.SetClothes(1, 0, 0);
                        }
                        else if (index == 2) //Oberkörper
                        {
                            if (iPlayer.IsMale())
                            {
                                iPlayer.SetClothes(3, 15, 0); //Torso
                                iPlayer.SetClothes(8, 15, 0); //Undershirt
                                iPlayer.SetClothes(11, 15, 0); //Tops
                            }
                            else
                            {
                                iPlayer.SetClothes(3, 15, 0); //Torso
                                iPlayer.SetClothes(8, 15, 0); //Undershirt
                                iPlayer.SetClothes(11, 15, 0); //Tops
                            }
                        }
                        else if (index == 3) //Hose
                        {
                            if (iPlayer.IsMale())
                            {
                                iPlayer.SetClothes(4, 61, 0);
                            }
                            else
                            {
                                iPlayer.SetClothes(4, 15, 0);
                            }
                        }
                        else if (index == 4) //Schuhe
                        {
                            if (iPlayer.IsMale())
                            {
                                iPlayer.SetClothes(6, 34, 0);
                            }
                            else
                            {
                                iPlayer.SetClothes(6, 35, 0);
                            }
                        }

                        iPlayer.Character.ActiveClothes[choice] = false;
                    }
                    else //Spieler hat das Kleidungsstück nicht an
                    {
                        if (index == 1) //Maske
                        {
                            iPlayer.SetClothes(1, cloth.Variation, cloth.Texture);
                        }
                        else if (index == 2) //Oberkörper
                        {
                            if (!iPlayer.Character.Clothes.ContainsKey(3) ||
                                !iPlayer.Character.Clothes.ContainsKey(8))
                                return false;

                            uint torsoClothId = iPlayer.Character.Clothes[3];
                            uint topsClothId = iPlayer.Character.Clothes[8];
                            Cloth torsoCloth = ClothModule.Instance[torsoClothId];
                            Cloth topsCloth = ClothModule.Instance[topsClothId];
                            iPlayer.SetClothes(choice, cloth.Variation, cloth.Texture);
                            iPlayer.SetClothes(3, torsoCloth.Variation, torsoCloth.Texture);
                            iPlayer.SetClothes(8, topsCloth.Variation, topsCloth.Texture);
                        }
                        else if (index == 3) //Hose
                        {
                            iPlayer.SetClothes(4, cloth.Variation, cloth.Texture);
                        }
                        else if (index == 4) //Schuhe
                        {
                            iPlayer.SetClothes(6, cloth.Variation, cloth.Texture);
                        }

                        iPlayer.Character.ActiveClothes[choice] = true;
                    }
                }
                else if(accsoire != -1)
                {
                    if (!iPlayer.Character.EquipedProps.ContainsKey(accsoire)) return false;
                    if (!iPlayer.Character.ActiveProps.ContainsKey(accsoire)) return false;
                    uint propId = iPlayer.Character.EquipedProps[accsoire];
                    Prop prop = PropModule.Instance[propId];
                    if (prop == null) return false;

                    if (iPlayer.Character.ActiveProps[accsoire]) //Spieler hat das Kleidungsstück an
                    {
                        if (index == 5) //Hut
                        {
                            iPlayer.Player.ClearAccessory(0);
                        }
                        
                        iPlayer.Character.ActiveProps[accsoire] = false;
                    }
                    else //Spieler hat das Kleidungsstück nicht an
                    {
                        if (index == 5) //Maske
                        {
                            iPlayer.Player.SetAccessories(0, prop.Variation, prop.Texture);
                        }
                        iPlayer.Character.ActiveProps[accsoire] = true;
                    }
                }
                return false;
            }
        }
    }
}
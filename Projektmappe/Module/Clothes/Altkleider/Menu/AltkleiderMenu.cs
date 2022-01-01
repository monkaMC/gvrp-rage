using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Clothes.Props;
using GVRP.Module.Items;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Windows;

namespace GVRP.Module.Clothes.Altkleider
{
    public class AltkleiderMenuBuilder : MenuBuilder
    {
        public AltkleiderMenuBuilder() : base(PlayerMenu.Altkleider)
        {
        }

        public override Menu.Menu Build(DbPlayer iPlayer)
        {
            var menu = new Menu.Menu(Menu, "Altkleider");
            menu.Add(MSG.General.Close());

            foreach (var clothId in iPlayer.Character.Wardrobe.ToList())
            {
                var cloth = ClothModule.Instance.Get(clothId);
                if (cloth == null || cloth.TeamId != (int)teams.TEAM_CIVILIAN || cloth.Gender != iPlayer.Customization.Gender) continue;
                if (cloth.Slot == 3) continue; //Körper
                menu.Add($"{cloth.Name}");
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
                    MenuManager.DismissMenu(iPlayer.Player, (uint)PlayerMenu.Altkleider);
                    ClothModule.SaveCharacter(iPlayer);
                    return false;
                }
                
                int idx = 1;

                foreach (var clothId in iPlayer.Character.Wardrobe.ToList())
                {
                    var cloth = ClothModule.Instance.Get(clothId);
                    if (cloth == null || cloth.TeamId != (int)teams.TEAM_CIVILIAN || cloth.Gender != iPlayer.Customization.Gender) continue;
                    if (cloth.Slot == 3) continue; //Körper

                    if (idx == index)
                    {
                        if(!iPlayer.Container.CanInventoryItemAdded(AltkleiderModule.AltkleiderSackId, 1))
                        {
                            iPlayer.SendNewNotification("Du hast kein Platz im Inventar!");
                            return false;
                        }

                        Logging.Logger.AddToAltkleiderLog(iPlayer.Id, "Cloth " + cloth.Id);

                        iPlayer.Character.Wardrobe.Remove(cloth.Id);
                        MySQLHandler.ExecuteAsync($"DELETE FROM `player_ownedclothes` WHERE player_id = '{iPlayer.Id}' AND clothes_id = '{cloth.Id}'");

                        MenuManager.DismissMenu(iPlayer.Player, (uint)PlayerMenu.Altkleider);
                        iPlayer.SendNewNotification($"Du hast {cloth.Name} in einen Altkleidersack verstaut!");

                        Dictionary<string, dynamic> ItemData = new Dictionary<string, dynamic>();
                        ItemData.Add(AltkleiderModule.PriceInfoString, (int)Convert.ToInt32(cloth.Price / 5));

                        iPlayer.Container.AddItem(AltkleiderModule.AltkleiderSackId, 1, ItemData);
                        return true;
                    }
                    idx++;
                }
                return false;
            }
        }
    }
}
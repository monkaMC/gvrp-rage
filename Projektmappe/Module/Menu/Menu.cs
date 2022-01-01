using System.Collections.Generic;
using Newtonsoft.Json;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Menu
{
    public class Menu
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public PlayerMenu MenuId { get; set; }

        public class Item
        {
            public string Label { get; set; }

            public string Description { get; set; }

            public Item(string label, string description)
            {
                Label = label;
                Description = description;
            }
        }

        public List<Item> Items = new List<Item>();

        public Menu(PlayerMenu menuId, string title = "", string description = "")
        {
            MenuId = menuId;
            Title = title;
            Description = description;
        }

        public void Add(string label, string description = "")
        {
            Items.Add(new Item(label, description));
        }

        public void Show(DbPlayer iPlayer, bool freeze = false)
        {
            if(iPlayer == null || !iPlayer.IsValid()) return;
            iPlayer.WatchMenu = (uint) MenuId;
            iPlayer.Player.TriggerEvent("componentServerEvent", "NativeMenu", "showNativeMenu", JsonConvert.SerializeObject(this), (uint) MenuId);
        }
    }

    public interface IMenuEventHandler
    {
        bool OnSelect(int index, DbPlayer iPlayer);
    }

    public abstract class MenuBuilder
    {
        public PlayerMenu Menu { get; set; }

        public MenuBuilder(PlayerMenu menu)
        {
            Menu = menu;
        }

        public abstract Menu Build(DbPlayer iPlayer);

        public abstract IMenuEventHandler GetEventHandler();
    }
}
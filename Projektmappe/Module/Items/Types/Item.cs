using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Items
{
    public class Item
    {
        public uint Id { get; }
        public int Durability { get; }
        public int Amount { get; set; }
        public ItemModel Model { get; }
        public Dictionary<string, dynamic> Data { get; set; }

        public Item(uint id, int durability, int amount)
        {
            Id = id;
            Durability = durability;
            Amount = amount;
            Model = ItemModelModule.Instance.GetAll().ContainsKey(id) ? ItemModelModule.Instance.Get(id) : null;
            Data = new Dictionary<string, dynamic>();
        }

        public Item(uint id, int durability, int amount, Dictionary<string, dynamic> data)
        {
            Id = id;
            Durability = durability;
            Amount = amount;

            Model = ItemModelModule.Instance.GetAll().ContainsKey(id) ? ItemModelModule.Instance.Get(id) : null;
            Data = data == null ? new Dictionary<string, dynamic>() : data;
        }

        public Item(ItemModel model, int durability, int amount)
        {
            Id = model.Id;
            Durability = durability;
            Amount = amount;
            Model = model;
            Data = new Dictionary<string, dynamic>();
        }

        public Item(ItemModel model, int durability, int amount, Dictionary<string, dynamic> data)
        {
            Id = model.Id;
            Durability = durability;
            Amount = amount;
            Model = model;
            Data = data == null ? new Dictionary<string, dynamic>() : data;
        }
    }
    
    public class SaveItem
    {
        public uint Id { get; set; }
        public int Durability { get; set; }
        public int Amount { get; set; }
        public Dictionary<string, dynamic> Data { get; }
        
        [JsonConstructor]
        public SaveItem(uint id, int durability, int amount, Dictionary<string, dynamic> data)
        {
            Id = id;
            Durability = durability;
            Amount = amount;
            Data = data == null ? new Dictionary<string, dynamic>() : data;
        }
    }
}
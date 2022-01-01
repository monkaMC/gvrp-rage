using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.Items;

namespace GVRP.Module.Items
{
    public class ClientContainerObject
    {
        public int Id { get; set; }
        public int MaxWeight { get; set; }
        public int MaxSlots { get; set; }
        public string Name { get; set; }
        public int Money { get; set; }
        public int Blackmoney { get; set; }
        public List<ClientContainerSlotObject> Slots { get; set; }

        public ClientContainerObject(int maxWeight, int maxSlots)
        {
            MaxWeight = maxWeight;
            MaxSlots = maxSlots;
            Slots = new List<ClientContainerSlotObject>();
        }

    }

    public class ClientContainerSlotObject
    {
        public int Id { get; set; }
        public int Slot { get; set; }
        public Dictionary<string, dynamic> Data { get; set; }
        public int Amount { get; set; }
        public int Weight { get; set; }
        public string Name { get; set; }
        public string ImagePath { get; set; }

        public ClientContainerSlotObject(int id, int slot, ItemModel model, Dictionary<string, dynamic> data, int amount)
        {
            Id = id;
            Slot = slot;
            Name = model.Name;
            Weight = model.Weight;
            ImagePath = model.ImagePath;
            Data = data;
            Amount = amount;
        }
    }
}

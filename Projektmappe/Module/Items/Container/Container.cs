using GTANetworkAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTANetworkMethods;
using GVRP.Module.Logging;
using GVRP.Handler;
using GVRP.Module.Vehicles;

namespace GVRP.Module.Items
{
    public enum ContainerMoveTypes
    {
        SelfInventory = 1,
        ExternInventory = 2,
    }

    public enum ContainerTempIds
    {
        Bank = 1,
        Juwe = 2,
    }

    public class Container
    {
        public uint Id { get; }
        public ContainerTypes Type { get; }
        public int MaxWeight { get; set; }
        public int MaxSlots { get; set; }
        public Dictionary<int, Item> Slots { get; }
        public bool IsUsed { get; }
        public Dictionary<int, DateTime> IntelligentContainerSaving { get; set; }

        public bool Locked { get; set; }
        public Container(MySqlDataReader reader)
        {
            try
            {
                Id = reader.GetUInt32("id");
                Type = (ContainerTypes)Enum.ToObject(typeof(ContainerTypes), reader.GetInt32("type"));
                MaxWeight = reader.GetInt32("max_weight");
                MaxSlots = reader.GetInt32("max_slots");
                IsUsed = false;
                Slots = new Dictionary<int, Item>();
                Locked = false;

                SaveItem dbItem = new SaveItem(0, 0, 0, null);
                for (int i = 0; i < ContainerManager.GetMaxSlots(Type); i++)
                {
                    dbItem = (reader.GetString("slot_" + i) == "" ? new SaveItem(0, 0, 0, null) : (reader.GetString("slot_" + i) == "[]" ? new SaveItem(0, 0, 0, null) : JsonConvert.DeserializeObject<List<SaveItem>>(reader.GetString("slot_" + i)).First()) ?? new SaveItem(0, 0, 0, null));
                    Slots.Add(i, new Item(dbItem.Id, dbItem.Durability, dbItem.Amount, dbItem.Data));
                }

                IntelligentContainerSaving = new Dictionary<int, DateTime>();
                
                ContainerManager.CheckStaticContainerInserting(this);
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        public Container(uint id, ContainerTypes type, int maxWeight, int maxSlots, Dictionary<int, Item> slots)
        {
            Id = id;
            Type = type;
            MaxWeight = maxWeight;
            MaxSlots = maxSlots;
            Slots = slots;

            IsUsed = false;
            Slots = new Dictionary<int, Item>();
            Locked = false;

            SaveItem dbItem = new SaveItem(0, 0, 0, null);
            for (int i = 0; i < ContainerManager.GetMaxSlots(type); i++)
            {
                Slots.Add(i, new Item(dbItem.Id, dbItem.Durability, dbItem.Amount, dbItem.Data));
            }

            IntelligentContainerSaving = new Dictionary<int, DateTime>();
            
            ContainerManager.CheckStaticContainerInserting(this);
        }
    }
}
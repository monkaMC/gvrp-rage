using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace GVRP.Module.Items
{
    public class ItemOrder
    {
        public uint Id { get; }
        public int ItemId { get; }
        public ItemModel Item { get; }
        public int ItemAmount { get; }
        public int OwnerId { get; }
        public int Hours { get; }
        public int NpcId { get; }
        public DateTime DateTime { get; }

        public ItemOrder(uint id, int itemId, int itemAmount, int ownerId, int hours, int npcId, DateTime dateTime)
        {
            Id = id;
            ItemId = itemId;
            Item = ItemModelModule.Instance.Get((uint)ItemId);
            ItemAmount = itemAmount;
            OwnerId = ownerId;
            Hours = hours;
            NpcId = npcId;
            DateTime = dateTime;
        }
    }
}

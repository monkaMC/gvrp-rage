
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using GVRP.Module.Logging;

namespace GVRP.Module.Items
{
    public class ItemOrderNpcItem : Loadable<uint>
    {
        public uint Id { get; }
        public int NpcId { get; }
        public Dictionary<ItemModel, int> RequiredItems { get; }
        public int RequiredMoney { get; }
        public int RewardItemId { get; }
        public ItemModel RewardItem { get; }
        public int RewardItemAmount { get; }
        public int Hours { get; }
        public int Limited { get; }
        public int RangRestricted { get; }

        public ItemOrderNpcItem(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            NpcId = reader.GetInt32("npc_id");
            RewardItemId = reader.GetInt32("reward_item_id");
            RewardItem = ItemModelModule.Instance.Get((uint)RewardItemId);
            RewardItemAmount = reader.GetInt32("reward_item_amount");
            Hours = reader.GetInt32("hours");
            RequiredMoney = reader.GetInt32("required_money");
            Limited = reader.GetInt32("limited");
            RangRestricted = reader.GetInt32("restricted_rang");

            RequiredItems = new Dictionary<ItemModel, int>();
            var itemsString = reader.GetString("required_items");
            if (itemsString.Contains(","))
            {
                string[] itemsData = itemsString.Split(',');
                foreach (var xItemData in itemsData)
                {
                    if (xItemData.Contains(":"))
                    {
                        var xItemSplit = xItemData.Split(':');
                        
                        if (xItemSplit == null || xItemSplit.Length < 2) continue;
                        ItemModel itemModel = ItemModelModule.Instance.Get(Convert.ToUInt32(xItemSplit[0]));
                        if (itemModel == null) continue;
                        if (!RequiredItems.ContainsKey(itemModel)) RequiredItems.Add(itemModel, Convert.ToInt32(xItemSplit[1]));
                    }
                }
            }
            else if (itemsString != "")
            {
                if (itemsString.Contains(":"))
                {
                    string[] xItemSplit = itemsString.Split(':');
                    if (xItemSplit != null && xItemSplit.Length > 1)
                    {
                        ItemModel itemModel = ItemModelModule.Instance.Get(Convert.ToUInt32(xItemSplit[0]));
                        if (itemModel != null)
                        {
                            if (!RequiredItems.ContainsKey(itemModel)) RequiredItems.Add(itemModel, Convert.ToInt32(xItemSplit[1]));
                        }
                    }
                }
            }
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}

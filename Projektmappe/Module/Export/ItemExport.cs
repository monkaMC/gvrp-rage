using MySql.Data.MySqlClient;
using System;
using GVRP.Handler;
using GVRP.Module.Items;

namespace GVRP.Module.Export
{
    public class ItemExport : Loadable<uint>
    {
        public ItemModel Item { get; }
        public int Price { get; set; }
        public uint NpcId { get; }

        public int MinPrice { get; }
        public int MaxPrice { get; }
        public int TempCountSaving { get; set; }

        public ItemExport(MySqlDataReader reader) : base(reader)
        {
            Item = ItemModelModule.Instance.Get(reader.GetUInt32(0));
            Price = reader.GetInt32("price");
            NpcId = reader.GetUInt32("npc_id");
            MinPrice = reader.GetInt32("min_price");
            MaxPrice = reader.GetInt32("max_price");
            TempCountSaving = 0;

            if(MinPrice > 0 && MaxPrice > 0)
            {
                Price = new Random().Next(MinPrice, MaxPrice);
            }
        }

        public override uint GetIdentifier()
        {
            return Item.Id;
        }
    }
}
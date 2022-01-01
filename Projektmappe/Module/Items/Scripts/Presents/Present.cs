using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace GVRP.Module.Items.Scripts.Presents
{
    public class Present : Loadable<uint>
    {
        public uint Id { get; }
        
        public ItemModel Item { get; set; }
        public ItemModel ResultItem { get; set; }
        public int Percent { get; set; }
        public int Amount { get; set; }

        public Present(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");

            Item = ItemModelModule.Instance.Get(reader.GetUInt32("item_id"));
            ResultItem = ItemModelModule.Instance.Get(reader.GetUInt32("result_item_id"));

            Percent = reader.GetInt32("percent");
            Amount = reader.GetInt32("amount");
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}

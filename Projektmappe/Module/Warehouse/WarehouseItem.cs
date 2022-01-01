using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace GVRP.Module.Warehouse
{
    public class WarehouseItem : Loadable<uint>
    {
        public uint Id { get; }
        public uint WareHouseId { get; set; }
        public uint RequiredItemId { get; set; }
        public int RequiredItemPrice { get; set; }

        public uint ResultItemId { get; set; }
        public int ResultItemPrice { get; set; }
        public int ResultItemBestand { get; set; }
        public int RequiredToResultItemAmount { get; set; }
        
        public WarehouseItem(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            RequiredItemId = reader.GetUInt32("required_item_id");
            ResultItemId = reader.GetUInt32("result_item_id");
            RequiredItemPrice = reader.GetInt32("required_item_price");
            ResultItemPrice = reader.GetInt32("result_item_price");
            ResultItemBestand = reader.GetInt32("result_item_bestand");
            RequiredToResultItemAmount = reader.GetInt32("required_to_result_item_amount");
            WareHouseId = reader.GetUInt32("warehouse_id");
        }

        public override uint GetIdentifier()
        {
            return Id;
        }

        public void UpdateBestand()
        {
            MySQLHandler.ExecuteAsync($"UPDATE warehouses_items SET result_item_bestand = '{ResultItemBestand}' WHERE id = '{Id}'");
        }
    }
}

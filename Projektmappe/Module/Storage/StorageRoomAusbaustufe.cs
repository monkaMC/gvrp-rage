using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace GVRP.Module.Storage
{
    public class StorageRoomAusbaustufe : Loadable<uint>
    {
        public uint Id { get; }
        public int RequiredMoney { get; }
        public string RequiredItems { get; }
        public int ToSlots { get; }
        public int ToWeight { get; }
        public int Tax { get; }

        public StorageRoomAusbaustufe(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            RequiredMoney = reader.GetInt32("required_money");
            RequiredItems = reader.GetString("required_items");
            ToSlots = reader.GetInt32("to_slots");
            ToWeight = reader.GetInt32("to_weight");
            Tax = reader.GetInt32("tax");
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}

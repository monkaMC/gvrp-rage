using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace GVRP.Module.Houses
{
    public class HouseRent
    {
        public uint Id { get; set; }
        public uint HouseId { get; set; }
        public uint PlayerId { get; set; }
        public int SlotId { get; set; }
        public int RentPrice { get; set; }

        public HouseRent(MySqlDataReader reader)
        {
            Id = reader.GetUInt32("id");
            HouseId = reader.GetUInt32("house_id");
            PlayerId = reader.GetUInt32("player_id");
            SlotId = reader.GetInt32("slot_id");
            RentPrice = reader.GetInt32("rent_price");
        }
        
        public HouseRent(uint houseId, uint playerId, int slotId, int rentPrice)
        {
            HouseId = houseId;
            PlayerId = playerId;
            SlotId = slotId;
            RentPrice = rentPrice;
        }
    }
}

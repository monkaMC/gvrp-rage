using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace GVRP.Module.VehicleRent
{
    public class PlayerVehicleDBRentKey : Loadable<uint>
    {
        public uint Id { get; set; }
        public uint OwnerId { get; set; }
        public uint PlayerId { get; set; }
        public uint VehicleId { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime EndingDate { get; set; }

        public PlayerVehicleDBRentKey(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            OwnerId = reader.GetUInt32("owner_id");
            PlayerId = reader.GetUInt32("player_id");
            VehicleId = reader.GetUInt32("vehicle_id");
            BeginDate = reader.GetDateTime("begin_date");
            EndingDate = reader.GetDateTime("ending_date");
        }
        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using GTANetworkAPI;
using GTANetworkMethods;
using MySql.Data.MySqlClient;
using GVRP.Module.Banks;
using GVRP.Module.Spawners;

namespace GVRP.Module.ClawModule
{
    public class Claw : Loadable<uint>
    {
        public uint Id { get; set; }
        public string PlayerName { get; set; }
        public uint PlayerId { get; set; }
        public uint VehicleId { get; set; }
        public bool Status { get; set; }
        public String Reason { get; set; }
        public String TimeStamp { get; set; }

        public Claw(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            PlayerName = reader.GetString("Name");
            PlayerId = reader.GetUInt32("player_id");
            VehicleId = reader.GetUInt32("vehicle_id");
            Status = reader.GetInt32("status") == 1;
            Reason = reader.GetString("reason");
            TimeStamp = reader.GetString("timestamp");
        }

        public Claw()
        {

        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}

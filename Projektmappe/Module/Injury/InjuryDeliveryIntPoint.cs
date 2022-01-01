using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace GVRP.Module.Injury
{
    public class InjuryDeliverIntPoint : Loadable<uint>
    {
        public uint Id { get; }
        public uint DeliverId { get; }
        public Vector3 Position { get; set; }
        public float Heading { get; set; }
        public int Dimension { get; set; }

        public InjuryDeliverIntPoint(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            DeliverId = reader.GetUInt32("deliver_id");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            Heading = reader.GetFloat("heading");
            Dimension = reader.GetInt32("dimension");
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
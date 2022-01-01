using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Module.NpcSpawner;

namespace GVRP.Module.Delivery
{
    public class DeliveryJobSpawnpoint : Loadable<uint>
    {

        public uint Id { get; }
        public uint DeliveryJobId { get; set; }
        public Vector3 Position { get; set; }
        public float Heading { get; set; }
        public DeliveryJobSpawnpoint(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            DeliveryJobId = reader.GetUInt32("delivery_job_id");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"),
                reader.GetFloat("pos_z"));
            Heading = reader.GetFloat("heading");
            Spawners.Markers.Create(36, Position, new Vector3(), new Vector3(), 1.0f, 255, 255, 0, 0);
        }


        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}

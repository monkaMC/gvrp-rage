using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Module.NpcSpawner;

namespace GVRP.Module.Delivery
{
    public class DeliveryJobPharmacy : Loadable<uint>
    {

        public uint Id { get; }
        public String Name { get; set; }
        public Vector3 Position { get; set; }
        public float Heading { get; set; }
        public PedHash PedHash { get; set; }
        public DeliveryJobPharmacy(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"),
                reader.GetFloat("pos_z"));
            Heading = reader.GetFloat("heading");
            PedHash = Enum.TryParse(reader.GetString("pedhash"), true, out PedHash skin) ? skin : GTANetworkAPI.PedHash.Abigail;
            new Npc(PedHash, Position, Heading, 0);

        }


        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}

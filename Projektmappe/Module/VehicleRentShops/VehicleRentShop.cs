using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace GVRP.Module.VehicleRentShops
{
    public class VehicleRentShop : Loadable<uint>
    {
        public uint Id { get; set; }
        public Vector3 Position { get; set; }
        public float Heading { get; set; }
        public int Price { get; set; }
        public uint VehicleModelId { get; set; }
        public int MaxRentAmount { get; set; }

        public VehicleRentShop(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            Heading = reader.GetFloat("heading");

        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}

using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace GVRP.Module.Jailescape
{
    public class Jailtunnel : Loadable<uint>
    {
        public uint Id { get; set; }

        public Vector3 Position { get; set; }

        public float Heading { get; set; }


        public int UsersGoTrough { get; set; }

        public Jailtunnel(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");

            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));

            Heading = reader.GetFloat("heading");

            UsersGoTrough = 0;
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}

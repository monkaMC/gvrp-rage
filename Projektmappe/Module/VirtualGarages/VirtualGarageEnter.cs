using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace GVRP.Module.VirtualGarages
{
    public class VirtualGarageEnter : Loadable<uint>
    {
        public uint Id { get; }

        public VirtualGarage VirtualGarage { get; set; }

        public Vector3 Position { get; set; }

        public Vector3 Destination { get; set; }
        public float EnterHeading { get; set; }
        public float DestinationHeading { get; set; }
        
        public uint EnterDimension { get; set; }
        public uint DestinationDimension { get; set; }

        public VirtualGarageEnter(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"),
                reader.GetFloat("pos_z"));
            Destination = new Vector3(reader.GetFloat("dest_pos_x"), reader.GetFloat("dest_pos_y"),
                reader.GetFloat("dest_pos_z"));
            EnterHeading = reader.GetFloat("enter_heading");
            DestinationHeading = reader.GetFloat("dest_heading");
            EnterDimension = reader.GetUInt32("enter_dimension");
            DestinationDimension = reader.GetUInt32("dest_dimension");
            VirtualGarage = VirtualGarageModule.Instance.Get(reader.GetUInt32("virtual_garage_id"));

        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}

using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GVRP.Module.Spawners;

namespace GVRP.Module.Jails
{
    public class JailCell : Loadable<uint>
    {
        public uint Id { get; set; }
        public Vector3 Position { get; set; }
        public float Range { get; set; }

        public int Group { get; set; }

        public JailSpawn JailSpawn { get; set; }
        
        public JailCell(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            Range = reader.GetFloat("range");
            Group = reader.GetInt32("group");

            ColShape ColShape = ColShapes.Create(Position, Range);
            ColShape.SetData("jailGroup", Group);
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}

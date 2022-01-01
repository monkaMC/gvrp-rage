using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.Spawners;

namespace GVRP.Module.InteriorProp
{
    public class InteriorsProp : Loadable<uint>
    {
        public uint Id { get; }
        public Vector3 Position { get; }
        public ColShape ColShape { get; set; }
        public float Range { get; set; }
        public string Name { get; set; }
        public List<string> Props { get; set; }
        public uint InteriorId { get; set; }
        public bool AutoLoad { get; set; }

        public InteriorsProp(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Position = new Vector3(reader.GetFloat("pos_x"),
                reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            Props = new List<string>();
            Range = reader.GetFloat("range");
            Name = reader.GetString("name");
            InteriorId = reader.GetUInt32("interior_id");
            AutoLoad = reader.GetInt32("autoload") == 1;

            ColShape = ColShapes.Create(Position, Range);
            ColShape.SetData("interiorProps", Id);
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}

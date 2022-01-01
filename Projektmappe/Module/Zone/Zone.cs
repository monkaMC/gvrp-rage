using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using MySql.Data.MySqlClient;


namespace GVRP.Module.Zone
{
    public class Zone : Loadable<uint>
    {
        public uint Id { get; }
        public string Name { get; set; }
        public float MinX { get; set; }
        public float MaxX { get; set; }
        public float MinY { get; set; }
        public float MaxY { get; set; }
        public float MinZ { get; set; }
        public float MaxZ { get; set; }

        public Zone(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
            MinX = reader.GetFloat("min_x");
            MaxX = reader.GetFloat("max_x");
            MinY = reader.GetFloat("min_y");
            MaxY = reader.GetFloat("max_y");
            MinZ = reader.GetFloat("min_z");
            MaxZ = reader.GetFloat("max_z");
        }

        public bool IsPositionInside(Vector3 position)
        {
            return (MinX <= position.X && position.X <= MaxX && MinY <= position.Y && position.Y <= MaxY && MinZ <= position.Z && position.Z <= MaxZ);
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}

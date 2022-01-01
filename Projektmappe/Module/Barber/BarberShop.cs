using System.Collections.Generic;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Module.Spawners;

namespace GVRP.Module.Barber
{
    public class BarberShop : Loadable<uint>
    {
        public uint Id { get; }
        public string Name { get; }
        public Vector3 Position { get; }
        public float Heading { get; }
        public ColShape ColShape { get; set; }

        public BarberShop(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            Heading = reader.GetFloat("heading");

            ColShape = ColShapes.Create(Position, 3.0f);
            ColShape.SetData("barberShopId", Id);
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
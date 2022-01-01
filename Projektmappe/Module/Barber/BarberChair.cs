using GTANetworkAPI;
using MySql.Data.MySqlClient;

namespace GVRP.Module.Barber
{
    public class BarberChair : Loadable<uint>
    {
        public uint Id { get; }
        public uint BarberShopId { get; }
        public Vector3 Position { get; }
        public float Heading { get; }

        public BarberChair(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            BarberShopId = reader.GetUInt32("barber_shop_id");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            Heading = reader.GetFloat("heading");
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
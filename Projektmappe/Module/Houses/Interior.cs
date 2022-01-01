using GTANetworkAPI;
using MySql.Data.MySqlClient;

namespace GVRP.Module.Houses
{
    public class Interior : Loadable<uint>
    {
        public uint Id { get; set; }
        public int Type { get; set; }
        public Vector3 Position { get; set; }
        public float Heading { get; set; }
        public int Price { get; set; }
        public string Comment { get; set; }

        public Interior(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32(0);
            Position = new Vector3(reader.GetFloat(1),
                reader.GetFloat(2), reader.GetFloat(3));
            Heading = reader.GetFloat(4);
            Type = reader.GetInt32(5);
            Price = reader.GetInt32(6);
            Comment = reader.GetString(7);
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
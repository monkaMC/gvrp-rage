using System.Collections.Generic;
using GTANetworkAPI;
using MySql.Data.MySqlClient;

namespace GVRP.Module.Players.Seat
{
    public class PlayerSeat : Loadable<int>
    {
        public int Hash { get; }
        
        public List<Place> Places { get; }

        public PlayerSeat(MySqlDataReader reader) : base(reader)
        {
            Hash = reader.GetInt32("hash");
            Places = new List<Place>();
        }

        public override int GetIdentifier()
        {
            return Hash;
        }

        public class Place
        {
            public int Hash { get; }
            public Vector3 Offset { get; }
            public float Heading { get; }
            
            public Place(MySqlDataReader reader)
            {
                Hash = reader.GetInt32("hash");
                Offset = new Vector3(reader.GetFloat("offset_x"), reader.GetFloat("offset_y"), reader.GetFloat("offset_z"));
                Heading = reader.GetFloat("offset_heading");
            }
        }
    }
}
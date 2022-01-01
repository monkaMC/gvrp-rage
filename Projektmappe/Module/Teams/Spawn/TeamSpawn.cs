
using GTANetworkAPI;
using MySql.Data.MySqlClient;

namespace GVRP.Module.Teams.Spawn
{
    public class TeamSpawn : Loadable<uint>
    {
        public uint Id { get; }
        public uint Index { get; }
        public uint TeamId { get; }
        public string Name { get; }
        public Vector3 Position { get; }
        public float Heading { get; }

        public TeamSpawn(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32(0);
            Index = reader.GetUInt32(1);
            TeamId = reader.GetUInt32(2);
            Name = reader.GetString(3);
            Position = new Vector3(reader.GetFloat(4), reader.GetFloat(5), reader.GetFloat(6));
            Heading = reader.GetFloat(7);
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
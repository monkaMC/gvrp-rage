using MySql.Data.MySqlClient;

namespace GVRP.Module.ItemPlacementFiles
{
    public class ItemPlacementFile : Loadable<uint>
    {
        public uint Id { get; }
        public string Hash { get; }
        public string Name { get; }
        public bool Active { get; }
        
        public ItemPlacementFile(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32(0);
            Hash = reader.GetString(1);
            Name = reader.GetString(2);
            Active = reader.GetBoolean(3);
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
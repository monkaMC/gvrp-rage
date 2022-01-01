using MySql.Data.MySqlClient;

namespace GVRP.Module.Assets.Tattoo
{
    public class AssetsTattooZone : Loadable<uint>
    {
        public uint Id { get; }
        public string Name { get; }

        public AssetsTattooZone(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
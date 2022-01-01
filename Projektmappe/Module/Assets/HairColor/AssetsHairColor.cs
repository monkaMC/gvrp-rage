using MySql.Data.MySqlClient;

namespace GVRP.Module.Assets.HairColor
{
    public class AssetsHairColor : Loadable<uint>
    {
        public uint Id { get; }
        public string Name { get; }
        public int CustomisationId { get; }
        public int Price { get; }

        public AssetsHairColor(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
            CustomisationId = reader.GetInt32("customisation_id");
            Price = reader.GetInt32("price");
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
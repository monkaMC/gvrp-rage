using MySql.Data.MySqlClient;

namespace GVRP.Module.Assets.Hair
{
    public class AssetsHair : Loadable<uint>
    {
        public uint Id { get; }
        public string Name { get; }
        public int CustomisationId { get; }
        public int Price { get; }
        public int Gender { get; }

        public int BarberShopId { get; }

        public AssetsHair(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
            CustomisationId = reader.GetInt32("customisation_id");
            Price = reader.GetInt32("price");
            Gender = reader.GetInt32("gender");
            BarberShopId = reader.GetInt16("shop_id");


        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
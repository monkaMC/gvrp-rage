using MySql.Data.MySqlClient;
using GVRP.Module.Barber;

namespace GVRP.Module.Assets.Beard
{
    public class AssetsBeard : Loadable<uint>
    {
        public uint Id { get; }
        public string Name { get; }
        public int CustomisationId { get; }
        public int Price { get; }
        
        public int BarberShopId { get; }

        public AssetsBeard(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
            CustomisationId = reader.GetInt32("customisation_id");
            Price = reader.GetInt32("price");
            BarberShopId = reader.GetInt16("shop_id");


        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
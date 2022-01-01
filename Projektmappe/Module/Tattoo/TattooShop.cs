using GTANetworkAPI;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Collections.Generic;
using GVRP.Module.Assets.Tattoo;
using GVRP.Module.Spawners;

namespace GVRP.Module.Tattoo
{
    public class TattooAddedItem
    {
        public int TattooLicenseId { get; set; }
        public int Price { get; set; }
        public uint AssetsTattooId { get; set; }
    }

    public class TattooShop : Loadable<uint>
    {
        public uint Id { get; }
        public string Name { get; }
        public Vector3 Position { get; }
        public float Heading { get; }
        public ColShape ColShape { get; set; }
        public int BusinessId { get; set; }
        public List<TattooAddedItem> tattooLicenses { get; set; }
        public int Price { get; }
        public int Bank { get; set; }

        public TattooShop(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            Heading = reader.GetFloat("heading");
            ColShape = ColShapes.Create(Position, 2.0f);
            ColShape.SetData("tattooShopId", Id);
            BusinessId = reader.GetInt32("business_id");
            Price = reader.GetInt32("price");
            Bank = reader.GetInt32("bank");

            tattooLicenses = reader.GetString("tattoo_licenses") != "" ? JsonConvert.DeserializeObject<List<TattooAddedItem>>(reader.GetString("tattoo_licenses")) : new List<TattooAddedItem>();
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
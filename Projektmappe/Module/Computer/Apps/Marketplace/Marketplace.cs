using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace GVRP.Module.Computer.Apps.Marketplace
{
    public class Marketplace : Loadable<uint>
    {
        [JsonIgnore]
        public uint Id { get; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; }

        [JsonProperty(PropertyName = "category_id")]
        public uint CategoryId { get; }

        [JsonProperty(PropertyName = "price")]
        public int Price { get; }

        [JsonProperty(PropertyName = "phone")]
        public int Phone { get; }

        [JsonIgnore]
        public uint PlayerId { get; }

        [JsonProperty(PropertyName = "search")]
        public bool Search { get; }

        public Marketplace(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
            Description = reader.GetString("description");
            CategoryId = reader.GetUInt32("category_id");
            Price = reader.GetInt32("price");
            Phone = reader.GetInt32("phone");
            PlayerId = reader.GetUInt32("player_id");
            Search = reader.GetInt32("search") == 1;
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
    public class Category : Loadable<uint>
    {
        [JsonIgnore]
        public uint Id { get; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; }

        [JsonProperty(PropertyName = "icon_path")]
        public string IconPath { get; }

        [JsonProperty(PropertyName = "categorys")]
        public List<Marketplace> MarketplaceCategorys { get; }

        public Category(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
            IconPath = reader.GetString("icon_path");

            MarketplaceCategorys = new List<Marketplace>();
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
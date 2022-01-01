using System.Collections.Generic;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;

namespace GVRP.Module.PointsOfInterest
{
    public class PointOfInterest : Loadable<uint>
    {
        [JsonIgnore]
        public uint Id { get; }
        [JsonProperty(PropertyName = "name")]
        public string Name { get; }
        [JsonProperty(PropertyName = "description")]
        public string Description { get; }
        [JsonIgnore]
        public uint CategoryId { get; }
        [JsonProperty(PropertyName = "x")]
        public float X { get; }
        [JsonProperty(PropertyName = "y")]
        public float Y { get; }
        [JsonProperty(PropertyName ="blip")]
        public uint Blip { get; }
        [JsonProperty(PropertyName = "blip_color")]
        public uint BlipColor { get; }



        public PointOfInterest(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
            Description = reader.GetString("description");
            CategoryId = reader.GetUInt32("category_id");
            X = reader.GetFloat("pos_X");
            Y = reader.GetFloat("pos_y");
            Blip = reader.GetUInt32("blip");
            BlipColor = reader.GetUInt32("blip_color");
        }

        public override uint GetIdentifier()
        {
            return Id;
        }

        public class Category : Loadable<uint>
        {
            [JsonIgnore]
            public uint Id { get; }
            [JsonProperty(PropertyName = "name")]
            public string Name { get; }

            [JsonProperty(PropertyName = "locations")]
            public List<PointOfInterest> PointOfInterests { get; }

            public Category(MySqlDataReader reader) : base(reader)
            {
                Id = reader.GetUInt32("id");
                Name = reader.GetString("name");
                
                PointOfInterests = new List<PointOfInterest>();
            }

            public override uint GetIdentifier()
            {
                return Id;
            }
        }
    }
}
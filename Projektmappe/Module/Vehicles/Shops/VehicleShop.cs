using System.Collections.Generic;
using GTANetworkAPI;
using Newtonsoft.Json;

namespace GVRP.Module.Vehicles.Shops
{
    public class VehicleShop
    {
        public int Id { get; set; }

        [JsonIgnore]
        public PedHash PedHash { get; set; }

        [JsonIgnore]
        public Vector3 Position { get; set; }

        [JsonIgnore]
        public float Heading { get; set; }

        [JsonIgnore]
        public int Type { get; set; }

        [JsonIgnore]
        public Vector3 SpawnPosition { get; set; }

        [JsonIgnore]
        public float SpawnHeading { get; set; }

        [JsonIgnore]
        public bool Marker { get; set; }

        [JsonIgnore]
        public bool Activated { get; set; }

        [JsonIgnore]
        public ColShape ColShape { get; set; }
        
        [JsonIgnore]
        public Blip Blip { get; set; }
        
        [JsonIgnore]
        public HashSet<int> RestrictedTeams { get; set; }

        [JsonIgnore]
        public bool TeamCarShop { get; set; }

        [JsonIgnore]
        public uint Dimension { get; set; }
        [JsonIgnore]
        public string Description { get; set; }

        public Dictionary<int, ShopVehicle> Vehicles { get; set; }

        public List<int> PlayerIds { get; set; }
    }
}
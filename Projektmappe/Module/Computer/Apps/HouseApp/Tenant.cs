using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace GVRP.Module.Computer.Apps.HouseApp
{
    public class Tenant
    {
        [JsonProperty(PropertyName = "id")]
        public int SlotId { get; set; }
        [JsonProperty(PropertyName = "player_id")]
        public uint PlayerId { get; set; }
        [JsonProperty(PropertyName = "name")]
        public String Name { get; set; }
        [JsonProperty(PropertyName = "price")]
        public int RentPrice { get; set; }
    }
}

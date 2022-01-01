using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace GVRP.Module.Computer.Apps.HouseApp
{
    public class HouseVehicle
    {
        [JsonProperty(PropertyName = "id")]
        public uint Id { get; set; }
        [JsonProperty(PropertyName = "name")]
        public String Name { get; set; }
        [JsonProperty(PropertyName = "owner")]
        public String Owner { get; set; }
    }
}

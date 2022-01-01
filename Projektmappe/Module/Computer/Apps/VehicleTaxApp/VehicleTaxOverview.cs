using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace GVRP.Module.Computer.Apps.VehicleTaxApp
{
    public class VehicleTaxOverview
    {
        [JsonProperty(PropertyName = "model")]
        public String Model { get; set; }

        [JsonProperty(PropertyName = "tax")]
        public int Tax { get; set; }

        [JsonProperty(PropertyName = "inv_weight")]
        public int Weight { get; set; }

        [JsonProperty(PropertyName = "inv_size")]
        public int Slots { get; set; }

        [JsonProperty(PropertyName = "fuel")]
        public int Fuel { get; set; }

    }
}

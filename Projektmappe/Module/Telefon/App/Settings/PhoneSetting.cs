using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace GVRP.Module.Telefon.App.Settings
{
    public class PhoneSetting
    {
        [JsonProperty(PropertyName = "flugmodus")]
        public bool flugmodus { get; set; }
        [JsonProperty(PropertyName = "lautlos")]
        public bool lautlos { get; set; }
        [JsonProperty(PropertyName = "blockCalls")]
        public bool blockCalls { get; set; }
        public PhoneSetting(bool flugmodus, bool lautlos, bool blockCalls)
        {
            this.flugmodus = flugmodus;
            this.lautlos = lautlos;
            this.blockCalls = blockCalls;
        }
    }
}

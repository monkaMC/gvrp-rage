using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace GVRP.Module.Keys
{
    public class VHKey
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "id")]
        public uint id { get; set; }

        public VHKey(string name, uint id)
        {
            this.Name = name;
            this.id = id;
        }
    }

    
}

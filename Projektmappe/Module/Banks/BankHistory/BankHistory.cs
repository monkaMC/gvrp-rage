using System;
using Newtonsoft.Json;

namespace GVRP.Module.Banks.BankHistory
{
    public class BankHistory
    {
        [JsonIgnore]
        public uint PlayerId { get; set; } // Kann ignoriert werden...

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "value")]
        public int Value { get; set; }

        [JsonIgnore]
        public DateTime Date { get; set; }
    }
    
}
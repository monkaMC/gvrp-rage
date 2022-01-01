using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using GVRP.Module.Clothes.Props;

namespace GVRP.Module.Computer.Apps.FahrzeuguebersichtApp
{
    public class PlateHistory
    {
        [JsonProperty(PropertyName = "id")]
        public uint Id { get; set; }

        [JsonProperty(PropertyName = "owner")]
        public String Owner { get; set; }

        [JsonProperty(PropertyName = "officer")]
        public String Officer { get; set; }

        [JsonProperty(PropertyName = "plate")]
        public String Plate { get; set; }
        [JsonProperty(PropertyName = "status")]
        public bool Status { get; set; }

        [JsonProperty(PropertyName = "timestamp")]
        public String TimeStamp { get; set; }

    }
}

using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using GVRP.Module.Clothes.Props;

namespace GVRP.Module.Computer.Apps.FahrzeuguebersichtApp
{
    public class OverviewVehicle
    {
        [JsonProperty(PropertyName = "id")]
        public uint Id { get; set; }

        [JsonProperty(PropertyName = "color1")]
        public uint Color1 { get; set; }

        [JsonProperty(PropertyName = "color2")]
        public uint Color2 { get; set; }

        [JsonProperty(PropertyName = "fuel")]
        public double Fuel { get; set; }

        [JsonProperty(PropertyName = "inGarage")]
        public bool InGarage { get; set; }

        [JsonProperty(PropertyName = "km")]
        public double Km { get; set; }

        [JsonProperty(PropertyName = "garage")]
        public String GarageName { get; set; }

        [JsonProperty(PropertyName = "vehiclehash")]
        public String Vehiclehash { get; set; }

        [JsonProperty(PropertyName = "besitzer")]
        public String Besitzer { get; set; }

        [JsonProperty(PropertyName = "plate")]
        public String Plate { get; set; }

        [JsonProperty(PropertyName = "carCor")]
        public CarCoorinate CarCor { get; set; }
    }

    public class CarCoorinate
    {
        [JsonProperty(PropertyName = "x")]
        public float position_x { get; set; }
        [JsonProperty(PropertyName = "y")]
        public float position_y { get; set; }
        [JsonProperty(PropertyName = "z")]
        public float position_z { get; set; }
    }
}

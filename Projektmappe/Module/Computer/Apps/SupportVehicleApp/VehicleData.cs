using Newtonsoft.Json;

namespace GVRP.Module.Computer.Apps.SupportVehicleApp
{
    public class VehicleData
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "inGarage")]
        public int InGarage { get; set; }

        [JsonProperty(PropertyName = "garage")]
        public string Garage { get; set; }

        [JsonProperty(PropertyName = "vehiclehash")]
        public string Vehiclehash { get; set; }
    }
}
using GTANetworkAPI;
using Newtonsoft.Json;
using System.Collections.Generic;
using GVRP.Handler;
using GVRP.Module.Vehicles.Data;

namespace GVRP.Module.Vehicles.Shops
{
    public class ShopVehicle
    {
        public int Id { get; set; }

        [JsonIgnore]
        public int VehicleShopId { get; set; }

        [JsonIgnore]
        public VehicleData Data { get; set; }

        [JsonIgnore]
        public Vector3 Position { get; set; }

        [JsonIgnore]
        public float Heading { get; set; }

        [JsonIgnore]
        public int PrimaryColor { get; set; }

        [JsonIgnore]
        public int SecondaryColor { get; set; }

        [JsonIgnore]
        public int Dimension { get; set; }

        public string Name { get; set; }

        public int Price { get; set; }

        [JsonIgnore]
        public SxVehicle Entity { get; set; }

        [JsonIgnore]
        public ColShape ColShape { get; set; }

        [JsonIgnore]
        public bool IsSpecialCar { get; set; }
        
        [JsonIgnore]
        public int LimitedBuyed { get; set; }


        [JsonIgnore]
        public int LimitedAmount { get; set; }

        public void IncreaseLimitedAmount()
        {
            LimitedBuyed++;
            if(LimitedBuyed >= LimitedAmount)
            {
                if(Entity != null)
                    Entity.entity.DeleteVehicle();
                if(ColShape != null)
                    ColShape.Delete();
            }
            MySQLHandler.ExecuteAsync($"UPDATE carshop_special_vehicles SET limited_buyed = '{LimitedBuyed}' WHERE carshopId = '{VehicleShopId}' AND id = '{Id}'");
            return;
        }

        public bool CanPurchased()
        {
            return (LimitedAmount == 0 || LimitedBuyed < LimitedAmount);
        }
    }
}
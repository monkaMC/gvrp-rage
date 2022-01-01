using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using Newtonsoft.Json;
using GVRP.Module.Assets.Beard;
using GVRP.Module.Assets.Chest;
using GVRP.Module.Assets.Hair;
using GVRP.Module.Assets.HairColor;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Barber
{
    public class BarberShopModule : SqlModule<BarberShopModule, BarberShop, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `barber_shops`;";
        }

        public Dictionary<uint, ListJsonBarberObject> MaleListJsonBarberObject = new Dictionary<uint, ListJsonBarberObject>();
        public Dictionary<uint, ListJsonBarberObject> FemaleListJsonBarberObject = new Dictionary<uint, ListJsonBarberObject>();



        private static uint blip = 0;
        private static int color = 0;

        protected override bool OnLoad()
        {
            PointsOfInterest.PointOfInterestModule.Instance.GetAll().TryGetValue(677, out PointsOfInterest.PointOfInterest temp);
            if (temp == null) return base.OnLoad();
            blip = temp.Blip;
            color = (int) temp.BlipColor;
            return base.OnLoad();
        }

        protected override void OnLoaded()
        {
            if (GetAll() != null)
            {
                foreach (var barber in GetAll())
                {
                    MaleListJsonBarberObject.Add(barber.Key, BarberShopModule.Instance.GetListJsonBarberObject(barber.Key, 0));
                    FemaleListJsonBarberObject.Add(barber.Key, BarberShopModule.Instance.GetListJsonBarberObject(barber.Key, 1));
                }
            }
            base.OnLoaded();
        }

        protected override void OnItemLoaded(BarberShop barber)
        {
            Main.ServerBlips.Add(Spawners.Blips.Create(barber.Position, barber.Name, 71, 1.0f, color: color));
            return;
        }

        public ListJsonBarberObject GetListJsonBarberObject(uint BarberShopId, int gender)
        {
            IEnumerable<KeyValuePair<uint, AssetsHair>> hairs = AssetsHairModule.Instance.GetAll().Where(x => x.Value.BarberShopId == BarberShopId && x.Value.Gender == gender);
            IEnumerable<KeyValuePair<uint, AssetsBeard>> beards = AssetsBeardModule.Instance.GetAll().Where(x => x.Value.BarberShopId == BarberShopId);
            IEnumerable<KeyValuePair<uint, AssetsChest>> chests = AssetsChestModule.Instance.GetAll().Where(x => x.Value.BarberShopId == BarberShopId);

            Dictionary<uint, AssetsHairColor> colors = AssetsHairColorModule.Instance.GetAll();
            ListJsonBarberObject objectToClient = new ListJsonBarberObject();

            List<JsonBarberObject> temp = new List<JsonBarberObject>();


            foreach (KeyValuePair<uint, AssetsHair> hair in hairs)
            {
                var value = hair.Value;
                var jsonBarberObject = new JsonBarberObject
                {
                    Id = value.Id,
                    CustomizationId = (uint)value.CustomisationId,
                    Name = value.Name,
                    Price = (uint) value.Price
                };

                temp.Add(jsonBarberObject);
            }
            
            objectToClient.Hairs = temp;
            temp = new List<JsonBarberObject>();

            foreach (KeyValuePair<uint, AssetsBeard> beard in beards)
            {
                var value = beard.Value;
                var jsonBarberObject = new JsonBarberObject
                {
                    Id = value.Id,
                    CustomizationId = (uint) value.CustomisationId,
                    Name = value.Name,
                    Price = (uint) value.Price
                };

                temp.Add(jsonBarberObject);
            }
            objectToClient.Beards = temp;
            temp = new List<JsonBarberObject>();
            foreach (KeyValuePair<uint, AssetsChest> chest in chests)
            {
                var value = chest.Value;
                var jsonBarberObject = new JsonBarberObject
                {
                    Id = value.Id,
                    CustomizationId = (uint) value.CustomisationId,
                    Name = value.Name,
                    Price = (uint) value.Price
                };

                temp.Add(jsonBarberObject);
            }
            objectToClient.Chests = temp;
            temp = new List<JsonBarberObject>();
            foreach (KeyValuePair<uint, AssetsHairColor> color in colors)
            {
                var value = color.Value;
                var jsonBarberObject = new JsonBarberObject
                {
                    Id = value.Id,
                    CustomizationId = (uint) value.CustomisationId,
                    Name = value.Name,
                    Price = (uint) value.Price
                };
                temp.Add(jsonBarberObject);
            }
            objectToClient.Colors = temp;
            return objectToClient;
        }
    }

    public class JsonBarberObject
    {
        [JsonProperty(PropertyName = "id")]
        public uint Id { get; set; }

        [JsonProperty(PropertyName = "price")]
        public uint Price { get; set; }

        [JsonProperty(PropertyName = "name")]
        public String Name { get; set; }

        [JsonProperty(PropertyName = "customid")]
        public uint CustomizationId { get; set; }
    }

    public class ListJsonBarberObject
    {
        [JsonProperty(PropertyName = "hairs")]
        public List<JsonBarberObject> Hairs { get; set; }
        [JsonProperty(PropertyName = "beards")]
        public List<JsonBarberObject> Beards { get; set; }
        [JsonProperty(PropertyName = "chests")]
        public List<JsonBarberObject> Chests { get; set; }

        [JsonProperty(PropertyName = "colors")]
        public List<JsonBarberObject> Colors { get; set; }

    }

}

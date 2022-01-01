using System.Collections.Generic;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Module.Items;
using GVRP.Module.Logging;

namespace GVRP.Module.Vehicles.Data
{
    public class VehicleData
    {
        public static int maxMods = 0;

        public uint Id { get; set; }
        public long Hash { get; set; }
        public string Model { get; set; }
        public int Price { get; set; }
        public float Multiplier { get; set; }
        public int Fuel { get; }
        public float FuelConsumption { get; }
        public uint ClassificationId { get; }
        public VehicleClassification Classification { get; set; }
        public Vector3 Offset { get; }
        public int InventorySize { get; }
        public int InventoryWeight { get; }
        public int modded_car { get; }
        public string mod_car_name { get; }
        public bool InGarageOnRestart { get; }
        public HashSet<uint> AllowedItems { get; set; }

        public bool IsShopVehicle { get; set; }

        public int Tax { get; }

        public int Slots { get; }

        public bool Disabled { get; }

        //For validation
        public Dictionary<int, Dictionary<int, string>> Mods { get; set; }

        //For shop
        public Dictionary<int, Dictionary<int, string>> BuyableMods { get; set; }

        public VehicleCarsellCategory CarsellCategory { get; set; }
        public VehicleData(MySqlDataReader reader)
        {
            Id = reader.GetUInt32("id");
            Hash = reader.GetInt64("hash");
            Model = reader.GetString("model");
            Price = reader.GetInt32("price");
            Multiplier = reader.GetFloat("multiplier");
            Fuel = reader.GetInt32("fuel");
            FuelConsumption = reader.GetFloat("fuel_consumption");
            ClassificationId = reader.GetUInt32("classification");
            Tax = reader.GetInt32("tax");
            InventorySize = reader.GetInt32("inv_size");
            InventoryWeight = reader.GetInt32("inv_weight");
            modded_car = reader.GetInt32("mod_car");
            mod_car_name = reader.GetString("mod_car_name");
            InGarageOnRestart = reader.GetInt32("ingarage_onrestart") == 1;
            Slots = reader.GetInt32("slots");
            Disabled = reader.GetBoolean("disabled");
            IsShopVehicle = reader.GetInt32("is_shop_vehicle") == 1;

            CarsellCategory = VehicleCarsellCategoryModule.Instance.Get(reader.GetUInt32("carsell_category"));

            if (!reader.IsDBNull(9) && !reader.IsDBNull(10) && !reader.IsDBNull(11))
            {
                Offset = new Vector3(reader.GetFloat("offset_x"), reader.GetFloat("offset_y"),
                    reader.GetFloat("offset_z"));
            }
            else
            {
                Offset = new Vector3(0,0,0);
            }

            AllowedItems = new HashSet<uint>();
            var allowedItemsString = reader.GetString("allowed_items");
            if (!string.IsNullOrEmpty(allowedItemsString))
            {
                var splittedItems = allowedItemsString.Split(',');
                foreach (var splittedItem in splittedItems)
                {
                    if (!uint.TryParse(splittedItem, out var splittedItemId)) continue;
                    AllowedItems.Add(splittedItemId);
                }
            }

            if (Multiplier > 200f) Multiplier = 1.0f;

            var staticDatas = VehicleDataModule.Instance.GetStaticDatas();
            Mods = new Dictionary<int, Dictionary<int, string>>();
            BuyableMods = new Dictionary<int, Dictionary<int, string>>();
            if (staticDatas.ContainsKey(Hash))
            {
                var staticVehicleData = staticDatas[Hash];
                if (staticVehicleData.mods != null)
                {
                    foreach (var modType in staticVehicleData.mods)
                    {
                        var modTypeMods = new Dictionary<int, string>();
                        if (modType.Value?.list == null) continue;
                        foreach (var mod in modType.Value.list)
                        {
                            if (mod.Value?.Name == null) continue;
                            var modName = mod.Value.Name;
                            modTypeMods.Add(mod.Key, modName);
                        }

                        Mods.Add(modType.Key, modTypeMods);
                    }
                }
            }

            var perlColor = new Dictionary<int, string>();
            for (var i = 0; i < 75; i++)
            {
                perlColor.Add(i, $"Perl Lack {i}");
            }

            Mods.Add(66, perlColor);
            Mods.Add(67, perlColor);

            //Todo: cleanup
            if (staticDatas.ContainsKey(Hash))
            {
                var staticVehicleData = staticDatas[Hash];
                if (staticVehicleData.mods != null)
                {
                    foreach (var modType in staticVehicleData.mods)
                    {
                        var modTypeMods = new Dictionary<int, string>();
                        if (modType.Value?.list == null) continue;
                        foreach (var mod in modType.Value.list)
                        {
                            if (mod.Value?.Name == null) continue;
                            var modName = mod.Value.Name;

                            modTypeMods.Add(mod.Key, modName);
                        }

                        BuyableMods.Add(modType.Key, modTypeMods);
                    }
                }
            }

            foreach (var mod in BuyableMods)
            {
                if (mod.Value.ContainsKey(-1))
                {
                    mod.Value.Remove(-1);
                }
            }

            foreach (var mod in Mods)
            {
                maxMods += mod.Value.Count;
            }

            /*Mods = API.Shared.getVehicleValidMods((VehicleHash) Hash);
            BuyableMods = API.Shared.getVehicleValidMods((VehicleHash) Hash);
            foreach (var mod in BuyableMods)
            {
                if (mod.Value.ContainsKey(-1))
                {
                    mod.Value.Remove(-1);
                }
            }*/
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Handler;
using GVRP.Module.Configurations;
using GVRP.Module.Vehicles.Data;
using GVRP.Module.Vehicles.Shops;

namespace GVRP.Module.Vehicles.Mod
{
    public sealed class VehicleModModule : Module<VehicleModModule>
    {
        private Dictionary<int, VehicleModType> types;

        protected override bool OnLoad()
        {
            types = LoadTypes();
            return true;
        }

        public Dictionary<int, VehicleModType> GeModTypes()
        {
            return types;
        }

        private static Dictionary<int, VehicleModType> LoadTypes()
        {
            var loadedTypes = new Dictionary<int, VehicleModType>();
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText =
                    "SELECT * FROM `vehicle_mod_types`;";
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows) return loadedTypes;
                    while (reader.Read())
                    {
                        var type = new VehicleModType(reader);
                        loadedTypes.Add(type.Id, type);
                    }
                }
            }
            return loadedTypes;
        }

        public IEnumerable<VehicleModType> GetAvailableVehicleModTypes(SxVehicle vehicle)
        {
            return from type in types
                let availableMods = GetModsForVehicleAndModType(vehicle, type.Key)
                where availableMods != null && availableMods.Count > 0
                select type.Value;
        }

        public Dictionary<int, string> GetModsForVehicleAndModType(SxVehicle vehicle, int type)
        {
            var validMods = vehicle.Data.BuyableMods;
            return validMods.ContainsKey(type) ? validMods[type] : null;
        }

        public int GetPriceByVehicleAndType(SxVehicle veh, int type)
        {
            return !types.ContainsKey(type) ? 0 : GetPriceByVehicleAndType(veh, types[type]);
        }

        public int GetPriceByVehicleAndType(SxVehicle veh, VehicleModType type)
        {
            var vehiclePricePercent = VehicleShopModule.Instance.GetVehiclePriceFromHash(veh.Data) / type.Divisor;
            if (vehiclePricePercent > type.MaxPrice)
            {
                vehiclePricePercent = type.MaxPrice;
            }
            return vehiclePricePercent;
        }

        public bool ValidateMod(VehicleData data, int type, int slot)
        {
            return data.Mods.ContainsKey(type) && data.Mods[type].ContainsKey(slot);
        }

        //Todo: generify
        public string GetTranslatedModName(string modName)
        {
            return modName; //!API.Shared.tryGetLocalizedGameText(modName, out var translation) ? modName : translation;//TODO: rage
        }
    }
}
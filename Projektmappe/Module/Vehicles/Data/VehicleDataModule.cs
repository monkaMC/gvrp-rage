using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using GVRP.Module.Configurations;
using GVRP.Module.Items;
using GVRP.Module.Logging;

namespace GVRP.Module.Vehicles.Data
{
    public sealed class VehicleDataModule : Module<VehicleDataModule>
    {
        public Dictionary<uint, VehicleData> data;

        private Dictionary<long, VehicleStaticData> staticDatas;

        public override Type[] RequiredModules()
        {
            return new[] {typeof(VehicleClassificationModule), typeof(VehicleCarsellCategoryModule)};
        }

        protected override bool OnLoad()
        {
            using (var sr = new StreamReader("vehicleInfo.json"))
            {
                var line = sr.ReadToEnd();
                staticDatas = JsonConvert.DeserializeObject<Dictionary<long, VehicleStaticData>>(line);
            }

            data = new Dictionary<uint, VehicleData>();
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText =
                    "SELECT * FROM `vehicledata`;";
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows) return false;
                    while (reader.Read())
                    {
                        var vehicleData = new VehicleData(reader);
                        if (!data.ContainsKey(vehicleData.Id))
                        {
                            data.Add(vehicleData.Id, vehicleData);
                        }
                        else
                        {
                            Logger.Print($"Duplicate vehicle data {vehicleData.Id}");
                        }

                        var classificationId = vehicleData.ClassificationId;
                        var classification = VehicleClassificationModule.Instance[classificationId];
                        if (classification != null)
                        {
                            vehicleData.Classification = classification;
                        }
                        else
                        {
                            Logger.Print($"Unknown classification in vehicle data {classificationId}");
                        }
                    }
                }
            }

            Logger.Print("MaxMods" + VehicleData.maxMods);
            return true;
        }

        public Dictionary<long, VehicleStaticData> GetStaticDatas()
        {
            return staticDatas;
        }

        public VehicleData GetDataById(uint id)
        {
            return data.TryGetValue(id, out var vehicleData) ? vehicleData : null;
        }

        public bool ContainsKey(uint id)
        {
            return data.ContainsKey(id);
        }
        
        public VehicleData GetDataByName(string model)
        {
            var searchedModel = model.ToLower();
            foreach (var vehicleData in data)
            {
                if (vehicleData.Value.Model.ToLower().StartsWith(searchedModel)) return vehicleData.Value;
            }

            return null;
        }

        public VehicleData GetDataByModel(string Model)
        {
            return data.FirstOrDefault(v => v.Value.Model == Model).Value;
        }

        public VehicleData GetData(uint hash)
        {
            return (from data in data where data.Value.Hash == hash select data.Value).FirstOrDefault();
        }
    }
}
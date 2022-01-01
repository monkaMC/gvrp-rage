using System;
using GTANetworkAPI;
using GVRP.Module.Logging;

namespace GVRP.Module.Vehicles.Garages
{
    public class GarageSpawnModule : SqlModule<GarageSpawnModule, GarageSpawn, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `garages_spawns`;";
        }

        public override Type[] RequiredModules()
        {
            return new[] {typeof(GarageModule)};
        }

        protected override void OnItemLoaded(GarageSpawn garageSpawn)
        {
            var garage = GarageModule.Instance[garageSpawn.GarageId];
            if (garage == null)
            {
                Logger.Print($"Unknown GarageId {garageSpawn.GarageId}");
            }
            else
            {
                garage.Spawns.Add(garageSpawn);
            }
        }
    }
}
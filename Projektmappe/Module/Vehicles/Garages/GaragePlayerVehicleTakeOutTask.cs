using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Tasks;

namespace GVRP.Module.Vehicles.Garages
{
    public class GaragePlayerVehicleTakeOutTask : SqlTask
    {
        private readonly Garage garage;
        private readonly uint vehicleId;
        private readonly DbPlayer dbPlayer;
        private readonly GarageSpawn spawnPosition;

        public GaragePlayerVehicleTakeOutTask(Garage garage, uint vehicleId, DbPlayer dbPlayer, GarageSpawn spawnPosition)
        {
            this.garage = garage;
            this.vehicleId = vehicleId;
            this.dbPlayer = dbPlayer;
            this.spawnPosition = spawnPosition;
        }

        public override string GetQuery()
        {
            return
                $"UPDATE `vehicles` SET `inGarage` = '0' WHERE `inGarage` = '1' AND `garage_id` = '{garage.Id}' AND `id` = '{vehicleId}';";
        }

        public override void OnFinished(int result)
        {
            if (result != 1) return;

            var spawn = garage.GetFreeSpawnPosition();

            if (spawn == null)
            {
                dbPlayer.SendNewNotification("Kein freier Ausparkpunkt.");
                return;
            }

            VehiclesModule.LoadServerVehicle(vehicleId, garage, dbPlayer, spawn);
        }
    }
}
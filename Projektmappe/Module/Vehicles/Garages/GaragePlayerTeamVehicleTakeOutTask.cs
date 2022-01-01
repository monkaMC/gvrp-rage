using GVRP.Module.Players.Db;
using GVRP.Module.Tasks;

namespace GVRP.Module.Vehicles.Garages
{
    public class GaragePlayerTeamVehicleTakeOutTask : SqlTask
    {
        private readonly uint vehicleId;
        private readonly DbPlayer dbPlayer;
        private readonly Garage garage;
        private readonly GarageSpawn spawnPosition;

        public GaragePlayerTeamVehicleTakeOutTask(Garage garage, uint vehicleId, DbPlayer dbPlayer, GarageSpawn spawnPosition)
        {
            this.vehicleId = vehicleId;
            this.dbPlayer = dbPlayer;
            this.garage = garage;
            this.spawnPosition = spawnPosition;
        }

        public override string GetQuery()
        {
            return $"UPDATE `fvehicles` SET `inGarage` = '0', `lastGarage`='{garage.Id}' WHERE `inGarage` = '1' AND `id` = '{vehicleId}' AND `team` = '{dbPlayer.Team.Id}';";
        }

        public override void OnFinished(int result)
        {
            if (result != 1) return;
            VehiclesModule.LoadServerTeamVehicle(garage, vehicleId, dbPlayer, spawnPosition);
        }
    }
}
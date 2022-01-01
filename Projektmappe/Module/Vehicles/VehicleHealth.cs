using GVRP.Handler;

namespace GVRP.Module.Vehicles
{
    public static class VehicleHealth
    {
        // repair kit id

        public static void Repair(this SxVehicle vehicle)
        {
            vehicle.RepairState = true;
            vehicle.entity.Repair();

            //set Health to max
            vehicle.entity.Health = VehicleHandler.MaxVehicleHealth;
        }

        public static void SetHealth(this SxVehicle vehicle, float health)
        {
            vehicle.RepairState = true;
            vehicle.entity.Health = health;
        }
    }
}
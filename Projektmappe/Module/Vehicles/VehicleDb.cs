using GTANetworkAPI;
using GVRP.Handler;

namespace GVRP.Module.Vehicles
{
    public static class VehicleDb
    {
        public static SxVehicle GetVehicle(this Vehicle vehicle)
        {
            if (vehicle == null) return null;
            if (!vehicle.HasData("vehicle"))
            {
                vehicle.DeleteVehicle();
                return null;
            }
            var dbVehicleData = vehicle.GetData("vehicle");
            if (dbVehicleData is SxVehicle dbVehicle)
            {
                return dbVehicle;
            }
            return null;
        }

        public static bool IsValid(this SxVehicle sxVehicle)
        {
            return sxVehicle != null && sxVehicle.entity != null && sxVehicle.Data != null && sxVehicle.entity.Handle != null;
        }
    }
}
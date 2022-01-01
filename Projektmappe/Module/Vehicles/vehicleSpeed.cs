using System;
using GTANetworkAPI;
using GVRP.Handler;

namespace GVRP.Module.Vehicles
{
    public static class VehicleSpeed
    {
        public static int GetSpeed(this SxVehicle sxVeh)
        {
            var velocity = NAPI.Entity.GetEntityVelocity(sxVeh.entity);
            var speed = Math.Sqrt(
                velocity.X * velocity.X +
                velocity.Y * velocity.Y +
                velocity.Z * velocity.Z
            );
            return Convert.ToInt32(speed * 3.6); // from m/s to km/h
        }
    }
}
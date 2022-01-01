using System;
using GTANetworkAPI;


namespace GVRP.Module.Players
{
    public static class PlayerSpeed
    {
        public static int GetSpeed(this Client player)
        {
            if (!player.IsInVehicle) return 0;
            var velocity = player.Velocity;
            var speed = Math.Sqrt(
                velocity.X * velocity.X +
                velocity.Y * velocity.Y +
                velocity.Z * velocity.Z
            );

            return Convert.ToInt32(speed * 3.6); // from m/s to km/h
        }
    }
}
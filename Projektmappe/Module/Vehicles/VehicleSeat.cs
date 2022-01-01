using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;

namespace GVRP.Module.Vehicles
{
    public static class VehicleSeat
    {
        public static int GetNextFreeSeat(this Vehicle vehicle, int offset = 0)
        {
            var seats = new bool[(int) Math.Round((double)vehicle.MaxOccupants)];

            var unavailableSeats = new HashSet<int>();

            foreach (var player in vehicle.Occupants)
            {
                unavailableSeats.Add(player.VehicleSeat);
            }

            for (int i = offset, length = (int) Math.Round((double)vehicle.MaxOccupants); i < length; i++)
            {
                if (!unavailableSeats.Contains(i))
                {
                    return i;
                }
            }

            return -2;
        }

        public static bool IsSeatFree(this Vehicle vehicle, int seat)
        {
            return vehicle.IsValidSeat(seat) && vehicle.Occupants.All(player => player.VehicleSeat != seat);
        }

        public static bool IsValidSeat(this Vehicle vehicle, int seat)
        {
            return seat > -2 && seat < vehicle.MaxOccupants - 1;
        }
    }
}

using GTANetworkAPI;
using System;
using GVRP.Module.Doors;

namespace GVRP.Module.Players
{
    public static class PlayerDoor
    {
        public static void SetDoorState(this Client player, Door door)
        {
            //TriggerEvent does not accept int64 -> Workaround. Range between -2147483648 and 4294967295 allowed.
            if (door.Model >= 0)
                player.TriggerEvent("setStateOfClosestDoorOfType", (uint)door.Model, door.Position.X, door.Position.Y, door.Position.Z, door.Locked, 0, false);
            else
                player.TriggerEvent("setStateOfClosestDoorOfType", (int)door.Model, door.Position.X, door.Position.Y, door.Position.Z, door.Locked, 0, false);

        }
    }
}
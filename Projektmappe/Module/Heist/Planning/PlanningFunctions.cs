using GTANetworkAPI;
using GVRP.Handler;
using GVRP.Module.Players;
using GVRP.Module.Vehicles;

namespace GVRP.Module.Heist.Planning
{
    public class PlanningFunctions : Script
    {
        [RemoteEvent]
        public void PlanningroomSetVehicleColor(Client player, string returnstring)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (!dbPlayer.HasData("planningroom_vehicle_tuning")) return;

            if (returnstring.Length < 2 || !returnstring.Contains(" ")) return;

            string[] splittedReturn = returnstring.Split(" ");
            if (splittedReturn.Length != 2) return;

            if (!int.TryParse(splittedReturn[0], out int color1)) return;
            if (!int.TryParse(splittedReturn[1], out int color2)) return;

            SxVehicle sxVehicle = VehicleHandler.Instance.FindTeamPlanningVehicle(dbPlayer.TeamId, (uint)dbPlayer.GetData("planningroom_vehicle_tuning"));
            if (sxVehicle == null || !sxVehicle.IsValid() || !sxVehicle.PlanningVehicle) return;

            sxVehicle.color1 = color1;
            sxVehicle.color2 = color2;

            sxVehicle.entity.PrimaryColor = color1;
            sxVehicle.entity.SecondaryColor = color2;

            dbPlayer.SendNewNotification($"Fahrzeugfarbe auf {color1} {color2} geändert!", PlayerNotification.NotificationType.SUCCESS);
            return;
        }

        [RemoteEvent]
        public void PlanningroomSetVehiclePlate(Client player, string returnstring)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (!dbPlayer.HasData("planningroom_vehicle_tuning")) return;

            if (returnstring.Length < 2) return;

            SxVehicle sxVehicle = VehicleHandler.Instance.FindTeamPlanningVehicle(dbPlayer.TeamId, (uint)dbPlayer.GetData("planningroom_vehicle_tuning"));
            if (sxVehicle == null || !sxVehicle.IsValid() || !sxVehicle.PlanningVehicle) return;

            sxVehicle.plate = returnstring;
            sxVehicle.entity.NumberPlate = returnstring;

            dbPlayer.SendNewNotification($"Kennzeichen auf {returnstring} geändert!", PlayerNotification.NotificationType.SUCCESS);
            return;
        }
    }
}

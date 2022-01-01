using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Handler;
using GVRP.Module.Chat;
using GVRP.Module.Freiberuf.Mower;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Freiberuf
{
    public static class FreiberufFunctions
    {
        public static List<SxVehicle> JobVehicles = new List<SxVehicle>();

        public static void RemoveJobVehicleIfExist(this DbPlayer dbPlayer, int catId)
        {
            foreach (var vehicle in VehicleHandler.Instance.GetPlayerVehicles(dbPlayer.Id))
            {
                if (vehicle == null) continue;
                if (vehicle.jobid != catId || vehicle.ownerId != dbPlayer.Id) continue;
                VehicleHandler.Instance.DeleteVehicle(vehicle);
                return;
            }
        }

        public static bool IsJobVehicleAtPoint(this DbPlayer dbPlayer, Vector3 pos)
        {
            foreach (SxVehicle Vehicle in VehicleHandler.Instance.GetJobVehicles())
            {
                if (Vehicle == null) continue;
                if (Vehicle.jobid != MowerModule.MowerJobVehMarkId && Utils.IsPointNearPoint(7.0f, Vehicle.entity.Position, pos))
                {
                    dbPlayer.SendNewNotification("Ein Fahrzeug blockiert derzeit den Ausparkpunkt!", PlayerNotification.NotificationType.FREIBERUF, "Freiberuf");
                    return true;
                }

                if (Utils.IsPointNearPoint(5.0f, Vehicle.entity.Position, pos))
                {
                    dbPlayer.SendNewNotification("Jemand anderes hat gerade den Job begonnen, bitte warte kurz!", PlayerNotification.NotificationType.FREIBERUF, "Freiberuf");
                    return true;
                }
            }
            return false;
        }

        public static SxVehicle GetJobVehicle(this DbPlayer dbPlayer, int catId)
        {
            foreach (var vehicle in VehicleHandler.Instance.GetJobVehicles())
            {
                if (vehicle == null) continue;
                if (vehicle.jobid != catId || vehicle.ownerId != dbPlayer.Id) continue;
                return vehicle;
            }
            return null;
        }

        public static SxVehicle GetNearestJobVehicle(this DbPlayer dbPlayer, int catId, float range)
        {
            foreach (var vehicle in VehicleHandler.Instance.GetClosestJobVehicles(dbPlayer.Player.Position, range))
            {
                if (vehicle == null) continue;
                if (vehicle.jobid != catId) continue;
                return vehicle;
            }
            return null;
        }

        public static List<SxVehicle> GetAllJobVehicles()
        {
            return JobVehicles;
        }
    }
}

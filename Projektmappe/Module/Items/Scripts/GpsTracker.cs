using GTANetworkMethods;
using System;
using GVRP.Module.Chat;
using System.Threading.Tasks;

using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Vehicles;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool GpsTracker(DbPlayer iPlayer, ItemModel ItemData)
        {
            if (!iPlayer.Player.IsInVehicle) return false;
            {
                if (iPlayer.job[0] != (int) jobs.JOB_MECH)
                {
                    iPlayer.SendNewNotification( MSG.Error.NoPermissions());
                    return false;
                }


                var vehicle = iPlayer.Player.Vehicle.GetVehicle();
                if (vehicle.databaseId == 0) return false;
                if (!vehicle.GpsTracker)
                {
                    //Vehicle has no gps tracker
                    var table = vehicle.IsTeamVehicle() ? "fvehicles" : "vehicles";
                    MySQLHandler.ExecuteAsync($"UPDATE {table} SET gps_tracker = 1 WHERE id = {vehicle.databaseId}");
                    vehicle.GpsTracker = true;
                    iPlayer.SendNewNotification("Der GPS-Tracker wurde eingebaut.");
       //             iPlayer.JobSkillsIncrease(2);
                }
                else
                {
                    //Vehicle already has gps tracker
                    iPlayer.SendNewNotification("Dieses Fahrzeug ist bereits mit einem GpsTracker ausgestattet.");
                    return false;
                }
                // RefreshInventory
                return true;
            }
        }
    }
}
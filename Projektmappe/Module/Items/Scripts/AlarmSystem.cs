using GTANetworkMethods;
using GVRP.Module.Chat;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Vehicles;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool AlarmSystem(DbPlayer iPlayer, ItemModel ItemData)
        {
            if (!iPlayer.Player.IsInVehicle) return false;
            {
                if (iPlayer.Team.Id == (int)teams.TEAM_LSC)
                {
                    var vehicle = iPlayer.Player.Vehicle.GetVehicle();
                    if (vehicle.databaseId == 0) return false;
                    if (!vehicle.GpsTracker)
                    {
                        //Vehicle has no gps tracker
                        var table = vehicle.IsTeamVehicle() ? "fvehicles" : "vehicles";
                        MySQLHandler.ExecuteAsync($"UPDATE {table} SET alarm_system = 1 WHERE id = {vehicle.databaseId}");
                        vehicle.GpsTracker = true;
                        iPlayer.SendNewNotification("Die Alarmanlage wurde eingebaut.");
                    }
                    else
                    {
                        //Vehicle already has gps tracker
                        iPlayer.SendNewNotification("Dieses Fahrzeug ist bereits mit einer AlarmAnlage ausgestattet.");
                        return false;
                    }
                }

            }
            return false;
        }
    }
}
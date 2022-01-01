using System;
using GVRP.Handler;
using GVRP.Module.Chat;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Vehicles;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static int TuningRadius = 5;

        public static bool TuningParts(DbPlayer iPlayer, ItemModel ItemData)
        {
            iPlayer.SendNewNotification("Das Teil passt nicht mehr!", PlayerNotification.NotificationType.ERROR);
            return false;

            /* D E A K T I V I E R T
             * if (!(iPlayer.TeamId == 0))
            {
                iPlayer.SendNewNotification("Nur geschultes Personal vom LSC kann Tuningteile anbringen!");
                return false;
            }

            var playerPosition = iPlayer.Player.Position;
            if (playerPosition.DistanceTo(new GTANetworkAPI.Vector3(733.606, -1083.12, 22.1689)) > TuningRadius ||
                playerPosition.DistanceTo(new GTANetworkAPI.Vector3(-212.341, -1326.57, 30.8904)) > TuningRadius ||
                playerPosition.DistanceTo(new GTANetworkAPI.Vector3(-1156.03, -2012.04, 13.1803)) > TuningRadius ||
                playerPosition.DistanceTo(new GTANetworkAPI.Vector3(-334.72, -135.036, 39.0096)) > TuningRadius ||
                playerPosition.DistanceTo(new GTANetworkAPI.Vector3(1178.52, 2639.42, 37.7538)) > TuningRadius)
            {
                iPlayer.SendNewNotification("Sie müssen in einer Werkstatt sein.", PlayerNotification.NotificationType.ERROR, "Fehler");
                return false;
            }

            if (!Configurations.Configuration.Instance.TuningActive || !iPlayer.IsValid()) return false;
            if (!iPlayer.Player.IsInVehicle || iPlayer.Player.Vehicle == null) return false;

            // Get Closest Car
            SxVehicle sxVeh = iPlayer.Player.Vehicle.GetVehicle();
            if (sxVeh != null && sxVeh.IsValid() && (sxVeh.IsPlayerVehicle() || sxVeh.IsTeamVehicle()))
            {
                string[] parts = ItemData.Script.ToLower().Replace("tune_", "").Split('_');
                int modid = Convert.ToInt32(parts[0]);
            
                iPlayer.SetData("tuneVeh", sxVeh.databaseId);
                iPlayer.SetData("tuneSlot", modid);
                iPlayer.SetData("tuneIndex", 0);
            
                iPlayer.Player.TriggerEvent("hideInventory");
                iPlayer.watchDialog = 0;
                iPlayer.ResetData("invType");
            
                MenuManager.Instance.Build(PlayerMenu.MechanicTune, iPlayer).Show(iPlayer);
                return true;
            }
            else
            {
                iPlayer.SendNewNotification(
                     "Dieses Fahrzeug können Sie nicht tunen!");
                return false;
            }*/
        }
    }
}
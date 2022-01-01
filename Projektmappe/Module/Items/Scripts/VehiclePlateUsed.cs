using System.Threading.Tasks;
using GVRP.Handler;
using GVRP.Module.Chat;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Vehicles;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> VehiclePlateUsed(DbPlayer iPlayer, Item item)
        {
            if (item.Data == null) return false;
            if (!item.Data.ContainsKey("Plate")) return false;
            string plate = (string)item.Data["Plate"];

            //if player is in vehicle put on plate
            if (iPlayer.Player.IsInVehicle)
            {
                SxVehicle sxVehicle = iPlayer.Player.Vehicle.GetVehicle();
                if (sxVehicle == null) return false;

                if (sxVehicle.IsPlayerVehicle() && sxVehicle.ownerId != iPlayer.Id)
                {
                    iPlayer.SendNewNotification("Nicht dein Fahrzeug!");
                    return false;
                }

                if (sxVehicle.IsTeamVehicle() && sxVehicle.teamid != iPlayer.Team.Id)
                {
                    iPlayer.SendNewNotification("Nicht dein Fahrzeug!");
                    return false;
                }

                if (sxVehicle.SyncExtension.EngineOn)
                {
                    iPlayer.SendNewNotification("Der Motor des Fahrzeugs muss für diesen Vorgang ausgeschaltet sein.");
                    return false;
                }

                iPlayer.SendNewNotification("Nummernschild wird angebracht...");
                iPlayer.Container.RemoveItem(596);
                sxVehicle.CanInteract = false;
                Chats.sendProgressBar(iPlayer, 60000);
                await Task.Delay(60000);
                iPlayer.SendNewNotification("Sitzt, wackelt und kriegt Luft... " + plate);
                sxVehicle.entity.NumberPlate = plate;
                sxVehicle.CanInteract = true;
                Logging.Logger.AddVehiclePlateLog(iPlayer.Id, sxVehicle.databaseId, plate);

                return true;
            }
            else
            {
                //show numberplate
                iPlayer.SendNewNotification(plate, PlayerNotification.NotificationType.INFO, "Kennzeichen");
                return false;
            }
        }
    }
}
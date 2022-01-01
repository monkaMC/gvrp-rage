using GTANetworkAPI;
using System.Collections.Generic;
using System.Threading.Tasks;
using GVRP.Handler;
using GVRP.Module.Delivery;
using GVRP.Module.Events.Halloween;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Vehicles;

namespace GVRP.Module.AsyncEventTasks
{
    public static partial class AsyncEventTasks
    {
        public static void PlayerEnterVehicleTask(Client player, Vehicle vehicle, sbyte seat)
        {

            Task.Delay(1000);
            //Todo: maybe save vehicle and player position here
            DbPlayer iPlayer = player.GetPlayer();

            if (iPlayer == null || vehicle == null)
            {
                return;
            }

            Modules.Instance.OnPlayerEnterVehicle(iPlayer, vehicle, seat);

            if (!vehicle.HasData("serverhash") || (string)vehicle.GetData("serverhash") != "1312asdbncaw13JADGWSh1")
            {
                Players.Players.Instance.SendMessageToAuthorizedUsers("anticheat", $"ANTI CARHACK " + player.Name);
                vehicle.Delete();
                return;
            }

            SxVehicle sxVeh = vehicle.GetVehicle();
            if (sxVeh == null || !sxVeh.IsValid())
            {
                return;
            }

            // Respawnstate
            sxVeh.respawnInteractionState = true;

            if (sxVeh.jobid > 0)
            {
                if (player.VehicleSeat == -1 && sxVeh.jobid != iPlayer.job[0] && sxVeh.jobid != 99 &&
                    sxVeh.jobid != 999 && sxVeh.jobid != -1)
                {
                    if (sxVeh.jobid == 999 && (iPlayer.RankId == 0))
                    {
                        player.WarpOutOfVehicle();
                    }
                }
            }

            VehicleHandler.Instance.AddPlayerToVehicleOccupants(sxVeh, player.GetPlayer(), seat);

            float newVehicleHealth = NAPI.Vehicle.GetVehicleEngineHealth(vehicle) + NAPI.Vehicle.GetVehicleBodyHealth(vehicle);
            player.TriggerEvent("initialVehicleData", sxVeh.fuel.ToString().Replace(",", "."), sxVeh.Data.Fuel.ToString().Replace(",", "."), newVehicleHealth.ToString().Replace(",", "."),
            VehicleHandler.MaxVehicleHealth.ToString().Replace(",", "."), sxVeh.entity.MaxSpeed.ToString().Replace(",", "."), sxVeh.entity.Locked ? "true" : "false", string.Format("{0:0.00}", sxVeh.Distance).Replace(",", "."), sxVeh.entity.EngineStatus ? "true" : "false");

            Task.Run(async () =>
            {
                await Task.Delay(1000);// Workaround for locked vehs

            // Resync Entity Lock & Engine Status
            if (sxVeh.SyncExtension != null)
                {
                    NAPI.Vehicle.SetVehicleEngineStatus(sxVeh.entity, sxVeh.SyncExtension.EngineOn);
                    NAPI.Vehicle.SetVehicleLocked(sxVeh.entity, sxVeh.SyncExtension.Locked);
                    if (seat == -1)
                    {
                        iPlayer.Player.TriggerEvent("setPlayerVehicleMultiplier", sxVeh.DynamicMotorMultiplier);
                        sxVeh.LastDriver = iPlayer.GetName();
                    }
                }

                if (sxVeh.entity.Locked || sxVeh.SyncExtension.Locked || iPlayer.IsTied || iPlayer.IsCuffed)
                {
                    if (iPlayer.HasData("vehicleData"))
                    {
                        player.WarpOutOfVehicle();
                    }
                }

            });

            if (iPlayer.HasData("delivery_has_package") && iPlayer.GetData("delivery_has_package") == true)
            {
                iPlayer.SendNewNotification("Du hast noch deine Ladung dabei! Sie muss in das Firmenfahrzeug gelegt werden!");
                player.WarpOutOfVehicle();
                return;
            }

            if (iPlayer.HasData("delivery_tour_start"))
            {


                //Finde offene Aufträge
                if (DeliveryJobModule.Instance.DeliveryOrders.TryGetValue(iPlayer, out DeliveryOrder deliveryOrder))
                {
                    if (iPlayer.HasData("delivery_tour_start"))
                    {
                        iPlayer.SetData("delivery_tour_start", true);

                        if (deliveryOrder.NextPosition == null)
                        {
                            //Spieler startet nun
                            foreach (KeyValuePair<Vector3, bool> keyValuePair in deliveryOrder.DeliveryPositions)
                            {
                                if (keyValuePair.Value == false)
                                {
                                    iPlayer.SendNewNotification("Dein nächstes Ziel wurde markiert. Bring die Ware dort hin und liefer sie ab.");
                                    iPlayer.Player.TriggerEvent("setPlayerGpsMarker", keyValuePair.Key.X, keyValuePair.Key.Y);
                                    deliveryOrder.NextPosition = keyValuePair.Key;
                                    return;
                                }
                            }
                            //Spieler hat keine offenen Aufträge mehr und ist somit fertig
                            iPlayer.SendNewNotification("Du hast deine letzte Lieferung abgegeben. Fahr nun zurück zum Auftraggeber um den Auftrag zu beenden.");
                            iPlayer.Player.TriggerEvent("setPlayerGpsMarker", deliveryOrder.DeliveryJob.Position.X, deliveryOrder.DeliveryJob.Position.Y);
                        }
                        else
                        {
                            iPlayer.SendNewNotification("Dein Ziel wurde auf der Karte markiert. Bring die Ware zum Ziel.");
                            iPlayer.Player.TriggerEvent("setPlayerGpsMarker", deliveryOrder.NextPosition.X, deliveryOrder.NextPosition.Y);
                            return;
                        }
                    }
                }
            }

        }
    }
}

using System;
using System.Linq;
using GTANetworkAPI;
using System.Threading.Tasks;
using GVRP.Handler;
using GVRP.Module.Blitzer;
using GVRP.Module.Chat;
using GVRP.Module.ClawModule;
using GVRP.Module.Injury;
using GVRP.Module.Logging;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.PlayerAnimations;
using GVRP.Module.Vehicles;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> wkasten(DbPlayer iPlayer, ItemModel ItemData)
        {
            if (!iPlayer.CanInteract()) return false;

            //if in vehicle remove numberplate
            if (iPlayer.Player.IsInVehicle)
            {
                SxVehicle sxVehicle = iPlayer.Player.Vehicle.GetVehicle();

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
                    iPlayer.SendNewNotification("Der Motor des Fahrzeugs muss für diesen Vorgang ausgeschaltet sein");
                    return false;
                }

                sxVehicle.CanInteract = false;
                iPlayer.SendNewNotification("Nummernschild wird entfernt...");
                Chats.sendProgressBar(iPlayer, 25000);
                iPlayer.SetCannotInteract(true);
                await Task.Delay(25000);
                iPlayer.SetCannotInteract(false);
                sxVehicle.entity.NumberPlate = "";
                iPlayer.SendNewNotification("Sie haben das Kennzeichen entfernt");
                sxVehicle.CanInteract = true;

                return true;
            }
            else
            {
                //not in vehicle -> trying to remove PoliceObject
                if (iPlayer.IsCuffed || iPlayer.IsTied)
                {
                    return false;
                }
                else
                {
                    SxVehicle sxVehicle = VehicleHandler.Instance.GetClosestVehicle(iPlayer.Player.Position);
                    if (sxVehicle != null && (iPlayer.Team.Id == (int)teams.TEAM_POLICE || iPlayer.Team.Id == (int)teams.TEAM_COUNTYPD) && iPlayer.IsInDuty())
                    {
                        if (sxVehicle.WheelClamp == 0)
                        {
                            iPlayer.SendNewNotification("An diesem Fahrzeug ist keine Kralle angebracht...");
                            return false;
                        }

                        Chats.sendProgressBar(iPlayer, 60000);
                        iPlayer.PlayAnimation((int) (AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@world_human_welding@male@idle_a", "idle_a");
                        iPlayer.Player.TriggerEvent("freezePlayer", true);
                        iPlayer.SetCannotInteract(true);
                        await Task.Delay(60000);
                        iPlayer.SetCannotInteract(false);
                        iPlayer.Player.TriggerEvent("freezePlayer", false);
                        NAPI.Player.StopPlayerAnimation(iPlayer.Player);

                        if (sxVehicle != null && sxVehicle.IsValid() && sxVehicle.entity.Position.DistanceTo(iPlayer.Player.Position) < 10.0)
                        {
                            if (iPlayer.isInjured() || iPlayer.IsCuffed || iPlayer.IsTied) return false;
                            sxVehicle.WheelClamp = 0;
                            String updateString = $"UPDATE {(sxVehicle.IsTeamVehicle() ? "fvehicles" : "vehicles")} SET WheelClamp = '0' WHERE id = '{sxVehicle.databaseId}'";
                            MySQLHandler.ExecuteAsync(updateString);
                            iPlayer.SendNewNotification("Die Parkkralle wurde erfolgreich geöffnet und entfernt.");
                            Logger.AddVehicleClawLog(iPlayer.Id, sxVehicle.databaseId, "wkasten", true);
                            Claw claw = new Claw();
                            claw.Id = ClawModule.ClawModule.Instance.GetAll().OrderByDescending(c => c.Key).First().Key + 1;
                            claw.PlayerId = iPlayer.Id;
                            claw.PlayerName = iPlayer.Player.Name;
                            claw.Reason = "wkasten";
                            claw.VehicleId = sxVehicle.databaseId;
                            claw.TimeStamp = DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
                            claw.Status = false;
                            ClawModule.ClawModule.Instance.Add(claw.Id, claw);
                            return true;
                        }
                        return false;


                    }
                    else
                    {
                        PoliceObject pObject;
                        if ((pObject = PoliceObjectModule.Instance.GetNearest(iPlayer.Player.Position)) != null)
                        {
                            if (!iPlayer.Container.CanInventoryItemAdded(pObject.Item, 1))
                            {
                                iPlayer.SendNewNotification("Dein inventar ist voll!");
                                return false;
                            }

                            // Remove Blitzer if Item is one
                            if (pObject.Item.Id == 484)
                            {
                                if (iPlayer.Team.Id != (uint) teams.TEAM_POLICE && iPlayer.Team.Id != (uint)teams.TEAM_COUNTYPD) return false;
                                BlitzerModule.Instance.RemoveBlitzer(BlitzerModule.Instance.GetNearestBlitzer(iPlayer));
                            }
                            else if (pObject.Item.Id == 485)
                            {
                                if (iPlayer.Team.Id != (uint) teams.TEAM_POLICE && iPlayer.Team.Id != (uint)teams.TEAM_COUNTYPD) return false;
                                BlitzerModule.Instance.RemoveBlitzer(BlitzerModule.Instance.GetNearestBlitzer(iPlayer));
                            }

                            PoliceObjectModule.Instance.Delete(pObject);
                            iPlayer.PlayAnimation(
                                (int) (AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["fixing"].Split()[0], Main.AnimationList["fixing"].Split()[1]);
                            iPlayer.Player.TriggerEvent("freezePlayer", true);
                            iPlayer.SetCannotInteract(true);
                            await Task.Delay(4000);
                            iPlayer.SetCannotInteract(false);
                            iPlayer.Player.TriggerEvent("freezePlayer", false);
                            NAPI.Player.StopPlayerAnimation(iPlayer.Player);

                            iPlayer.Container.AddItem(pObject.Item, 1);
                            iPlayer.SendNewNotification("Erfolgreich entfernt!");
                            return true;
                        }
                    }






                }
            }
            return false;
        }
    }
}
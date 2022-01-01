using GTANetworkAPI;
using System;
using System.Linq;
using System.Threading.Tasks;
using GVRP.Handler;
using GVRP.Module.Chat;
using GVRP.Module.Configurations;
using GVRP.Module.Doors;
using GVRP.Module.GTAN;
using GVRP.Module.Heist.Planning;
using GVRP.Module.Injury;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.JumpPoints;
using GVRP.Module.Players.PlayerAnimations;
using GVRP.Module.Teams;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> Brecheisen(DbPlayer iPlayer, ItemModel ItemData)
        {
            if (!iPlayer.CanInteract()) return false;
            // Check Door
            if (iPlayer.TryData("doorId", out uint doorId))
            {
                var door = DoorModule.Instance.Get(doorId);
                if (door != null)
                {
                    if (door.OpenWithWelding || door.AdminUnbreakable || door.OpenWithHacking) return false;
                    if (!door.Locked)
                    {
                        iPlayer.SendNewNotification("Tuer ist bereits aufgeschlossen!", notificationType: PlayerNotification.NotificationType.SUCCESS);
                        return false;
                    }
                    if (door.LastBreak.AddMinutes(5) > DateTime.Now) return false; // Bei einem Break, kann 5 min nicht interagiert werden

                        Chats.sendProgressBar(iPlayer, 20000);

                    
                        iPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "missheistdockssetup1ig_3@talk", "oh_hey_vin_dockworker");
                        iPlayer.Player.TriggerEvent("freezePlayer", true);
                        iPlayer.SetData("userCannotInterrupt", true);

                        await Task.Delay(20000);
                        iPlayer.ResetData("userCannotInterrupt");

                        if (iPlayer.IsCuffed || iPlayer.IsTied || iPlayer.isInjured()) return true;

                        iPlayer.Player.TriggerEvent("freezePlayer", false);

                        door.Break();
                        
                        iPlayer.SendNewNotification("Tuer aufgebrochen!", notificationType:PlayerNotification.NotificationType.SUCCESS);
                        NAPI.Player.StopPlayerAnimation(iPlayer.Player);
                        return true;
                }
            }

            if (iPlayer.Team.IsGangsters())
            {
                foreach (StaticContainer staticContainer in StaticContainerModule.Instance.GetAll().Values.ToList())
                {
                    if (staticContainer.Locked && staticContainer.Position.DistanceTo(iPlayer.Player.Position) <= staticContainer.Range)
                    {
                        if (staticContainer.Id == (uint)StaticContainerTypes.PLANNINGOUTFITMR1 || staticContainer.Id == (uint)StaticContainerTypes.PLANNINGOUTFITMR2 || staticContainer.Id == (uint)StaticContainerTypes.PLANNINGOUTFITMR3 || staticContainer.Id == (uint)StaticContainerTypes.PLANNINGOUTFITMR4 || staticContainer.Id == (uint)StaticContainerTypes.PLANNINGOUTFITMR5 || staticContainer.Id == (uint)StaticContainerTypes.PLANNINGOUTFITMR6 || staticContainer.Id == (uint)StaticContainerTypes.PLANNINGOUTFITMR7 || staticContainer.Id == (uint)StaticContainerTypes.PLANNINGOUTFITMR8 || staticContainer.Id == (uint)StaticContainerTypes.PLANNINGOUTFITMR9 || staticContainer.Id == (uint)StaticContainerTypes.PLANNINGOUTFITMR10)
                        {
                            PlanningRoom room = PlanningModule.Instance.GetPlanningRoomByTeamId(iPlayer.Team.Id);

                            DateTime actualDate = DateTime.Now;
                            if (iPlayer.Team.LastOutfitPreQuest.AddHours(1) >= actualDate)
                            {
                                iPlayer.SendNewNotification("Die Pre-Quest zur Beschaffung der Outfits ist nicht aktiv!", PlayerNotification.NotificationType.ERROR);
                                return false;
                            }

                            if (room.PlanningOutfitCounter >= 2)
                            {
                                iPlayer.SendNewNotification("Es können nur maximal 2 Spinde zur selben Zeit geöffnet werden!", PlayerNotification.NotificationType.ERROR);
                                return false;
                            }

                            int time = 180000;
                            if (Configuration.Instance.DevMode) time = 60000;

                            iPlayer.SendNewNotification("Sie beginnen nun damit den Spind aufzubrechen!", PlayerNotification.NotificationType.INFO);
                            room.PlanningOutfitCounter++;

                            Chats.sendProgressBar(iPlayer, time);

                            iPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "missheistdockssetup1ig_3@talk", "oh_hey_vin_dockworker");
                            iPlayer.Player.TriggerEvent("freezePlayer", true);
                            iPlayer.SetData("userCannotInterrupt", true);

                            TeamModule.Instance.SendChatMessageToDepartments($"Es wurde ein nicht autorisierter Zugriff gemeldet! Deltares-Cooperation Secure System - Objekt {staticContainer.Name}!");

                            await Task.Delay(time);

                            iPlayer.ResetData("userCannotInterrupt");
                            if (iPlayer.IsCuffed || iPlayer.IsTied || iPlayer.isInjured()) return true;
                            iPlayer.Player.TriggerEvent("freezePlayer", false);
                            NAPI.Player.StopPlayerAnimation(iPlayer.Player);

                            iPlayer.SendNewNotification("Spind aufgebrochen!", notificationType: PlayerNotification.NotificationType.SUCCESS);

                            staticContainer.Container.ClearInventory();
                            staticContainer.Container.AddItem(PlanningModule.Instance.CasinoRequiredOutfitId, 1);

                            staticContainer.Locked = false;
                            room.PlanningOutfitCounter--;
                            PlanningModule.Instance.LastOutfitRob = DateTime.Now;
                            return true;
                        }
                    }
                }
            }

            // Check Jumppoint
            if (iPlayer.TryData("jumpPointId", out int jumpPointId))
            {
                var jumpPoint = JumpPointModule.Instance.Get(jumpPointId);
                if (jumpPoint != null)
                {
                    if (jumpPoint.Unbreakable) return false;
                    if (jumpPoint.AdminUnbreakable) return false;
                    if (!jumpPoint.Locked)
                    {
                        iPlayer.SendNewNotification("Eingang ist bereits aufgeschlossen!", notificationType: PlayerNotification.NotificationType.SUCCESS);
                        return false;
                    }
                    if (jumpPoint.LastBreak.AddMinutes(5) > DateTime.Now) return false; // Bei einem Break, kann 5 min nicht interagiert werden

                    Chats.sendProgressBar(iPlayer, 30000);

                    
                    iPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "missheistdockssetup1ig_3@talk", "oh_hey_vin_dockworker");
                    iPlayer.Player.TriggerEvent("freezePlayer", true);
                    iPlayer.SetCannotInteract(true);

                    await Task.Delay(30000);

                    iPlayer.SetCannotInteract(false);
                    iPlayer.Player.TriggerEvent("freezePlayer", false);
                    jumpPoint.Locked = false;
                    jumpPoint.LastBreak = DateTime.Now;
                    jumpPoint.Destination.Locked = false;
                    jumpPoint.Destination.LastBreak = DateTime.Now;

                    iPlayer.SendNewNotification("Eingang aufgebrochen!", notificationType:PlayerNotification.NotificationType.SUCCESS);
                    NAPI.Player.StopPlayerAnimation(iPlayer.Player);
                    return true;
                }
            }

            var l_Vehicle = VehicleHandler.Instance.GetClosestVehicle(iPlayer.Player.Position, 3.0f);
            if (l_Vehicle == null)
                return false;

            if (l_Vehicle.entity.Model != (uint)VehicleHash.Journey && l_Vehicle.entity.Model != (uint)VehicleHash.Camper)
                return false;

            if (l_Vehicle.SyncExtension.Locked)
            {
                Chats.sendProgressBar(iPlayer, 30000);
                
                foreach(DbPlayer insidePlayer in l_Vehicle.Visitors.ToList())
                {
                    if (insidePlayer == null || !insidePlayer.IsValid() || insidePlayer.Dimension[0] == 0 || insidePlayer.DimensionType[0] != DimensionType.Camper) continue;
                    insidePlayer.SendNewNotification($"Irgendetwas rappelt an der Tür...");
                }

                iPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "missheistdockssetup1ig_3@talk", "oh_hey_vin_dockworker");
                iPlayer.Player.TriggerEvent("freezePlayer", true);
                iPlayer.SetCannotInteract(true);

                await Task.Delay(30000);

                iPlayer.SetCannotInteract(false);
                iPlayer.Player.TriggerEvent("freezePlayer", false);

                l_Vehicle.SyncExtension.SetLocked(false);

                iPlayer.SendNewNotification("Fahrzeug aufgebrochen!", notificationType:PlayerNotification.NotificationType.SUCCESS);
                NAPI.Player.StopPlayerAnimation(iPlayer.Player);
                return true;
            }
            else
            {
                iPlayer.SendNewNotification( "Fahrzeug ist offen!");
                return false;
            }
        }
    }
}
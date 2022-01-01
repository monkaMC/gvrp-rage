using GTANetworkAPI;
using System;
using System.Linq;
using System.Threading.Tasks;
using GVRP.Handler;
using GVRP.Module.Chat;
using GVRP.Module.Doors;
using GVRP.Module.GTAN;
using GVRP.Module.Houses;
using GVRP.Module.Injury;
using GVRP.Module.Laboratories;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.JumpPoints;
using GVRP.Module.Players.PlayerAnimations;
using GVRP.Module.Teams;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> BatterieRam(DbPlayer iPlayer, ItemModel ItemData)
        {
            // Only Cops
            if (iPlayer.TeamId != (int)teams.TEAM_FIB && iPlayer.TeamId != (int)teams.TEAM_SWAT && iPlayer.TeamId != (int)teams.TEAM_FIB) return false;

            // Check Door
            if (iPlayer.TryData("doorId", out uint doorId))
            {
                var door = DoorModule.Instance.Get(doorId);
                if (door != null)
                {
                    if (door.AdminUnbreakable) return false;
                    if (!door.Locked)
                    {
                        iPlayer.SendNewNotification("Tuer ist bereits aufgeschlossen!", notificationType: PlayerNotification.NotificationType.SUCCESS);
                        return false;
                    }
                    if (door.LastBreak.AddMinutes(5) > DateTime.Now) return false; // Bei einem Break, kann 5 min nicht interagiert werden

                    Chats.sendProgressBar(iPlayer, 30000);

                    iPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "missheistdockssetup1ig_3@talk", "oh_hey_vin_dockworker");
                    iPlayer.Player.TriggerEvent("freezePlayer", true);
                    iPlayer.SetData("userCannotInterrupt", true);

                    await Task.Delay(30000);
                    if (iPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(iPlayer.Player) || !iPlayer.Player.Exists) return false;
                    iPlayer.ResetData("userCannotInterrupt");

                    if (iPlayer.IsCuffed || iPlayer.IsTied || iPlayer.isInjured()) return true;

                    iPlayer.Player.TriggerEvent("freezePlayer", false);

                    door.Break();

                    iPlayer.SendNewNotification("Tuer aufgebrochen!", notificationType: PlayerNotification.NotificationType.SUCCESS);
                    NAPI.Player.StopPlayerAnimation(iPlayer.Player);
                    return true;
                }
            }

            if(iPlayer.TryData("houseId", out uint houseid))
            {
                var house = HouseModule.Instance[houseid];
                if (house != null)
                {
                    if(!house.Locked)
                    {
                        iPlayer.SendNewNotification("Eingang ist bereits aufgeschlossen!", notificationType: PlayerNotification.NotificationType.SUCCESS);
                        return false;
                    }
                    if (house.LastBreak.AddMinutes(10) > DateTime.Now) return false; // Bei einem Break, kann 10 min nicht interagiert werden

                    Chats.sendProgressBar(iPlayer, 30000);
                    
                    iPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "missheistdockssetup1ig_3@talk", "oh_hey_vin_dockworker");
                    iPlayer.Player.TriggerEvent("freezePlayer", true);

                    await Task.Delay(30000);
                    if (iPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(iPlayer.Player) || !iPlayer.Player.Exists) return false;
                    iPlayer.Player.TriggerEvent("freezePlayer", false);
                    house.Locked = false;
                    house.LastBreak = DateTime.Now;

                    iPlayer.SendNewNotification("Eingang aufgebrochen!", notificationType: PlayerNotification.NotificationType.SUCCESS);
                    NAPI.Player.StopPlayerAnimation(iPlayer.Player);
                    return true;
                }
            }

            // Check Jumppoint
            if (iPlayer.TryData("jumpPointId", out int jumpPointId))
            {
                var jumpPoint = JumpPointModule.Instance.Get(jumpPointId);
                if (jumpPoint != null)
                {
                    if (jumpPoint.AdminUnbreakable) return false;
                    if (!jumpPoint.Locked)
                    {
                        iPlayer.SendNewNotification("Eingang ist bereits aufgeschlossen!", notificationType: PlayerNotification.NotificationType.SUCCESS);
                        return false;
                    }
                    if (jumpPoint.LastBreak.AddMinutes(5) > DateTime.Now) return false; // Bei einem Break, kann 5 min nicht interagiert werden

                    int time = 30000;
                    Methlaboratory methlaboratory = MethlaboratoryModule.Instance.GetLaboratoryByPosition(iPlayer.Player.Position);
                    if (methlaboratory != null)
                    {
                        time = LaboratoryModule.TimeToBreakDoor;
                        TeamModule.Instance.Get(methlaboratory.TeamId).SendNotification("Das Sicherheitssystem des Methlabors meldet einen Alarm...", time: 30000);

                    }

                    Chats.sendProgressBar(iPlayer, time);


                    iPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "missheistdockssetup1ig_3@talk", "oh_hey_vin_dockworker");
                    iPlayer.Player.TriggerEvent("freezePlayer", true);

                    await Task.Delay(time);
                    if (iPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(iPlayer.Player) || !iPlayer.Player.Exists) return false;
                    iPlayer.Player.TriggerEvent("freezePlayer", false);
                    jumpPoint.Locked = false;
                    jumpPoint.LastBreak = DateTime.Now;
                    jumpPoint.Destination.Locked = false;
                    jumpPoint.Destination.LastBreak = DateTime.Now;

                    iPlayer.SendNewNotification("Eingang aufgebrochen!", notificationType: PlayerNotification.NotificationType.SUCCESS);
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

                Random rnd = new Random();

                if(rnd.Next(0, 100) <= 10) // 10%
                {
                    foreach (DbPlayer insidePlayer in l_Vehicle.Visitors.ToList())
                    {
                        if (insidePlayer == null || !insidePlayer.IsValid() || insidePlayer.Dimension[0] == 0 || insidePlayer.DimensionType[0] != DimensionType.Camper) continue;
                        insidePlayer.SendNewNotification($"Irgendetwas rappelt an der Tür...");
                    }
                }

                iPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "missheistdockssetup1ig_3@talk", "oh_hey_vin_dockworker");
                iPlayer.Player.TriggerEvent("freezePlayer", true);

                await Task.Delay(30000);
                if (iPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(iPlayer.Player) || !iPlayer.Player.Exists) return false;
                iPlayer.Player.TriggerEvent("freezePlayer", false);

                l_Vehicle.SyncExtension.SetLocked(false);

                iPlayer.SendNewNotification("Fahrzeug aufgebrochen!", notificationType: PlayerNotification.NotificationType.SUCCESS);
                NAPI.Player.StopPlayerAnimation(iPlayer.Player);
                return true;

            }
            else
            {
                iPlayer.SendNewNotification("Fahrzeug ist offen!");
                return false;
            }
        }
    }
}
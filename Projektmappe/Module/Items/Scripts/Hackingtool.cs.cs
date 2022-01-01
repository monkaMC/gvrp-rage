using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GVRP.Handler;
using GVRP.Module.Asservatenkammer;
using GVRP.Module.Chat;
using GVRP.Module.Doors;
using GVRP.Module.GTAN;
using GVRP.Module.Houses;
using GVRP.Module.Injury;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.JumpPoints;
using GVRP.Module.Players.PlayerAnimations;
using GVRP.Module.Robbery;
using GVRP.Module.Spawners;
using GVRP.Module.Teams;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static async Task<bool> Hackingtool(DbPlayer iPlayer, ItemModel ItemData)
        {
            if (!iPlayer.CanInteract()) return false;
            // Check Door
            if (iPlayer.TryData("doorId", out uint doorId))
            {
                var door = DoorModule.Instance.Get(doorId);
                if (door != null)
                {
                    if (!door.OpenWithWelding && !door.OpenWithHacking) return false;
                    if (door.AdminUnbreakable) return false;
                    if (!door.Locked)
                    {
                        iPlayer.SendNewNotification("Tuer ist bereits aufgeschlossen!", notificationType: PlayerNotification.NotificationType.SUCCESS);
                        return false;
                    }
                    if (door.LastBreak.AddMinutes(5) > DateTime.Now) return false; // Bei einem Break, kann 5 min nicht interagiert werden

                    // Check Duty Cops
                    if(TeamModule.Instance.DutyCops < 10)
                    {
                        iPlayer.SendNewNotification("Die Sicherheitssysteme lassen das nicht zu!", notificationType: PlayerNotification.NotificationType.SUCCESS);
                        return false;
                    }

                    int time = 240000;

                    Chats.sendProgressBar(iPlayer, time);

                    TeamModule.Instance.SendChatMessageToDepartments($"Es wird gerade versucht eine Sicherheitstuer zu hacken! Gray-Cooperation Secure System - Object {door.Name}!");

                    iPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@world_human_welding@male@idle_a", "idle_a");
                    iPlayer.Player.TriggerEvent("freezePlayer", true);
                    iPlayer.SetCannotInteract(true);

                    await Task.Delay(time);

                    iPlayer.SetCannotInteract(false);

                    if (iPlayer.IsCuffed || iPlayer.IsTied || iPlayer.isInjured()) return true;

                    iPlayer.Player.TriggerEvent("freezePlayer", false);
                    door.Break();

                    iPlayer.SendNewNotification("Tuer aufgebrochen!", notificationType: PlayerNotification.NotificationType.SUCCESS);
                    NAPI.Player.StopPlayerAnimation(iPlayer.Player);
                    return true;
                }
            }

            if (iPlayer.Player.Position.DistanceTo(StaatsbankRobberyModule.HackingPoint) < 2.0f &&
                StaatsbankRobberyModule.Instance.IsActive &&
                StaatsbankRobberyModule.Instance.RobberTeam == iPlayer.Team)
            {

                Door Maindoor = DoorModule.Instance.Get((uint)StaatsbankRobberyModule.MainDoorId);
                Door Maindoor2 = DoorModule.Instance.Get(Maindoor.Pair);

                Door SideDoor = DoorModule.Instance.Get((uint)StaatsbankRobberyModule.SideDoorId);
                Door SideDoor2 = DoorModule.Instance.Get(SideDoor.Pair);

                if (Maindoor == null || SideDoor == null || Maindoor2 == null || SideDoor2 == null) return false;

                if (StaatsbankRobberyModule.Instance.DoorHacked)
                {
                    iPlayer.SendNewNotification("Die Sicherheitssysteme blockieren deinen Hackvorgang!");
                    return false;
                }

                Chats.sendProgressBar(iPlayer, 60000);

                iPlayer.SendNewNotification("Du beginnst dich in den Computer zu hacken!");

                iPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "anim@heists@prison_heistig1_P1_guard_checks_bus", "loop");
                iPlayer.Player.TriggerEvent("freezePlayer", true);
                iPlayer.SetData("userCannotInterrupt", true);

                await Task.Delay(60000);

                iPlayer.ResetData("userCannotInterrupt");
                if (iPlayer.IsCuffed || iPlayer.IsTied || iPlayer.isInjured()) return true;
                iPlayer.Player.TriggerEvent("freezePlayer", false);
                NAPI.Player.StopPlayerAnimation(iPlayer.Player);

                iPlayer.SendNewNotification("Du konntest durch den Computer die Haupttüren für 5 Minuten verschließen!");

                Maindoor.SetLocked(true);
                Maindoor.LastBreak = DateTime.Now;
                Maindoor2.LastBreak = DateTime.Now;

                SideDoor.SetLocked(true);
                SideDoor.LastBreak = DateTime.Now;
                SideDoor2.LastBreak = DateTime.Now;

                StaatsbankRobberyModule.Instance.DoorHacked = true;
                return true;
            }

            HousesVoltage housevoltage = HousesVoltageModule.Instance.GetAll().Values.Where(hv => hv.Position.DistanceTo(iPlayer.Player.Position) < 3.0f).FirstOrDefault();
            if (housevoltage != null)
            {
                Module.Menu.MenuManager.Instance.Build(GVRP.Module.Menu.PlayerMenu.HackingVoltageMenu, iPlayer).Show(iPlayer);
                return false;
            }

            if (iPlayer.Player.Position.DistanceTo(AsservatenkammerModule.AserHackPosition) < 2.0f)
            {
                
                // Nur als Gangler, nur bei mind 20 cops, nur alle 2h, nicht parallel
                if ((!Configurations.Configuration.Instance.DevMode) && (!iPlayer.IsAGangster() || TeamModule.Instance.DutyCops < 20 || AsservatenkammerModule.Instance.AserHackActive || AsservatenkammerModule.Instance.LastAserHack.AddHours(2) > DateTime.Now))
                {
                    iPlayer.SendNewNotification("Die Sicherheitssysteme blockieren deinen Hackvorgang!");
                    return false;
                }

                AsservatenkammerModule.Instance.AserHackActive = true;
                Chats.sendProgressBar(iPlayer, 240000);

                iPlayer.SendNewNotification("Du beginnst dich in den Computer zu hacken!");

                TeamModule.Instance.SendChatMessageToDepartments("Es wurde ein Sicherheitseinbruch in der LSPD Asservatenkammer gemeldet!");

                iPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "anim@heists@prison_heistig1_P1_guard_checks_bus", "loop");
                iPlayer.Player.TriggerEvent("freezePlayer", true);
                iPlayer.SetData("userCannotInterrupt", true);

                await Task.Delay(240000);

                iPlayer.ResetData("userCannotInterrupt");
                if (iPlayer.IsCuffed || iPlayer.IsTied || iPlayer.isInjured()) return true;
                iPlayer.Player.TriggerEvent("freezePlayer", false);
                NAPI.Player.StopPlayerAnimation(iPlayer.Player);

                iPlayer.SendNewNotification("Du konntest die LSPD Asservatenkammer aufschließen!");
                AsservatenkammerModule.Instance.LastAserHack = DateTime.Now;

                StaticContainer AserKammer = StaticContainerModule.Instance.Get((uint)StaticContainerTypes.ASERLSPD);

                // Half Items at rob...
                foreach(Item item in AserKammer.Container.Slots.Values)
                {
                    if(item != null && item.Id > 0)
                    {
                        if (item.Amount > 10) item.Amount = item.Amount / 2;
                    }
                }

                StaticContainerModule.Instance.Get((uint)StaticContainerTypes.ASERLSPD).Locked = false;
                return true;
            }
            return false;
        }
    }
}
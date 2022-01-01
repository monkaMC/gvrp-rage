using GTANetworkAPI;
using System;
using System.Linq;
using System.Threading.Tasks;
using GVRP.Handler;
using GVRP.Module.Chat;
using GVRP.Module.Configurations;
using GVRP.Module.Doors;
using GVRP.Module.Gangwar;
using GVRP.Module.GTAN;
using GVRP.Module.Injury;
using GVRP.Module.Laboratories;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.JumpPoints;
using GVRP.Module.Players.PlayerAnimations;
using GVRP.Module.Robbery;
using GVRP.Module.Shops;
using GVRP.Module.Teams;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static int SchweissgeraetTimeToBreakDoor = 90000;

        public static async Task<bool> Schweissgereat(DbPlayer iPlayer, ItemModel ItemData)
        {
            if (!iPlayer.CanInteract()) return false;
            // Check Door
            if (iPlayer.TryData("doorId", out uint doorId))
            {
                var door = DoorModule.Instance.Get(doorId);

                if (door != null)
                {
                    if (!door.OpenWithWelding || door.AdminUnbreakable || door.OpenWithHacking) return false;
                    if (!door.Locked)
                    {
                        iPlayer.SendNewNotification("Tuer ist bereits aufgeschlossen!", notificationType: PlayerNotification.NotificationType.SUCCESS);
                        return false;
                    }
                    if (door.LastBreak.AddMinutes(5) > DateTime.Now) return false; // Bei einem Break, kann 5 min nicht interagiert werden

                    int time = SchweissgeraetTimeToBreakDoor;

                    Chats.sendProgressBar(iPlayer, time);

                    if(!door.LessSecurity || door.LessSecurityChanged.AddMinutes(5) < DateTime.Now) TeamModule.Instance.SendChatMessageToDepartments($"Es wird gerade versucht eine Sicherheitstuer aufzubrechen! Gray-Cooperation Secure System - Object {door.Name}!");

                    iPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@world_human_welding@male@idle_a", "idle_a");
                    iPlayer.Player.TriggerEvent("freezePlayer", true);
                    iPlayer.SetCannotInteract(true);

                    await Task.Delay(time);
                    iPlayer.SetCannotInteract(false);

                    if (!iPlayer.CanInteract()) return true;

                    iPlayer.Player.TriggerEvent("freezePlayer", false);
                    door.Break();

                    if (door.LessSecurity && door.LessSecurityChanged.AddMinutes(5) > DateTime.Now) TeamModule.Instance.SendChatMessageToDepartments($"Es wird gerade versucht eine Sicherheitstuer aufzubrechen! Gray-Cooperation Secure System - Object {door.Name}!");

                    iPlayer.SendNewNotification("Tuer aufgebrochen!", notificationType:PlayerNotification.NotificationType.SUCCESS);
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
                    if (!jumpPoint.Unbreakable) return false;
                    if (jumpPoint.AdminUnbreakable) return false;

                    if (!jumpPoint.Locked)
                    {
                        iPlayer.SendNewNotification("Eingang ist bereits aufgeschlossen!", notificationType:PlayerNotification.NotificationType.SUCCESS);
                        return false;
                    }

                    if (jumpPoint.LastBreak.AddMinutes(5) > DateTime.Now) return false; // Bei einem Break, kann 5 min nicht interagiert werden

                    int time = SchweissgeraetTimeToBreakDoor;

                    Methlaboratory methlaboratory = MethlaboratoryModule.Instance.GetLaboratoryByJumppointId(jumpPointId);
                    Weaponlaboratory weaponlaboratory = WeaponlaboratoryModule.Instance.GetLaboratoryByJumppointId(jumpPointId);
                    Cannabislaboratory cannabislaboratory = CannabislaboratoryModule.Instance.GetLaboratoryByJumppointId(jumpPointId);

                    if (methlaboratory != null)
                    {
                        time = LaboratoryModule.TimeToBreakDoor;
                        if (!MethlaboratoryModule.Instance.CanMethLaboratyRaided(methlaboratory, iPlayer))
                        {
                            iPlayer.SendNewNotification("Hier scheint nichts los zu sein...");
                            return false;
                        }
                        methlaboratory.LaborMemberCheckedOnHack = true;
                        TeamModule.Instance.Get(methlaboratory.TeamId).SendNotification("Das Sicherheitssystem des Methlabors meldet einen Alarm...", time:30000);
                    }
                    else if (weaponlaboratory != null)
                    {
                        time = LaboratoryModule.TimeToBreakDoor;
                        if (!WeaponlaboratoryModule.Instance.CanWeaponLaboratyRaided(weaponlaboratory, iPlayer))
                        {
                            iPlayer.SendNewNotification("Hier scheint nichts los zu sein...");
                            return false;
                        }
                        weaponlaboratory.LaborMemberCheckedOnHack = true;
                        TeamModule.Instance.Get(weaponlaboratory.TeamId).SendNotification("Das Sicherheitssystem des Waffenlabors meldet einen Alarm...", time: 30000);
                    }
                    else if (cannabislaboratory != null)
                    {
                        time = LaboratoryModule.TimeToBreakDoor;

                        if (!CannabislaboratoryModule.Instance.CanCannabislaboratyRaided(cannabislaboratory, iPlayer))
                        {
                            iPlayer.SendNewNotification("Hier scheint nichts los zu sein...");
                            return false;
                        }

                        cannabislaboratory.LaborMemberCheckedOnHack = true;
                        TeamModule.Instance.Get(cannabislaboratory.TeamId).SendNotification("Das Sicherheitssystem des Cannabislabors meldet einen Alarm...", time: 30000);
                    }
                    else
                    {
                        TeamModule.Instance.SendChatMessageToDepartments($"Es wird gerade versucht eine Sicherheitstuer aufzubrechen! Gray-Cooperation Secure System - Object {jumpPoint.Name}!");
                    }

                    Chats.sendProgressBar(iPlayer, time);

                    iPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@world_human_welding@male@idle_a", "idle_a");
                    iPlayer.Player.TriggerEvent("freezePlayer", true);
                    iPlayer.SetCannotInteract(true);

                    await Task.Delay(time);

                    iPlayer.SetCannotInteract(false);

                    if (!iPlayer.CanInteract()) return true;

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

            // Shoprob
            var closestShop = ShopsModule.Instance.GetRobableShopAtPos(iPlayer.Player.Position, 12.0f);

            // Shop
            if (closestShop != null && closestShop.RobPosition.X != 0 && iPlayer.Player.Position.DistanceTo(closestShop.RobPosition) < 1.5)
            {
                if (iPlayer.Level < 5)
                {
                    iPlayer.SendNewNotification("Du musst mindestens Level 5 sein um einen Shop auszurauben!");
                    return true;
                }

                if (RobberyModule.Instance.IsAnyShopInRobbing())
                {
                    iPlayer.SendNewNotification("Ein Store wird bereits ausgeraubt!");
                    return true;
                }

                if (RobberyModule.Instance.Get((int)closestShop.Id) != null)
                {
                    iPlayer.SendNewNotification(
                        "Dieser Store wurde bereits ausgeraubt!");
                    return true;
                }

                if (TeamModule.Instance.DutyCops < 8 && !Configurations.Configuration.Instance.DevMode)
                {
                    iPlayer.SendNewNotification(
                        "Es muessen mindestens 8 Beamte im Dienst sein!");
                    return true;
                }


                if (iPlayer.Player.Dimension != 0)
                {
                    //DBLogging.LogAdminAction(iPlayer.Player, iPlayer.GetName(), adminLogTypes.perm, "Community-Ausschluss Shop Auto Cheat", 0, Configurations.Configuration.Instance.DevMode);
                    Players.Players.Instance.SendMessageToAuthorizedUsers("anticheat",
                        "Haus Bug Use " + iPlayer.GetName());
                    iPlayer.warns[0] = 3;
                    SocialBanHandler.Instance.AddEntry(iPlayer.Player);
                    iPlayer.Player.SendNotification("ANTI CHEAT (IM SUPPORT MELDEN) (dimension not 0)");
                    iPlayer.Player.Kick("ANTI CHEAT (IM SUPPORT MELDEN)!");
                    return true;
                }

                NAPI.Player.StopPlayerAnimation(iPlayer.Player);
                if (!iPlayer.IsCuffed && !iPlayer.IsTied && !iPlayer.isInjured())
                {
                    RobberyModule.Instance.Add((int)closestShop.Id, iPlayer, 1, copinterval: Utils.RandomNumber(2, 10), endinterval: Utils.RandomNumber(25, 35));
                }

                iPlayer.SendNewNotification("Sie beginnen nun damit den Tresor aufzuschweißen!");

                Chats.sendProgressBar(iPlayer, 60000);

                iPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@world_human_welding@male@idle_a", "idle_a");
                iPlayer.Player.TriggerEvent("freezePlayer", true);
                iPlayer.SetCannotInteract(true);

                await Task.Delay(60000);

                iPlayer.SetCannotInteract(false);
                if (iPlayer.IsCuffed || iPlayer.IsTied || iPlayer.isInjured()) return true;
                iPlayer.Player.TriggerEvent("freezePlayer", false);
                NAPI.Player.StopPlayerAnimation(iPlayer.Player);

                iPlayer.SendNewNotification("Tresor aufgebrochen! Du wirst regelmäßig Geld erhalten.", notificationType: PlayerNotification.NotificationType.SUCCESS);
                
                return true;
            }


            StaatsbankTunnel tunnel = StaatsbankRobberyModule.Instance.StaatsbankTunnels.Where(t => t.IsActiveForTeam == iPlayer.Team).FirstOrDefault();

            // Staatsbankrob
            if (tunnel != null && StaatsbankRobberyModule.Instance.IsActive && StaatsbankRobberyModule.Instance.RobberTeam == iPlayer.Team)
            {
                if(!tunnel.IsOutsideOpen && iPlayer.Player.Position.DistanceTo(tunnel.Position) < 3.0f)
                {
                    iPlayer.SendNewNotification("Sie beginnen nun damit die Gitterstäbe aufzuschweißen!");

                    Chats.sendProgressBar(iPlayer, 60000);

                    iPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@world_human_welding@male@idle_a", "idle_a");
                    iPlayer.Player.TriggerEvent("freezePlayer", true);
                    iPlayer.SetCannotInteract(true);

                    await Task.Delay(60000);

                    iPlayer.SetCannotInteract(false);
                    if (iPlayer.IsCuffed || iPlayer.IsTied || iPlayer.isInjured()) return true;
                    iPlayer.Player.TriggerEvent("freezePlayer", false);
                    NAPI.Player.StopPlayerAnimation(iPlayer.Player);

                    iPlayer.SendNewNotification("Gitterstäbe aufgeschweißt!", notificationType: PlayerNotification.NotificationType.SUCCESS);

                    iPlayer.Team.SendNotification("Die Gitterstäbe wurden aufgeschweißt, es kann nun ein Tunnel gegraben werden!");

                    tunnel.IsOutsideOpen = true;
                    return true;
                }
            }

            if (StaatsbankRobberyModule.Instance.IsActive && StaatsbankRobberyModule.Instance.RobberTeam != null && iPlayer.Team == StaatsbankRobberyModule.Instance.RobberTeam)
            {
                foreach (StaticContainer staticContainer in StaticContainerModule.Instance.GetAll().Values.ToList())
                {
                    if (staticContainer.Locked && staticContainer.Position.DistanceTo(iPlayer.Player.Position) <= staticContainer.Range)
                    {
                        if (staticContainer.Id == (uint)StaticContainerTypes.STAATSBANK1 || staticContainer.Id == (uint)StaticContainerTypes.STAATSBANK2 || staticContainer.Id == (uint)StaticContainerTypes.STAATSBANK3
                            || staticContainer.Id == (uint)StaticContainerTypes.STAATSBANK4 || staticContainer.Id == (uint)StaticContainerTypes.STAATSBANK5 || staticContainer.Id == (uint)StaticContainerTypes.STAATSBANK6
                            || staticContainer.Id == (uint)StaticContainerTypes.STAATSBANK7 || staticContainer.Id == (uint)StaticContainerTypes.STAATSBANK8)
                        {
                            if(StaatsbankRobberyModule.Instance.CountInBreakTresor >= 2)
                            {
                                iPlayer.SendNewNotification("Es können nur maximal 2 Schließfächer zur selben Zeit geöffnet werden!");
                                return false;
                            }

                            int time = 480000;
                            if (Configuration.Instance.DevMode) time = 60000;
                            // Aufschließen lul
                            iPlayer.SendNewNotification("Sie beginnen nun damit das Schließfach auszuschweißen!");
                            StaatsbankRobberyModule.Instance.CountInBreakTresor++;

                            Chats.sendProgressBar(iPlayer, time);

                            iPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@world_human_welding@male@idle_a", "idle_a");
                            iPlayer.Player.TriggerEvent("freezePlayer", true);
                            iPlayer.SetCannotInteract(true);

                            await Task.Delay(time);

                            iPlayer.SetCannotInteract(false);
                            if (iPlayer.IsCuffed || iPlayer.IsTied || iPlayer.isInjured()) return true;
                            iPlayer.Player.TriggerEvent("freezePlayer", false);
                            NAPI.Player.StopPlayerAnimation(iPlayer.Player);

                            iPlayer.SendNewNotification("Schließfach aufgeschweißt!", notificationType: PlayerNotification.NotificationType.SUCCESS);

                            StaatsbankRobberyModule.Instance.LoadContainerBankInv(staticContainer.Container);
                            staticContainer.Locked = false;
                            StaatsbankRobberyModule.Instance.CountInBreakTresor--;
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
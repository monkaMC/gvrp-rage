using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GVRP.Handler;
using GVRP.Module.Chat;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Customization;
using GVRP.Module.Items;
using GVRP.Module.NSA.Observation;
using GVRP.Module.PlayerName;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Windows;
using GVRP.Module.Teams;
using GVRP.Module.Telefon.App;
using GVRP.Module.Vehicles;

namespace GVRP.Module.NSA
{
    public class NSAFunctions : Script
    {
        [RemoteEvent]
        public void NSACheckNumber(Client player, string returnstring)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
           
            if (dbPlayer.TeamRank < 5)
            {
                Discord.SendMessage(dbPlayer.GetName() + " hat einen Nice Try versucht NSAClonePlayer @here");
                player.Ban("Bye du HS!");
                return;
            }

            if (Int32.TryParse(returnstring, out int number))
            {
                if (number == 0) return;
                DbPlayer targetPlayer = TelefonInputApp.GetPlayerByPhoneNumber(number);
                if (targetPlayer == null || !targetPlayer.IsValid()) return;

                dbPlayer.SendNewNotification($"Rufnummer {number} ist {targetPlayer.GetName()} zugewiesen!");
                return;
            }
            dbPlayer.SendNewNotification("Ungültige Nummer!");
            return;
        }

        [RemoteEvent]
        public void NSASuspendate(Client player, string returnstring)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if ((dbPlayer.TeamId != (int)teams.TEAM_FIB && dbPlayer.TeamId != (int)teams.TEAM_GOV) && dbPlayer.TeamRank < 11) return;

            if (dbPlayer.TeamRank < 5)
            {
                Discord.SendMessage(dbPlayer.GetName() + " hat einen Nice Try versucht NSAClonePlayer @here");
                player.Ban("Bye du HS!");
                return;
            }

            DbPlayer target = Players.Players.Instance.FindPlayer(returnstring);
            if (target != null && target.IsValid())
            {
                if(target.Suspension)
                {
                    target.Suspension = false;

                    dbPlayer.SendNewNotification($"Sie haben {target.GetName()} Suspendierung aufgehoben!");
                    TeamModule.Instance.SendChatMessageToDepartments($"{dbPlayer.GetName()} hat die Suspendierung von {target.GetName()} aufgehoben!");

                    target.Save();
                    return;
                }

                if(target.IsACop() || target.TeamId == (int)teams.TEAM_MEDIC || target.TeamId == (int)teams.TEAM_DPOS || target.TeamId == (int)teams.TEAM_DRIVINGSCHOOL)
                {
                    target.RemoveWeapons();
                    target.SetDuty(false);

                    target.Suspension = true;

                    TeamModule.Instance.SendChatMessageToDepartments($"{dbPlayer.GetName()} hat {target.GetName()} des Dienstes suspendiert!");

                    dbPlayer.SendNewNotification($"Sie haben {target.GetName()} des Dienstes suspendiert!");

                    target.Save();
                    return;
                }
            }

            return;
        }

        [RemoteEvent]
        public void NSASetPeilsender(Client player, string returnstring)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
           
            if (dbPlayer.TeamRank < 5)
            {
                Discord.SendMessage(dbPlayer.GetName() + " hat einen Nice Try versucht NSATrigger @here");
                player.Ban("Bye du HS!");
                return;
            }
            // Prüfe Fahrzeug in der Nähe
            SxVehicle sxVeh = VehicleHandler.Instance.GetClosestVehicle(dbPlayer.Player.Position);
            if(sxVeh != null && sxVeh.IsValid())
            {
                Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                {
                    Chats.sendProgressBar(dbPlayer, (20 * 1000));
                    await Task.Delay(20 * 1000);
                    
                    NSAObservationModule.NSAPeilsenders.Add(new NSAPeilsender() { CreatorId = dbPlayer.Id, Name = returnstring, VehicleId = sxVeh.databaseId });
                    dbPlayer.SendNewNotification($"Peilsender ({returnstring}) wurde an Fahrzeug {sxVeh.databaseId} angebracht!");

                    dbPlayer.Container.RemoveItem(696);
                }));
                return;
            }
            
            return;
        }
        public static DiscordHandler Discord = new DiscordHandler();

        [RemoteEvent]
        public void NSAClonePlayer(Client player, string returnstring)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if(dbPlayer.TeamRank < 5)                       
            {
                Discord.SendMessage(dbPlayer.GetName() + " hat einen Nice Try versucht NSAClonePlayer @here");
                player.Ban("Bye du HS!");
                return;
            }
            
            DbPlayer target = Players.Players.Instance.FindPlayer(returnstring);
            if (target != null && target.IsValid())
            {
                dbPlayer.SendNewNotification("Person Identifying System started...");
                dbPlayer.SetData("clonePerson", target.Id);

                Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                {
                    Chats.sendProgressBar(dbPlayer, (3 * 1000));
                    await Task.Delay(3 * 1000);
                    
                    dbPlayer.ApplyCharacter();
                    
                }));
            }

            return;
        }

        [RemoteEvent]
        public void BankHistoryCallback(Client p_Player, string p_Name)
        {
            var l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.IsValid())
                return;

            var l_TargetPlayer = Players.Players.Instance.FindPlayer(p_Name);
            if (l_TargetPlayer == null || !l_TargetPlayer.IsValid())
                return;

            l_DbPlayer.SetData("nsa_bank_check", l_TargetPlayer.Id);
        }

        [RemoteEvent]
        public void NSAAddPhoneHearing(Client player, string returnstring)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (Int32.TryParse(returnstring, out int number))
            {
                DbPlayer targetPlayer = TelefonInputApp.GetPlayerByPhoneNumber(number);
                if (targetPlayer == null || !targetPlayer.IsValid()) return;

                if (dbPlayer.TeamRank < 5)
                {
                    Discord.SendMessage(dbPlayer.GetName() + " hat einen Nice Try versucht NSAClonePlayer @here");
                    player.Ban("Bye du HS!");
                    return;
                }

                // Enable this if list with obersvations is active
                if (!targetPlayer.HasData("current_caller")) return;
                if (targetPlayer.IsInAdminDuty()) return;
                
                DbPlayer ConPlayer = TelefonInputApp.GetPlayerByPhoneNumber(targetPlayer.GetData("current_caller"));
                if (ConPlayer == null || !ConPlayer.IsValid()) return;
                if (ConPlayer.IsInAdminDuty()) return;

                if(NSAObservationModule.ObservationList.Where(o => o.Value.PlayerId == ConPlayer.Id || o.Value.PlayerId == targetPlayer.Id).Count() == 0 && !targetPlayer.IsACop() && !ConPlayer.IsACop()
                    && !targetPlayer.IsAMedic() && !ConPlayer.IsAMedic())
                {
                    dbPlayer.SendNewNotification("Spieler ist nicht fuer eine Observation freigegeben!");
                    return;
                }
                
                string voiceHashPush = targetPlayer.VoiceHash + "~3~0~0~2;" + ConPlayer.VoiceHash;
                dbPlayer.Player.TriggerEvent("setCallingPlayer", voiceHashPush);
                
                dbPlayer.SetData("nsa_activePhone", number);
                
                dbPlayer.SendNewNotification("Mithören gestartet " + targetPlayer.handy[0]);
                return;
            }
            dbPlayer.SendNewNotification("Ungültige Nummer!");
            return;
        }

        [RemoteEvent]
        public void NSAChangePhoneNumber(Client player, string returnstring)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (dbPlayer.TeamRank < 5)
            {
                Discord.SendMessage(dbPlayer.GetName() + " hat einen Nice Try versucht NSAClonePlayer @here");
                player.Ban("Bye du HS!");
                return;
            }

            if (Int32.TryParse(returnstring, out int number))
            {
                if (PlayerNameModule.Instance.GetAll().ToList().Where(pn => pn.Value.HandyNr == number && pn.Value.Id != dbPlayer.Id).Count() > 0)
                {
                    dbPlayer.SendNewNotification($"Rufnummer bereits in Benutzung!");
                    return;
                }

                dbPlayer.handy[0] = (uint)number;
                dbPlayer.SendNewNotification($"Rufnummer auf {number} geändert!");
                return;
            }
            dbPlayer.SendNewNotification("Ungültige Nummer!");
            return;
        }

        [RemoteEvent]
        public void AddObservationPlayer(Client player, string returnstring)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (dbPlayer.TeamRank < 5)
            {
                Discord.SendMessage(dbPlayer.GetName() + " hat einen Nice Try versucht NSAClonePlayer @here");
                player.Ban("Bye du HS!");
                return;
            }

            DbPlayer l_FindPlayer = Players.Players.Instance.FindPlayer(returnstring);
            if (l_FindPlayer == null || !l_FindPlayer.IsValid())
                return;

            if(NSAObservationModule.ObservationList.ContainsKey(l_FindPlayer.Id))
            {
                dbPlayer.SendNewNotification("Spieler ist bereits in Observation!");
                return;
            }
            else
            {
                dbPlayer.SetData("nsaAddPlayer", l_FindPlayer.Id);
                ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Obersvation", Callback = "AddObservationPlayerReason", Message = "Geben Sie einen Grund für die Observation an:" });
            }
        }


        [RemoteEvent]
        public void AddObservationPlayerReason(Client player, string returnstring)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (!dbPlayer.HasData("nsaAddPlayer")) return;
            DbPlayer l_FindPlayer = Players.Players.Instance.GetByDbId(dbPlayer.GetData("nsaAddPlayer"));
            if (l_FindPlayer == null || !l_FindPlayer.IsValid())
                return;

            if (dbPlayer.TeamRank < 5)
            {
                Discord.SendMessage(dbPlayer.GetName() + " hat einen Nice Try versucht NSAClonePlayer @here");
                player.Ban("Bye du HS!");
                return;
            }

            if (Regex.IsMatch(returnstring, @"^[a-zA-Z ._]+$"))
            {

                if (NSAObservationModule.ObservationList.ContainsKey(l_FindPlayer.Id))
                {
                    dbPlayer.SendNewNotification("Spieler ist bereits in Observation!");
                    return;
                }
                else
                {
                    NSAObservationModule.Instance.AddObservation(dbPlayer, l_FindPlayer, returnstring);
                    dbPlayer.SendNewNotification($"{l_FindPlayer.GetName()} zur Observation hinzugefügt! (Grund: {returnstring})");
                    dbPlayer.ResetData("nsaAddPlayer");
                }
            }
            else
            {
                dbPlayer.SendNewNotification("Grund enthält ungültige Zeichen!");
                return;
            }
        }

        [RemoteEvent]
        public void SetCarColorNSA(Client player, string returnstring)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (dbPlayer.TeamRank < 5)
            {
                Discord.SendMessage(dbPlayer.GetName() + " hat einen Nice Try versucht NSAClonePlayer @here");
                player.Ban("Bye du HS!");
                return;
            }

            if (!dbPlayer.HasData("nsa_work_vehicle")) return;

            if (returnstring.Length < 2 || !returnstring.Contains(" ")) return;

            string[] splittedReturn = returnstring.Split(" ");
            if (splittedReturn.Length != 2) return;

            if (!Int32.TryParse(splittedReturn[0], out int color1)) return;
            if (!Int32.TryParse(splittedReturn[1], out int color2)) return;

            SxVehicle sxVehicle = VehicleHandler.Instance.FindTeamVehicle(dbPlayer.TeamId, (uint)dbPlayer.GetData("nsa_work_vehicle"));
            if (sxVehicle == null || !sxVehicle.IsValid()) return;

            sxVehicle.color1 = color1;
            sxVehicle.color2 = color2;

            sxVehicle.entity.PrimaryColor = color1;
            sxVehicle.entity.SecondaryColor = color2;

            dbPlayer.SendNewNotification($"Fahrzeugfarbe auf {color1} {color2} geändert!");
            return;
        }

        [RemoteEvent]
        public void SetCarPlateNSA(Client player, string returnstring)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (dbPlayer.TeamRank < 5)
            {
                Discord.SendMessage(dbPlayer.GetName() + " hat einen Nice Try versucht NSAClonePlayer @here");
                player.Ban("Bye du HS!");
                return;
            }

            if (!dbPlayer.HasData("nsa_work_vehicle")) return;

            if (returnstring.Length < 2) return;
            
            SxVehicle sxVehicle = VehicleHandler.Instance.FindTeamVehicle(dbPlayer.TeamId, (uint)dbPlayer.GetData("nsa_work_vehicle"));
            if (sxVehicle == null || !sxVehicle.IsValid()) return;
            
            sxVehicle.plate = returnstring;

            sxVehicle.entity.NumberPlate = returnstring;

            dbPlayer.SendNewNotification($"Kennzeichen auf {returnstring} geändert!");
            return;
        }
    }
}

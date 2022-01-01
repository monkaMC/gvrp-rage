using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.Chat;
using GVRP.Module.Items;
using GVRP.Module.Logging;
using GVRP.Module.NSA;
using GVRP.Module.NSA.Observation;
using GVRP.Module.Players.Db;
using GVRP.Module.ReversePhone;
using GVRP.Module.Telefon.App;
using GVRP.Module.Voice;

namespace GVRP.Module.Players.Phone
{
    public static class PhoneCall
    {
        public static string PHONECALL_TYPE = "phone_calling";
        public static string PHONENUMBER = "phone_number";

        public static bool IsPlayerInCall(Client player)
        {
            DbPlayer iPlayer = player.GetPlayer();
            if (iPlayer == null || !iPlayer.IsValid()) return false;
            // is the requested player in phone a call
            if (!iPlayer.HasData(PHONECALL_TYPE)) return false;
            return iPlayer.GetData(PHONECALL_TYPE) == "waiting" ||
                   iPlayer.GetData(PHONECALL_TYPE) == "incoming" ||
                   iPlayer.GetData(PHONECALL_TYPE) == "active";
        }
        
        public static async void CancelPhoneCall(this DbPlayer dbPlayer)
        {
            Client player = dbPlayer.Player;
            if (dbPlayer == null) return;

            if (dbPlayer.HasData("current_caller"))
            {
                int callNumber = dbPlayer.GetData("current_caller");

                NSAModule.Instance.StopNASCAll((int)dbPlayer.handy[0], callNumber);

                DbPlayer dbCalledPlayer = await CallManageApp.GetPlayerByPhoneNumber(callNumber);
                if (dbCalledPlayer != null && dbCalledPlayer.HasData("current_caller"))
                {
                    if (dbCalledPlayer.GetData("current_caller") == dbPlayer.handy[0])
                    {
                        dbCalledPlayer.ResetData("current_caller");
                        NSAObservationModule.CancelPhoneHearing((int)dbCalledPlayer.handy[0]);
                        dbCalledPlayer.Player.TriggerEvent("cancelPhoneCall", "");
                        dbCalledPlayer.Player.TriggerEvent("setCallingPlayer", "");
                        if (!NAPI.Player.IsPlayerInAnyVehicle(dbCalledPlayer.Player))
                            NAPI.Player.StopPlayerAnimation(dbCalledPlayer.Player);
                    }
                }

                if (!NAPI.Player.IsPlayerInAnyVehicle(dbPlayer.Player))
                    NAPI.Player.StopPlayerAnimation(dbPlayer.Player);
            }

            NSAObservationModule.CancelPhoneHearing((int)dbPlayer.handy[0]);
            dbPlayer.ResetData("current_caller");
            dbPlayer.Player.TriggerEvent("cancelPhoneCall", "");
            dbPlayer.Player.TriggerEvent("setCallingPlayer", "");
        }

        public static bool CanUserstartCall(DbPlayer dbPlayer)
        {
            // can player have a call
            if (!CanPlayerHaveCall(dbPlayer))
            {
                dbPlayer.SendNewNotification(
                    
                    "Fuer diese Aktion benötigst du ein verfuegbares " +
                    ItemModelModule.Instance.Get(174).Name);
                return false;
            }

            // is player already in call
            if (IsPlayerInCall(dbPlayer.Player))
            {
                dbPlayer.SendNewNotification(
                     "Du befindest dich bereits in einem Gespraech.");
                return false;
            }

            // does player have enough money for code
            if (dbPlayer.guthaben[0] < 10)
            {
                dbPlayer.SendNewNotification(
                    Chats.MsgHandy +
                    "Dein Guthaben reicht nicht aus. Ein Anruf kostet $10.");
                return false;
            }

            // player can have a phone call
            return true;
        }

        public static bool CanPlayerHaveCall(DbPlayer dbPlayer)
        {
            // verify is not cuffed or tied
            if (dbPlayer.IsCuffed || dbPlayer.IsTied) return false;

            // verify has item smartphone
            return dbPlayer.Container.GetItemAmount(174) >= 1;
        }
        
        public static void SetPlayerCallStatus(Client player, string state = "waiting",
            uint phoneNumber = 0)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            // set player in phone a call
            dbPlayer.SetData(PHONECALL_TYPE, state);

            // reset connected number on 0
            if (phoneNumber == 0)
            {
                dbPlayer.ResetData(PHONENUMBER);
                return;
            }

            // set the connected phone number
            dbPlayer.SetData(PHONENUMBER, phoneNumber);
        }
        
        public static void StartPhoneCall(DbPlayer dbPlayer, uint number)
        {
            if (dbPlayer.Container.GetItemAmount(
                    174) < 1)
            {
                dbPlayer.SendNewNotification(
                    
                    "Sie besitzen kein Telefon.");
                return;
            }

            // is not own phone number
            if (number == dbPlayer.handy[0])
            {
                dbPlayer.SendNewNotification(
                     "Sie können sich nicht selber anrufen.");
                return;
            }

            // player can start a phone call
            if (!CanUserstartCall(dbPlayer)) return;

            try
            {
                // find number of reqested player in the Users lists
                foreach (var user in Players.Instance.GetValidPlayers())
                {
                    // player object not valid
                    if (!user.IsValid()) continue;

                    // player found by phone number
                    if (user.handy[0] == number &&
                        user.Container.GetItemAmount(174) < 1)
                    {
                        var requestorNumber = dbPlayer.handy[0];
                        var requestedNumber = user.handy[0];
                        
                        // is requested player already in phone call
                        if (IsPlayerInCall(user.Player))
                        {
                            dbPlayer.SendNewNotification(
                                
                                "Der Anschluss ist zurzeit besetzt!");
                            return;
                        }


                        // set requested player in state incoming
                        SetPlayerCallStatus(user.Player, "incoming", requestorNumber);
           

                        // set requestor player in state waiting
                        dbPlayer.guthaben[0] = dbPlayer.guthaben[0] - 10;
                        SetPlayerCallStatus(dbPlayer.Player, "waiting", requestedNumber);

                        // Set Funk to push-to-talk if active (dauersenden)
                        if(dbPlayer.funkStatus == FunkStatus.Active)
                        {
                            dbPlayer.funkStatus = FunkStatus.Deactive;
                            VoiceModule.Instance.refreshFQVoiceForPlayerFrequenz(dbPlayer);
                        }
                        return;
                    }
                }

                // not a valid number
                dbPlayer.SendNewNotification(
                    Chats.MsgHandy +
                    "Die von Ihnen gewaehlte Nummer ist derzeit nicht verfuegbar!");
            }
            catch (Exception e)
            {
                Logger.Print(e.ToString());
            }
        }
    }
}

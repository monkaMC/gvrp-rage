using GTANetworkAPI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.Injury;
using GVRP.Module.Logging;
using GVRP.Module.NSA.Observation;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Phone;
using GVRP.Module.ReversePhone;
using GVRP.Module.Teams;

namespace GVRP.Module.Telefon.App
{
    public class CallManageApp : SimpleApp
    {
        public CallManageApp() : base("CallManageApp")
        {
        }

        public static async Task<DbPlayer> GetPlayerByPhoneNumber(int phoneNumer)
        {
            foreach (var player in Players.Players.Instance.GetValidPlayers())
            {
                if (player == null || !player.IsValid() || (int)player.handy[0] != phoneNumer) continue;
                return player;
            }

            return null;
        }

        [RemoteEvent]
        public void requestCall(Client player, int number)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || dbPlayer.isInjured()) return;
            PhoneCall.StartPhoneCall(dbPlayer, (uint)number);
        }

        [RemoteEvent]
        public async void resetPhoneData (Client player)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;
                await callEnded(player, 0);
        }

        [RemoteEvent]
        public async void callDeclined (Client player)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.CanInteract()) return;

            if (dbPlayer.HasData("current_caller"))
            {
                int callNumber = dbPlayer.GetData("current_caller");
                if (callNumber == 0) return;

                DbPlayer dbCalledPlayer = await GetPlayerByPhoneNumber(callNumber);
                if (dbCalledPlayer != null && dbCalledPlayer.IsValid() && dbCalledPlayer.HasData("current_caller"))
                {
                    if (dbCalledPlayer.GetData("current_caller") == dbPlayer.handy[0])
                    {
                        dbCalledPlayer.SetData("current_caller", 0);
                        dbCalledPlayer.ResetData("current_caller");
                        dbCalledPlayer.Player.TriggerEvent("cancelCall", 0);
                        dbCalledPlayer.Player.TriggerEvent("setCallingPlayer", "");

                        ReversePhoneModule.Instance.AddPhoneHistory(dbPlayer, (int)dbCalledPlayer.handy[0], 0);
                        ReversePhoneModule.Instance.AddPhoneHistory(dbCalledPlayer, (int)dbPlayer.handy[0], 0);

                        if (!NAPI.Player.IsPlayerInAnyVehicle(dbCalledPlayer.Player) && dbCalledPlayer.CanInteract())
                            NAPI.Player.StopPlayerAnimation(dbCalledPlayer.Player);
                    }

                    foreach (var item in dbCalledPlayer.playerWhoHearRingtone)
                    {
                        TriggerEvent(item.Player, "responseRingtoneSound", 0, false);
                    }
                    dbCalledPlayer.playerWhoHearRingtone = new List<DbPlayer>();

                }
            }

            dbPlayer.ResetData("current_caller");
            dbPlayer.Player.TriggerEvent("cancelCall", 0);
            dbPlayer.Player.TriggerEvent("setCallingPlayer", "");

            if (!NAPI.Player.IsPlayerInAnyVehicle(dbPlayer.Player))
                NAPI.Player.StopPlayerAnimation(dbPlayer.Player);
        }

        [RemoteEvent]
        public async Task callEnded (Client player, int p_Ti)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.CanInteract()) return;

            if (dbPlayer.HasData("current_caller") == false) return;


            int callNumber = dbPlayer.GetData("current_caller");
            if (callNumber == 0) return;

            NSAObservationModule.CancelPhoneHearing((int)dbPlayer.handy[0]);
            DbPlayer dbCalledPlayer = await GetPlayerByPhoneNumber(callNumber);
            if (dbCalledPlayer == null) return;
            if (dbCalledPlayer.HasData("current_caller") == false) return;

            NSAObservationModule.CancelPhoneHearing((int)dbCalledPlayer.handy[0]);

            dbCalledPlayer.SetData("current_caller", 0); ///< Hackfix
            dbCalledPlayer.ResetData("current_caller");
            dbPlayer.SetData("current_caller", 0); ///< Hackfix
            dbPlayer.ResetData("current_caller");
            TriggerEvent(dbPlayer.Player, "cancelPhoneCall", "");
            TriggerEvent(dbCalledPlayer.Player, "cancelPhoneCall", "");
            dbCalledPlayer.Player.TriggerEvent("setCallingPlayer", "");
            dbPlayer.Player.TriggerEvent("setCallingPlayer", "");

            ReversePhoneModule.Instance.AddPhoneHistory(dbPlayer, (int)dbCalledPlayer.handy[0], p_Ti);
            ReversePhoneModule.Instance.AddPhoneHistory(dbCalledPlayer, (int)dbPlayer.handy[0], p_Ti);

            if (!NAPI.Player.IsPlayerInAnyVehicle(dbCalledPlayer.Player) && dbPlayer.CanInteract())
                NAPI.Player.StopPlayerAnimation(dbCalledPlayer.Player);
        }

        [RemoteEvent]
        public async void muteCall(Client p_Player, bool p_Muted)
        {
            DbPlayer l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.IsValid() || !l_DbPlayer.IsValid())
                return;

            if (!l_DbPlayer.HasData("current_caller"))
                return;

            DbPlayer l_CurrentCaller = await GetPlayerByPhoneNumber(l_DbPlayer.GetData("current_caller"));
            if (l_CurrentCaller == null || !l_CurrentCaller.IsValid())
                return;

            if (p_Muted)
                l_CurrentCaller.Player.TriggerEvent("setCallingPlayer", "");
            else
                l_CurrentCaller.Player.TriggerEvent("setCallingPlayer", l_DbPlayer.VoiceHash);
        }

        [RemoteEvent]
        public async void callAccepted(Client player)
        {
            DbPlayer dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.CanInteract()) return;

            if (player != null && dbPlayer.HasData("current_caller"))
            {
                int callNumber = dbPlayer.GetData("current_caller");
                DbPlayer dbCalledPlayer = await GetPlayerByPhoneNumber(callNumber);
                if (dbCalledPlayer != null && dbCalledPlayer.IsValid() && dbCalledPlayer.IsValid())
                {
                    if (dbCalledPlayer.HasData("current_caller") && dbCalledPlayer.GetData("current_caller") == dbPlayer.handy[0])
                    {
                        TriggerEvent(player, "acceptCall");
                        TriggerEvent(dbCalledPlayer.Player, "acceptCall");
                        dbPlayer.Player.TriggerEvent("setCallingPlayer", dbCalledPlayer.VoiceHash);
                        dbCalledPlayer.Player.TriggerEvent("setCallingPlayer", dbPlayer.VoiceHash);

                        if((NSAObservationModule.ObservationList.ContainsKey(dbPlayer.Id) && NSAObservationModule.ObservationList[dbPlayer.Id].Agreed)
                            || (NSAObservationModule.ObservationList.ContainsKey(dbCalledPlayer.Id) && NSAObservationModule.ObservationList[dbCalledPlayer.Id].Agreed))
                        {
                            TeamModule.Instance.SendMessageToNSA($"[OBSERVATION] {dbPlayer.GetName()} hat einen Anruf mit {dbCalledPlayer.GetName()} gestartet!");
                        }
                    }
                    else
                    {
                        dbPlayer.SendNewNotification("Die Person hat keinen Anruf mehr mit dir offen.");
                        dbPlayer.ResetData("current_caller");
                    }
                }
            }
        }
    }
}
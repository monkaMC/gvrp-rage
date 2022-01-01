using System.Collections.Generic;
using GTANetworkAPI;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.Players;
using Newtonsoft.Json;
using System;
using GVRP.Module.Injury;
using GVRP.Module.Players.Phone;
using GVRP.Module.Teams;
using GVRP.Module.Players.Db;
using System.Linq;
using GVRP.Module.Items;
using GVRP.Module.LeitstellenPhone;

namespace GVRP.Module.Telefon.App
{
    public class TelefonInputApp : SimpleApp
    {
        public TelefonInputApp() : base("TelefonInput") { }

        public static DbPlayer GetPlayerByPhoneNumber(int p_PhoneNumber)
        {
            try
            {
                foreach (var l_Player in Players.Players.Instance.GetValidPlayers())
                {
                    if ((int)l_Player.handy[0] != p_PhoneNumber)
                        continue;

                    return l_Player;
                }
                return null;
            }
            catch (Exception e)
            {
                Logging.Logger.Crash(e);
                return null;
            } 
        }

        [RemoteEvent]
        public void callUserPhone(Client p_Player, int p_CallingNumber)
        {
            DbPlayer l_Caller = p_Player.GetPlayer();
            if (l_Caller == null || !l_Caller.IsValid())
                return;

            int selfnumber = (int)l_Caller.handy[0];

            // Wenn Player Leitstellentelefon hat
            TeamLeitstellenObject teamLeitstellenObject = LeitstellenPhoneModule.Instance.GetByAcceptor(l_Caller);
            if (teamLeitstellenObject != null)
            {
                selfnumber = teamLeitstellenObject.Number;
            }

           

            if (selfnumber == p_CallingNumber)
            {
                l_Caller.SendNewNotification("Du kannst dich nicht selber anrufen.", notificationType:PlayerNotification.NotificationType.ERROR);
                return;
            }

            if (l_Caller.phoneSetting.flugmodus)
            {
                //Flugmodus aktiviert... kein Anruf möglich
                l_Caller.SendNewNotification("Der Flugmodus ist aktiviert... Kein Empfang", title: "NO SIGNAL", notificationType:PlayerNotification.NotificationType.ERROR);
                return;
            }

            DbPlayer l_CalledPlayer = null;
            int l_CalledNumber = 0;



            if (LeitstellenPhoneModule.TeamNumberPhones.ContainsKey(p_CallingNumber))
            {
                teamLeitstellenObject = LeitstellenPhoneModule.Instance.GetLeitstelleByNumber(p_CallingNumber);

                if (teamLeitstellenObject == null || teamLeitstellenObject.Acceptor == null || !teamLeitstellenObject.Acceptor.IsValid() || teamLeitstellenObject.Acceptor.TeamId != teamLeitstellenObject.TeamId) {
                    l_Caller.SendNewNotification("Die angegebene Rufnummer ist derzeit nicht verfuegbar.", notificationType: PlayerNotification.NotificationType.ERROR);
                    return;
                }
                if(teamLeitstellenObject.StaatsFrakOnly && !l_Caller.IsACop() && l_Caller.TeamId != (int)teams.TEAM_NEWS &&
                    l_Caller.TeamId != (int)teams.TEAM_FIB && l_Caller.TeamId != (int)teams.TEAM_DPOS && l_Caller.TeamId != (int)teams.TEAM_MEDIC &&
                    l_Caller.TeamId != (int) teams.TEAM_DRIVINGSCHOOL)
                {
                    l_Caller.SendNewNotification("Diese Nummer ist für Sie nicht verfügbar!.", notificationType: PlayerNotification.NotificationType.ERROR);
                    return;
                }

                l_CalledPlayer = teamLeitstellenObject.Acceptor;
                l_CalledNumber = teamLeitstellenObject.Number;
            }
            else
            {
                l_CalledPlayer = GetPlayerByPhoneNumber(p_CallingNumber);
            }


            if (l_CalledPlayer == null || l_CalledPlayer.Container.GetItemAmount(174) == 0 || l_CalledPlayer.phoneSetting.flugmodus || l_CalledPlayer.IsInAdminDuty())
            {
                l_Caller.SendNewNotification("Die angegebene Rufnummer ist derzeit nicht verfuegbar.", notificationType:PlayerNotification.NotificationType.ERROR);
                return;
            }
            if (l_CalledPlayer.phoneSetting.blockCalls)
            {
                l_Caller.SendNewNotification("Die angegebene Rufnummer hat eingehende Anrufe blockiert.", notificationType: PlayerNotification.NotificationType.ERROR);
                return;
            }

            CallUser l_CallerData = new CallUser()
            {
                method = "outgoing",
                caller = l_CalledNumber == 0 ? (int)l_CalledPlayer.handy[0] : l_CalledNumber,
                name = l_Caller.PhoneContacts.TryGetPhoneContactNameByNumber((uint)l_CalledNumber)
            };

            CallUser l_CallingData = new CallUser()
            {
                method = "incoming",
                caller = (int)selfnumber,
                name = l_CalledPlayer.PhoneContacts.TryGetPhoneContactNameByNumber((uint)selfnumber)
            };
            
            l_Caller.ResetData("current_caller");

            if (l_CalledPlayer.HasData("current_caller"))
            {
                l_Caller.SendNewNotification("Die angegebene Rufnummer ist derzeit im Gespraech.", notificationType:PlayerNotification.NotificationType.ERROR);
                return;
            }
            l_Caller.SetData("current_caller", (int)l_CalledPlayer.handy[0]);
            l_CalledPlayer.SetData("current_caller", (int)l_Caller.handy[0]);

            l_Caller.Player.TriggerEvent("setPhoneCallData", NAPI.Util.ToJson(l_CallerData));
            l_CalledPlayer.Player.TriggerEvent("setPhoneCallData", NAPI.Util.ToJson(l_CallingData));
        }
    }

    public class CallUser
    {
        [JsonProperty(PropertyName = "method")]
        public string method { get; set; }
        [JsonProperty(PropertyName = "caller")]
        public int caller { get; set; }
        [JsonProperty(PropertyName = "name")]
        public string name { get; set; }
    }
}

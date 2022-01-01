using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.Players;
using GVRP.Module.Support;

namespace GVRP.Module.Computer.Apps.SupportApp
{
    public class SupportKonversation : SimpleApp
    {
        public SupportKonversation() : base("SupportKonversation") { }

        [RemoteEvent]
        public async void requestSupportKonversation(Client client, string name)
        {
            
                var dbPlayer = client.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid()) return;
                if (string.IsNullOrEmpty(name)) return;

                if (dbPlayer.RankId == 0)
                {
                    dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                    return;
                }

                var findplayer = Players.Players.Instance.FindPlayer(name);
                if (findplayer == null) return;

                List<konversationObject> konversationList = new List<konversationObject>();
                var messages = KonversationModule.Instance.GetTicketKonversation(findplayer);

                foreach (var message in messages)
                {
                    konversationList.Add(new konversationObject() { id = (int)message.Player.Id, sender = message.Player.GetName(), receiver = message.Receiver, message = message.Message, date = message.Created_at });
                }

                var konvObject = new konvObject { konversation = konversationList, status = TicketModule.Instance.getCurrentChatStatus(findplayer) };
                var konversationJson = NAPI.Util.ToJson(konvObject);

                TriggerEvent(client, "responseSupportKonversation", konversationJson);
            
        }

        [RemoteEvent]
        public async void supportMessageSent(Client client, string name, string message)
        {
            
                var dbPlayer = client.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid()) return;
                if (string.IsNullOrEmpty(message)) return;

                if (dbPlayer.RankId == 0)
                {
                    dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                    return;
                }

                var findplayer = Players.Players.Instance.FindPlayer(name);
                if (findplayer == null) return;

                Konversation konversationMessage = new Konversation(dbPlayer, true, message);
                bool response = KonversationModule.Instance.Add(findplayer, konversationMessage);

                var konvMessage = new konversationObject() { id = (int)konversationMessage.Player.Id, sender = konversationMessage.Player.GetName(), receiver = konversationMessage.Receiver, message = konversationMessage.Message, date = konversationMessage.Created_at };
                var messageJson = NAPI.Util.ToJson(konvMessage);

                TriggerEvent(client, "updateSupportKonversation", messageJson);

                findplayer.SendNewNotification($"Antwort von {dbPlayer.GetName()}: {message}", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN, duration:20000);
                dbPlayer.SendNewNotification($"Die Antwort wurde an {findplayer.GetName()} gesendet!", title:"Admin", notificationType: PlayerNotification.NotificationType.ADMIN);
            
        }

        [RemoteEvent]
        public async void allowTicketResponse(Client client, string name, bool status)
        {
            
                var dbPlayer = client.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid()) return;
                if (string.IsNullOrEmpty(name)) return;

                if (dbPlayer.RankId == 0)
                {
                    dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                    return;
                }

                var findplayer = Players.Players.Instance.FindPlayer(name);
                if (findplayer == null) return;

                switch (status)
                {
                    case false:
                        TicketModule.Instance.ChangeChatStatus(findplayer, false);
                        findplayer.SendNewNotification($"Das Team Mitglied {dbPlayer.GetName()} hat den Chat geschlossen! Der /chat Befehl steht dir nicht mehr zur verfügung!", PlayerNotification.NotificationType.ADMIN, duration: 10000);
                        break;
                    case true:
                        TicketModule.Instance.ChangeChatStatus(findplayer, true);
                        findplayer.SendNewNotification($"Das Team Mitglied {dbPlayer.GetName()} hat eine Konversation mit dir begonnen. Um zu kommunizieren nutze: /chat nachricht", PlayerNotification.NotificationType.ADMIN, duration:15000);
                        break;
                }
            
        }

        public void sendMessage(Client client, string json)
        {
            var dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (string.IsNullOrEmpty(json)) return;

            TriggerEvent(client, "updateSupportKonversation", json);
        }

        public class konvObject
        {
            public List<konversationObject> konversation { get; set; }
            public bool status { get; set; }
        }

        public class konversationObject
        {
            [JsonProperty(PropertyName = "id")]
            public int id { get; set; }

            [JsonProperty(PropertyName = "sender")]
            public string sender { get; set; }

            [JsonProperty(PropertyName = "receiver")]
            public bool receiver { get; set; }

            [JsonProperty(PropertyName = "message")]
            public string message { get; set; }

            [JsonProperty(PropertyName = "date")]
            public DateTime date { get; set; }
        }
    }
}

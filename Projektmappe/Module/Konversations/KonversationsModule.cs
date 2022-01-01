using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GVRP.Module.Konversations
{
    public sealed class KonversationsModule : Module<KonversationsModule>
    {
        public Dictionary<Conversation, List<ConvMessage>> konversations = new Dictionary<Conversation, List<ConvMessage>>();
        private List<QueueObject> conversationsToSafe = new List<QueueObject>();

        public override bool Load(bool reload = false)
        {
            konversations = new Dictionary<Conversation, List<ConvMessage>>();
            return base.Load(reload);
        }

        // Ich hoffe der Shit funktioniert, weil die Funktion SaveConversation zusammen mit OnMinuteUpdate aufm Hauptthread ausgeführt wird
        // Sollte deshalb nichts aus dem Dictionary verloren gehen bei Masse
        public override void OnMinuteUpdate()
        {
            var l_Conversations = conversationsToSafe;
            if (l_Conversations.Count() == 0) return;

            conversationsToSafe = new List<QueueObject>();

            var l_KonversationsQuery = "INSERT IGNORE INTO `sms_konversations` (`id`, `player_1`, `player_2`) VALUES ";
            var l_MessagesQuery = "INSERT IGNORE INTO `sms_konversations_messages` (`sms_konversation_id`, `message`, `sender_id`) VALUES ";

            foreach (var l_Conv in l_Conversations)
            {
                l_KonversationsQuery += $"('{l_Conv.konv_id.ToString()}', '{l_Conv.player_1.ToString()}', '{l_Conv.player_2.ToString()}'),";
                l_MessagesQuery += $"('{l_Conv.konv_id.ToString()}', '{l_Conv.message}', '{l_Conv.sender.ToString()}'),";
            }

            l_KonversationsQuery = l_KonversationsQuery.Remove(l_KonversationsQuery.Length - 1);
            l_KonversationsQuery += ";";

            l_MessagesQuery = l_MessagesQuery.Remove(l_MessagesQuery.Length - 1);
            l_MessagesQuery += ";";

            MySQLHandler.ExecuteAsync(l_KonversationsQuery);
            MySQLHandler.ExecuteAsync(l_MessagesQuery);
        }

        public void SaveConversation(Conversation conversation, ConvMessage message)
        {
            var l_Object = new QueueObject();
            l_Object.konv_id = conversation.Id;
            l_Object.player_1 = conversation.Player1;
            l_Object.player_2 = conversation.Player2;
            l_Object.message = message.Message;
            l_Object.sender = message.SenderId;

            conversationsToSafe.Add(l_Object);
        }
    }

    public class QueueObject
    {
        public uint konv_id { get; set; }
        public uint player_1 { get; set; }
        public uint player_2 { get; set; }
        public string message { get; set; }
        public uint sender { get; set; }
    }
}

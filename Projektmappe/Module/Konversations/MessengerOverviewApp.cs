using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.Items;
using GVRP.Module.LeitstellenPhone;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Konversations
{
    public class MessengerOverviewApp : SimpleApp
    {
        public MessengerOverviewApp() : base("MessengerOverviewApp") { }

        [RemoteEvent]
        public void deletePhoneChat(Client p_Player, uint p_ConversationID)
        {
            DbPlayer l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null)
                return;

            if (l_DbPlayer.HasData("nsa_smclone")) return;
            var l_List = KonversationsModule.Instance.konversations.Keys.ToList();
            var l_Conv = l_List.Find(c => c.Id == p_ConversationID);
            if (l_Conv == null)
                return;

            KonversationsModule.Instance.konversations.Remove(l_Conv);
            MySQLHandler.ExecuteAsync($"DELETE FROM `sms_konversations` WHERE `id`={p_ConversationID};");
            MySQLHandler.ExecuteAsync($"DELETE FROM `sms_konversations_messages` WHERE `sms_konversation_id`={p_ConversationID};");
        }


        [RemoteEvent]
        public void sendPhoneMessage(Client p_Player, uint p_Receiver, string p_Message)
        {

            DbPlayer l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null)
                return;

            if (l_DbPlayer.HasData("nsa_smclone")) return;
            // Disable SMS Function for Leitstellen Module
            if (LeitstellenPhoneModule.TeamNumberPhones.ContainsKey((int) p_Receiver)) return;

            uint l_ConversationID = 0;
            Conversation l_Conversation = new Conversation();

            p_Message = p_Message.Replace("\"", "");
            p_Message = p_Message.Replace("'", "");
            p_Message = p_Message.Replace("`", "");
            p_Message = p_Message.Replace("´", "");

            var l_List = KonversationsModule.Instance.konversations.Keys.ToList();
            if (l_List.Exists(l_Konv => l_Konv.Player1 == l_DbPlayer.handy[0] && l_Konv.Player2 == p_Receiver))
            {
                var l_Conv = l_List.Find(l_C => l_C.Player1 == l_DbPlayer.handy[0] && l_C.Player2 == p_Receiver);
                var l_ConvMsgs = KonversationsModule.Instance.konversations.TryGetValue(l_Conv, out List<ConvMessage> l_MessengerList);
                if (!l_ConvMsgs)
                    return;

                if (l_MessengerList.Count == 0)
                    l_ConversationID = 0;
                else
                    l_ConversationID = l_MessengerList.First().KonversationId;
            }
            else if (l_List.Exists(l_Konv => l_Konv.Player1 == p_Receiver && l_Konv.Player2 == l_DbPlayer.handy[0]))
            {
                var l_Conv = l_List.Find(l_C => l_C.Player1 == p_Receiver && l_C.Player2 == l_DbPlayer.handy[0]);
                var l_ConvMsgs = KonversationsModule.Instance.konversations.TryGetValue(l_Conv, out List<ConvMessage> l_MessageList2);
                if (!l_ConvMsgs)
                    return;

                if (l_MessageList2.Count == 0)
                    l_ConversationID = 0;
                else
                    l_ConversationID = l_MessageList2.First().KonversationId;
            }

            if (l_ConversationID == 0)
            {
                if (l_List.Count == 0)
                    l_ConversationID = 1;
                else
                    l_ConversationID = l_List.Max(l_M => l_M.Id) + 1;

                l_Conversation.Id = l_ConversationID;
                l_Conversation.LastUpdated = DateTime.Now;
                l_Conversation.Player1 = l_DbPlayer.handy[0];
                l_Conversation.Player2 = p_Receiver;
                KonversationsModule.Instance.konversations.Add(l_Conversation, new List<ConvMessage>());
            }
            else
            {
                l_Conversation = l_List.Find(c => c.Id == l_ConversationID);
                if (l_Conversation == null)
                    return;
            }

            l_Conversation.LastUpdated = DateTime.Now;
            var l_MessageList = KonversationsModule.Instance.konversations[l_Conversation];
            uint l_MessageID;
            if (l_MessageList.Count == 0)
                l_MessageID = 0;
            else
                l_MessageID = l_MessageList.Max(l_Msg => l_Msg.Id);

            ConvMessage l_Message = new ConvMessage();
            l_Message.Id = l_MessageID;
            l_Message.KonversationId = l_ConversationID;
            l_Message.Message = p_Message;
            l_Message.SenderId = l_DbPlayer.handy[0];
            l_Message.TimeStamp = DateTime.Now;


            ClientKonversationMessage l_NewMessage = new ClientKonversationMessage();
            l_NewMessage.Id = l_MessageID;
            l_NewMessage.KonversationMessageUpdatedTime = new MessengerListApp().GetUpdatedTimeFormated(DateTime.Now);
            l_NewMessage.MessageSenderName = l_DbPlayer.handy[0].ToString();
            l_NewMessage.Message = p_Message;
            l_NewMessage.Receiver = false;

            var l_Json = NAPI.Util.ToJson(l_NewMessage);
            TriggerEvent(p_Player, "updateChat", l_Json);
            l_NewMessage.Receiver = true;
            l_Json = NAPI.Util.ToJson(l_NewMessage);
            DbPlayer l_Player = Players.Players.Instance.GetValidPlayers().Find(l_DbP => l_DbP.handy[0] == p_Receiver);
            if (l_Player != null && l_Player.IsValid())
            {
                TriggerEvent(l_Player.Player, "updateChat", l_Json);
            }

            KonversationsModule.Instance.konversations[l_Conversation].Add(l_Message);
            KonversationsModule.Instance.SaveConversation(l_Conversation, l_Message);

            var l_TargetPlayer = Players.Players.Instance.GetValidPlayers().ToList().Find(l_Pl => l_Pl.handy[0] == p_Receiver);
            if (l_TargetPlayer == null || !l_TargetPlayer.IsValid())
                return;

            if (l_TargetPlayer.Container.GetItemAmount(174) == 0)
            {
                l_DbPlayer.SendNewNotification("SMS versendet.");
            }
            else
            {
                if (!l_TargetPlayer.phoneSetting.flugmodus)
                {
                    l_TargetPlayer.SendNewNotification($"Neue SMS von: {l_DbPlayer.handy[0].ToString()}");
                }
            }

        }

        [RemoteEvent]
        public void responsePhoneMessage(Client p_Player, uint p_MsgPartner, string p_Answer)
        {
            DbPlayer l_DbPlayer = p_Player.GetPlayer();
            if (l_DbPlayer == null || !l_DbPlayer.IsValid())
                return;

            if (l_DbPlayer.HasData("nsa_smclone")) return;
            if (p_Answer == "")
                return;

            if (!l_DbPlayer.TakeBankMoney(1))
            {
                l_DbPlayer.SendNewNotification("Du benötigst mindestens $1 auf deinem Bank Konto!");
                return;
            }

            p_Answer = p_Answer.Replace("\"", "");
            p_Answer = p_Answer.Replace("'", "");
            p_Answer = p_Answer.Replace("`", "");
            p_Answer = p_Answer.Replace("´", "");


            // !A C H T U N G! - GRAUSAMER CODE BEGINNT
            var l_List = KonversationsModule.Instance.konversations.Keys.ToList();
            var l_Conv = l_List.Find(l_C => l_C.Player1 == l_DbPlayer.handy[0] && l_C.Player2 == p_MsgPartner);
            if (l_Conv == null)
                l_Conv = l_List.Find(l_C => l_C.Player2 == l_DbPlayer.handy[0] && l_C.Player1 == p_MsgPartner);

            if (l_Conv == null)
                return;

            var l_ConvMsgs = KonversationsModule.Instance.konversations.TryGetValue(l_Conv, out List<ConvMessage> l_MessageList);
            if (!l_ConvMsgs)
                return;
            // GRAUSAMER CODE VORBEI

            var l_Date = new MessengerListApp().GetUpdatedTimeFormated(DateTime.Now);
            uint l_ID;
            if (l_MessageList.Count == 0)
                l_ID = 1;
            else
                l_ID = l_MessageList.Max(l_M => l_M.Id) + 1;

            l_Conv.LastUpdated = DateTime.Now;
            ClientKonversationMessage l_NewMessage      = new ClientKonversationMessage();
            l_NewMessage.Id                             = l_ID;
            l_NewMessage.KonversationMessageUpdatedTime = l_Date;
            l_NewMessage.MessageSenderName              = l_DbPlayer.handy[0].ToString();
            l_NewMessage.Message                        = p_Answer;
            l_NewMessage.Receiver                       = false;

            var l_Json = NAPI.Util.ToJson(l_NewMessage);
            TriggerEvent(p_Player, "updateChat", l_Json);

            ConvMessage l_Message       = new ConvMessage();
            l_Message.Id                = l_ID;
            l_Message.KonversationId    = l_Conv.Id;
            l_Message.Message           = p_Answer;
            l_Message.SenderId          = l_DbPlayer.handy[0];
            l_Message.TimeStamp         = DateTime.Now;

            l_MessageList.Add(l_Message);
            KonversationsModule.Instance.konversations[l_Conv] = l_MessageList;

            MySQLHandler.ExecuteAsync($"INSERT INTO `sms_konversations_messages` (`sms_konversation_id`, `message`, `sender_id`) VALUES ({l_Conv.Id}, '{p_Answer}', '{l_DbPlayer.handy[0].ToString()}');");

            var l_Player = Players.Players.Instance.GetValidPlayers().Find(l_DbP => l_DbP.handy[0] == p_MsgPartner);
            if (l_Player == null)
                return;

            if (l_Player.Container.GetItemAmount(174) == 0)
            {
                l_DbPlayer.SendNewNotification("SMS versendet.");
                return;
            }

            if (!l_Player.phoneSetting.flugmodus)
            {
                l_NewMessage.Receiver = true;
                l_Json = NAPI.Util.ToJson(l_NewMessage);
                TriggerEvent(l_Player.Player, "updateChat", l_Json);
                l_Player.SendNewNotification("Du hast eine SMS erhalten!");
                l_Player.Player.TriggerEvent("playSMSRingtone");
            }
        }
    }
}

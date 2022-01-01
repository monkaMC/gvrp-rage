using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GVRP.Module.Konversations
{
    public class KonversationMessageModule : SqlModule<KonversationMessageModule, KonversationMessage, uint>
    {
        public override Type[] RequiredModules()
        {
            return new[] { typeof(KonversationsModule), typeof(KonversationModule) };
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `sms_konversations_messages`;";
        }

        protected override void OnItemLoaded(KonversationMessage message)
        {
            ConvMessage l_Message       = new ConvMessage();
            l_Message.Id                = message.Id;
            l_Message.KonversationId    = message.KonversationId;
            l_Message.Message           = message.Message;
            l_Message.SenderId          = message.SenderId;
            l_Message.TimeStamp         = message.TimeStamp;

            if (!KonversationModule.Instance.GetAll().ContainsKey(l_Message.KonversationId)) return;
            Konversation l_Konversation = KonversationModule.Instance.Get(l_Message.KonversationId);
            
            var l_List = KonversationsModule.Instance.konversations.Keys.ToList();
            var l_Conv = l_List.Find(c => c.Id == l_Konversation.Id);
            if (l_Conv == null || l_Message == null)
                return;

            if(KonversationsModule.Instance.konversations.ContainsKey(l_Conv) && KonversationsModule.Instance.konversations[l_Conv] != null) KonversationsModule.Instance.konversations[l_Conv].Add(l_Message);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace GVRP.Module.Konversations
{
    public class KonversationModule : SqlModule<KonversationModule, Konversation, uint>
    {
        public override Type[] RequiredModules()
        {
            return new[] { typeof(KonversationsModule) };
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `sms_konversations`;";
        }

        protected override void OnItemLoaded(Konversation konversation)
        {
            Conversation l_Conversation = new Conversation();
            l_Conversation.Id           = konversation.Id;
            l_Conversation.LastUpdated  = konversation.LastUpdated;
            l_Conversation.Player1      = konversation.Player1;
            l_Conversation.Player2      = konversation.Player2;

            KonversationsModule.Instance.konversations.Add(l_Conversation, new List<ConvMessage>());
        }
    }
}

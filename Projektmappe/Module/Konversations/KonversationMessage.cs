using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace GVRP.Module.Konversations
{
    public class KonversationMessage : Loadable<uint>
    {
        public uint Id { get; }
        public uint KonversationId { get; }
        public string Message { get; }
        public DateTime TimeStamp { get; }
        public uint SenderId { get; }

        public KonversationMessage(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            KonversationId = reader.GetUInt32("sms_konversation_id");
            Message = reader.GetString("message");
            TimeStamp = reader.GetDateTime("timestamp");
            SenderId = reader.GetUInt32("sender_id");
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }

    public class ConvMessage
    {
        public uint Id { get; set; }
        public uint KonversationId { get; set; }
        public string Message { get; set; }
        public DateTime TimeStamp { get; set; }
        public uint SenderId { get; set; }
    }
}

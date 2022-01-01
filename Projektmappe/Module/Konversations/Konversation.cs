using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace GVRP.Module.Konversations
{
    public class Konversation : Loadable<uint>
    {
        public uint Id { get; }
        public uint Player1 { get; }
        public uint Player2 { get; }
        public DateTime LastUpdated { get; }

        public Konversation(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Player1 = reader.GetUInt32("player_1");
            Player2 = reader.GetUInt32("player_2");
            LastUpdated = reader.GetDateTime("last_updated");
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }

    public class Conversation
    {
        public uint Id { get; set; }
        public uint Player1 { get; set; }
        public uint Player2 { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}

using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace GVRP.Module.Warrants
{
    public class Warrant
    {
        public uint WarrantedPlayerId { get; set; }
        public uint AcceptorPlayerId { get; set; }
        public uint CreatorPlayerId { get; set; }
        public string Reason { get; set; }
        public bool Accepted { get; set; }
        public DateTime AcceptedDate { get; set; }

        public Warrant(MySqlDataReader reader)
        {
            WarrantedPlayerId = reader.GetUInt32("warrant_player_id");
            AcceptorPlayerId = reader.GetUInt32("acceptor_player_id");
            CreatorPlayerId = reader.GetUInt32("creator_player_id");
            Reason = reader.GetString("reason");
            Accepted = reader.GetInt32("accepted") == 1;
        }

        public Warrant(uint warrantPlayerId, uint acceptorPlayerId, uint creatorPlayerId, string reason, bool accepted)
        {
            WarrantedPlayerId = warrantPlayerId;
            AcceptorPlayerId = acceptorPlayerId;
            CreatorPlayerId = creatorPlayerId;
            Reason = reason;
            Accepted = accepted;
        }

    }
}

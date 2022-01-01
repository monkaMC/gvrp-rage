using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace GVRP.Module.NSA.Observation
{
    public class NSADBObservation : Loadable<uint>
    {
        public uint Id { get; set; }

        public int PlayerId { get; set; }
        public int AcceptedPlayerId { get; set; }

        public DateTime Added { get; set; }
        public bool Agreed { get; set; }

        public string Reason { get; set; }

        public NSADBObservation(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            PlayerId = reader.GetInt32("player_id");
            AcceptedPlayerId = reader.GetInt32("accepted_player_id");
            Added = reader.GetDateTime("added");
            Agreed = reader.GetInt32("agreed") == 1;
            Reason = reader.GetString("reason");
        }
        public override uint GetIdentifier()
        {
            return Id;
        }
    }

    public class NSAObservation
    {
        public int PlayerId { get; set; }
        public int AcceptedPlayerId { get; set; }

        public DateTime Added { get; set; }
        public bool Agreed { get; set; }
        public string Reason { get; set; }

    }
}

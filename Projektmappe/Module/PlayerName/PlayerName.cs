using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace GVRP.Module.PlayerName
{
    public class PlayerName : Loadable<uint>
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public uint ForumId { get; set; }

        public int HandyNr { get; set; }
        public uint RankId { get; set; }

        public PlayerName(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("Name");
            ForumId = reader.GetUInt32("forumid");
            HandyNr = reader.GetInt32("handy");
            RankId = reader.GetUInt32("rankId");
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}

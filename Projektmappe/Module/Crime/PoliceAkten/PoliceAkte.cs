using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace GVRP.Module.Crime.PoliceAkten
{
    public class PoliceAkte : Loadable<uint>
    {
        public uint Id { get; }
        public uint PlayerId { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public DateTime Created { get; set; }
        public DateTime Closed { get; set; }
        public string Officer { get; set; }
        public bool Open { get; set; }

        public PoliceAkte(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            PlayerId = reader.GetUInt32("player_id");
            Title = reader.GetString("title");
            Text = reader.GetString("text");
            Created = reader.GetDateTime("created");
            Closed = reader.GetDateTime("closed");
            Officer = reader.GetString("officer");
            Open = reader.GetInt32("open") == 1;
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }

    public class PoliceServerAkte
    {
        public uint Id { get; }
        public uint PlayerId { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public DateTime Created { get; set; }
        public DateTime Closed { get; set; }
        public string Officer { get; set; }
        public bool Open { get; set; }

        public PoliceServerAkte(uint id, uint player_id, string title, string text, DateTime created, DateTime closed, string officer, bool open)
        {
            Id = id;
            PlayerId = player_id;
            Title = title;
            Text = text;
            Created = created;
            Closed = closed;
            Officer = officer;
            Open = open;
        }
    }
}

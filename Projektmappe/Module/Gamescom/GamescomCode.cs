using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Text;
using GTANetworkMethods;
using MySql.Data.MySqlClient;

namespace GVRP.Module.Gamescom
{
    public class GamescomCode : Loadable<uint>
    {

        public uint Id { get; }
        public string Code { get; set; }
        public uint PlayerId { get; set; }
        public int RewardId { get; set; }
        public String Text { get; set; }

        public GamescomCode(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Code = reader.GetString("code");
            PlayerId = reader.GetUInt32("player_id");
            RewardId = 0;
            Text = "";
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }

}

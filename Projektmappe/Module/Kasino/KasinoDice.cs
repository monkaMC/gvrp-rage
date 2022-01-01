using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Kasino
{
    public class KasinoDice : Loadable<uint>
    {
        public uint Id { get; }
        public int Price { get; set; }
        public int MinPrice { get; set; }
        public int MaxPrice { get; set; }
        public Vector3 Position { get; }
        public int Radius { get; set; }
        public bool IsInGame { get; set; } = false;

        public List<DbPlayer> Participant { get; set; }

        public DateTime StartTime { get; set; }
        public bool FinishGame { get; set; }


        public KasinoDice(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Price = 0;
            MinPrice = reader.GetInt32("minprice");
            MaxPrice = reader.GetInt32("maxprice");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"),
                reader.GetFloat("pos_z"));
            Radius = reader.GetInt32("radius");
            Participant = new List<DbPlayer>();
            StartTime = DateTime.MinValue;
            FinishGame = false;
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}

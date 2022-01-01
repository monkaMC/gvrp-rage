using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;

namespace GVRP.Module.Farming
{
    public class FarmPosition : Loadable<uint>
    {
        public uint Id { get; }
        public uint FarmSpotId { get; }
        public Vector3 Position { get; }
        public uint range { get; }
        //public DateTime ExhaustDate { get; set; }
        //public int ActualExhaustState { get; set; }

        public FarmPosition(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32(0);
            FarmSpotId = reader.GetUInt32(1);
            Position = new Vector3(reader.GetFloat(2), reader.GetFloat(3),
                reader.GetFloat(4));
            range = reader.GetUInt32(5);
            //ExhaustDate = DateTime.Now;
            //ActualExhaustState = 0;
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
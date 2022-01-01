using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace GVRP.Module.Business.FuelStations
{
    public class FuelStationGas : Loadable<uint>
    {
        public uint Id { get; }
        public uint FuelStationId { get; }
        public Vector3 Position { get; }

        public FuelStationGas(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            FuelStationId = reader.GetUInt32("fuelstation_id");
            Position = new Vector3(reader.GetFloat("pos_x"),
                reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}

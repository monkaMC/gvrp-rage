using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using GVRP.Module.NpcSpawner;

namespace GVRP.Module.Schwarzgeld
{
    public class ExchangeLocation : Loadable<uint>
    {
        public uint Id { get; set; }
        public int Team_id { get; set; }
        public Vector3 Position { get; set; }
        public float Heading { get; set; }
        public Vector3 VehicleSpawn { get; set; }
        public float VehicleRotation { get; set; }
        public Vector3 ExchangeDestroyLocation { get; set; }
        public PedHash PedHash { get; set; }
        public int ExchangedAmount { get; set; }
        public bool Bestochen { get; set; }
        public bool Alerted { get; set; }
        public Marker ExchangeMarker { get; set; }

        public ExchangeLocation(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Team_id = reader.GetInt32("team_id");
            Position = new Vector3(reader.GetInt32("pos_x"), reader.GetInt32("pos_y"), reader.GetInt32("pos_z"));
            Heading = reader.GetFloat("heading");
            VehicleSpawn = new Vector3(reader.GetInt32("vehicle_x"), reader.GetInt32("vehicle_y"), reader.GetInt32("vehicle_z"));
            VehicleRotation = reader.GetFloat("vehicle_rotation");
            ExchangeDestroyLocation = new Vector3(reader.GetInt32("destroy_x"), reader.GetInt32("destroy_y"), reader.GetInt32("destroy_z"));
            PedHash = Enum.TryParse(reader.GetString("ped_hash"), true, out PedHash skin) ? skin : PedHash.Abigail;
            ExchangedAmount = reader.GetInt32("exchanged_amount");
            Bestochen = false;
            Alerted = false;

            new Npc(PedHash, Position, Heading, 0);
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}

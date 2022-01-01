using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using GVRP.Module.Items;
using GVRP.Module.NpcSpawner;

namespace GVRP.Module.NpcSpawner
{
    public class AdditionallyNpc : Loadable<uint>
    {
        public uint Id { get; set; }
        public Vector3 Position { get; set; }
        public float Heading { get; set; }
        public uint Dimension { get; set; }
        public PedHash PedHash { get; set; }

        public AdditionallyNpc(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"),
                reader.GetFloat("pos_z"));
            Heading = reader.GetFloat("heading");
            Dimension = reader.GetUInt32("dimension");
            PedHash = Enum.TryParse(reader.GetString("ped_hash"), true, out PedHash skin) ? skin : PedHash.Trucker01SMM;

            new Npc(PedHash, Position, Heading, Dimension);
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using MySql.Data.MySqlClient;

namespace GVRP.Module.Pet
{
    public class PetData : Loadable<uint>
    {
        public uint Id { get; }
        public string Name { get; }
        public int Price { get; }
        public PedHash Model { get; }

        public PetData(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
            Model = (PedHash)Enum.Parse(typeof(PedHash), reader.GetString("model"), true);
            Price = reader.GetInt32("price");
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}

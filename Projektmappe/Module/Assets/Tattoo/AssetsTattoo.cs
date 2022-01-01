using System.Collections.Generic;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Assets.Tattoo
{
    public class AssetsTattoo : Loadable<uint>
    {
        public uint Id { get; }
        public string Name { get; }
        public string HashMale { get; }
        public string HashFemale { get; }
        public string Collection { get; }
        public int ZoneId { get; }
        public int Price { get; }
        
        public AssetsTattoo(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
            HashMale = reader.GetString("hash_male");
            HashFemale = reader.GetString("hash_female");
            Collection = reader.GetString("collection");
            ZoneId = reader.GetInt32("zone_id");
            Price = reader.GetInt32("price");
        }

        public override uint GetIdentifier()
        {
            return Id;
        }

        public string GetHashForPlayer(DbPlayer dbPlayer)
        {
            return dbPlayer.Customization.Gender == 0 ? HashMale : HashFemale;
        }
    }
}
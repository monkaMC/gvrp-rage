using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace GVRP.Module.Business.Raffinery
{
    public class RaffineryAusbaustufe : Loadable<uint>
    {
        public uint Id { get; }
        public int Kosten { get; }
        public int MinGenerate { get; }
        public int MaxGenerate { get; }
        public int StorageVolume { get; }

        public RaffineryAusbaustufe(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Kosten = reader.GetInt32("kosten");
            MinGenerate = reader.GetInt32("min_generate");
            MaxGenerate = reader.GetInt32("max_generate");
            StorageVolume = reader.GetInt32("storage_volume");
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}

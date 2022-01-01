using System;
using System.Collections.Generic;
using GTANetworkAPI;
using MySql.Data.MySqlClient;

namespace GVRP.Module.Farming
{
    public class FarmSpot : Loadable<uint>
    {
        public uint Id { get; }
        public uint ItemId { get; }
        public uint RequiredItemId { get; }
        public float RequiredItemChanceToBreak { get; }
        public int MinResultAmount { get; }
        public int MaxResultAmount { get; }
        public string RessourceName { get; }
        public FarmType SpecialType { get; }
        public List<Vector3> Positions { get; }

        public uint RequiredLevel { get; }

        public FarmSpot(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            ItemId = reader.GetUInt32("item_id");
            MinResultAmount = reader.GetInt32("min_amount");
            MaxResultAmount = reader.GetInt32("max_amount");
            RessourceName = reader.GetString("ressource_name");
            SpecialType = (FarmType)reader.GetInt32("type");
            RequiredItemId = reader.GetUInt32("required_item");
            RequiredItemChanceToBreak = reader.GetFloat("breakchance");
            Positions = new List<Vector3>();
            RequiredLevel = reader.GetUInt32("required_level");
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
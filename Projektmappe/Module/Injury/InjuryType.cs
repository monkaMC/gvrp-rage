using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Module.Items;

namespace GVRP.Module.Injury
{
    public class InjuryType : Loadable<uint>
    {
        public uint Id { get; }
        public string Name { get; }
        public int TimeToDeath { get; set; }
        public int StabilizedInjuryId { get; }
        public bool NeedHospital { get; }
        public uint ItemToStabilizeId { get; }
        

        public InjuryType(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
            TimeToDeath = reader.GetInt32("time_to_death");
            StabilizedInjuryId = reader.GetInt32("stabilized_injury_id");
            NeedHospital = reader.GetInt32("need_hospital") == 1; 
            ItemToStabilizeId = reader.GetUInt32("item_to_stabilize");
        }
        
        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
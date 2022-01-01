using MySql.Data.MySqlClient;

namespace GVRP.Module.Players.PlayerTask
{
    public enum PlayerTaskTypeId
    {
        KellerAusbau = 1,
        LaborAusbau = 2,
        VehicleImport = 3,
        Sicherheitssystem = 4,
        MoneyKellerAusbau = 5,
    }
    
    public class PlayerTaskType : Loadable<uint>
    {
        
        private uint Identifier { get; }
        public PlayerTaskTypeId Id { get; }
        public string Name { get; }
        public int TaskTime { get; }

        public PlayerTaskType(MySqlDataReader reader) : base(reader)
        {
            Identifier = reader.GetUInt32(0);
            Id = (PlayerTaskTypeId) Identifier;
            Name = reader.GetString(1);
            TaskTime = reader.GetInt32(2);
        }

        public override uint GetIdentifier()
        {
            return Identifier;
        }
    }
}
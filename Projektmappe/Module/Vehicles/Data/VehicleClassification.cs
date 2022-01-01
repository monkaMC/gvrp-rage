using MySql.Data.MySqlClient;

namespace GVRP.Module.Vehicles.Data
{
    public enum VehicleClassificationTypes
    {
        Pkw = 1,
        Fahrrad = 2,
        Boot = 3,
        Lkw = 4,
        Trailer = 5,
        SmallLkw = 6,
        Motorrad = 7,
        Helikopter = 8,
        Flugzeug = 9,
        Baufahrzeug = 10
    }

    public class VehicleClassification : Loadable<uint>
    {
        public uint Id { get; }
        public string Name { get; }
        public bool UseBreak { get; }
        public uint ScrapYard { get; }
        
        public VehicleClassification(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32(0);
            Name = reader.GetString(1);
            UseBreak = reader.GetBoolean(2);
            ScrapYard = reader.GetUInt32(3);
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
using System.Data;

namespace GVRP.Module.Vehicles.Mod
{
    public class VehicleModType
    {
        public int Id { get; }
        public string Name { get; }
        public int MaxPrice { get; }
        public int Divisor { get; }

        public VehicleModType(IDataRecord reader)
        {
            Id = reader.GetInt32(0);
            Name = reader.GetString(1);
            MaxPrice = reader.GetInt32(2);
            Divisor = reader.GetInt32(3);
        }
    }
}
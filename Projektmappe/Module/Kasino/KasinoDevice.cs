using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;

namespace GVRP.Module.Kasino
{
    public enum MachineType
    {
        SlotMachine,
        Roulett,
        Wheel
    }
    public class KasinoDevice : Loadable<uint>
    {
        public uint Id { get; }
        public MachineType Type { get; }
        public int Price { get; }
        public int MinPrice { get; }
        public int MaxPrice { get; }
        public int PriceStep { get; }
        public int MaxMultiple { get; }
        public Vector3 Position { get; }
        public int Radius { get; set; }
        public bool IsInUse { get; set; } = false;

        public KasinoDevice(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Type = (MachineType)Enum.Parse(typeof(MachineType), reader.GetString("type"));
            Price = reader.GetInt32("price");
            MinPrice = reader.GetInt32("minprice");
            MaxPrice = reader.GetInt32("maxprice");
            PriceStep = reader.GetInt32("pricestep");
            MaxMultiple = reader.GetInt32("maxmultiple");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"),
                reader.GetFloat("pos_z"));
            Radius = reader.GetInt32("radius");
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}

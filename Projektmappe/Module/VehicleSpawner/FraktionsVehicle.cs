using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace GVRP.Module.VehicleSpawner
{
    public class FraktionsVehicle : Loadable<uint>
    {
        public uint Id { get; }
        public string VehicleHash { get; }
        public uint TeamId { get; }
        public int Color1 { get; }
        public int Color2 { get; }
        public int Model { get; }
        public int Fuel { get; }
        public Vector3 Position { get; set; }
        public float Rotation { get; set; }
        public string Tuning { get; set; }
        public string Neon { get; set; }
        public bool GpsTracker { get; set; }
        public bool Registered { get; set; }
        public string Plate { get; set; }
        public int WheelClamp { get; set; }
        
        public bool AlarmSystem { get; set; }
        public uint lastGarage { get; set; }

        public int CarSellPrice { get; set; }

        public FraktionsVehicle(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            VehicleHash = reader.GetString("vehiclehash");
            TeamId = reader.GetUInt32("team");
            Color1 = reader.GetInt32("color1");
            Color2 = reader.GetInt32("color2");
            Model = reader.GetInt32("model");
            Fuel = reader.GetInt32("fuel");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            Rotation = reader.GetFloat("rotation");
            Tuning = reader.GetString("tuning");
            Neon = "";
            GpsTracker = reader.GetInt32("gps_tracker") == 1;
            Registered = reader.GetInt32("registered") == 1;
            Plate = reader.GetString("plate");
            WheelClamp = reader.GetInt32("WheelClamp");
            AlarmSystem = reader.GetInt32("alarm_system") == 1;
            lastGarage = reader.GetUInt32("lastGarage");
            CarSellPrice = reader.GetInt32("carsell_price");
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}

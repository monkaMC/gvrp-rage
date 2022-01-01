using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.Vehicles;

namespace GVRP.Module.VehicleSpawner
{
    public class PlayerVehicle : Loadable<uint>
    {
        public uint Id { get; }
        public VehicleHash Hash { get; }
        public int PlayerId { get; set; }
        public Vector3 Position { get; set; }
        public float Rotation { get; set; }
        public int ColorPrimary { get; set; }
        public int ColorSecondary { get; set; }
        public float Fuel { get; set; }
        public string Tuning { get; set; }
        public string Neon { get; set; }
        public int Health { get; set; }
        public int InGarage { get; set; }
        public long Mileage { get; set; }
        public int GarageId { get; set; }
        public string Plate { get; set; }
        public int PlateStyle { get; set; }
        public int Model { get; set; }
        public bool GpsTracker { get; set; }

        public bool Registered { get; set; }
        public bool TuningState { get; set; }
        public int WheelClamp { get; set; }
        public bool AlarmSystem { get; set; }

        
        public PlayerVehicle(MySqlDataReader reader) : base(reader)
        {
            if(!Enum.TryParse(reader.GetString("vehiclehash"), true, out VehicleHash hash))
            {
                hash = VehicleHash.Faggio;
            }

            Id = reader.GetUInt32("id");
            Hash = hash;
            PlayerId = reader.GetInt32("owner");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            Rotation = reader.GetFloat("heading");
            ColorPrimary = reader.GetInt32("color1");
            ColorSecondary = reader.GetInt32("color2");
            Fuel = reader.GetFloat("fuel");
            Tuning = reader.GetString("tuning");
            Neon = reader.GetString("Neon");
            Health = reader.GetInt32("zustand");
            InGarage = reader.GetInt32("inGarage");
            Mileage = reader.GetInt64("km");
            GarageId = reader.GetInt32("garage_id");
            Plate = reader.GetString("plate");
            Model = reader.GetInt32("model");
            GpsTracker = reader.GetInt32("gps_tracker") == 1;
            Registered = reader.GetInt32("registered") == 1;
            TuningState = reader.GetInt32("TuningState") == 1;
            WheelClamp = reader.GetInt32("WheelClamp");
            AlarmSystem = reader.GetInt32("alarm_system") == 1;
            PlateStyle = 1;
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}

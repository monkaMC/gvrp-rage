using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using MySql.Data.MySqlClient;

namespace GVRP.Module.Delivery
{
    public class DeliveryJobType : Loadable<uint>
    {
        public enum DeliverToType
        {
            HAUS,
            APOTHEKE
        }



        public uint Id { get; }
        public DeliveryJob DeliveryJob { get; set; }
        public String Name { get; set; }
        public uint VehicleModel { get; set; }
        public int NeededSkillPoints { get; set; }
        public int MinPositionAmount { get; set; }
        public int MaxPositionAmount { get; set; }
        public int RequiredLicence { get; set; }
        public int MaxDistance { get; set; }
        public DeliverToType DeliverTo { get; set; }


        public DeliveryJobType(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            DeliveryJob = DeliveryJobModule.Instance.Get(reader.GetUInt32("delivery_job_id"));
            Name = reader.GetString("name");
            VehicleModel = reader.GetUInt32("vehicle_model");
            NeededSkillPoints = reader.GetInt32("needed_skillpoints");
            MinPositionAmount = reader.GetInt32("min_positions");
            MaxPositionAmount = reader.GetInt32("max_positions");
            RequiredLicence = reader.GetInt32("required_licence");
            MaxDistance = reader.GetInt32("max_distance");
            Enum.TryParse(typeof(DeliverToType), reader.GetInt32("deliver_to").ToString(), out object type);
            DeliverTo = (DeliverToType) type;
        }


        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}

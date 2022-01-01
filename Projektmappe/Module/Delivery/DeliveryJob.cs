using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Module.NpcSpawner;

namespace GVRP.Module.Delivery
{
    public class DeliveryJob : Loadable<uint>
    {

        public uint Id { get; }
        public uint SkillpointType { get; }
        public String Name { get; set; }
        public Vector3 Position { get; set; }
        public float Heading { get; set; }
        public PedHash PedHash { get; set; }

        public Vector3 LoadPosition { get; set; }
        public float LoadHeading { get; set; }
        public PedHash LoadPedHash { get; set; }
        public int RequiredLevel { get; set; }


        public List<KeyValuePair<uint, DeliveryJobSpawnpoint>> DeliveryJobSpawnpoints { get; set; }
        public DeliveryJob(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            SkillpointType = reader.GetUInt32("skillpoint_type");
            Name = reader.GetString("name");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"),
                reader.GetFloat("pos_z"));
            Heading = reader.GetFloat("heading");
            PedHash = Enum.TryParse(reader.GetString("pedhash"), true, out PedHash skin) ? skin : GTANetworkAPI.PedHash.Abigail;
            new Npc(PedHash, Position, Heading, 0);

            LoadPosition = new Vector3(reader.GetFloat("load_pos_x"), reader.GetFloat("load_pos_y"),
                reader.GetFloat("load_pos_z"));
            LoadHeading = reader.GetFloat("load_heading");
            LoadPedHash = Enum.TryParse(reader.GetString("load_pedhash"), true, out PedHash loadSkin) ? loadSkin : GTANetworkAPI.PedHash.Abigail;
            RequiredLevel = reader.GetInt32("required_level");
            new Npc(LoadPedHash, LoadPosition, LoadHeading, 0);

            DeliveryJobSpawnpoints = DeliveryJobSpawnpointModule.Instance.GetAll().Where(d => d.Value.DeliveryJobId == Id).ToList();

            NAPI.TextLabel.CreateTextLabel(Name, Position.Add(new Vector3(0, 0, 1.1d)), 18, 1.5f, 0, new Color(230, 123, 0), true, 0);
            NAPI.TextLabel.CreateTextLabel(Name + " Warenausgabe", LoadPosition.Add(new Vector3(0, 0, 1.1d)), 18, 1.5f, 0, new Color(230, 123, 0), true, 0);


        }


        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}

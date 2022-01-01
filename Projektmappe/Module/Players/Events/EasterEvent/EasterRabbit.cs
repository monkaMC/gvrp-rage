using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.NpcSpawner;

namespace GVRP.Module.Players.Events.EasterEvent
{
    public class EasterRabbit : Loadable<uint>
    {
        public uint Id { get; set; }
        public Vector3 Position { get; set; }
        public float Heading { get; set; }

        public DateTime LastUsed { get; set; }
        
        public EasterRabbit(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"),
                reader.GetFloat("pos_z"));
            Heading = reader.GetFloat("heading");

            LastUsed = DateTime.Now.AddMinutes(-15);

            if(EasterRabbitModule.Instance.isActive) new Npc(GTANetworkAPI.PedHash.Rabbit, Position, Heading, 0);
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}


using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using GVRP.Module.NpcSpawner;
using GVRP.Module.Spawners;

namespace GVRP.Module.Items
{
    public class ItemOrderNpc : Loadable<uint>
    {
        public uint Id { get; }
        public PedHash NpcName { get; }
        public Vector3 Position { get; }
        public float Heading { get; }
        public string Name { get; }
        public List<ItemOrderNpcItem> NpcItems { get; }
        public Npc Ped { get; }
        public HashSet<int> RequiredTeams { get; }

        public ItemOrderNpc(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            NpcName = Enum.TryParse(reader.GetString("npchash"), true, out PedHash skin) ? skin : PedHash.Autoshop02SMM;
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            Heading = reader.GetFloat("heading");
            Name = reader.GetString("name");

            NpcItems = ItemOrderNpcItemModule.Instance.GetAll().Values.Where(ioni => ioni.NpcId == Id).ToList();
            Ped = new Npc(NpcName, Position, Heading, 0);

            RequiredTeams = new HashSet<int>();
            string teamString = reader.GetString("required_teams");
            if (!string.IsNullOrEmpty(teamString))
            {
                var splittedTeams = teamString.Split(',');
                foreach (var teamIdString in splittedTeams)
                {
                    if (!int.TryParse(teamIdString, out var teamId)) continue;
                    RequiredTeams.Add(teamId);
                }
            }
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}

using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using GVRP.Module.Items;
using GVRP.Module.NpcSpawner;
using GVRP.Module.Teams;

namespace GVRP.Module.Shops
{
    public class Shop : Loadable<uint>
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public float Heading { get; set; }
        public List<ShopItem> ShopItems { get; set; }
        public bool Robbed { get; set; }
        public PedHash Ped { get; set; }
        public Vector3 DeliveryPosition { get; set; }
        public Vector3 RobPosition { get; set; }
        public HashSet<Team> Teams { get; set; }

        public bool SchwarzgeldUse { get; }

        public bool Marker { get; }

        public Shop(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"),
                reader.GetFloat("pos_z"));
            Heading = reader.GetFloat("heading");

            Ped = Enum.TryParse(reader.GetString("ped_hash"), true, out PedHash skin) ? skin : PedHash.ShopKeep01;
            DeliveryPosition = new Vector3(reader.GetFloat("deliver_pos_x"), reader.GetFloat("deliver_pos_y"),
                reader.GetFloat("deliver_pos_z"));
            Robbed = false;
            ShopItems = new List<ShopItem>();
            SchwarzgeldUse = reader.GetInt32("schwarzgelduse") == 1;
            Marker = reader.GetInt32("marker") == 1;
            if(Position.X != 0 && Position.Y != 0) new Npc(Ped, Position, Heading, 0);

            var teamString = reader.GetString("team");
            Teams = new HashSet<Team>();
            if (!string.IsNullOrEmpty(teamString))
            {
                var splittedTeams = teamString.Split(',');
                foreach (var teamIdString in splittedTeams)
                {
                    if (!uint.TryParse(teamIdString, out var teamId) || Teams.Contains(TeamModule.Instance[teamId])) continue;
                    Teams.Add(TeamModule.Instance[teamId]);
                }
            }

            RobPosition = new Vector3(reader.GetFloat("rob_pos_x"), reader.GetFloat("rob_pos_y"),
                reader.GetFloat("rob_pos_z"));
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }

    public class ShopItem : Loadable<uint>
    {
        public uint Id { get; set; }
        public uint ShopId { get; set; }
        public uint ItemId { get; set; }
        public int Price { get; set; }
        public string Name { get; set; }

        public int Stored { get; set; }
        public int StoredMax { get; set; }
        public bool IsStoredItem { get; set; }
        public int EKPrice { get; }
        public int RequiredChestItemId { get; }

        public ShopItem(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            ShopId = reader.GetUInt32("shop_id");
            ItemId = reader.GetUInt32("item_id");
            Name = ItemModelModule.Instance.Get(ItemId).Name;
            Price = reader.GetInt32("price");
            Stored = reader.GetInt32("stored");
            StoredMax = reader.GetInt32("max_stored");
            EKPrice = reader.GetInt32("ek_price");
            RequiredChestItemId = reader.GetInt32("required_chest_item_id");
            IsStoredItem = StoredMax > 0 && EKPrice > 0;
        }
        
        public override uint GetIdentifier()
        {
            return Id;
        }

        public int GetRequiredAmount()
        {
            return StoredMax - Stored;
        }

        public void SaveStoreds()
        {
            MySQLHandler.ExecuteAsync($"UPDATE shops_items SET stored = '{Stored}' WHERE id = '{Id}';");
        }
    }
}
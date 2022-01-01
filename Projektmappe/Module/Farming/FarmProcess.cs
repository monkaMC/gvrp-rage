using System;
using System.Collections.Generic;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Module.Items;
using GVRP.Module.Logging;
using GVRP.Module.NpcSpawner;
using GVRP.Module.Spawners;

namespace GVRP.Module.Farming
{
    public class FarmProcess : Loadable<uint>
    {
        public uint Id { get; }
        public PedHash NpcPed { get; }
        public Vector3 NpcPosition { get; }
        public float NpcHeading { get; }
        public string ProcessName { get; }
        public uint RewardItemId { get; }
        public int RewardItemAmount { get; }
        public GTANetworkAPI.Marker Ped { get; }
        public Dictionary<ItemModel, int> RequiredItems { get; }
        public int RequiredTime { get; }
        public bool UseFromVehicle { get; set; }

        public FarmProcess(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            NpcPosition = new Vector3(reader.GetFloat("npc_pos_x"), reader.GetFloat("npc_pos_y"), reader.GetFloat("npc_pos_z"));
            NpcHeading = reader.GetFloat("npc_float");
            ProcessName = reader.GetString("npc_name");
            NpcPed = (PedHash) Enum.Parse(typeof(PedHash), reader.GetString("npc_pedhash"), true);
            RewardItemId = reader.GetUInt32("reward_item_id");
            RewardItemAmount = reader.GetInt32("reward_item_amount");
            RequiredTime = reader.GetInt32("required_time");
            UseFromVehicle = reader.GetInt32("use_from_vehicle") == 1;

            RequiredItems = new Dictionary<ItemModel, int>();
            var itemsString = reader.GetString("required_items");
            if (itemsString.Contains(","))
            {
                string[] itemsData = itemsString.Split(',');
                foreach (var xItemData in itemsData)
                {
                    if (xItemData.Contains(":"))
                    {
                        string[] xItemSplit = xItemData.Split(':');

                        if (xItemSplit == null || xItemSplit.Length < 2) continue;
                        ItemModel itemModel = ItemModelModule.Instance.Get(Convert.ToUInt32(xItemSplit[0]));
                        if (itemModel == null) continue;
                        if (!RequiredItems.ContainsKey(itemModel)) RequiredItems.Add(itemModel, Convert.ToInt32(xItemSplit[1]));
                    }
                }
            }
            else if(itemsString != "")
            {
                if (itemsString.Contains(":"))
                {
                    string[] xItemSplit = itemsString.Split(':');
                    if (xItemSplit != null && xItemSplit.Length > 1)
                    {
                        ItemModel itemModel = ItemModelModule.Instance.Get(Convert.ToUInt32(xItemSplit[0]));
                        if (itemModel != null)
                        {
                            if (!RequiredItems.ContainsKey(itemModel)) RequiredItems.Add(itemModel, Convert.ToInt32(xItemSplit[1]));
                        }
                    }
                }
            }
            new Npc(NpcPed, NpcPosition, NpcHeading, 0);
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
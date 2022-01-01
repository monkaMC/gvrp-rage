using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using GVRP.Module.Configurations;
using GVRP.Module.Logging;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Items
{
    public class ItemOrderModule
    {
        public static ItemOrderModule Instance = new ItemOrderModule();
        public static List<ItemOrder> ItemOrders;

        public void Load()
        {
            ItemOrders = new List<ItemOrder>();

            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText =
                    $"SELECT * FROM `itemorder_orders` ORDER BY id;";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            ItemOrder itemOrder = new ItemOrder(
                                reader.GetUInt32("id"),
                                reader.GetInt32("item_id"),
                                reader.GetInt32("item_amount"),
                                reader.GetInt32("owner_id"),
                                reader.GetInt32("hours"),
                                reader.GetInt32("npc_id"),
                                reader.GetDateTime("timestamp_ordered")
                            );

                            ItemOrders.Add(itemOrder);
                        }
                    }
                }
            }
        }

        public List<ItemOrder> GetFinishedOrders()
        {
            return ItemOrders.Where(itemOrder => itemOrder.DateTime.AddHours(itemOrder.Hours) <= DateTime.Now).ToList();
        }

        public List<ItemOrder> GetPlayerFinishedListByNpc(DbPlayer dbPlayer, ItemOrderNpc itemOrderNpc)
        {
            return GetFinishedOrders().Where(itemOrder => itemOrder.NpcId == itemOrderNpc.Id && itemOrder.OwnerId == dbPlayer.Id).ToList();
        }

        public int GetItemOrderCountByItem(DbPlayer dbPlayer, ItemOrderNpcItem itemOrderNpcItem)
        {
            return ItemOrders.Where(itemOrder => itemOrder.OwnerId == dbPlayer.Id && itemOrder.ItemId == itemOrderNpcItem.Id).Count();
        }

        public bool DeleteOrder(ItemOrder itemOrder)
        {
            if (ItemOrders.Contains(itemOrder))
            {
                var order = ItemOrders.First(kvp => kvp == itemOrder);
                ItemOrders.Remove(order);

                // Query Action
                MySQLHandler.ExecuteAsync($"DELETE FROM `itemorder_orders` WHERE `id` = '{itemOrder.Id}';");
                return true;
            }
            return false;
        }
        
        public ItemOrder AddDbOrder(int itemId, int itemAmount, int ownerId, int hours, int npcId)
        {
            MySQLHandler.Execute($"INSERT INTO `itemorder_orders` (`item_id`, `item_amount`, `owner_id`, `hours`, `npc_id`) VALUES " +
                $"('{itemId}', '{itemAmount}', '{ownerId}', '{hours}', '{npcId}');");


            var query = string.Format($"SELECT * FROM `itemorder_orders` " +
                $"WHERE `item_id` = '{itemId}' " +
                $"AND `item_amount` = '{itemAmount}' " +
                $"AND `owner_id` = '{ownerId}'" +
                $"AND `npc_id` = '{npcId}'" +
                $"AND `timestamp_ordered` BETWEEN timestamp(DATE_SUB(NOW(), INTERVAL 60 SECOND)) AND timestamp(NOW()) ORDER BY `id` DESC" +
                $";");

            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = @query;
                using (var reader = cmd.ExecuteReader())
                {

                    if (!reader.HasRows) return null;
                    while (reader.Read())
                    {
                        ItemOrder xOrder = new ItemOrder(
                                reader.GetUInt32("id"),
                                reader.GetInt32("item_id"),
                                reader.GetInt32("item_amount"),
                                reader.GetInt32("owner_id"),
                                reader.GetInt32("hours"),
                                reader.GetInt32("npc_id"),
                                reader.GetDateTime("timestamp_ordered")
                            );
                        if (xOrder != null) ItemOrders.Add(xOrder);
                        return xOrder;
                    }
                }
            }
            return null;
        }
    }
}

using GVRP.Handler;
using GVRP.Module.Items;

namespace GVRP.Module.BlackMarket
{
    public class BlackMarketItem
    {
        public int Id { get; set; }
        public ItemsModule Item { get; set; }
        public int ItemAmount { get; set; }
        public int PlayerId { get; set; }
        public int DirectBuy { get; set; }
        public int Bid { get; set; }
        public int BidPlayerId { get; set; }
        public int ExpandMin { get; set; }
    }

    public static class BlackMarketItemFunctions
    {
        public static void SaveBid(this BlackMarketItem blackMarketItem)
        {
            var query = string.Format(
                $"UPDATE `blackmarket` SET bid = '{blackMarketItem.Bid}', bid_player_id = '{blackMarketItem.BidPlayerId}' WHERE `id` = '{blackMarketItem.Id}';");
            MySQLHandler.ExecuteAsync(query);
        }

        public static bool IsActive(this BlackMarketItem blackMarketItem)
        {
            return blackMarketItem.ExpandMin >= 1;
        }
    }
}
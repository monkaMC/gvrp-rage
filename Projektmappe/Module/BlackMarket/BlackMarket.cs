using System.Collections.Generic;

namespace GVRP.Module.BlackMarket
{
    public sealed class BlackMarket
    {
        private readonly List<BlackMarketItem> blackMarketItems;

        public static int Count = 0;
        public static BlackMarket Instance { get; } = new BlackMarket();

        private BlackMarket()
        {
            blackMarketItems = new List<BlackMarketItem>();
        }

        public void LoadBlackMarket()
        {
            // ?
        }

        public void LoadItems()
        {
            Count = 1;
        }

        public void DecreaseItems()
        {
            foreach (var blackMarketItem in blackMarketItems)
            {
                if (blackMarketItem.IsActive())
                {
                    blackMarketItem.ExpandMin--;
                }
            }
        }
    }
}
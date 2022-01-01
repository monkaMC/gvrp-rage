using GVRP.Handler;
using GVRP.Module.Items;

namespace GVRP.Module.Armory
{
    public class ArmoryItem
    {
        public int ItemId { get; set; }
        public ItemModel Item { get; set; }
        public int RestrictedRang { get; set; }
        public int Packets { get; set; }

        public int Price { get; set; }
    }
}
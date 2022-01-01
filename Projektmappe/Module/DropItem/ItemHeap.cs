using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using GVRP.Module.Items;

namespace GVRP.Module.DropItem
{
    public class ItemHeap
    {
        public Container Container { get; set; }
        public DateTime CreateDateTime { get; set; }
        public ColShape ColShape { get; set; }
        public Marker Marker { get; set; }

        public ItemHeap()
        {

        }
    }
}

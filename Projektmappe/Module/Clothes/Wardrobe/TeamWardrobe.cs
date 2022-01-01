using GTANetworkAPI;
using System.Collections.Generic;

namespace GVRP.Module.Clothes.Wardrobe
{
    public class TeamWardrobe
    {
        public int Id;

        public HashSet<int> Teams { get; set; }

        public Vector3 Position;

        public ColShape Colshape;

        public float Range;
    }
}
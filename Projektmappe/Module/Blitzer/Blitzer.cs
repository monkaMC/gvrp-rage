using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace GVRP.Module.Blitzer
{
    public class Blitzer
    {
        public int Id { get; set; }
        public float Range { get; set; }
        public string Owner { get; set; }
        public int TeamId { get; set; }
        public ColShape Shape { get; set; }
        public int SpeedLimit { get; set; }

        public Blitzer(int id, float range, string owner, int teamId, int speedLimit, ColShape shape)
        {
            Id = id;
            Range = range;
            Owner = owner;
            TeamId = teamId;
            Shape = shape;
            SpeedLimit = speedLimit;
        }
    }
}

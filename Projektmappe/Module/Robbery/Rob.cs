using System;
using System.Collections.Generic;
using System.Linq;
using GVRP.Module.Chat;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Shops;
using GVRP.Module.Teams;

namespace GVRP.Module.Robbery
{
    public class Rob
    {
        public int Id { get; set; }
        public DbPlayer Player { get; set; }
        public int Interval { get; set; }
        public int CopInterval { get; set; }
        public int EndInterval { get; set; }
        public bool Disabled { get; set; }
        public RobType Type { get; set; }
    }

    public enum RobType
    {
        Shop,
        Juwelier
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Farming
{
    public class FarmingModule : Module<FarmingModule>
    {
        public static List<DbPlayer> FarmingList = new List<DbPlayer>();
        public Dictionary<FarmSpot, int> FarmAmount = new Dictionary<FarmSpot, int>();
        public Dictionary<FarmProcess, int> ProcessAmount = new Dictionary<FarmProcess, int>();


        protected override bool OnLoad()
        {
            FarmingList = new List<DbPlayer>();
            return base.OnLoad();
        }

        public override void OnFiveSecUpdate()
        {
            List<DbPlayer> RemovePlayer = new List<DbPlayer>();
            foreach(DbPlayer xPlayer in FarmingList.ToList())
            {
                if (xPlayer == null || !xPlayer.IsValid()) continue;
                if (!xPlayer.HasData("pressedEOnFarm")) continue;
                if (!FarmSpotModule.Instance.PlayerFarmSpot(xPlayer)) RemovePlayer.Add(xPlayer);
            }
            foreach (DbPlayer xPlayer in RemovePlayer.ToList())
            {
                if (xPlayer == null || !xPlayer.IsValid()) continue;
                if(FarmingModule.FarmingList.Contains(xPlayer)) FarmingModule.FarmingList.Remove(xPlayer);
            }
        }
    }
}

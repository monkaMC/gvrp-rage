using System;
using System.Threading.Tasks;
using GVRP.Module.Geschenk;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool Geschenk(DbPlayer dbPlayer)
        {
            GeschenkModule.Instance.GenerateRandomReward(dbPlayer);
            return false;
        }
    }
}

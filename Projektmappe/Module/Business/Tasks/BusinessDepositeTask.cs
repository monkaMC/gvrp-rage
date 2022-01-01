using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Tasks;

namespace GVRP.Module.Business.Tasks
{
    public class BusinessDepositeTask : SqlTask
    {
        private readonly Business business;
        private readonly DbPlayer dbPlayer;
        private readonly int amount;

        public BusinessDepositeTask(Business business, DbPlayer dbPlayer, int amount)
        {
            this.business = business;
            this.dbPlayer = dbPlayer;
            this.amount = amount;
        }

        public override string GetQuery()
        {
            return $"UPDATE `player` SET money = money - '{amount}' WHERE id = '{dbPlayer.Id}' AND money >= '{amount}';";
        }

        public override void OnFinished(int result)
        {
            if (result == 1)
            {
                if (!dbPlayer.TakeMoney(amount)) return;
                var businessResult =
                    ExecuteNonQuery($"UPDATE `business` SET money = money + '{amount}' WHERE id = '{business.Id}';");
                if (businessResult == 1)
                {
                    business.Money += amount;
                }
            }
        }
    }
}
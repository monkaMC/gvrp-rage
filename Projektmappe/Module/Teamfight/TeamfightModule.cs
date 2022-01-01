using System.Collections.Generic;
using System.Linq;
using GVRP.Module.Logging;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Teamfight
{
    public class TeamfightModule : SqlModule<TeamfightModule, Teamfight, uint>
    {
        public int KillPoints = 1;
        public int TeamfightEndPoints = 5;
        public float ConversionFactorWinner = 0.66f;
        public float ConversionFactorLoser = 0.2f;

        protected override string GetQuery()
        {
            return "SELECT * FROM `teamfight`;";
        }

        public bool IsInTeamfight(uint teamId)
        {
            var list = TeamfightModule.Instance.GetAll().Where(st => st.Value.Active == 1);

            foreach (var fight in list)
            {
                if (fight.Value.Team_a == teamId || fight.Value.Team_b == teamId) return true;
            }

            return false;
        }

        /*public override void OnPlayerFirstSpawn(DbPlayer dbPlayer)
        {
            if (dbPlayer.Team.IsInTeamfight() && dbPlayer.Teamfight == 0)
            {
                TeamfightFunctions.StorePlayerweaponsInsideTeamfightContainer(dbPlayer);
                dbPlayer.SetTeamfight();
            }
            else if (!dbPlayer.Team.IsInTeamfight() && dbPlayer.Teamfight == 1)
            {
                dbPlayer.RemoveWeapons();
                dbPlayer.SetTeamfight();
            }
        }*/

        public override void OnFiveMinuteUpdate()
        {
        }

        public Teamfight getOwnTeamFight(uint teamId)
        {
            return TeamfightModule.Instance.GetAll().Where(st => st.Value.Active == 1 && (st.Value.Team_a == teamId || st.Value.Team_b == teamId)).FirstOrDefault().Value;
        }

        public List<Teamfight> getActiveTeamfights()
        {
            return TeamfightModule.Instance.GetAll().Values.Where(st => st.Active == 1).ToList();
        }

        public void createTeamFight(Teamfight fight)
        {
            TeamfightModule.Instance.Insert(fight.Id, fight, "teamfight", "team_a", fight.Team_a, "team_b", fight.Team_b, "team_a_money", fight.Team_a_money, "team_b_money", fight.Team_b_money);
        }
    }
}

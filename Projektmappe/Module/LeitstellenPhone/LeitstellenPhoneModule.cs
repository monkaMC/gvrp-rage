using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GVRP.Module.Players.Db;

namespace GVRP.Module.LeitstellenPhone
{
    public class TeamLeitstellenObject
    {
        public uint TeamId { get; set; }
        public int Number { get; set; }
        public DbPlayer Acceptor { get; set; }
        public bool StaatsFrakOnly { get; set; }
    }

    public class LeitstellenPhoneModule : Module<LeitstellenPhoneModule>
    {
        public static Dictionary<int, TeamLeitstellenObject> TeamNumberPhones = new Dictionary<int, TeamLeitstellenObject>();

        protected override bool OnLoad()
        {
            TeamNumberPhones = new Dictionary<int, TeamLeitstellenObject>();

            RegisterNumber((uint)teams.TEAM_GOV, 910, true);
            RegisterNumber((uint)teams.TEAM_POLICE, 911, true);
            RegisterNumber((uint)teams.TEAM_FIB, 912, true);
            RegisterNumber((uint)teams.TEAM_ARMY, 913, true);
            RegisterNumber((uint)teams.TEAM_MEDIC, 914, true);
            RegisterNumber((uint)teams.TEAM_DPOS, 915, true);
            RegisterNumber((uint)teams.TEAM_SWAT, 999, true);
            RegisterNumber((uint)teams.TEAM_NEWS, 916, true);
            RegisterNumber((uint)teams.TEAM_DRIVINGSCHOOL, 917, true);
            RegisterNumber((uint)teams.TEAM_COUNTYPD, 920, true);
            RegisterNumber((uint)teams.TEAM_LSC, 926, false);
            return base.OnLoad();
        }

        public void RegisterNumber(uint teamId, int number, bool staatsfrakonly)
        {
            if(!TeamNumberPhones.ContainsKey(number))
            {
                TeamNumberPhones.Add(number, new TeamLeitstellenObject() {
                    TeamId = teamId,
                    Number = number,
                    Acceptor = null,
                    StaatsFrakOnly = staatsfrakonly
                });
            }
        }

        public bool hasLeitstelleFunction(uint teamid)
        {
            return TeamNumberPhones.ToList().Where(lt => lt.Value.TeamId == teamid).Count() > 0;
        }

        public TeamLeitstellenObject GetLeitstelle(uint teamid)
        {
            return TeamNumberPhones.Values.ToList().Where(lt => lt.TeamId == teamid).FirstOrDefault();
        }

        public TeamLeitstellenObject GetLeitstelleByNumber(int number)
        {
            if (!TeamNumberPhones.ContainsKey(number)) return null;
            return TeamNumberPhones[number];
        }

        public TeamLeitstellenObject GetByAcceptor(DbPlayer dbPlayer)
        {
            return TeamNumberPhones.Values.ToList().Where(lt => lt.Acceptor != null && lt.Acceptor == dbPlayer).FirstOrDefault();
        }
    }
}

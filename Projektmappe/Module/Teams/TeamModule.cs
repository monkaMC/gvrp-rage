using MySql.Data.MySqlClient;
using System;
using System.Linq;
using GVRP.Module.Items;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Teams.Shelter;
using GVRP.Module.Teams.Spawn;

namespace GVRP.Module.Teams
{
    public sealed class TeamModule : SqlModule<TeamModule, Team, uint>
    {
        public int DutyCops => TeamModule.Instance.Get(1).Members.Where(member => member.Value.Duty).Count() + // LSPD
                               TeamModule.Instance.Get(5).Members.Where(member => member.Value.Duty).Count() + // FIB
                               TeamModule.Instance.Get(23).Members.Where(member => member.Value.Duty).Count() + // IAA
                               TeamModule.Instance.Get(21).Members.Where(member => member.Value.Duty).Count(); // SWAT

        public override Type[] RequiredModules()
        {
            return new[] {typeof(TeamSpawnModule), typeof(ItemModelModule) };
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `team`;";
        }

        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {
            dbPlayer.LastUninvite = reader.GetDateTime("lastuninvite");
        }

        public override int GetOrder()
        {
            return 2;
        }
        
        protected override bool OnLoad()
        {
            var result = base.OnLoad();
            foreach (var teamSpawn in TeamSpawnModule.Instance.GetAll())
            {
                var team = this[teamSpawn.Value.TeamId];
                team?.TeamSpawns.Add(teamSpawn.Value.Index, teamSpawn.Value);
            }

            return result;
        }

        public bool IsWeaponTeamId(uint Id)
        {
            return Id == (int)teams.TEAM_HUSTLER || Id == (int)teams.TEAM_BRATWA || Id == (int)teams.TEAM_ICA;
        }
        public bool IsMethTeamId(uint Id)
        {
            return Id == (int)teams.TEAM_TRIADEN || Id == (int)teams.TEAM_YAKUZA || Id == (int)teams.TEAM_LCN;
        }
        public bool IsWeedTeamId(uint Id)
        {
            return Id == (int)teams.TEAM_BALLAS || Id == (int)teams.TEAM_LOST || Id == (int)teams.TEAM_VAGOS ||
                   Id == (int)teams.TEAM_GROVE || Id == (int)teams.TEAM_MIDNIGHT || Id == (int)teams.TEAM_MARABUNTA ||
                   Id == (int)teams.TEAM_REDNECKS || Id == (int)teams.TEAM_HOH;
        }

        public bool IsGangsterTeamId(uint Id)
        {
            return IsWeedTeamId(Id) || IsMethTeamId(Id) || IsWeaponTeamId(Id);
        }

        public override void OnPlayerConnected(DbPlayer dbPlayer)
        {
            dbPlayer.SetTeam(dbPlayer.TeamId);
        }

        public override void OnPlayerDisconnected(DbPlayer dbPlayer, string reason)
        {
            dbPlayer.Team.RemoveMember(dbPlayer);
        }

        public Team GetByName(string name)
        {
            foreach (var team in GetAll().Values)
            {
                if (string.Equals(team.Name, name, StringComparison.CurrentCultureIgnoreCase)
                    || string.Equals(team.ShortName, name, StringComparison.CurrentCultureIgnoreCase)
                    || team.Name.ToLower().Contains(name.ToLower())) return team;
            }
            
            return null;
        }

        public Team GetById(int id)
        {
            foreach (var team in GetAll().Values)
            {
                if (team.Id == id) return team;
            }

            return null;
        }

        public void SendChatMessage(string message, params uint[] ids)
        {
            foreach (var id in ids)
            {
                this[id].SendNotification(message);
            }
        }

        public void SendMessageToTeam(string message, teams teamId, int time = 5000, int requiredRang = 0)
        {
            Team team = Get((uint)teamId);

            team.SendNotification(message, time:time, rang: requiredRang);
            return;
        }

        public void SendMessageToNSA(string message)
        {
            foreach (var iPlayer in TeamModule.Instance.GetById((int)teams.TEAM_FIB).Members.Values.Where(p => p.IsNSA).ToList())
            {
                iPlayer.SendNewNotification(message, PlayerNotification.NotificationType.FRAKTION);
            }
            return;
        }

        public void SendChatMessageToDepartments(DbPlayer sourceDbPlayer, string message)
        {
            foreach (var team in GetAll())
            {
                if (!team.Value.IsCops()) continue;
                
                team.Value.SendNotification(sourceDbPlayer.Team.ShortName + " Rang " +
                        sourceDbPlayer.TeamRank + " | " + sourceDbPlayer.GetName() + ": " + message);
            }
        }

        public void SendChatMessageToDepartments(string message)
        {
            foreach (var team in GetAll())
            {
                if (!team.Value.IsCops()) continue;

                team.Value.SendNotification(message, 10000);
            }
        }
        
        public DbPlayer CheckIfDutyCopIsInRange(DbPlayer sourceDbPlayer, float range)
        {
            foreach (var team in GetAll())
            {
                if (!team.Value.IsCops()) continue;
                foreach (var dbPlayer in team.Value.Members.Values)
                {
                    if (dbPlayer.Duty && 
                        dbPlayer.Player.Position.DistanceTo(sourceDbPlayer.Player.Position) < range && 
                        dbPlayer.Player.Dimension == sourceDbPlayer.Player.Dimension &&
                        dbPlayer.DimensionType == sourceDbPlayer.DimensionType) return dbPlayer;
                }
            }
            return null;
        }

        public override void OnFifteenMinuteUpdate()
        {
           
        }
        public void SendChatMessageToDutyTeamMembers(DbPlayer sourceDbPlayer, string message)
        {
            foreach (var dbPlayer in sourceDbPlayer.Team.Members)
            {
                if (dbPlayer.Value.IsACop() && !dbPlayer.Value.Duty) continue;
                dbPlayer.Value.SendNewNotification( sourceDbPlayer.Team.ShortName + " Rang " +
                    sourceDbPlayer.TeamRank + " | " + sourceDbPlayer.GetName() + ": " + message);
            }
        }
    }

    public static class TeamPlayerExtension
    {
        public static void SaveLastUninvite(this DbPlayer xPlayer)
        {
            MySQLHandler.ExecuteAsync($"UPDATE player SET `lastuninvite` = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' WHERE `id` = '{xPlayer.Id}'");
        }

        public static void SaveLastBankRobbery(this DbPlayer dbPlayer)
        {
            MySQLHandler.ExecuteAsync($"UPDATE team SET `lastbankrob` = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' WHERE `id` = '{dbPlayer.Team.Id}'");
        }

        public static void SaveLastOutfitPreQuest(this DbPlayer dbPlayer)
        {
            MySQLHandler.ExecuteAsync($"UPDATE team SET `lastoutfitprequest` = '{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}' WHERE `id` = '{dbPlayer.Team.Id}'");
        }
    }
}
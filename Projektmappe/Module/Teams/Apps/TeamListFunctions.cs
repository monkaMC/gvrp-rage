using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using static GVRP.Module.Teams.Apps.TeamListApp;

namespace GVRP.Module.Teams.Apps
{
    public static class TeamListFunctions
    {
        public static List<TeamMember> GetTeamMembersForList(DbPlayer dbPlayer)
        {
            var members = new List<TeamMember>();
            foreach (var member in dbPlayer.Team.Members.ToList())
            {
                var currentDbPlayer = member.Value;
                if (currentDbPlayer == null || !currentDbPlayer.IsValid()) continue;

                if (currentDbPlayer.IsInAdminDuty() || currentDbPlayer.IsInGuideDuty()) continue;

                var currentTeamRankPermission = currentDbPlayer.TeamRankPermission;

                string l_Name = currentDbPlayer.GetName();

                if (!dbPlayer.Team.IsGangsters())
                {
                    if (currentDbPlayer.IsInDuty())
                        l_Name = currentDbPlayer.Player.Name + " (ID)";
                    else
                        l_Name = currentDbPlayer.Player.Name + " (NiD)";
                }

                members.Add(new TeamMember(currentDbPlayer.Id, l_Name,
                    currentDbPlayer.TeamRank, currentTeamRankPermission.Inventory, currentTeamRankPermission.Bank,
                    currentTeamRankPermission.Manage, currentDbPlayer.handy[0]));
            }
            members = members.OrderByDescending(t => t.Rank).ToList();

            if (dbPlayer.TeamId == (uint)teams.TEAM_COUNTYPD)
            {
                var tmp = new List<TeamMember>();

                foreach (var l_Member in TeamModule.Instance.GetById((int)teams.TEAM_POLICE).Members)
                {
                    var l_FIBPlayer = l_Member.Value;
                    if (l_FIBPlayer == null || !l_FIBPlayer.IsValid())
                        continue;

                    if (l_FIBPlayer.IsInAdminDuty()) continue;

                    var l_Permission = l_FIBPlayer.TeamRankPermission;

                    string l_Name = l_FIBPlayer.Player.Name;
                    if (l_FIBPlayer.IsInDuty())
                    {
                        l_Name = $"[LSPD] {l_FIBPlayer.Player.Name} (ID)";
                    }
                    else
                    {
                        l_Name = $"[LSPD] {l_FIBPlayer.Player.Name} (NiD)";
                    }

                    tmp.Add(new TeamMember(l_FIBPlayer.Id, l_Name,
                        l_FIBPlayer.TeamRank, l_Permission.Inventory, l_Permission.Bank,
                        l_Permission.Manage, l_FIBPlayer.handy[0]));

                }
                tmp = tmp.OrderByDescending(t => t.Rank).ToList();
                members = members.Concat(tmp).ToList();
                tmp.Clear();
            }

            if (dbPlayer.TeamId == (uint)teams.TEAM_POLICE)
            {
                var tmp = new List<TeamMember>();

                foreach (var l_Member in TeamModule.Instance.GetById((int)teams.TEAM_COUNTYPD).Members)
                {
                    var l_FIBPlayer = l_Member.Value;
                    if (l_FIBPlayer == null || !l_FIBPlayer.IsValid())
                        continue;

                    if (l_FIBPlayer.IsInAdminDuty()) continue;

                    var l_Permission = l_FIBPlayer.TeamRankPermission;

                    string l_Name = l_FIBPlayer.Player.Name;
                    if (l_FIBPlayer.IsInDuty())
                    {
                        l_Name = $"[LSSD] {l_FIBPlayer.Player.Name} (ID)";
                    }
                    else
                    {
                        l_Name = $"[LSSD] {l_FIBPlayer.Player.Name} (NiD)";
                    }

                    tmp.Add(new TeamMember(l_FIBPlayer.Id, l_Name,
                        l_FIBPlayer.TeamRank, l_Permission.Inventory, l_Permission.Bank,
                        l_Permission.Manage, l_FIBPlayer.handy[0]));

                }
                tmp = tmp.OrderByDescending(t => t.Rank).ToList();
                members = members.Concat(tmp).ToList();
                tmp.Clear();
            }

            if (dbPlayer.TeamId == (uint)teams.TEAM_FIB)
            {
                var tmp = new List<TeamMember>();

                foreach (var l_Member in TeamModule.Instance.GetById((int)teams.TEAM_GOV).Members)
                {
                    var l_FIBPlayer = l_Member.Value;
                    if (l_FIBPlayer == null || !l_FIBPlayer.IsValid())
                        continue;

                    if (l_FIBPlayer.IsInAdminDuty()) continue;

                    var l_Permission = l_FIBPlayer.TeamRankPermission;

                    string l_Name = l_FIBPlayer.Player.Name;
                    if (l_FIBPlayer.IsInDuty())
                    {
                        l_Name = $"[GOV] {l_FIBPlayer.Player.Name} (ID)";
                    }
                    else
                    {
                        l_Name = $"[GOV] {l_FIBPlayer.Player.Name} (NiD)";
                    }

                    tmp.Add(new TeamMember(l_FIBPlayer.Id, l_Name,
                        l_FIBPlayer.TeamRank, l_Permission.Inventory, l_Permission.Bank,
                        l_Permission.Manage, l_FIBPlayer.handy[0]));

                }
                tmp = tmp.OrderByDescending(t => t.Rank).ToList();
                members = members.Concat(tmp).ToList();
                tmp.Clear();

                if (dbPlayer.IsNSA)
                {
                    
                    foreach (var l_Member in TeamModule.Instance.GetById((int)teams.TEAM_ARMY).Members.ToList())
                    {
                        var l_FIBPlayer = l_Member.Value;
                        if (l_FIBPlayer == null || !l_FIBPlayer.IsValid())
                            continue;

                        if (l_FIBPlayer.IsInAdminDuty()) continue;

                        var l_Permission = l_FIBPlayer.TeamRankPermission;

                        string l_Name = l_FIBPlayer.Player.Name;
                        if (l_FIBPlayer.IsInDuty())
                        {
                            l_Name = $"[ARMY] {l_FIBPlayer.Player.Name} (ID)";
                        }
                        else
                        {
                            l_Name = $"[ARMY] {l_FIBPlayer.Player.Name} (NiD)";
                        }

                        tmp.Add(new TeamMember(l_FIBPlayer.Id, l_Name,
                            l_FIBPlayer.TeamRank, l_Permission.Inventory, l_Permission.Bank,
                            l_Permission.Manage, l_FIBPlayer.handy[0]));

                    }
                    tmp = tmp.OrderByDescending(t => t.Rank).ToList();
                    members = members.Concat(tmp).ToList();
                    tmp.Clear();

                    foreach (var l_Member in TeamModule.Instance.GetById((int)teams.TEAM_POLICE).Members.ToList())
                    {
                        var l_PDPlayer = l_Member.Value;
                        if (l_PDPlayer == null || !l_PDPlayer.IsValid())
                            continue;
                        if (l_PDPlayer.IsInAdminDuty()) continue;

                        var l_Permission = l_PDPlayer.TeamRankPermission;

                        string l_Name = l_PDPlayer.Player.Name;
                        if (l_PDPlayer.IsInDuty())
                        {
                            l_Name = $"[LSPD] {l_PDPlayer.Player.Name} (ID)";
                        }
                        else
                        {
                            l_Name = $"[LSPD] {l_PDPlayer.Player.Name} (NiD)";
                        }

                        tmp.Add(new TeamMember(l_PDPlayer.Id, l_Name,
                            l_PDPlayer.TeamRank, l_Permission.Inventory, l_Permission.Bank,
                            l_Permission.Manage, l_PDPlayer.handy[0]));
                    }
                    tmp = tmp.OrderByDescending(t => t.Rank).ToList();
                    members = members.Concat(tmp).ToList();


                    foreach (var l_Member in TeamModule.Instance.GetById((int)teams.TEAM_COUNTYPD).Members.ToList())
                    {
                        var l_PDPlayer = l_Member.Value;
                        if (l_PDPlayer == null || !l_PDPlayer.IsValid())
                            continue;
                        if (l_PDPlayer.IsInAdminDuty()) continue;

                        var l_Permission = l_PDPlayer.TeamRankPermission;

                        string l_Name = l_PDPlayer.Player.Name;
                        if (l_PDPlayer.IsInDuty())
                        {
                            l_Name = $"[LSSD] {l_PDPlayer.Player.Name} (ID)";
                        }
                        else
                        {
                            l_Name = $"[LSSD] {l_PDPlayer.Player.Name} (NiD)";
                        }

                        tmp.Add(new TeamMember(l_PDPlayer.Id, l_Name,
                            l_PDPlayer.TeamRank, l_Permission.Inventory, l_Permission.Bank,
                            l_Permission.Manage, l_PDPlayer.handy[0]));
                    }
                    tmp = tmp.OrderByDescending(t => t.Rank).ToList();
                    members = members.Concat(tmp).ToList();
                }
            }
            
            return members;
        }
    }
}

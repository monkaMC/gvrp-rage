using GVRP.Module.ClientUI.Apps;
using GTANetworkAPI;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Players;
using GVRP.Module.Players.Phone.Apps;
using GVRP.Module.Players.Windows;
using GVRP.Module.Teams.Permission;
using GVRP.Module.Swat;
using GVRP.Module.Forum;
using System;
using GVRP.Module.FIB;

namespace GVRP.Module.Teams.Apps
{
    public class TeamManageApp : SimpleApp
    {
        public TeamManageApp() : base("TeamManageApp")
        {
        }

        [RemoteEvent]
        public void editTeamMember(Client player, uint editPlayerId, uint rank, bool bank, bool inventory, bool manage)
        {

            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || dbPlayer.IsSwatDuty()) return;
            if (dbPlayer.TeamId == (uint) TeamList.Zivilist) return;
            var teamRankPermission = dbPlayer.TeamRankPermission;
            var editDbPlayer = Players.Players.Instance.GetByDbId(editPlayerId);
            if (editDbPlayer == null) return;
            if (editDbPlayer.TeamId != dbPlayer.TeamId) return;
            
            // Spezial SWAT
            if (dbPlayer.IsSwatDuty())
            {
                if (!dbPlayer.HasSwatLeaderRights()) return;
                if (!editDbPlayer.HasSwatRights())
                {
                    dbPlayer.SendNewNotification($"{editDbPlayer.GetName()} ist kein Mitglied des SWATS.");
                    return;
                }
                
                editDbPlayer.SetSwatRights(manage);
                if(manage) dbPlayer.SendNewNotification($"Managerechte von {editDbPlayer.GetName()} hinzugefügt.");
                else dbPlayer.SendNewNotification($"Managerechte von {editDbPlayer.GetName()} entfernt.");
                return;
            }
            if (teamRankPermission.Manage < 1 || editDbPlayer.TeamRankPermission.Manage >= teamRankPermission.Manage) return;

            //Validations
            //Kann keine Rechte geben die er selbst nicht hat
            //Kann nur befördern zu einem Rang unter seinem
            //Kann niemanden mit seinem Rang oder höher veraendern
            if (rank >= dbPlayer.TeamRank)
            {
                dbPlayer.SendNewNotification("Du kannst niemandem mit deinem oder einem höheren Rang veraendern!");
                dbPlayer.SendNewNotification("Du kannst nur bis zu einem Rang unter deinem befördern!");
                return;
            }
            if (rank > 11)
            {
                dbPlayer.SendNewNotification("Rang 12 kann nicht auf der Insel vergeben werden!");
                return;
            };
            if (bank && teamRankPermission.Bank == false)
            {
                dbPlayer.SendNewNotification("Du kannst keine Rechte vergeben, welche du nicht hast. (Bank)!");
                return;
            }
             if(manage && teamRankPermission.Manage != 2)
            {
                dbPlayer.SendNewNotification("Nur Rang 11 und 12 können Manage-Rechte vergeben!");
                return;
            };

            editDbPlayer.SynchronizeForum();

            editDbPlayer.SetTeamRankPermission(bank, manage ? (editDbPlayer.TeamRankPermission.Manage == 2 ? 2 : 1) : 0, inventory, editDbPlayer.TeamRankPermission.Title);
            editDbPlayer.TeamRank = rank;
            editDbPlayer.Save();
            ComponentManager.Get<TeamListApp>().SendTeamMembers(dbPlayer);
        }

        [RemoteEvent]
        public void kickMember(Client player, uint editPlayerId)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (dbPlayer.TeamId == (uint) TeamList.Zivilist) return;
            var teamRankPermission = dbPlayer.TeamRankPermission;
            if (teamRankPermission == null || teamRankPermission.Manage < 1) return;
            var editDbPlayer = Players.Players.Instance.GetByDbId(editPlayerId);
            if(editDbPlayer == null || !editDbPlayer.IsValid()) return;
            if (!editDbPlayer.IsValid() || editDbPlayer.TeamId != dbPlayer.TeamId ||
                editDbPlayer.TeamRankPermission.Manage >= teamRankPermission.Manage) return;

            uint oldTeam = editDbPlayer.TeamId;

            // Spezial SWAT
            if (dbPlayer.IsSwatDuty())
            {
                if (!editDbPlayer.HasSwatRights())
                {
                    dbPlayer.SendNewNotification($"{editDbPlayer.GetName()} ist kein Mitglied des SWATS.");
                    return;
                }
                
                if(editDbPlayer.IsSwatDuty())
                {
                    editDbPlayer.SetSwatDuty(false);
                }

                editDbPlayer.RemoveSwatRights();
                dbPlayer.SendNewNotification($"{editDbPlayer.GetName()} aus dem SWAT entfernt.");
                ComponentManager.Get<TeamListApp>().SendTeamMembers(dbPlayer);
                return;
            }

            if(editDbPlayer.Team.IsGangsters())
            {
                if (editDbPlayer.Team.IsInTeamfight())
                {
                    editDbPlayer.RemoveWeapons();
                    dbPlayer.SetTeamfight();
                }


                editDbPlayer.LastUninvite = DateTime.Now;
                editDbPlayer.SaveLastUninvite();
            }


            editDbPlayer.Team.RemoveMember(editDbPlayer);
            editDbPlayer.SetTeam((uint) TeamList.Zivilist);
            editDbPlayer.TeamRank = 0;
            editDbPlayer.fgehalt[0] = 0;
            editDbPlayer.Player.TriggerEvent("updateDuty", false);
            editDbPlayer.UpdateApps();
            ComponentManager.Get<TeamListApp>().SendTeamMembers(dbPlayer);

            editDbPlayer.SynchronizeForum();

            dbPlayer.Team.SendNotification($"{editDbPlayer.GetName()} wurde aus der Fraktion geworfen");
            dbPlayer.SendNewNotification($"Du hast {editDbPlayer.GetName()} aus der Fraktion geworfen", title:$"{dbPlayer.Team.Name}");
            editDbPlayer.SendNewNotification( $"Du wurdest von {dbPlayer.GetName()} aus der Fraktion geworfen", title:$"{dbPlayer.Team.Name}");
            

        }
        
        [RemoteEvent]
        public void addPlayer(Client player, string playerName)
        {
            Main.m_AsyncThread.AddToAsyncThread(new System.Threading.Tasks.Task(() =>
            {
                var dbPlayer = player.GetPlayer();
                if (dbPlayer == null) return;
                if (!dbPlayer.IsValid()) return;
                var teamRankPermission = dbPlayer.TeamRankPermission;
                if (teamRankPermission.Manage < 1) return;
                var editDbPlayer = Players.Players.Instance.FindPlayer(playerName);
                if (editDbPlayer == null || !editDbPlayer.IsValid()) return;

                // 7 Days
           //     if(editDbPlayer.LastUninvite.AddDays(7) > DateTime.Now)
           //     {
           //         dbPlayer.SendNewNotification($"Spieler kann erst nach einer Woche erneut einer Fraktion beitreten.");
           //         return;
//                }

                if (dbPlayer.Team.GetMemberCount() >= dbPlayer.Team.MaxMembers)
                {
                    dbPlayer.SendNewNotification($"Ihre Fraktion hat die maximale Anzahl an Mitglieder erreicht! ({dbPlayer.Team.MaxMembers} Mitglieder)");
                    return;
                }

                // Spezial SWAT
                if (dbPlayer.IsSwatDuty())
                {
                    if (editDbPlayer.HasSwatRights())
                    {
                        dbPlayer.SendNewNotification($"{playerName} ist bereits Mitglied des SWATS.");
                        return;
                    }

                    // Wenn kein FLeader dann...
                    if (dbPlayer.Swat == 2)
                    {
                        if (!editDbPlayer.IsACop())
                        {
                            dbPlayer.SendNewNotification($"{playerName} ist nicht zugelassen.");
                            return;
                        }

                        editDbPlayer.SetSwatRights(false);
                    }
                    else
                    {
                        if (editDbPlayer.TeamId != (uint)TeamList.Zivilist)
                        {
                            dbPlayer.SendNewNotification($"{playerName} ist bereits in einer Fraktion.");
                            return;
                        }

                        editDbPlayer.SetTeam((int)teams.TEAM_SWAT);
                        editDbPlayer.SetTeamRankPermission(true, 0, true, "");
                        editDbPlayer.Player.TriggerEvent("updateDuty", true);
                    }
                    dbPlayer.SendNewNotification($"{playerName} zum SWAT hinzugefügt.");
                    return;
                }

                if (editDbPlayer.IsUndercover()) return; // just disable for undercover for no message... (abused...)

                if (editDbPlayer.TeamId != (uint)TeamList.Zivilist)
                {
                    dbPlayer.SendNewNotification($"{playerName} ist bereits in einer Fraktion.");
                    return;
                }
                ComponentManager.Get<ConfirmationWindow>().Show()(editDbPlayer, new ConfirmationObject($"{dbPlayer.Team.Name}", $"Möchtest du die Einladung von {dbPlayer.GetName()} annehmen?", "addPlayerConfirmed", dbPlayer.GetName(), dbPlayer.Team.Name));
            }));
        }
    }
}
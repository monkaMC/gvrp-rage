using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.Computer.Apps.FahrzeugUebersichtApp;
using GVRP.Module.Configurations;
using GVRP.Module.Forum;
using GVRP.Module.Logging;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Swat;
using GVRP.Module.Teams;
using GVRP.Module.Teams.Permission;

namespace GVRP.Module.Computer.Apps.FraktionUebersichtApp.Apps
{
    public class FraktionEditApp : SimpleApp
    {
        public FraktionEditApp() : base("FraktionEditApp") { }


        [RemoteEvent]
        public void editFraktionMember(Client player, uint playerId, uint memberRank, int payday, string title)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || dbPlayer.IsSwatDuty()) return;
            if (dbPlayer.TeamId == (uint) TeamList.Zivilist) return;
            var teamRankPermission = dbPlayer.TeamRankPermission;
            var editDbPlayer = Players.Players.Instance.GetByDbId(playerId);
            if (teamRankPermission.Manage < 1) return;

            if(title.Length < 0 || title.Length > 50)
            {
                dbPlayer.SendNewNotification("Diese Beschreibung ist zu nicht zulässig!");
                return;
            }

            if (editDbPlayer != null)
            {
                if (dbPlayer.Id == editDbPlayer.Id && dbPlayer.TeamRank == 12)
                {
                    memberRank = dbPlayer.TeamRank;
                }
                else
                {
                    if (memberRank >= dbPlayer.TeamRank || editDbPlayer.TeamRank >= dbPlayer.TeamRank)
                    {
                        dbPlayer.SendNewNotification("Du kannst niemandem mit deinem oder einem höheren Rang veraendern!");
                        dbPlayer.SendNewNotification("Du kannst nur bis zu einem Rang unter deinem befördern!");
                        return;
                    }
                    if (memberRank > 11)
                    {
                        dbPlayer.SendNewNotification("Rang 12 kann nicht auf der Insel vergeben werden!");
                        return;
                    };
                }

                editDbPlayer.SynchronizeForum();

                if (dbPlayer.Team.HasDuty && dbPlayer.Team.Salary[(int)dbPlayer.TeamRank] > 0)
                {
                    dbPlayer.Team.SendNotification($"{dbPlayer.GetName()} hat {editDbPlayer.GetName()} auf Rang {memberRank} gesetzt.");
                    payday = 0;
                }
                else
                {
                    dbPlayer.Team.SendNotification($"{dbPlayer.GetName()} hat {editDbPlayer.GetName()} auf Rang {memberRank} gesetzt und den Payday auf {payday} $ angepasst.");
                    editDbPlayer.fgehalt[0] = payday;
                }

                editDbPlayer.TeamRankPermission.Title = title;
                editDbPlayer.TeamRankPermission.Save();

                editDbPlayer.TeamRank = memberRank;
                editDbPlayer.Save();

            }
            else
            {
                using (MySqlConnection conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    conn.Open();

                    cmd.CommandText = "SELECT player.id, player.name, player.rang, player.fgehalt, player_rights.* FROM player INNER JOIN player_rights ON player_rights.accountid = player.id WHERE player.id = @id ORDER BY rang DESC";
                    cmd.Parameters.AddWithValue("@id", playerId);
                    cmd.Prepare();

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Frakmember overview = new Frakmember
                            {
                                Id = reader.GetUInt32("id"),
                                Name = reader.GetString("name"),
                                Rang = reader.GetUInt32("rang"),
                                Payday = reader.GetInt32("fgehalt"),
                                Title = "",
                                Bank = reader.GetInt32("r_bank") == 1,
                                Manage = reader.GetInt32("r_manage") >= 1,
                                Storage = reader.GetInt32("r_inventory") == 1
                            };

                            if (memberRank >= dbPlayer.TeamRank || overview.Rang >= dbPlayer.TeamRank)
                            {
                                dbPlayer.SendNewNotification("Du kannst niemandem mit deinem oder einem höheren Rang veraendern!");
                                dbPlayer.SendNewNotification("Du kannst nur bis zu einem Rang unter deinem befördern!");
                                return;
                            }
                            if (memberRank > 11)
                            {
                                dbPlayer.SendNewNotification("Rang 12 kann nicht auf der Insel vergeben werden!");
                                return;
                            };

                            if (dbPlayer.Team.HasDuty && dbPlayer.Team.Salary[(int)dbPlayer.TeamRank] > 0)
                            {
                                MySQLHandler.ExecuteAsync($"UPDATE player SET rang = '{memberRank}' WHERE id = '{playerId}'");
                                MySQLHandler.ExecuteAsync($"UPDATE player_rights SET title = '{title}' WHERE accountid = '{playerId}'");
                                dbPlayer.Team.SendNotification($"{dbPlayer.GetName()} hat {overview.Name} auf Rang {memberRank} gesetzt.");
                            }
                            else
                            {
                                MySQLHandler.ExecuteAsync($"UPDATE player SET rang = '{memberRank}', fgehalt = '{payday}' WHERE id = '{playerId}'");
                                MySQLHandler.ExecuteAsync($"UPDATE player_rights SET title = '{title}' WHERE accountid = '{playerId}'");
                                dbPlayer.Team.SendNotification($"{dbPlayer.GetName()} hat {overview.Name} auf Rang {memberRank} gesetzt und den Payday auf {payday} $ angepasst.");
                            }

                        }
                    }
                    conn.Close();
                }
            }

        }

        [RemoteEvent]
        public void kickFraktionMember(Client player, uint playerId, int rang)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || dbPlayer.IsSwatDuty()) return;
            if (dbPlayer.TeamId == (uint)TeamList.Zivilist) return;
            var teamRankPermission = dbPlayer.TeamRankPermission;
            var editDbPlayer = Players.Players.Instance.GetByDbId(playerId);
            if (teamRankPermission.Manage < 1) return;

            if (editDbPlayer != null)
            {
                if (dbPlayer.Id == editDbPlayer.Id || rang == 12)
                {
                    dbPlayer.SendNewNotification("Du kannst niemandem mit deinem oder einem höheren Rang veraendern!");
                    return;
                }

                editDbPlayer.SynchronizeForum();
                
                dbPlayer.Team.SendNotification($"{dbPlayer.GetName()} hat {editDbPlayer.GetName()} aus der Fraktion entlassen.");

                if (editDbPlayer.Team.IsGangsters())
                {
                    if (editDbPlayer.Team.IsInTeamfight())
                    {
                        editDbPlayer.RemoveWeapons();
                    }


                    editDbPlayer.LastUninvite = DateTime.Now;
                    editDbPlayer.SaveLastUninvite();
                }


                editDbPlayer.Team.RemoveMember(editDbPlayer);
                editDbPlayer.SetTeam((uint)TeamList.Zivilist);
                editDbPlayer.TeamRank = 0;
                editDbPlayer.fgehalt[0] = 0;
                editDbPlayer.Player.TriggerEvent("updateDuty", false);
                editDbPlayer.UpdateApps();

                editDbPlayer.SynchronizeForum();
                editDbPlayer.Save();
            }
            else
            {
                using (MySqlConnection conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                using (MySqlCommand cmd = conn.CreateCommand())
                {
                    conn.Open();

                    cmd.CommandText = "SELECT player.id, player.name, player.rang, player.fgehalt, player_rights.* FROM player INNER JOIN player_rights ON player_rights.accountid = player.id WHERE player.id = @id ORDER BY rang DESC";
                    cmd.Parameters.AddWithValue("@id", playerId);
                    cmd.Prepare();

                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            Frakmember overview = new Frakmember
                            {
                                Id = reader.GetUInt32("id"),
                                Name = reader.GetString("name"),
                                Rang = reader.GetUInt32("rang"),
                                Payday = reader.GetInt32("fgehalt"),
                                Title = "",
                                Bank = reader.GetInt32("r_bank") == 1,
                                Manage = reader.GetInt32("r_manage") >= 1,
                                Storage = reader.GetInt32("r_inventory") == 1
                            };

                            if (dbPlayer.Id == overview.Id || overview.Rang == 12)
                            {
                                dbPlayer.SendNewNotification("Du kannst niemandem mit deinem oder einem höheren Rang veraendern!");
                                return;
                            }
                            
                            MySQLHandler.ExecuteAsync($"UPDATE player SET rang = '0', team = '0' WHERE id = '{playerId}'");
                            MySQLHandler.ExecuteAsync($"UPDATE player_rights SET title = '', r_bank = 0, r_inventory = 0, r_manage = 0 WHERE accountid = '{playerId}'");
                            dbPlayer.Team.SendNotification($"{dbPlayer.GetName()} hat {overview.Name} aus der Fraktion entlassen.");

                        }
                    }
                    conn.Close();
                }
            }

        }
    }
}
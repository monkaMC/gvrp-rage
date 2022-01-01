using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Module.Configurations;
using GVRP.Module.Logging;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Clothes.Team
{
    public class TeamSkinModule : Module<TeamSkinModule>
    {
        private Dictionary<uint, TeamSkin> teamSkins;

        private Dictionary<uint, TeamSkinCloth> teamClothes;

        private Dictionary<uint, TeamSkinProp> teamProps;

        protected override bool OnLoad()
        {
            teamSkins = new Dictionary<uint, TeamSkin>();
            teamClothes = new Dictionary<uint, TeamSkinCloth>();
            teamProps = new Dictionary<uint, TeamSkinProp>();
            LoadProps();
            LoadClothes();
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText =
                    "SELECT * FROM `team_skins`";
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows) return false;
                    while (reader.Read())
                    {
                        var teamSkin = new TeamSkin
                        {
                            Id = reader.GetUInt32(0),
                            Name = reader.GetString(1),
                            TeamId = reader.GetUInt32(3)
                        };
                        if (Enum.TryParse(reader.GetString(2), out PedHash hash))
                        {
                            teamSkin.Hash = hash;
                        }
                        else
                        {
                            continue;
                        }

                        teamSkin.Clothes = teamClothes.Values.Where(cloth => cloth.TeamSkinId == teamSkin.Id).ToList();
                        teamSkin.Props = teamProps.Values.Where(prop => prop.TeamSkinId == teamSkin.Id).ToList();
                        teamSkins.Add(teamSkin.Id, teamSkin);
                    }
                }
                conn.Close();
            }
            return true;
        }

        public void LoadClothes()
        {
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText =
                    "SELECT * FROM `team_skin_clothes`";
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows) return;
                    while (reader.Read())
                    {
                        var teamSkinCloth = new TeamSkinCloth
                        {
                            Id = reader.GetUInt32(0),
                            Name = reader.GetString(1),
                            Slot = reader.GetInt32(2),
                            Variation = reader.GetInt32(3),
                            Texture = reader.GetInt32(4),
                            TeamSkinId = reader.GetInt32(5)
                        };
                        teamClothes.Add(teamSkinCloth.Id, teamSkinCloth);
                    }
                }
                conn.Close();
            }
        }

        public void LoadProps()
        {
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText =
                    "SELECT * FROM `team_skin_props`";
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.HasRows) return;
                    while (reader.Read())
                    {
                        var teamSkinProp = new TeamSkinProp
                        {
                            Id = reader.GetUInt32(0),
                            Name = reader.GetString(1),
                            Slot = reader.GetInt32(2),
                            Variation = reader.GetInt32(3),
                            Texture = reader.GetInt32(4),
                            TeamSkinId = reader.GetInt32(5)
                        };
                        teamProps.Add(teamSkinProp.Id, teamSkinProp);
                    }
                }
                conn.Close();
            }
        }

        public TeamSkin GetTeamSkin(DbPlayer iPlayer)
        {
            return teamSkins.Values.FirstOrDefault(skin =>
                skin.Hash == iPlayer.Character.Skin && skin.TeamId == iPlayer.TeamId);
        }

        public TeamSkinCloth GetTeamCloth(uint id)
        {
            return teamClothes.ContainsKey(id) ? teamClothes[id] : null;
        }

        public TeamSkinProp GetTeamProp(uint id)
        {
            return teamProps.ContainsKey(id) ? teamProps[id] : null;
        }

        public List<TeamSkin> GetSkinsForTeam(uint teamId)
        {
            return teamSkins.Values.Where(skin => skin.TeamId == teamId).ToList();
        }
    }
}
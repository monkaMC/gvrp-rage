using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GVRP.Module.Computer.Apps.PoliceAktenSearchApp;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using static GVRP.Module.Computer.Apps.PoliceAktenSearchApp.PoliceAddAktenApp;
using static GVRP.Module.Computer.Apps.PoliceAktenSearchApp.PoliceOverviewApp;

namespace GVRP.Module.Crime.PoliceAkten
{
    public sealed class PoliceAktenModule : SqlModule<PoliceAktenModule, PoliceAkte, uint>
    {
        public Dictionary<uint, PoliceServerAkte> akten = new Dictionary<uint, PoliceServerAkte>();

        protected override string GetQuery()
        {
            return "SELECT * FROM `player_policeakten`;";
        }

        protected override void OnLoaded()
        {
            akten = new Dictionary<uint, PoliceServerAkte>();

            // Konvert into new
            foreach(PoliceAkte pAkte in Instance.GetAll().Values)
            {
                if(!akten.ContainsKey(pAkte.Id))
                {
                    akten.Add(pAkte.Id, new PoliceServerAkte(pAkte.Id, pAkte.PlayerId, pAkte.Title, pAkte.Text, pAkte.Created, pAkte.Closed, pAkte.Officer, pAkte.Open));
                }
            }
        }

        public void AddServerAkte(DbPlayer foundPlayer, ResponseAkteJson responseAkteJson)
        {
            uint newId = 1;
            if(akten.Count() > 0) newId = akten.Keys.Max()+1;

            akten.Add(newId, new PoliceServerAkte(newId, foundPlayer.Id, responseAkteJson.Title, responseAkteJson.Text, responseAkteJson.Created, responseAkteJson.Closed, responseAkteJson.Officer, responseAkteJson.Open));
            CreateAkteDB(akten[newId]);
        }

        public List<ResponseAkteJson> GetPlayerClientJsonAkten(DbPlayer dbPlayer)
        {
            List<ResponseAkteJson> responseAkteJsons = new List<ResponseAkteJson>();
            foreach(PoliceServerAkte akte in akten.Values.ToList().Where(a => a.PlayerId == dbPlayer.Id))
            {
                responseAkteJsons.Add(new ResponseAkteJson() { AktenId = akte.Id, Officer = akte.Officer, Closed = akte.Closed, Created = akte.Created, Open = akte.Open, Text = akte.Text, Title = akte.Title });
            }

            return responseAkteJsons;
        }

        public ResponseAkteJson GetOpenAkteOrNew(DbPlayer dbPlayer)
        {
            ResponseAkteJson responseAkteJson = new ResponseAkteJson();

            if(akten.Where(a => a.Value.Open && a.Value.PlayerId == dbPlayer.Id).Count() > 0)
            {
                PoliceServerAkte policeAkte = akten.Where(a => a.Value.Open && a.Value.PlayerId == dbPlayer.Id).First().Value;

                responseAkteJson.AktenId = policeAkte.Id;
                responseAkteJson.Created = policeAkte.Created;
                responseAkteJson.Closed = policeAkte.Closed;
                responseAkteJson.Title = policeAkte.Title;
                responseAkteJson.Text = policeAkte.Text;
                responseAkteJson.Officer = policeAkte.Officer;
                responseAkteJson.Open = policeAkte.Open;
            }
            else // No open found set data...
            {
                responseAkteJson.AktenId = 0;
                responseAkteJson.Closed = DateTime.Now;
                responseAkteJson.Created = DateTime.Now;
                responseAkteJson.Title = "";
                responseAkteJson.Text = "";
                responseAkteJson.Officer = "";
                responseAkteJson.Open = true;
            }

            return responseAkteJson;
        }

        public void SaveServerAkte(DbPlayer foundPlayer, ResponseAkteJson responseAkteJson)
        {
            if(akten.ContainsKey(responseAkteJson.AktenId))
            {
                // Gleiche Daten ab...
                PoliceServerAkte policeServerAkte = akten[responseAkteJson.AktenId];
                if (policeServerAkte == null) return;

                policeServerAkte.Officer = responseAkteJson.Officer;
                policeServerAkte.Open = responseAkteJson.Open;
                policeServerAkte.PlayerId = foundPlayer.Id;
                policeServerAkte.Text = responseAkteJson.Text;
                policeServerAkte.Title = responseAkteJson.Title;
                policeServerAkte.Created = responseAkteJson.Created;
                policeServerAkte.Closed = responseAkteJson.Closed;

                akten[responseAkteJson.AktenId] = policeServerAkte;

                SaveAkteDB(akten[responseAkteJson.AktenId]);
            }
        }


        public void DeleteServerAkte(uint id)
        {
            if (akten.ContainsKey(id))
            {
                akten.Remove(id);
                MySQLHandler.ExecuteAsync($"DELETE FROM player_policeakten WHERE id = '{id}'");
            }
        }

        // Save To DB
        private void SaveAkteDB(PoliceServerAkte policeServerAkte)
        {
            MySQLHandler.ExecuteAsync($"UPDATE player_policeakten SET officer = '{policeServerAkte.Officer}', title = '{policeServerAkte.Title}', text = '{policeServerAkte.Text}', created = '{policeServerAkte.Created.ToString("yyyy-MM-dd")}', closed = '{policeServerAkte.Closed.ToString("yyyy-MM-dd")}', open = '{(policeServerAkte.Open ? 1 : 0)}' WHERE id = '{policeServerAkte.Id}' AND player_id = '{policeServerAkte.PlayerId}'");
        }
        private void CreateAkteDB(PoliceServerAkte policeServerAkte)
        {
            MySQLHandler.ExecuteAsync($"INSERT INTO player_policeakten (`id`, `player_id`, `title`, `text`, `created`, `closed`, `officer`, `open`) VALUES" +
                $"('{policeServerAkte.Id}', '{policeServerAkte.PlayerId}', '{policeServerAkte.Title}', '{policeServerAkte.Text}', '{policeServerAkte.Created.ToString("yyyy-MM-dd")}', '{policeServerAkte.Closed.ToString("yyyy-MM-dd")}', '{policeServerAkte.Officer}', '{(policeServerAkte.Open ? 1 : 0)}')");
        }
    }

    public static class AktenPlayerExtension
    {
        public static bool CanAktenEdit(this DbPlayer dbPlayer)
        {
            if (!dbPlayer.IsInDuty()) return false;

            if (dbPlayer.Team.Id == (int)teams.TEAM_POLICE && dbPlayer.TeamRank > 6) return true;
            if (dbPlayer.Team.Id == (int)teams.TEAM_COUNTYPD && dbPlayer.TeamRank > 6) return true;
            if (dbPlayer.Team.Id == (int)teams.TEAM_FIB && dbPlayer.TeamRank > 6) return true;
            if (dbPlayer.Team.Id == (int)teams.TEAM_ARMY && dbPlayer.TeamRank > 6) return true;
            if (dbPlayer.Team.Id == (int)teams.TEAM_GOV && dbPlayer.TeamRank > 6) return true;

            return false;
        }

        public static bool CanEditData(this DbPlayer dbPlayer)
        {
            if (!dbPlayer.IsInDuty()) return false;

            if (dbPlayer.Team.Id == (int)teams.TEAM_POLICE && dbPlayer.TeamRank > 6) return true;
            if (dbPlayer.Team.Id == (int)teams.TEAM_COUNTYPD && dbPlayer.TeamRank > 6) return true;
            if (dbPlayer.Team.Id == (int)teams.TEAM_FIB && dbPlayer.TeamRank > 6) return true;
            if (dbPlayer.Team.Id == (int)teams.TEAM_ARMY && dbPlayer.TeamRank > 6) return true;
            if (dbPlayer.Team.Id == (int)teams.TEAM_GOV && dbPlayer.TeamRank > 6) return true;

            return false;
        }

        public static bool CanAktenView(this DbPlayer dbPlayer)
        {
            if (!dbPlayer.IsInDuty()) return false;

            if (dbPlayer.Team.Id == (int)teams.TEAM_POLICE && dbPlayer.TeamRank > 0) return true;
            if (dbPlayer.Team.Id == (int)teams.TEAM_COUNTYPD && dbPlayer.TeamRank > 0) return true;
            if (dbPlayer.Team.Id == (int)teams.TEAM_FIB && dbPlayer.TeamRank > 0) return true;
            if (dbPlayer.Team.Id == (int)teams.TEAM_ARMY && dbPlayer.TeamRank > 3) return true;
            if (dbPlayer.Team.Id == (int)teams.TEAM_GOV && dbPlayer.TeamRank > 3) return true;
            return false;
        }

        public static bool CanAktenCreate(this DbPlayer dbPlayer)
        {
            if (!dbPlayer.IsInDuty()) return false;

            if (dbPlayer.Team.Id == (int)teams.TEAM_POLICE && dbPlayer.TeamRank > 1) return true;
            if (dbPlayer.Team.Id == (int)teams.TEAM_COUNTYPD && dbPlayer.TeamRank > 1) return true;
            if (dbPlayer.Team.Id == (int)teams.TEAM_FIB && dbPlayer.TeamRank > 1) return true;
            if (dbPlayer.Team.Id == (int)teams.TEAM_ARMY && dbPlayer.TeamRank > 4) return true;
            if (dbPlayer.Team.Id == (int)teams.TEAM_GOV && dbPlayer.TeamRank > 4) return true;

            return false;
        }
        public static bool CanAktenDelete(this DbPlayer dbPlayer)
        {
            if (!dbPlayer.IsInDuty()) return false;

            if (dbPlayer.Team.Id == (int)teams.TEAM_POLICE && dbPlayer.TeamRank > 9) return true;
            if (dbPlayer.Team.Id == (int)teams.TEAM_COUNTYPD && dbPlayer.TeamRank > 9) return true;
            if (dbPlayer.Team.Id == (int)teams.TEAM_FIB && dbPlayer.TeamRank > 9) return true;
            if (dbPlayer.Team.Id == (int)teams.TEAM_ARMY && dbPlayer.TeamRank > 9) return true;
            if (dbPlayer.Team.Id == (int)teams.TEAM_GOV && dbPlayer.TeamRank > 9) return true;

            return false;
        }
    }
}

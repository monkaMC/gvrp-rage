using System;
using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using GVRP.Module.Logging;
using GVRP.Module.Players.Db;
using GVRP.Module.Items;
using GVRP.Module.Teams;
using GTANetworkAPI;
using GVRP.Module.Spawners;
using GVRP.Module.Players;
using GVRP.Module.Injury;
using GVRP.Module.FIB;

namespace GVRP.Module.Voice
{
    public enum FunkStatus
    {
        Deactive = 0,
        Hearing = 1,
        Active = 2
    }

    public class VoiceSettings
    {
        public double Room { get; set; }
        public int Active { get; set; }

        public VoiceSettings(double room, int active)
        {
            Room = room;
            Active = active;
        }
    }

    public sealed class VoiceModule : Module<VoiceModule>
    {

        public Dictionary<double, List<DbPlayer>> voiceFQ;
        public Dictionary<double, string> voiceFQDataStrings;

        public static int AdminFunkFrequenz = 9999;
        protected override bool OnLoad()
        {
            voiceFQ = new Dictionary<double, List<DbPlayer>>();
            voiceFQDataStrings = new Dictionary<double, string>();


            // Radio Bahamas
            ColShape polO = ColShapes.Create(new Vector3(-1384.67, -617.208, 30.8196), 9.0f);
            polO.SetData("triggerRadio", 1);

            // XMas Market
            ColShape polX = ColShapes.Create(new Vector3(869.644, -51.8499, 79.0112), 80.0f);
            polX.SetData("triggerXmas", 1);

            return true;
        }

        public override void OnPlayerDisconnected(DbPlayer dbPlayer, string reason)
        {
            RemoveFromVoice(dbPlayer);
        }

        public override void OnPlayerDeath(DbPlayer dbPlayer, NetHandle killer, uint weapon)
        {
            if(dbPlayer.isInjured()) 
                Instance.turnOffFunk(dbPlayer);
        }

        public override void OnPlayerFirstSpawn(DbPlayer dbPlayer)
        {
            InitPlayerVoice(dbPlayer);
            AddToVoice(dbPlayer);
        }

        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if (colShape.HasData("triggerRadio"))
            {
                if (colShapeState == ColShapeState.Enter)
                    SetRadio(dbPlayer, true);
                else
                    SetRadio(dbPlayer, false);
                return true;
            }

            if (colShape.HasData("triggerXmas"))
            {
                if (colShapeState == ColShapeState.Enter)
                    SetXMasRadio(dbPlayer, true);
                else
                    SetXMasRadio(dbPlayer, false);
                return true;
            }

            return false;
        }

        public void refreshFQVoiceForPlayerFrequenz(DbPlayer dbPlayer)
        {
            if (dbPlayer == null)
                return;

            if (hasPlayerRadio(dbPlayer))
            {
                double frequenz = getPlayerFrequenz(dbPlayer);
                refreshFQVoiceForFrequenz(frequenz);
            }
            else if (voiceFQDataStrings.ToList().Where(f => f.Value.Contains(dbPlayer.VoiceHash)).Count() > 0)
            {
                double frequenz = voiceFQDataStrings.FirstOrDefault(f => f.Value.Contains(dbPlayer.VoiceHash)).Key;
                refreshFQVoiceForFrequenz(frequenz);
            }
            else
            {
                dbPlayer.Player.TriggerEvent("setRadioChatPlayers", "");
            }
        }

        public void refreshFQVoiceForFrequenz(double frequenz)
        {
            actualizeFrequenzDataString(frequenz);
            foreach (DbPlayer xx in voiceFQ[frequenz].ToList())
            {
                if (hasPlayerRadio(xx) && getPlayerFrequenz(xx) == frequenz)
                {
                    xx.Player.TriggerEvent("setRadioChatPlayers", voiceFQDataStrings[frequenz]);
                }
            }
        }

        public void turnOffFunk(DbPlayer dbPlayer)
        {
            if (!hasPlayerRadio(dbPlayer)) return;

            dbPlayer.funkStatus = FunkStatus.Deactive;

            dbPlayer.Player.TriggerEvent("updateVoiceState", (int)dbPlayer.funkStatus);

            VoiceModule.Instance.refreshFQVoiceForPlayerFrequenz(dbPlayer);
        }

        private void actualizeFrequenzDataString(double frequenz)
        {
            if(frequenz < 1)
            {
                voiceFQDataStrings[frequenz] = "";
                return;
            }
            string s = "";
            foreach (DbPlayer xx in voiceFQ[frequenz].ToList().Where(p => p.funkStatus == FunkStatus.Active))
            {
                s += ";" + xx.VoiceHash + "~-6~0~0~4";
            }

            if (!voiceFQDataStrings.ContainsKey(frequenz)) voiceFQDataStrings.Add(frequenz, "");
            voiceFQDataStrings[frequenz] = s + ";";
        }

        private bool isFrequenzLocked(double frequenz)
        {
            foreach (DbTeam dbTeam in TeamModule.Instance.GetAll().Values.ToList())
            {
                if (dbTeam.Frequenzen.Contains(frequenz)) return true;
            }
            return false;
        }

        private bool isFrequenzLockedForTeam(DbTeam team, double frequenz)
        {
            return (!team.Frequenzen.Contains(frequenz) && isFrequenzLocked(frequenz));
        }

        private bool isFrequenzAvailableByCount(double frequenz)
        {
            if (!voiceFQ.ContainsKey(frequenz) || isFrequenzLocked(frequenz)) return true;
            if (frequenz == 0) return true;
            if (voiceFQ[frequenz].Count >= 10) return false;
            else return true;
        }

        private void InitPlayerVoice(DbPlayer iPlayer)
        {
            // Set Player Sync Voice Date
            VoiceListHandler.Instance.InitPlayerVoice(iPlayer);
            iPlayer.Player.TriggerEvent("setVoiceType", 1);
            iPlayer.Player.SetSharedData("voiceRange", 12);
            iPlayer.Player.SetSharedData("VOICE_RANGE_TYPE", 1);
            iPlayer.SetData("voiceType", 1);
        }

        private bool CheckSecretServicePerm(uint rank, double frequenz)
        {
            if (rank >= 9)
                return true;

            if (frequenz == 1000.5)
                return true;

            int l_Frequenz = (int)frequenz;

            if (l_Frequenz == 1001 || l_Frequenz == 1002 || l_Frequenz == 1003 || l_Frequenz == 1005 || l_Frequenz == 1006 || l_Frequenz == 1007 || l_Frequenz == 1008)
                return false;

            return true;
        }

        private bool CheckNSATerm(uint rank, double frequenz)
        {
            int l_Frequenz = (int)frequenz;

            if (l_Frequenz == 1000 || l_Frequenz == 1004 || l_Frequenz == 1005 || l_Frequenz == 1001 || l_Frequenz == 9000)
                return true;

            return false;
        }

        private void CheckFrequenz(double frequenz)
        {
            if (!voiceFQ.ContainsKey(frequenz)) voiceFQ.Add(frequenz, new List<DbPlayer>());
            if (!voiceFQDataStrings.ContainsKey(frequenz)) voiceFQDataStrings.Add(frequenz, "");
        }

        private void AddToVoice(DbPlayer dbPlayer)
        {
            if (hasPlayerRadio(dbPlayer))
            {
                double frequenz = getPlayerFrequenz(dbPlayer);
                CheckFrequenz(frequenz);
                voiceFQ[frequenz].Add(dbPlayer);
                actualizeFrequenzDataString(frequenz);
                refreshFQVoiceForPlayerFrequenz(dbPlayer);
            }
        }



        public void SetRadio(DbPlayer dbPlayer, bool on = true)
        {
            if (on) dbPlayer.Player.TriggerEvent("addVoiceParams", "radioon_club~0~0~0~2.0");
            else dbPlayer.Player.TriggerEvent("addVoiceParams", "");
        }

        public void SetXMasRadio(DbPlayer dbPlayer, bool on = true)
        {
            if (on) dbPlayer.Player.TriggerEvent("addVoiceParams", "xmas~0~0~0~2.0");
            else dbPlayer.Player.TriggerEvent("addVoiceParams", "");
        }

        public void RemoveFromVoice(DbPlayer dbPlayer)
        {
            // Remove From Radio Frequenz
            if(hasPlayerRadio(dbPlayer))
            {
                double frequenz = getPlayerFrequenz(dbPlayer);
                CheckFrequenz(frequenz);
                voiceFQ[frequenz].Remove(dbPlayer);
                refreshFQVoiceForFrequenz(frequenz);
            }

            // Remove from DeathVoice
            VoiceListHandler.RemoveFromDeath(dbPlayer);
        }

        public void ChangeFrequenz(DbPlayer dbPlayer, double frequenz, bool ignoreLocked = false)
        {
            if (hasPlayerRadio(dbPlayer))
            {
                CheckFrequenz(frequenz);

                if (isFrequenzLockedForTeam(dbPlayer.Team, frequenz) && !ignoreLocked)
                {
                    if (voiceFQ.ContainsKey(frequenz)) voiceFQ[frequenz].Remove(dbPlayer);
                    dbPlayer.SendNewNotification("Ihre Sicherheitsstufe reicht fuer diese Frequenz nicht aus!");
                    return;
                }

                if(!isFrequenzAvailableByCount(frequenz))
                {
                    dbPlayer.SendNewNotification("Diese Frequenz ist bereits voll!");
                    return;
                }

                if (dbPlayer.TeamId == (int)teams.TEAM_GOV)
                {
                    if (!CheckSecretServicePerm(dbPlayer.TeamRank, frequenz))
                    {
                        if (voiceFQ.ContainsKey(frequenz)) voiceFQ[frequenz].Remove(dbPlayer);
                        dbPlayer.SendNewNotification("Ihre Sicherheitsstufe reicht fuer diese Frequenz nicht aus!");
                        return;
                    }
                }

                if (dbPlayer.TeamId == (int)teams.TEAM_FIB)
                {
                    if (dbPlayer.IsNSA && !CheckNSATerm(dbPlayer.TeamRank, frequenz))
                    {
                        if (voiceFQ.ContainsKey(frequenz)) voiceFQ[frequenz].Remove(dbPlayer);
                        dbPlayer.SendNewNotification("Ihre Sicherheitsstufe reicht fuer diese Frequenz nicht aus!");
                        return;
                    }
                }

                if (dbPlayer.IsACop() && dbPlayer.IsInDuty() && !isFrequenzLocked(frequenz) && !dbPlayer.IsUndercover() && Convert.ToInt32(frequenz) != AdminFunkFrequenz)
                {
                    if (voiceFQ.ContainsKey(frequenz)) voiceFQ[frequenz].Remove(dbPlayer);
                    dbPlayer.SendNewNotification("Im Dienst können Sie keine oeffentlichen Kanäle nutzen!");
                    return;
                }

                if(Convert.ToInt32(frequenz) == AdminFunkFrequenz)
                {
                    if(!dbPlayer.Rank.Features.Contains("adminfunk"))
                    {
                        dbPlayer.SendNewNotification("Warum du auch immer hier rein willst, du darfst das aber nicht!");
                        return;
                    }
                }

                // Remove From Old Frequenz
                refreshFQVoiceForFrequenz(getPlayerFrequenz(dbPlayer));
                if(voiceFQ.ContainsKey(getPlayerFrequenz(dbPlayer))) voiceFQ[getPlayerFrequenz(dbPlayer)].Remove(dbPlayer);

                // Change Item to new
                dbPlayer.Container.EditFirstItemData(ItemModelModule.Instance.GetByType(ItemModelTypes.Radio), "Fq", frequenz);

                // Check new fq if exist
                CheckFrequenz(frequenz);

                // Add Playert to FQ and refresh
                if (!voiceFQ[frequenz].Contains(dbPlayer)) voiceFQ[frequenz].Add(dbPlayer);
                refreshFQVoiceForFrequenz(frequenz);
            }
        }

        public bool hasPlayerRadio(DbPlayer dbPlayer)
        {
            if(dbPlayer.Container.GetItemAmount(ItemModelModule.Instance.GetByType(ItemModelTypes.Radio)) > 0) return true;
            else
            {
                dbPlayer.Player.TriggerEvent("setRadioChatPlayers", "");
                dbPlayer.funkStatus = FunkStatus.Deactive;
                return false;
            }
        }

        public double getPlayerFrequenz(DbPlayer dbPlayer)
        {
            double fq = 0.0;

            if(hasPlayerRadio(dbPlayer))
            {
                try
                {
                    // Get Frequenz from Funkgerat Item
                    Item item = dbPlayer.Container.GetItemById((int)ItemModelModule.Instance.GetByType(ItemModelTypes.Radio).Id);
                    if (item.Data == null)
                    {
                        item.Data = new Dictionary<string, dynamic>();
                    }
                    if (!item.Data.ContainsKey("Fq"))
                    {
                        item.Data.TryAdd("Fq", 0.0);
                    }
                    fq = (double)item.Data["Fq"];
                }
                catch (Exception e)
                {
                    Logger.Crash(e);
                }
            }

            return fq;
        }

        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {
            dbPlayer.funkStatus = FunkStatus.Deactive;
        }
    }
}
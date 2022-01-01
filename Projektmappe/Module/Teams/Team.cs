using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Module.Items;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Teamfight;
using GVRP.Module.Teams.Blacklist;
using GVRP.Module.Teams.Spawn;

namespace GVRP.Module.Teams
{
    public class Team : DbTeam
    {
        public Dictionary<uint, DbPlayer> Members { get; }
        public Dictionary<uint, TeamSpawn> TeamSpawns { get; }
        public Teamfight.Teamfight RequestedTeamfight = null;

        public List<BlacklistEntry> blacklistEntries { get; set; }

        public Team(MySqlDataReader reader) : base(reader)
        {
            Members = new Dictionary<uint, DbPlayer>();
            TeamSpawns = new Dictionary<uint, TeamSpawn>();
            blacklistEntries = new List<BlacklistEntry>();

            this.LoadBlacklistEntries();
        }

        public bool IsNearSpawn(Vector3 Position, float Distance = 60.0f)
        {
            return TeamSpawns.Where(ts => ts.Value.Position.DistanceTo(Position) < Distance).Count() > 0;
        }

        public bool IsInRobbery()
        {
            if (Robbery.StaatsbankRobberyModule.Instance.RobberTeam != null && Robbery.StaatsbankRobberyModule.Instance.RobberTeam.Id == Id) return true;
            if (Robbery.WeaponFactoryRobberyModule.Instance.RobberTeam != null && Robbery.WeaponFactoryRobberyModule.Instance.RobberTeam.Id == Id) return true;
            if (Robbery.RobberyModule.Instance.Robberies.Where(r => r.Value.Player != null && r.Value.Player.IsValid() && r.Value.Player.Team.Id == Id).Count() > 0) return true;
            return false;
        }

        public bool IsInTeamfight()
        {
            if (TeamfightModule.Instance.IsInTeamfight(Id)) return true;

            return false;
        }

        public async void SendChatMessage(string message, int rang = 0)
        {

            if (Id == 0) return;
            foreach (var iPlayer in Members.Values.Where(m => m.TeamRank >= rang).ToList())
            {
                iPlayer.SendNewNotification(message);
            }

        }

        public async void SendNotification(string message, int time = 5000, int rang = 0)
        {
            String colorCode = Utils.HexConverter(this.RgbColor);
            if (Id == 0) return;
            foreach (var iPlayer in Members.Values.Where(m => m.TeamRank >= rang).ToList())
            {
                iPlayer.SendNewNotification(message, title: this.Name, notificationType: PlayerNotification.NotificationType.FRAKTION, duration: time);
            }
        }

        public async void SendNotification(string title, string message, int rang = 0)
        {
            if (Id == 0) return;
            foreach (var iPlayer in Members.Values.Where(m => m.TeamRank >= rang).ToList())
            {
                iPlayer.SendNewNotification(message);
            }
        }

        public void AddMember(DbPlayer iPlayer)
        {
            Members[iPlayer.Id] = iPlayer;
        }

        public void RemoveMember(DbPlayer iPlayer)
        {
            Members.Remove(iPlayer.Id);
        }

        public bool IsMember(DbPlayer iPlayer)
        {
            return Members.ContainsKey(iPlayer.Id);
        }

        public bool CanWeaponEquipedForTeam(WeaponHash weaponHash)
        {
            /*
            if(Id == (int)teams.TEAM_CIVILIAN)
            {
                if (weaponHash == WeaponHash.AssaultRifle || weaponHash == WeaponHash.Gusenberg || weaponHash == WeaponHash.CarbineRifle || 
                    weaponHash == WeaponHash.BullpupRifle || weaponHash == WeaponHash.AdvancedRifle || weaponHash == WeaponHash.AssaultSMG) return false;
            }*/

            return true;
        }
    }
}
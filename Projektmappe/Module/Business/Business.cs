using System.Collections.Generic;
using System.Linq;
using MySql.Data.MySqlClient;
using GVRP.Module.Business.Tasks;
using GVRP.Module.Chat;
using GVRP.Module.Configurations;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Tasks;

namespace GVRP.Module.Business
{
    public class Business : Loadable<uint>
    {
        public uint Id { get; }
        public string Name { get; set; }
        public int Money { get; set; }

        public Dictionary<uint, Member> Members;

        public bool Locked { get; set; }
        public Dictionary<uint, string> VehicleKeys { get; }
        public List<uint> StorageKeys { get; }
        public HashSet<DbPlayer> Visitors { get; }
        public BusinessBranch BusinessBranch { get; set; }

        public List<Banks.BankHistory.BankHistory> BankHistory { get; set; }
        
        public string MessageOfTheDay { get; set; }

        public Business(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
            Money = reader.GetInt32("money");
            MessageOfTheDay = reader.GetString("motd");
            Locked = true;
            Visitors = new HashSet<DbPlayer>();
            Members = new Dictionary<uint, Member>();
            VehicleKeys = new Dictionary<uint, string>();
            StorageKeys = new List<uint>();
            BankHistory = new List<Banks.BankHistory.BankHistory>();
        }
        
        public override uint GetIdentifier()
        {
            return Id;
        }
        
        public class Member
        {
            public uint BusinessId { get; set; }
            public uint PlayerId { get; set; }
            public bool Manage { get; set; }
            public bool Money { get; set; }
            public bool Inventory { get; set; }
            public int Salary { get; set; }
            public bool Owner { get; set; }
            public bool Raffinery { get; set; }
            public bool Fuelstation { get; set; }
            public bool NightClub { get; set; }
            public bool Tattoo { get; set; }
                       
            public Member()
            {

            }

            public Member(MySqlDataReader reader)
            {
                BusinessId = reader.GetUInt32("business_id");
                PlayerId = reader.GetUInt32("player_id");
                Manage = reader.GetInt32("manage") == 1;
                Money = reader.GetInt32("money") == 1;
                Inventory = reader.GetInt32("inventory") == 1;
                Salary = reader.GetInt32("gehalt");
                Owner = reader.GetInt32("owner") == 1;
                Fuelstation = reader.GetInt32("fuelstation") == 1;
                Raffinery = reader.GetInt32("raffinery") == 1;
                NightClub = reader.GetInt32("nightclub") == 1;
                Tattoo = reader.GetInt32("tattoo") == 1;
            }
        }

        public void AddMember(Member member)
        {
            if (Members.ContainsKey(member.PlayerId)) return;
            Members.Add(member.PlayerId, member);
        }

        public void RemoveMember(uint playerId)
        {
            if (Members.ContainsKey(playerId))
            {
                Members.Remove(playerId);
            }
        }

        public Member GetMember(uint playerId)
        {
            return Members.ContainsKey(playerId) ? Members[playerId] : null;
        }

        public Dictionary<uint, Member> GetMembers()
        {
            return Members;
        }

        // Load Members List for Startup etc...
        public void LoadBusinessMemberList()
        {
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"SELECT * FROM `business_members` WHERE `business_id` = '{Id}';";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            Members.Add(reader.GetUInt32("player_id"), new Member(reader));
                        }
                    }
                }
                conn.Close();
            }
        }

        // Load Business Bank History
        public void LoadBankHistory()
        {
            BankHistory = new List<Banks.BankHistory.BankHistory>();

            // Load Player Bank
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText =
                    $"SELECT * FROM `business_bankhistory` WHERE business_id = '{Id}' ORDER BY date DESC LIMIT 10;";
                using (var reader2 = cmd.ExecuteReader())
                {
                    if (reader2.HasRows)
                    {
                        while (reader2.Read())
                        {
                            var bankHistory = new Banks.BankHistory.BankHistory
                            {
                                PlayerId = reader2.GetUInt32(1),
                                Name = reader2.GetString(2),
                                Value = reader2.GetInt32(3),
                                Date = reader2.GetDateTime(4)
                            };

                            BankHistory.Add(bankHistory);
                        }
                    }
                }
            }
        }

        public void Deposite(DbPlayer dbPlayer, int amount)
        {
            SynchronizedTaskManager.Instance.Add(
                new BusinessDepositeTask(this, dbPlayer, amount));
            dbPlayer.SendNewNotification("Sie haben " + amount + "$ in den Tresor gelegt.", title: "Tresor", notificationType: PlayerNotification.NotificationType.ERROR);
        }

        public void Disburse(DbPlayer dbPlayer, int amount)
        {
            SynchronizedTaskManager.Instance.Add(
                new BusinessDisburseTask(this, dbPlayer, amount));
            dbPlayer.SendNewNotification("Sie haben " + amount + "$ aus den Tresor genommen.", title: "Tresor", notificationType: PlayerNotification.NotificationType.SUCCESS);
        }

        public void GiveMoney(int amount)
        {
            Money += amount;
            MySQLHandler.ExecuteAsync($"UPDATE `business` SET money = money + '{amount}' WHERE id = '{Id}'");
        }

        public bool TakeMoney(int amount)
        {
            if (Money < amount) return false;
            Money -= amount;
            MySQLHandler.ExecuteAsync(
                $"UPDATE `business` SET money = money - '{amount}' WHERE id = '{Id}' AND money >= '{amount}'");
            return true;
        }

        public bool IsMember(DbPlayer iPlayer)
        {
            return iPlayer.BusinessMembership.BusinessId == Id;
        }

        public void SendMessageToMembers(string message)
        {
            foreach (var member in GetMembers())
            {
                var player = Players.Players.Instance.GetByDbId(member.Key);
                if (player != null && player.IsValid())
                    player.SendNewNotification(Chats.MsgBusiness + message);
            }
        }

        public void SaveName()
        {
            MySQLHandler.ExecuteAsync(
                $"UPDATE `business` SET name = '{MySqlHelper.EscapeString(Name)}' WHERE id = '{Id}'");
        }
        
        public void ChangeMotd(string motd)
        {
            MessageOfTheDay = motd;
            MySQLHandler.ExecuteAsync(
                $"UPDATE `business` SET motd = '{MySqlHelper.EscapeString(MessageOfTheDay)}' WHERE id = '{Id}'");
        }
    }
}
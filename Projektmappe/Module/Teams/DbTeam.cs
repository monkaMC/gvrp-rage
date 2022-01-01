using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using GVRP.Module.Configurations;
using GVRP.Module.Items;
using GVRP.Module.Teams.MetaData;

namespace GVRP.Module.Teams
{
    public class DbTeam : Loadable<uint>
    {
        public uint Id { get; }
        public string Name { get; }
        public string ShortName { get; }
        public int ColorId { get; }
        public bool HasDuty { get; }
        public HashSet<double>Frequenzen { get; }
        public Dictionary<int, int> Salary { get; }
        public int BlipColor { get; }
        public Color RgbColor { get; }

        public bool IsBusinessTeam { get; set; }

        public int MaxMembers { get; }
        public int MedicSlots { get; set; }
        public int MedicSlotsUsed { get; set; }
        public DateTime LastBankRobbery { get; set; }
        public DateTime LastOutfitPreQuest { get; set; }

        public Container MineAluContainer { get; set; }
        public Container MineBronceContainer { get; set; }
        public Container MineIronContainer { get; set; }
        public Container MineZinkContainer { get; set; }
        public Container MineContainerSchmelze { get; set; }
        public Container MineContainerSchmelzCoal { get; set; }
        public Container MineContainerStorage { get; set; }

        public bool IsGangster { get;set; }

        public TeamMetaData TeamMetaData { get; set; }
        
        public List<Banks.BankHistory.BankHistory> BankHistory { get; set; }
        protected DbTeam(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
            ShortName = reader.GetString("name_short");
            ColorId = reader.GetInt32("color");
            HasDuty = reader.GetInt32("hasDuty") == 1;
            BlipColor = reader.GetInt32("blip_color");
            MedicSlots = reader.GetInt32("medicslots");
            MedicSlotsUsed = reader.GetInt32("medicslotsused");
            var temp = reader.GetString("rgb").Split(",");
            MaxMembers = reader.GetInt32("max_slots");
            RgbColor = new Color(Int32.Parse(temp[0]), Int32.Parse(temp[1]), Int32.Parse(temp[2]));
            
            IsGangster = reader.GetInt32("isGangster") == 1;

            IsBusinessTeam = reader.GetInt32("isBusinessTeam") == 1;
            LastBankRobbery = reader.GetDateTime("lastbankrob");
            LastOutfitPreQuest = reader.GetDateTime("lastoutfitprequest");

            BankHistory = new List<Banks.BankHistory.BankHistory>();

            TeamMetaData = new TeamMetaData(Id);

            if (IsBusinessTeam && Id == (uint)teams.TEAM_MINE1 || Id == (uint)teams.TEAM_MINE2)
            {
                MineAluContainer = ContainerManager.LoadContainer(Id, ContainerTypes.MINECONTAINERALU);
                MineBronceContainer = ContainerManager.LoadContainer(Id, ContainerTypes.MINECONTAINERBRONCE);
                MineIronContainer = ContainerManager.LoadContainer(Id, ContainerTypes.MINECONTAINERIRON);
                MineZinkContainer = ContainerManager.LoadContainer(Id, ContainerTypes.MINECONTAINERZINK);
                MineContainerSchmelze = ContainerManager.LoadContainer(Id, ContainerTypes.MINECONTAINERSCHMELZE);
                MineContainerSchmelzCoal = ContainerManager.LoadContainer(Id, ContainerTypes.MINECONTAINERSCHMELZCOAL);
                MineContainerStorage = ContainerManager.LoadContainer(Id, ContainerTypes.MINEBASESTORAGE);
            }

            var frequenzString = reader.GetString("frequenzen");
            Frequenzen = new HashSet<double>();
            if (!string.IsNullOrEmpty(frequenzString))
            {
                var splittedFrequenzen = frequenzString.Split(',');
                foreach (var frequenz in splittedFrequenzen)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        double fqx = (double)i / (double)10;
                        Frequenzen.Add(Convert.ToDouble(frequenz) + fqx);
                    }
                }
            }

            Salary = new Dictionary<int, int>();
            Salary.Add(0, reader.GetInt32("salary_0"));
            Salary.Add(1, reader.GetInt32("salary_1"));
            Salary.Add(2, reader.GetInt32("salary_2"));
            Salary.Add(3, reader.GetInt32("salary_3"));
            Salary.Add(4, reader.GetInt32("salary_4"));
            Salary.Add(5, reader.GetInt32("salary_5"));
            Salary.Add(6, reader.GetInt32("salary_6"));
            Salary.Add(7, reader.GetInt32("salary_7"));
            Salary.Add(8, reader.GetInt32("salary_8"));
            Salary.Add(9, reader.GetInt32("salary_9"));
            Salary.Add(10, reader.GetInt32("salary_10"));
            Salary.Add(11, reader.GetInt32("salary_11"));
            Salary.Add(12, reader.GetInt32("salary_12"));
            Salary.Add(13, reader.GetInt32("salary_12"));
            Salary.Add(14, reader.GetInt32("salary_12"));

            // Load Bankhistory
            LoadBankHistory();
        }

        public override uint GetIdentifier()
        {
            return Id;
        }

        public bool IsGangsters()
        {
            return IsMethTeam() || IsWeedTeam() || IsWeaponTeam();
        }

        public bool IsWeaponTeam()
        {
            return TeamModule.Instance.IsWeaponTeamId(Id);
        }
        
        public bool IsMethTeam()
        {
            return TeamModule.Instance.IsMethTeamId(Id);
        }

        public bool IsWeedTeam()
        {
            return TeamModule.Instance.IsWeedTeamId(Id);
        } 

        public void LoadBankHistory()
        {
            BankHistory = new List<Banks.BankHistory.BankHistory>();

            // Load Player Bank
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText =
                    $"SELECT * FROM `team_bankhistory` WHERE team_id = '{Id}' ORDER BY date DESC LIMIT 10;";
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

        public bool IsCops()
        {
            return Id == (int) teams.TEAM_POLICE || Id == (int)teams.TEAM_COUNTYPD || Id == (int) teams.TEAM_FIB ||
                   Id == (int)teams.TEAM_GOV || Id == (int) teams.TEAM_ARMY || Id == (int) teams.TEAM_SWAT;
        }

        public bool GetsExtraNightPayday()
        {
            return Id == (int)teams.TEAM_POLICE || Id == (int)teams.TEAM_COUNTYPD || Id == (int)teams.TEAM_FIB ||
                   Id == (int)teams.TEAM_ARMY || Id == (int)teams.TEAM_MEDIC;
        }

        public bool IsStaatsfraktion()
        {
            return Id == (int)teams.TEAM_POLICE || Id == (int)teams.TEAM_COUNTYPD || Id == (int)teams.TEAM_FIB ||
                   Id == (int)teams.TEAM_ARMY || Id == (int)teams.TEAM_SWAT || 
                   Id == (int)teams.TEAM_MEDIC || Id == (int)teams.TEAM_GOV || 
                   Id == (int)teams.TEAM_DPOS || Id == (int)teams.TEAM_NEWS;
        }

        public bool IsDpos()
        {
            return Id == (int)teams.TEAM_DPOS;
        }

        public bool CanRegisterVehicles()
        {
            return Id == (int) teams.TEAM_DPOS || Id == (int) teams.TEAM_DRIVINGSCHOOL;
        }
        
        public bool IsMedics()
        {
            return Id == (int) teams.TEAM_MEDIC;
        }

        public override string ToString()
        {
            return $"Team:{Id}, {Name}, {ShortName}, {ColorId}";
        }

        public void UpdateMedicSlots(int newSlots)
        {
            MySQLHandler.ExecuteAsync($"UPDATE team SET medicslotsused = '{newSlots}' WHERE id = '{Id}'");
        }

        public int GetMemberCount()
        {
            int slots = 0;

            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"SELECT COUNT(*) as Anzahl FROM player WHERE team = '{Id}'";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            slots = reader.GetInt32("Anzahl");
                            break;
                        }
                    }
                }
            }

            return slots;
        }
    }
}
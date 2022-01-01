using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using GVRP.Module.Items;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Spawners;

namespace GVRP.Module.Teams.Shelter
{
    public class TeamShelter : Loadable<uint>
    {
        public uint Id { get; }
        public Team Team { get; }
        public int Money { get; set; }
        public Container Container { get; set; }
        public Vector3 MenuPosition { get; }
        public ColShape MenuColShape { get; }
        public Vector3 InventarPosition { get; }
        public uint Dimension { get; }
        public Vector3 DealerPosition { get; set; } = null;
        public Vector3 LoboratoryPosition { get; set; } = null;

        public TeamShelter(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Team = TeamModule.Instance.Get(reader.GetUInt32("teamid"));
            Money = reader.GetInt32("money");
            Container = ContainerManager.LoadContainer(Id, ContainerTypes.SHELTER, 0);
            
            Dimension = reader.GetUInt32("interior_dimension");
            InventarPosition = new Vector3(reader.GetFloat("inventar_pos_x"), reader.GetFloat("inventar_pos_y"), reader.GetFloat("inventar_pos_z"));

            MenuPosition = new Vector3(reader.GetFloat("menu_pos_x"), reader.GetFloat("menu_pos_y"), reader.GetFloat("menu_pos_z"));
            MenuColShape = ColShapes.Create(MenuPosition, 3f, Dimension);
            MenuColShape.SetData("teamShelterMenuId", reader.GetUInt32("teamid"));
            Console.WriteLine(reader.GetFloat("menu_pos_x") + " " +  reader.GetFloat("menu_pos_y") + " " + reader.GetFloat("menu_pos_z") + " " + reader.GetFloat("teamid"));
        }

        public override uint GetIdentifier()
        {
            return Team.Id;
        }


        public void Deposit(DbPlayer p_DbPlayer, int p_Amount)
        {
            p_DbPlayer.TakeMoney(p_Amount);

            p_DbPlayer.Team.AddBankHistory(p_Amount, $"Einzahlung von {p_DbPlayer.GetName()}");

            Money += p_Amount;
            MySQLHandler.ExecuteAsync($"UPDATE `team_shelter` SET money = money + '{p_Amount}' WHERE id = '{Id}';");
        }

        public void TakeMoney(int p_Amount)
        {
            Money -= p_Amount;
            MySQLHandler.ExecuteAsync($"UPDATE `team_shelter` SET money = money - '{p_Amount}' WHERE id = '{Id}';");
        }

        public void Disburse(DbPlayer p_DbPlayer, int p_Amount)
        {
            p_DbPlayer.GiveMoney(p_Amount);

            p_DbPlayer.Team.AddBankHistory(-p_Amount, $"Auszahlung von {p_DbPlayer.GetName()}");

            Money -= p_Amount;
            MySQLHandler.ExecuteAsync($"UPDATE `team_shelter` SET money = money - '{p_Amount}' WHERE id = '{Id}';");
        }

        public void GiveMoney(int amount)
        {
            Money += amount;
            MySQLHandler.ExecuteAsync($"UPDATE `team_shelter` SET money = money + '{amount}' WHERE id = '{Id}';");
        }
    }
}
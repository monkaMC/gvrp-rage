using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Module.Spawners;

namespace GVRP.Module.Banks
{
    public class Bank : Loadable<uint>
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public int Type { get; set; }
        public ColShape ColShape { get; set; }
        public int ActMoney { get; set; }
        public int MaxMoney { get; set; }
        public bool Unlimited { get; set; }

        public Bank(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
            Position = new Vector3(reader.GetFloat("pos_x"),
                reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            ActMoney = reader.GetInt32("actual_money");
            MaxMoney = reader.GetInt32("max_money");
            Type = reader.GetInt32("type");
            ColShape = ColShapes.Create(Position, 1.5f, 0);
            ColShape.SetData("bankId", Id);
            if (ActMoney == -1 || MaxMoney == -1)
                Unlimited = true;
            else
                Unlimited = false;
            if (Type == 2)
            {
                Main.ServerBlips.Add(Blips.Create(Position, Name, 207, 1.0f, true, 25));
            }
            else if (Type == 3)
            {
                Main.ServerBlips.Add(Blips.Create(Position, Name, 207, 1.0f, true, 46));
            }
        }

        public void SaveActMoneyToDb()
        {
            if (!Unlimited)
            {
                string query = $"UPDATE `bank` SET actual_money = {ActMoney} WHERE id = {Id};";
                MySQLHandler.ExecuteAsync(query);
            }
        }

        public bool CanMoneyDeposited(int amount)
        {
            return Unlimited == false ? (ActMoney + amount) <= MaxMoney : true;
        }

        public bool CanMoneyWithdrawn(int amount)
        {
            return Unlimited == false ? (ActMoney - amount) >= 0 : true;
        }

        public void DepositMoney(int amount)
        {
            if (!Unlimited)
                ActMoney = ActMoney + amount;
        }
        public void WithdrawMoney(int amount)
        {
            if (!Unlimited)
                ActMoney = ActMoney - amount;
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
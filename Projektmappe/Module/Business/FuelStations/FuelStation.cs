using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GVRP.Handler;
using GVRP.Module.Configurations;
using GVRP.Module.Items;
using GVRP.Module.Players.Db;
using GVRP.Module.Spawners;

namespace GVRP.Module.Business.FuelStations
{
    public class FuelStation : Loadable<uint>
    {
        public uint Id { get; }
        public string Name { get; set; }
        public Vector3 Position { get; }
        public Vector3 InfoCardPosition { get; }
        public int InfoCardRange { get; }
        public ColShape InfoCardColshape { get; }
        public int Price { get; set; }
        public List<FuelStationGas> Gas { get; set; }
        public Container Container { get; set; }
        public ColShape ColShape { get; set; }
        public int BuyPrice { get; set; }
        public Business OwnerBusiness { get; set; }

        public FuelStation(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name").Trim();
            Price = reader.GetInt32("price");
            BuyPrice = reader.GetInt32("buy_price");
            Position = new Vector3(reader.GetFloat("pos_x"),
                reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            InfoCardPosition = new Vector3(reader.GetFloat("infocard_x"),
                reader.GetFloat("infocard_y"), reader.GetFloat("infocard_z"));
            InfoCardRange = reader.GetInt32("range");
            Gas = new List<FuelStationGas>();
            ColShape = ColShapes.Create(Position, 2.0f);
            ColShape.SetData("fuelstationId", Id);
            InfoCardColshape = ColShapes.Create(InfoCardPosition, InfoCardRange);
            InfoCardColshape.SetData("fuelstationInfoId", Id);
            OwnerBusiness = BusinessModule.Instance.GetAll().Values.FirstOrDefault(b => b.BusinessBranch.FuelstationId == Id);
        }

        public override uint GetIdentifier()
        {
            return Id;
        }

        public bool IsOwnedByBusines()
        {
            return OwnerBusiness != null;
        }

        public Business GetOwnedBusiness()
        {
            return OwnerBusiness;
        }

        public void SetFuelPrice(int amount)
        {
            Price = amount;
            MySQLHandler.ExecuteAsync($"UPDATE business_fuelstations SET `price` = '{amount}' WHERE `id` = '{Id}'");
        }
        
        public void SetFuelName(string name)
        {
            Name = name;
            MySQLHandler.ExecuteAsync($"UPDATE business_fuelstations SET `name` = '{name}' WHERE `id` = '{Id}'");
        }

        public void ReloadData()
        {
            Task.Run(async () =>
            {
                using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                using (var cmd = conn.CreateCommand())
                {
                    await conn.OpenAsync();
                    cmd.CommandText = $"SELECT * FROM `business_fuelstations` where id = {Id};";
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                Name = reader.GetString("name");
                                Price = reader.GetInt32("price");
                                BuyPrice = reader.GetInt32("buy_price");
                            }
                        }
                    }
                    await conn.CloseAsync();
                }
            });
        }

        public void LogFuelStationProgress(DbPlayer dbPlayer, int amount, int price, bool duty = false)
        {
            MySQLHandler.ExecuteAsync($"INSERT INTO `business_fuelstations_logs` (player_id, fuelsum_amount, fuelsum_price, fuel_duty, fuel_id) VALUES ('{dbPlayer.Id}', '{amount}', '{price}', '{(duty ? 1:0)}', '{Id}')");
        }
    }
}

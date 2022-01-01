using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Handler;
using GVRP.Module.Carsell.Menu;
using GVRP.Module.Configurations;
using GVRP.Module.Items;
using GVRP.Module.Menu;
using GVRP.Module.NSA.Menu;
using GVRP.Module.PlayerName;
using GVRP.Module.Players.Db;
using GVRP.Module.Teams;
using GVRP.Module.Teams.Shelter;
using GVRP.Module.Vehicles;
using GVRP.Module.Vehicles.Data;

namespace GVRP.Module.JobFactions.Carsell
{
    public enum CustomerOrderStatus
    {
        Ordered = 1,
        Delivered = 2,
    }

    public class DeliveryOrder
    {
        public uint Id { get; set; }
        public uint SellerId { get; set; }
        public uint PlayerId { get; set; }
        public uint TeamId { get; set; }
        public uint VehicleDataId { get; set; }
        public int Wheel { get; set; }
        public int Color1 { get; set; }
        public int Color2 { get; set; }
    }

    public class JobCarsellFactionModule : Module<JobCarsellFactionModule>
    {
        public static int MaxFVehicles = 30;
        public static int FVehicleBuyPrice = 10000;

        public static uint Carsell_1_Item = 1040;
        public static uint Carsell_2_Item = 1041;
        public static uint Carsell_3_Item = 1042;
        public static uint Carsell_4_Item = 1043;
        public static uint Carsell_5_Item = 1044;
        public static uint Carsell_6_Item = 1045;
        public static uint Carsell_7_Item = 1046;
        public static uint Carsell_8_Item = 1047;
        public static uint Carsell_9_Item = 1048;
        public static uint Carsell_10_Item = 1049;

        public static uint GarageTeam1 = 956;
        public static uint GarageTeam2 = 957;
        public static uint GarageTeam3 = 958;

        public List<DeliveryOrder> DeliverableOrderList = new List<DeliveryOrder>();

        public static List<int> WhitelistedColors = new List<int>() { 147, 134, 89, 53, 64, 135, 145, 44 };

        public static Vector3 MenuPosition = new Vector3(-32.2191, -1114.52, 26.4223);
        public static Vector3 TuningPosition = new Vector3(-31.0492, -1090.27, 26.4378);

        public static Vector3 MenuPosition2 = new Vector3(1774.55, 3322.77, 41.4499);
        public static Vector3 TuningPosition2 = new Vector3(1769.35, 3330.92, 41.4471);

        public static Vector3 MenuPosition3 = new Vector3(-22.4131, -1661.99, 29.4797);
        public static Vector3 TuningPosition3 = new Vector3(-9.77742, -1663.82, 29.4796);

        public override Type[] RequiredModules()
        {
            return new[] { typeof(VehicleDataModule), typeof(TeamModule) };
        }

        public bool CanAddVehicle(uint teamId)
        {
            int amount = 0;
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"SELECT COUNT(*) FROM fvehicles WHERE team = '{teamId}';";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            amount += reader.GetInt32(0);
                        }
                    }
                }
                conn.Close();
            }

            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"SELECT COUNT(*) FROM jobfaction_carsell_fvorder WHERE team_id = '{teamId}';";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            amount += reader.GetInt32(0);
                        }
                    }
                }
                conn.Close();
            }
            return amount < MaxFVehicles;
        }

        public int GetCategoryAmount(uint carsellCategoryId, uint teamId)
        {
            int amount = 0;
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"SELECT COUNT(*) FROM fvehicles WHERE team = '{teamId}' AND model IN (SELECT id FROM vehicledata WHERE `carsell_category` = '{carsellCategoryId}');";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            return amount += reader.GetInt32(0);
                        }
                    }
                }
                conn.Close();
            }

            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"SELECT COUNT(*) FROM jobfaction_carsell_fvorder WHERE team_id = '{teamId}' AND vehicle_data_id IN (SELECT id FROM vehicledata WHERE `carsell_category` = '{carsellCategoryId}');";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            return amount += reader.GetInt32(0);
                        }
                    }
                }
                conn.Close();
            }
            return amount;
        }

        protected override bool OnLoad()
        {
            // Load Menus
            MenuManager.Instance.AddBuilder(new CarsellBuycarMenuBuilder());
            MenuManager.Instance.AddBuilder(new CarsellBuycarSubMenuBuilder());
            MenuManager.Instance.AddBuilder(new CarsellMenuBuilder());
            MenuManager.Instance.AddBuilder(new CarsellDeleteMenuBuilder());
            MenuManager.Instance.AddBuilder(new CarsellCustomerMenuBuilder());
            MenuManager.Instance.AddBuilder(new CarsellTuneWheelMenuBuilder());
            MenuManager.Instance.AddBuilder(new CarsellDeliverCustomerMenuBuilder());

            //Insert ordered Fvehicles
            InsertOrderedVehicles();

            // Deliver Orders
            DeliverCustomerOrders();

            return base.OnLoad();
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if(key == Key.E)
            {
                if(!dbPlayer.Player.IsInVehicle)
                {
                    if(dbPlayer.Player.Position.DistanceTo(MenuPosition) < 3.0f && dbPlayer.Team.Id == (int)JobFactions.Carsell)
                    {
                        Module.Menu.MenuManager.Instance.Build(GVRP.Module.Menu.PlayerMenu.CarsellMenu, dbPlayer).Show(dbPlayer);
                        return true;
                    }
                    if (dbPlayer.Player.Position.DistanceTo(MenuPosition2) < 3.0f && dbPlayer.Team.Id == (int)JobFactions.Carsell2)
                    {
                        Module.Menu.MenuManager.Instance.Build(GVRP.Module.Menu.PlayerMenu.CarsellMenu, dbPlayer).Show(dbPlayer);
                        return true;
                    }
                    if (dbPlayer.Player.Position.DistanceTo(MenuPosition3) < 3.0f && dbPlayer.Team.Id == (int)JobFactions.Carsell3)
                    {
                        Module.Menu.MenuManager.Instance.Build(GVRP.Module.Menu.PlayerMenu.CarsellMenu, dbPlayer).Show(dbPlayer);
                        return true;
                    }
                }
                else
                {
                    if (dbPlayer.Player.Position.DistanceTo(TuningPosition) < 5.0f && dbPlayer.Team.Id == (int)JobFactions.Carsell)
                    {
                        SxVehicle sxVeh = dbPlayer.Player.Vehicle.GetVehicle();
                        if (sxVeh == null || !sxVeh.IsValid() || sxVeh.teamid != dbPlayer.TeamId) return false;

                        Module.Menu.MenuManager.Instance.Build(GVRP.Module.Menu.PlayerMenu.CarsellCustomerMenu, dbPlayer).Show(dbPlayer);
                        return true;
                    }
                    if (dbPlayer.Player.Position.DistanceTo(TuningPosition2) < 5.0f && dbPlayer.Team.Id == (int)JobFactions.Carsell2)
                    {
                        SxVehicle sxVeh = dbPlayer.Player.Vehicle.GetVehicle();
                        if (sxVeh == null || !sxVeh.IsValid() || sxVeh.teamid != dbPlayer.TeamId) return false;

                        Module.Menu.MenuManager.Instance.Build(GVRP.Module.Menu.PlayerMenu.CarsellCustomerMenu, dbPlayer).Show(dbPlayer);
                        return true;
                    }
                    if (dbPlayer.Player.Position.DistanceTo(TuningPosition3) < 5.0f && dbPlayer.Team.Id == (int)JobFactions.Carsell3)
                    {
                        SxVehicle sxVeh = dbPlayer.Player.Vehicle.GetVehicle();
                        if (sxVeh == null || !sxVeh.IsValid() || sxVeh.teamid != dbPlayer.TeamId) return false;

                        Module.Menu.MenuManager.Instance.Build(GVRP.Module.Menu.PlayerMenu.CarsellCustomerMenu, dbPlayer).Show(dbPlayer);
                        return true;
                    }
                }
            }
            return false;
        }
        
        public void InsertOrderedVehicles()
        {
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"SELECT * FROM jobfaction_carsell_fvorder;";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            DbTeam team = TeamModule.Instance.GetById(reader.GetInt32(1));
                            if (team == null) continue;
                            VehicleData vehData = VehicleDataModule.Instance.GetDataById(reader.GetUInt32(3));
                            if (vehData == null) continue;

                            // Insert Into Fvehicles on Startup
                            MySQLHandler.ExecuteAsync($"INSERT INTO `fvehicles` (`vehiclehash`, `team`, `color1`, `color2`, `inGarage`, `model`, `fuel`, `gps_tracker`, `registered`, `plate`) " +
                                $"VALUES ('{(vehData.mod_car_name.Length <= 0 ? vehData.Model : vehData.mod_car_name)}', '{team.Id}', '-1', '-1', '1', '{vehData.Id}', '{vehData.Fuel}', '1', '1', '{team.ShortName}');");
                        }
                    }
                }
                conn.Close();

                MySQLHandler.ExecuteAsync($"DELETE FROM jobfaction_carsell_fvorder WHERE 1 = 1;");
            }
            return;
        }


        public void DeliverCustomerOrders()
        {
            DeliverableOrderList = new List<DeliveryOrder>();

            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = $"SELECT * FROM jobfaction_carsell_orders WHERE status = 1 AND ordered < CURRENT_DATE - INTERVAL 3 DAY;";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            DeliverableOrderList.Add(new DeliveryOrder() { 
                                Id = reader.GetUInt32("id"),
                                PlayerId = reader.GetUInt32("player_id"),
                                SellerId = reader.GetUInt32("seller_id"),
                                TeamId = reader.GetUInt32("team_id"),
                                VehicleDataId = reader.GetUInt32("vehicle_data_id"),
                                Wheel = reader.GetInt32("wheel"),
                                Color1 = reader.GetInt32("color1"),
                                Color2 = reader.GetInt32("color2"),
                            });
                        }
                    }
                }
                conn.Close();
            }
            return;
        }

        public void InsertCustomerOrder(uint sellerId, uint playerId, uint team_id, int vehicleDataId, int wheelId, int color1, int color2)
        {
            MySQLHandler.ExecuteAsync($"INSERT INTO jobfaction_carsell_orders (`seller_id`, `player_id`, `team_id`, `vehicle_data_id`, `wheel`, `color1`, `color2`, `status`)" +
                $" VALUES ('{sellerId}', '{playerId}', '{team_id}', '{vehicleDataId}', '{wheelId}', '{color1}', '{color2}', '1');");
            return;
        }
    }
}

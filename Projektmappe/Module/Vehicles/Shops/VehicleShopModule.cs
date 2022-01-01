using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Handler;
using GVRP.Module.Configurations;
using GVRP.Module.GTAN;
using GVRP.Module.Logging;
using GVRP.Module.NpcSpawner;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Spawners;
using GVRP.Module.Vehicles.Data;

namespace GVRP.Module.Vehicles.Shops
{
    //Todo: split into multiple modules
    public sealed class VehicleShopModule : Module<VehicleShopModule>
    {
        private Dictionary<int, ShopVehicle> shopVehicles;
        private Dictionary<int, VehicleShop> vehicleShops;

        public override Type[] RequiredModules()
        {
            return new[] { typeof(VehicleDataModule) };
        }

        protected override bool OnLoad()
        {
            if (shopVehicles != null)
            {
                foreach (var shopVehicle in shopVehicles.Values)
                {
                    shopVehicle.ColShape.Delete();
                    shopVehicle.Entity?.entity.Delete();
                }

                shopVehicles.Clear();
            }
            else
            {
                shopVehicles = new Dictionary<int, ShopVehicle>();
            }

            if (vehicleShops == null)
            {
                vehicleShops = new Dictionary<int, VehicleShop>();
            }
            else
            {
                foreach (var vehicleShop in vehicleShops.Values)
                {
                    vehicleShop.Blip.Delete();
                    vehicleShop.ColShape.Delete();
                }

                vehicleShops.Clear();
            }

            LoadVehicles();
            LoadSpecialVehicles();
            LoadCarShops();
            return true;
        }

        public void LoadSpecialVehicles()
        {
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = @"SELECT * FROM `carshop_special_vehicles` ORDER BY `id`;";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var model = reader.GetUInt32(2);
                            var data = VehicleDataModule.Instance.GetDataById(model);
                            if (data != null)
                            {
                                var shopVehicle = new ShopVehicle
                                {
                                    Id = reader.GetInt32("id"),
                                    VehicleShopId = reader.GetInt32("carshopId"),
                                    Data = data,
                                    Name = reader.GetString("vehicleHashName"),
                                    Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"),
                                    reader.GetFloat("pos_z") - 1.0f),
                                    Heading = reader.GetFloat("heading"),
                                    PrimaryColor = reader.GetInt32("primary_color"),
                                    SecondaryColor = reader.GetInt32("secondary_color"),
                                    Dimension = reader.GetInt32("dimension"),
                                    Price = data.Price - reader.GetInt32("discount"),
                                    LimitedBuyed = reader.GetInt32("limited_buyed"),
                                    LimitedAmount = reader.GetInt32("limited_amount"),
                                    IsSpecialCar = true,
                                };
                                if (shopVehicle.LimitedBuyed < shopVehicle.LimitedAmount || shopVehicle.LimitedAmount == 0)
                                {
                                    shopVehicle.Entity = VehicleHandler.Instance
                                            .CreateServerVehicle(shopVehicle.Data.Id, false, shopVehicle.Position,
                                                shopVehicle.Heading,
                                                shopVehicle.PrimaryColor, shopVehicle.SecondaryColor, Convert.ToUInt32(shopVehicle.Dimension), true,
                                                true)
                                        ;
                                    shopVehicle.Entity.SyncExtension.SetEngineStatus(false);
                                    var colShape = ColShapes.Create(shopVehicle.Position, 3f, (uint)shopVehicle.Dimension);
                                    colShape.SetData("shopVehicleId", shopVehicle.Id);
                                    shopVehicle.ColShape = colShape;
                                    shopVehicles.Add(shopVehicle.Id, shopVehicle);
                                }
                            }
                            else
                            {
                                Logger.Print($"Vehicle data not found {model}");
                            }
                        }
                    }
                }
            }
        }

        public void LoadVehicles()
        {
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = @"SELECT * FROM `carshop_vehicles` ORDER BY `id`;";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var model = reader.GetUInt32(2);
                            var data = VehicleDataModule.Instance.GetDataById(model);
                            if (data != null)
                            {
                                var shopVehicle = new ShopVehicle
                                {
                                    Id = reader.GetInt32("id"),
                                    VehicleShopId = reader.GetInt32("carshopId"),
                                    Data = data,
                                    Name = reader.GetString("vehicleHashName"),
                                    Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"),
                                    reader.GetFloat("pos_z") - 1.0f),
                                    Heading = reader.GetFloat("heading"),
                                    PrimaryColor = reader.GetInt32("primary_color"),
                                    SecondaryColor = reader.GetInt32("secondary_color"),
                                    Dimension = reader.GetInt32("dimension"),
                                    Price = data.Price - reader.GetInt32("discount"),
                                    IsSpecialCar = false,
                                    LimitedBuyed = 0,
                                    LimitedAmount = 0,                                    
                                };
                                shopVehicle.Entity = VehicleHandler.Instance
                                        .CreateServerVehicle(shopVehicle.Data.Id, false, shopVehicle.Position,
                                            shopVehicle.Heading,
                                            shopVehicle.PrimaryColor, shopVehicle.SecondaryColor, Convert.ToUInt32(shopVehicle.Dimension), true,
                                            true)
                                    ;
                                shopVehicle.Entity.SyncExtension.SetEngineStatus(false);

                                var colShape = ColShapes.Create(shopVehicle.Position, 3f, (uint)shopVehicle.Dimension);
                                colShape.SetData("shopVehicleId", shopVehicle.Id);
                                shopVehicle.ColShape = colShape;
                                shopVehicles.Add(shopVehicle.Id, shopVehicle);
                            }
                            else
                            {
                                Logger.Print($"Vehicle data not found {model}");
                            }
                        }
                    }
                }
            }
        }

        public void LoadCarShops()
        {
            PointsOfInterest.PointOfInterestModule.Instance.GetAll().TryGetValue(145, out PointsOfInterest.PointOfInterest temp);

            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = @"SELECT * FROM `carshop_shops` ORDER BY `id`;";
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var vehicleShop = new VehicleShop
                            {
                                Id = reader.GetInt32("id"),
                                PedHash = Enum.TryParse(reader.GetString("ped_hash"), true, out PedHash skin) ? skin : PedHash.PrologueHostage01AMM,
                                Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"),
                                    reader.GetFloat("pos_z") - 1.0f),
                                Heading = reader.GetFloat("heading"),
                                SpawnPosition = new Vector3(reader.GetFloat("spawnpos_x"), reader.GetFloat("spawnpos_y"),
                                    reader.GetFloat("spawnpos_z") - 1.0f),
                                SpawnHeading = reader.GetFloat("spawnheading"),
                                Marker = reader.GetBoolean("marker"),
                                Activated = reader.GetInt32("activated") == 1,
                                Dimension = reader.GetUInt32("dimension"),
                                Description = reader.GetString("description"),
                                Vehicles = new Dictionary<int, ShopVehicle>()
                            };
                            if(vehicleShop.Id == 1001)
                            {
                                vehicleShop.PlayerIds = new List<int>();
                                string playerIdsString = reader.GetString("player_ids");
                                if (!string.IsNullOrEmpty(playerIdsString))
                                {
                                    string[] splittedPlayerIds = playerIdsString.Split(",");
                                    
                                    foreach(string playerIdString in splittedPlayerIds)
                                    {
                                        if(!int.TryParse(playerIdString, out int playerId)) return;
                                        vehicleShop.PlayerIds.Add(playerId);
                                    }
                                }

                            }


                            vehicleShop.RestrictedTeams = new HashSet<int>();
                            string teamString = reader.GetString("restricted_teams");
                            if (!string.IsNullOrEmpty(teamString))
                            {
                                var splittedTeams = teamString.Split(',');
                                foreach (var teamIdString in splittedTeams)
                                {
                                    if (!int.TryParse(teamIdString, out var teamId)) continue;
                                    vehicleShop.RestrictedTeams.Add(teamId);
                                }
                            }

                            if (vehicleShop.RestrictedTeams.Count > 0)
                            {
                                vehicleShop.TeamCarShop = true;
                            }
                            else vehicleShop.TeamCarShop = false;

                            vehicleShop.ColShape = ColShapes.Create(vehicleShop.Position, 3f);
                            vehicleShop.ColShape.SetData("vehicleShopId", vehicleShop.Id);
                            Log(@"Carshop hinzugefuegt ID: " + vehicleShop.Id);

                            var position = new Vector3();

                            new Npc(vehicleShop.PedHash, vehicleShop.Position, vehicleShop.Heading, vehicleShop.Dimension);

                            if (vehicleShop.Marker)
                            {
                                vehicleShop.Blip = Blips.Create(vehicleShop.Position, "Fahrzeughandel", 225, 1.0f, color:(int)4);
                                Main.ServerBlips.Add(vehicleShop.Blip);
                            }

                            foreach (var shopVehicle in shopVehicles)
                            {
                                if (shopVehicle.Value.VehicleShopId == vehicleShop.Id)
                                {
                                    vehicleShop.Vehicles.Add(shopVehicle.Key, shopVehicle.Value);
                                }
                            }

                            vehicleShops.Add(vehicleShop.Id, vehicleShop);
                        }
                    }
                }
            }
        }

        public VehicleShop GetThisShop(Vector3 position)
        {
            return vehicleShops.Values.FirstOrDefault(shop => shop.Position.DistanceTo(position) <= 3.0f);
        }

        public List<ShopVehicle> GetVehsFromCarShop(int vehicleShopId)
        {
            return shopVehicles.Values.Where(vehicle => vehicle.VehicleShopId == vehicleShopId).ToList();
        }

        public int GetVehiclePriceFromHash(VehicleData data)
        {
            if (data == null) return 5000;
            return data.Price / 4;
        }

        public Dictionary<int, ShopVehicle> GetAll()
        {
            return shopVehicles;
        }

        public ShopVehicle GetShopVehicle(int id)
        {
            return shopVehicles.ContainsKey(id) ? shopVehicles[id] : null;
        }

        public VehicleShop GetVehicleShop(int id)
        {
            return vehicleShops.ContainsKey(id) ? vehicleShops[id] : null;
        }

        public Dictionary<int, VehicleShop> GetAllShops()
        {
            return vehicleShops;
        }

        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if (colShapeState == ColShapeState.Enter)
            {
                if (colShape.TryData("shopVehicleId", out int shopVehicleId))
                {
                    var shopVehicle = GetShopVehicle(shopVehicleId);
                    if (shopVehicle == null) return false;
                    dbPlayer.SendNewNotification($"Preis: ${shopVehicle.Price} Steuer: ${shopVehicle.Data.Tax} Inventar: {shopVehicle.Data.InventoryWeight/1000} kg Slots: {shopVehicle.Data.InventorySize}", title: $"{shopVehicle.Name}");
                    return true;
                }

                if (colShape.TryData("vehicleShopId", out int vehicleShopId))
                {
                    var vehicleShop = GetVehicleShop(vehicleShopId);
                    if (vehicleShop == null) return false;
                    dbPlayer.SetData("vehicleShopId", shopVehicleId);
                    dbPlayer.SendNewNotification("Benutze E um ein Fahrzeug zu kaufen!", title: "Fahrzeughandel");
                    return true;
                }
            }
            else if (colShapeState == ColShapeState.Exit)
            {
                if (colShape.TryData("vehicleShopId", out int vehicleShopId))
                {
                    if (dbPlayer.TryData("vehicleShopId", out int playerVehicleShopId))
                    {
                        if (vehicleShopId == playerVehicleShopId)
                        {
                            dbPlayer.ResetData("vehicleShopId");
                        }
                    }
                }
            }

            return false;
        }
    }
}
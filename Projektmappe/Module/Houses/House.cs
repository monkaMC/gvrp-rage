
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using GTANetworkAPI;
using GVRP.Module;
using GVRP.Module.Configurations;
using GVRP.Module.Houses;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.PlayerTask;
using GVRP.Module.Vehicles.Garages;
using GVRP.Module.Items;
using System;
using GVRP.Module.Schwarzgeld;
using GVRP.Module.Logging;
using System.Linq;

namespace GVRP
{
    public class House : Loadable<uint>
    {
        public uint Id { get; set; }
        public int Type { get; set; }
        public int Price { get; set; }
        public Interior Interior { get; set; }
        public uint InteriorId { get; }
        public uint OwnerId { get; set; }
        public string OwnerName { get; set; }
        public Vector3 Position { get; set; }
        public float Heading { get; set; }
        public bool Locked { get; set; }
        public int Maxrents { get; set; }
        public int InventoryCash { get; set; }
        public int Keller { get; set; }
        public int MoneyKeller { get; set; }
        public Garage Garage { get; set; }
        public uint GarageId { get; set; }
        public List<DbPlayer> PlayersInHouse { get; }
        public ColShape ColShape { get; set; }
        public Vector3 ColShapePosition { get; set; }
        public Container Container { get; set; }
        public Container LaborContainer { get; set; }
        public bool KellerLoaded { get; set; }
        public DateTime LastBreak { get; set; }

        public string ShowPhoneNumber { get; set; }
        public float TrashAmount { get; set; }

        public int BLAmount { get; set; }
        
        public bool IsDimensionNullHouse { get; set; }
        public Container BlackMoneyInvContainer { get; set; }
        public Container BlackMoneyCodeContainer { get; set; }

        public Container BlackMoneyBatterieContainer { get; set; }

        public int BlackMoneyTick { get; set; }

        public bool Disabled { get; set; }

        public House(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Type = reader.GetInt32("type");
            Price = reader.GetInt32("price");
            InteriorId = reader.GetUInt32("interiorid");
            OwnerId = reader.GetUInt32("ownerID");
            OwnerName = reader.GetString("owner");
            Position = new Vector3(reader.GetFloat("posX"), reader.GetFloat("posY"), reader.GetFloat("posZ"));
            ColShapePosition = new Vector3(reader.GetFloat("colshapeX"), reader.GetFloat("colshapeY"), reader.GetFloat("colshapeZ"));
            Heading = reader.GetFloat("heading");
            Maxrents = reader.GetInt32("maxrents");
            Locked = true;
            InventoryCash = reader.GetInt32("inv_cash");
            Container = ContainerManager.LoadContainer(Id, ContainerTypes.HOUSE, 0);
            LaborContainer = ContainerManager.LoadContainer(Id, ContainerTypes.LABOR_MEERTRAEUBEL, 0, 0);
            Keller = reader.GetInt32("keller");
            MoneyKeller = reader.GetInt32("moneykeller");
            Garage = GarageModule.Instance.GetHouseGarage(reader.GetUInt32("garage"));
            GarageId = reader.GetUInt32("garage");
            KellerLoaded = false;
            TrashAmount = reader.GetFloat("trash_amount");
            IsDimensionNullHouse = reader.GetInt32("dimension_null_house") == 1;
            ShowPhoneNumber = reader.GetString("show_phonenumber");
            PlayersInHouse = new List<DbPlayer>();
            LastBreak = DateTime.Now.Add(new TimeSpan(0, -10, 0)); // set lastbreak for load now -10 min

            // Set Disabled State
            if (Position.X == 0 || Position.Y == 0)
            {
                Disabled = true;
            }
            else Disabled = false;

            BlackMoneyInvContainer = ContainerManager.LoadContainer(Id, ContainerTypes.BLACKMONEYINVENTORY, 0);
            BlackMoneyCodeContainer = ContainerManager.LoadContainer(Id, ContainerTypes.BLACKMONEYCODES, 0);
            BlackMoneyBatterieContainer = ContainerManager.LoadContainer(Id, ContainerTypes.BLACKMONEYBATTERIE, 0);

            BlackMoneyTick = 0;

            BLAmount = reader.GetInt32("bl_amount");

            // Moneykeller
            NAPI.Marker.CreateMarker(25, (SchwarzgeldModule.BlackMoneyEndPoint - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(255, 0, 0, 155), true, Id);
            NAPI.Marker.CreateMarker(25, (SchwarzgeldModule.BlackMoneyCodeContainer - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(0, 255, 0, 155), true, Id);
            NAPI.Marker.CreateMarker(25, (SchwarzgeldModule.BlackMoneyInvPosition - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(0, 255, 0, 155), true, Id);
            NAPI.Marker.CreateMarker(25, (SchwarzgeldModule.BlackMoneyBatteriePoint - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(0, 255, 0, 155), true, Id);

            NAPI.Marker.CreateMarker(25, (SchwarzgeldModule.MarkedDollarWorkbench - new Vector3(0f, 0f, 0.95f)), new Vector3(), new Vector3(), 1f, new Color(255, 0, 0, 155), true, Id);

            // Gleiche Slots mit Tenants
            if (HouseRentModule.Instance.houseRents.ToList().Where(hm => hm.HouseId == Id).Count() < Maxrents)
            {
                List<HouseRent> thisHouseRents = HouseRentModule.Instance.houseRents.Where(hm => hm.HouseId == Id).ToList();
                for(int i = 1; i < Maxrents+1; i++)
                {
                    HouseRent xRent = thisHouseRents.Where(h => h.SlotId == i).FirstOrDefault();
                    if(xRent == null) // Not found? Anlegen...
                    {
                        this.AddCleanTenant(i, 0);
                    }
                }
            }

        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }

    //Todo: move to House class instead of extension functions
    public static class HouseFunctions
    {
      
        public static void SaveTrash(this House house)
        {
            var query = string.Format($"UPDATE `houses` SET trash_amount = '{house.TrashAmount}' WHERE `id` = '{house.Id}';");
            MySQLHandler.ExecuteAsync(query);
        }

        public static void SaveBlackMoney(this House house)
        {
            var query = string.Format($"UPDATE `houses` SET bl_amount = '{house.BLAmount}' WHERE `id` = '{house.Id}';");
            MySQLHandler.ExecuteAsync(query);
        }

        public static void SaveShowPhoneNumber(this House house)
        {
            var query = string.Format($"UPDATE `houses` SET show_phonenumber = '{house.ShowPhoneNumber}' WHERE `id` = '{house.Id}';");
            MySQLHandler.ExecuteAsync(query);
        }

        // 5 * Mietplätze + 5 für vermieter
        public static int GetGarageSize(this House house)
        {
            return (house.Maxrents * 5) + 5;
        }

        public static int GetInventorySize(this House house)
        {
            return house.Container.MaxWeight;
        }

        public static void SaveOwner(this House house)
        {
            var query =
                $"UPDATE `houses` SET owner = '{house.OwnerName}', ownerID = '{house.OwnerId}' WHERE `id` = '{house.Id}';";
            MySQLHandler.ExecuteAsync(query);
        }

        public static void SaveInterior(this House house)
        {
            var query = $"UPDATE `houses` SET interiorid = '{house.Interior.Id}' WHERE `id` = '{house.Id}';";
            MySQLHandler.ExecuteAsync(query);
        }
        
        public static void SaveGarage(this House house)
        {
            var query = string.Format(
                $"UPDATE `houses` SET garage = '{GarageModule.Instance.GetHouseGarage(house.Id).Id}' WHERE `id` = '{house.Id}';");
            MySQLHandler.ExecuteAsync(query);
        }

        public static void SaveKeller(this House house)
        {
            house.Container.ChangeWeight(800000);
            house.Container.ChangeSlots(30);
            var query = string.Format($"UPDATE `houses` SET keller = '{house.Keller}' WHERE `id` = '{house.Id}';");
            MySQLHandler.ExecuteAsync(query);
        }

        public static void SaveMoneyKeller(this House house)
        {
            house.Container.ChangeWeight(800000);
            house.Container.ChangeSlots(30);
            var query = string.Format($"UPDATE `houses` SET moneykeller = '{house.MoneyKeller}' WHERE `id` = '{house.Id}';");
            MySQLHandler.ExecuteAsync(query);
        }

        public static void SaveHouseBank(this House house)
        {
            var query = string.Format(
                $"UPDATE `houses` SET inv_cash = '{house.InventoryCash}' WHERE `id` = '{house.Id}';");
            MySQLHandler.ExecuteAsync(query);
        }

        public static bool CanKellerBuild(this House iHouse)
        {
            // Build Keller
            if (iHouse.Keller != 0) return false;

            return iHouse.Container.GetItemAmount(312) >= 25 && iHouse.Container.GetItemAmount(310) >= 60;
        }

        public static void BuildKeller(this House iHouse, DbPlayer iPlayer)
        {
            // 25 Zement, 60 Holzplanken
            iHouse.Container.RemoveItem(312, 25);
            iHouse.Container.RemoveItem(310, 60);

            iPlayer.AddTask(PlayerTaskTypeId.KellerAusbau);
        }

        public static bool CanMoneyKellerBuild(this House iHouse)
        {
            // Build Keller
            if (iHouse.MoneyKeller != 0) return false;

            return iHouse.Container.GetItemAmount(312) >= 10 && iHouse.Container.GetItemAmount(310) >= 30;
        }

        public static void BuildMoneyKeller(this House iHouse, DbPlayer iPlayer)
        {
            // 10 Zement, 30 Holzplanken
            iHouse.Container.RemoveItem(312, 10);
            iHouse.Container.RemoveItem(310, 30);

            iPlayer.AddTask(PlayerTaskTypeId.MoneyKellerAusbau);
        }

        public static bool CanGarageBuild(this House iHouse)
        {
            // Build Keller
            if (iHouse.GarageId != 0) return false;
            
            return iHouse.Container.GetItemAmount(312) >= 50 && iHouse.Container.GetItemAmount(310) >= 100;
        }

        public static void BuildGarage(this House iHouse)
        {
            // 80 Zement, 150 Holzplanken
            iHouse.Container.RemoveItem(312, 50);
            iHouse.Container.RemoveItem(310, 100);

            iHouse.Garage = GarageModule.Instance.GetHouseGarage(iHouse.GarageId);
            iHouse.SaveGarage();
        }
        
        public static bool CanKellerUpgraded(this House iHouse)
        {
            // Build Keller
            if (iHouse.Keller != 1) return false;
            
            return iHouse.Container.GetItemAmount(312) >= 40 && iHouse.Container.GetItemAmount(310) >= 100;
        }

        public static void UpgradeKeller(this House iHouse, DbPlayer iPlayer)
        {
            // 40 Zement, 100 Holzplanken
            iHouse.Container.RemoveItem(312, 40);
            iHouse.Container.RemoveItem(310, 100);

            iPlayer.AddTask(PlayerTaskTypeId.LaborAusbau);
        }
        
        public static int GetFreeRents(this House house)
        {
            return house.Maxrents - HouseRentModule.Instance.houseRents.Where(hr => hr.HouseId == house.Id && hr.PlayerId != 0).Count();
        }

        public static void Break(this House house)
        {
            house.Locked = false;
            house.LastBreak = DateTime.Now;
        }

        public static void ReloadHouse(this House house)
        {
            var query = $"SELECT * FROM `houses` WHERE id = '{house.Id}'";
            using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
            using (var cmd = conn.CreateCommand())
            {
                conn.Open();
                cmd.CommandText = @query;
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            house.Interior = InteriorModule.Instance.Get(reader.GetUInt32(3));

                            house.OwnerId = reader.GetUInt32("ownerID");
                            house.OwnerName = reader.GetString("owner");
                            house.InventoryCash = reader.GetInt32("inv_cash");
                            house.Keller = reader.GetInt32("keller");
                            house.MoneyKeller = reader.GetInt32("moneykeller");
                            house.GarageId = reader.GetUInt32("garage");
                            house.Price = reader.GetInt32("price");
                            house.Locked = true;
                        }
                    }
                }
            }
        }
    }
}
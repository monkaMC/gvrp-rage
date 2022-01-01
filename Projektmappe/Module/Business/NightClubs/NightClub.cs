using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GVRP.Module.Configurations;
using GVRP.Module.Items;
using GVRP.Module.Logging;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Shops.Windows;

namespace GVRP.Module.Business.NightClubs
{
    public class NightClub : Loadable<uint>
    {
        public uint Id { get; }
        public string Name { get; set; }
        public int Price { get; set; }
        public Vector3 Position { get; set; }
        public float Rotation { get; set; }
        public Vector3 GaragePosition { get; set; }
        public float GarageRotation { get; set; }
        public int Style { get; set; }
        public int Drinks { get; set; }
        public int Lights { get; set; }
        public int Effects { get; set; }
        public int Clubname { get; set; }
        public int Entrylighting { get; set; }
        public int Security { get; set; }
        public ColShape ColShape { get; set; }
        public ColShape GarageColShape { get; set; }
        public ColShape InteriorColshape { get; set; }
        public Business OwnerBusiness { get; set; }
        public bool Locked { get; set; }
        public bool GarageLocked { get; set; }
        public Container Container { get; set; }
        public List<NightClubItem> NightClubShopItems{ get; set; }
        public List<DbPlayer> PlayersInsideClub = new List<DbPlayer>();

        public NightClub(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
            Price = reader.GetInt32("price");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            Rotation = reader.GetFloat("float");
            GaragePosition = new Vector3(reader.GetFloat("garage_pos_x"), reader.GetFloat("garage_pos_y"), reader.GetFloat("garage_pos_z"));
            GarageRotation = reader.GetFloat("garage_float");

            Style = reader.GetInt32("style");
            Drinks = reader.GetInt32("drinks");
            Lights = reader.GetInt32("lights");
            Effects = reader.GetInt32("effects");
            Clubname = reader.GetInt32("clubname");
            Entrylighting = reader.GetInt32("entrylighting");
            Security = reader.GetInt32("security");

            InteriorColshape = Spawners.ColShapes.Create(new Vector3(-1605.38, -3005.81, -76.005), 100.0f);
            InteriorColshape.SetData("nightclubInterriorColshape", Id);

            ColShape = Spawners.ColShapes.Create(Position, 2.0f);
            ColShape.SetData("nightclubId", Id);

            GarageColShape = Spawners.ColShapes.Create(GaragePosition, 3.0f);
            GarageColShape.SetData("nightclubId", Id);
            
            OwnerBusiness = BusinessModule.Instance.GetAll().Values.FirstOrDefault(b => b.BusinessBranch.NightClubId == Id);

            Main.ServerBlips.Add(Spawners.Blips.Create(Position, "Nightclub", 93, 1.0f));

            // Default Locked
            Locked = true;
            GarageLocked = true;
        }

        public override uint GetIdentifier()
        {
            return Id;
        }

        public bool IsOwnedByBusines()
        {
            return OwnerBusiness != null;
        }

        public void LoadNightCLubInterrior(DbPlayer dbPlayer)
        {
            int style = Style;
            int drinks = Drinks;
            int lights = Lights;
            int effects = Effects;
            int clubName = Clubname;
            int entryLight = Entrylighting;
            int security = Security;

            dbPlayer.Player.TriggerEvent("loadNightclubInterrior", style, drinks, lights, effects, clubName, entryLight, security);
        }

        public void Upgrade(DbPlayer dbPlayer, int category, int index)
        {
            bool UpgradeStatus = false;

            switch (category)
            {
                case 1:
                    UpgradeStatus = UpgradeInterrior(dbPlayer, index);
                    break;
                case 2:
                    UpgradeStatus = UpgradeDrinks(dbPlayer, index);
                    break;
                case 3:
                    UpgradeStatus = UpgradeLights(dbPlayer, index);
                    break;
                case 4:
                    UpgradeStatus = UpgradeEffects(dbPlayer, index);
                    break;
                case 5:
                    UpgradeStatus = UpgradeClubname(dbPlayer, index);
                    break;
                case 6:
                    UpgradeStatus = UpgradeEntrylighting(dbPlayer, index);
                    break;
                case 7:
                    UpgradeStatus = UpgradeSecurity(dbPlayer, index);
                    break;
                default:
                    break;
            }

            if (UpgradeStatus)
            {
                PlayersInsideClub.ForEach(player => LoadNightCLubInterrior(player));
                SaveNightclub();
            }
        }

        private bool UpgradeInterrior(DbPlayer dbPlayer, int index)
        {
            int price = 200000;

            if (price > 0 && price <= dbPlayer.money[0])
            {
                dbPlayer.TakeMoney(price);
                dbPlayer.SendNewNotification("Umbau abgeschlossen!", PlayerNotification.NotificationType.BUSINESS);
            }
            else
            {
                dbPlayer.SendNewNotification("Du hast nicht das benötigte Geld dabei!", PlayerNotification.NotificationType.BUSINESS);
                return false;
            }

            Style = index;
            return true;
        }

        private bool UpgradeDrinks(DbPlayer dbPlayer, int index)
        {
            Drinks = index;
            return true;
        }

        private bool UpgradeLights(DbPlayer dbPlayer, int index)
        {
            int price = 20000;

            if (price > 0 && price <= dbPlayer.money[0])
            {
                dbPlayer.TakeMoney(price);
                dbPlayer.SendNewNotification("Umbau abgeschlossen!", PlayerNotification.NotificationType.BUSINESS);
            }
            else
            {
                dbPlayer.SendNewNotification("Du hast nicht das benötigte Geld dabei!", PlayerNotification.NotificationType.BUSINESS);
                return false;
            }

            Lights = index;
            return true;
        }

        private bool UpgradeEffects(DbPlayer dbPlayer, int index)
        {
            int price = 20000;

            if (price > 0 && price <= dbPlayer.money[0])
            {
                dbPlayer.TakeMoney(price);
                dbPlayer.SendNewNotification("Umbau abgeschlossen!", PlayerNotification.NotificationType.BUSINESS);
            }
            else
            {
                dbPlayer.SendNewNotification("Du hast nicht das benötigte Geld dabei!", PlayerNotification.NotificationType.BUSINESS);
                return false;
            }

            Effects = index;
            return true;
        }

        private bool UpgradeClubname(DbPlayer dbPlayer, int index)
        {
            Clubname = index;
            return true;
        }

        private bool UpgradeEntrylighting(DbPlayer dbPlayer, int index)
        {
            int price = 50000;

            if (price > 0 && price <= dbPlayer.money[0])
            {
                dbPlayer.TakeMoney(price);
                dbPlayer.SendNewNotification("Umbau abgeschlossen!", PlayerNotification.NotificationType.BUSINESS);
            }
            else
            {
                dbPlayer.SendNewNotification("Du hast nicht das benötigte Geld dabei!", PlayerNotification.NotificationType.BUSINESS);
                return false;
            }

            Entrylighting = index;
            return true;
        }

        private bool UpgradeSecurity(DbPlayer dbPlayer, int index)
        {
            int price = 50000;

            if (price > 0 && price <= dbPlayer.money[0])
            {
                dbPlayer.TakeMoney(price);
                dbPlayer.SendNewNotification("Umbau abgeschlossen!", PlayerNotification.NotificationType.BUSINESS);
            }
            else
            {
                dbPlayer.SendNewNotification("Du hast nicht das benötigte Geld dabei!", PlayerNotification.NotificationType.BUSINESS);
                return false;
            }

            Security = index;
            return true;
        }

        public void UnloadNightClubInterior(DbPlayer dbPlayer)
        {
            dbPlayer.Player.TriggerEvent("unloadNightclubInterrior");
        }

        public Business GetOwnedBusiness()
        {
            return OwnerBusiness;
        }

        public List<ShopItemX> ConvertForShop()
        {
            List<ShopItemX> shopItems = new List<ShopItemX>();

            foreach(NightClubItem nightClubItem in NightClubShopItems)
            {
                ShopItemX shopItemX = new ShopItemX(nightClubItem.ItemId, nightClubItem.Name, nightClubItem.Price);
                shopItems.Add(shopItemX);
            }

            return shopItems;
        }

        public void SaveNightclub()
        {
            string query = $"UPDATE `business_nightclubs` SET `style` = '{Style}', `drinks` = '{Drinks}', `lights` = '{Lights}', `effects` = '{Effects}', `clubname` = '{Clubname}', `entrylighting` = '{Entrylighting}', `security` = '{Security}' WHERE `id` = {Id};";
            MySQLHandler.ExecuteAsync(query);
        }

        public void SetName(string name)
        {
            Name = name;
            MySQLHandler.ExecuteAsync($"UPDATE business_nightclubs SET `name` = '{name}' WHERE `id` = '{Id}'");
        }

        public void ReloadData()
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
            {
                using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                using (var cmd = conn.CreateCommand())
                {
                    await conn.OpenAsync();
                    cmd.CommandText = $"SELECT * FROM `business_nightclubs` where id = {Id};";
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                Price = reader.GetInt32("price");
                            }
                        }
                    }
                    await conn.CloseAsync();
                }
            }));
        }
    }
}

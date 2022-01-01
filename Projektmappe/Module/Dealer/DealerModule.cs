using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GVRP.Module.Commands;
using GVRP.Module.Dealer.Menu;
using GVRP.Module.Delivery;
using GVRP.Module.Gangwar;
using GVRP.Module.Menu;
using GVRP.Module.NpcSpawner;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Teams;

namespace GVRP.Module.Dealer
{
    public class DealerModule : SqlModule<DealerModule, Dealer, uint>
    {
        public uint MethItemId          = 1;
        public uint DiamondItemId       = 21;
        public uint GoldBarrenItemId    = 487;
        public int MaxDealerCount       = 1;
        public int DiamondCap           = 15;
        public int GoldCap              = 15;
        public int WeaponsCap           = 5;
        public static Random random = new Random();

        public uint BigDealer = 0;
        public uint VehicleClawAmount = 0;
        public uint Maulwurf = 0;
        public int MaxMaulwuerfe = 0;
        public int MaxVehicleClawAmount = 1;
        public List<int> MaulwurfSpawnChances = new List<int> { 8, 28, 48, 68, 78, 86, 92, 97, 100 };
        
        public float MaulwurfAlarmChance = 1.0f;

        protected override string GetQuery()
        {
            return $"SELECT * FROM `dealer` ORDER BY RAND() LIMIT {MaxDealerCount};";
        }

        protected override bool OnLoad()
        {
            int rnd = Utils.RandomNumber(1, 100);
            MaulwurfSpawnChances.ForEach(item =>
            {
                if (rnd > item)
                    MaxMaulwuerfe++;
            });
            MenuManager.Instance.AddBuilder(new DealerSellMenu());
            return base.OnLoad();
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (dbPlayer.Player.IsInVehicle) return false;
            if (!dbPlayer.Team.IsGangsters()) return false;
            if (key != Key.E) return false;

            Dealer dealer = DealerModule.Instance.GetAll().Values.Where(d => d.Position.DistanceTo(dbPlayer.Player.Position) < 2.0f).FirstOrDefault();

            if(dealer != null)
            {
                dbPlayer.SetData("current_dealer", dealer.Id);
                MenuManager.Instance.Build(PlayerMenu.DealerSellMenu, dbPlayer).Show(dbPlayer);
                return true;
            }

            return false;
        }

        public override void OnFiveMinuteUpdate()
        {
            foreach (var l_Dealer in this.GetAll().Values)
            {
                foreach (var l_Resource in l_Dealer.DealerResources)
                {
                    int l_ResetTime = Configurations.Configuration.Instance.DevMode ? 10 : 60;

                    if (l_Resource.TimeSinceFull.AddMinutes(l_ResetTime) <= DateTime.Now && l_Resource.IsFull())
                    {
                        l_Resource.Sold = 0;
                        l_Resource.TimeSinceFull = DateTime.Now;
                    }
                }

                l_Dealer.Alert = false;
            }
        }
        
        public Dealer GetRandomDealer()
        {
            return GetAll().ElementAt(random.Next(0, MaxDealerCount)).Value;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandsavedealer(Client player, string comment)
        {
            var iPlayer = player.GetPlayer();


            if (!Configurations.Configuration.Instance.DevMode)
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            string x = player.Position.X.ToString().Replace(",", ".");
            string y = player.Position.Y.ToString().Replace(",", ".");
            string z = player.Position.Z.ToString().Replace(",", ".");
            string heading = player.Rotation.Z.ToString().Replace(",", ".");
            
            Main.ServerNpcs.Add(new Npc(PedHash.Abigail, player.Position, player.Heading, 0));

            MySQLHandler.ExecuteAsync($"INSERT INTO dealer (pos_x, pos_y, pos_z, heading, note) VALUES('{MySqlHelper.EscapeString(x)}', '{MySqlHelper.EscapeString(y)}', '{MySqlHelper.EscapeString(z)}', '{MySqlHelper.EscapeString(heading)}', '{MySqlHelper.EscapeString(comment)}')");
            iPlayer.SendNewNotification(string.Format("Dealer saved as: {0}", comment), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandshowdealer(Client player, string arg)
        {
            var iPlayer = player.GetPlayer();

            if (!Configurations.Configuration.Instance.DevMode)
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            if (!int.TryParse(arg, out int id)) return;
            var dealer = GetAll().ElementAt(id).Value;
            player.SendWayPoint(dealer.Position.X, dealer.Position.Y);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandrandomdealer(Client player)
        {
            var iPlayer = player.GetPlayer();

            if (!Configurations.Configuration.Instance.DevMode)
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            var dealer = GetRandomDealer();
            var position = Utils.GenerateRandomPosition(dealer.Position);
            player.SendWayPoint(position.X, position.Y);
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandgotodealer(Client player, string arg)
        {
            var iPlayer = player.GetPlayer();

            if (!Configurations.Configuration.Instance.DevMode)
            {
                iPlayer.SendNewNotification(MSG.Error.NoPermissions(), title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
                return;
            }

            if (!int.TryParse(arg, out int id)) return;
            var dealer = GetAll().ElementAt(id).Value;
            player.Position = dealer.Position;
        }
    }
}

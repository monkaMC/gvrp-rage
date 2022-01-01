using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GVRP.Module.Menu;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Teams.AmmoArmory
{
    public sealed class AmmoArmoryModule : SqlModule<AmmoArmoryModule, AmmoArmorie, uint>
    {
        public static uint AmmoChestItem = 609;
        public static int PacketsMax = 2500;
        public static int MaxLagerBestand = 24000;

        public override Type[] RequiredModules()
        {
            return new[] { typeof(AmmoArmoryItemModule) };
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `team_ammoarmories` ORDER BY team_id;";
        }

        public override bool Load(bool reload = false)
        {

            MenuManager.Instance.AddBuilder(new AmmoArmorieMenuBuilder());
            MenuManager.Instance.AddBuilder(new AmmoArmoriePriceMenuBuilder());
            return base.Load(reload);
        }

        public AmmoArmorie GetByPosition(Vector3 Position, float range = 3.0f)
        {
            return Instance.GetAll().Values.Where(a => a.Position.DistanceTo(Position) < range).FirstOrDefault();
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (dbPlayer.Player.IsInVehicle || key != Key.E) return false;

            AmmoArmorie ammoArmorie = GetByPosition(dbPlayer.Player.Position);
            if (ammoArmorie == null || !dbPlayer.Team.IsGangsters() || dbPlayer.Team.Id != ammoArmorie.TeamId) return false;

            MenuManager.Instance.Build(PlayerMenu.AmmoArmorieMenu, dbPlayer).Show(dbPlayer);
            return true;
        }
    }

    public sealed class AmmoArmoryItemModule : SqlModule<AmmoArmoryItemModule, AmmoArmorieItem, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `team_ammoarmories_items` ORDER BY id;";
        }
    }
}

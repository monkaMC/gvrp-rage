using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GVRP.Module.Menu;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Teams.AmmoPackageOrder
{
    public class AmmoPackageOrderModule : Module<AmmoPackageOrderModule>
    {
        public static Vector3 LoadPosition = new Vector3(-7225.02, -308.183, 6.15816);
        public static int AmmoChestToPackageMultipliert = 10;
        public static int AmmoOrderSourcePrice = 1600;

        public override bool Load(bool reload = false)
        {
            MenuManager.Instance.AddBuilder(new AmmoPackageOrderMenuBuilder());
            return base.Load(reload);
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (dbPlayer.Player.IsInVehicle) return false;

            if (dbPlayer.Team.Id != (int)teams.TEAM_NNM && dbPlayer.Team.Id != (int)teams.TEAM_HUSTLER && dbPlayer.Team.Id != (int)teams.TEAM_BRATWA && dbPlayer.Team.Id != (uint)teams.TEAM_ICA) return false;
            if (dbPlayer.TeamRank < 9) return false;

            if (dbPlayer.Player.Position.DistanceTo(LoadPosition) < 1.5f)
            {
                MenuManager.Instance.Build(PlayerMenu.AmmoPackageOrderMenu, dbPlayer).Show(dbPlayer);
                return true;
            }
            return false;
        }

        public override void OnServerBeforeRestart()
        {
            // Sonntags 16 Uhr wende
            if(DateTime.Now.DayOfWeek == DayOfWeek.Sunday && DateTime.Now.Hour == 15)
            {
                foreach (DbTeam dbTeam in TeamModule.Instance.GetAll().Values.ToList())
                {
                    if (dbTeam.IsGangsters() && dbTeam.TeamMetaData != null)
                    {
                        dbTeam.TeamMetaData.OrderedPackets = 0;
                        dbTeam.TeamMetaData.SaveOrderedPackets();
                    }
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Handler;
using GVRP.Module.Assets.Hair;
using GVRP.Module.Assets.HairColor;
using GVRP.Module.Assets.Tattoo;
using GVRP.Module.Barber.Windows;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Customization;
using GVRP.Module.GTAN;
using GVRP.Module.Logging;
using GVRP.Module.Menu;
using GVRP.Module.Players;

using GVRP.Module.Players.Db;
using GVRP.Module.Tattoo.Windows;
using GVRP.Module.Teams.Apps;
using GVRP.Module.Teams.Permission;
using GVRP.Module.Vehicles;

namespace GVRP.Module.Swat
{
    public sealed class SwatModule : Module<SwatModule>
    {
        public override void OnPlayerFirstSpawn(DbPlayer dbPlayer)
        {
            if (dbPlayer.SwatDuty == 1)
            {
                dbPlayer.SetSwatDuty(true);
            }
        }
    }

    public static class SwatPlayerExtension
    {
        public static bool HasSwatRights(this DbPlayer dbPlayer)
        {
            return dbPlayer.Swat > 0 || dbPlayer.TeamId == (int)teams.TEAM_SWAT;
        }

        public static bool HasSwatLeaderRights(this DbPlayer dbPlayer)
        {
            return dbPlayer.TeamId == (int)teams.TEAM_SWAT && dbPlayer.TeamRank >= 11;
        }

        public static void SetSwatRights(this DbPlayer dbPlayer, bool leaderrights)
        {
            dbPlayer.Swat = leaderrights ? 2 : 1;
            dbPlayer.Save();
            return;
        }

        public static void RemoveSwatRights(this DbPlayer dbPlayer)
        {
            dbPlayer.Swat = 0;
            if(dbPlayer.TeamId == (int)teams.TEAM_SWAT)
            {
                dbPlayer.SetTeam((int)teams.TEAM_CIVILIAN);
                dbPlayer.SetTeamRankPermission(false, 0, false, "");
            }
            dbPlayer.Save();
            return;
        }

        public static void SetSwatDuty(this DbPlayer dbPlayer, bool duty)
        {
            if (duty)
            {
                dbPlayer.SetData("swatOld_team", dbPlayer.TeamId);
                dbPlayer.SetData("swatOld_rang", dbPlayer.TeamRank);
                dbPlayer.SetData("swatOld_rights_manage", dbPlayer.TeamRankPermission.Manage);
                dbPlayer.SetData("swatOld_rights_bank", dbPlayer.TeamRankPermission.Bank);
                dbPlayer.SetData("swatOld_rights_inv", dbPlayer.TeamRankPermission.Inventory);
                dbPlayer.SetData("swatOld_rights_title", dbPlayer.TeamRankPermission.Title);

                dbPlayer.SetTeam((int)teams.TEAM_SWAT, false);
                dbPlayer.SetTeamRankPermission(true, dbPlayer.HasSwatLeaderRights() ? 2 : 0, true, "");
                dbPlayer.SendNewNotification("Swatdienst angetreten!");
                dbPlayer.SetDuty(true);
                dbPlayer.SwatDuty = 1;
                dbPlayer.Player.TriggerEvent("updateDuty", true);
            }
            else
            {
                // Revert Old Data
                dbPlayer.SetTeam(dbPlayer.GetData("swatOld_team"), false);
                dbPlayer.SetTeamRankPermission((bool)dbPlayer.GetData("swatOld_rights_bank"), (int)dbPlayer.GetData("swatOld_rights_manage"), (bool)dbPlayer.GetData("swatOld_rights_inv"), (string)dbPlayer.GetData("swatOld_rights_title"));
                dbPlayer.TeamRank = dbPlayer.GetData("swatOld_rang");

                dbPlayer.SendNewNotification("Swatdienst beendet!");
                dbPlayer.SetDuty(true);
                dbPlayer.SwatDuty = 0;
                dbPlayer.Player.TriggerEvent("updateDuty", true);
                dbPlayer.RemoveWeapons();
            }

            dbPlayer.UpdateApps();
            ComponentManager.Get<TeamListApp>().SendTeamMembers(dbPlayer);
        }

        public static bool IsSwatDuty(this DbPlayer dbPlayer)
        {
            if (dbPlayer.SwatDuty == 1 && dbPlayer.TeamId == (int)teams.TEAM_SWAT) return true;
            else return false;
        }
    }
}
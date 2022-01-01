using System;
using System.Runtime.CompilerServices;
using GVRP.Handler;
using GVRP.Module.Commands;
using GVRP.Module.Injury;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.RemoteEvents
{
    public static class PlayerRemoteEventPermissions
    {

        public static bool CanAccessRemoteEvent(this DbPlayer dbPlayer, [CallerMemberName] string callerName = "")
        {
            if (dbPlayer == null) return false;
            if (!dbPlayer.IsValid()) return false;
            if (Configurations.Configuration.Instance.DevMode) return true;
            var methodName = callerName.ToLower();
            var remoteEventPermission = RemoteEventPermissions.Instance[methodName];
            if (remoteEventPermission == null) return true;
            if (remoteEventPermission.TeamId != null && dbPlayer.TeamId != remoteEventPermission.TeamId) return false;
            if (remoteEventPermission.PlayerRankPermission && !dbPlayer.Rank.CanAccessEvent(methodName))
                return false;
            if (!remoteEventPermission.AllowedDeath && dbPlayer.isInjured()) return false;
            if (!remoteEventPermission.AllowedOnCuff && dbPlayer.IsCuffed) return false;
            if (!remoteEventPermission.AllowedOnTied && dbPlayer.IsTied) return false;
            return true;
        }
    }
}
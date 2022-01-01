using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.Injury;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Teams.Blacklist
{
    public class BlacklistEvents : Script
    {
        [RemoteEvent]
        public void SetBlacklist(Client player, string returnstring)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            
            DbPlayer target = Players.Players.Instance.FindPlayer(returnstring);
            if (target != null && target.IsValid() && target.isInjured() && target.Level >= 5)
            {
                dbPlayer.SetData("blsetplayer", target.Id);
                
                Module.Menu.MenuManager.Instance.Build(GVRP.Module.Menu.PlayerMenu.BlacklistTypeMenu, dbPlayer).Show(dbPlayer);
            }
            return;
        }
    }
}

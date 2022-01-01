using System;
using GTANetworkAPI;
using GVRP.Module.Chat;
using GVRP.Module.Logging;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Strings;

namespace GVRP.Module.Admin
{
    public class AdminModule : Module<AdminModule>
    {
        public override bool OnPlayerDeathBefore(DbPlayer dbPlayer, NetHandle killer, uint weapon)
        {
            if (killer.GetEntityType() != EntityType.Player || killer.ToPlayer() == dbPlayer.Player) return false;
            var xKiller = killer.ToPlayer();
            var iKiller = xKiller.GetPlayer();

            if (iKiller == null) return false;

            if (iKiller.Level <= 3 && dbPlayer.Player.Name != xKiller.Name)
            {
              //  dbPlayer.SendNewNotification( StringsModule.Instance["KILL_WILL_NOTICE"]);
                Players.Players.Instance.SendMessageToAuthorizedUsers("deathlog",
                    "Neulingskill: " + iKiller.Player.Name + " hat " + dbPlayer.Player.Name + " getoetet!");
            }
            string killerweapon = Convert.ToString((WeaponHash)weapon) != "" ? Convert.ToString((WeaponHash)weapon) : "unbekannt";
            
            // Reset killer informations
            dbPlayer.ResetData("killername");
            dbPlayer.ResetData("killerweapon");
            dbPlayer.SetData("killername", iKiller.Player.Name.ToString());
            dbPlayer.SetData("killerweapon", killerweapon);            

            // Deathlog
            LogHandler.LogDeath(dbPlayer.Player.Name, iKiller.Player.Name, killerweapon);
            return false;
        }
    }
}
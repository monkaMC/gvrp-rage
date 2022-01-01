using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Handler;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.FIB
{
    public class FIBEvents : Script
    {
        public static DiscordHandler Discord = new DiscordHandler();

        [RemoteEvent]
        public void FIBSetUnderCoverName(Client player, string returnstring)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (dbPlayer.TeamId != 5)
            {
                Discord.SendMessage(dbPlayer.GetName() + " hat einen Nice Try versucht NSAClonePlayer @here");
                player.Ban("Bye du HS!");
                return;
            }

            if (!returnstring.Contains("_") || returnstring.Length < 3)
            {
                dbPlayer.SendNewNotification("Bitte Format einhalten: Max_Mustermann!");
                return;
            }

            string[] ucName = returnstring.Split("_");

            if(ucName.Length < 2 || ucName[0].Length < 3 || ucName[1].Length < 3)
            {
                dbPlayer.SendNewNotification("Bitte Format einhalten: Max_Mustermann!");
                return;
            }
            
            dbPlayer.SendNewNotification($"Sie sind nun als {ucName[0]}_{ucName[1]} im Undercover dienst!");
            dbPlayer.Team.SendNotification($"{dbPlayer.GetName()} ist nun als {ucName[0]}_{ucName[1]} im Undercover dienst!", 5000, 10);

            dbPlayer.SetUndercover(ucName[0], ucName[1]);
            return;
        }
    }
}

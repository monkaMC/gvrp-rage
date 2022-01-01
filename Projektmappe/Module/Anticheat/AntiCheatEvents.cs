using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using GTANetworkAPI;
using GVRP.Handler;
using GVRP.Module.Business;
using GVRP.Module.Business.FuelStations;
using GVRP.Module.GTAN;
using GVRP.Module.Logging;
using GVRP.Module.Players;
namespace GVRP.Module.Anticheat

{
    class AntiCheatEvents : Script
    {

        [RemoteEvent]
        public void __ragemp_cheat_detected(Client player, int cheatCode)
        {

            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null) return;

            string l_Cheat = "Cheat Engine";

            switch (cheatCode)
            {
                case 0:
                case 1:
                    l_Cheat = "Cheat Engine";
                    break;
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                    l_Cheat = "Externer Hack";
                    break;
                case 7:
                    l_Cheat = "Mod-Menü";
                    break;
                case 8:
                case 9:
                    l_Cheat = "Speed Hack";
                    break;
                case 11:
                    l_Cheat = "Nutzung von Sandboxie";
                    break;
                default:
                    break;
            }

            Logger.AddActionLogg(dbPlayer.Id, cheatCode);
            Players.Players.Instance.SendMessageToAuthorizedUsers("log", $"Dringender Anticheat-Verdacht: {dbPlayer.Player.Name} ({l_Cheat}) gegeben.");
        }
    }
}

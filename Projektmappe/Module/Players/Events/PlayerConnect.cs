using GTANetworkAPI;
using System;
using System.Threading.Tasks;
using GVRP.Module.Configurations;
using GVRP.Module.Logging;
using GVRP.Module.Players.Db;
using GVRP.Module.Tasks;

namespace GVRP.Module.Players.Events
{
    public class PlayerConnect : Script
    {
        //[ServerEvent(Event.PlayerConnected)]
        public static void OnPlayerConnected(Client player)
        {

            // Console.WriteLine(player.Name);
            if (player.Name == "WeirdNewbie")
            {
                player.Kick("Namen ändern!");
                return;
            }

            player.Position = new Vector3(17.4809, 637.872, 210.595);
            var crumbs = player.Name.Split('_');
            if (crumbs.Length > 1)
            {

            }
            else
            {
                player.Kick("Name nicht richtig gesetzt!");
                return;
            }

            //Unsichtbar, Freeze
            player.Transparency = 0;
            player.Dimension = 1337; // There is no PlayerID at this point, so count it up
            player.TriggerEvent("OnPlayerReady");

            // Alreade logged in, delete PlayerObject
            var dbPlayer = player.GetPlayer();
            if (dbPlayer != null || dbPlayer.IsValid(true))
            {
                try
                {
                    Players.Instance.players.RemoveAll(p => p == dbPlayer || p.Player.Name == player.Name);
                }
                catch (System.Exception e)
                {
                    Logging.Logger.Crash(e);
                }
            }

            //     if (!Configuration.Instance.IsServerOpen)
            //     {
            //         player.SendNotification("Server wird heruntergefahren");
            //         player.Kick();
            //         return;
            //      }




            if (!Configurations.Configuration.Instance.DevMode)
            {
                PlayerAsis.Instance.CheckAsi(player);
            }

            if (!Configuration.Instance.Ptr && SocialBanHandler.Instance.IsPlayerSocialBanned(player))
            {
                player.SendNotification("Bitte melde dich beim Support im Teamspeak (Social-Ban)");
                player.Kick();
                return;
            }

            Modules.Instance.OnClientConnected(player);

            player.SetData("loginStatusCheck", 1);

            //player.Freeze(true);
            player.TriggerEvent("freezePlayer", true);

            SynchronizedTaskManager.Instance.Add(new PlayerLoginTask(player));


        }
    }
}

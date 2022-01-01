using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Commands;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Windows;
using static GVRP.Module.Chat.Chats;

namespace GVRP.Module.Einreiseamt
{
    public class EinreiseamtModule : Module<EinreiseamtModule>
    {
        public static Vector3 PositionPC1 = new Vector3(-1083.455, -2820.458, 27.70875);
        public static Vector3 PositionPC2 = new Vector3(-1073.432, -2820.561, 27.70875);
        public static Vector3 PositionPC3 = new Vector3(-1067.761, -2811.134, 27.70873);
        public static Vector3 PositionPC4 = new Vector3(-1078.073, -2810.812, 27.70873);
        public static Vector3 PositionPC5 = new Vector3(-1080.59, -2832.646, 27.70875);
        public static Vector3 PositionPC6 = new Vector3(-1082.448, -2831.652, 27.70875);
        public static Vector3 PositionPC7 = new Vector3(-1085.726, -2829.788, 27.70875);
        public static Vector3 PositionPC8 = new Vector3(-1087.892, -2828.37, 27.70875);

        public override bool Load(bool reload = false)
        {
            MenuManager.Instance.AddBuilder(new EinreiseAmtMenuBuilder());
            return reload;
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (dbPlayer.Player.IsInVehicle) return false;
            if (key == Key.E)
            {
                if (!dbPlayer.IsEinreiseAmt())
                {

                    return false;
                }
                if (dbPlayer.Player.Position.DistanceTo(PositionPC1) < 1.5f || dbPlayer.Player.Position.DistanceTo(PositionPC2) < 1.5f || dbPlayer.Player.Position.DistanceTo(PositionPC3) < 1.5f || dbPlayer.Player.Position.DistanceTo(PositionPC4) < 1.5f || dbPlayer.Player.Position.DistanceTo(PositionPC5) < 1.5f || dbPlayer.Player.Position.DistanceTo(PositionPC6) < 1.5f || dbPlayer.Player.Position.DistanceTo(PositionPC7) < 1.5f || dbPlayer.Player.Position.DistanceTo(PositionPC8) < 1.5f)
                {
                    ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Einreiseamt-Formular", Callback = "EinreiseAmtPlayer", Message = "Geben Sie einen Namen ein" });
                    return true;
                }
            }
            return false;
        }

        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandeinreiseamt(Client player)
        {
            var iPlayer = player.GetPlayer();
            if (iPlayer == null || !iPlayer.IsValid()) return;
            if (!iPlayer.IsEinreiseAmt()) return;

            string names = "";
            foreach(DbPlayer xPlayer in Players.Players.Instance.GetValidPlayers().Where(p => p.NeuEingereist()))
            {
                names += xPlayer.GetName() + " ,";
            }

            iPlayer.SendNewNotification("Aktuelle Spieler: " + names);
        }


        [CommandPermission(PlayerRankPermission = true)]
        [Command]
        public void Commandemember(Client player)
        {
            var iPlayer = player.GetPlayer();
            if (iPlayer == null || !iPlayer.IsValid()) return;
            if (!iPlayer.IsEinreiseAmt()) return;

            string names = "";
            foreach (DbPlayer xPlayer in Players.Players.Instance.GetValidPlayers().Where(p => p.IsEinreiseAmt()))
            {
                names += xPlayer.GetName() + " ,";
            }

            iPlayer.SendNewNotification("Aktuelle Einreisebeamte: " + names);
        }

        [CommandPermission(PlayerRankPermission = true, AllowedDeath = true)]
        [Command(GreedyArg = true)]
        public void Commande(Client player, string message)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsEinreiseAmt()) return;

            foreach (DbPlayer iPlayer in Players.Players.Instance.GetValidPlayers().Where(p => p.IsEinreiseAmt()))
            {
                iPlayer.SendNewNotification("[EA] " + dbPlayer.GetName() + ": " + message);
            }
        }


        [CommandPermission(PlayerRankPermission = true, AllowedDeath = true)]
        [Command(GreedyArg = true)]
        public void Commandgiveea(Client player, string destplayer)
        {
            Logging.Logger.Debug("command giveea " + destplayer);
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || !dbPlayer.IsEinreiseAmt() || dbPlayer.Team.Id != (int)teams.TEAM_GOV || dbPlayer.TeamRank < 8) return;

            DbPlayer foundPlayer = Players.Players.Instance.FindPlayer(destplayer);
            if (foundPlayer == null || !foundPlayer.IsValid()) return;

            if(foundPlayer.Einwanderung == 1)
            {
                foundPlayer.Einwanderung = 0;
                foundPlayer.SendNewNotification($"{dbPlayer.GetName()} hat ihnen die Einreiselizenz entzogen!");
                dbPlayer.SendNewNotification($"Sie haben {foundPlayer.GetName()} die Einreiselizenz entzogen!");
                return;
            }
            else
            {
                foundPlayer.Einwanderung = 1;
                foundPlayer.SendNewNotification($"{dbPlayer.GetName()} hat ihnen die Einreiselizenz ausgestellt!");
                dbPlayer.SendNewNotification($"Sie haben {foundPlayer.GetName()} die Einreiselizenz ausgestellt!");
                return;
            }
        }
    }

    public static class EinreisePlayerExtension
    {
        public static bool IsEinreiseAmt(this DbPlayer dbPlayer)
        {
            return dbPlayer.Einwanderung == 1;
        }

        public static void EinreiseSpawn(this DbPlayer dbPlayer)
        {
            dbPlayer.SendNewNotification("Neuer Spieler im Einreiseamt | Insg: " + Players.Players.Instance.GetValidPlayers().Where(p => p.NeuEingereist()).Count());
        }

        public static bool NeuEingereist(this DbPlayer dbPlayer)
        {
            return dbPlayer.hasPerso[0] == 0;
        }
    }
}

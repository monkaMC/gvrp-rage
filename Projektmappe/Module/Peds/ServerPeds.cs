using System;
using System.Collections.Generic;
using GVRP;
using GVRP.Module.Logging;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP
{
    public class ServerPeds
    {
        public static ServerPeds Instance { get; } = new ServerPeds();
        private readonly List<ServerPed> serverPedList;

        private ServerPeds()
        {
            serverPedList = new List<ServerPed>();
        }

        public void Add(ServerPed serverPed)
        {
            serverPedList.Add(serverPed);
        }

        public void LoadForPlayer(DbPlayer iPlayer)
        {
            foreach (var serverPed in serverPedList)
            {
                //iPlayer.player.TriggerEvent("SPAWN_PED", serverPed.Model, serverPed.Position, serverPed.Heading);

                //CREATE_PED(int pedType, Hash modelHash, float x, float y, float z,  float heading, BOOL isNetwork, BOOL thisScriptCheck)
                iPlayer.Player.SendNative(0xD49F9B0955C367DE, 28, serverPed.Position.X, serverPed.Position.Y,
                    serverPed.Position.Z, serverPed.Heading, false, false);
                Logger.Debug("PED SPAWNED FOR PLAYER!");
            }
        }
    }
}
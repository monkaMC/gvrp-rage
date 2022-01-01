using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using GVRP.Module.Players.Db;

namespace GVRP.Module.NpcSpawner
{
    public class Npc
    {
        public PedHash PedHash { get; set; }
        public Vector3 Position { get; set; }
        public float Heading { get; set; }
        public UInt32 Dimension { get; set; }

        public Npc(PedHash pedHash, Vector3 position, float heading, uint dimension)
        {
            PedHash = pedHash;
            Position = position;
            Heading = heading;
            Dimension = dimension;

            // Add To NPCList
            Main.ServerNpcs.Add(this);
            foreach(DbPlayer dbPlayer in Players.Players.Instance.GetValidPlayers())
            {
                dbPlayer.Player.TriggerEvent("loadNpc", PedHash, Position.X, Position.Y, Position.Z, Heading, Dimension);
            }
        }
    }
}

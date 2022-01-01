using GTANetworkAPI;
using System;
using System.Collections.Generic;
using GVRP.Module.Injury;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Service
{
    public class Service
    {
        public Vector3 Position { get; }
        public string Message { get; set; }
        public uint TeamId { get; }
        public DbPlayer Player { get; }
        public HashSet<string> Accepted { get; }
        public string Telnr { get; }

        public DateTime Created { get; set; }
        
        public Service(Vector3 position, string message, uint teamId, DbPlayer iPlayer, string description = "", string telefon = "0")
        {
            Position = position;
            Message = message;
            TeamId = teamId;
            Player = iPlayer;
            Telnr = telefon;
            Accepted = new HashSet<string>();
            Created = DateTime.Now;
        }
    }
}
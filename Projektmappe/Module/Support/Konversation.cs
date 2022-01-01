using System;
using System.Text.RegularExpressions;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Support
{
    public class Konversation
    {
        public DbPlayer Player { get; set; }
        public bool Receiver { get; set; }
        public string Message { get; set; }
        public DateTime Created_at { get; set; }

        public Konversation(DbPlayer iPlayer, bool receiver, string message)
        {
            Player = iPlayer;
            Receiver = receiver;
            Message = Regex.Replace(message, @"[^a-zA-Z0-9\s]", ""); ;
            Created_at = DateTime.Now;
        }
    }
}
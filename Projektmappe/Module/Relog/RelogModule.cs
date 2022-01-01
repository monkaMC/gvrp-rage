//using System;
//using System.Collections.Generic;
//using GTANetworkAPI;
//using GVRP.Module.Configurations;
//using GVRP.Module.Players.Db;

//namespace GVRP.Module.Relog
//{
//    public class RelogModule : Module<RelogModule>
//    {
//        private readonly Dictionary<string, DateTime> relogs;

//        public RelogModule()
//        {
//            relogs = new Dictionary<string, DateTime>();
//        }

//        /*public override bool OnClientConnected(Client client)
//        {
//            if (Configuration.Instance.DevMode) return false;
//            if (!relogs.ContainsKey(client.Name)) return true;
//            var lastRelog = relogs[client.Name];
//            if (lastRelog < DateTime.Now)
//            {
//                // Remove expired relog
//                relogs.Remove(client.Name);
//            }
//            else if (!Main.devmode)
//            {
//                client.SendNotification("Bitte warte 10 Sekunden.");
//                client.
//                Bitte warte 10 Sekunden.");
//                return false;
//            }

//            return true;
//        }*/

//        public override void OnPlayerDisconnected(DbPlayer dbPlayer, string reason)
//        {
//            if (relogs.ContainsKey(dbPlayer.GetName()))
//            {
//                relogs[dbPlayer.GetName()] = DateTime.Now.AddSeconds(10);
//            }
//            else
//            {
//                relogs.Add(dbPlayer.GetName(), DateTime.Now.AddSeconds(10));
//            }
//        }
//    }
//}
using System.Collections.Generic;
using GTANetworkAPI;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.Players;
using Newtonsoft.Json;
using System;
using MySql.Data.MySqlClient;
using GVRP.Module.Chat;
using GVRP.Module.Configurations;
using GVRP.Module.Items;

using GVRP.Module.Players.Db;
using GVRP.Module.Players.Phone;
using GVRP.Module.Players.PlayerAnimations;

namespace GVRP.Module.Messenger.App
{
    public class MessengerApp : SimpleApp
    {
        public MessengerApp() : base("MessengerApp")
        {
        }

        [RemoteEvent]
        public void sendMessage(Client client, uint number, string messageContent)
        {
        }

        [RemoteEvent]
        public void forwardMessage(Client client, uint number, uint messageId)
        {
            // Forwars selected message in "original" and fake-proof. TBD later.
        }
    }
}
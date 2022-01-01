using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.Players;
using Newtonsoft.Json;
using GVRP.Module.Menu;
using GVRP.Module.RemoteEvents;

namespace GVRP.Module.Jobs.Taxi.App
{
    public class TaxiApp : SimpleApp
    {
        public TaxiApp() : base("TaxiApp")
        {
        }

        private class TaxiFound
        {
            [JsonProperty(PropertyName = "id")] public uint PlayerId { get; }
            [JsonProperty(PropertyName = "name")] public string Name { get; }
            [JsonProperty(PropertyName = "number")] public int Number { get; }
            [JsonProperty(PropertyName = "price")] public int Price { get; }

            public TaxiFound(uint playerId, string name, int number, int price)
            {
                PlayerId = playerId;
                Name = name;
                Number = number;
                Price = price;
            }
        }

        [RemoteEvent]
        public void requestTaxiList(Client player)
        {
            SendTaxiList(player);
        }
        
        private void SendTaxiList(Client player)
        {
            var taxiList = new List<TaxiFound>();
            var Users = Players.Players.Instance.GetValidPlayers();

            for (int index = 0; index < Users.Count; index++)
            {
                if (!Users[index].IsValid()) continue;
                if (Users[index].HasData("taxi"))
                {
                    taxiList.Add(new TaxiFound(Users[index].Id, Users[index].GetName(), (int)Users[index].handy[0], Users[index].GetData("taxi")));
                }
            }
            Console.WriteLine("Nigga not work??");
            TriggerEvent(player, "responseTaxiList", NAPI.Util.ToJson(taxiList));
            Console.WriteLine("Nigga not work2");
        }

        
        [RemoteEventPermission]
        [RemoteEvent]
        public void requestTaxiDriver(Client client, string driverName, string message, int preis)
        {
            var dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessRemoteEvent() || !dbPlayer.IsValid()) return;
            var driverDbPlayer = Players.Players.Instance.FindPlayer(driverName);
            if (!driverDbPlayer.IsValid()) return;
            if (driverDbPlayer.Id == dbPlayer.Id) return;

            if (driverDbPlayer.HasData("taxi") &&
                driverDbPlayer.Lic_Taxi[0] == 1)
            {
                // taxifahrer gefunden yay
                driverDbPlayer.SendNewNotification(
                        "Sie haben eine Taxianfrage von " + dbPlayer.GetName() + 
                        " (" + dbPlayer.ForumId + ") Ort: " + message +", benutzen Sie /acceptservice um diese anzunehmen!", duration: 20000);
                dbPlayer.SendNewNotification("Anfrage an den Taxifahrer wurde gestellt!");
                dbPlayer.SetData("taxi_request", driverDbPlayer.GetName());
                Console.WriteLine("Kek lol not work");
                dbPlayer.SetData("taxi_request_price", preis);
                Console.WriteLine("taxi dumb nigga");
                return;
            }
        }
    }
}
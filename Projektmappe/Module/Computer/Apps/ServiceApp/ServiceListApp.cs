using GTANetworkAPI;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.Players;
using GVRP.Module.Service;

namespace GVRP.Module.Computer.Apps.ServiceApp
{
    public class ServiceListApp : SimpleApp
    {
        public ServiceListApp() : base("ServiceListApp") { }

        [RemoteEvent]
        public async void requestOpenServices(Client client)
        {
            
                var dbPlayer = client.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid()) return;
                if (!dbPlayer.IsACop() && dbPlayer.TeamId != (int)teams.TEAM_MEDIC && dbPlayer.TeamId != (int)teams.TEAM_DRIVINGSCHOOL && dbPlayer.TeamId != (int)teams.TEAM_DPOS && dbPlayer.TeamId != (int)teams.TEAM_NEWS && dbPlayer.TeamId != (int)teams.TEAM_LSC && dbPlayer.TeamId != (int) teams.TEAM_GOV) return;

                List<serviceObject> serviceList = new List<serviceObject>();
                var teamServices = ServiceModule.Instance.GetAvailableServices(dbPlayer);

                foreach (var service in teamServices)
                {
                    string accepted = string.Join(',', service.Accepted);

                    serviceList.Add(new serviceObject() { id = (int)service.Player.Id, name = service.Player.GetName(), message = ServiceModule.Instance.GetSpecialDescriptionForPlayer(dbPlayer, service), posX = service.Position.X, posY = service.Position.Y, posZ = service.Position.Z, accepted = accepted, telnr = service.Telnr });
                }

                var serviceJson = NAPI.Util.ToJson(serviceList);
                TriggerEvent(client, "responseOpenServiceList", serviceJson);
            
        }

        [RemoteEvent]
        public async void acceptOpenService(Client client, string name)
        {
            
                var dbPlayer = client.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid()) return;
                if (!dbPlayer.IsACop() && dbPlayer.TeamId != (int)teams.TEAM_MEDIC && dbPlayer.TeamId != (int)teams.TEAM_DRIVINGSCHOOL && dbPlayer.TeamId != (int)teams.TEAM_DPOS && dbPlayer.TeamId != (int)teams.TEAM_NEWS && dbPlayer.TeamId != (int)teams.TEAM_LSC && dbPlayer.TeamId != (int) teams.TEAM_GOV) return;

                var findplayer = Players.Players.Instance.FindPlayer(name);
                if (findplayer == null) return;

                bool response = ServiceModule.Instance.Accept(dbPlayer, findplayer);

                dbPlayer.SendNewNotification(response ? "Sie haben einen Service entgegengenommen!" : "Der Service konnte nicht entgegengenommen werden!");
                findplayer.SendNewNotification("Ihr Service wurde entgegen genommen!");

                dbPlayer.Team.SendNotification($"{dbPlayer.GetName()} hat den Notruf von {findplayer.GetName()} angenommen");
            
        }

        public class serviceObject
        {
            [JsonProperty(PropertyName = "id")]
            public int id { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string name { get; set; }

            [JsonProperty(PropertyName = "message")]
            public string message { get; set; }
            
            [JsonProperty(PropertyName = "posX")]
            public float posX { get; set; }

            [JsonProperty(PropertyName = "posY")]
            public float posY { get; set; }

            [JsonProperty(PropertyName = "posZ")]
            public float posZ { get; set; }

            /*[JsonProperty(PropertyName = "accepted")]
            public HashSet<string> accepted { get; set; }*/

            [JsonProperty(PropertyName = "accepted")]
            public string accepted { get; set; }
            [JsonProperty(PropertyName ="telnr")]
            public string telnr { get; set; }
        }
    }
}

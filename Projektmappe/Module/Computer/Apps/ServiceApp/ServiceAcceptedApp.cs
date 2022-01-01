using GTANetworkAPI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.Players;
using GVRP.Module.Service;
using static GVRP.Module.Computer.Apps.ServiceApp.ServiceListApp;

namespace GVRP.Module.Computer.Apps.ServiceApp
{
    public class ServiceAcceptedApp : SimpleApp
    {
        public ServiceAcceptedApp() : base("ServiceAcceptedApp") { }

        [RemoteEvent]
        public async void RequestTeamServiceList(Client client)
        {
            
                var dbPlayer = client.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid()) return;
                if (!dbPlayer.IsACop() && dbPlayer.TeamId != (int)teams.TEAM_MEDIC && dbPlayer.TeamId != (int)teams.TEAM_DRIVINGSCHOOL && dbPlayer.TeamId != (int)teams.TEAM_DPOS && dbPlayer.TeamId != (int)teams.TEAM_NEWS && dbPlayer.TeamId != (int)teams.TEAM_LSC && dbPlayer.TeamId != (int) teams.TEAM_GOV) return;

                List<serviceObject> serviceList = new List<serviceObject>();
                var teamServices = ServiceModule.Instance.GetAcceptedTeamServices(dbPlayer);

                foreach (var service in teamServices)
                {
                    string accepted = string.Join(',', service.Accepted);

                    serviceList.Add(new serviceObject() { id = (int)service.Player.Id, name = service.Player.GetName(), message = ServiceModule.Instance.GetSpecialDescriptionForPlayer(dbPlayer, service), posX = service.Position.X, posY = service.Position.Y, posZ = service.Position.Z, accepted = accepted, telnr = service.Telnr });
                }

                var serviceJson = NAPI.Util.ToJson(serviceList);
                TriggerEvent(client, "responseTeamServiceList", serviceJson);
            
        }
    }
}

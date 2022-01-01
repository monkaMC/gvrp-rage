using GTANetworkAPI;
using System.Collections.Generic;
using System.Linq;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.Players;
using GVRP.Module.Service;
using static GVRP.Module.Computer.Apps.ServiceApp.ServiceListApp;

namespace GVRP.Module.Computer.Apps.ServiceApp
{
    public class ServiceOwnApp : SimpleApp
    {
        public ServiceOwnApp() : base("ServiceOwnApp") { }

        [RemoteEvent]
        public void requestOwnServices(Client client)
        {
            var dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (!dbPlayer.IsACop() && dbPlayer.TeamId != (int)teams.TEAM_MEDIC && dbPlayer.TeamId != (int)teams.TEAM_DRIVINGSCHOOL && dbPlayer.TeamId != (int)teams.TEAM_DPOS && dbPlayer.TeamId != (int)teams.TEAM_NEWS && dbPlayer.TeamId != (int)teams.TEAM_LSC && dbPlayer.TeamId != (int) teams.TEAM_GOV) return;

            List<serviceObject> serviceList = new List<serviceObject>();
            var acceptedServices = ServiceModule.Instance.GetAcceptedServices(dbPlayer);

            foreach (var service in acceptedServices)
            {
                string accepted = string.Join(',', service.Accepted);

                serviceList.Add(new serviceObject() { id = (int)service.Player.Id, name = service.Player.GetName(), message = ServiceModule.Instance.GetSpecialDescriptionForPlayer(dbPlayer, service), posX = service.Position.X, posY = service.Position.Y, posZ = service.Position.Z, accepted = accepted, telnr = service.Telnr });
            }

            var serviceJson = NAPI.Util.ToJson(serviceList);
            TriggerEvent(client, "responseOwnServiceList", serviceJson);
        }

        [RemoteEvent]
        public void finishOwnAcceptedService(Client client, string creator)
        {
            var dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (!dbPlayer.IsACop() && dbPlayer.TeamId != (int)teams.TEAM_MEDIC && dbPlayer.TeamId != (int)teams.TEAM_DRIVINGSCHOOL && dbPlayer.TeamId != (int)teams.TEAM_DPOS && dbPlayer.TeamId != (int)teams.TEAM_NEWS && dbPlayer.TeamId != (int)teams.TEAM_LSC && dbPlayer.TeamId != (int) teams.TEAM_GOV) return;

            var findplayer = Players.Players.Instance.FindPlayer(creator);
            if (findplayer == null) return;

            bool response = ServiceModule.Instance.Cancel(dbPlayer, findplayer, dbPlayer.TeamId);

            if (response)
            {
                findplayer.ResetData("service");

                findplayer.SendNewNotification("Ihr Service wurde bearbeitet und somit geschlossen!");
                dbPlayer.SendNewNotification($"Sie haben den Service von {findplayer.GetName()} beendet!");
            }
            else
            {
                dbPlayer.SendNewNotification("Der Service konnte nicht beendet werden!");
            }
        }
    }
}

using GTANetworkAPI;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.Players;
using GVRP.Module.Teams;

namespace GVRP.Module.Service
{
    public class ServiceApp : SimpleApp
    {
        public ServiceApp() : base("ServiceRequestApp") { }

        [RemoteEvent]
        public void cancelServiceRequest(Client client)
        {
            var dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (dbPlayer.HasData("service") && dbPlayer.GetData("service") > 0)
            {
                bool status = ServiceModule.Instance.CancelOwnService(dbPlayer, (uint)dbPlayer.GetData("service"));

                if (status)
                {
                    switch (dbPlayer.GetData("service"))
                    {
                        case 1:
                            dbPlayer.ResetData("service");
                            TeamModule.Instance[(int)teams.TEAM_POLICE].SendNotification($"Der Notruf von { dbPlayer.GetName() } ({ dbPlayer.ForumId }) wurde abgebrochen!");
                            dbPlayer.SendNewNotification("Sie haben den Notruf abgebrochen!");

                            break;
                        case 7:
                            dbPlayer.ResetData("service");
                            TeamModule.Instance[(int)teams.TEAM_MEDIC].SendNotification($"Der Notruf von { dbPlayer.GetName() } ({ dbPlayer.ForumId }) wurde abgebrochen!");
                            dbPlayer.SendNewNotification("Sie haben den Notruf abgebrochen!");

                            break;
                        case 3:
                            dbPlayer.ResetData("service");
                            TeamModule.Instance[(int)teams.TEAM_DRIVINGSCHOOL].SendNotification($"Die Anfrage von { dbPlayer.GetName() } ({ dbPlayer.ForumId }) wurde abgebrochen!");
                            dbPlayer.SendNewNotification("Sie haben die Anfrage abgebrochen!");

                            break;
                        case 16:
                            dbPlayer.ResetData("service");
                            TeamModule.Instance[(int)teams.TEAM_DPOS].SendNotification($"Die Anfrage von { dbPlayer.GetName() } ({ dbPlayer.ForumId }) wurde abgebrochen!");
                            dbPlayer.SendNewNotification("Sie haben die Anfrage abgebrochen!");

                            break;
                        case 4:
                            dbPlayer.ResetData("service");
                            TeamModule.Instance[(int)teams.TEAM_NEWS].SendNotification($"Die Anfrage von { dbPlayer.GetName() } ({ dbPlayer.ForumId }) wurde abgebrochen!");
                            dbPlayer.SendNewNotification("Sie haben die Anfrage abgebrochen!");

                            break;
                        case 26:
                            dbPlayer.ResetData("service");
                            TeamModule.Instance[(int)teams.TEAM_LSC].SendNotification($"Die Anfrage von { dbPlayer.GetName() } ({ dbPlayer.ForumId }) wurde abgebrochen!");
                            dbPlayer.SendNewNotification("Sie haben die Anfrage abgebrochen!");
                            break;
                        case 14:
                            dbPlayer.ResetData("service");
                            TeamModule.Instance[(int) teams.TEAM_GOV].SendNotification($"Die Anfrage von { dbPlayer.GetName() } ({ dbPlayer.ForumId }) wurde abgebrochen!");
                            dbPlayer.SendNewNotification("Sie haben die Anfrage abgebrochen!");
                            break;
                    }
                }
            }
        }
    }
}
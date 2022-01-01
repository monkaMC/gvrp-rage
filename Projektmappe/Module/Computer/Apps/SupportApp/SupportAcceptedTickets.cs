using GTANetworkAPI;
using System.Collections.Generic;
using System.Threading.Tasks;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.Players;
using GVRP.Module.Support;
using static GVRP.Module.Computer.Apps.SupportApp.SupportOpenTickets;

namespace GVRP.Module.Computer.Apps.SupportApp
{
    public class SupportAcceptedTickets : SimpleApp
    {
        public SupportAcceptedTickets() : base("SupportAcceptedTickets") { }

        [RemoteEvent]
        public async void requestAcceptedTickets(Client client)
        {
            
                var dbPlayer = client.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid()) return;

                if (dbPlayer.RankId == 0)
                {
                    dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                    return;
                }

                List<ticketObject> ticketList = new List<ticketObject>();
                var tickets = TicketModule.Instance.GetAcceptedTickets(dbPlayer);

                foreach (var ticket in tickets)
                {
                    string accepted = string.Join(',', ticket.Accepted);

                    ticketList.Add(new ticketObject() { id = (int)ticket.Player.Id, creator = ticket.Player.GetName(), text = ticket.Description, created_at = ticket.Created_at, accepted_by = accepted });
                }

                var serviceJson = NAPI.Util.ToJson(ticketList);
                
                TriggerEvent(client, "responseAcceptedTicketList", serviceJson);
            
        }
    }
}

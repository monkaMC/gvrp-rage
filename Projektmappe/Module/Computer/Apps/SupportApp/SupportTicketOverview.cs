using GTANetworkAPI;
using System.Threading.Tasks;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.Commands;
using GVRP.Module.Logging;
using GVRP.Module.Players;
using GVRP.Module.Support;

namespace GVRP.Module.Computer.Apps.SupportApp
{
    public class SupportTicketOverview : SimpleApp
    {
        public SupportTicketOverview() : base("SupportTicketOverview") { }

        [RemoteEvent]
        public async void supportTeleportToPlayer(Client client, string name)
        {
            var dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (string.IsNullOrEmpty(name)) return;
            if (!dbPlayer.CanAccessMethod()) return;

            if (dbPlayer.RankId == 0)
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            var destinationPlayer = Players.Players.Instance.FindPlayer(name);
            if (destinationPlayer == null) return;

            if (dbPlayer.Player.IsInVehicle)
            {
                dbPlayer.Player.Vehicle.Position = destinationPlayer.Player.Position;
                dbPlayer.Player.Vehicle.Dimension = destinationPlayer.Player.Dimension;
            }
            else
            {
                dbPlayer.Player.SetPosition(destinationPlayer.Player.Position);
            }

            dbPlayer.DimensionType[0] = destinationPlayer.DimensionType[0];
            dbPlayer.Player.Dimension = destinationPlayer.Player.Dimension;

            dbPlayer.SendNewNotification("Sie haben sich zu " + destinationPlayer.GetName() + " teleportiert!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);

            if (dbPlayer.Rank.CanAccessFeature("silentTeleport")) return;
            if (dbPlayer.IsInGuideDuty())
            {
                destinationPlayer.SendNewNotification("Guide " + destinationPlayer.GetName() + " hat sich zu ihnen teleportiert!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            }
            else if (dbPlayer.RankId < 6)
            {
                destinationPlayer.SendNewNotification("Administrator " + dbPlayer.GetName() + " hat sich zu ihnen teleportiert!", title: "ADMIN", notificationType: PlayerNotification.NotificationType.ADMIN);
            }
            
        }

        [RemoteEvent]
        public async void closeTicket(Client client, string name)
        {
            
            var dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (string.IsNullOrEmpty(name)) return;

            if (dbPlayer.RankId == 0)
            {
                dbPlayer.SendNewNotification(MSG.Error.NoPermissions());
                return;
            }

            var findplayer = Players.Players.Instance.FindPlayer(name);
            if (findplayer == null) return;

            var ticket = TicketModule.Instance.GetTicketByOwner(findplayer);

            if (ticket != null)
            {
                Logger.AddSupportLog(ticket.Player.Id, dbPlayer.Id, ticket.Description, ticket.Created_at);
            }
            bool ticketResponse = TicketModule.Instance.DeleteTicketByOwner(findplayer);
            //bool konversationResponse = KonversationModule.Instance.Delete(findplayer);

            dbPlayer.SendNewNotification(ticketResponse ? $"Sie haben das Ticket von {findplayer.GetName()} geschlossen!" : $"Das Ticket von {findplayer.GetName()} konnte nicht geschlossen werden!");
            findplayer.SendNewNotification("Ihr Ticket wurde geschlossen!");
            
        }
    }
}

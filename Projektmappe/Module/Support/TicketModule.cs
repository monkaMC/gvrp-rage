using System;
using System.Collections.Generic;
using System.Linq;
using GVRP.Module.Logging;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Support
{
    public sealed class TicketModule : Module<TicketModule>
    {
        public Dictionary<string, Ticket> tickets;

        public override bool Load(bool reload = false)
        {
            tickets = new Dictionary<string, Ticket>();
            return true;
        }

        public bool Add(DbPlayer iPlayer, Ticket ticket)
        {
            if (tickets.ContainsKey(iPlayer.GetName())) return false;

            tickets.Add(iPlayer.GetName(), ticket);
            return true;
        }

        public bool Accept(DbPlayer iPlayer, DbPlayer destinationPlayer)
        {
            var createdTicket = GetTicketByOwner(destinationPlayer);
            if (createdTicket == null) return false;
            if (createdTicket.Accepted.Count > 0) return false;

            bool status = createdTicket.Accepted.Add(iPlayer.GetName());
            return status;
        }

        public Dictionary<string, Ticket> GetAll()
        {
            return tickets;
        }

        public List<Ticket> GetAcceptedTickets(DbPlayer iPlayer)
        {
            return (from kvp in tickets where kvp.Value.Accepted.Contains(iPlayer.GetName()) select kvp.Value).ToList();
        }

        public List<Ticket> GetOpenTickets()
        {
            List<String> toBeRemoved = new List<String>();
            foreach (var ticket in tickets)
            {
                if (DateTime.Compare(ticket.Value.Created_at.AddMinutes(15), DateTime.Now) < 0 || !Players.Players.Instance.FindPlayer(ticket.Key).IsValid()) toBeRemoved.Add(ticket.Key);
            }

            foreach (var ticket in toBeRemoved)
            {
                if (tickets.ContainsKey(ticket)) tickets.Remove(ticket);
            }

            return (from kvp in tickets orderby kvp.Value.Created_at ascending where kvp.Value.Accepted.Count() == 0 select kvp.Value).ToList();
        }

        public Ticket GetTicketByOwner(DbPlayer iPlayer)
        {
            return tickets.ContainsKey(iPlayer.GetName()) ? tickets[iPlayer.GetName()] : null;
        }

        public bool DeleteTicketByOwner(DbPlayer iPlayer)
        {
            var createdTicket = GetTicketByOwner(iPlayer);
            if (createdTicket == null) return false;
            bool status = tickets.Remove(iPlayer.GetName());
            return status;
        }

        public bool ChangeChatStatus(DbPlayer iPlayer, bool status)
        {
            var createdTicket = GetTicketByOwner(iPlayer);
            if (createdTicket == null) return false;

            createdTicket.ChatStatus = status;
            return true;
        }

        public bool getCurrentChatStatus(DbPlayer iPlayer)
        {
            var createdTicket = GetTicketByOwner(iPlayer);
            if (createdTicket == null) return false;

            bool status = createdTicket.ChatStatus;
            return status;
        }

        public string getCurrentTicketSupporter(DbPlayer iPlayer)
        {
            var createdTicket = GetTicketByOwner(iPlayer);
            if (createdTicket == null) return null;

            string user = string.Join(',', createdTicket.Accepted);
            return user;
        }
    }
}

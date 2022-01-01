using System.Collections.Generic;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Support
{
    public sealed class KonversationModule : Module<KonversationModule>
    {
        public Dictionary<uint, List<Konversation>> konversationList;

        public override bool Load(bool reload = false)
        {
            konversationList = new Dictionary<uint, List<Konversation>>();
            return true;
        }

        public List<Konversation> GetTicketKonversation(DbPlayer destinationPlayer)
        {
            List<Konversation> ticketKonversation;

            if (!konversationList.ContainsKey(destinationPlayer.Id))
            {
                ticketKonversation = new List<Konversation>();
                konversationList.Add(destinationPlayer.Id, ticketKonversation);
            }
            else
            {
                ticketKonversation = konversationList[destinationPlayer.Id];
            }

            return ticketKonversation;
        }

        public bool Add(DbPlayer destinationPlayer, Konversation konversationMessage)
        {
            var ticketKonversation = GetTicketKonversation(destinationPlayer);
            ticketKonversation.Add(konversationMessage);
            return true;
        }

        public bool Delete(DbPlayer destinationPlayer)
        {
            var ticketKonversation = GetTicketKonversation(destinationPlayer);
            if (ticketKonversation.Count == 0) return false;

            bool status = konversationList.Remove(destinationPlayer.Id);
            return status;
        }
    }
}

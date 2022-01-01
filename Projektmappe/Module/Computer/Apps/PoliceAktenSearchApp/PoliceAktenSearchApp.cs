using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using Newtonsoft.Json;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Computer.Apps.PoliceAktenSearchApp
{
    public class PoliceAktenSearchApp : SimpleApp
    {
        public PoliceAktenSearchApp() : base("PoliceAktenSearchApp")
        {
        }
        
        [RemoteEvent]
        public async void requestPlayerResults(Client player, string searchQuery)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            var foundedPlayers = new List<string>();
            int counter = 0;
            foreach (var findPlayer in Players.Players.Instance.GetValidPlayers().Where
                (p => p.GetName().ToLower().Contains(searchQuery.ToLower()) || p.CustomData.Phone.Trim() == searchQuery.Trim() || 
                (dbPlayer.TeamId == (int)teams.TEAM_FIB && (p.CustomData.Address.ToLower().Contains(searchQuery.ToLower())) || p.CustomData.Info.Contains(searchQuery.ToLower()) || p.CustomData.Membership.ToLower().Contains(searchQuery.ToLower()))))
            {
                if (counter >= 10)
                    break;

                foundedPlayers.Add(findPlayer.GetName());
                counter++;
            }

            TriggerEvent(player, "responsePlayerResults", NAPI.Util.ToJson(foundedPlayers));
        }
    }
}

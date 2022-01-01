using GVRP.Module.ClientUI.Apps;
using GTANetworkAPI;
using GVRP.Module.Players;
using GVRP.Module.Players.Phone.Apps;

namespace GVRP.Module.Teams.Apps
{
    public class TeamEditApp : SimpleApp
    {
        public TeamEditApp() : base("TeamEditApp")
        {
        }

        [RemoteEvent]
        public void leaveTeam(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            dbPlayer.SetTeam((uint) TeamList.Zivilist);
        }
    }
}
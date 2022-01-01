using System;
using System.Threading.Tasks;
using GTANetworkAPI;
using GVRP.Handler;
using GVRP.Module.Chat;
using GVRP.Module.GTAN;
using GVRP.Module.Items;
using GVRP.Module.Players;

using GVRP.Module.Players.PlayerAnimations;
using GVRP.Module.Vehicles;

namespace GVRP
{
    public class PlayerCheckpointEventHandler : Script
    {
        public static Random rnd = new Random();
        
        [RemoteEvent]
        public void onPlayerEnterCheckpoint(Client client, object[] args)
        {
            OnPlayerEnterCheckpoint(client);
        }

        [ServerEvent(Event.PlayerEnterColshape)]
        public void onPlayerEnterColShape(ColShape shape, Client player)
        {
            OnPlayerEnterCheckpoint(player);
        }

        public static void OnPlayerEnterCheckpoint(Client player)
        {
            var iPlayer = player.GetPlayer();

            if (iPlayer == null || !iPlayer.IsValid()) return;


            string adddata = "";
            float floatData = 0.0f;
            if (iPlayer.HasData("checkpointData") && iPlayer.HasData("checkpointFloat") && iPlayer.HasData("checkpointPos"))
            {
                adddata = iPlayer.GetData("checkpointData");
                floatData = iPlayer.GetData("checkpointFloat");

                if (player.Position.DistanceTo(iPlayer.GetData("checkpointPos")) > 15.0f)
                    return;
            }

            if (adddata == "arsenalrob")
            {
                if (!iPlayer.Container.CanInventoryItemAdded(172))
                {
                    iPlayer.SendNewNotification( "Inventar ist voll!");
                    return;
                }

                iPlayer.Container.AddItem(172, 1);
                return;
            }
            return;
        }
    }
}
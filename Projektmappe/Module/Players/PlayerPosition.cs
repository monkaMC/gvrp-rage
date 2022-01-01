

using GTANetworkAPI;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Players
{
    public static class PlayerPosition
    {
        public static void SetRotation(this Client client, float rotation)
        {
            client.Rotation = new Vector3(0, 0, rotation);
        }

        public static void SetPosition(this Client player, Vector3 pos)
        {
            //var originalDimension = player.Dimension;

            // Workaround Vehicle Dismiss
            //player.Dimension = -1;


            DbPlayer iPlayer = player.GetPlayer();
            if (iPlayer == null || !iPlayer.IsValid()) return;

            iPlayer.SetData("Teleport", 2);
            player.Position = pos;


            //player.Dimension = originalDimension;
        }
        
        public static void SetWaypoint(this DbPlayer iPlayer, float x, float y)
        {
            iPlayer.SetData("waypoint_x", x);
            iPlayer.SetData("waypoint_y", y);

            iPlayer.Player.SendWayPoint(x, y);
        }

        public static Vector3 GetWaypoint(this DbPlayer iPlayer)
        {
            float x = iPlayer.GetData("waypoint_x");
            float y = iPlayer.GetData("waypoint_y");
            float z = 0.0f;
            return new Vector3(x, y, z);
        }

        public static void SendWayPoint(this Client player, float x, float y)
        {
            player.TriggerEvent("setPlayerGpsMarker", x, y);
        }
    }
}
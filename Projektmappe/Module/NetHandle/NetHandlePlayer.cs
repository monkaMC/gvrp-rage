using GTANetworkAPI;

namespace GVRP
{
    public static class NetHandlePlayer
    {
        public static Client ToPlayer(this NetHandle netHandle)
        {
            return NAPI.Player.GetPlayerFromHandle(netHandle);
        }
    }
}
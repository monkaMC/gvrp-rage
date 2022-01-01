using GTANetworkAPI;

namespace GVRP
{
    public static class NetHandleType
    {
        public static EntityType GetEntityType(this NetHandle netHandle)
        {
            return netHandle.Type;
        }
    }
}
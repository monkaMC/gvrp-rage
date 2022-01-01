using GTANetworkAPI;

namespace GVRP.Module.Spawners
{
    public static class ObjectSpawn
    {
        public static Object Create(int model, Vector3 position, Vector3 rotation, uint dimension = 0, byte alpha = 255)
        {
            return NAPI.Object.CreateObject(model, position, rotation, alpha, dimension);
        }

        public static Object Create(uint model, Vector3 position, Vector3 rotation, byte alpha = 255, uint dimension = 0)
        {
            return NAPI.Object.CreateObject(model, position, rotation, alpha, dimension);
        }
    }
}
using GTANetworkAPI;

namespace GVRP.Module.Spawners
{
    public static class ColShapes
    {
        public static ColShape Create(Vector3 position, float range, uint dimension = 4294967295)
        {
            return NAPI.ColShape.CreateSphereColShape(position, range, dimension);
        }
    }
}
/*using GTANetworkAPI;

namespace GVRP.Module.GTAN
{
    public static class VehicleGTAN
    {
        public static void AttachTo(this Vehicle vehicle, Vehicle target, string bone, Vector3 offset, Vector3 rotation)
        {
            vehicle.attachTo(target, bone, offset, rotation);
        }
        
        public static void Delete(this Vehicle vehicle)
        {
            vehicle.delete();
        }
        
        public static void Detach(this Vehicle vehicle)
        {
            vehicle.detach();
        }
        
        public static bool TryData<T>(this ColShape colShape, string key, out T value)
        {
            var HasData = colShape.HasData(key);
            value = HasData ? colShape.GetData(key) : default(T);
            return HasData;
        }
    }
}*/
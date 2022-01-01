using GTANetworkAPI;
using GVRP.Module.Vehicles.Data;

namespace GVRP.Module.Vehicles
{
    public static class Flatbed
    {
        public static Vector3 GetFlatbedVehicleOffset(this VehicleHash p_Hash)
        {
            var l_VehicleData = VehicleDataModule.Instance.GetData((uint)p_Hash);
            if (l_VehicleData == null || l_VehicleData.Offset == null) return null;

            float l_X = l_VehicleData.Offset.X;
            float l_Y = l_VehicleData.Offset.Y;
            float l_Z = l_VehicleData.Offset.Z;
            Vector3 l_Offset = new Vector3()
            {
                X = l_X,
                Y = l_Y,
                Z = l_Z
            };

            return l_Offset;
        }
    }
}
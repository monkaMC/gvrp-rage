using GTANetworkAPI;

namespace GVRP.Module.Spawners
{
    public static class Peds
    {
        public static Marker Create(PedHash skin, Vector3 pos, float heading, uint dimension = 0)
        {
            //Ped Bot = API.createPed(skin, pos, heading, dimension);
            return API.Shared.CreateMarker(0, new Vector3(pos.X, pos.Y, pos.Z), new Vector3(), new Vector3(),
                0.8f,
                new Color(255, 255, 0), false, dimension);
        }
    }
}
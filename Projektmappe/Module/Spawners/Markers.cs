using GTANetworkAPI;

namespace GVRP.Module.Spawners
{
    public static class Markers
    {
        public static Marker Create(int markerType, Vector3 pos, Vector3 dir, Vector3 rot, float scale, int alpha,
            int r, int g, int b, uint dimension = 0, bool bobUpAndDown = false)
        {
            return NAPI.Marker.CreateMarker(markerType, pos, dir, rot, scale, new Color(r, g, b), bobUpAndDown, dimension);
        }

        public static Marker CreateDefaultMarker(Vector3 posititon, uint dimension = 0)
        {
            return Create(0, posititon, new Vector3(), new Vector3(),
                0.8f, 255, 255, 0, 0, dimension);
        }

        public static Marker CreateSimple(int markerType, Vector3 pos, float size, int r, int g, int b,
            int alpha = 255, uint dimension = 0, bool bobUpAndDown = false)
        {
            return Create(markerType, pos, new Vector3(), new Vector3(), size, alpha, r, g, b,
                dimension, bobUpAndDown);
        }
    }
}
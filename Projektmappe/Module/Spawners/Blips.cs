



using GTANetworkAPI;

namespace GVRP.Module.Spawners
{
    public static class Blips
    {
        public static Blip Create(Vector3 pos, string name, uint sprite, float scale,
            bool shortrange = true, int color = 0, int alpha = 255)
        {
            var pBlip = NAPI.Blip.CreateBlip(pos);
            pBlip.Name = name;
            pBlip.Sprite = sprite;
            pBlip.Scale = scale;
            pBlip.ShortRange = shortrange;
            pBlip.Color = color;
            pBlip.Transparency = alpha;
            return pBlip;
        }
    }
}
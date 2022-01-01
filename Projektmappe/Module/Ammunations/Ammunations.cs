using System.Collections.Generic;
using GTANetworkAPI;
using GVRP.Module.Spawners;

namespace GVRP.Module.Ammunations
{
    public class Ammunations
    {
        public static Ammunations Instance { get; } = new Ammunations();

        private readonly Dictionary<int, Ammunation> ammunations;

        private static uint blip = 0;
        private static int color = 0;

        private Ammunations()
        {
            ammunations = new Dictionary<int, Ammunation>();
        }

        public void Load()
        {
            PointsOfInterest.PointOfInterestModule.Instance.GetAll().TryGetValue(140, out PointsOfInterest.PointOfInterest temp);
            blip = temp.Blip;
            color = (int)temp.BlipColor;
            // Ammunations
            Add(new Vector3(21.006, -1106.372, 29.797));
            Add(new Vector3(811.254, -2157.673, 29.619));
            Add(new Vector3(843.329, -1033.942, 28.195));
            Add(new Vector3(-1305.258, -393.208, 36.696));
            Add(new Vector3(252.862, -49.209, 69.941));
            Add(new Vector3(2568.893, 294.226, 108.735));
            Add(new Vector3(1692.728, 3759.273, 34.705));
            Add(new Vector3(-331.176, 6083.048, 31.455));
            Add(new Vector3(-3172.487, 1086.728, 20.839));
            Add(new Vector3(-1118.809, 2697.982, 18.55415));
            Add(new Vector3(-663.686, -938.757, 21.8292));
        }

        public bool Add(Vector3 position)
        {
            var ammunation = new Ammunation(ammunations.Count, position);
            OnAmmunationSpawn(ammunation);
            ammunations.Add(ammunation.Id, ammunation);
            return true;
        }

        public Ammunation Get(int id)
        {
            return ammunations.ContainsKey(id) ? ammunations[id] : null;
        }

        public static void OnAmmunationSpawn(Ammunation ammunation)
        {
            Main.ServerBlips.Add(Blips.Create(ammunation.Position, "Ammunation", blip, 1.0f, color:color));

            // Pickup System
            // CreateMarkerForUser(new Vector3(kvp.Value.pos.X, kvp.Value.pos.Y, kvp.Value.pos.Z + 1.5f), 29, 0.8f, 255, 255, 0, 0); // AD punkt
        }
    }
}
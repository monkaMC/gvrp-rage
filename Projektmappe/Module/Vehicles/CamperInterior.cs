using System.IO;
using GTANetworkAPI;


namespace GVRP.Module.Vehicles
{
    public static class CamperInterior
    {
        public static Vector3 baseVector;

        public static string output = "";

        public static void SpawnObjects(this Vehicle vehicle, Vector3 vector3)
        {
            baseVector = new Vector3(-42.16517f + vector3.X, -1483.09f + vector3.Y, 40.31f + vector3.Z);
            Create(vehicle, -1007599668, new Vector3(-42.16517f, -1483.09f, 40.31f),
                new Vector3(-0.375022f, 4.988406E-06f, -91.37488f), 100);
            Create(vehicle, -1007599668, new Vector3(-42.30888f, -1481.448f, 40.06f),
                new Vector3(-0.05004552f, -17.09796f, -92.49992f), 100);
            Create(vehicle, 1437126442, new Vector3(-42.1f, -1484.16f, 41.98501f),
                new Vector3(0f, 0f, 0f), 100);
            Create(vehicle, 1437126442, new Vector3(-43.37f, -1484.16f, 41.98501f),
                new Vector3(0f, 0f, 0f), 100);
            Create(vehicle, 1437126442, new Vector3(-43.3f, -1482.88f, 40.76f),
                new Vector3(1.001786E-05f, 5.008955E-06f, -89.72442f), 100);
            Create(vehicle, 1437126442, new Vector3(-43.3f, -1482.844f, 43.88f),
                new Vector3(1.001786E-05f, 5.008955E-06f, -89.72442f), 100);
            Create(vehicle, 1437126442, new Vector3(-43.3f, -1481.76f, 42.26f),
                new Vector3(1.001786E-05f, 5.008955E-06f, -89.72442f), 100);
            Create(vehicle, 1437126442, new Vector3(-43.3f, -1480.5f, 40.9f),
                new Vector3(1.001786E-05f, 5.008955E-06f, -89.72442f), 100);
            Create(vehicle, 1437126442, new Vector3(-43.3f, -1479.24f, 40.9f),
                new Vector3(1.001786E-05f, 5.008955E-06f, -89.72442f), 100);
            Create(vehicle, 1437126442, new Vector3(-43.3f, -1480.48f, 43.88f),
                new Vector3(1.001786E-05f, 5.008955E-06f, -89.72442f), 100);
            Create(vehicle, 1437126442, new Vector3(-43.3f, -1479.35f, 43.88f),
                new Vector3(1.001786E-05f, 5.008955E-06f, -89.72442f), 100);
            Create(vehicle, 1437126442, new Vector3(-40.84f, -1484.152f, 40.77f),
                new Vector3(1.001786E-05f, 5.008956E-06f, 89.84934f), 100);
            Create(vehicle, 1437126442, new Vector3(-40.84f, -1484.152f, 43.87f),
                new Vector3(1.001786E-05f, 5.008956E-06f, 89.84934f), 100);
            Create(vehicle, 1437126442, new Vector3(-40.84f, -1482.95f, 42.26f),
                new Vector3(1.001738E-05f, -5.008957E-06f, 89.99931f), 100);
            Create(vehicle, 1437126442, new Vector3(-40.84f, -1482.22f, 42.1f),
                new Vector3(1.00439E-05f, -5.008954E-06f, 89.99929f), 100);
            Create(vehicle, 283948267, new Vector3(-40.9f, -1481.01f, 42.02773f),
                new Vector3(1.001789E-05f, 5.008956E-06f, 92.49981f), 100);
            Create(vehicle, 1437126442, new Vector3(-40.94f, -1479.66f, 42.1f),
                new Vector3(1.004386E-05f, -5.008956E-06f, 179.9989f), 100);
            Create(vehicle, 1437126442, new Vector3(-42.08f, -1479.66f, 42.1f),
                new Vector3(1.004386E-05f, -5.008956E-06f, 179.9989f), 100);
            Create(vehicle, -1007599668, new Vector3(-42.07669f, -1480.228f, 39.99f),
                new Vector3(-0.375022f, 4.988406E-06f, -91.37488f), 100);
            Create(vehicle, 913904359, new Vector3(-42.02736f, -1481.388f, 43.21f),
                new Vector3(89.97375f, 8.06536E-05f, -90.5993f), 100);
            Create(vehicle, -1781967271, new Vector3(-42.81f, -1480.43f, 40.92f),
                new Vector3(1.001788E-05f, -5.008954E-06f, 90.14926f), 100);
            Create(vehicle, -1818341338, new Vector3(-42.1447f, -1483.989f, 41.45f),
                new Vector3(0f, 0f, 0f), 100);
            Create(vehicle, 538990259, new Vector3(-42.93f, -1481.47f, 41f),
                new Vector3(1.001789E-05f, -5.008954E-06f, 89.82452f), 100);
            Create(vehicle, 831856463, new Vector3(-42.77552f, -1481.638f, 41.14f),
                new Vector3(1.250003f, -13.25f, -68.32494f), 100);
            Create(vehicle, 1110699354, new Vector3(-41.88f, -1479.71f, 42.4987f),
                new Vector3(0f, 0f, 0f), 100);
            Create(vehicle, -422877666, new Vector3(-41.16179f, -1483.82f, 41.6069f),
                new Vector3(91.49928f, 6.2699E-05f, -57.74999f), 100);
            Create(vehicle, -1603796423, new Vector3(-41.56113f, -1483.744f, 41.58f),
                new Vector3(1.001787E-05f, -5.008956E-06f, -128.7498f), 100);
            Create(vehicle, 933678382, new Vector3(-41.45288f, -1483.331f, 42.05069f),
                new Vector3(1.001788E-05f, -5.008957E-06f, -0.2500072f), 100);
            Create(vehicle, -1565454253, new Vector3(-41.01327f, -1482.444f, 41.29f),
                new Vector3(1.001789E-05f, 5.008956E-06f, 87.49981f), 100);
            Create(vehicle, -1103279079, new Vector3(-42.03777f, -1478.395f, 40.37676f),
                new Vector3(0f, 0f, 0f), 100);
            File.WriteAllText("camper.txt", output);
        }

        public static void Create(Vehicle vehicle, int model, Vector3 position, Vector3 rotation, int dimension)
        {
            var newPositionX = position.X - baseVector.X;
            var newPositionY = position.Y - baseVector.Y;
            var newPositionZ = position.Z - baseVector.Z;
            //var obj = Objects.Create(model, position, rotation, dimension);
            //obj.attachTo(vehicle, "chassis_dummy", new Vector3(newPositionX, newPositionY, newPositionZ), rotation);
            //obj.collisionless = false;
            output +=
                $"Attach(vehicle, API.createObject({model}, new Vector3({newPositionX.ToString().Replace(",", ".")}, {newPositionY.ToString().Replace(",", ".")}, {newPositionZ.ToString().Replace(",", ".")}), new Vector3({rotation.X.ToString().Replace(",", ".")}, {rotation.Y.ToString().Replace(",", ".")}, {rotation.Z.ToString().Replace(",", ".")})));";
            output += "\r\n";
            //Logging.Log($"API.createObject({model}, new Vector3({newPositionX.ToString().Replace(",", ".")}, {newPositionY.ToString().Replace(",", ".")}, {newPositionZ.ToString().Replace(",", ".")}), new Vector3({rotation.X.ToString().Replace(",", ".")}, {rotation.Y.ToString().Replace(",", ".")}, {rotation.Z.ToString().Replace(",", ".")}));");
        }
    }
}
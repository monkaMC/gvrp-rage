using GTANetworkAPI;

namespace GVRP.Module.Jobs
{
    public class Job
    {
        public int Id { get; set; }
        public int Level { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Legal { get; set; }
        public Vector3 Position { get; set; }
        public float Heading { get; set; }
        public PedHash Skin { get; set; }
        public string Helps { get; set; }
        public bool disablegang { get; set; }
        public bool disablezivi { get; set; }
        public bool disabled { get; set; }
        
        public Marker Marker { get; set; }
        public int NotificationId { get; set; }
    }
}
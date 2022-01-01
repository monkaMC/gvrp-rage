using GTANetworkAPI;

namespace GVRP.Module.Vehicles
{
    public class JobVehicle : ServerVehicle
    {
        public int JobId { get; }

        public JobVehicle(int id, Vehicle vehicle, int jobId) : base(id, vehicle)
        {
            JobId = jobId;
        }
    }
}
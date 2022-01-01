using GTANetworkAPI;

namespace GVRP.Module.Vehicles
{
    public class ServerVehicle
    {
        public int Id { get; }
        public Vehicle Vehicle { get; }

        public ServerVehicle(int id, Vehicle vehicle)
        {
            Id = id;
            Vehicle = vehicle;
            Vehicle.SetData("ServerVehicle", this);
        }

        public bool IsJobVehicle(int jobId = 0)
        {
            return this is JobVehicle && jobId == 0 || jobId == ((JobVehicle) this).JobId;
        }

        public bool IsTeamVehicle()
        {
            return this is TeamVehicle;
        }
        
        public virtual void Death()
        {
            Vehicle.SafeDelete();
        }
    }
}
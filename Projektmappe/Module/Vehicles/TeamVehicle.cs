using GTANetworkAPI;

namespace GVRP.Module.Vehicles
{
    public class TeamVehicle : ServerVehicle
    {
        public int TeamId { get; }

        public TeamVehicle(int id, Vehicle vehicle, int teamId) : base(id, vehicle)
        {
            TeamId = teamId;
        }

        public override void Death()
        {
            base.Death();
            //Todo: move to team garage
        }
    }
}
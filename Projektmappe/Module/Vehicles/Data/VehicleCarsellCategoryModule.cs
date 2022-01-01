namespace GVRP.Module.Vehicles.Data
{
    public sealed class VehicleCarsellCategoryModule : SqlModule<VehicleCarsellCategoryModule, VehicleCarsellCategory, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `vehicle_carsell_category`;";
        }
    }
}
namespace GVRP.Module.Vehicles.Data
{
    public sealed class VehicleClassificationModule : SqlModule<VehicleClassificationModule, VehicleClassification, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `vehicle_classes`;";
        }
    }
}
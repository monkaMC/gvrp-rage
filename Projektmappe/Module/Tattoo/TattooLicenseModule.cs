using GVRP.Module.Assets.Tattoo;

namespace GVRP.Module.Tattoo
{
    public class TattooLicenseModule : SqlModule<TattooLicenseModule, TattooLicense, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `tattoo_licenses`;";
        }
    }
}

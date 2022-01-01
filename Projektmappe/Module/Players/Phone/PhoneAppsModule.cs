namespace GVRP.Module.Players.Phone
{
    public class PhoneAppsModule : SqlModule<PhoneAppsModule, Apps.PhoneApp, string>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `phone_apps`";
        }
    }
}
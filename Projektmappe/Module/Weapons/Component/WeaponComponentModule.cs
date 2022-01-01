namespace GVRP.Module.Weapons.Component
{
    public class WeaponComponentModule : SqlModule<WeaponComponentModule, WeaponComponent, int>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `weapon_component`;";
        }
    }
}
namespace GVRP.Module.Weapons.Type
{
    public class WeaponTypeModule : SqlModule<WeaponTypeModule, WeaponType, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `weapon_data_type`;";
        }
    }
}
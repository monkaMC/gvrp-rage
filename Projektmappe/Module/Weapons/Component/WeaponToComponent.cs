using MySql.Data.MySqlClient;

namespace GVRP.Module.Weapons.Component
{
    public class WeaponToComponent
    {
        public int WeaponId { get; }
        public int ComponentId { get; }

        public WeaponToComponent(MySqlDataReader reader)
        {
            WeaponId = reader.GetInt32("weapon_data_id");
            ComponentId = reader.GetInt32("weapon_component_id");
        }
    }
}
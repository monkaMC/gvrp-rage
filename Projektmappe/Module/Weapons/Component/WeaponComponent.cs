using MySql.Data.MySqlClient;

namespace GVRP.Module.Weapons.Component
{
    public class WeaponComponent : Loadable<int>
    {
        public int Id { get; }
        public string Hash { get; }
        public string Name { get; }

        public WeaponComponent(MySqlDataReader reader): base(reader)
        {
            Id = reader.GetInt32("id");
            Hash = reader.GetString("hash");
            Name = reader.GetString("name");
        }

        public override int GetIdentifier()
        {
            return Id;
        }
    }
}
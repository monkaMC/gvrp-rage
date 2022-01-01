using MySql.Data.MySqlClient;

namespace GVRP.Module.Weapons.Type
{
    public class WeaponType : Loadable<uint>
    {
        public uint Id { get; }
        public string Name { get; }

        public WeaponType(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
        }
            
        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
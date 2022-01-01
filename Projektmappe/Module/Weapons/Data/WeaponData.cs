using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace GVRP.Module.Weapons.Data
{
    public class WeaponData : Loadable<int>
    {
        public int Id { get; }
        public int Hash { get; }
        public uint TypeId { get; }
        public string Name { get; }
        public bool DisableDriveBy { get; }

        public Dictionary<string, string> Components { get; }

        public int Weight { get; }

        public WeaponData(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetInt32("id");
            Hash = reader.GetInt32("hash");
            TypeId = reader.GetUInt32("type_id");
            Name = reader.GetString("name");
            DisableDriveBy = reader.GetInt32("disable_driveby") == 1;
            Weight = reader.GetInt32("weight");

            Components = new Dictionary<string, string>();
        }

        public override int GetIdentifier()
        {
            return Id;
        }
    }
}
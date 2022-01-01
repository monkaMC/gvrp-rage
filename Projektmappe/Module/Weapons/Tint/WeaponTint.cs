using System;
using MySql.Data.MySqlClient;

namespace GVRP.Module.Weapons.Tint
{
    public class WeaponTint : Loadable<Tuple<uint, uint>>
    {
        public uint Id { get; }
        public uint TypeId { get; }
        public string Name { get; }
        
        public Tuple<uint, uint> Tuple { get; }

        public WeaponTint(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            TypeId = reader.GetUInt32("type_id");
            Name = reader.GetString("name");
            
            Tuple = new Tuple<uint, uint>(Id, TypeId);
        }

        public override Tuple<uint, uint> GetIdentifier()
        {
            return Tuple;
        }
    }
}
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace GVRP.Module.AnimationMenu
{
    public class AnimationCategory : Loadable<uint>
    {
        public uint Id { get; }
        public string Name { get; }
        public int Order { get; set; }

        public AnimationCategory(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
            Order = reader.GetInt32("order");
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}

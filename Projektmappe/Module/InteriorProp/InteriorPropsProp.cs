using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace GVRP.Module.InteriorProp
{
    public class InteriorPropsProp : Loadable<uint>
    {
        public uint Id { get; }
        public string ObjectName { get; set; }
        public uint InteriorsPropId { get; set; }

        public InteriorPropsProp(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            ObjectName = reader.GetString("name");
            InteriorsPropId = reader.GetUInt32("interiors_props_id");
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}

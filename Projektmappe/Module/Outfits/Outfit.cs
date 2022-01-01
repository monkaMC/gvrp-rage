using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GVRP.Module.Outfits
{
    public class Outfit : Loadable<uint>
    {
        public uint Id { get; }
        public int DataId { get; }
        public string Name { get; }
        public bool Male { get; }
        public List<OutfitProp> Props { get; set; }
        public List<OutfitComponent> Components { get; set; }

        public Outfit(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            DataId = reader.GetInt32("data_id");
            Male = reader.GetInt32("gender") == 0;

            Props = OutfitPropModule.Instance.GetAll().Values.Where(op => op.OutfitId == Id).ToList();
            Components = OutfitComponentModule.Instance.GetAll().Values.Where(op => op.OutfitId == Id).ToList();
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}

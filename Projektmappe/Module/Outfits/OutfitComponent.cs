using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.Clothes;

namespace GVRP.Module.Outfits
{
    public class OutfitComponent : Loadable<uint>
    {
        public uint Id { get; }
        public int OutfitId { get; }
        public int Slot { get; }
        public int Component { get; }
        public int Texture { get; }

        public OutfitComponent(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            OutfitId = reader.GetInt32("outfit_id");
            Slot = reader.GetInt32("slot");
            Component = reader.GetInt32("component");
            Texture = reader.GetInt32("texture");
            
            ClothModule.Instance.Add(OutfitsModule.Instance.GetPropValue(Id), new Cloth(OutfitsModule.Instance.GetPropValue(Id), "Generated", Slot, Component, Texture, 0, 0, 0, 0, 2, 0, false));
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}

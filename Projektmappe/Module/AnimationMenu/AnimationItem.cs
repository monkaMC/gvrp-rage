using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text;

namespace GVRP.Module.AnimationMenu
{
    public class AnimationItem : Loadable<uint>
    {
        public uint Id { get; }
        public uint CategoryId { get; }
        public string Name { get; }
        public string AnimDic { get; }
        public string AnimName { get; }
        public int AnimFlag { get; }

        public string Icon { get; }

        public AnimationItem(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            CategoryId = reader.GetUInt32("category_id");
            Name = reader.GetString("name");
            AnimDic = reader.GetString("anim_dic");
            AnimName = reader.GetString("anim_name");
            AnimFlag = reader.GetInt32("flag");
            Icon = reader.GetString("icon");
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}

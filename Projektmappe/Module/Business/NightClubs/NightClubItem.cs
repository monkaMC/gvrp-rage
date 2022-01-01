using MySql.Data.MySqlClient;
using GVRP.Module.Items;

namespace GVRP.Module.Business.NightClubs
{
    public class NightClubItem : Loadable<uint>
    {
        public uint Id { get; }
        public uint NightClubId { get; set; }
        public uint ItemId { get; set; }
        public string Name { get; }
        public int Price { get; set; }

        public NightClubItem(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            NightClubId = reader.GetUInt32("business_nightclub_id");
            ItemId = reader.GetUInt32("item_id");
            Name = ItemModelModule.Instance.Get(ItemId).Name;
            Price = reader.GetInt32("price");
        }

        public void SetPrice(int newprice)
        {
            Price = newprice;
            MySQLHandler.ExecuteAsync($"UPDATE business_nightclubs_items SET price = '{Price}' WHERE id = '{Id}'");
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}

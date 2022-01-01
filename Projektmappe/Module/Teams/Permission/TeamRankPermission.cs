using MySql.Data.MySqlClient;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Teams.Permission
{
    public class TeamRankPermission : Loadable<uint>
    {
        public uint PlayerId { get; }
        public bool Bank { get; set; }
        public bool Inventory { get; set; }
        public int Manage { get; set; }

        public string Title { get; set; }

        public TeamRankPermission(DbPlayer dbPlayer) : base(null)
        {
            PlayerId = dbPlayer.Id;
            Bank = false;
            Inventory = false;
            Manage = 0;
            Title = "";
        }

        public TeamRankPermission(MySqlDataReader reader) : base(reader)
        {
            PlayerId = reader.GetUInt32("accountid");
            Bank = reader.GetInt32("r_bank") == 1;
            Inventory = reader.GetInt32("r_inventory") == 1;
            Manage = reader.GetInt32("r_manage");
            Title = reader.GetString("title");
        }

        public override uint GetIdentifier()
        {
            return PlayerId;
        }
    }
}
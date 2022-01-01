using MySql.Data.MySqlClient;

namespace GVRP.Module.Forum
{
    public class TeamForumSyncItem : Loadable<uint>
    {
        public uint Id { get; }
        public int LeaderGroup { get; }
        public int MemberGroup { get; }

        public TeamForumSyncItem(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("team_id");
            LeaderGroup = reader.GetInt32("leader_group");
            MemberGroup = reader.GetInt32("member_group");
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }
}
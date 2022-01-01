using MySql.Data.MySqlClient;

namespace GVRP.Module.Forum
{
    public class JobForumSyncItem : Loadable<uint>
    {
        public uint job_id { get; }
        public uint group_id { get; }

        public JobForumSyncItem(MySqlDataReader reader) : base(reader)
        {
            job_id = reader.GetUInt32("group_id");
            group_id = reader.GetUInt32("job_id");
        }

        public override uint GetIdentifier()
        {
            return job_id;
        }
    }
}
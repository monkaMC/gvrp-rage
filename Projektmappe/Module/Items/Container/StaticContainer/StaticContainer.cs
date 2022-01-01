using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Module.Spawners;

namespace GVRP.Module.Items
{
    public enum StaticContainerTypes
    {
        STAATSBANK1 = 1,
        WEAPONFACTORY = 2,
        BRATWA = 3,
        HIGHROLLINHUSTLER = 4,
        ICA = 5,
        ASERLSPD = 6,
        STAATSBANK2 = 7,
        STAATSBANK3 = 8,
        STAATSBANK4 = 9,
        STAATSBANK5 = 10,
        STAATSBANK6 = 11,
        STAATSBANK7 = 12,
        STAATSBANK8 = 13,
        PLANNINGOUTFITMR1 = 14,
        PLANNINGOUTFITMR2 = 15,
        PLANNINGOUTFITMR3 = 16,
        PLANNINGOUTFITMR4 = 17,
        PLANNINGOUTFITMR5 = 18,
        PLANNINGOUTFITMR6 = 19,
        PLANNINGOUTFITMR7 = 20,
        PLANNINGOUTFITMR8 = 21,
        PLANNINGOUTFITMR9 = 22,
        PLANNINGOUTFITMR10 = 23,
    }

    public class StaticContainer : Loadable<uint>
    {
        public uint Id { get; }
        public uint ContainerId { get; }
        public Container Container { get; set; }
        public Vector3 Position { get; }
        public string Name { get; }
        public int Dimension { get; }
        public bool Locked { get; set; }
        public int TeamId { get; set; }

        public float Range { get; set; }

        public StaticContainer(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            ContainerId = reader.GetUInt32("container_id");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            Dimension = reader.GetInt32("dimension");
            Locked = reader.GetInt32("locked") == 1;
            TeamId = reader.GetInt32("teamid");
            Name = reader.GetString("name");
            Range = reader.GetFloat("range");
            Container = ContainerManager.LoadContainer(ContainerId, ContainerTypes.STATIC);

            CreateColshape();
        }

        public override uint GetIdentifier()
        {
            return Id;
        }

        private void CreateColshape()
        {
            if (Range == 0) Range = 5.0f; // workaround for old Containers
            ColShape l_Colshape = ColShapes.Create(Position, Range);
            l_Colshape.SetData("static_container_id", Id);
            l_Colshape.SetData("static_container_teamid", TeamId);
        }
    }
}

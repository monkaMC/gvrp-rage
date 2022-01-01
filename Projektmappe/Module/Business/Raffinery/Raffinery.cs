using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GVRP.Module.Configurations;
using GVRP.Module.Items;
using GVRP.Module.Spawners;

namespace GVRP.Module.Business.Raffinery
{
    public class Raffinery : Loadable<uint>
    {
        public uint Id { get; }
        public Vector3 Position { get; }
        public RaffineryAusbaustufe AusbauStufe { get; set; }
        public Container Container { get; set; }
        public int FörderMengeMin { get; set; }
        public ColShape ColShape { get; set; }
        public int BuyPrice { get; set; }
        public Business OwnerBusiness { get; set; }

        public Raffinery(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            BuyPrice = reader.GetInt32("buy_price");
            Position = new Vector3(reader.GetFloat("pos_x"),
                reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            AusbauStufe = RaffineryAusbaustufeModule.Instance.Get(reader.GetUInt32("ausbaustufe"));
            ColShape = ColShapes.Create(Position, 2.0f);
            ColShape.SetData("raffineryId", Id);
            OwnerBusiness = BusinessModule.Instance.GetAll().Values.FirstOrDefault(b => b.BusinessBranch.RaffinerieId == Id);


            Container = ContainerManager.LoadContainer(Id, ContainerTypes.RAFFINERY);

            Random random = new Random();
            FörderMengeMin = random.Next(AusbauStufe.MinGenerate, AusbauStufe.MaxGenerate);
        }

        public override uint GetIdentifier()
        {
            return Id;
        }

        public bool IsOwnedByBusines()
        {
            return OwnerBusiness != null;
        }

        public Business GetOwnedBusiness()
        {
            return OwnerBusiness;
        }

        public void ReloadData()
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
            {
                using (var conn = new MySqlConnection(Configuration.Instance.GetMySqlConnection()))
                using (var cmd = conn.CreateCommand())
                {
                    await conn.OpenAsync();
                    cmd.CommandText = $"SELECT * FROM `business_raffinery` where id = {Id};";
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                AusbauStufe = RaffineryAusbaustufeModule.Instance.Get(reader.GetUInt32("ausbaustufe"));
                                BuyPrice = reader.GetInt32("buy_price");
                            }
                        }
                    }
                    await conn.CloseAsync();
                }
            }));
        }
    }
}

using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using GVRP.Module.NpcSpawner;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Dealer
{
    public class Dealer : Loadable<uint>
    {
        public uint Id { get; }
        public Vector3 Position { get; set; }
        public int TeamId { get; set; }
        public float Heading { get; set; }
        public string Note { get; set; }
        public PedHash PedHash { get; set; }
        public int VehicleClawPrice { get; set; }
        public bool BigDealer { get; set; }
        public bool Maulwurf { get; set; }
        public DealerResource MethResource { get; set; }
        public DealerResource MethCristallResource { get; set; }
        public DealerResource DiamondResource { get; set; }
        public DealerResource GoldResource { get; set; }
        public DealerResource WeaponResource { get; set; }
        public DealerResource CannabisResource { get; set; }
        public List<DealerResource> DealerResources { get; set; }
        public bool Alert { get; set; }
        public DbPlayer LastSeller { get; set; }
        public bool VehicleClaw { get; set; }
        public int VehicleClawBought { get; set; }

        public Dealer(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32(0);
            Position = new GTANetworkAPI.Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            TeamId = reader.GetInt32("team_id");
            Heading = reader.GetFloat("heading");
            Note = reader.GetString("note");
            Alert = false;
            PedHash = Enum.TryParse(reader.GetString("pedhash"), true, out PedHash skin) ? skin : PedHash.Abigail;

            if (DealerModule.Instance.BigDealer < 2)
            {
                BigDealer = true;
                DealerModule.Instance.BigDealer++;
            }
            else BigDealer = false;

            if (DealerModule.Instance.VehicleClawAmount < 1)
            {
                VehicleClaw = true;
                DealerModule.Instance.VehicleClawAmount++;
            }

            if (DealerModule.Instance.Maulwurf < DealerModule.Instance.MaxMaulwuerfe)
            {
                Maulwurf = true;
                DealerModule.Instance.Maulwurf++;
            }
            else Maulwurf = false;

            Random random       = new Random();
            //                                       Ressource      Cap     Preisrange Normal                   Preisrange "Rein"               DateTime seitdem full - Init Wert
            MethResource        = new DealerResource("Meth",        2500,   (uint)random.Next(230, 255),        (uint)random.Next(450, 480),    DateTime.Now);
            MethCristallResource= new DealerResource("MethKristall",800,    (uint)random.Next(230, 255),        0,    DateTime.Now);
            DiamondResource     = new DealerResource("Juwelen",     50,     (uint)random.Next(5500, 5700),      0,                              DateTime.Now);
            GoldResource        = new DealerResource("Goldbarren",  100,    (uint)random.Next(11000, 13000),    0,                              DateTime.Now);
            WeaponResource      = new DealerResource("Waffenset",   4,      (uint)random.Next(400, 430),        0,                              DateTime.Now);
            CannabisResource    = new DealerResource("Cannabis",    4000,   (uint)random.Next(450, 480),        0,                              DateTime.Now);

            // WICHTIG! Wenn eine neue Ressource hinzugefügt wird zu einem Dealer, packt die DealerResource in die List<DealerResource> ein, sonst wird der Dealer nicht resettet!!!
            DealerResources     = new List<DealerResource>()
            {
                MethResource, MethCristallResource, DiamondResource, GoldResource, WeaponResource, CannabisResource
            };

            VehicleClawPrice    = 100000;
            VehicleClawBought   = 0;

            new Npc(PedHash, Position, Heading, 0);
        }

        public override uint GetIdentifier()
        {
            return Id;
        }
    }

    public class DealerResource
    {
        public string Name { get; set; }
        public uint Cap { get; set; }
        public uint Sold { get; set; }
        public uint Price { get; set; }
        public uint PurePrice { get; set; }
        public DateTime TimeSinceFull { get; set; }

        public DealerResource(string name, uint cap, uint price, uint purePrice, DateTime timeSinceFull)
        {
            Name            = name;
            Cap             = cap;
            Price           = price;
            PurePrice       = purePrice;
            TimeSinceFull   = timeSinceFull;
            Sold            = 0;
        }

        public bool IsFull()
        {
            return (Sold >= Cap);
        }

        public uint GetAvailableAmountToSell()
        {
            return (Cap - Sold);
        }
    }
}

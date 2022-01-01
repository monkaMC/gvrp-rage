using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GVRP.Module.Injury
{
    public class InjuryDeliver : Loadable<uint>
    {
        public uint Id { get; }
        public string Name { get; }
        public Vector3 Position { get; set; }
        public ColShape ColShape { get; set; }

        public bool BadMedics { get; set; }

        public InjuryDeliver(MySqlDataReader reader) : base(reader)
        {
            Id = reader.GetUInt32("id");
            Name = reader.GetString("name");
            Position = new Vector3(reader.GetFloat("pos_x"), reader.GetFloat("pos_y"), reader.GetFloat("pos_z"));
            BadMedics = reader.GetInt32("badmedics") == 1;
        }

        public override uint GetIdentifier()
        {
            return Id;
        }

        public InjuryDeliverIntPoint GetFreePoint()
        {
            InjuryDeliverIntPoint returnPoint = InjuryDeliverIntPointModule.Instance.GetAll().Values.Where(ijdip => ijdip.DeliverId == Id).First();

            foreach (InjuryDeliverIntPoint injuryDeliverIntPoint in InjuryDeliverIntPointModule.Instance.GetAll().Values.Where(ijdip => ijdip.DeliverId == Id))
            {
                // find player on that point
                if (Players.Players.Instance.GetValidPlayers().Where(p => p.isInjured() && p.Player.Position.DistanceTo(injuryDeliverIntPoint.Position) < 1.2f).Count() > 0) continue;
                else return injuryDeliverIntPoint;
            }
            return null;
        }
    }
}
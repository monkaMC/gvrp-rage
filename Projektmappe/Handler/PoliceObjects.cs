using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Module;
using GVRP.Module.Items;
using GVRP.Module.Players.Db;
using GVRP.Module.Spawners;

namespace GVRP
{
    public class PoliceObject
    {
        public int Id { get; set; }
        public Vector3 Position { get; set; }
        public string Owner { get; set; }
        public ColShape Shape { get; set; }
        public Object Entity { get; set; }
        public ItemModel Item { get; set; }
    }

    public class PoliceObjectModule : Module<PoliceObjectModule>
    {

        private Dictionary<int, PoliceObject> objects = new Dictionary<int, PoliceObject>();
        private const int MaxPoliceCounts = 30;
        private int currUnique;

        protected override bool OnLoad()
        {
            objects = new Dictionary<int, PoliceObject>();
            return base.OnLoad();
        }

        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {
            if(objects.Count() > 0)
            {

            }
        }

        public bool IsMaxReached()
        {
            return MaxPoliceCounts < objects.Count;
        }

        public PoliceObject Add(int model, Client player, ItemModel item, bool nail = false)
        {
            var polO = new PoliceObject
            {
                Id = currUnique++,
                Position = player.Position,
                Owner = player.Name,
                Item = item
            };

            var pos = new Vector3(player.Position.X, player.Position.Y, player.Position.Z - 1f);
            var rot = player.Rotation;

            if (nail)
            {
                pos = new Vector3(player.Position.X, player.Position.Y, player.Position.Z - 0.9f);
                rot = new Vector3(player.Rotation.X, player.Rotation.Y, player.Rotation.Z + 90.0f);
                polO.Shape = ColShapes.Create(pos, 7.0f, 0);
                polO.Shape.SetData("nail", 1);
            }

            polO.Entity = ObjectSpawn.Create(model, pos, rot);

            objects.Add(polO.Id, polO);
            return polO;
        }

        public Dictionary<int, PoliceObject> GetAll()
        {
            return objects;
        }

        public PoliceObject GetNearest(Vector3 position)
        {
            foreach (var kvp in objects)
            {
                var l_Entity = kvp.Value.Entity;
                if (l_Entity == null)
                    continue;

                if (l_Entity.Position.DistanceTo(position) <= 2.0f)
                    return kvp.Value;
            }

            return null;
        }

        public void Delete(PoliceObject obj)
        {
            obj.Shape?.Delete();
            obj.Entity.Delete();
            objects.Remove(obj.Id);
        }
    }
}
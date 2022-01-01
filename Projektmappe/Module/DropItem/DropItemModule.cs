using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkAPI;
using GVRP.Module.GTAN;
using GVRP.Module.Items;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Spawners;
using Task = GTANetworkMethods.Task;

namespace GVRP.Module.DropItem
{
    public sealed class DropItemModule : Module<DropItemModule>
    {
        public  Dictionary<int, ItemHeap> ItemHeapDictionary = new Dictionary<int, ItemHeap>();
        public int Counter = 0;


        public void DropInventoryItem(DbPlayer dbPlayer, Dictionary<ItemModel, int> droppedItems)
        {
            ItemHeap itemHeap = null;

            if (!dbPlayer.Rank.CanAccessFeature("dropitem")) return;
            /*
            if (dbPlayer.TryData("dropItemHeap", out int dropItemHeapId))
            {
                //player already is in a dropHeap 
                if (ItemHeapDictionary.TryGetValue(dropItemHeapId, out itemHeap))
                {
                    foreach (var VARIABLE in droppedItems)
                    {
                        itemHeap.Container.AddItem(VARIABLE.Key.Id, VARIABLE.Value);
                    }
                    return;
                }
                else
                {
                    dbPlayer.ResetData("dropItemHeap");
                }
            }

            //there is no heap, so fucking create one
            ColShape colShape = ColShapes.Create(dbPlayer.Player.Position, 1.0f);
            colShape.SetData("dropItemHeap", Counter);
            dbPlayer.SetData("dropItemHeap", Counter);

            Marker marker = Markers.Create(43, dbPlayer.Player.Position.Subtract(new Vector3(0, 0, 1.0)), new Vector3(), new Vector3(), 0.5f, 100, 255, 255, 0);

            itemHeap = new ItemHeap();
            itemHeap.ColShape = colShape;
            itemHeap.Marker = marker;
            itemHeap.CreateDateTime = DateTime.Now;
            itemHeap.Container = ContainerManager.LoadContainer((uint) Counter, ContainerTypes.HEAP);

            ItemHeapDictionary.Add(Counter++, itemHeap);

            foreach (var VARIABLE in droppedItems)
            {
                itemHeap.Container.AddItem(VARIABLE.Key.Id, VARIABLE.Value);
            }*/

        }

        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            /*if (colShapeState == ColShapeState.Exit)
            {
                if (!dbPlayer.TryData("dropItemHeap", out int dropItemHeapId)) return false;
                dbPlayer.ResetData("dropItemHeap");
                return false;
            }
            else
            {
                if (!colShape.TryData("dropItemHeap", out int dropItemHeapId)) return false;
                if (ItemHeapDictionary.TryGetValue(dropItemHeapId, out ItemHeap itemHeap))
                {
                    dbPlayer.SetData("dropItemHeap", dropItemHeapId);
                }
                return false;
            }*/
            return false;
        }

        public override void OnMinuteUpdate()
        {
            /*foreach (var VARIABLE in ItemHeapDictionary.ToList().Where(d => d.Value.CreateDateTime.AddMinutes(1) < DateTime.Now))
            {
                if (ItemHeapDictionary.TryGetValue(VARIABLE.Key, out ItemHeap temp))
                {
                    NAPI.Task.Run(async () =>
                    {
                        temp.ColShape.Delete();
                        temp.Marker.Delete();
                    });
                    temp.Container.ClearInventory();
                    //temp.Container.ClearInventory();
                    ItemHeapDictionary.Remove(VARIABLE.Key);
                }
            }*/

        }
    }
}

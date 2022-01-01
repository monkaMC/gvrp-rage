using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using GVRP.Module.ClientUI.Windows;
using GVRP.Module.Items;
using GVRP.Module.Logging;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Players.Windows
{
    public class InventoryWindow : Window<Func<DbPlayer, List<ClientContainerObject>, bool>>
    {
        private class ShowEvent : Event
        {
            //private string InventoryContent { get; } // --- appears to be empty if used?
            [JsonProperty(PropertyName = "inventory")] private List<ClientContainerObject> InventoryContent { get; } // --- Produces incorrect json

            public ShowEvent(DbPlayer dbPlayer, List<ClientContainerObject> inventoryContent) : base(dbPlayer)
            {
                InventoryContent = inventoryContent;
            }
        }
        public override Func<DbPlayer, List<ClientContainerObject>, bool> Show()
        {
            return (player, inventoryContent) => OnShow(new ShowEvent(player, inventoryContent));
        }

        public InventoryWindow() : base("Inventory")
        {
        }
    }

    public class inventoryContentObject
    {
        public string content;
    }
}

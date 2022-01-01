using GTANetworkAPI;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.Computer.Apps.Marketplace;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Computer.Apps.MarketplaceApp
{
    public class MarketplaceApp : SimpleApp
    {
        public MarketplaceApp() : base("MarketplaceApp") { }

        [RemoteEvent]
        public async void requestMarketplaceCategories(Client client)
        {
            
                DbPlayer dbPlayer = client.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid()) return;

                var marketplaceCategorys = MarketplaceCategoryModule.Instance.GetAll();
                var categoryList = new List<CategoryObject>();

                foreach (var category in marketplaceCategorys.Values)
                {
                    CategoryObject categoryObject = new CategoryObject()
                    {
                        id = (int)category.Id,
                        name = category.Name,
                        icon_path = category.IconPath
                    };

                    categoryList.Add(categoryObject);
                }

                var categoryJson = NAPI.Util.ToJson(categoryList);
                TriggerEvent(client, "responseMarketPlaceCategories", categoryJson);
            
        }
        
        public class CategoryObject
        {
            [JsonProperty(PropertyName = "id")]
            public int id { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string name { get; set; }

            [JsonProperty(PropertyName = "icon_path")]
            public string icon_path { get; set; }
        }
    }
}

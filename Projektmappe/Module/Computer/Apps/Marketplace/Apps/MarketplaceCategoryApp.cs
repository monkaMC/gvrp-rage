using GTANetworkAPI;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GVRP.Module.Chat;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.Computer.Apps.Marketplace;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Computer.Apps.MarketplaceApp
{
    public class MarketplaceCategoryApp : SimpleApp
    {
        public MarketplaceCategoryApp() : base("MarketplaceCategory") { }

        [RemoteEvent]
        public async void requestMarketPlaceOffers(Client client, int category)
        {
            try
            {
                DbPlayer dbPlayer = client.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid()) return;

                var offerList = new List<OfferObject>();
                var offers = MarketplaceOfferModule.Instance.OfferObjects.Where(x => x.Value.CategoryId == (uint)category);

                // Only online players marketoffers...
                foreach (KeyValuePair<int, OfferObject> item in offers.ToList().Where(o => Players.Players.Instance.GetValidPlayers().Where(dp => dp.Id == o.Value.PlayerId).Count() > 0))
                {
                    offerList.Add(item.Value);
                }
                var offerJson = NAPI.Util.ToJson(offerList);
                TriggerEvent(client, "responseMarketPlaceOffers", offerJson);
            }
            catch (System.Exception e)
            {
                Logging.Logger.Crash(e);
            }
        }

        public class OfferObject
        {
            [JsonProperty(PropertyName = "id")]
            public int id { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string name { get; set; }

            [JsonProperty(PropertyName = "description")]
            public string description { get; set; }

            [JsonProperty(PropertyName = "price")]
            public int price { get; set; }

            [JsonProperty(PropertyName = "phone")]
            public int phone { get; set; }

            [JsonIgnore]
            public int CategoryId { get; set; }

            [JsonIgnore]
            public uint PlayerId { get; set; }

            [JsonProperty(PropertyName = "search")]
            public bool Search { get; set; }

        }
    }
}

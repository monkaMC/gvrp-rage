using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GVRP.Module.Chat;
using GVRP.Module.ClientUI.Apps;
using GVRP.Module.Computer.Apps.Marketplace;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using static GVRP.Module.Computer.Apps.MarketplaceApp.MarketplaceCategoryApp;

namespace GVRP.Module.Computer.Apps.MarketplaceApp
{
    public class MarketplaceOfferApp : SimpleApp
    {
        public MarketplaceOfferApp() : base("MarketplaceOffer") { }

        [RemoteEvent]
        public async void addOffer(Client client, int category, string name, int price, string description, bool search)
        {
            
                DbPlayer dbPlayer = client.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid()) return;

                if (MarketplaceOfferModule.Instance.OfferObjects.Where(x => x.Value.phone == (int)dbPlayer.handy[0]).Count() == 3)
                {
                    dbPlayer.SendNewNotification("Du hast bereits 3 Anzeigen aufgegeben.", title: "Werbung", notificationType: PlayerNotification.NotificationType.ERROR);
                    return;
                }

                if (!dbPlayer.TakeBankMoney(1500))
                {
                    dbPlayer.SendNewNotification("Du hast nicht genug Geld auf dem Konto ($ 1500", title: "Werbung", notificationType: PlayerNotification.NotificationType.ERROR);
                    return;
                }



                string offerName = replaceContent(name);
                string offerDescription = replaceContent(description);

                int newId = 0;
                if (MarketplaceOfferModule.Instance.OfferObjects.Count != 0)
                {
                    while (MarketplaceOfferModule.Instance.OfferObjects.ContainsKey(newId))
                    {
                        newId++;
                    }
                }

                // Lol hier könnte man hart DB ficken
                name = MySqlHelper.EscapeString(name);
                description = MySqlHelper.EscapeString(description);

                // Add to current list
                OfferObject OfferObject = new OfferObject()
                {
                    id = newId,
                    name = offerName,
                    description = offerDescription,
                    price = price,
                    phone = (int)dbPlayer.handy[0],
                    CategoryId = category,
                    PlayerId = dbPlayer.Id,
                    Search = search,
                };

                MarketplaceOfferModule.Instance.OfferObjects.Add(newId, OfferObject);

                // Add to db
                MySQLHandler.ExecuteAsync($"INSERT INTO `marketplace_offers` (`name`, `player_id`, `description`, `category_id`, `price`, `phone`, `search`, `date`) VALUES ('{ offerName }', '{dbPlayer.Id}', '{ offerDescription }', '{ category }', '{ price }', '{ dbPlayer.handy[0] }', '{(search ? 1 : 0)}', '{ DateTime.Now:yyyy-MM-dd H:mm:ss}');");
                dbPlayer.SendNewNotification("Angebot erfolgreich erstellt.", title: "Werbung", notificationType: PlayerNotification.NotificationType.SUCCESS);
            
        }

        public string replaceContent(string input)
        {
            //return Regex.Replace(input, @"^[a-zA-Z0-9\s]+$", "");
            return Regex.Replace(input, @"[^a-zA-Z0-9\s]", "");

        }
    }
}

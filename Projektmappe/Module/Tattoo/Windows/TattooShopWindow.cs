using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using GVRP.Module.Assets.Tattoo;
using GVRP.Module.Business;
using GVRP.Module.ClientUI.Windows;
using GVRP.Module.Customization;
using GVRP.Module.GTAN;
using GVRP.Module.Logging;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Tattoo;

namespace GVRP.Module.Tattoo.Windows
{
    public class TattooShopWindow : Window<Func<DbPlayer, List<ClientTattoo>, bool>>
    {
        private class ShowEvent : Event
        {
            [JsonProperty(PropertyName = "tattoos")] private List<ClientTattoo> Tattoos { get; }

            public ShowEvent(DbPlayer dbPlayer, List<ClientTattoo> tattoos) : base(dbPlayer)
            {
                tattoos = tattoos.OrderBy(t => t.Name).ToList();
                Tattoos = tattoos;
            }
        }

        public TattooShopWindow() : base("TattooShop")
        {
        }

        public override Func<DbPlayer, List<ClientTattoo>, bool> Show()
        {
            return (player, tattoos) => OnShow(new ShowEvent(player, tattoos));
        }

        [RemoteEvent]
        public void syncTattoo(Client client, string hash)
        {
            var dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            dbPlayer.ClearDecorations();

            AssetsTattoo assetsTattoo = AssetsTattooModule.Instance.GetAll().Values.ToList().Find(t => t.HashFemale == hash || t.HashMale == hash);
            if (assetsTattoo != null)
            {

                Decoration decoration = new Decoration();
                decoration.Collection = NAPI.Util.GetHashKey(assetsTattoo.Collection);
                decoration.Overlay = dbPlayer.Customization.Gender == 0 ? NAPI.Util.GetHashKey(assetsTattoo.HashMale) : NAPI.Util.GetHashKey(assetsTattoo.HashFemale);

                NAPI.Player.SetPlayerDecoration(client, decoration);
            }
        }

        [RemoteEvent]
        public void tattooShopBuy(Client client, string hash)
        {
            try
            {
                var dbPlayer = client.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid()) return;
                if (!dbPlayer.TryData("tattooShopId", out uint tattooShopId)) return;
                var tattooShop = TattooShopModule.Instance.Get(tattooShopId);
                if (tattooShop == null) return;


                Business.Business business = BusinessModule.Instance.GetById((uint)tattooShop.BusinessId);

                if (business == null) return;

                if (business.GetMembers().Count > 0)
                {
                    foreach (var member in business.GetMembers().Values)
                    {
                        DbPlayer memberPlayer = Players.Players.Instance.GetByDbId(member.PlayerId);
                        if (memberPlayer == null || !memberPlayer.IsValid()) continue;

                        if ((member.Manage || member.Owner || member.Tattoo) && memberPlayer.Player.Position.DistanceTo(dbPlayer.Player.Position) < 10)
                        {

                            AssetsTattoo assetsTattoo = AssetsTattooModule.Instance.GetAll().Values.ToList().Find(t => t.HashFemale == hash || t.HashMale == hash);
                            if (assetsTattoo != null)
                            {
                                TattooAddedItem tattooAddedItem = tattooShop.tattooLicenses.Find(l => l.AssetsTattooId == assetsTattoo.Id);
                                TattooLicense tattooLicense = TattooLicenseModule.Instance.Get((uint) tattooAddedItem.TattooLicenseId);
                                if (tattooLicense == null) return;

                                if (!dbPlayer.TakeMoney(assetsTattoo.Price))
                                {
                                    dbPlayer.SendNewNotification(MSG.Money.NotEnoughMoney(assetsTattoo.Price));
                                    return;
                                }
                                Decoration decoration = new Decoration();
                                decoration.Collection = NAPI.Util.GetHashKey(assetsTattoo.Collection);
                                decoration.Overlay = dbPlayer.Customization.Gender == 0 ? NAPI.Util.GetHashKey(assetsTattoo.HashMale) : NAPI.Util.GetHashKey(assetsTattoo.HashFemale);

                                NAPI.Player.SetPlayerDecoration(client, decoration);
                                dbPlayer.AddTattoo(assetsTattoo.Id);

                                dbPlayer.SendNewNotification($"Tattoo {assetsTattoo.Name} fuer ${assetsTattoo.Price} gekauft!");

                                tattooShop.AddBank((int) (assetsTattoo.Price * 0.5));
                                Logger.AddTattoShopLog(tattooShop.Id, dbPlayer.Id, (int) (assetsTattoo.Price * 0.5), true);


                                dbPlayer.ApplyDecorations();
                                return;
                            }
                        }
                    }
                }


                dbPlayer.SendNewNotification("Um dir ein Tatto stechen zu lassen muss ein Tätowierer anwesend sein.");
                return;



            }
            catch (Exception ex)
            {
                Logger.Crash(ex);
            }
        }
    }
}

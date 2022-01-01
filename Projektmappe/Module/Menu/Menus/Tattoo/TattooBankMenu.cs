using System;
using System.Collections.Generic;
using System.Linq;
using GVRP.Module.Assets.Tattoo;
using GVRP.Module.Banks.Windows;
using GVRP.Module.Business;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.GTAN;
using GVRP.Module.Houses;
using GVRP.Module.Items;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Tattoo;
using GVRP.Module.Tattoo.Windows;

namespace GVRP
{
    public class TattooBankMenuBuilder : MenuBuilder
    {
        public TattooBankMenuBuilder() : base(PlayerMenu.TattooBankMenu)
        {
        }

        public override Menu Build(DbPlayer iPlayer)
        {
            if (!iPlayer.TryData("tattooShopId", out uint tattooShopId)) return null;
            var tattooShop = TattooShopModule.Instance.Get(tattooShopId);
            if (tattooShop == null) return null;

            if (!iPlayer.GetActiveBusinessMember().Manage || iPlayer.GetActiveBusinessMember().BusinessId != tattooShop.BusinessId) return null;

            var menu = new Menu(Menu, "Tattoo Shop");


            menu.Add($"Bank öffnen");
            menu.Add($"Tattoowieren");

            menu.Add(MSG.General.Close());
            return menu;
        }

        public override IMenuEventHandler GetEventHandler()
        {
            return new EventHandler();
        }

        private class EventHandler : IMenuEventHandler
        {
            public bool OnSelect(int index, DbPlayer dbPlayer)
            {
                if (!dbPlayer.TryData("tattooShopId", out uint tattooShopId)) return false;
                var tattooShop = TattooShopModule.Instance.Get(tattooShopId);
                if (tattooShop == null) return false;

                switch (index)
                {
                    case 0: // Bank
                        if (!dbPlayer.GetActiveBusinessMember().Manage || dbPlayer.GetActiveBusinessMember().BusinessId != tattooShop.BusinessId) return false;

                        MenuManager.DismissCurrent(dbPlayer);

                        Business biz = BusinessModule.Instance.GetById((uint)tattooShop.BusinessId);
                        if (biz != null)
                        {
                            ComponentManager.Get<BankWindow>().Show()(dbPlayer, "TattooStudioBank", dbPlayer.GetName(), dbPlayer.money[0], tattooShop.Bank, 0, biz.BankHistory);
                        }
                        break;
                    case 1: // Tattowieren

                        if (dbPlayer.Player.Position.DistanceTo(tattooShop.Position) > 5.0f) return false;

                        List<ClientTattoo> cTattooList = new List<ClientTattoo>();

                        if (tattooShop.tattooLicenses.Count <= 0)
                        {
                            return false;
                        }

                        foreach (TattooAddedItem tattooAddedItem in tattooShop.tattooLicenses)
                        {
                            AssetsTattoo assetsTattoo = AssetsTattooModule.Instance.Get((uint)tattooAddedItem.AssetsTattooId);
                            if (assetsTattoo.GetHashForPlayer(dbPlayer) != "")
                            {
                                cTattooList.Add(new ClientTattoo(assetsTattoo.GetHashForPlayer(dbPlayer), assetsTattoo.ZoneId, assetsTattoo.Price, assetsTattoo.Name));
                            }
                        }

                        dbPlayer.SetTattooClothes();

                        MenuManager.DismissCurrent(dbPlayer);
                        ComponentManager.Get<TattooShopWindow>().Show()(dbPlayer, cTattooList);
                        break;
                    default:
                        MenuManager.DismissCurrent(dbPlayer);
                        break;
                }
                MenuManager.DismissCurrent(dbPlayer);
                return true;
            }
        }
    }
}
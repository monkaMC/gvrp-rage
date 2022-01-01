using System;
using System.Collections.Generic;
using System.Linq;
using GVRP.Module.Assets.Tattoo;
using GVRP.Module.Business;
using GVRP.Module.GTAN;
using GVRP.Module.Houses;
using GVRP.Module.Items;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Tattoo;

namespace GVRP
{
    public class TattooBuyMenuBuilder : MenuBuilder
    {
        public TattooBuyMenuBuilder() : base(PlayerMenu.TattooBuyMenu)
        {
        }

        public override Menu Build(DbPlayer iPlayer)
        {
            if (!iPlayer.TryData("tattooShopId", out uint tattooShopId)) return null;
            var tattooShop = TattooShopModule.Instance.Get(tattooShopId);
            if (tattooShop == null || tattooShop.BusinessId != 0) return null;

  
            iPlayer.SendNewNotification("Derzeit deaktivert!");
            var menu = new Menu(Menu, "TattooShop");

            return menu;
        }

        public override IMenuEventHandler GetEventHandler()
        {
            return new EventHandler();
        }

        private class EventHandler : IMenuEventHandler
        {
            public bool OnSelect(int index, DbPlayer iPlayer)
            {
                if(index == 0)
                {
                    MenuManager.DismissCurrent(iPlayer);
                    return false;
                }
                if(index == 1)
                {
                    // Buy
                    if (!iPlayer.TryData("tattooShopId", out uint tattooShopId)) return false;
                    var tattooShop = TattooShopModule.Instance.Get(tattooShopId);
                    if (tattooShop == null || tattooShop.BusinessId != 0) return false;

                    if (!iPlayer.GetActiveBusinessMember().Owner)
                    {
                        iPlayer.SendNewNotification("Sie muessen ein Business besitzen!");
                    }

                    if (!iPlayer.TakeMoney(tattooShop.Price))
                    {
                        iPlayer.SendNewNotification(MSG.Money.NotEnoughMoney(tattooShop.Price));
                        return false;
                    }

                    tattooShop.SetBusiness((int)iPlayer.GetActiveBusinessMember().BusinessId);
                    iPlayer.SendNewNotification("Tattoshop erworben!");
                    return true;
                }
                MenuManager.DismissCurrent(iPlayer);
                return false;
            }
        }
    }
}
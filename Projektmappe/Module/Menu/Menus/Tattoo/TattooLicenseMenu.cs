using System;
using System.Collections.Generic;
using System.Linq;
using GVRP.Module.Assets.Tattoo;
using GVRP.Module.Business;
using GVRP.Module.Houses;
using GVRP.Module.Items;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Tattoo;

namespace GVRP
{
    public class TattooLicenseMenuBuilder : MenuBuilder
    {
        public TattooLicenseMenuBuilder() : base(PlayerMenu.TattooLicenseMenu)
        {
        }

        public override Menu Build(DbPlayer iPlayer)
        {
            if (!iPlayer.HasTattooShop()) return null;

            List<TattooLicense> licenses = TattooLicenseModule.Instance.GetAll().Values.ToList();
            if (licenses == null || licenses.Count == 0) return null;

            var tattooshop = TattooShopFunctions.GetTattooShop(iPlayer);
            if (tattooshop == null) return null;

            var menu = new Menu(Menu, "Tattoo Licenses");

            var tattoos = new List<TattooLicenseList>();

            foreach (TattooLicense lic in licenses)
            {
                AssetsTattoo assetsTattoo = AssetsTattooModule.Instance.Get(lic.AssetsTattooId);
                if (assetsTattoo == null) continue;
                if (tattooshop.tattooLicenses.Find(t => t.AssetsTattooId == lic.AssetsTattooId) != null) continue;
                TattooLicenseList tat = new TattooLicenseList()
                {
                    Name = assetsTattoo.Name,
                    Price = lic.Price
                };
                tattoos.Add(tat);
            }

            tattoos = tattoos.OrderBy(t => t.Name).ToList();
            
            foreach(TattooLicenseList tatlic in tattoos)
            {
                menu.Add($"{tatlic.Name} {tatlic.Price}$");
            }

            menu.Add(MSG.General.Close());
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
                List<TattooLicense> licenses = TattooLicenseModule.Instance.GetAll().Values.ToList();
                int idx = 0;

                var tattooshop = TattooShopFunctions.GetTattooShop(iPlayer);
                if (tattooshop == null)
                {
                    MenuManager.DismissCurrent(iPlayer);
                    return false;
                }

                var tattoos = new List<TattooLicenseList>();

                foreach (TattooLicense lic in licenses)
                {
                    AssetsTattoo assetsTattoo = AssetsTattooModule.Instance.Get(lic.AssetsTattooId);
                    if (assetsTattoo == null) continue;
                    if (tattooshop.tattooLicenses.Find(t => t.AssetsTattooId == lic.AssetsTattooId) != null) continue;
                    TattooLicenseList tat = new TattooLicenseList()
                    {
                        Name = assetsTattoo.Name,
                        Price = lic.Price,
                        Id = lic.Id
                    };
                    tattoos.Add(tat);
                }

                tattoos = tattoos.OrderBy(t => t.Name).ToList();

                foreach(TattooLicenseList tatlic in tattoos)
                {
                    if (index == idx)
                    {
                        if (!iPlayer.TakeMoney(tatlic.Price))
                        {
                            iPlayer.SendNewNotification(MSG.Money.NotEnoughMoney(tatlic.Price));
                            return false;
                        }
                        TattooShop tattooShop = iPlayer.GetTattooShop();
                        TattooLicense lic = TattooLicenseModule.Instance.Get(tatlic.Id);
                        tattooShop.AddLicense(lic);

                        iPlayer.SendNewNotification($"Lizenz {tatlic.Name} erworben!");
                        return true;
                    }
                    idx++;
                }

                MenuManager.DismissCurrent(iPlayer);
                return false;
            }
        }
    }

    public class TattooLicenseList
    {
        public string Name;
        public int Price;
        public uint Id;
    }
}
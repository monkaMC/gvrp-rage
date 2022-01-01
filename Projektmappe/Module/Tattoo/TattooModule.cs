using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using GVRP.Handler;
using GVRP.Module.Assets.Hair;
using GVRP.Module.Assets.HairColor;
using GVRP.Module.Assets.Tattoo;
using GVRP.Module.Barber.Windows;
using GVRP.Module.Business;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Customization;
using GVRP.Module.GTAN;
using GVRP.Module.Logging;
using GVRP.Module.Menu;
using GVRP.Module.Players;

using GVRP.Module.Players.Db;
using GVRP.Module.Tattoo.Windows;

namespace GVRP.Module.Tattoo
{
    public class ClientTattoo
    {
        public string TattooHash { get; }
        public int ZoneId { get; }
        public int Price { get; }
        public string Name { get; }

        public ClientTattoo(string tattooHash, int zoneId, int price, string name)
        {
            TattooHash = tattooHash;
            ZoneId = zoneId;
            Price = price;
            Name = name;
        }
    }
    public sealed class TattooModule : Module<TattooModule>
    {
        public override Type[] RequiredModules()
        {
            return new[] { typeof(TattooShopModule) };
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key != Key.E || dbPlayer.Player.IsInVehicle) return false;

            if (dbPlayer.Player.Position.DistanceTo(new Vector3(-229.387f, -34.0732f, 49.5305f)) < 5.0f)
            {
                MenuManager.Instance.Build(PlayerMenu.TattooLicenseMenu, dbPlayer).Show(dbPlayer);
                return true;
            }

            if (!dbPlayer.TryData("tattooShopId", out uint tattooShopId)) return false;
            var tattooShop = TattooShopModule.Instance.Get(tattooShopId);
            if (tattooShop == null) return false;

            if (dbPlayer.Player.Position.DistanceTo(tattooShop.Position) > 5.0f) return false;

            if (tattooShop.BusinessId == 0)
            {
                MenuManager.Instance.Build(PlayerMenu.TattooBuyMenu, dbPlayer).Show(dbPlayer);
                return true;
            }

            List<ClientTattoo> cTattooList = new List<ClientTattoo>();

            if (tattooShop.tattooLicenses.Count <= 0)
            {
                return false;
            }

            if (dbPlayer.IsMemberOfBusiness() && dbPlayer.GetActiveBusinessMember().Manage && dbPlayer.GetActiveBusinessMember().BusinessId == tattooShop.BusinessId)
            {
                MenuManager.Instance.Build(PlayerMenu.TattooBankMenu, dbPlayer).Show(dbPlayer);
                return true;
            }
            else
            {
                foreach (TattooAddedItem tattooAddedItem in tattooShop.tattooLicenses)
                {
                    AssetsTattoo assetsTattoo = AssetsTattooModule.Instance.Get((uint)tattooAddedItem.AssetsTattooId);
                    if(assetsTattoo.GetHashForPlayer(dbPlayer) != "")
                    {
                        cTattooList.Add(new ClientTattoo(assetsTattoo.GetHashForPlayer(dbPlayer), assetsTattoo.ZoneId, assetsTattoo.Price, assetsTattoo.Name));
                    }
                }
                dbPlayer.SetTattooClothes();

                ComponentManager.Get<TattooShopWindow>().Show()(dbPlayer, cTattooList);
                return true;
            }
        }

        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if (!colShape.TryData("tattooShopId", out uint tattooShopId)) return false;
            switch (colShapeState)
            {
                case ColShapeState.Enter:
                    dbPlayer.SetData("tattooShopId", tattooShopId);
                    
                    return false;
                case ColShapeState.Exit:
                    if (!dbPlayer.HasData("tattooShopId")) return false;
                    dbPlayer.ResetData("tattooShopId");

                    // Resett Tattoo Sync
                    dbPlayer.ApplyCharacter();
                    
                    return false;
                default:
                    return false;
            }
        }
        
    }
}
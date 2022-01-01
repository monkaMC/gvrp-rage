using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using GVRP.Handler;
using GVRP.Module.Assets.Beard;
using GVRP.Module.Assets.Chest;
using GVRP.Module.Assets.Hair;
using GVRP.Module.Assets.HairColor;
using GVRP.Module.Barber.Windows;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Customization;
using GVRP.Module.GTAN;
using GVRP.Module.Logging;
using GVRP.Module.Players;

using GVRP.Module.Players.Db;
using GVRP.Module.Tattoo;

namespace GVRP.Module.Barber
{
    public sealed class BarberModule : Module<BarberModule>
    {
        public override Type[] RequiredModules()
        {
            return new[] { typeof(BarberShopModule), typeof(AssetsBeardModule), typeof(AssetsHairModule), typeof(AssetsHairColorModule), typeof(AssetsChestModule) };
        }
        
        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key != Key.E || dbPlayer.Player.IsInVehicle) return false;
            if (!dbPlayer.TryData("barberShopId", out uint barberShopId)) return false;
            var barberShop = BarberShopModule.Instance.Get(barberShopId);
            if (barberShop == null) return false;

            if (dbPlayer.Player.Position.DistanceTo(barberShop.Position) > 5.0f) return false;

            int gender = dbPlayer.Customization.Gender;

            try
            {
                dbPlayer.Player.ClearAccessory(0);
                dbPlayer.Player.ClearAccessory(1);
                dbPlayer.Player.ClearAccessory(2);
                dbPlayer.Player.ClearAccessory(6);
                dbPlayer.Player.ClearAccessory(7);
                TattooShopFunctions.SetTattooClothes(dbPlayer);
                
                if (dbPlayer.Customization.Gender == 0)
                {
                    ComponentManager.Get<BarberShopWindow>().Show()(dbPlayer, BarberShopModule.Instance.MaleListJsonBarberObject[barberShopId]);
                }
                else
                {
                    ComponentManager.Get<BarberShopWindow>().Show()(dbPlayer, BarberShopModule.Instance.FemaleListJsonBarberObject[barberShopId]);
                }

            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }

            return true;
        }

        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if (!colShape.TryData("barberShopId", out uint barberShopId)) return false;
            switch (colShapeState)
            {
                case ColShapeState.Enter:
                    dbPlayer.SetData("barberShopId", barberShopId);
                    var barberShop = BarberShopModule.Instance.Get(barberShopId);
                    dbPlayer.SendNewNotification("Benutze \"E\" um dir die Haare schneiden zu lassen", title: barberShop.Name, notificationType: PlayerNotification.NotificationType.INFO);
                    return false;
                case ColShapeState.Exit:
                    if (!dbPlayer.HasData("barberShopId")) return false;
                    dbPlayer.ResetData("barberShopId");
                    return false;
                default:
                    return false;
            }
        }
    }
}
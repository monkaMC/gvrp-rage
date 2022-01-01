using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Shops.Windows;

namespace GVRP.Module.Business.NightClubs
{
    public class NightClubsModule : Module<NightClubsModule>
    {
        public static Vector3 InteriorEnterPosition = new Vector3(-1569.44, -3017.32, -74.3962);
        public static float InteriorEnterRotation = 357.449f;

        public static Vector3 SellNpcPosition = new Vector3(-1585.06, -3012.83, -76.005);
        public static float SellNpcRotation = 82.8533f;

        public static Vector3 SellNpc3Position = new Vector3(-1610.44, -3019.02, -75.205);
        public static float SellNpc3Rotation = 187.485f;

        public static Vector3 SellNpc2Position = new Vector3(-1577.73, -3016.62, -76.006);
        public static float SellNpc2Rotation = 0.344802f;

        public static Vector3 GarageOutPosition = new Vector3(-1642.37, -2989.79, -77.3895);
        public static float GarageOutFloat = 267.708f;

        public static Vector3 InventoryPosition = new Vector3(-1615.48, -2996.74, -78.1497);

        public static Vector3 ComputerPosition = new Vector3(-1617.43, -3013.3, -75.2051);

        protected override bool OnLoad()
        {
            MenuManager.Instance.AddBuilder(new NightClubManageMenuBuilder());
            MenuManager.Instance.AddBuilder(new NightClubMenuBuilder());
            MenuManager.Instance.AddBuilder(new NightClubPriceMenuBuilder());
            MenuManager.Instance.AddBuilder(new NightClubAnpassungMenuBuilder());
            MenuManager.Instance.AddBuilder(new NightClubAnpassungHandlerMenuBuilder());
            return base.OnLoad();
        }

        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if (colShape.HasData("nightclubId"))
            {
                NightClub nightClub = NightClubModule.Instance.Get(colShape.GetData("nightclubId"));
                
                if (nightClub == null) return false;

                if (colShapeState == ColShapeState.Enter)
                {
                    dbPlayer.SetData("nightclubId", nightClub.Id);
                    Dictionary<String, String> temp = new Dictionary<string, string>();

                    if (nightClub.IsOwnedByBusines())
                    {
                        temp.Add("Besitzer", nightClub.OwnerBusiness.Name);
                    }
                    else
                    {
                        temp.Add("Kein Eigentümer", $"Zum Verkauf (${nightClub.Price})");
                    }

                    dbPlayer.Player.TriggerEvent("sendInfocard", nightClub.Name, "red", "nightclubs.jpg", 20000, JsonConvert.SerializeObject(temp));
                    return true;
                }
                else if (colShapeState == ColShapeState.Exit)
                {
                    if (dbPlayer.HasData("nightclubId"))
                    {
                        dbPlayer.ResetData("nightclubId");
                    }

                    return true;
                }
            }
            else if (colShape.HasData("nightclubInterriorColshape"))
            {
                NightClub nightClub = NightClubModule.Instance.GetByDimension(dbPlayer.Player.Dimension);
                if (nightClub == null) return false;

                if (colShapeState == ColShapeState.Enter)
                {
                    dbPlayer.SetData("insideNightclub", nightClub.Id);

                    nightClub.LoadNightCLubInterrior(dbPlayer);

                    if (!nightClub.PlayersInsideClub.Contains(dbPlayer))
                    {
                        nightClub.PlayersInsideClub.Add(dbPlayer);
                    }
                }
                else if (colShapeState == ColShapeState.Exit)
                {
                    if (dbPlayer.HasData("insideNightclub"))
                    {
                        dbPlayer.ResetData("insideNightclub");

                        nightClub.UnloadNightClubInterior(dbPlayer);

                        if (nightClub.PlayersInsideClub.Contains(dbPlayer))
                        {
                            nightClub.PlayersInsideClub.Remove(dbPlayer);
                        }
                    }
                }
            }

            return false;
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key == Key.E)
            {
                // Buys... :)
                if (dbPlayer.Player.Dimension != 0 && (dbPlayer.Player.Position.DistanceTo(SellNpcPosition) < 4.0 || dbPlayer.Player.Position.DistanceTo(SellNpc2Position) < 4.0 || dbPlayer.Player.Position.DistanceTo(SellNpc3Position) < 4.0))
                {
                    NightClub nightClub = NightClubModule.Instance.Get(dbPlayer.Player.Dimension);
                    if (nightClub != null)
                    {
                        dbPlayer.SetData("nightClubShopId", nightClub.Id);
                        ComponentManager.Get<ShopWindow>().Show()(dbPlayer, nightClub.Name, (int)nightClub.Id, nightClub.ConvertForShop());
                        return true;
                    }
                }
                // Computer
                if (dbPlayer.Player.Dimension != 0 && dbPlayer.Player.Position.DistanceTo(ComputerPosition) < 2.5f)
                {
                    NightClub nightClub = NightClubModule.Instance.Get(dbPlayer.Player.Dimension);
                    if (nightClub != null && dbPlayer.IsMemberOfBusiness() && dbPlayer.ActiveBusiness.BusinessBranch.NightClubId == nightClub.Id)
                    {
                        MenuManager.Instance.Build(PlayerMenu.NightClubManageMenu, dbPlayer).Show(dbPlayer);
                        return true;
                    }
                }
                // Enter/Exit
                if (dbPlayer.HasData("nightclubId"))
                {
                    NightClub nightClub = NightClubModule.Instance.Get(dbPlayer.GetData("nightclubId"));
                    if (nightClub != null)
                    {
                        if (nightClub.Position.DistanceTo(dbPlayer.Player.Position) < 2.0f && !dbPlayer.Player.IsInVehicle)
                        {
                            if (!nightClub.IsOwnedByBusines())
                            {
                                MenuManager.Instance.Build(PlayerMenu.NightClubMenu, dbPlayer).Show(dbPlayer);
                                return true;
                            }
                            else
                            {
                                // Enter
                                if (!nightClub.Locked)
                                {
                                    dbPlayer.Player.SetPosition(InteriorEnterPosition);
                                    dbPlayer.Player.SetRotation(InteriorEnterRotation);
                                    dbPlayer.Player.Dimension = nightClub.Id;
                                    return true;
                                }
                            }
                        }
                        else
                        {
                            // Enter
                            if (!nightClub.GarageLocked)
                            {
                                if (dbPlayer.Player.IsInVehicle)
                                {
                                    dbPlayer.Player.Vehicle.Position = GarageOutPosition;
                                    dbPlayer.Player.Vehicle.Rotation = new Vector3(0, 0, GarageOutFloat);
                                    dbPlayer.Player.Vehicle.Dimension = nightClub.Id;
                                    dbPlayer.Player.Dimension = nightClub.Id;
                                }
                                else
                                {
                                    dbPlayer.Player.SetPosition(GarageOutPosition);
                                    dbPlayer.Player.SetRotation(GarageOutFloat);
                                    dbPlayer.Player.Dimension = nightClub.Id;
                                }
                                return true;
                            }
                        }

                    }
                }
                if (dbPlayer.Player.Dimension != 0 && dbPlayer.Player.Position.DistanceTo(InteriorEnterPosition) < 2.0f && !dbPlayer.Player.IsInVehicle)
                {
                    NightClub nightClub = NightClubModule.Instance.Get(dbPlayer.Player.Dimension);
                    if (nightClub != null && !nightClub.Locked)
                    {
                        dbPlayer.Player.SetPosition(nightClub.Position);
                        dbPlayer.Player.SetRotation(nightClub.Rotation);
                        dbPlayer.Player.Dimension = 0;
                        return true;
                    }
                }
                if (dbPlayer.Player.Dimension != 0 && dbPlayer.Player.Position.DistanceTo(GarageOutPosition) < 2.0f)
                {
                    NightClub nightClub = NightClubModule.Instance.Get(dbPlayer.Player.Dimension);
                    if (nightClub != null && !nightClub.GarageLocked)
                    {
                        if (dbPlayer.Player.IsInVehicle)
                        {
                            dbPlayer.Player.Vehicle.Position = nightClub.GaragePosition;
                            dbPlayer.Player.Vehicle.Rotation = new Vector3(0, 0, nightClub.GarageRotation);
                            dbPlayer.Player.Vehicle.Dimension = 0;
                            dbPlayer.Player.Dimension = 0;
                            return true;
                        }
                        else
                        {
                            dbPlayer.Player.SetPosition(nightClub.GaragePosition);
                            dbPlayer.Player.SetRotation(nightClub.GarageRotation);
                            dbPlayer.Player.Dimension = 0;
                            return true;
                        }
                    }
                }
            }
            if (key == Key.L)
            {
                if (dbPlayer.HasData("nightclubId"))
                {
                    NightClub nightClub = NightClubModule.Instance.Get(dbPlayer.GetData("nightclubId"));
                    if (nightClub != null)
                    {
                        if (nightClub.Position.DistanceTo(dbPlayer.Player.Position) < 2.0f)
                        {
                            if (nightClub.IsOwnedByBusines() && dbPlayer.IsMemberOfBusiness() && dbPlayer.ActiveBusiness.BusinessBranch.NightClubId == nightClub.Id)
                            {
                                if (!nightClub.Locked)
                                {
                                    nightClub.Locked = true;
                                    dbPlayer.SendNewNotification(nightClub.Name + " abgeschlossen!", title: "Nightclub", notificationType: PlayerNotification.NotificationType.ERROR);
                                    return true;
                                }
                                else
                                {
                                    nightClub.Locked = false;
                                    dbPlayer.SendNewNotification(nightClub.Name + " aufgeschlossen!", title: "Nightclub", notificationType: PlayerNotification.NotificationType.SUCCESS);
                                    return true;
                                }
                            }
                        }
                        if (nightClub.GaragePosition.DistanceTo(dbPlayer.Player.Position) < 2.0f)
                        {
                            if (nightClub.IsOwnedByBusines() && dbPlayer.IsMemberOfBusiness() && dbPlayer.ActiveBusiness.BusinessBranch.NightClubId == nightClub.Id)
                            {
                                if (!nightClub.GarageLocked)
                                {
                                    nightClub.GarageLocked = true;
                                    dbPlayer.SendNewNotification(nightClub.Name + " Garage abgeschlossen!", title: "Garage", notificationType: PlayerNotification.NotificationType.ERROR);
                                    return true;
                                }
                                else
                                {
                                    nightClub.GarageLocked = false;
                                    dbPlayer.SendNewNotification(nightClub.Name + " Garage aufgeschlossen!", title: "Garage", notificationType: PlayerNotification.NotificationType.SUCCESS);
                                    return true;
                                }
                            }
                        }
                    }
                }
                if (dbPlayer.Player.Dimension != 0 && dbPlayer.Player.Position.DistanceTo(InteriorEnterPosition) < 2.0f)
                {
                    NightClub nightClub = NightClubModule.Instance.Get(dbPlayer.Player.Dimension);
                    if (nightClub != null)
                    {
                        if (nightClub.IsOwnedByBusines() && dbPlayer.IsMemberOfBusiness() && dbPlayer.ActiveBusiness.BusinessBranch.NightClubId == nightClub.Id)
                        {
                            if (!nightClub.Locked)
                            {
                                nightClub.Locked = true;
                                dbPlayer.SendNewNotification(nightClub.Name + " abgeschlossen!", title: "Nightclub", notificationType: PlayerNotification.NotificationType.ERROR);
                                return true;
                            }
                            else
                            {
                                nightClub.Locked = false;
                                dbPlayer.SendNewNotification(nightClub.Name + " aufgeschlossen!", title: "Nightclub", notificationType: PlayerNotification.NotificationType.SUCCESS);
                                return true;
                            }
                        }
                    }
                }
                if (dbPlayer.Player.Dimension != 0 && dbPlayer.Player.Position.DistanceTo(GarageOutPosition) < 2.0f)
                {
                    NightClub nightClub = NightClubModule.Instance.Get(dbPlayer.Player.Dimension);
                    if (nightClub != null)
                    {
                        if (nightClub.IsOwnedByBusines() && dbPlayer.IsMemberOfBusiness() && dbPlayer.ActiveBusiness.BusinessBranch.NightClubId == nightClub.Id)
                        {
                            if (!nightClub.GarageLocked)
                            {
                                nightClub.GarageLocked = true;
                                dbPlayer.SendNewNotification(nightClub.Name + " Garage abgeschlossen!", title: "Garage", notificationType: PlayerNotification.NotificationType.ERROR);
                                return true;
                            }
                            else
                            {
                                nightClub.GarageLocked = false;
                                dbPlayer.SendNewNotification(nightClub.Name + " Garage aufgeschlossen!", title: "Garage", notificationType: PlayerNotification.NotificationType.SUCCESS);
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}
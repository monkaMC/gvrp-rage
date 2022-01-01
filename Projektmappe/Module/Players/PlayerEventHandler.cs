using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GTANetworkAPI;
using Newtonsoft.Json;
using GVRP.Handler;
using GVRP.Module.AnimationMenu;
using GVRP.Module.Attachments;
using GVRP.Module.Chat;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Clothes;
using GVRP.Module.Clothes.Props;
using GVRP.Module.Clothes.Team;
using GVRP.Module.Configurations;
using GVRP.Module.Crime;
using GVRP.Module.Injury;
using GVRP.Module.Items;
using GVRP.Module.Kasino;
using GVRP.Module.Keys;
using GVRP.Module.Keys.Windows;
using GVRP.Module.Logging;
using GVRP.Module.Menu;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Phone;
using GVRP.Module.Players.PlayerAnimations;
using GVRP.Module.Players.Windows;
using GVRP.Module.RemoteEvents;
using GVRP.Module.Schwarzgeld;
using GVRP.Module.Staatskasse;
using GVRP.Module.Swat;
using GVRP.Module.Teams;
using GVRP.Module.Vehicles;
using GVRP.Module.Weapons.Data;
using static GVRP.AnimationContent;

namespace GVRP.Module.Players
{
    public class PlayerEventHandler : Script
    {
        //TODO: Fix clientside
        [RemoteEventPermission]
        [RemoteEvent]
        public void REQUEST_PEDS_PLAYER_GIVEMONEY_DIALOG(Client client, Client destinationPlayer)
        {
            var dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.CanAccessRemoteEvent()) return;
            var destinationDbPlayer = destinationPlayer.GetPlayer();
            if (destinationDbPlayer == null || !destinationDbPlayer.IsValid()) return;
            if (destinationDbPlayer.Id == dbPlayer.Id) return;
            if (destinationDbPlayer.Player.Position.DistanceTo(dbPlayer.Player.Position) > 20f) return;

            dbPlayer.SetData("sInteraction", destinationDbPlayer);
            ComponentManager.Get<GiveMoneyWindow>().Show()(dbPlayer, destinationDbPlayer);
        }

        //TODO: Fix clientside
        [RemoteEventPermission]
        [RemoteEvent]
        public void REQUEST_PEDS_PLAYER_SHOW_PERSO(Client client, Client destinationPlayer)
        {
            var dbPlayer = client.GetPlayer();
            if (!dbPlayer.CanAccessRemoteEvent()) return;
            var destinationDbPlayer = destinationPlayer.GetPlayer();
            if (!destinationDbPlayer.IsValid()) return;
            if (destinationDbPlayer.Id == dbPlayer.Id) return;
            if (destinationDbPlayer.Player.Position.DistanceTo(dbPlayer.Player.Position) > 20f) return;

            if (dbPlayer.hasPerso[0] == 0)
            {
                dbPlayer.SendNewNotification("Du besitzt keinen Personalausweis!");
                return;
            }

            dbPlayer.SendNewNotification("Sie haben Ihre Personalien gezeigt!");
            dbPlayer.ShowIdCard(destinationPlayer);
        }

        //TODO: Fix clientside
        [RemoteEventPermission]
        [RemoteEvent]
        public void REQUEST_PEDS_PLAYER_GETPERSO(Client client, Client destinationPlayer)
        {
            var dbPlayer = client.GetPlayer();
            if (!dbPlayer.CanAccessRemoteEvent()) return;
            var destinationDbPlayer = destinationPlayer.GetPlayer();
            if (!destinationDbPlayer.IsValid()) return;
            if (destinationDbPlayer.Id == dbPlayer.Id) return;
            if (destinationDbPlayer.Player.Position.DistanceTo(dbPlayer.Player.Position) > 2.5f) return;


            if (destinationDbPlayer.isInjured() || destinationDbPlayer.IsTied || destinationDbPlayer.IsCuffed)
            {
                if (destinationDbPlayer.hasPerso[0] == 0 || destinationDbPlayer.IsSwatDuty())
                {
                    dbPlayer.SendNewNotification("Spieler hat keinen Perso!");
                }
                else
                {
                    dbPlayer.SendNewNotification("Sie haben den Personalausweis genommen!");
                    destinationDbPlayer.ShowIdCard(client);
                }
            }


        }

        [RemoteEventPermission]
        [RemoteEvent]
        public void REQUEST_PEDS_PLAYER_GIVEKEY(Client client, Client destinationPlayer)
        {
            var dbPlayer = client.GetPlayer();
            if (!dbPlayer.CanAccessRemoteEvent()) return;
            var destinationDbPlayer = destinationPlayer.GetPlayer();
            if (!destinationDbPlayer.IsValid()) return;
            if (destinationDbPlayer.Player.Position.DistanceTo(dbPlayer.Player.Position) > 2.5f) return;

            Dictionary<string, List<VHKey>> keys = new Dictionary<string, List<VHKey>>();

            List<VHKey> houses = HouseKeyHandler.Instance.GetOwnHouseKey(dbPlayer);
            List<VHKey> vehicles = VehicleKeyHandler.Instance.GetOwnVehicleKeys(dbPlayer);
            List<VHKey> storages = StorageKeyHandler.Instance.GetOwnStorageKey(dbPlayer);

            keys.Add("Häuser", houses);
            keys.Add("Fahrzeuge", vehicles);
            keys.Add("Lagerräume", storages);

            ComponentManager.Get<KeyWindow>().Show()(dbPlayer, destinationDbPlayer.GetName(), keys);
        }

        [RemoteEventPermission]
        [RemoteEvent]
        public void REQUEST_PEDS_PLAYER_GIVEITEM(Client client, Client destinationPlayer)
        {
            var dbPlayer = client.GetPlayer();
            if (!dbPlayer.IsValid()) return;

            if (!dbPlayer.CanAccessRemoteEvent())
            {
                dbPlayer.SendNewNotification( MSG.Error.NoPermissions());
                return;
            }

            var destinationDbPlayer = destinationPlayer.GetPlayer();
            if (!destinationDbPlayer.IsValid()) return;
            if (destinationDbPlayer.Player.Position.DistanceTo(dbPlayer.Player.Position) > 2.5f) return;
            
            dbPlayer.SetData("giveitem", destinationDbPlayer.Id);
            ItemsModuleEvents.RequestInventory(client);
        }

        [RemoteEventPermission]
        [RemoteEvent]
        public void requestPlayerKeys(Client client)
        {
            var dbPlayer = client.GetPlayer();
            if (!dbPlayer.CanAccessRemoteEvent()) return;

            Dictionary<string, List<VHKey>> keys = new Dictionary<string, List<VHKey>>();

            List<VHKey> houses = HouseKeyHandler.Instance.GetAllKeysPlayerHas(dbPlayer);
            List<VHKey> vehicles = VehicleKeyHandler.Instance.GetAllKeysPlayerHas(dbPlayer);
            List<VHKey> storages = StorageKeyHandler.Instance.GetAllKeysPlayerHas(dbPlayer);

            keys.Add("Häuser", houses);
            keys.Add("Fahrzeuge", vehicles);
            keys.Add("Lagerräume", storages);

            ComponentManager.Get<KeyWindow>().Show()(dbPlayer, null, keys);
        }

        [RemoteEventPermission]
        [RemoteEvent]
        public void packblackmoney(Client client)
        {
            var dbPlayer = client.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || dbPlayer.IsTied || dbPlayer.IsCuffed || !dbPlayer.CanInteract()) return;

            if (dbPlayer.blackmoney[0] > 0)
            {
                int blAmount = dbPlayer.blackmoney[0];
                int freeSlots = dbPlayer.Container.GetInventoryFreeSlots();
                int maxAddableAmount = freeSlots * ItemModelModule.Instance.GetById(SchwarzgeldModule.SchwarzgeldId).MaximumStacksize;
                if(maxAddableAmount < blAmount)
                {
                    blAmount = maxAddableAmount;
                }

                if (!dbPlayer.Container.CanInventoryItemAdded(SchwarzgeldModule.SchwarzgeldId, blAmount))
                {
                    dbPlayer.SendNewNotification($"Dafür reicht dein Inventar nicht aus!");
                    return;
                }

                Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                {
                    Chats.sendProgressBar(dbPlayer, 5000);

                    dbPlayer.Player.TriggerEvent("freezePlayer", true);
                    dbPlayer.SetData("userCannotInterrupt", true);
                    dbPlayer.PlayAnimation(
                        (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["fixing"].Split()[0], Main.AnimationList["fixing"].Split()[1]);

                    dbPlayer.TakeBlackMoney(blAmount);

                    await Task.Delay(5000);
                    
                    dbPlayer.Player.TriggerEvent("freezePlayer", false);
                    dbPlayer.ResetData("userCannotInterrupt");
                    NAPI.Player.StopPlayerAnimation(dbPlayer.Player);
                    
                    dbPlayer.Container.AddItem(SchwarzgeldModule.SchwarzgeldId, blAmount);
                    dbPlayer.SendNewNotification($"Du hast dein Schwarzgeld gepackt (${blAmount})!");
                }));
                
                return;
            }
        }


        //TODO: Fix clientside / missing Seil
        [RemoteEventPermission(AllowedOnCuff = false, AllowedOnTied = false)]
        [RemoteEvent]
        public void REQUEST_PEDS_PLAYER_TIE(Client client, Client destinationPlayer)
        {
            var dbPlayer = client.GetPlayer();
            if (!dbPlayer.CanAccessRemoteEvent()) return;
            var destinationDbPlayer = destinationPlayer.GetPlayer();
            if (!destinationDbPlayer.IsValid()) return;
            if (destinationDbPlayer.Id == dbPlayer.Id) return;
            if (destinationDbPlayer.Player.Position.DistanceTo(dbPlayer.Player.Position) > 2.5f) return;
            
            if (destinationDbPlayer.isInjured() || destinationDbPlayer.IsCuffed)
            {
                return;
            }

            // tie or untie the player
            if (destinationDbPlayer.IsTied)
            {
                // is already tied - everybody can untie this person
                destinationDbPlayer.SetTied(false);

                // send messages and /me animations
                dbPlayer.SendNewNotification("Sie haben jemanden entfesselt!");
                destinationDbPlayer.SendNewNotification("Jemand hat Sie entfesselt!");
                return;
            }
            else
            {
                // is not tied                                    
                // verify has requiered item in inventory
                if (dbPlayer.Container.GetItemAmount(988) == 0)
                {
                    dbPlayer.SendNewNotification("Sie benoetigen ein Seil um einen Spieler zu fesseln!");
                    return;
                }

                // remove one item from iunventory
                dbPlayer.Container.RemoveItem(988, 1);

                // is already tied - everybody can untie this person
                destinationDbPlayer.SetTied(true);

                // Cancel phone call when tied
                if (PhoneCall.IsPlayerInCall(destinationDbPlayer.Player))
                {
                    destinationDbPlayer.CancelPhoneCall();
                }

                // send messages and /me animations
                dbPlayer.SendNewNotification("Sie haben jemanden gefesselt!");
                destinationDbPlayer.SendNewNotification("Jemand hat Sie gefesselt!");
            }
        }

        [RemoteEventPermission]
        [RemoteEvent]
        public void REQUEST_PEDS_PLAYER_CASINO(Client client, Client destinationPlayer)
        {
            var dbPlayer = client.GetPlayer();
            if (!dbPlayer.CanAccessRemoteEvent()) return;

            if (!dbPlayer.IsInCasinoDuty()) return;
            var destinationDbPlayer = destinationPlayer.GetPlayer();
            if (!destinationDbPlayer.IsValid()) return;
            if (destinationDbPlayer.Id == dbPlayer.Id) return;
            if (destinationDbPlayer.Player.Position.DistanceTo(dbPlayer.Player.Position) > 5.0f) return;

            if (KasinoModule.Instance.CasinoGuests.Contains(destinationDbPlayer))
            {
                dbPlayer.SendNewNotification($"Casino Zugang entzogen für Kunde {destinationDbPlayer.Player.Name}", PlayerNotification.NotificationType.ERROR);
                destinationDbPlayer.SendNewNotification($"Casino Zugang entzogen", PlayerNotification.NotificationType.ERROR);
                KasinoModule.Instance.CasinoGuests.Remove(destinationDbPlayer);
            }
            else
            {
                dbPlayer.SendNewNotification($"Casino Zugang gewährt für Kunde {destinationDbPlayer.Player.Name}", PlayerNotification.NotificationType.SUCCESS);
                destinationDbPlayer.SendNewNotification($"Casino Zugang gewährt", PlayerNotification.NotificationType.SUCCESS);
                KasinoModule.Instance.CasinoGuests.Add(destinationDbPlayer);
            }


        }




        //TODO: Fix serverside
        [RemoteEventPermission]
        [RemoteEvent]
        public void REQUEST_PEDS_PLAYER_CUFF(Client client, Client destinationPlayer)
        {
            var dbPlayer = client.GetPlayer();
            if (!dbPlayer.CanAccessRemoteEvent()) return;

            if (!dbPlayer.IsInCasinoDuty())
            {
                if ((!dbPlayer.IsACop() && !dbPlayer.IsGoverment()) || !dbPlayer.IsInDuty() || dbPlayer.IsCuffed || dbPlayer.IsTied) return;
            }

            var destinationDbPlayer = destinationPlayer.GetPlayer();
            if (!destinationDbPlayer.IsValid()) return;
            if (destinationDbPlayer.Id == dbPlayer.Id) return;
            if (destinationDbPlayer.Player.Position.DistanceTo(dbPlayer.Player.Position) > 3.2f) return;

            if (destinationDbPlayer.isInjured()) return;
            if (!client.IsInVehicle && !destinationDbPlayer.Player.IsInVehicle)
            {
                if (client.IsInVehicle || destinationDbPlayer.Player.IsInVehicle) return;
                if ((destinationDbPlayer.IsCuffed || destinationDbPlayer.IsTied) && !destinationDbPlayer.HasData("SMGkilledPos"))
                {
                    destinationDbPlayer.SetCuffed(false);
                    destinationDbPlayer.SetTied(false);
                    dbPlayer.SendNewNotification("Sie haben jemanden die Handschellen abgenommen!");
                    destinationDbPlayer.SendNewNotification("Ein Beamter hat Ihnen die Handschellen abgenommen!");
                    return;
                }
                else
                {
                    // Cancel phone call when arrested
                    if (PhoneCall.IsPlayerInCall(destinationDbPlayer.Player))
                    {
                        destinationDbPlayer.CancelPhoneCall();
                    }

                    destinationDbPlayer.SetCuffed(true);
                    dbPlayer.SendNewNotification("Sie haben jemanden die Handschellen angelegt!");
                    destinationDbPlayer.SendNewNotification("Ein Beamter hat Ihnen die Handschellen angelegt!");
                }
            }
        }

        [RemoteEventPermission]
        [RemoteEvent]
        public async Task REQUEST_PEDS_PLAYER_FRISK(Client client, Client destinationPlayer)
        {
            var dbPlayer = client.GetPlayer();
            if (!dbPlayer.CanAccessRemoteEvent()) return;
            if (dbPlayer.IsCuffed || dbPlayer.IsTied) return;
            var destinationDbPlayer = destinationPlayer.GetPlayer();
            if (!destinationDbPlayer.IsValid()) return;
            if (destinationDbPlayer.Id == dbPlayer.Id) return;
            if (destinationDbPlayer.Player.Position.DistanceTo(dbPlayer.Player.Position) > 3.2f) return;

            ItemsModuleEvents.resetFriskInventoryFlags(dbPlayer);
            ItemsModuleEvents.resetDisabledInventoryFlag(dbPlayer);
            
            if (!destinationDbPlayer.IsCuffed && !destinationDbPlayer.IsTied && !destinationDbPlayer.isInjured())
            {
                dbPlayer.SendNewNotification("Person ist nicht gefesselt", notificationType: PlayerNotification.NotificationType.ERROR);
                return;
            }

            if (!dbPlayer.HasData("lastfriskperson") || dbPlayer.GetData("lastfriskperson") != destinationDbPlayer.Id)
            {
                dbPlayer.Player.TriggerEvent("freezePlayer", true);
                dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@prop_human_parking_meter@male@base", "base");

                Chats.sendProgressBar(dbPlayer, 8000);
                await Task.Delay(8000);

                dbPlayer.Player.TriggerEvent("freezePlayer", false);
                dbPlayer.Player.StopAnimation();
            }

            dbPlayer.SetData("lastfriskperson", destinationDbPlayer.Id);

            var lWeapons = destinationDbPlayer.Weapons;
            if (lWeapons.Count > 0)
            {
                var lWeaponListContainer = new List<WeaponListContainer>();
                foreach (var lWeapon in lWeapons)
                {
                    var lData = WeaponDataModule.Instance.Get(lWeapon.WeaponDataId);
                    var weapon = ItemModelModule.Instance.GetByScript("w_" + Convert.ToString(lData.Name.ToLower()));
                    if (weapon == null) continue;
                    lWeaponListContainer.Add(new WeaponListContainer(lData.Name, lWeapon.Ammo, weapon.ImagePath));
                }

                if (dbPlayer.IsACop() && dbPlayer.Duty)
                {
                    dbPlayer.SetData("disableFriskInv", true);
                    dbPlayer.SetData("friskInvUserID", destinationDbPlayer.Id);
                    dbPlayer.SetData("friskInvUserName", destinationDbPlayer.GetName());
                }
                else
                {
                    dbPlayer.SetData("disableinv", true);
                }

                var lWeaponListObject = new WeaponListObject(destinationDbPlayer.GetName(), dbPlayer.IsACop(), lWeaponListContainer);
                ComponentManager.Get<FriskWindow>().Show()(dbPlayer, lWeaponListObject);
                return;
            }


            ItemsModuleEvents.resetFriskInventoryFlags(dbPlayer);
            ItemsModuleEvents.resetDisabledInventoryFlag(dbPlayer);
            
            dbPlayer.SetData("friskInvUserID", destinationDbPlayer.Id);
            destinationDbPlayer.Container.ShowFriskInventory(dbPlayer, destinationDbPlayer, "Spieler", (destinationDbPlayer.money[0] + destinationDbPlayer.blackmoney[0]));

            if (destinationDbPlayer.blackmoney[0] > 0 && dbPlayer.TeamId == (int)teams.TEAM_FIB)
            {
                dbPlayer.SendNewNotification($"Sie konnten von ${(destinationDbPlayer.money[0] + destinationDbPlayer.blackmoney[0])} insgesamt ${destinationDbPlayer.blackmoney[0]} Schwarzgeld feststellen! (/takebm zum entfernen)");
            }
        }

        //TODO: Fix serverside
        [RemoteEventPermission]
        [RemoteEvent]
        public void REQUEST_PEDS_PLAYER_TAKEPERSON(Client client, Client destinationPlayer)
        {
            var dbPlayer = client.GetPlayer();
            if (!dbPlayer.CanAccessRemoteEvent()) return;
            
            var destinationDbPlayer = destinationPlayer.GetPlayer();
            if (!destinationDbPlayer.IsValid()) return;
            if (destinationDbPlayer.Id == dbPlayer.Id) return;
            if (destinationDbPlayer.Player.Position.DistanceTo(dbPlayer.Player.Position) > 2.5f) return;
            
            if (client.IsInVehicle || destinationDbPlayer.Player.IsInVehicle)
            {
                dbPlayer.SendNewNotification(
                    
                    "Sie oder die Person duerfen nicht in einem Fahrzeug sein!");
                return;
            }

            if (!destinationDbPlayer.HasData("follow"))
            {
                if (!destinationDbPlayer.IsCuffed && !destinationDbPlayer.IsTied)
                {
                    dbPlayer.SendNewNotification(
                         "Spieler ist nicht gefesselt/gecuffed!");
                    return;
                }

                dbPlayer.SendNewNotification(
              "Sie haben jemanden gepackt!");
                destinationDbPlayer.SendNewNotification(
              "Jemand hat Sie gepackt!");
                destinationDbPlayer.SetCuffed(false);
                destinationDbPlayer.SetData("follow", client.Name);
                destinationDbPlayer.Player.TriggerEvent("toggleShooting", true);
                destinationDbPlayer.PlayAnimation(AnimationScenarioType.Animation,"anim@move_m@prisoner_cuffed_rc","aim_low_loop", -1, true, AnimationLevels.UserCop,
                    (int) (AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.Loop |
                           AnimationFlags.AllowPlayerControl), true);
            }
            else
            {
                destinationDbPlayer.StopAnimation();
                dbPlayer.SendNewNotification(
              "Sie haben jemanden losgelassen!");
                destinationDbPlayer.SendNewNotification(
              "Jemand hat Sie losgelassen!");
                destinationDbPlayer.SetCuffed(true);
                destinationDbPlayer.ResetData("follow");
                destinationDbPlayer.Player.TriggerEvent("toggleShooting", false);
            }
        }
        
        //TODO: Check
        [RemoteEventPermission]
        [RemoteEvent]
        public async Task REQUEST_PEDS_PLAYER_STABALIZE(Client client, Client destinationPlayer)
        {
            
            var dbPlayer = client.GetPlayer();
            var destinationDbPlayer = destinationPlayer.GetPlayer();

            if (!destinationDbPlayer.IsValid()) return;

            if (destinationDbPlayer.Id == dbPlayer.Id) return;
            if (destinationDbPlayer.Player.Position.DistanceTo(dbPlayer.Player.Position) > 5.0f) return;
            if (!destinationDbPlayer.isInjured()) return;

            if (!client.IsInVehicle)
            {
                uint stabilizeItemId = destinationDbPlayer.Injury.ItemToStabilizeId;

                if (destinationDbPlayer.Injury.Id == InjuryModule.Instance.InjuryDeathScreenId)
                {
                    dbPlayer.SendNewNotification("Es scheint als wäre die Person ins Koma gefallen!");
                    return;
                }

                if (stabilizeItemId != 0 && destinationDbPlayer.Injury.Id != InjuryModule.Instance.InjuryKrankentransport)
                {
                    InjuryPlayerExtension.Medicate(destinationDbPlayer, dbPlayer);
                }
                else
                {

                    if ((destinationDbPlayer.Injury.Id == InjuryModule.Instance.InjuryKrankentransport) && 
                    (dbPlayer.IsAMedic() && dbPlayer.IsInDuty() || dbPlayer.ParamedicLicense))
                    {
                        // Anpassung für Revive in KH1
                        InjuryDeliverIntPoint injuryDeliveryPoint = InjuryDeliverIntPointModule.Instance.GetAll().FirstOrDefault(dlp => dlp.Value.Position.DistanceTo(dbPlayer.Player.Position) < 13.0f).Value;

                        if (injuryDeliveryPoint != null)
                        {
                            await Task.Run(async () =>
                            {
                                dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl),
                                    Main.AnimationList["revive"].Split()[0], Main.AnimationList["revive"].Split()[1]);
                                dbPlayer.Player.TriggerEvent("freezePlayer", true);
                                if (dbPlayer.IsAGangster())
                                {
                                    Chats.sendProgressBar(dbPlayer, 15000);
                                    await Task.Delay(15000);
                                }
                                else
                                {
                                    Chats.sendProgressBar(dbPlayer, 9000);
                                    await Task.Delay(9000);
                                }
                                if (dbPlayer.IsCuffed || dbPlayer.IsTied || dbPlayer.isInjured())
                                {
                                    dbPlayer.SendNewNotification("Stabilisierung fehlgeschlagen!");
                                    return;
                                }
                                dbPlayer.Player.TriggerEvent("freezePlayer", false);
                                NAPI.Player.StopPlayerAnimation(dbPlayer.Player);
                                destinationDbPlayer.revive();
                                destinationDbPlayer.SendNewNotification($"Du wurdest vom Medic behandelt!");
                                dbPlayer.SendNewNotification($"Du hast den Patienten behandelt!");
                            });
                            return;
                        }
                        else if(destinationDbPlayer.Injury.Id == InjuryModule.Instance.InjuryKrankentransport) // Krankentransport Bug
                        {
                            SxVehicle sxVehicle = VehicleHandler.Instance.GetClosestVehicleFromTeamFilter(dbPlayer.Player.Position, (int)dbPlayer.TeamId, 15.0f, 4);

                            if (sxVehicle == null)
                            {
                                dbPlayer.SendNewNotification($"Kein Krankenwagen zum Transport in der naehe!");
                                return;
                            }

                            await Task.Run(async () =>
                            {
                                dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl),
                                    Main.AnimationList["revive"].Split()[0], Main.AnimationList["revive"].Split()[1]);
                                dbPlayer.Player.TriggerEvent("freezePlayer", true);
                                if (dbPlayer.IsAGangster())
                                {
                                    Chats.sendProgressBar(dbPlayer, 15000);
                                    await Task.Delay(15000);
                                }
                                else
                                {
                                    Chats.sendProgressBar(dbPlayer, 9000);
                                    await Task.Delay(9000);
                                }
                                VehicleHandler.Instance.TrySetPlayerIntoVehicleOccupants(sxVehicle, destinationDbPlayer);
                                await Task.Delay(1000);
                                dbPlayer.Player.TriggerEvent("freezePlayer", false);
                                NAPI.Player.StopPlayerAnimation(dbPlayer.Player);
                                destinationDbPlayer.SetWayToKH();
                                destinationDbPlayer.Freeze(true);
                                destinationDbPlayer.Player.TriggerEvent("noweaponsoninjury", true);
                                destinationDbPlayer.SendNewNotification($"Du wurdest transportbereit gemacht!");
                                dbPlayer.SendNewNotification($"Du hast den Patienten transportbereit gemacht!");
                            });
                            return;
                        }
                    }

                    // Normales Stabilisieren

                    uint Verbandskasten = 39;
                    //no first aid licence
                    if (dbPlayer.Lic_FirstAID[0] == 0)
                    {
                        dbPlayer.SendNewNotification("Dafür benötigst du eine Erste-Hilfe-Ausbildung!", title: "Erste Hilfe Schein");
                        return;
                    }

                    if (dbPlayer.Container.GetItemAmount(Verbandskasten) > 0)
                    {
                        Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                        {
                            if (destinationDbPlayer.Injury.Id != 17)
                            {
                                Chats.sendProgressBar(dbPlayer, 25000);
                                dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), Main.AnimationList["revive"].Split()[0], Main.AnimationList["revive"].Split()[1]);
                                dbPlayer.Player.TriggerEvent("freezePlayer", true);
                                await Task.Delay(25000);
                                if (dbPlayer.IsCuffed || dbPlayer.IsTied || dbPlayer.isInjured())
                                {
                                    dbPlayer.SendNewNotification("Stabilisierung fehlgeschlagen!");
                                    return;
                                }
                                dbPlayer.Player.TriggerEvent("freezePlayer", false);
                                NAPI.Player.StopPlayerAnimation(dbPlayer.Player);
                            }
                            dbPlayer.SendNewNotification($"{destinationDbPlayer.Injury.Name} stabilisiert!");
                            dbPlayer.Container.RemoveItem(Verbandskasten);

                            if (destinationDbPlayer.Injury.Id == InjuryModule.Instance.InjuryDeathScreenId)
                            {
                                destinationDbPlayer.SendNewNotification("Du kannst nichts mehr für diese Person tun...");
                                return;
                            }

                            destinationDbPlayer.Stabilize();
                        }));
                    }
                    else
                    {
                        dbPlayer.SendNewNotification($"Kein Verbandskoffer!");
                    }
                }
            }
        }

        [RemoteEventPermission(AllowedDeath = false, AllowedOnCuff = false, AllowedOnTied = false)]
        [RemoteEvent]
        public async void computerCheck(Client player, int type)
        {
            
                var dbPlayer = player.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid()) return;
                if (!dbPlayer.CanInteract()) return;
                if (dbPlayer.IsInAnimation()) return;

            // Kein Cop kein Reg Rang 9 und kein Laptop

                if (type == 1)
                {
                if (dbPlayer.Container.GetItemAmount(173) < 1)
                {
                    dbPlayer.SendNewNotification("Du hast keinen Computer dabei!.", PlayerNotification.NotificationType.ERROR, "", 5000);
                    return;
                }
                if (!player.IsInVehicle)
                    {
                        NAPI.Player.PlayPlayerAnimation(player, (int)(AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.AllowPlayerControl), "amb@code_human_in_bus_passenger_idles@female@tablet@idle_a", "idle_a");
                    }

                    player.TriggerEvent("openComputer");
                }
                else
                {
                    if (dbPlayer.RankId == 1 || dbPlayer.RankId == 2 || dbPlayer.RankId == 3 || dbPlayer.RankId == 4 || dbPlayer.RankId == 5 || dbPlayer.RankId == 6 || dbPlayer.RankId == 8 || dbPlayer.RankId == 11)
                    {
                        player.TriggerEvent("openIpad");
                    }
                }
            
        }

        [RemoteEvent]
        public  void closeComputer(Client player, uint type)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                DbPlayer iPlayer = player.GetPlayer();
                if (iPlayer == null || !iPlayer.IsValid()) return;
                if (!player.IsInVehicle)
                {
                    NAPI.Player.StopPlayerAnimation(player);
                }

                if (type == 1)
                {
                    player.TriggerEvent("closeComputer");
                }
                else
                {
                    player.TriggerEvent("closeIpad");
                }
            }));
        }

        [RemoteEventPermission]
        [RemoteEvent]
        public async void REQUEST_PEDS_PLAYER_SHOW_LIC(Client client, Client destinationPlayer)
        {
            
                var dbPlayer = client.GetPlayer();
                if (dbPlayer == null || !dbPlayer.CanAccessRemoteEvent()) return;
                var destinationDbPlayer = destinationPlayer.GetPlayer();
                if (!destinationDbPlayer.IsValid()) return;
                if (destinationDbPlayer.Id == dbPlayer.Id) return;
                if (destinationDbPlayer.Player.Position.DistanceTo(dbPlayer.Player.Position) > 12f) return;

                dbPlayer.ShowLicenses(destinationPlayer);
                dbPlayer.SendNewNotification(
                    "Sie haben Ihre Lizenzen gezeigt!");
            
        }

        [RemoteEventPermission]
        [RemoteEvent]
        public async void REQUEST_PEDS_PLAYER_TAKE_LIC(Client client, Client destinationPlayer)
        {
            
                var dbPlayer = client.GetPlayer();
                if (!dbPlayer.CanAccessRemoteEvent()) return;

                if (!dbPlayer.IsACop() || !dbPlayer.Duty) return;

                var destinationDbPlayer = destinationPlayer.GetPlayer();
                if (!destinationDbPlayer.IsValid()) return;
                if (destinationDbPlayer.Id == dbPlayer.Id) return;
                if (destinationDbPlayer.Player.Position.DistanceTo(dbPlayer.Player.Position) > 12f) return;

                if (!destinationDbPlayer.IsCuffed && !destinationDbPlayer.IsTied) return;

                destinationDbPlayer.ShowLicenses(client);
                dbPlayer.SendNewNotification("Sie haben sich die Lizenzen genommen!");
                destinationDbPlayer.SendNewNotification("Ein Beamter hat sich Ihre Lizenzen genommen!");
            
        }



        [RemoteEventPermission]
        [RemoteEvent]
        public async void Indicator(Client player, int indicator)
        {
            
                var dbPlayer = player.GetPlayer();
                if (!dbPlayer.CanAccessRemoteEvent()) return;
                if (!player.IsInVehicle) return;
                if (!player.Vehicle.HasSharedData("INDICATOR_" + indicator))
                {
                    player.Vehicle.SetSharedData("INDICATOR_" + indicator, true);
                }
                else
                {
                    player.Vehicle.ResetSharedData("INDICATOR_" + indicator);
                }
            
        }

        [RemoteEventPermission]
        [RemoteEvent]
        public void Siren(Client player)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                var dbPlayer = player.GetPlayer();
                if (!dbPlayer.CanAccessRemoteEvent()) return;
                if (!player.IsInVehicle) return;
                if (!player.Vehicle.HasSharedData("SIREN"))
                {
                    player.Vehicle.SetSharedData("SIREN", true);
                }
                else
                {
                    player.Vehicle.ResetSharedData("SIREN");
                }
            }));
        }

        [RemoteEvent]
        public async Task Pressed_E(Client player)
        {
            Console.WriteLine(player.Name + "Pressed_E");

            var dbPlayer = player.GetPlayer();
            if (!dbPlayer.IsValid()) return;

            if (!Configuration.Instance.EKeyActivated)
            {
                dbPlayer.SendNewNotification("Der E-Muskel ist für ein paar Minuten deaktiviert!");
                return;
            }

            await Main.TriggerPlayerPoint(dbPlayer);
        }
        
        [RemoteEvent]
        public void Pressed_H(Client player)
        {

            var dbPlayer = player.GetPlayer();
            if (!dbPlayer.IsValid()) return;

            if (!dbPlayer.CanInteract()) return;
            if (player.IsInVehicle) return;
            if (dbPlayer.HasData("handsup"))
            {
                dbPlayer.ResetData("handsup");
                dbPlayer.StopAnimation();
                return;
            }
            else
            {
                dbPlayer.SetData("handsup", 1);
                dbPlayer.PlayAnimation(49, "missfbi5ig_21",
                    "hand_up_scientist");
                return;
            }
        }

        [RemoteEvent]
        public void Pressed_M(Client player)
        {
            try
            {
                if (player == null) return;
                var dbPlayer = player.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid() || dbPlayer.Character == null || dbPlayer.Character.Clothes == null) return;
                
                if (dbPlayer.IsInAdminDuty() || dbPlayer.jailtime[0] > 0) return;

               MenuManager.Instance.Build(PlayerMenu.MobileClothMenu, dbPlayer).Show(dbPlayer);
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }

        [RemoteEvent]
        public void Pressed_T(Client player)
        {
            if (player == null) return;
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            ComponentManager.Get<ChatWindow>().Show()(dbPlayer);
        }
        
        [RemoteEvent]
        public void Pressed_L(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            Main.TriggerPlayer_L(dbPlayer);
        }

        [RemoteEvent]
        public void Pressed_K(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
        }


        [RemoteEvent]
        public void Pressed_J(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            Main.TriggerPlayer_J(dbPlayer);
        }

        [RemoteEvent]
        public async Task Pressed_KOMMA(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || dbPlayer.IsCuffed || dbPlayer.IsTied) return;
            if (!dbPlayer.CanInteract()) return;
            await new ItemsModuleEvents().useInventoryItem(player, 4);
        }

        [RemoteEvent]
        public async Task Pressed_PUNKT(Client player)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || dbPlayer.IsCuffed || dbPlayer.IsTied) return;
            if (!dbPlayer.CanInteract()) return;
            await new ItemsModuleEvents().useInventoryItem(player, 5);
        }

        [RemoteEvent]
        public void requestPlayerSyncData(Client player, Client requestedPlayer)
        {
            var dbPlayer = requestedPlayer.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid() || dbPlayer.Character == null) return;

            //Prop Sync
            Dictionary<int, uint> equipedProps = dbPlayer.Character.EquipedProps;
            var propsToSync = new Dictionary<int, List<int>>();

            foreach (var kvp in equipedProps.ToList())
            {
                var prop = PropModule.Instance[kvp.Value];
                if (prop == null) continue;

                var propValues = new List<int>
                {
                    prop.Variation,
                    prop.Texture
                };

                propsToSync.Add(kvp.Key, propValues);
            }

            //New clothes sync
            var clothesToSync = new Dictionary<int, List<int>>();

            try
            {
                clothesToSync = dbPlayer.HasData("clothes") ?
                ((Dictionary<int, List<int>>)dbPlayer.GetData("clothes")).ToDictionary(kvp => kvp.Key, kvp => kvp.Value) : clothesToSync;
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }

            player.TriggerEvent("responsePlayerSyncData", requestedPlayer,
                JsonConvert.SerializeObject(propsToSync),
                dbPlayer.HasData("alkTime"),
                AttachmentModule.Instance.SerializeAttachments(dbPlayer),
                JsonConvert.SerializeObject(clothesToSync));
        }
        
        [RemoteEventPermission(AllowedDeath = false, AllowedOnCuff = false, AllowedOnTied = false)]
        [RemoteEvent]
        public async void Keks(Client player, bool state)
        {
            
            DbPlayer iPlayer = player.GetPlayer();
            if (iPlayer == null) return;
            if (!iPlayer.CanAccessRemoteEvent()) return;

            if (player.IsReloading) return;
            if (iPlayer.IsInAnimation()) return;

            if (iPlayer.Container.GetItemAmount(174) < 1) {
                iPlayer.SendNewNotification("Du hast kein Handy!", PlayerNotification.NotificationType.ERROR, "", 5000);
                    return;
            }



            if (state && !player.IsInVehicle)
            {						
                NAPI.Player.PlayPlayerAnimation(iPlayer.Player, (int)(AnimationFlags.Loop | AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.AllowPlayerControl), "amb@world_human_stand_mobile@male@text@idle_a", "idle_a");
            }
            if (!state && !player.IsInVehicle)
            {
                NAPI.Player.StopPlayerAnimation(iPlayer.Player);
            }

                //Call remote trigger phone
            player.TriggerEvent("hatNudeln", state);
            
        }

        [RemoteEventPermission]
        [RemoteEvent]
        public async void changeVoiceRange(Client player)
        {
            
                var iPlayer = player.GetPlayer();
                if (iPlayer == null) return;

                // 1 = normal, 2 = whisper, 3 = schreien 4 (optional) = mega
                // 

                int voicetype = 1;
                if (iPlayer.HasData("voiceType"))
                {
                    voicetype = iPlayer.GetData("voiceType");
                }

                if (iPlayer.jailtime[0] > 0) return; // in jail ignore it...

                if (voicetype == 1)
                {
                    player.SetSharedData("voiceRange", (int)VoiceRange.shout);
                    iPlayer.SetData("voiceType", 2);
                    player.TriggerEvent("setVoiceType", 2);
                }
                else if (voicetype == 2)
                {
                    player.SetSharedData("voiceRange", (int)VoiceRange.whisper);
                    iPlayer.SetData("voiceType", 3);
                    player.TriggerEvent("setVoiceType", 3);
                }
                else if (voicetype == 3)
                {
                    if (iPlayer.CanUseMegaphone())
                    {
                        player.SetSharedData("voiceRange", (int)VoiceRange.megaphone);
                        iPlayer.SetData("voiceType", 4);
                        player.TriggerEvent("setVoiceType", 4);
                    }
                    else
                    {
                        player.SetSharedData("voiceRange", (int)VoiceRange.normal);
                        iPlayer.SetData("voiceType", 1);
                        player.TriggerEvent("setVoiceType", 1);
                    }
                }
                else if (voicetype == 4)
                {
                    player.SetSharedData("voiceRange", (int)VoiceRange.normal);
                    iPlayer.SetData("voiceType", 1);
                    player.TriggerEvent("setVoiceType", 1);
                }
            
        }
    }
}
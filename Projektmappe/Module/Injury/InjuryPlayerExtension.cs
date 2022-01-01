using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GTANetworkAPI;
using GVRP.Handler;
using GVRP.Module.AnimationMenu;
using GVRP.Module.Chat;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Items;
using GVRP.Module.Logging;
using GVRP.Module.NSA.Observation;
using GVRP.Module.Players;
using GVRP.Module.Players.Buffs;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Events;
using GVRP.Module.Players.Windows;
using GVRP.Module.Service;
using GVRP.Module.Telefon.App;
using GVRP.Module.Vehicles;
using GVRP.Module.Voice;
using static GVRP.Module.Players.Db.DbPlayer;

namespace GVRP.Module.Injury
{
    public static class InjuryPlayerExtension
    {
        private static uint FirstAidKitGangsterId = 676;
        private static uint FirstAidKitCops = 677;

        public static bool isInjured(this DbPlayer dbPlayer)
        {
            return dbPlayer.Injury.Id != 0;
        }

        public static bool isAlive(this DbPlayer dbPlayer)
        {
            return !isInjured(dbPlayer);
        }

        public static void revive(this DbPlayer dbPlayer)
        {
            if (dbPlayer.isInjured())
            {
                dbPlayer.Freeze(false, false, true);
                dbPlayer.Freezed = false;
                dbPlayer.Player.StopAnimation();
                dbPlayer.Player.SetSharedData("death", false);
                dbPlayer.Injury = InjuryTypeModule.Instance.Get(0); // Gets Alive Injury
                dbPlayer.deadtime[0] = 0;
                VoiceListHandler.RemoveFromDeath(dbPlayer);

                ServiceModule.Instance.RemoveInjuredPlayerService(dbPlayer);

                //dbPlayer.Player.TriggerEvent("stopScreenEffect", "DeathFailMPIn");
                dbPlayer.Player.TriggerEvent("disableAllPlayerActions", false);
                ComponentManager.Get<DeathWindow>().Close(dbPlayer.Player);

                NAPI.Player.StopPlayerAnimation(dbPlayer.Player);

                PlayerSpawn.InitPlayerSpawnData(dbPlayer.Player);
            }
            return;
        }

        public static void SetParamedicLicense(this DbPlayer dbPlayer)
        {
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            // Gebe lizenz
            MySQLHandler.ExecuteAsync($"UPDATE player SET mediclic = 1 WHERE id = '{dbPlayer.Id}'");
            MySQLHandler.ExecuteAsync($"UPDATE team SET medicslotsused = medicslotsused+1 WHERE id = '{dbPlayer.Team.Id}'");

            dbPlayer.Team.MedicSlotsUsed += 1;
            dbPlayer.ParamedicLicense = true;

            return;
        }


        public static void RemoveParamedicLicense(this DbPlayer dbPlayer)
        {
            if (dbPlayer == null || !dbPlayer.IsValid()) return;
            if (!dbPlayer.ParamedicLicense) return;

            // Gebe lizenz
            MySQLHandler.ExecuteAsync($"UPDATE player SET mediclic = 0 WHERE id = '{dbPlayer.Id}'");
            MySQLHandler.ExecuteAsync($"UPDATE team SET medicslotsused = medicslotsused-1 WHERE id = '{dbPlayer.Team.Id}'");

            dbPlayer.Team.MedicSlotsUsed -= 1;
            dbPlayer.ParamedicLicense = false;

            return;
        }

        public static void SetWayToKH(this DbPlayer dbPlayer)
        {
            if (dbPlayer.isInjured() && dbPlayer.Player.IsInVehicle)
            {
                InjuryType WayToKh = InjuryTypeModule.Instance.Get(InjuryModule.Instance.InjuryKrankentransport);
                dbPlayer.SetPlayerInjury(WayToKh);
            }
            return;
        }

        public static void SetPlayerInjury(this DbPlayer iPlayer, InjuryType injuryType)
        {
            iPlayer.Injury = injuryType;
            iPlayer.deadtime[0] = 0;
        }

        // Injury Time over? Set Deathscreen
        public static void SetDeathScreen(this DbPlayer dbPlayer)
        {
            dbPlayer.Weapons.Clear();
            dbPlayer.Player.TriggerEvent("emptyWeaponAmmo", 0);
            dbPlayer.Container.ClearInventory();

            dbPlayer.ResetBuffs();
            dbPlayer.TakeBlackMoney(dbPlayer.blackmoney[0]); // reset lul

            dbPlayer.SendNewNotification($"Du bist deinen {dbPlayer.Injury.Name} erlegen und befindest dich nun im Koma!");
            dbPlayer.Injury = InjuryTypeModule.Instance.Get(InjuryModule.Instance.InjuryDeathScreenId);
            dbPlayer.deadtime[0] = 0;
            dbPlayer.UHaftTime = 0;
            dbPlayer.Player.TriggerEvent("disableAllPlayerActions", true);
            LogHandler.LogKilled(dbPlayer.Player.Name, dbPlayer.GetData("killername"), dbPlayer.GetData("killerweapon"));
            ServiceModule.Instance.RemoveInjuredPlayerService(dbPlayer);
            ComponentManager.Get<DeathWindow>().Show()(dbPlayer);
        }

        // Spawn Player after DeathScreen
        public static void SetPlayerDied(this DbPlayer dbPlayer)
        {
            dbPlayer.Player.TriggerEvent("disableAllPlayerActions", false);
            ComponentManager.Get<DeathWindow>().Close(dbPlayer.Player);

            dbPlayer.Injury = InjuryTypeModule.Instance.Get(0);
            if (dbPlayer.jailtime[0] <= 0) dbPlayer.SetData("komaSpawn", true);
            PlayerSpawn.OnPlayerSpawn(dbPlayer.Player);
        }

        // Set Player to Stabilized Injury if exists
        public static void Stabilize(this DbPlayer dbPlayer)
        {
            if (dbPlayer.isInjured())
            {
                if (dbPlayer.Injury.StabilizedInjuryId != 0 && dbPlayer.Injury.Id != InjuryModule.Instance.InjuryGangwar)
                {
                    dbPlayer.SendNewNotification($"Sie wurden stabilisiert!");
                    dbPlayer.SetPlayerInjury(InjuryTypeModule.Instance.Get((uint)dbPlayer.Injury.StabilizedInjuryId));
                }
            }
        }

        public static void Medicate(this DbPlayer dbPlayer, DbPlayer medic)
        {
            if (dbPlayer == null || medic == null || !dbPlayer.IsValid() || !medic.IsValid()) return;
            if (dbPlayer.isInjured())
            {
                if (dbPlayer.Injury.Id == InjuryModule.Instance.InjuryDeathScreenId)
                {
                    medic.SendNewNotification("Diese Person liegt bereits im Koma!");
                    return;
                }

                if ((medic.IsAMedic() && medic.Duty) || (medic.ParamedicLicense && (!medic.IsAGangster() || dbPlayer.TeamId == medic.TeamId)))
                {
                    if (dbPlayer.Injury.ItemToStabilizeId != 0 || dbPlayer.Player.Dimension != 0)
                    {
                        // Wenn Spieler ins KH gebracht werden muss dann in einen Krankenwagen setzen
                        if (dbPlayer.Injury.NeedHospital)
                        {
                            if ((medic.Container.GetItemAmount(412) > 0 && medic.Container.GetItemAmount(dbPlayer.Injury.ItemToStabilizeId) > 0) ||
                                (medic.IsAGangster() && medic.Container.GetItemAmount(FirstAidKitGangsterId) > 0) ||
                                (!medic.IsAGangster() && medic.Container.GetItemAmount(FirstAidKitCops) > 0))
                            {
                                // Normal
                                if (medic.Container.GetItemAmount(412) > 0 && medic.Container.GetItemAmount(dbPlayer.Injury.ItemToStabilizeId) > 0)
                                {
                                    // Remove Item
                                    medic.Container.RemoveItem(dbPlayer.Injury.ItemToStabilizeId, 1);
                                }
                                // Bad Notfallmedics
                                else if (medic.IsAGangster() && medic.Container.GetItemAmount(FirstAidKitGangsterId) > 0)
                                {
                                    // Remove Item
                                    medic.Container.RemoveItem(FirstAidKitGangsterId, 1);
                                }
                                // Beamten Notfallmedics
                                else if (medic.Team.Id != (int)teams.TEAM_MEDIC && medic.Team.IsStaatsfraktion() && medic.Container.GetItemAmount(FirstAidKitCops) > 0)
                                {
                                    // Remove Item
                                    medic.Container.RemoveItem(FirstAidKitCops, 1);
                                }
                                // Meldung zum behandeln
                                else
                                {
                                    medic.SendNewNotification(
                                        $"Fuer die Behandlung von {dbPlayer.Injury.Name} benötigen Sie {ItemModelModule.Instance.Get(412).Name} und {ItemModelModule.Instance.Get(dbPlayer.Injury.ItemToStabilizeId).Name}.");
                                    return;
                                }

                                SxVehicle sxVehicle = VehicleHandler.Instance.GetClosestVehicleFromTeamFilter(dbPlayer.Player.Position, (int)medic.TeamId, 15.0f, 4);

                                if (sxVehicle == null || (medic.IsAGangster() && sxVehicle.Data.ClassificationId == 8)) // Remove Heli for Gang medics
                                {
                                    medic.SendNewNotification($"Kein Krankenwagen zum Transport in der naehe!");
                                    return;
                                }

                                Task.Run(async () =>
                                {
                                    medic.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl),
                                        Main.AnimationList["revive"].Split()[0], Main.AnimationList["revive"].Split()[1]);
                                    medic.Player.TriggerEvent("freezePlayer", true);
                                    if (medic.IsAGangster())
                                    {
                                        Chats.sendProgressBar(medic, 15000);
                                        await Task.Delay(15000);
                                    }
                                    else
                                    {
                                        Chats.sendProgressBar(medic, 9000);
                                        await Task.Delay(9000);
                                    }
                                    VehicleHandler.Instance.TrySetPlayerIntoVehicleOccupants(sxVehicle, dbPlayer);
                                    await Task.Delay(1000);
                                    medic.Player.TriggerEvent("freezePlayer", false);
                                    NAPI.Player.StopPlayerAnimation(medic.Player);
                                    dbPlayer.SetWayToKH();
                                    dbPlayer.Freeze(true);
                                    dbPlayer.Player.TriggerEvent("noweaponsoninjury", true);
                                    dbPlayer.SendNewNotification($"Du wurdest transportbereit gemacht!");
                                    medic.SendNewNotification($"Du hast den Patienten transportbereit gemacht!");
                                });
                                return;
                            }
                        }
                        else
                        {
                            if (medic.Container.GetItemAmount(412) > 0 && medic.Container.GetItemAmount(dbPlayer.Injury.ItemToStabilizeId) > 0)
                            {
                                // Remove Item
                                medic.Container.RemoveItem(dbPlayer.Injury.ItemToStabilizeId, 1);
                            }
                            else if (medic.IsAGangster() && medic.Container.GetItemAmount(FirstAidKitGangsterId) > 0)
                            {
                                // Remove Item
                                medic.Container.RemoveItem(FirstAidKitGangsterId, 1);
                            }
                            else if (!medic.IsAGangster() && medic.Container.GetItemAmount(FirstAidKitCops) > 0)
                            {
                                // Remove Item
                                medic.Container.RemoveItem(FirstAidKitCops, 1);
                            }
                            else
                            {
                                medic.SendNewNotification(
                                    $"Fuer die Behandlung von {dbPlayer.Injury.Name} benötigen Sie {ItemModelModule.Instance.Get(412).Name} und {ItemModelModule.Instance.Get(dbPlayer.Injury.ItemToStabilizeId).Name}.");
                                return;
                            }

                            Task.Run(async () =>
                            {
                                medic.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl),
                                    Main.AnimationList["revive"].Split()[0], Main.AnimationList["revive"].Split()[1]);
                                medic.Player.TriggerEvent("freezePlayer", true);
                                if (medic.IsAGangster())
                                {
                                    Chats.sendProgressBar(medic, 15000);
                                    await Task.Delay(15000);
                                }
                                else
                                {
                                    Chats.sendProgressBar(medic, 9000);
                                    await Task.Delay(9000);
                                }
                                medic.Player.TriggerEvent("freezePlayer", false);
                                NAPI.Player.StopPlayerAnimation(medic.Player);
                            });

                            //ToDo: Make player walk injured for some time
                            dbPlayer.revive();

                            dbPlayer.SendNewNotification($"Du wurdest vom Medic behandelt!");
                            medic.SendNewNotification($"Du hast den Patienten behandelt!");
                            return;
                        }
                    }
                }
                else
                {
                    medic.SendNewNotification($"{dbPlayer.Injury.Name} koennen sie nicht behandeln!");
                }
            }
        }

        public static void SetPlayerKomaSpawn(this DbPlayer dbPlayer)
        {
            int khcosts = 0;
            if (dbPlayer.Level < 10)
            {
                khcosts = dbPlayer.Level * 200;
            }
            else
            {
                khcosts = dbPlayer.Level * 800;
            }

            if (dbPlayer.IsACop() && dbPlayer.IsInDuty())
            {
                khcosts = khcosts / 4; // wegen Beamter im Dienst weil Steuern etc...
            }

            dbPlayer.TakeBankMoney(khcosts, "Krankenhauskosten", true);
            dbPlayer.SendNewNotification($"Sie wurden nach ihrem Koma aus dem Krankenhaus entlassen! Krankenhauskosten von ${khcosts} wurde von ihrem Konto abgebucht!");
            return;
        }



        public static void ApplyDeathEffects(this DbPlayer dbPlayer)
        {
            try
            {
                if (dbPlayer.isInjured())
                {
                    // Set Voice To Normal
                    dbPlayer.Player.SetSharedData("voiceRange", (int)VoiceRange.normal);
                    dbPlayer.SetData("voiceType", 1);
                    dbPlayer.Player.TriggerEvent("setVoiceType", 1);

                    // Disable Funk complete
                    VoiceModule.Instance.turnOffFunk(dbPlayer);

                    dbPlayer.Player.SetSharedData("death", true);

                    // Cancel Phonecall
                    dbPlayer.Player.TriggerEvent("hangupCall");
                    dbPlayer.Player.TriggerEvent("cancelPhoneCall");
                    dbPlayer.ResetData("current_caller");

                    if (dbPlayer.HasData("current_caller"))
                    {
                        NSAObservationModule.CancelPhoneHearing((int)dbPlayer.handy[0]);
                        var result = int.TryParse(dbPlayer.GetData("current_caller"), out int number);
                        if (result)
                        {
                            DbPlayer l_CalledPlayer = TelefonInputApp.GetPlayerByPhoneNumber(number);
                            if (l_CalledPlayer != null)
                            {
                                l_CalledPlayer.Player.TriggerEvent("hangupCall");
                                l_CalledPlayer.Player.TriggerEvent("cancelPhoneCall");
                                l_CalledPlayer.ResetData("current_caller");
                                NSAObservationModule.CancelPhoneHearing(number);
                            }
                        }
                    }

                    dbPlayer.Freeze(true, false, true);

                    NAPI.Player.StopPlayerAnimation(dbPlayer.Player);
                    dbPlayer.PlayAnimation(
                        (int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "combat@damage@rb_writhe", "rb_writhe_loop");

                    dbPlayer.Player.SetSharedData("death", true);
                }
            }
            catch (Exception e)
            {
                Logger.Crash(e);
            }
        }
    }
}

using GTANetworkAPI;
using System;
using System.Linq;
using GVRP.Module.Customization;
using GVRP.Module.Houses;
using GVRP.Module.Injury;
using GVRP.Module.Logging;
using GVRP.Module.Menu;
using GVRP.Module.Players.Db;
using GVRP.Module.Crime;
using GVRP.Module.Gangwar;
using System.Threading.Tasks;
using GVRP.Module.Einreiseamt;
using GVRP.Module.AnimationMenu;
using GVRP.Module.Swat;
using GVRP.Module.Events.Halloween;
using System.Threading;

namespace GVRP.Module.Players.Events
{
    public class PlayerSpawn : Script
    {
        public static void InitPlayerSpawnData(Client player)
        {
            try { 
            var iPlayer = player.GetPlayer();
            if (iPlayer == null || !iPlayer.IsValid())
            {
                iPlayer.Kick();
                return;
            }

            if (player == null)
            {
                player.Kick();
                return;
            }

                Task.Run(async () =>
                {
                    await Task.Delay(3000);
                    if (iPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(iPlayer.Player) || !iPlayer.Player.Exists) return;
                    if (iPlayer.isInjured())
                    {
                        iPlayer.Player.TriggerEvent("startScreenEffect", "DeathFailMPIn", 5000, true);
                    }
                    else
                    {
                        iPlayer.Player.TriggerEvent("stopScreenEffect", "DeathFailMPIn");
                    }
                });

                iPlayer.Player.TriggerEvent("updateInjured", iPlayer.isInjured());
                iPlayer.Player.SetSharedData("deathStatus", iPlayer.isInjured());

                player.Transparency = 255;

            player.TriggerEvent("setPlayerHealthRechargeMultiplier");

            // Workaround for freeze fails
            if (iPlayer.Freezed == false)
            {
                player.TriggerEvent("freezePlayer", false);
            }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }


        [ServerEvent(Event.PlayerSpawn)]
        public static void OnPlayerSpawn(Client player)
        {
            try {
                if (player == null) return;
                if (player.Name == "WeirdNewbie")
                {
                    player.Kick("Namen ändern!");
                    return;
                }
                var firstName = "";
                var lastName = "";


                var iPlayer = player.GetPlayer();


            if (player == null)
            {
                player.Kick();
                return;
            }


            if (iPlayer == null || !iPlayer.IsValid())
                {
                   
                    // anti blocking player (online etc..)

                    // da isn anderer Spieler?
                    if (NAPI.Pools.GetAllPlayers().ToList().Where(p => p != null && p.Name == player.Name && p.HasData("connectedAt")).Count() > 0)
                    {
                        Client olderPlayer = NAPI.Pools.GetAllPlayers().ToList().Where(p => p != null && p.Name == player.Name && p.HasData("connectedAt")).First();

                        if (olderPlayer != null && olderPlayer != player && (!olderPlayer.HasData("Connected") || olderPlayer.GetData("Connected") != true))
                        {
                            olderPlayer.SendNotification("Duplicate Entry 1");
                            olderPlayer.ResetData("Duplicate Entry!");
                            olderPlayer.Kick("Duplicate Entry!");
                            Main.Discord.SendMessage($"LOGIN FEHLER!", $"(PlayerSpawn.cs - 85) {olderPlayer.Name} - {DateTime.Now.ToString()}");
                            return;
                        }
                    }

                    player.SetData("connectedAt", DateTime.Now);

                    PlayerConnect.OnPlayerConnected(player);
                    return;
                }
                else
                {
                if (iPlayer == null || !iPlayer.IsValid())
                {
                    iPlayer.Kick();
                    return;
                }
                if (iPlayer.Firstspawn) 
                        Modules.Instance.OnPlayerLoggedIn(iPlayer);
                }
            if (iPlayer == null || !iPlayer.IsValid())
            {
                iPlayer.Kick();
                return;
            }
            // Interrupt wrong Spawn saving
            iPlayer.ResetData("lastPosition");
                Modules.Instance.OnPlayerSpawn(iPlayer);

                // init Spawn details
                var pos = new Vector3();
                float heading = 0.0f;
                player.TriggerEvent("guiReady");



                uint dimension = 0;
                DimensionType dimensionType = DimensionType.World;

                // Default Data required for Spawn
                bool FreezedNoAnim = false;
                iPlayer.SetData("Teleport", 2);

                if (iPlayer.NeuEingereist())
                {

                    iPlayer.jailtime[0] = 0;
                    iPlayer.ApplyCharacter();

                    pos = new GTANetworkAPI.Vector3(-1142.33, -2792.99, 27.71);
                    heading = 336.17f;
                    dimension = 188;

                    //iPlayer.Player.Freeze(true, true, true);
                    iPlayer.Player.TriggerEvent("freezePlayer", true);
                    iPlayer.Player.Position = pos;
                    iPlayer.Player.Dimension = dimension;

                    Task.Run(async () =>
                    {
                        await Task.Delay(20000);
                        if (iPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(iPlayer.Player) || !iPlayer.Player.Exists) return;
                        //iPlayer.Player.Freeze(false, true, true);
                        iPlayer.Player.TriggerEvent("freezePlayer", false);
                        iPlayer.EinreiseSpawn();
                    });
                }
                else if (iPlayer.isInjured())
                {
                    pos.X = iPlayer.dead_x[0];
                    pos.Y = iPlayer.dead_y[0];
                    pos.Z = iPlayer.dead_z[0];
                    FreezedNoAnim = true;

                    if (iPlayer.HasData("tmpDeathDimension"))
                    {
                        dimension = iPlayer.GetData("tmpDeathDimension");
                        iPlayer.ResetData("tmpDeathDimension");
                    }

                    if (GangwarTownModule.Instance.IsTeamInGangwar(iPlayer.Team) && iPlayer.DimensionType[0] == DimensionType.Gangwar)
                    {
                        dimension = GangwarModule.Instance.DefaultDimension;
                    }
                }
                else if (iPlayer.HasData("komaSpawn"))
                {
                    iPlayer.ResetData("komaSpawn");

                    Vector3 spawnPos = InjuryModule.Instance.GetClosestHospital(new Vector3(iPlayer.dead_x[0], iPlayer.dead_y[0], iPlayer.dead_z[0]));
                    iPlayer.SetPlayerKomaSpawn();

                    pos.X = spawnPos.X;
                    pos.Y = spawnPos.Y;
                    pos.Z = spawnPos.Z;
                    dimension = 0;
                }
                else if (iPlayer.HasData("SMGkilledPos") && iPlayer.HasData("SMGkilledDim"))
                {

                    pos = (Vector3)iPlayer.GetData("SMGkilledPos");
                    heading = 0.0f;
                    dimension = iPlayer.GetData("SMGkilledDim");

                    iPlayer.SetStunned(true);
                    FreezedNoAnim = true;
                }
                else if (iPlayer.jailtime[0] > 1 && !iPlayer.Firstspawn)
                {
                    //Jail Spawn
                    if (iPlayer.jailtime[0] > 1)
                    {
                        pos.X = 1691.28f;
                        pos.Y = 2565.91f;
                        pos.Z = 45.5648f;
                        heading = 177.876f;
                    }
                }
                else
                {
                    if (iPlayer.spawnchange[0] == 1 && (iPlayer.ownHouse[0] > 0 || iPlayer.IsTenant())) //Haus
                    {
                        House iHouse;
                        if ((iHouse = HouseModule.Instance.Get(iPlayer.ownHouse[0])) != null)
                        {
                            pos = iHouse.Position;
                            heading = iHouse.Heading;
                        }
                        else if ((iHouse = HouseModule.Instance.Get(iPlayer.GetTenant().HouseId)) != null)
                        {
                            pos = iHouse.Position;
                            heading = iHouse.Heading;
                        }
                    }
                    else
                    {
                        if (iPlayer.Team.TeamSpawns.TryGetValue(iPlayer.fspawn[0], out var spawn))
                        {
                            pos = spawn.Position;
                            heading = spawn.Heading;
                        }
                        else
                        {
                            spawn = iPlayer.Team.TeamSpawns.FirstOrDefault().Value;
                            if (spawn != null)
                            {
                                pos = spawn.Position;
                                heading = spawn.Heading;
                            }
                        }
                    }
                }

                // Setting Pos
                if (iPlayer.Firstspawn)
                {
                    if (iPlayer.pos_x[0] != 0f)
                    {
                        iPlayer.spawnProtection = DateTime.Now;

                        pos = new GTANetworkAPI.Vector3(iPlayer.pos_x[0], iPlayer.pos_y[0], iPlayer.pos_z[0] + 0.1f);
                        heading = iPlayer.pos_heading[0];
                    }

                    Task.Run(async () =>
                    {
                        await Task.Delay(9000);
                        if (iPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(iPlayer.Player) || !iPlayer.Player.Exists) return;
                        Modules.Instance.OnPlayerFirstSpawnAfterSync(iPlayer);
                    });
                }

                if (iPlayer.Firstspawn)
                {
                    Main.OnPlayerFirstSpawn(player);

                    // Fallback ...
                    if(iPlayer.DimensionType[0] == DimensionType.Gangwar)
                    {
                        iPlayer.Dimension[0] = 0;
                        iPlayer.DimensionType[0] = DimensionType.World;
                    }

                    // Load player Dimension from DB
                    dimension = iPlayer.Dimension[0];
                    dimensionType = iPlayer.DimensionType[0];

                    DialogMigrator.CloseUserDialog(player, Dialogs.menu_info);

                    // Connect to TS
              //      Console.Write(iPlayer.GetName());
                    Teamspeak.Connect(player, iPlayer.GetName());

                    var crumbs = player.Name.Split('_');
                    if (crumbs.Length > 1)
                    {
                        firstName = crumbs[0].ToString();
                        lastName = crumbs[1].ToString();
                        // Support multiple lastNames
                        for (int i = 2; i < crumbs.Length; i++)
                        {
                            lastName += "_" + crumbs[i];
                        }

                        iPlayer.Player.TriggerEvent("onPlayerLoaded", firstName, lastName, iPlayer.Id, iPlayer.rp[0],
                            iPlayer.ActiveBusiness?.Id ?? 0, iPlayer.grade[0], iPlayer.money[0], 0,
                            iPlayer.ownHouse[0], iPlayer.TeamId, iPlayer.TeamRank, iPlayer.Level, iPlayer.isInjured(), iPlayer.IsInDuty(),
                            iPlayer.IsTied, iPlayer.IsCuffed, iPlayer.VoiceHash, iPlayer.funkStatus, iPlayer.handy[0], iPlayer.job[0], 
                            0, iPlayer.GetJsonAnimationsShortcuts(), iPlayer.RankId >= (uint)adminlevel.Supporter ? true : false, 
                            Configurations.Configuration.Instance.WeaponDamageMultipier, Configurations.Configuration.Instance.MeeleDamageMultiplier, 
                            Configurations.Configuration.Instance.PlayerSync, Configurations.Configuration.Instance.VehicleSync, iPlayer.blackmoney[0]);
                    }
                    else
                    {
                        player.Kick();
                    }

                    // Cuff & Tie
                    if (iPlayer.IsCuffed)
                    {
                        iPlayer.SetCuffed(true);
                        FreezedNoAnim = true;
                    }

                    if (iPlayer.IsTied)
                    {
                        iPlayer.SetTied(true);
                        FreezedNoAnim = true;
                    }
                }
                else
                {
                    iPlayer.SetHealth(100);
                }

                if (iPlayer.jailtime[0] > 0)
                {
                    iPlayer.ApplyCharacter();
                }

                InitPlayerSpawnData(player);

                iPlayer.LoadPlayerWeapons();
            if (iPlayer == null || !iPlayer.IsValid())
            {
                iPlayer.Kick();
                return;
            }
                if (iPlayer.NeuEingereist())
                {
                    if (iPlayer.isInjured()) iPlayer.revive();
                    iPlayer.Player.TriggerEvent("showhud");

                    iPlayer.jailtime[0] = 0;
                    iPlayer.ApplyCharacter();

                    // Start Customization
                    if (iPlayer.HasData("firstCharacter"))
                        iPlayer.StartCustomization();
                }
                else
                {
                    iPlayer.Player.Position = pos;
                    iPlayer.Player.SetRotation(heading);
                    iPlayer.Player.Dimension = dimension;

                    if (!FreezedNoAnim)
                    {
                        // uncuff....
                        iPlayer.Player.StopAnimation();
                        iPlayer.SetTied(false);
                        iPlayer.SetMedicCuffed(false);
                        iPlayer.SetCuffed(false);
                        Task.Run(async () =>
                        {
                            player.TriggerEvent("freezePlayer", true);
                            player.SetPosition(pos);
                            player.SetRotation(heading);
                            await Task.Delay(1000);
                            if (player == null || !NAPI.Pools.GetAllPlayers().Contains(player) || !player.Exists) return;
                            player.SetPosition(pos);
                            player.SetRotation(heading);
                            await Task.Delay(1500);
                            if (player == null || !NAPI.Pools.GetAllPlayers().Contains(player) || !player.Exists) return;
                            player.TriggerEvent("freezePlayer", false);
                            NAPI.ClientEvent.TriggerClientEvent(player, "showhud");
                        });

                    }
                    if (iPlayer == null || !iPlayer.IsValid())
                    {
                        iPlayer.Kick();
                        return;
                    }
                    if (iPlayer.Firstspawn)
                    {
                        iPlayer.Firstspawn = false;

                        Task.Run(async () =>
                        {
                            iPlayer.CanInteract(false);
                            iPlayer.Player.TriggerEvent("moveSkyCamera", iPlayer.Player, "up", 1, false);
                            await Task.Delay(5000);
                            iPlayer.Player.TriggerEvent("moveSkyCamera", iPlayer.Player, "down", 1, false);
                            iPlayer.CanInteract(true);

                        });
                        iPlayer.ApplyCharacter();
                        iPlayer.ApplyPlayerHealth();
                        Task.Run(async () =>
                        {
                            await Task.Delay(25000);
                            if (iPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(iPlayer.Player) || !iPlayer.Player.Exists) return;
                            iPlayer.SetData("canlool", true);

                        });

                    }
                    if (iPlayer.isInjured())
                    {
                        iPlayer.ApplyDeathEffects();
                    }
                    else
                    {
                        iPlayer.Player.TriggerEvent("stopScreenEffect", "DeathFailMPIn");
                    }

                    if (iPlayer.HasData("SMGkilledPos") && iPlayer.HasData("SMGkilledDim"))
                    {

                        Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                        {

                            await Task.Delay(1500);
                            if (iPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(iPlayer.Player) || !iPlayer.Player.Exists) return;
                            iPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "misstrevor3_beatup", "guard_beatup_kickidle_dockworker");

                            await Task.Delay(30000);
                            if (iPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(iPlayer.Player) || !iPlayer.Player.Exists) return;
                            iPlayer.SetStunned(false);
                            iPlayer.ResetData("SMGkilledPos");
                        }));
                    }
                }
              
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }

}

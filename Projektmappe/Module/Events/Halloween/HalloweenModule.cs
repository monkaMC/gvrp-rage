using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GVRP.Handler;
using GVRP.Module.Chat;
using GVRP.Module.Commands;
using GVRP.Module.Customization;
using GVRP.Module.Doors;
using GVRP.Module.Farming;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.JumpPoints;
using static GVRP.Module.Chat.Chats;

namespace GVRP.Module.Events.Halloween
{
    //public class HalloweenModule : Module<HalloweenModule>
    //{
    //    public static bool isActive = false;

    //    public static List<Vector3> spawnPositions = new List<Vector3>();

    //    public List<DbPlayer> activeZombies = new List<DbPlayer>();

    //    public override bool Load(bool reload = false)
    //    {
    //        spawnPositions = new List<Vector3>();
    //        spawnPositions.Add(new Vector3(-1733.95, - 233.64, 54.9494));
    //        spawnPositions.Add(new Vector3(-287.057, 2837.26, 55.137));
    //        spawnPositions.Add(new Vector3(-303.213, 6152.16, 32.2342));

    //        isActive = false;
    //        activeZombies = new List<DbPlayer>();
    //        return base.Load(reload);
    //    }

    //    public Vector3 GetClosestSpawn(Vector3 Position)
    //    {
    //        Vector3 returnPos = spawnPositions.First();
    //        foreach (Vector3 pos in spawnPositions)
    //        {
    //            if (Position.DistanceTo(pos) < Position.DistanceTo(returnPos))
    //            {
    //                returnPos = pos;
    //            }
    //        }

    //        return returnPos;
    //    }

    //    public override void OnPlayerDisconnected(DbPlayer dbPlayer, string reason)
    //    {
    //        if(HalloweenModule.isActive)
    //        {
    //            if (HalloweenModule.Instance.activeZombies.Contains(dbPlayer)) HalloweenModule.Instance.activeZombies.Remove(dbPlayer);
    //        }
    //    }

    //    public override void OnPlayerSpawn(DbPlayer dbPlayer)
    //    {
    //        if(HalloweenModule.isActive)
    //        {
    //            if(dbPlayer.IsZombie())
    //            {
    //                if(dbPlayer.HasData("zombie"))
    //                {
    //                    if(dbPlayer.GetData("zombie") == 1)
    //                    {
    //                        dbPlayer.Player?.TriggerEvent("updatesuperjump", true);
    //                    }
    //                    else
    //                    {
    //                        dbPlayer.Player?.TriggerEvent("updaterunspeed", true);
    //                    }
    //                }
    //            }
    //        }
    //    }

    //    public override void OnFiveSecUpdate()
    //    {
    //        foreach(DbPlayer iPlayer in activeZombies.ToList())
    //        {
    //            if (iPlayer == null || !iPlayer.IsValid()) continue;
                
    //            DbPlayer rangedPlayer = Players.Players.Instance.GetClosestPlayerForPlayerForHalloweenEvent(iPlayer);

    //            rangedPlayer.InfestPlayer();
    //        }

    //        base.OnFiveSecUpdate();
    //    }

    //    [CommandPermission(PlayerRankPermission = true)]
    //    [Command]
    //    public void Commandstarthalloween(Client player)
    //    {
    //        var iPlayer = player.GetPlayer();
    //        if (iPlayer == null || !iPlayer.IsValid() || (iPlayer.Rank.Id != 6 && iPlayer.Rank.Id != 5)) return;
                        
    //        // Set Active
    //        if(!HalloweenModule.isActive)
    //        {
    //            HalloweenModule.isActive = true;

    //            // Alle türen auf
    //            foreach(Door door in DoorModule.Instance.GetAll().Values)
    //            {
    //                door.SetLocked(false);
    //            }

    //            // Alle Jumppoints auf
    //            foreach (JumpPoint jp in JumpPointModule.Instance.jumpPoints.Values)
    //            {
    //                jp.Locked = false;
    //            }

    //            // alle fahrzeuge ausschalten
    //            foreach(SxVehicle sxVeh in VehicleHandler.Instance.GetAllVehicles())
    //            {
    //                sxVeh.SyncExtension.SetEngineStatus(false);
    //            }

    //            foreach(DbPlayer dbPlayer in FarmingModule.FarmingList.ToList())
    //            {
    //                dbPlayer.Player.TriggerEvent("freezePlayer", false);
    //                NAPI.Player.StopPlayerAnimation(dbPlayer.Player);
    //                dbPlayer.ResetData("pressedEOnFarm");
    //                if (FarmingModule.FarmingList.Contains(dbPlayer)) FarmingModule.FarmingList.Remove(dbPlayer);
    //            }

    //            foreach(DbPlayer dbPlayer in Players.Players.Instance.GetValidPlayers())
    //            {
    //                dbPlayer.SendNewNotification($"1337Sexuakbar$halloween", duration: 104000);
    //            }

    //            Chats.SendGlobalMessage($"ACHTUNG, durch ein Virus kommt es zu tollwutähnlichen Symptomen! Alle Waffen sind zum Schutz vor infiszierten Personen erlaubt! Gott beschütze Sie!", COLOR.LIGHTBLUE, ICON.GOV);
    //        }
    //        else
    //        {
    //            HalloweenModule.isActive = false;

    //            foreach(DbPlayer dbPlayer in HalloweenModule.Instance.activeZombies)
    //            {
    //                if(dbPlayer != null && dbPlayer.IsValid())
    //                {
    //                    dbPlayer.ApplyCharacter();
    //                    if(dbPlayer.HasData("zombie"))
    //                    {
    //                        iPlayer.Player?.TriggerEvent("updatesuperjump", false);
    //                        iPlayer.Player?.TriggerEvent("updaterunspeed", false);
    //                        dbPlayer.ResetData("zombie");
    //                    }
    //                }
    //            }
    //            HalloweenModule.Instance.activeZombies.Clear();
    //        }
    //        return;
    //    }
        
    //    [CommandPermission(PlayerRankPermission = true)]
    //    [Command]
    //    public void Commandsetzombie(Client player, string returnstring)
    //    {
    //        var iPlayer = player.GetPlayer();
    //        if (iPlayer == null || !iPlayer.IsValid() || (iPlayer.Rank.Id != 6 && iPlayer.Rank.Id != 5 && iPlayer.Rank.Id != 8)) return;

    //        if (returnstring.Length < 2) return;

    //        DbPlayer target = Players.Players.Instance.FindPlayer(returnstring);
    //        if (target != null && target.IsValid())
    //        {
    //            target.InfestPlayer(true);
    //            iPlayer.SendNewNotification($"Du hast {target.Player.Name} zum Zombie gemacht!");
    //        }

    //        return;
    //    }

    //    public override void OnPlayerWeaponSwitch(DbPlayer dbPlayer, WeaponHash oldgun, WeaponHash newgun)
    //    {
    //        if (HalloweenModule.isActive)
    //        {
    //            if (dbPlayer.IsZombie())
    //            {
    //                NAPI.Player.SetPlayerCurrentWeapon(dbPlayer.Player, WeaponHash.Unarmed);
    //            }
    //        }
    //    }
    //}

    //public static class HalloweenPlayerExtension
    //{
    //    public static void InfestPlayer(this DbPlayer iPlayer, bool superjump = false)
    //    {
    //        if (!HalloweenModule.isActive || iPlayer.IsZombie()) return;

    //        HalloweenModule.Instance.activeZombies.Add(iPlayer);

    //        iPlayer.Player.SetSkin(GTANetworkAPI.PedHash.Zombie01);
    //        iPlayer.SendNewNotification("Du wurdest infisziert!");
    //        NAPI.Player.SetPlayerCurrentWeapon(iPlayer.Player, WeaponHash.Unarmed);

    //        if(superjump)
    //        {
    //            iPlayer.SetData("zombie", 1);
    //            iPlayer.Player?.TriggerEvent("updatesuperjump", true);
    //            return;
    //        }

    //        Random random = new Random();

    //        if(random.Next(1, 100) >= 90)
    //        {
    //            iPlayer.SetData("zombie", 1);
    //            iPlayer.Player?.TriggerEvent("updatesuperjump", true);
    //        }
    //        else
    //        {
    //            iPlayer.SetData("zombie", 2);
    //            iPlayer.Player?.TriggerEvent("updaterunspeed", true);
    //        }

    //    }

    //    public static bool IsZombie(this DbPlayer iPlayer)
    //    {
    //        return HalloweenModule.isActive && HalloweenModule.Instance.activeZombies.Contains(iPlayer);
    //    }
    //}
}

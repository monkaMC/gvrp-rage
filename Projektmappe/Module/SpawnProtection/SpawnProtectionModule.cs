using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using GVRP.Handler;
using GVRP.Module.Assets.Hair;
using GVRP.Module.Assets.HairColor;
using GVRP.Module.Assets.Tattoo;
using GVRP.Module.Barber.Windows;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Customization;
using GVRP.Module.GTAN;
using GVRP.Module.Logging;
using GVRP.Module.Menu;
using GVRP.Module.Players;

using GVRP.Module.Players.Db;
using GVRP.Module.Tattoo.Windows;
using GVRP.Module.Vehicles;

namespace GVRP.Module.SpawnProtection
{
    public sealed class SpawnProtectionModule : Module<SpawnProtectionModule>
    {
        public override bool Load(bool reload = false)
        {
            return true;
        }

        public override void OnPlayerFirstSpawn(DbPlayer dbPlayer)
        {
            // Set SpawnProtection
            dbPlayer.SetData("spawnProtectionSet", DateTime.Now);
            dbPlayer.Player.TriggerEvent("setSpawnProtection", true);
        }

        public override void OnTenSecUpdate()
        {
            foreach(DbPlayer dbPlayer in Players.Players.Instance.GetValidPlayers())
            {
                if (dbPlayer == null || !dbPlayer.IsValid()) return;

                if (dbPlayer.HasData("spawnProtectionSet"))
                {
                    DateTime spawnProtectionTime = dbPlayer.GetData("spawnProtectionSet");
                    if(spawnProtectionTime.AddSeconds(20) <= DateTime.Now)
                    {
                        dbPlayer.ResetData("spawnProtectionSet");
                        dbPlayer.Player.TriggerEvent("setSpawnProtection", false);
                    }
                }
            }
        }
    }
}
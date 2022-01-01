using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.Items;
using GVRP.Module.Node;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Players.Drunk
{
    public sealed class DrunkModule : Module<DrunkModule>
    {
        public void SetPlayerDrunk(DbPlayer dbPlayer, bool state)
        {
            NAPI.Pools.GetAllPlayers().ForEach((player) =>
            {
                if (player.Position.DistanceTo(dbPlayer.Player.Position) < 280)
                {
               //     player.TriggerEvent("setPlayerDrunk", dbPlayer.Player, state);
                }
            });

            if (state) dbPlayer.SetData("alkTime", DateTime.Now);
        }

        public void IncreasePlayerAlkLevel(DbPlayer dbPlayer, int level)
        {
            if (dbPlayer.HasData("alkLevel"))
            {
                level += (int)dbPlayer.GetData("alkLevel");
            }

            dbPlayer.SetData("alkLevel", level);

            if (level > 39)
            {
                SetPlayerDrunk(dbPlayer, true);
            }
        }

        public override void OnPlayerMinuteUpdate(DbPlayer dbPlayer)
        {
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            if (dbPlayer.HasData("alkTime"))
            {
                var oldTime = (DateTime)dbPlayer.GetData("alkTime");

                if (oldTime.AddMinutes(5) < DateTime.Now) //5 Minuten
                {
                    dbPlayer.ResetData("alkLevel");
                    dbPlayer.ResetData("alkTime");
                    SetPlayerDrunk(dbPlayer, false);
                }
                if(dbPlayer.HasData("alkLevel") && dbPlayer.GetData("alkLevel") >= 40)
                {
                    dbPlayer.Player.TriggerEvent("startScreenEffect", "DefaultFlash", 5000, true);
                }
                else
                {
                    dbPlayer.Player.TriggerEvent("stopScreenEffect", "DefaultFlash");
                }
            }
        }
    }
}

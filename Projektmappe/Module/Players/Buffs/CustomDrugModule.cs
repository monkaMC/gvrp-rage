using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Players.Buffs
{
    public class CustomDrugModule : Module<CustomDrugModule>
    {
        public string NewDrugEffect = "DrugsMichaelAliensFight";

        public void SetTrip(DbPlayer dbPlayer, string skin, string sound)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
            {
                Random rnd = new Random();
                dbPlayer.SendNewNotification($"1337Sexuakbar$" + sound, duration: 90000);

                await Task.Delay(15000);

                dbPlayer.Player.TriggerEvent("setBlackout", true);

                dbPlayer.Player.TriggerEvent("spawndrugped", dbPlayer.Player.Position.X + 10, dbPlayer.Player.Position.Y + 20, dbPlayer.Player.Position.Z, rnd.Next(1, 360), skin);
                await Task.Delay(6000);
                dbPlayer.Player.TriggerEvent("destroydrugped");
                
                dbPlayer.Player.TriggerEvent("spawndrugped", dbPlayer.Player.Position.X + 20, dbPlayer.Player.Position.Y - 20, dbPlayer.Player.Position.Z, rnd.Next(1, 360), skin);
                await Task.Delay(6000);
                dbPlayer.Player.TriggerEvent("destroydrugped");

                dbPlayer.Player.TriggerEvent("spawndrugped", dbPlayer.Player.Position.X - 10, dbPlayer.Player.Position.Y + 10, dbPlayer.Player.Position.Z, rnd.Next(1, 360), skin);
                await Task.Delay(3000);
                dbPlayer.Player.TriggerEvent("destroydrugped");
                await Task.Delay(2000);

                dbPlayer.Player.TriggerEvent("spawndrugped", dbPlayer.Player.Position.X + 5, dbPlayer.Player.Position.Y + 10, dbPlayer.Player.Position.Z, rnd.Next(1, 360), skin);
                await Task.Delay(6000);
                dbPlayer.Player.TriggerEvent("destroydrugped");
                
                dbPlayer.Player.TriggerEvent("spawndrugped", dbPlayer.Player.Position.X + 10, dbPlayer.Player.Position.Y - 10, dbPlayer.Player.Position.Z, rnd.Next(1, 360), skin);
                await Task.Delay(6000);
                dbPlayer.Player.TriggerEvent("destroydrugped");

                dbPlayer.Player.TriggerEvent("spawndrugped", dbPlayer.Player.Position.X - 5, dbPlayer.Player.Position.Y + 5, dbPlayer.Player.Position.Z, rnd.Next(1, 360), skin);
                await Task.Delay(3000);
                dbPlayer.Player.TriggerEvent("destroydrugped");
                await Task.Delay(2000);

                dbPlayer.Player.TriggerEvent("spawndrugped", dbPlayer.Player.Position.X + 2, dbPlayer.Player.Position.Y + 3, dbPlayer.Player.Position.Z, rnd.Next(1, 360), skin);
                await Task.Delay(6000);
                dbPlayer.Player.TriggerEvent("destroydrugped");
                
                dbPlayer.Player.TriggerEvent("spawndrugped", dbPlayer.Player.Position.X + 3, dbPlayer.Player.Position.Y - 3, dbPlayer.Player.Position.Z, rnd.Next(1, 360), skin);
                await Task.Delay(6000);
                dbPlayer.Player.TriggerEvent("destroydrugped");

                dbPlayer.Player.TriggerEvent("spawndrugped", dbPlayer.Player.Position.X - 2, dbPlayer.Player.Position.Y + 2, dbPlayer.Player.Position.Z, rnd.Next(1, 360), skin);
                await Task.Delay(6000);
                dbPlayer.Player.TriggerEvent("destroydrugped");

                dbPlayer.Player.TriggerEvent("spawndrugped", dbPlayer.Player.Position.X - 1, dbPlayer.Player.Position.Y + 1, dbPlayer.Player.Position.Z, rnd.Next(1, 360), skin);
                await Task.Delay(6000);
                dbPlayer.Player.TriggerEvent("destroydrugped");

                dbPlayer.Player.TriggerEvent("setBlackout", false);
            }));
        }
        
        public void SetCustomDrugEffect(DbPlayer dbPlayer)
        {
            dbPlayer.Player.TriggerEvent("startScreenEffect", NewDrugEffect, 60000, true);

            Random rnd = new Random();
            if(rnd.Next(1, 100) <= 5)
            {
                if (dbPlayer.Buffs.LastDrugId == 1007 || dbPlayer.Buffs.LastDrugId == 1006 || dbPlayer.Buffs.LastDrugId == 1005) SetTrip(dbPlayer, "s_m_y_clown_01", "clown");
                else SetTrip(dbPlayer, "u_m_y_staggrm_01", "gay");
            }
        }
        
        public void RemoveEffect(DbPlayer dbPlayer)
        {
            dbPlayer.Player.TriggerEvent("stopScreenEffect", NewDrugEffect);
        }
        
    }
}

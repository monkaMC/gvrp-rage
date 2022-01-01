using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using GVRP.Module.Crime;
using GVRP.Module.Customization;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Jails
{
    public sealed class JailCellModule : SqlModule<JailCellModule, JailCell, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `jail_cells`;";
        }
    }

    public sealed class JailSpawnModule : SqlModule<JailSpawnModule, JailSpawn, uint>
    {

        protected override string GetQuery()
        {
            return "SELECT * FROM `jail_spawns`;";
        }
    }

    public class JailModule : Module<JailModule>
    {
        public static Vector3 PrisonZone = new Vector3(1681, 2604, 44);
        public static float Range = 200.0f;

        public static Vector3 PrisonSpawn = new Vector3(1836.71, 2587.8, 45.891);
        
        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if (dbPlayer.Player.IsInVehicle) return false;

            if (colShape == null || !colShape.HasData("jailGroup")) return false;

            if (colShapeState == ColShapeState.Enter)
            {

                var wanteds = dbPlayer.wanteds[0];
                if (dbPlayer.TempWanteds > 0 && dbPlayer.wanteds[0] < 30) wanteds = 30;

                int jailtime = CrimeModule.Instance.CalcJailTime(dbPlayer.Crimes);
                int jailcosts = CrimeModule.Instance.CalcJailCosts(dbPlayer.Crimes);
                
                // Checke auf Jailtime
                if (jailtime > 0 && jailtime <= 29 && colShape.GetData("jailGroup") != 5)
                {
                    dbPlayer.jailtime[0] = jailtime;
                    dbPlayer.ArrestPlayer(null, true, false);
                    dbPlayer.ApplyCharacter();
                    dbPlayer.SetData("inJailGroup", colShape.GetData("jailGroup"));
                } // group 5 == sg
                else if(colShape.GetData("jailGroup") == 5 && jailtime >= 30)
                {
                    dbPlayer.jailtime[0] = jailtime;
                    dbPlayer.ArrestPlayer(null, true, false);
                    dbPlayer.ApplyCharacter();
                    dbPlayer.SetData("inJailGroup", colShape.GetData("jailGroup"));
                }
                
            }
            else
            {
                dbPlayer.ResetData("inJailGroup");
            }

            return false;
        }


    }
}

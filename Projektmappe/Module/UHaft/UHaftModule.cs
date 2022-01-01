using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Crime;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Windows;

namespace GVRP.Module.UHaft
{
    public sealed class UHaftmodule : Module<UHaftmodule>
    {
        public static uint UhaftFluchtCrimeId = 99;
        public static Vector3 JailPdPosition = new Vector3(462.945, -997, 24);
        public static Vector3 UhaftComputerPosition = new Vector3(461.548, -989.305, 24.9149);

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (!dbPlayer.IsACop() || !dbPlayer.IsInDuty() || dbPlayer.Player.Position.DistanceTo(UhaftComputerPosition) > 2.0f || key != Key.E) return false;
            ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Untersuchungshaft", Callback = "SetUhaftPlayer", Message = "Geben Sie einen Namen ein" });
            return true;
        }
        
        public override void OnPlayerMinuteUpdate(DbPlayer iPlayer)
        {

            if (iPlayer == null || !iPlayer.IsValid())
            {
                return;
            }


            // Wenn Spieler in UHaft
            if (iPlayer.UHaftTime > 0)
            {
                // Nicht an den Zellen
                if(iPlayer.Player.Position.DistanceTo(JailPdPosition) > 30.0f)
                {
                    // Cop in Range
                    if(Players.Players.Instance.GetValidPlayers().Where(p => p != iPlayer && p.IsInDuty() && p.IsACop() && p.Player.Position.DistanceTo(iPlayer.Player.Position) < 20.0f).Count() <= 0)
                    {
                        // Flucht
                        string wantedstring = $"Untersuchungshaft Flucht - { DateTime.Now.Hour}:{ DateTime.Now.Minute} { DateTime.Now.Day}/{ DateTime.Now.Month}/{ DateTime.Now.Year}";
                        iPlayer.AddCrime("Leitstelle", CrimeReasonModule.Instance.Get((uint)UhaftFluchtCrimeId), wantedstring);

                        iPlayer.UHaftTime = 0;
                    }
                }

                if (iPlayer.UHaftTime > 0) iPlayer.UHaftTime++;
            }
        }
    }
}

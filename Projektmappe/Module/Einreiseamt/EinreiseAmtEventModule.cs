using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Windows;

namespace GVRP.Module.Einreiseamt
{
    public class EinreiseAmtEventModule : Script
    {
        [RemoteEvent]
        public void EinreiseAmtPlayer(Client player, string returnstring)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            var findPlayer = Players.Players.Instance.FindPlayer(returnstring);
            if (findPlayer == null || !findPlayer.IsValid() || !findPlayer.NeuEingereist())
            {
                dbPlayer.SendNewNotification("Bürger nicht gefunden!");
                return;
            }

            // Is In Range
            if (findPlayer.Player.Position.DistanceTo(EinreiseamtModule.PositionPC1) > 4.0f && findPlayer.Player.Position.DistanceTo(EinreiseamtModule.PositionPC6) > 4.0f && findPlayer.Player.Position.DistanceTo(EinreiseamtModule.PositionPC2) > 4.0f && findPlayer.Player.Position.DistanceTo(EinreiseamtModule.PositionPC5) > 4.0f  && findPlayer.Player.Position.DistanceTo(EinreiseamtModule.PositionPC3) > 4.0f && findPlayer.Player.Position.DistanceTo(EinreiseamtModule.PositionPC4) > 4.0f && findPlayer.Player.Position.DistanceTo(EinreiseamtModule.PositionPC7) > 4.0f && findPlayer.Player.Position.DistanceTo(EinreiseamtModule.PositionPC8) > 4.0f)
            {
                dbPlayer.SendNewNotification("Bürger ist nicht in der Naehe!");
                return;
            }

            dbPlayer.SetData("einreiseamtp", findPlayer.GetName());

            // Open Menu
            MenuManager.Instance.Build(PlayerMenu.EinreiseAmtMenu, dbPlayer).Show(dbPlayer);
        }

        [RemoteEvent]
        public void EinreiseAmtPlayerBirthday(Client player, string returnstring)
        {
            var dbPlayer = player.GetPlayer();
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            DbPlayer findPlayer = Players.Players.Instance.FindPlayer(dbPlayer.GetData("einreiseamtp"));
            if (findPlayer == null || !findPlayer.IsValid())
            {
                dbPlayer.SendNewNotification("Bürger nicht gefunden!");
                return;
            }

            if (!DateTime.TryParseExact(returnstring, new string[] {"dd.mm.yyyy"}, 
                System.Globalization.CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateTime dt))
            {
                dbPlayer.SendNewNotification("Geburtsdatum muss im Format TAG.MONAT.JAHR eingegeben werden : 09.12.1997");
                ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Einreiseamt-Formular", Callback = "EinreiseAmtPlayerBirthday", Message = "Geben Sie das Geburtsdatum ein : XX.XX.XXXX Beispiel : 09.12.1997 " });
            }
            else
            {
                DbPlayer foundPlayer = Players.Players.Instance.FindPlayer(dbPlayer.GetData("einreiseamtp"));
                MySQLHandler.ExecuteAsync($"UPDATE player SET birthday = '{returnstring}' WHERE id = '{foundPlayer.Id}';");
                dbPlayer.birthday[0] = returnstring;

                dbPlayer.ResetData("einreiseamtp");
            }


        }
    }
}

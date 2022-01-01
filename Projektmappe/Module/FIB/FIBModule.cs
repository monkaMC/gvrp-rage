using GTANetworkAPI;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Commands;
using GVRP.Module.FIB.Menu;
using GVRP.Module.Menu;
using GVRP.Module.NSA.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Windows;
using GVRP.Module.Teams;
using GVRP.Module.Vehicles.InteriorVehicles;


namespace GVRP.Module.FIB
{
    public class FIBModule : Module<FIBModule>
    {
        public static Vector3 UCPoint = new Vector3(152.084, -735.972, 242.152);

        public override void OnPlayerLoadData(DbPlayer dbPlayer, MySqlDataReader reader)
        {
            dbPlayer.UndercoverName = reader.GetString("ucname");

            if(dbPlayer.IsUndercover())
            {
                string[] nameParts = dbPlayer.UndercoverName.Split("_");
                if (nameParts.Length < 2 || nameParts[0].Length < 3 || nameParts[1].Length < 3) return;

                dbPlayer.SetUndercover(nameParts[0], nameParts[1]);
            }
        }

        protected override bool OnLoad()
        {
            MenuManager.Instance.AddBuilder(new FIBPhoneHistoryMenu());
            return base.OnLoad();
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (dbPlayer.TeamId != (int)teams.TEAM_FIB || dbPlayer.Player.IsInVehicle) return false;
            if (dbPlayer.TeamRank < 5) return false;
            if(dbPlayer.Player.Position.DistanceTo(UCPoint) < 2.0f)
            {

                if(dbPlayer.IsUndercover())
                {
                    dbPlayer.ResetUndercover();
                    dbPlayer.SendNewNotification("Sie haben den Undercoverdienst beendet!");
                    dbPlayer.Team.SendNotification($"{dbPlayer.GetName()} hat den Undercover Dienst beendet!", 5000, 10);
                }
                else
                {
                    // Set Undercover
                    ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Undercover Dienst", Callback = "FIBSetUnderCoverName", Message = "Bitte geben Sie einen Decknamen ein (Max_Mustermann):" });
                }
                return true;
            }

            return false;
        }
    }

    public static class FIBPlayerExtension
    {
        public static void SetUndercover(this DbPlayer dbPlayer, string fakename, string surname)
        {
            dbPlayer.fakePerso = true;
            dbPlayer.fakeName = fakename;
            dbPlayer.fakeSurname = surname;

            dbPlayer.UndercoverName = fakename + "_" + surname;
            dbPlayer.UpdateUndercoverName();
            return;
        }
        public static void ResetUndercover(this DbPlayer dbPlayer)
        {
            dbPlayer.fakePerso = false;
            dbPlayer.fakeName = "";
            dbPlayer.fakeSurname = "";

            dbPlayer.UndercoverName = "";
            dbPlayer.UpdateUndercoverName();
            return;
        }

        public static bool IsUndercover(this DbPlayer dbPlayer)
        {
            return dbPlayer.UndercoverName != null && dbPlayer.UndercoverName.Length >= 3;
        }

        public static void UpdateUndercoverName(this DbPlayer dbPlayer)
        {
            MySQLHandler.ExecuteAsync($"UPDATE player SET `ucname` = '{dbPlayer.UndercoverName}' WHERE `id` = '{dbPlayer.Id}'");
        }
    }
}

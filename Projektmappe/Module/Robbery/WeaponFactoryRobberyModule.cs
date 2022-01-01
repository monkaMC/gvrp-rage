using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GTANetworkAPI;
using MySql.Data.MySqlClient;
using GVRP.Handler;
using GVRP.Module.AnimationMenu;
using GVRP.Module.Chat;
using GVRP.Module.Injury;
using GVRP.Module.Items;
using GVRP.Module.Logging;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Phone;
using GVRP.Module.Shops;
using GVRP.Module.Teams;
using GVRP.Module.Voice;
using GVRP.Module.Weapons;
using GVRP.Module.Weapons.Data;

namespace GVRP.Module.Robbery
{
    public sealed class WeaponFactoryRobberyModule : Module<WeaponFactoryRobberyModule>
    {
        // Global Defines
        public bool HasWFRobbed = false;
        public bool IsActive = false;
        public int TimeLeft = 0;
        public int RobberyTime = 30;
        public Team RobberTeam = null;

        public Vector3 RobPosition = new Vector3(3606.72f, 3720.58, 29.6894);

        public override bool Load(bool reload = false)
        {
            HasWFRobbed = false;
            IsActive = false;
            TimeLeft = 0;
            RobberyTime = Configurations.Configuration.Instance.DevMode ? 3 : RobberyTime;
            return true;
        }

        public void LoadWeaponFactoryContainer()
        {
            StaticContainer StaticContainer = StaticContainerModule.Instance.Get((int)StaticContainerTypes.WEAPONFACTORY);
            StaticContainer.Container.ClearInventory();
            StaticContainer.Container.AddItem(303, 96);
            StaticContainer.Locked = false;
            
            TeamModule.Instance.SendMessageToTeam("Die Waffenfabrik ist nun offen!", (teams)RobberTeam.Id);

            IsActive = false;
            TimeLeft = RobberyTime;
            RobberTeam = null;
        }

        public bool CanWeaponFactoryRob()
        {
            var hour = DateTime.Now.Hour;
            var min = DateTime.Now.Minute;

            switch (hour)
            {
                case 7:
                case 15:
                case 23:
                    if (min >= 30)
                    {
                        return false;
                    }

                    break;
                case 8:
                case 16:
                case 0:
                    if (min < 30)
                    {
                        return false;
                    }

                    break;
            }

            return true;
        }

        public void StartRob(DbPlayer dbPlayer)
        {
            if (!dbPlayer.IsAGangster())
            {
                dbPlayer.SendNewNotification( "Große Heists sind nur fuer Gangs/Mafien!");
                return;
            }

            if (Configurations.Configuration.Instance.DevMode != true)
            {
                // Timecheck +- 30 min restarts
                if (!Instance.CanWeaponFactoryRob())
                {
                    dbPlayer.SendNewNotification(
                        "Es scheint als ob die Generatoren nicht bereit sind, das geht nicht. (mind 30 min vor und nach Restarts!)");
                    return;
                }
            }

            if (Instance.IsActive ||
                Instance.HasWFRobbed)
            {
                dbPlayer.SendNewNotification("Die Waffenfabrik wurde bereits ausgeraubt!");
                return;
            }

            if ((TeamModule.Instance.Get((int)teams.TEAM_ARMY).Members.Where(ip => ip.Value != null && ip.Value.IsValid() && ip.Value.Duty).Count() < 30) && !Configurations.Configuration.Instance.DevMode)
            {
                dbPlayer.SendNewNotification("Es muessen mindestens 30 Soldaten im Dienst sein!");
                return;
            }
            
            dbPlayer.SendNewNotification( "Sie versuchen nun den Tresor zu knacken!");

            // Set start datas
            TimeLeft = RobberyTime;
            IsActive = true;
            HasWFRobbed = true;
            RobberTeam = dbPlayer.Team;

            // Get Players For Respect
            int playersAtRob = RobberTeam.Members.Where(m => m.Value.Player.Position.DistanceTo(dbPlayer.Player.Position) < 300f).Count();
            dbPlayer.Team.TeamMetaData.AddRespect(playersAtRob * 80);

            // Messages
            TeamModule.Instance.SendChatMessageToDepartments("An Alle Einheiten, ein Einbruch in der Waffenfabrik wurde gemeldet!");
            TeamModule.Instance.SendMessageToTeam("Sie beginnen einen Ueberfall auf die Waffenfabrik!", (teams)RobberTeam.Id);
            TeamModule.Instance.SendMessageToTeam("Durch den Überfall erhält ihr Team Ansehen! (" + playersAtRob * 80 + "P)", (teams)RobberTeam.Id);
        }

        public void CancelRob()
        {
            TeamModule.Instance.SendChatMessageToDepartments("An Alle Einheiten, der Einbruch auf die Waffenfabrik wurde erfolgreich verhindert!");
            TeamModule.Instance.SendMessageToTeam("Der Ueberfall ist gescheitert!", (teams)RobberTeam.Id);

            IsActive = false;
            RobberTeam = null;
            TimeLeft = RobberyTime;
        }
        
        public override void OnMinuteUpdate()
        {
            if(IsActive)
            {
                // Check if Teamplayer is in Reange
                if (RobberTeam == null || RobberTeam.Members.Where(p => p.Value != null && p.Value.IsValid() && !p.Value.isInjured() && p.Value.Player.Position.DistanceTo(RobPosition) < 20.0f).Count() <= 0)
                {
                    CancelRob();
                    return;
                }

                if (TimeLeft == 1)
                {
                    LoadWeaponFactoryContainer();
                }
                else
                {
                    TimeLeft = TimeLeft - 1;
                }
                Logger.Debug($"Staatsraub timeleft {TimeLeft}");
            }
        }
    }
}
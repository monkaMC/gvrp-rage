using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GVRP.Module.Injury;
using GVRP.Module.Items;
using GVRP.Module.Logging;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.JumpPoints;
using GVRP.Module.Teams;

namespace GVRP.Module.Robbery
{
    public class StaatsbankTunnel
    {
        public Vector3 Position { get; set; }
        public float Heading { get; set; }
        public bool IsOutsideOpen { get; set; }
        public bool IsInsideOpen { get; set; }
        public Team IsActiveForTeam { get; set; }

        public JumpPoint jpOutside { get; set; }
        public JumpPoint jpInside { get; set; }

        public DateTime TunnelCreated { get; set; }
    }

    public sealed class StaatsbankRobberyModule : Module<StaatsbankRobberyModule>
    {
        // Global Defines
        public bool IsActive = false;
        public int TimeLeft = 0;
        public int RobberyTime = 60; // max zeit in SB
        public Team RobberTeam = null;

        public int CountInBreakTresor = 0;

        public bool DoorHacked = false;

        public static int MainDoorId = 2;
        public static int SideDoorId = 3;

        public DateTime LastStaatsbank = DateTime.Now.AddHours(-2);

        public List<StaatsbankTunnel> StaatsbankTunnels = new List<StaatsbankTunnel>();

        public static Vector3 DrillingPoint = new Vector3(249.063, 219.389, 101.684);
        public static Vector3 HackingPoint = new Vector3(264.818, 219.881, 101.683);

        public Vector3 RobPosition = new Vector3(263.758f, 214.239, 101.683);

        public override bool Load(bool reload = false)
        {
            StaatsbankTunnels = new List<StaatsbankTunnel>();

            StaatsbankTunnels.Add(new StaatsbankTunnel()
            {
                Position = new Vector3(-59.36, 184.86, 87.4008),
                Heading = 41.7401f,
                IsActiveForTeam = null,
                IsInsideOpen = false,
                IsOutsideOpen = false,
                jpInside = null,
                jpOutside = null
            });

            StaatsbankTunnels.Add(new StaatsbankTunnel()
            {
                Position = new Vector3(127.651, -114.29, 54.8409),
                Heading = 348.561f,
                IsActiveForTeam = null,
                IsInsideOpen = false,
                IsOutsideOpen = false,
                jpInside = null,
                jpOutside = null
            });

            StaatsbankTunnels.Add(new StaatsbankTunnel()
            {
                Position = new Vector3(1274.83, -1091.35, 38.7322),
                Heading = 126.774f,
                IsActiveForTeam = null,
                IsInsideOpen = false,
                IsOutsideOpen = false,
                jpInside = null,
                jpOutside = null
            });

            StaatsbankTunnels.Add(new StaatsbankTunnel()
            {
                Position = new Vector3(1257.24, -1066.06, 38.7322),
                Heading = 131.823f,
                IsActiveForTeam = null,
                IsInsideOpen = false,
                IsOutsideOpen = false,
                jpInside = null,
                jpOutside = null
            });
            
            IsActive = false;
            TimeLeft = 0;
            RobberyTime = Configurations.Configuration.Instance.DevMode ? 3 : 20;
            return true;
        }
        
        public void LoadContainerBankInv(Container container)
        {
            Random rnd = new Random();
            container.ClearInventory();
            container.AddItem(487, rnd.Next(18,33));
            container.AddItem(880, 1); // Banknotencodes
        }

        public bool CanStaatsbankRobbed()
        {
            // Timecheck +- 30 min restarts
            var hour = DateTime.Now.Hour;
            var min = DateTime.Now.Minute;

            if (Configurations.Configuration.Instance.DevMode) return true;

            // Check other Robs
            if(RobberyModule.Instance.Robberies.Where(r => r.Value.Type == RobType.Juwelier && RobberyModule.Instance.IsActive(r.Value.Id)).Count() > 0)
            {
                return false;
            }

            switch (hour)
            {
                case 7:
                case 15:
                case 23:
                    if (min >= 10)
                    {
                        return false;
                    }

                    break;
                case 8:
                case 16:
                case 0:
                    if (min < 15)
                    {
                        return false;
                    }

                    break;
            }


            return true;
        }

        public async void StartRob(DbPlayer dbPlayer)
        {
            Main.m_AsyncThread.AddToAsyncThread(new Task(() =>
            {
                if (!dbPlayer.IsAGangster())
                {
                    dbPlayer.SendNewNotification("Große Heists sind nur fuer Gangs/Mafien!");
                    return;
                }

                if (Configurations.Configuration.Instance.DevMode != true)
                {
                    // Timecheck +- 30 min restarts
                    if (!Instance.CanStaatsbankRobbed())
                    {
                        dbPlayer.SendNewNotification("Es scheint als ob die Generatoren nicht bereit sind, das geht nicht. (mind 30 min vor und nach Restarts!)");
                        return;
                    }
                }

                if (Instance.IsActive || (LastStaatsbank.AddHours(2) > DateTime.Now && !Configurations.Configuration.Instance.DevMode))
                {
                    dbPlayer.SendNewNotification("Die Staatsbank wurde bereits ausgeraubt!");
                    return;
                }

                if (TeamModule.Instance.DutyCops < 20 && !Configurations.Configuration.Instance.DevMode)
                {
                    dbPlayer.SendNewNotification("Es muessen mindestens 20 Beamte im Dienst sein!");
                    return;
                }

                DateTime actualDate = DateTime.Now;
                if (dbPlayer.Team.LastBankRobbery.AddHours(72) < actualDate)
                {
                    dbPlayer.SendNewNotification("Sie versuchen nun den Tresor zu knacken!");

                    // Set start datas
                    TimeLeft = RobberyTime;
                    IsActive = true;
                    DoorHacked = false;
                    RobberTeam = dbPlayer.Team;

                    // Messages
                    TeamModule.Instance.SendChatMessageToDepartments("An Alle Einheiten, ein Einbruch in der Staatsbank wurde gemeldet!");
                    TeamModule.Instance.SendMessageToTeam("Sie beginnen einen Ueberfall auf die Staatsbank!", (teams)RobberTeam.Id);

                    LastStaatsbank = DateTime.Now;
                    dbPlayer.SaveLastBankRobbery();
                }
                else
                {
                    dbPlayer.SendNewNotification("Die Staatsbank wurde heute bereits von deinem Team ausgeraubt!");
                    return;
                }
            }));
        }

        public void CloseRob()
        {
            StaticContainer StaticContainer = StaticContainerModule.Instance.Get((uint)StaticContainerTypes.STAATSBANK1);
            StaticContainer.Container.ClearInventory();
            StaticContainer.Locked = true;

            StaticContainer = StaticContainerModule.Instance.Get((uint)StaticContainerTypes.STAATSBANK1);
            StaticContainer.Container.ClearInventory();
            StaticContainer.Locked = true;

            StaticContainer = StaticContainerModule.Instance.Get((uint)StaticContainerTypes.STAATSBANK2);
            StaticContainer.Container.ClearInventory();
            StaticContainer.Locked = true;

            StaticContainer = StaticContainerModule.Instance.Get((uint)StaticContainerTypes.STAATSBANK3);
            StaticContainer.Container.ClearInventory();
            StaticContainer.Locked = true;

            StaticContainer = StaticContainerModule.Instance.Get((uint)StaticContainerTypes.STAATSBANK4);
            StaticContainer.Container.ClearInventory();
            StaticContainer.Locked = true;

            StaticContainer = StaticContainerModule.Instance.Get((uint)StaticContainerTypes.STAATSBANK5);
            StaticContainer.Container.ClearInventory();
            StaticContainer.Locked = true;

            StaticContainer = StaticContainerModule.Instance.Get((uint)StaticContainerTypes.STAATSBANK6);
            StaticContainer.Container.ClearInventory();
            StaticContainer.Locked = true;

            StaticContainer = StaticContainerModule.Instance.Get((uint)StaticContainerTypes.STAATSBANK7);
            StaticContainer.Container.ClearInventory();
            StaticContainer.Locked = true;

            StaticContainer = StaticContainerModule.Instance.Get((uint)StaticContainerTypes.STAATSBANK8);
            StaticContainer.Container.ClearInventory();
            StaticContainer.Locked = true;

            this.IsActive = false;
            this.RobberTeam = null;
            this.TimeLeft = RobberyTime;
            DoorHacked = false;
        }

        public void CancelRob()
        {
            TeamModule.Instance.SendChatMessageToDepartments("An Alle Einheiten, der Einbruch auf die Staatsbank wurde erfolgreich verhindert!");
            TeamModule.Instance.SendMessageToTeam("Der Überfall ist gescheitert!", (teams)RobberTeam.Id);

            IsActive = false;
            RobberTeam = null;
            TimeLeft = RobberyTime;
        }

        public override void OnMinuteUpdate()
        {
            if(IsActive)
            {
                // Check if Teamplayer is in Reange
                if(RobberTeam == null || RobberTeam.Members.Where(p => p.Value != null && p.Value.IsValid() && !p.Value.isInjured() && p.Value.Player.Position.DistanceTo(RobPosition) < 15.0f).Count() <= 0)
                {
                    CancelRob();
                    return;
                }

                if(TimeLeft == 45) // nach 15 min weil 60 XX
                {
                    // Get Players For Respect
                    int playersAtRob = RobberTeam.Members.Where(m => m.Value.Player.Position.DistanceTo(RobPosition) < 300f).Count();
                    RobberTeam.TeamMetaData.AddRespect(playersAtRob * 100);
                    TeamModule.Instance.SendMessageToTeam("Durch den Überfall erhält ihr Team Ansehen! (" + playersAtRob * 100 + "P)", (teams)RobberTeam.Id);
                    
                }
                if(TimeLeft == 60)
                {
                    CloseRob();
                }

                Logger.Debug($"Staatsraub timeleft {TimeLeft}");
                TimeLeft--;
            }

            // Schließe Tunnel wieder... nach 15 min
            StaatsbankTunnel staatsbankTunnel = StaatsbankTunnels.Where(
                t => t.IsActiveForTeam != null && t.IsInsideOpen && t.IsOutsideOpen 
                 && t.TunnelCreated != null && t.TunnelCreated.AddMinutes(15) < DateTime.Now).FirstOrDefault();

            if(staatsbankTunnel != null)
            {
                NAPI.Task.Run(() =>
                {
                    if (staatsbankTunnel.jpOutside != null)
                    {
                        JumpPointModule.Instance.jumpPoints.Remove(staatsbankTunnel.jpOutside.Id);
                        if (staatsbankTunnel.jpOutside.ColShape != null)
                        {
                            staatsbankTunnel.jpOutside.ColShape?.Delete();
                        }
                        staatsbankTunnel.jpOutside = null;
                    }

                    if (staatsbankTunnel.jpInside != null)
                    {
                        JumpPointModule.Instance.jumpPoints.Remove(staatsbankTunnel.jpInside.Id);
                        if (staatsbankTunnel.jpInside.ColShape != null)
                        {
                            staatsbankTunnel.jpInside.ColShape?.Delete();
                        }
                        staatsbankTunnel.jpInside = null;
                    }
                });
            }
        }
    }
}
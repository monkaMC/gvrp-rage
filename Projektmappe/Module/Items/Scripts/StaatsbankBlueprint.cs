using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GVRP.Handler;
using GVRP.Module.Chat;
using GVRP.Module.Doors;
using GVRP.Module.GTAN;
using GVRP.Module.Houses;
using GVRP.Module.Injury;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.JumpPoints;
using GVRP.Module.Players.PlayerAnimations;
using GVRP.Module.Robbery;
using GVRP.Module.Spawners;
using GVRP.Module.Teams;

namespace GVRP.Module.Items.Scripts
{
    public static partial class ItemScript
    {
        public static bool StaatsbankBlueprint(DbPlayer iPlayer, ItemModel ItemData)
        {
            // da selbe kriterium wie bankrob
            if (!StaatsbankRobberyModule.Instance.CanStaatsbankRobbed() || !iPlayer.IsAGangster()) return false;
            
            List<StaatsbankTunnel> list = StaatsbankRobberyModule.Instance.StaatsbankTunnels.Where(t => t.IsActiveForTeam == null).ToList();
            Random rand = new Random();
            
            if (list.Count <= 0) return false;
            
            StaatsbankTunnel staatsbankTunnel = list.ElementAt(rand.Next(0, list.Count));
            if (staatsbankTunnel == null) return false;

            // Setze den vorherigen frei, falls einer da war
            StaatsbankTunnel usedTunnelBevore = StaatsbankRobberyModule.Instance.StaatsbankTunnels.ToList().Where(t => t.IsActiveForTeam == iPlayer.Team).FirstOrDefault();

            if(usedTunnelBevore != null)
            {
                usedTunnelBevore.IsActiveForTeam = null;
            }

            staatsbankTunnel.IsActiveForTeam = iPlayer.Team;
            iPlayer.SendNewNotification("Du konntest durch den Bauplan eine Stelle für einen Tunnelbau ausmachen!");
            iPlayer.Player.TriggerEvent("setPlayerGpsMarker", staatsbankTunnel.Position.X, staatsbankTunnel.Position.Y);
            return true;
        }

        public static async Task<bool> StaatsbankDrill(DbPlayer iPlayer, ItemModel ItemData)
        {
            // da selbe kriterium wie bankrob
            if (iPlayer.Player.Position.DistanceTo(StaatsbankRobberyModule.DrillingPoint) > 4.0f || !StaatsbankRobberyModule.Instance.IsActive || StaatsbankRobberyModule.Instance.RobberTeam != iPlayer.Team) return false;

            StaatsbankTunnel staatsbankTunnel = StaatsbankRobberyModule.Instance.StaatsbankTunnels.Where(t => t.IsActiveForTeam == iPlayer.Team).FirstOrDefault();
            if (staatsbankTunnel == null) return false;

            if(!staatsbankTunnel.IsOutsideOpen)
            {
                iPlayer.SendNewNotification("Um einen Tunnel zu bohren, müssen erst Gitterstäbe in der Kanalisation durchgeschweißt werden!");
                return false;
            }

            Chats.sendProgressBar(iPlayer, 60000);

            iPlayer.SendNewNotification("Du beginnst nun den Tunnel zu bohren!");

            iPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@world_human_const_drill@male@drill@base", "base");
            iPlayer.Player.TriggerEvent("freezePlayer", true);
            iPlayer.SetData("userCannotInterrupt", true);

            await Task.Delay(60000);

            iPlayer.ResetData("userCannotInterrupt");
            if (iPlayer.IsCuffed || iPlayer.IsTied || iPlayer.isInjured()) return true;
            iPlayer.Player.TriggerEvent("freezePlayer", false);
            NAPI.Player.StopPlayerAnimation(iPlayer.Player);

            staatsbankTunnel.IsInsideOpen = true;
            iPlayer.SendNewNotification("Du hast einen Tunnel zur Kanalisation gebohrt! Achtung, der Tunnel wird mit der Zeit verschüttet (15 min)");
            
            TeamModule.Instance.SendMessageToTeam("[INFO] Die Seismografen haben unterhalb der Staatsbank erschütterungen wahrgenommen!", teams.TEAM_FIB);

            // jump Points...


            NAPI.Task.Run(() =>
            {
                staatsbankTunnel.jpInside = new JumpPoint
                {
                    Id = JumpPointModule.Instance.jumpPoints.Last().Key + 1,
                    Name = "Tunnel",
                    Position = iPlayer.Player.Position,
                    AdminUnbreakable = true,
                    DestinationId = JumpPointModule.Instance.jumpPoints.Last().Key + 2,
                    Dimension = 0,
                    Heading = iPlayer.Player.Heading,
                    InsideVehicle = false,
                    LastBreak = DateTime.Now.Add(new TimeSpan(0, -5, 0)),
                    Locked = false,
                    Range = 2,
                    Teams = new HashSet<Team>(),
                    Unbreakable = true
                };

                staatsbankTunnel.jpOutside = new JumpPoint
                {
                    Id = JumpPointModule.Instance.jumpPoints.Last().Key + 2,
                    Name = "Tunnel",
                    Position = staatsbankTunnel.Position,
                    AdminUnbreakable = true,
                    DestinationId = JumpPointModule.Instance.jumpPoints.Last().Key + 1,
                    Dimension = 0,
                    Heading = staatsbankTunnel.Heading,
                    InsideVehicle = false,
                    LastBreak = DateTime.Now.Add(new TimeSpan(0, -5, 0)),
                    Locked = false,
                    Range = 2,
                    Teams = new HashSet<Team>(),
                    Unbreakable = true
                };

                staatsbankTunnel.jpInside.Destination = staatsbankTunnel.jpOutside;
                staatsbankTunnel.jpOutside.Destination = staatsbankTunnel.jpInside;

                staatsbankTunnel.jpInside.ColShape = ColShapes.Create(staatsbankTunnel.jpInside.Position, 2, 0);
                staatsbankTunnel.jpOutside.ColShape = ColShapes.Create(staatsbankTunnel.jpOutside.Position, 2, 0);
                staatsbankTunnel.jpInside.ColShape.SetData("jumpPointId", staatsbankTunnel.jpInside.Id);
                staatsbankTunnel.jpOutside.ColShape.SetData("jumpPointId", staatsbankTunnel.jpOutside.Id);
                
                JumpPointModule.Instance.jumpPoints.Add(staatsbankTunnel.jpInside.Id, staatsbankTunnel.jpInside);
                JumpPointModule.Instance.jumpPoints.Add(staatsbankTunnel.jpOutside.Id, staatsbankTunnel.jpOutside);

                staatsbankTunnel.TunnelCreated = DateTime.Now;
            });

            return true;
        }
    }
}
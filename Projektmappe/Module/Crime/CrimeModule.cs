using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GVRP.Handler;
using GVRP.Module.Injury;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Vehicles;

namespace GVRP.Module.Crime
{
    public sealed class CrimeModule : Module<CrimeModule>
    {
        public List<Vector3> ComputerPositions = new List<Vector3>();
     
        public override bool Load(bool reload = false)
        {
            ComputerPositions = new List<Vector3>();
            ComputerPositions.Add(new Vector3(440.971, -978.654, 31.690));
            ComputerPositions.Add(new Vector3(461.575, -988.992, 24.9149));

            MenuManager.Instance.AddBuilder(new CrimeJailMenuBuilder());
            MenuManager.Instance.AddBuilder(new CrimeArrestMenuBuilder());
            return base.Load(reload);
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key != Key.J || !dbPlayer.Player.IsInVehicle) return false;
            SxVehicle sxVeh = dbPlayer.Player.Vehicle.GetVehicle();
            if (!sxVeh.IsValid() || sxVeh.teamid != dbPlayer.TeamId || (dbPlayer.TeamId != (int)teams.TEAM_POLICE && dbPlayer.TeamId != (int)teams.TEAM_COUNTYPD)) return false;
            try
            {
                if ((dbPlayer.TeamRank < 6 && sxVeh.GetSpeed() > 0) || (dbPlayer.Player.VehicleSeat == -1 && dbPlayer.TeamRank < 6))
                {
                    dbPlayer.SendNewNotification("Messungen während der Fahrt sind erst ab rang 6 erlaubt.");
                    return false;
                }

                if(dbPlayer.HasData("lastblitzed"))
                {
                    if(dbPlayer.GetData("lastblitzed").AddSeconds(15) > DateTime.Now)
                    {
                        dbPlayer.SendNewNotification("Du kannst nur alle 15 Sekunden blitzen!");
                        return false;
                    }
                }

                //Get Closest Player kleiner als 6 und speed größer als 0 messungen wärend der fahrt 
                List<DbPlayer> targetPlayers = Players.Players.Instance.GetPlayersInRange(dbPlayer.Player.Position, 40).ToList();

                bool targetFound = false;
                bool msgsend = false;
                foreach (DbPlayer tpl in targetPlayers)
                {
                    if (!tpl.Player.IsInVehicle) continue;
                    if (dbPlayer.Player.Vehicle == tpl.Player.Vehicle || tpl.Player.VehicleSeat != -1) continue;
                    SxVehicle targetVeh = tpl.Player.Vehicle.GetVehicle();
                    if (!targetVeh.IsValid() || targetVeh.GetSpeed() <= 0) continue;
                    if (!msgsend)
                    {
                        dbPlayer.SendNewNotification("Folgende Fahrzeuge wurden geblitzt:");
                        msgsend = true;
                    }
                    dbPlayer.SendNewNotification($"{targetVeh.GetSpeed()} KM/H - [{targetVeh.GetName()}]");
                    targetFound = true;
                }
                if(targetFound)
                {
                    dbPlayer.SetData("lastblitzed", DateTime.Now);
                }
                return true;
            }
            catch(Exception e)
            {
                Logging.Logger.Crash(e);
            }
            return false;
        }

        public bool IsInRangeOfPDComputers(Vector3 pos)
        {
            return (ComputerPositions.FindAll(p => p.DistanceTo(pos) < 3.0f).Count() > 0);
        }
        
        public override void OnPlayerMinuteUpdate(DbPlayer iPlayer)
        {
            if (iPlayer == null || !iPlayer.IsValid()) return;

            if (Instance.TicketHighIsJail(iPlayer.Crimes))
            {
                // Gebe Wanted wenn Ticketkosten größer als 30k
                return;
            }
        }

        public int CalcJailTime(IEnumerable<CrimePlayerReason> wantedList)
        {
            return wantedList.ToList().Sum(wanted => wanted.Jailtime);
        }

        public bool TicketHighIsJail(IEnumerable<CrimePlayerReason> wantedList)
        {
            return (wantedList.ToList().Sum(wanted => wanted.Jailtime) == 0 && wantedList.Sum(wanted => wanted.Costs) > 60000);
        }

        public int CalcJailCosts(IEnumerable<CrimePlayerReason> wantedList)
        {
            return wantedList.ToList().Sum(wanted => wanted.Costs);
        }

        public int CalcWantedStars(IEnumerable<CrimePlayerReason> wantedList)
        {
            return CalcJailTime(wantedList) / 10;
        }
    }
}
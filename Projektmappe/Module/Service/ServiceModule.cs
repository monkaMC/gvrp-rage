using System;
using System.Collections.Generic;
using System.Linq;
using GVRP.Module.Injury;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Zone;

namespace GVRP.Module.Service
{
    public sealed class ServiceModule : Module<ServiceModule>
    {
        public Dictionary<uint, List<Service>> serviceList;

        public override bool Load(bool reload = false)
        {
            serviceList = new Dictionary<uint, List<Service>>();
            return true;
        }

        public List<Service> GetServicesForTeam(uint teamId)
        {
            // CopSpecials
            if (teamId == (int)teams.TEAM_FIB || teamId == (int)teams.TEAM_COUNTYPD) teamId = (int)teams.TEAM_POLICE;

            List<Service> teamServices;

            if (!serviceList.ContainsKey(teamId))
            {
                teamServices = new List<Service>();
                serviceList.Add(teamId, teamServices);
            }
            else
            {
                teamServices = serviceList[teamId];
            }

            return teamServices;
        }

        public List<Service> GetAvailableServices(DbPlayer iPlayer)
        {
            return GetServicesForTeam(iPlayer.TeamId).Where(service => service.Accepted.Count() == 0).ToList();
        }

        public List<Service> GetCreatedServices(DbPlayer iPlayer)
        {
            try
            {
                List<Service> returnServices = new List<Service>();

                foreach(KeyValuePair<uint, List<Service>> kvp in serviceList)
                {
                    if(kvp.Value != null && kvp.Value.Count > 0)
                    {
                        foreach(Service service in kvp.Value.ToList().Where(s => s.Player.Id == iPlayer.Id))
                        {
                            returnServices.Add(service);
                        }
                    }
                }

                return returnServices;
            }
            catch(Exception e) {
                Logging.Logger.Crash(e);
                return new List<Service>();
            }
        }

        public List<Service> GetAcceptedTeamServices(DbPlayer iPlayer)
        {
            return GetServicesForTeam(iPlayer.TeamId).Where(service => service.Accepted.Count() > 0).ToList();
        }

        public List<Service> GetAcceptedServices(DbPlayer iPlayer)
        {
            return GetServicesForTeam(iPlayer.TeamId).Where(service => service.Accepted.Contains(iPlayer.GetName())).ToList();
        }

        public override void OnMinuteUpdate()
        {
            if (!serviceList.ContainsKey((int)teams.TEAM_MEDIC)) return;
            foreach(Service service in serviceList[(int)teams.TEAM_MEDIC].Where(s => s.Player != null && s.Player.IsValid() && s.Player.isInjured()))
            {
                service.Message = $"Verletzung: {service.Player.Injury.Name} | {service.Player.Injury.TimeToDeath - service.Player.deadtime[0]} Min";
            }
        }

        public void RemoveInjuredPlayerService(DbPlayer dbPlayer)
        {
            if (!serviceList.ContainsKey((int)teams.TEAM_MEDIC)) return;
            Instance.CancelOwnService(dbPlayer, (uint)teams.TEAM_MEDIC);
            dbPlayer.ResetData("service");
        }

        public bool Add(DbPlayer iPlayer, uint teamId, Service service)
        {
            var teamServices = GetServicesForTeam(teamId);
            foreach (var itr in teamServices)
            {
                if (itr.Player == null)
                    continue;

                if (itr.Player.Id == iPlayer.Id)
                    return false;
            }

            teamServices.Add(service);
            return true;
        }

        public string GetSpecialDescriptionForPlayer(DbPlayer iPlayer, Service service)
        {
            string desc = "[" + Convert.ToInt32(service.Position.DistanceTo(iPlayer.Player.Position)) + "m - gesendet: " + service.Created.ToString("HH:mm:ss") + " ";
            if (service.TeamId == (int)teams.TEAM_MEDIC)
            {
                desc += ZoneModule.Instance.IsInNorthZone(service.Position) ? " | Norden]" : "]";

            }
            else desc += "]";

            desc += service.Message;

            return desc;
        }

        public bool Accept(DbPlayer iPlayer, DbPlayer destinationPlayer)
        {
            var createdService = GetCreatedServices(destinationPlayer);
            if (createdService == null || createdService.Count() <= 0) return false;

            uint playerTeam = (iPlayer.TeamId != (uint)teams.TEAM_FIB && iPlayer.TeamId != (uint)teams.TEAM_COUNTYPD) ? (uint)iPlayer.TeamId : (uint)teams.TEAM_POLICE;

            if (playerTeam != createdService[0].TeamId) return false;
            if (createdService[0].Accepted.Contains(iPlayer.GetName())) return false;
            
            bool status = createdService[0].Accepted.Add(iPlayer.GetName());
            return status;
        }

        public bool CancelOwnService(DbPlayer iPlayer, uint teamId)
        {
            var teamServices = GetServicesForTeam(teamId);
            if (teamServices.Count == 0) return false;

            var createdService = GetCreatedServices(iPlayer);
            if (createdService.Count <= 0) return false;

            if (!teamServices.Contains(createdService[0])) return false;
            bool status = teamServices.Remove(createdService[0]);
            return status;
        }

        public bool Cancel(DbPlayer iPlayer, DbPlayer player, uint teamId)
        {
            var teamServices = GetServicesForTeam(teamId);
            if (teamServices.Count == 0) return false;

            var createdService = GetCreatedServices(player);
            if (createdService == null || createdService.Count() <= 0) return false;

            bool status = teamServices.Remove(createdService[0]);
            return status;
        }

        public override void OnPlayerDisconnected(DbPlayer dbPlayer, string reason)
        {
            if(dbPlayer.isInjured())
            {
                CancelOwnService(dbPlayer, (int)teams.TEAM_MEDIC);
            }
        }
    }
}

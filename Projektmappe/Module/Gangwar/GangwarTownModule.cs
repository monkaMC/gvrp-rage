using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using GVRP.Module.Logging;
using GVRP.Module.Players.Db;
using GVRP.Module.Spawners;
using GVRP.Module.Teams;

namespace GVRP.Module.Gangwar
{
    public class GangwarTownModule : SqlModule<GangwarTownModule, GangwarTown, uint>
    {
        public static Vector3 BankPosition = new Vector3(1044.59, -3194.81, -38.1579);

        protected override string GetQuery()
        {
            return "SELECT * FROM `gangwar_towns`;";
        }

        protected override void OnItemLoaded(GangwarTown gwTown)
        {
            // Create Map Blip on load
            gwTown.Blip = Blips.Create(gwTown.Position, "Gangwar", 543, 1.0f, color: gwTown.OwnerTeam.Id != 0 ? gwTown.OwnerTeam.BlipColor : 0);
            Main.ServerBlips.Add(gwTown.Blip);
        }

        public GangwarTown FindBank(DbPlayer dbPlayer)
        {
            if (dbPlayer.Player.Dimension == 0) return null;
            return Instance.GetAll().FirstOrDefault(gt => gt.Value.Id == dbPlayer.Id && dbPlayer.Player.Position.DistanceTo(BankPosition) < 1.5f).Value;
        }

        public GangwarTown GetByPosition(Vector3 Position)
        {
            return Instance.GetAll().FirstOrDefault(gt => gt.Value.InteriorPosition.DistanceTo(Position) < 3.0f).Value;
        }

        public GangwarTown FindActiveByTeam(Team team)
        {
            return Instance.GetAll().FirstOrDefault(gt => gt.Value.IsAttacked && (gt.Value.AttackerTeam == team || gt.Value.DefenderTeam == team)).Value;
        }
        
        public bool IsTeamInGangwar(Team team)
        {
            return GangwarTownModule.Instance.GetAll().Where(g => g.Value.AttackerTeam == team || g.Value.DefenderTeam == team).Count() > 0;
        }

        public GangwarTown FindByOwner(Team team)
        {
            return Instance.GetAll().FirstOrDefault(gt => gt.Value.OwnerTeam  != null && gt.Value.OwnerTeam == team).Value;
        }

        public GangwarTown GetNearByPlayer(DbPlayer dbPlayer)
        {
            foreach (KeyValuePair<uint, GangwarTown> kvp in GangwarTownModule.Instance.GetAll())
            {
                if ((dbPlayer.HasData("gangwarId") && dbPlayer.GetData("gangwarId") == kvp.Value.Id))
                {
                    return kvp.Value;
                }
            }
            return null;
        }

        public int GetCarShopDiscount(Team team)
        {
            return (Instance.GetOwnedTownsCount(team) * 5);
        }

        public int GetOwnedTownsCount(Team team)
        {
            return (Instance.GetAll().Where(gt => gt.Value.OwnerTeam == team).Count());
        }
    }
}
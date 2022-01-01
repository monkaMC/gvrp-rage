using System;
using System.Collections.Generic;
using System.Linq;
using GTANetworkAPI;
using GVRP.Module.Events.Halloween;
using GVRP.Module.GTAN;
using GVRP.Module.Houses;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Spawners;
using GVRP.Module.Teams;

namespace GVRP.Module.Doors
{
    public enum DoorGroups
    {
        MainJail = 1,
        MissionRowPolice = 2,
        SandyPolice = 3,
        PaletoPolice = 4,
        PierPolice = 5,
        GovGerichtMinisterium = 6,
        KrankenhausPaleto = 10,
        Krankenhaus3_1 = 15,
    }


    public sealed class DoorModule : SqlModule<DoorModule, Door, uint>
    {
        public override Type[] RequiredModules()
        {
            return new[] { typeof(TeamModule), typeof(HouseModule) };
        }

        protected override string GetQuery()
        {
            return "SELECT * FROM `doors`;";
        }
                
        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if (colShapeState == ColShapeState.Exit)
            {
                if (!dbPlayer.TryData("doorId", out uint playerDoorId)) return false;
                if (!colShape.TryData("doorId", out uint colShapeDoorId)) return false;
                if (colShapeDoorId != playerDoorId) return false;
                dbPlayer.ResetData("doorId");
                return false;
            }
            else
            {
                if (!colShape.TryData("doorId", out uint doorId)) return false;
                var door = Get(doorId);
                if (door == null) return false;

                dbPlayer.SetData("doorId", door.Id);
                dbPlayer.Player.SetDoorState(door);

                if (door.Pair == 0) return false;
                var pairDoor = Get(door.Pair);
                if (pairDoor == null) return false;

                dbPlayer.Player.SetDoorState(pairDoor);
                return false;
            }
        }

        public static List<Door> GetAllByGroup(DoorGroups group)
        {
            return Instance.GetAll().Values.Where(d => d.Group == (int)group).ToList();
        }

        public bool CanOpenHouseDoor(DbPlayer dbPlayer, Door door)
        {
            House ownHouse = HouseModule.Instance.Get(dbPlayer.ownHouse[0]);
            if (ownHouse != null && door.Houses.Contains(HouseModule.Instance.Get(dbPlayer.ownHouse[0]).Id)) return true;

            foreach (uint houseId in door.Houses)
            {
                if (dbPlayer.IsTenant() && dbPlayer.GetTenant().HouseId == houseId) return true;
                if (dbPlayer.HouseKeys.Contains(houseId)) return true;
            }

            return false;
        }

        public bool CanInteract(DbPlayer dbPlayer)
        {
            if (!dbPlayer.TryData("doorId", out uint doorId)) return false;
            var door = Get(doorId);
            if (door == null) return false;

            if ((door.Teams.Contains(dbPlayer.Team) && dbPlayer.TeamRank >= door.RangRestriction) || 
                dbPlayer.Rank.CanAccessFeature("enter_all") || 
                (door.Houses.Count > 0 && CanOpenHouseDoor(dbPlayer, door))) return true;
            if (dbPlayer.GovLevel == "A" || dbPlayer.GovLevel == "B")
            {
                if (door.Group == (int)DoorGroups.GovGerichtMinisterium || door.Group == (int)DoorGroups.Krankenhaus3_1 || door.Group == (int)DoorGroups.KrankenhausPaleto ||
                    door.Group == (int)DoorGroups.MainJail || door.Group == (int)DoorGroups.MissionRowPolice || door.Group == (int)DoorGroups.PaletoPolice || door.Group == (int)DoorGroups.PierPolice ||
                    door.Group == (int)DoorGroups.SandyPolice) return true;
            }
            return false;
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key != Key.L) return false;
            if (!dbPlayer.TryData("doorId", out uint doorId)) return false;
            var door = Get(doorId);
            if (door == null) return false;

            if (!CanInteract(dbPlayer)) return false;

            if (door.LastBreak.AddMinutes(5) > DateTime.Now) return false; // Bei einem Break, kann 5 min nicht interagiert werden

            door.SetLocked(!door.Locked);

            dbPlayer.SendNewNotification(door.Locked ? "Tuer abgeschlossen" : "Tuer aufgeschlossen", notificationType: door.Locked ? PlayerNotification.NotificationType.ERROR : PlayerNotification.NotificationType.SUCCESS);
            if (dbPlayer.Rank.CanAccessFeature("debug"))
            {
                dbPlayer.SendNewNotification("DOOR-ID : " + door.Id);
            }

            // Add
            door.LastUseds.Add(new LastUsedFrom() { Name = dbPlayer.GetName(), DateTime = DateTime.Now, Opened = !door.Locked });
            if(door.Pair > 0)
            {
                Door pair = Get(door.Pair);
                if(pair != null)
                {
                    pair.LastUseds.Add(new LastUsedFrom() { Name = dbPlayer.GetName(), DateTime = DateTime.Now, Opened = !pair.Locked });
                }
            }
            return true;
        }
    }
}
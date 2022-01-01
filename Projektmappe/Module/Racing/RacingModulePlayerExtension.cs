using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GVRP.Handler;
using GVRP.Module.Customization;
using GVRP.Module.Outfits;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Racing
{
    public static class RacingModulePlayerExtension
    {
        public static void SetPlayerIntoRacing(this DbPlayer dbPlayer, RacingLobby racingLobby)
        {
            Vector3 spawnPosition = RacingModule.Box1Veh;

            SxVehicle closesPointVehicle = VehicleHandler.Instance.GetClosestVehicle(RacingModule.Box1Veh, 4.0f, racingLobby.RacingDimension);
            // Search for Slot
            if(closesPointVehicle == null)
            {
                spawnPosition = RacingModule.Box1Veh;
            }

            closesPointVehicle = VehicleHandler.Instance.GetClosestVehicle(RacingModule.Box2Veh, 4.0f, racingLobby.RacingDimension);
            // Search for Slot
            if (closesPointVehicle == null)
            {
                spawnPosition = RacingModule.Box2Veh;
            }

            closesPointVehicle = VehicleHandler.Instance.GetClosestVehicle(RacingModule.Box3Veh, 4.0f, racingLobby.RacingDimension);
            // Search for Slot
            if (closesPointVehicle == null)
            {
                spawnPosition = RacingModule.Box3Veh;
            }

            closesPointVehicle = VehicleHandler.Instance.GetClosestVehicle(RacingModule.Box4Veh, 4.0f, racingLobby.RacingDimension);
            // Search for Slot
            if (closesPointVehicle == null)
            {
                spawnPosition = RacingModule.Box4Veh;
            }

            closesPointVehicle = VehicleHandler.Instance.GetClosestVehicle(RacingModule.Box5Veh, 4.0f, racingLobby.RacingDimension);
            // Search for Slot
            if (closesPointVehicle == null)
            {
                spawnPosition = RacingModule.Box5Veh;
            }

            closesPointVehicle = VehicleHandler.Instance.GetClosestVehicle(RacingModule.Box6Veh, 4.0f, racingLobby.RacingDimension);
            // Search for Slot
            if (closesPointVehicle == null)
            {
                spawnPosition = RacingModule.Box6Veh;
            }


            // Spawn Vehicle & Player
            NAPI.Task.Run(async () =>
            {
                dbPlayer.Player.Dimension = racingLobby.RacingDimension;
                dbPlayer.Dimension[0] = racingLobby.RacingDimension;
                dbPlayer.DimensionType[0] = DimensionType.RacingArea;

                Random rnd = new Random();
                int rx = rnd.Next(1179, 1185);

                OutfitsModule.Instance.SetPlayerOutfit(dbPlayer, rx);

                dbPlayer.Player.SetPosition(spawnPosition);

                dbPlayer.SetData("inRacing", racingLobby.LobbyId);

                var sxVehicle = VehicleHandler.Instance.CreateServerVehicle(RacingModule.RacingVehicleDataId, false,
                    spawnPosition, RacingModule.BoxVehHeading, Main.rndColor(),
                    Main.rndColor(), racingLobby.RacingDimension, true, true, false, 0, dbPlayer.GetName(), 0, 999, dbPlayer.Id);

                await Task.Delay(2000);
                if (dbPlayer.Player == null || !NAPI.Pools.GetAllPlayers().Contains(dbPlayer.Player) || !dbPlayer.Player.Exists) return;
                if (sxVehicle != null && sxVehicle.entity != null) dbPlayer.Player.SetIntoVehicle(sxVehicle.entity, -1);

                racingLobby.RacingVehicles.Add(sxVehicle);
            });

            if(!racingLobby.RacingPlayers.Contains(dbPlayer)) racingLobby.RacingPlayers.Add(dbPlayer);
        }

        public static void RemoveFromRacing(this DbPlayer dbPlayer)
        {
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            dbPlayer.Player.SetPosition(RacingModule.RacingMenuPosition);
            dbPlayer.Player.Dimension = 0;
            dbPlayer.Dimension[0] = 0;
            dbPlayer.DimensionType[0] = DimensionType.World;
            dbPlayer.ResetData("inRacing");

            dbPlayer.ApplyCharacter();

            // Cleanup Vehicles
            foreach (RacingLobby racingLobby in RacingModule.Lobbies)
            {
                SxVehicle playerRacingVehicle = racingLobby.RacingVehicles.ToList().Where(rv => rv.ownerId == dbPlayer.Id).FirstOrDefault();
                if (playerRacingVehicle == null) continue;

                racingLobby.RacingVehicles.Remove(playerRacingVehicle);
                VehicleHandler.Instance.DeleteVehicle(playerRacingVehicle);

                // Remove Player
                if (racingLobby.RacingPlayers.Contains(dbPlayer)) racingLobby.RacingPlayers.Remove(dbPlayer);
            }
        }

        public static void SetBestTime(this DbPlayer dbPlayer, int besttime)
        {
            if (dbPlayer == null || !dbPlayer.IsValid()) return;

            MySQLHandler.ExecuteAsync($"UPDATE player SET racing_besttime = '{besttime}' WHERE id = '{dbPlayer.Id}';");

            dbPlayer.RacingBestTimeSeconds = besttime;
            return;
        }
    }
}

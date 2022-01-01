using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GVRP.Handler;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Racing.Menu;
using GVRP.Module.Vehicles;

namespace GVRP.Module.Racing
{
    public class RacingLobby
    {
        public uint LobbyId { get; set; }
        public List<SxVehicle> RacingVehicles { get; set; }
        public List<DbPlayer> RacingPlayers { get; set; }
        public uint RacingDimension { get; set; }

        public ColShape startPositionShape { get; set; }
        public ColShape checkpoint1Shape { get; set; }
        public ColShape checkpoint2Shape { get; set; }
        public ColShape checkpoint3Shape { get; set; }

        public RacingLobby(uint lobbyid, uint racingdimension)
        {
            LobbyId = lobbyid;
            RacingDimension = racingdimension;
            RacingVehicles = new List<SxVehicle>();
            RacingPlayers = new List<DbPlayer>();

            startPositionShape = Spawners.ColShapes.Create(RacingModule.StartFinishPosition_20, 20, racingdimension);
            startPositionShape.SetData("racingColshape", 1);

            checkpoint1Shape = Spawners.ColShapes.Create(RacingModule.Checkpoint1_10, 10, racingdimension);
            checkpoint1Shape.SetData("racingColshape", 2);

            checkpoint2Shape = Spawners.ColShapes.Create(RacingModule.Checkpoint2_10, 10, racingdimension);
            checkpoint2Shape.SetData("racingColshape", 3);

            checkpoint3Shape = Spawners.ColShapes.Create(RacingModule.Checkpoint3_10, 10, racingdimension);
            checkpoint3Shape.SetData("racingColshape", 4);

            checkpoint3Shape = Spawners.ColShapes.Create(RacingModule.Checkpoint4_10, 10, racingdimension);
            checkpoint3Shape.SetData("racingColshape", 5);

            checkpoint3Shape = Spawners.ColShapes.Create(RacingModule.Checkpoint5_10, 10, racingdimension);
            checkpoint3Shape.SetData("racingColshape", 6);

            checkpoint3Shape = Spawners.ColShapes.Create(RacingModule.Checkpoint6_10, 10, racingdimension);
            checkpoint3Shape.SetData("racingColshape", 7);

            checkpoint3Shape = Spawners.ColShapes.Create(RacingModule.Checkpoint7_18, 18, racingdimension);
            checkpoint3Shape.SetData("racingColshape", 8);

            checkpoint3Shape = Spawners.ColShapes.Create(RacingModule.Checkpoint8_10, 10, racingdimension);
            checkpoint3Shape.SetData("racingColshape", 9);

            checkpoint3Shape = Spawners.ColShapes.Create(RacingModule.Checkpoint9_10, 10, racingdimension);
            checkpoint3Shape.SetData("racingColshape", 10);
        }
    }

    public class RacingModule : Module<RacingModule>
    {
        public static bool RacingDeactivated = false;

        public static Vector3 StartFinishPosition_20 = new Vector3(-2196.8, 2958.41, 32.088);
        public static Vector3 Checkpoint1_10 = new Vector3(-2340.63, 3195.14, 32.1083);
        public static Vector3 Checkpoint2_10 = new Vector3(-2193.45, 3285.42, 32.0946);
        public static Vector3 Checkpoint3_10 = new Vector3(-2013.39, 3259.61, 32.0943);
        public static Vector3 Checkpoint4_10 = new Vector3(-1968.7, 3149.38, 32.0943);
        public static Vector3 Checkpoint5_10 = new Vector3(-1837.01, 3075.23, 32.1446);
        public static Vector3 Checkpoint6_10 = new Vector3(-1790.77, 3155.36, 32.1587);
        public static Vector3 Checkpoint7_18 = new Vector3(-2029.15, 3322.68, 32.2893);
        public static Vector3 Checkpoint8_10 = new Vector3(-1856.46, 3324.95, 32.2172);
        public static Vector3 Checkpoint9_10 = new Vector3(-1739.62, 3158.89, 32.2286);

        public static Vector3 RacingMenuPosition = new Vector3(-1968.7, 3149.38, 32.0943);
        
        public static float BoxVehHeading = 328.53f;

        public static Vector3 Box1Veh = new Vector3(-2140.28, 2880.91, 32.0861);
        public static Vector3 Box2Veh = new Vector3(-2131.61, 2875.65, 32.0861);
        public static Vector3 Box3Veh = new Vector3(-2122.95, 2870.54, 32.0864);
        public static Vector3 Box4Veh = new Vector3(-2114.06, 2865.62, 32.0862);
        public static Vector3 Box5Veh = new Vector3(-2105.82, 2860.29, 32.0861);
        public static Vector3 Box6Veh = new Vector3(-2097.23, 2854.6, 32.0863);
        
        public static List<RacingLobby> Lobbies = new List<RacingLobby>();
        public static int MaxLobbyPlayers = 10;
        public static int LobbyEnterPrice = 2500;

        public static uint RacingVehicleDataId = 1218;
        
        protected override bool OnLoad()
        {
            Lobbies = new List<RacingLobby>();

            Lobbies.Add(new RacingLobby(1, 200));
            Lobbies.Add(new RacingLobby(2, 201));
            Lobbies.Add(new RacingLobby(3, 202));
            Lobbies.Add(new RacingLobby(4, 203));
            Lobbies.Add(new RacingLobby(5, 204));
            Lobbies.Add(new RacingLobby(6, 205));
            Lobbies.Add(new RacingLobby(7, 206));

            MenuManager.Instance.AddBuilder(new RacingEnterMenuBuilder());

            return base.OnLoad();
        }
        
        public override void OnTenSecUpdate()
        {
            if (RacingDeactivated) return;
            foreach(RacingLobby racingLobby in Lobbies)
            {
                foreach(SxVehicle sxVehicle in racingLobby.RacingVehicles.ToList())
                {
                    if (sxVehicle == null || !sxVehicle.IsValid()) return;

                    if (sxVehicle != null && sxVehicle.entity != null && sxVehicle.entity.IsSeatFree(-1))
                    {
                        if (sxVehicle.entity.HasData("racingLeaveCheck"))
                        {
                            racingLobby.RacingVehicles.Remove(sxVehicle);
                            VehicleHandler.Instance.DeleteVehicle(sxVehicle, false);
                        }
                        else sxVehicle.entity.SetData("racingLeaveCheck", 1);
                    }
                }

                foreach(DbPlayer dbPlayer in racingLobby.RacingPlayers.ToList())
                {
                    if (dbPlayer == null || !dbPlayer.IsValid())
                    {
                        if (racingLobby.RacingPlayers.Contains(dbPlayer)) racingLobby.RacingPlayers.Remove(dbPlayer);
                        continue;
                    }
                    
                    if (dbPlayer.DimensionType[0] != DimensionType.RacingArea)
                    {
                        racingLobby.RacingPlayers.Remove(dbPlayer);
                        continue;
                    }
                    if(dbPlayer.DimensionType[0] == DimensionType.RacingArea && !dbPlayer.Player.IsInVehicle)
                    {
                        if(dbPlayer.HasData("racingLeaveCheck"))
                        {
                            dbPlayer.RemoveFromRacing();
                            dbPlayer.ResetData("racingLeaveCheck");
                        }
                        else dbPlayer.SetData("racingLeaveCheck", 1);
                        continue;
                    }
                }
            }
        }
        
        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (RacingDeactivated) return false;
            if (dbPlayer.Dimension[0] == 0 && key == Key.E)
            {
                if(dbPlayer.Player.Position.DistanceTo(RacingMenuPosition) < 2.0f)
                {
                    if(Crime.CrimeModule.Instance.CalcJailTime(dbPlayer.Crimes) > 0)
                    {
                        dbPlayer.SendNewNotification("Gesucht können Sie nicht an einem Rennen teilnehmen!");
                        return true;
                    }
                    Module.Menu.MenuManager.Instance.Build(GVRP.Module.Menu.PlayerMenu.RacingEnterMenu, dbPlayer).Show(dbPlayer);
                    return true;
                }
            }

            return false;
        }

        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape colShape, ColShapeState colShapeState)
        {
            if (RacingDeactivated) return false;
            if (!dbPlayer.Player.IsInVehicle) return false;
            if (dbPlayer.DimensionType[0] == DimensionType.RacingArea && colShape.HasData("racingColshape") && colShapeState == ColShapeState.Enter)
            {
                if(colShape.GetData("racingColshape") == 1) // Start - End Colshape
                {
                    if(dbPlayer.HasData("racingState") && dbPlayer.GetData("racingState") == 10 && dbPlayer.HasData("racingRoundStartTime")) // has state 4 and is at start shape
                    {
                        // Track Time
                        DateTime startTime = dbPlayer.GetData("racingRoundStartTime");
                        // get diff
                        int milsec = Convert.ToInt32(DateTime.Now.Subtract(startTime).Milliseconds);
                        int min = Convert.ToInt32(DateTime.Now.Subtract(startTime).Minutes);
                        int sec = Convert.ToInt32(DateTime.Now.Subtract(startTime).Seconds);
                        int totalmil = Convert.ToInt32(DateTime.Now.Subtract(startTime).TotalMilliseconds);
                        dbPlayer.SendNewNotification($"Rundenzeit: {min}:{sec} {milsec} ms!");

                        if(dbPlayer.RacingBestTimeSeconds == 0 || dbPlayer.RacingBestTimeSeconds > totalmil)
                        {
                            dbPlayer.SetBestTime(totalmil);
                            dbPlayer.SendNewNotification($"Glückwunsch, neue Bestzeit!");
                            dbPlayer.SendNewNotification($"1337Sexuakbar$racinbbesttime", duration: 16000);
                        }
                    }

                    dbPlayer.SetData("racingState", 1);
                    dbPlayer.SetData("racingRoundStartTime", DateTime.Now);
                    dbPlayer.SendNewNotification("Rundenzeit wird nun gemessen...");
                    SxVehicle sxVeh = dbPlayer.Player.Vehicle.GetVehicle();
                    if (sxVeh != null && sxVeh.IsValid())
                    {
                        sxVeh.Repair();
                        sxVeh.fuel = sxVeh.Data.Fuel;
                    }
                }
                else
                {
                    if(dbPlayer.HasData("racingState"))
                    {
                        if (colShape.GetData("racingColshape") - 1 != dbPlayer.GetData("racingState"))
                        {
                            dbPlayer.SetData("racingState", 1);
                        }
                        else dbPlayer.SetData("racingState", colShape.GetData("racingColshape"));
                    }
                    else dbPlayer.SetData("racingState", 1);
                }

            }

            return false;
        }
    }
}

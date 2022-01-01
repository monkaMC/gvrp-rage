using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GVRP.Handler;
using GVRP.Module.GTAN;
using GVRP.Module.Injury;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Buffs;
using GVRP.Module.Players.Db;
using GVRP.Module.Teamfight;

namespace GVRP.Module.Gangwar
{
    public sealed class GangwarModule : Module<GangwarModule>
    {
        public uint DefaultDimension = 9999;

        // Point Settings
        public int KillPoints = 3; // if A kills B, A gets points
        public int TimerFlagPoints = 1; // each ten sec if a player is in range without a enemy
        public Color StandardColor = new Color(255, 140, 0, 255);
        
        public int GangwarTimeLimit = 45;
        public int GangwarTownLimit = 3;

        public List<GangwarTown> ActiveGangwarTowns = new List<GangwarTown>();

        protected override bool OnLoad()
        {
            if (Configurations.Configuration.Instance.DevMode) GangwarTimeLimit = 5;
            return true;
        }

        public override bool OnColShapeEvent(DbPlayer dbPlayer, ColShape p_ColShape, ColShapeState p_ColShapeState)
        {
            if (!p_ColShape.TryData("gangwarId", out uint l_GangWarID)) return false;
            GangwarTown gangwarTown = GangwarTownModule.Instance.Get(l_GangWarID);

            switch (p_ColShapeState)
            {
                case ColShapeState.Enter:
                    dbPlayer.SetData("gangwarId", l_GangWarID);
                    gangwarTown.Visitors.Add(dbPlayer);

                    if (GangwarTownModule.Instance.IsTeamInGangwar(dbPlayer.Team))
                    {
                        var attackerTeam = gangwarTown.AttackerTeam;
                        var defenderTeam = gangwarTown.DefenderTeam;
                        if (attackerTeam == null || defenderTeam == null) return true;
                        var l_Limit = Instance.GangwarTimeLimit * 60;
                        var l_Subtraction = (int)DateTime.Now.Subtract(gangwarTown.LastAttacked).TotalSeconds;
                        dbPlayer.Player.TriggerEvent("initializeGangwar", attackerTeam.ShortName, defenderTeam.ShortName, attackerTeam.Id, defenderTeam.Id, l_Limit - l_Subtraction);
                        dbPlayer.Player.TriggerEvent("updateGangwarScore", gangwarTown.AttackerPoints, gangwarTown.DefenderPoints);
                    }
                    return true;

                case ColShapeState.Exit:
                    dbPlayer.ResetData("gangwarId");
                    gangwarTown.Visitors.Remove(dbPlayer);

                    if (GangwarTownModule.Instance.IsTeamInGangwar(dbPlayer.Team))
                    {
                        dbPlayer.Player.TriggerEvent("finishGangwar");
                    }
                    return true;
                default:
                    return true;
            }
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (dbPlayer.Player.IsInVehicle) return false;
            if (key != Key.E) return false;

            GangwarTown gangwarTown = GangwarTownModule.Instance.GetByPosition(dbPlayer.Player.Position);
            if (gangwarTown != null && dbPlayer.Team.IsGangsters())
            {
                MenuManager.Instance.Build(PlayerMenu.GangwarInfo, dbPlayer).Show(dbPlayer);
                return true;
            }
            return false;
        }

        public override void OnMinuteUpdate()
        {
            foreach (GangwarTown gangwarTown in ActiveGangwarTowns.ToList())
            {
                if (gangwarTown.LastAttacked.AddMinutes(GangwarModule.Instance.GangwarTimeLimit) < System.DateTime.Now)
                {
                    // over time limit
                    gangwarTown.Finish();
                }
            }
        }

        public void TenSecUpdateHandle(GangwarTown gangwarTown)
        {
            // check Flags...
            int attackersFlagOne = 0;
            int defendersFlagOne = 0;
            int attackersFlagTwo = 0;
            int defendersFlagTwo = 0;
            int attackersFlagThree = 0;
            int defendersFlagThree = 0;

            foreach (DbPlayer dbPlayer in gangwarTown.Visitors.ToList())
            {
                if (dbPlayer == null || !dbPlayer.IsValid()) return;

                if (dbPlayer.TeamId != gangwarTown.AttackerTeam.Id) continue;
                if (dbPlayer.isInjured()) continue;

                if (dbPlayer.Player.Position.DistanceTo(gangwarTown.Flag_1) < 10.0f) attackersFlagOne++;
                if (dbPlayer.Player.Position.DistanceTo(gangwarTown.Flag_2) < 10.0f) attackersFlagTwo++;
                if (dbPlayer.Player.Position.DistanceTo(gangwarTown.Flag_3) < 10.0f) attackersFlagThree++;
            }

            foreach (DbPlayer dbPlayer in gangwarTown.Visitors.ToList())
            {
                if (dbPlayer == null || !dbPlayer.IsValid()) return;

                if (dbPlayer.TeamId != gangwarTown.DefenderTeam.Id) continue;
                if (dbPlayer.isInjured()) continue;

                if (dbPlayer.Player.Position.DistanceTo(gangwarTown.Flag_1) < 10.0f) defendersFlagOne++;
                if (dbPlayer.Player.Position.DistanceTo(gangwarTown.Flag_2) < 10.0f) defendersFlagTwo++;
                if (dbPlayer.Player.Position.DistanceTo(gangwarTown.Flag_3) < 10.0f) defendersFlagThree++;
            }

            // Setting Points

            // Flag 1
            if (attackersFlagOne == 0 && defendersFlagOne > 0)
            {
                gangwarTown.IncreasePoints(GangwarModule.Instance.TimerFlagPoints, 0);
            }
            else if (defendersFlagOne == 0 && attackersFlagOne > 0)
            {
                gangwarTown.IncreasePoints(0, GangwarModule.Instance.TimerFlagPoints);

            }

            // Flag 2
            if (attackersFlagTwo == 0 && defendersFlagTwo > 0)
            {
                gangwarTown.IncreasePoints(GangwarModule.Instance.TimerFlagPoints, 0);

            }
            else if (defendersFlagTwo == 0 && attackersFlagTwo > 0)
            {
                gangwarTown.IncreasePoints(0, GangwarModule.Instance.TimerFlagPoints);

            }

            // Flag 3
            if (attackersFlagThree == 0 && defendersFlagThree > 0)
            {
                gangwarTown.IncreasePoints(GangwarModule.Instance.TimerFlagPoints, 0);

            }
            else if (defendersFlagThree == 0 && attackersFlagThree > 0)
            {
                gangwarTown.IncreasePoints(0, GangwarModule.Instance.TimerFlagPoints);

            }
                
            
        }

        public override void OnTenSecUpdate()
        {
            foreach (GangwarTown gangwarTown in ActiveGangwarTowns.ToList())
            {
                TenSecUpdateHandle(gangwarTown);
            }
        }
    }
}

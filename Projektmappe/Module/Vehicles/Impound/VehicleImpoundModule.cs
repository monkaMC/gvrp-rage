using System;
using System.Collections.Generic;
using System.Text;
using GTANetworkAPI;
using GVRP.Handler;
using GVRP.Module.ClientUI.Components;
using GVRP.Module.Commands;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Players.Windows;

namespace GVRP.Module.Vehicles.Impound
{
    public sealed class VehicleImpoundModule : Module<VehicleImpoundModule>
    {
        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (dbPlayer == null || !dbPlayer.IsValid()) return false;

            if (!dbPlayer.CanAccessMethod()) return false;
            if (dbPlayer.TeamId != (int) teams.TEAM_DPOS || !dbPlayer.IsInDuty()) return false;

            if (dbPlayer.Player.Position.DistanceTo(new Vector3(401.34, -1631.674, 29.29195)) < 10.0f ||
                (dbPlayer.Player.Position.DistanceTo(new Vector3(-276.7973, 6053.857, 31.51515)) < 10.0f ||
                 dbPlayer.Player.Position.DistanceTo(new Vector3(1725.538, 3714.75, 34.19733)) < 10.0f ||
                 dbPlayer.Player.Position.DistanceTo(new Vector3(533.1231, -179.434, 54.38259)) < 10.0f))
            {
                foreach (var Vehicle in VehicleHandler.Instance.GetAllVehicles())
                {
                    if (Vehicle == null || Vehicle.teamid == (int) teams.TEAM_DPOS) continue;
                    if (dbPlayer.Player.Position.DistanceTo(Vehicle.entity.Position) < 5.0f)
                    {
                        dbPlayer.Player.SetData("impound_vehicle", Vehicle);
                        ComponentManager.Get<TextInputBoxWindow>().Show()(dbPlayer, new TextInputBoxWindowObject() { Title = "Beschlagnahmungszeit", Callback = "SetVehicleImpoundTime", Message = "Gib die Zeit der Beschlagnahmung in Minuten ein." });
                        return true;
                    }
                }
            }
            return false;



        }
    }
}

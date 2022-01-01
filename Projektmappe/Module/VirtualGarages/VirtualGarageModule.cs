using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GTANetworkAPI;
using GVRP.Module.Vehicles;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Handler;
using GVRP.Module.Houses;

namespace GVRP.Module.VirtualGarages
{
    public class VirtualGarageModule : SqlModule<VirtualGarageModule, VirtualGarage, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `virtual_garages`;";
        }

        protected override void OnItemLoaded(VirtualGarage loadable)
        {
            if(loadable.ShowMarker)
            {
                Main.ServerBlips.Add(Spawners.Blips.Create(loadable.Position, loadable.Name, 50, 1.0f, true, 50, 255));
            }
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {

            // See if Enter Exist is on pos
            VirtualGarageEnter virtualGarageEnter = VirtualGarageEnterModule.Instance.GetAll().Values.ToList().Where(vge => vge.Position.DistanceTo(dbPlayer.Player.Position) < 5.0f && dbPlayer.Player.Dimension == vge.EnterDimension).FirstOrDefault();
            if(virtualGarageEnter != null)
            {
                // Team und nicht drin?! -> weg
                if (virtualGarageEnter.VirtualGarage.Teams.Count() > 0 && !virtualGarageEnter.VirtualGarage.Teams.Contains(dbPlayer.TeamId)) return false;

                // Haus und kein Besitzer/Mieter?! -> weg
                if (virtualGarageEnter.VirtualGarage.Houses.Count() > 0 
                    && !virtualGarageEnter.VirtualGarage.Houses.Contains(dbPlayer.ownHouse[0]) 
                    && (!dbPlayer.IsTenant() || !virtualGarageEnter.VirtualGarage.Houses.Contains(dbPlayer.GetTenant().HouseId))) return false;
                
                if (!dbPlayer.Player.IsInVehicle)
                {
                    dbPlayer.Player.Dimension = virtualGarageEnter.DestinationDimension;
                    
                    Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                    {
                        dbPlayer.SetData("lastPosition", dbPlayer.Player.Position);

                        dbPlayer.Player.TriggerEvent("freezePlayer", true);
                        dbPlayer.Player.SetPosition(virtualGarageEnter.Destination);
                        dbPlayer.Player.SetRotation(virtualGarageEnter.DestinationHeading);
                        await Task.Delay(1000);
                        dbPlayer.Player.SetPosition(virtualGarageEnter.Destination);
                        dbPlayer.Player.SetRotation(virtualGarageEnter.DestinationHeading);
                        await Task.Delay(1500);
                        dbPlayer.Player.TriggerEvent("freezePlayer", false);
                    }));
                    return true;
                }
                else
                {
                    var vehicle = dbPlayer.Player.Vehicle;
                    vehicle.Dimension = virtualGarageEnter.DestinationDimension;
                    foreach (var occupant in vehicle.Occupants)
                    {
                        DbPlayer ocuPlayer = occupant.GetPlayer();
                        if (ocuPlayer != null && ocuPlayer.IsValid())
                        {
                            ocuPlayer.SetData("lastPosition", ocuPlayer.Player.Position);
                            occupant.Dimension = virtualGarageEnter.EnterDimension;
                        }
                        occupant.Dimension = virtualGarageEnter.DestinationDimension;
                    }

                    Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                    {
                        dbPlayer.SetData("lastPosition", dbPlayer.Player.Position);

                        dbPlayer.Player.TriggerEvent("freezePlayer", true);
                        vehicle.Rotation = new Vector3(0, 0, virtualGarageEnter.DestinationHeading);
                        vehicle.Position = virtualGarageEnter.Destination;
                        await Task.Delay(1000);
                        vehicle.Rotation = new Vector3(0, 0, virtualGarageEnter.DestinationHeading);
                        vehicle.Position = virtualGarageEnter.Destination;
                        await Task.Delay(1500);
                        dbPlayer.Player.TriggerEvent("freezePlayer", false);
                        dbPlayer.Player.Dimension = virtualGarageEnter.DestinationDimension;

                        SxVehicle sxVehicle = vehicle.GetVehicle();
                        if(sxVehicle != null && sxVehicle.IsValid())
                        {
                            sxVehicle.ChangeVirtualGarageStatus(VirtualGarageStatus.IN_VGARAGE);
                        }

                    }));
                    return true;
                }
            }

            virtualGarageEnter = VirtualGarageEnterModule.Instance.GetAll().Values.ToList().Where(vge => vge.Destination.DistanceTo(dbPlayer.Player.Position) < 5.0f && dbPlayer.Player.Dimension == vge.DestinationDimension).FirstOrDefault();
            if (virtualGarageEnter != null)
            {
                // Team und nicht drin?! -> weg
                if (virtualGarageEnter.VirtualGarage.Teams.Count() > 0 && !virtualGarageEnter.VirtualGarage.Teams.Contains(dbPlayer.TeamId)) return false;

                // Haus und kein Besitzer/Mieter?! -> weg
                if (virtualGarageEnter.VirtualGarage.Houses.Count() > 0
                    && !virtualGarageEnter.VirtualGarage.Houses.Contains(dbPlayer.ownHouse[0])
                    && (!dbPlayer.IsTenant() || !virtualGarageEnter.VirtualGarage.Houses.Contains(dbPlayer.GetTenant().HouseId))) return false;
                
                if (!dbPlayer.Player.IsInVehicle)
                {
                    dbPlayer.Player.Dimension = virtualGarageEnter.EnterDimension;

                    Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                    {
                        dbPlayer.ResetData("lastPosition");
                        dbPlayer.Player.TriggerEvent("freezePlayer", true);
                        dbPlayer.Player.SetPosition(virtualGarageEnter.Position);
                        dbPlayer.Player.SetRotation(virtualGarageEnter.EnterHeading);
                        await Task.Delay(1000);
                        dbPlayer.Player.SetPosition(virtualGarageEnter.Position);
                        dbPlayer.Player.SetRotation(virtualGarageEnter.EnterHeading);
                        await Task.Delay(1500);
                        dbPlayer.Player.TriggerEvent("freezePlayer", false);
                    }));
                    return true;
                }
                else
                {
                    var vehicle = dbPlayer.Player.Vehicle;
                    vehicle.Dimension = virtualGarageEnter.EnterDimension;
                    foreach (var occupant in vehicle.Occupants)
                    {
                        DbPlayer ocuPlayer = occupant.GetPlayer();
                        if (ocuPlayer != null && ocuPlayer.IsValid())
                        {
                            ocuPlayer.ResetData("lastPosition");
                            occupant.Dimension = virtualGarageEnter.EnterDimension;
                        }
                    }

                    Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
                    {
                        dbPlayer.ResetData("lastPosition");
                        dbPlayer.Player.TriggerEvent("freezePlayer", true);
                        vehicle.Rotation = new Vector3(0, 0, virtualGarageEnter.EnterHeading);
                        vehicle.Position = virtualGarageEnter.Position;
                        await Task.Delay(1000);
                        vehicle.Rotation = new Vector3(0, 0, virtualGarageEnter.EnterHeading);
                        vehicle.Position = virtualGarageEnter.Position;
                        await Task.Delay(1500);
                        dbPlayer.Player.TriggerEvent("freezePlayer", false);
                        dbPlayer.Player.Dimension = virtualGarageEnter.EnterDimension;

                        SxVehicle sxVehicle = vehicle.GetVehicle();
                        if (sxVehicle != null && sxVehicle.IsValid())
                        {
                            sxVehicle.ChangeVirtualGarageStatus(VirtualGarageStatus.IN_WORLD);
                        }
                    }));
                    return true;
                }
            }
            return false;
        }
    }
}

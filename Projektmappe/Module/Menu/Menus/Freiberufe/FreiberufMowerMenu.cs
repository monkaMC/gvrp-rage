using GTANetworkAPI;
using GVRP.Handler;
using GVRP.Module.Freiberuf;
using GVRP.Module.Freiberuf.Mower;
using GVRP.Module.Houses;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Vehicles.Data;

namespace GVRP
{
    public class FreiberufMowerMenuBuilder : MenuBuilder
    {
        public FreiberufMowerMenuBuilder() : base(PlayerMenu.FreiberufMowerMenu)
        {
        }

        public override Menu Build(DbPlayer iPlayer)
        {
            var menu = new Menu(Menu, "Freiberuf Rasenarbeiten");
            menu.Add("Arbeit starten");
            menu.Add("Rückgabe");
            menu.Add(MSG.General.Close());
            return menu;
        }

        public override IMenuEventHandler GetEventHandler()
        {
            return new EventHandler();
        }

        private class EventHandler : IMenuEventHandler
        {
            public bool OnSelect(int index, DbPlayer iPlayer)
            {
                switch (index)
                {
                    case 0:
                        if (MowerModule.PlayersInJob.Contains(iPlayer))
                        {
                            iPlayer.SendNewNotification("Job wurde bereits gestartet!");
                            break;
                        }

                        iPlayer.RemoveJobVehicleIfExist(MowerModule.MowerJobVehMarkId);

                        if(!iPlayer.IsJobVehicleAtPoint(MowerModule.MowerGetPoint))
                        {
                            // Spawning Vehicle
                            SxVehicle xVeh = VehicleHandler.Instance.CreateServerVehicle(VehicleDataModule.Instance.GetData((uint)VehicleHash.Mower).Id, false,
                                MowerModule.MowerSpawnPoint, MowerModule.MowerSpawnRotation, Main.rndColor(),
                                Main.rndColor(), 0, true, true, false, 0, iPlayer.GetName(), 0, MowerModule.MowerJobVehMarkId, iPlayer.Id);
                            xVeh.entity.SetData("loadage", 0);
                            MowerModule.PlayersInJob.Add(iPlayer);
                            iPlayer.SendNewNotification("Ihr Fahrzeug steht bereit, maehen sie den Rasen!");
                        }
                        break;
                    case 1:
                        SxVehicle sxVehicle = iPlayer.GetJobVehicle(MowerModule.MowerJobVehMarkId);
                        if(sxVehicle != null)
                        {
                            int loadage = sxVehicle.entity.GetData("loadage");
                            int verdienst = loadage * 10;

                            VehicleHandler.Instance.DeleteVehicle(sxVehicle, false);
                            iPlayer.GiveMoney(verdienst);
                            iPlayer.SendNewNotification("Sie wurden fuer Ihre Arbeit mit 9$/kg Schnitt belohnt!");
                            iPlayer.SendNewNotification($"Verdienst: {verdienst}$");
                            MowerModule.PlayersInJob.Remove(iPlayer);
                        }
                        break;
                    default:
                        MenuManager.DismissCurrent(iPlayer);
                        break;
                }

                return true;
            }
        }
    }
}
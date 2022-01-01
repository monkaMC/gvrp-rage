using GTANetworkAPI;
using GVRP.Handler;
using GVRP.Module.Freiberuf;
using GVRP.Module.Freiberuf.Mower;
using GVRP.Module.Government;
using GVRP.Module.Houses;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Teams;
using GVRP.Module.Vehicles.Data;
using GVRP.Module.Vehicles.Garages;

namespace GVRP.Module.Gangwar
{
    public class GangwarVehicleMenu : MenuBuilder
    {
        public GangwarVehicleMenu() : base(PlayerMenu.GangwarVehicleMenu)
        {
        }

        public override Menu.Menu Build(DbPlayer iPlayer)
        {
            var menu = new Menu.Menu(Menu, "Gangwar Fahrzeuge");

            menu.Add(MSG.General.Close());
            menu.Add("Revolter");
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
                uint model;
                Garage garage = GarageModule.Instance[iPlayer.GetData("garageId")];
                GangwarTown gangwarTown = GangwarTownModule.Instance.FindActiveByTeam(iPlayer.Team);

                switch (index)
                {
                    case 0: return true;
                    case 1: model = 8976; break;
                    //case 2: model = 40; break;
                    default: return true;
                }

                var spawn = garage.GetFreeSpawnPosition();
                if (spawn == null)
                {
                    iPlayer.SendNewNotification("Kein freier Ausparkpunkt.");
                    return false;
                }
                else
                {
                    var vehicle = VehicleHandler.Instance.CreateServerVehicle(model, true, spawn.Position, spawn.Heading, iPlayer.Team.ColorId, iPlayer.Team.ColorId,
                        GangwarModule.Instance.DefaultDimension, true, teamid: iPlayer.TeamId, plate: iPlayer.Team.ShortName);
                    gangwarTown.Vehicles.Add(vehicle);
                }

                return true;
            }
        }
    }
}
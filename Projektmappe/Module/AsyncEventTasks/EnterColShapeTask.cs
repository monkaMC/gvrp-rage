using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using GVRP.Module.Armory;
using GVRP.Module.Banks;
using GVRP.Module.Houses;
using GVRP.Module.Logging;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Teams;
using GVRP.Module.Vehicles.Garages;

namespace GVRP.Module.AsyncEventTasks
{
    public static partial class AsyncEventTasks
    {
        public static void EnterColShapeTask(ColShape shape, Client player)
        {
            DbPlayer iPlayer = player.GetPlayer();
            if (iPlayer == null || !iPlayer.IsValid()) return;

            if (shape.HasData("notificationId"))
            {
                var notification =
                    PlayerNotifications.Instance.GetById((int)shape.GetData("notificationId"));
                iPlayer.SendNotification(notification);
            }

            if (Modules.Instance.OnColShapeEvent(iPlayer, shape, ColShapeState.Enter)) return;

            if (shape.HasData("clothShopId"))
            {
                iPlayer.SetData("clothShopId", (uint)shape.GetData("clothShopId"));
                iPlayer.SendNewNotification("Benutze E um Kleidung zu kaufen!", title: "Kleidungsstore");
                return;
            }

            if (shape.HasData("teamWardrobe"))
            {
                HashSet<int> teams = shape.GetData("teamWardrobe");
                if (!teams.Contains((int)iPlayer.TeamId)) return;
                iPlayer.SetData("teamWardrobe", teams);
                iPlayer.SendNewNotification("Benutze E um Kleidung zu kaufen!", title: "Fraktionskleiderschrank");
                return;
            }

            if (shape.HasData("ammunationId"))
            {
                iPlayer.SendNewNotification("Benutze E um Waffen zu kaufen!", title: "Ammunation");

                int ammunationId = shape.GetData("ammunationId");
                iPlayer.SetData("ammunationId", ammunationId);
                return;
            }

            if (shape.HasData("name_change"))
            {
                iPlayer.SetData("name_change", true);
                iPlayer.SendNewNotification(title:"Namensänderung beantragen", text:"Drücke E um eine Namensänderung zu beantragen.");
                return;
            }


            if (shape.HasData("garageId"))
            {
                try
                {
                    uint garageId = shape.GetData("garageId");
                    if (!GarageModule.Instance.Contains(garageId)) return;
                    var garage = GarageModule.Instance[garageId];
                    if (garage == null) return;
                    var labelText = "";

                    if (garage.PublicTeamRestriction > 0 && iPlayer.TeamId != garage.PublicTeamRestriction)
                    {
                        return;
                    }

                    if (!garage.DisableInfos)
                    {

                        if (garage.IsTeamGarage())
                        {
                            labelText = labelText + " (Fraktionsgarage)";
                        }
                        else if (garage.Type == GarageType.VehicleCollection)
                        {
                            labelText = "Fahrzeug fuer 2500$ Kaution freikaufen";
                        }
                        else if (garage.Type == GarageType.VehicleAdminGarage)
                        {
                            labelText = "Fahrzeug fuer 5000$ Kaution freikaufen";
                        }
                        else
                        {
                            labelText = "Benutze E um die Garage zu öffnen!";
                        }
                    }
                    iPlayer.SetData("garageId", garageId);
                    return;
                }
                catch(Exception e) {
                    Logger.Crash(e);
                }
            }
            
            if (shape.HasData("bankId"))
            {
                var bankId = shape.GetData("bankId");
                if (bankId == null)
                {
                    return;
                }

                var parseBankId = uint.TryParse(bankId.ToString(), out uint bankIdNew);
                if (!parseBankId)
                {
                    return;
                }

                var bank = BankModule.Instance.Get(bankIdNew);
                iPlayer.SendNewNotification("Benutze E um auf dein Konto zuzugreifen!", title: bank.Name);
                iPlayer.SetData("bankId", bankId);
                return;
            }

            if (shape.HasData("ArmoryId"))
            {
                int ArmoryId = shape.GetData("ArmoryId");
                var Armory = ArmoryModule.Instance.Get(ArmoryId);
                if (Armory == null) return;
                iPlayer.SendNewNotification("Benutze E zum interagieren!", title: "Waffenkammer");
                iPlayer.SetData("ArmoryId", ArmoryId);
                return;
            }
        }
    }
}

using GTANetworkAPI;
using System;
using System.Threading.Tasks;
using GVRP.Handler;
using GVRP.Module.Attachments;
using GVRP.Module.Chat;
using GVRP.Module.Freiberuf;
using GVRP.Module.Items;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Teams;
using GVRP.Module.Vehicles.Data;

namespace GVRP.Module.Schwarzgeld
{
    public class ExchangeModule : Module<ExchangeModule>
    {
        public int ExchangeJobVehMarkId = 25;
        public int ExchangeValue = 50000;
        public int MaxExchangeVehicleAmount = 500000;
        public float ExchangeFactor = 0.9f;
        public int DailyLimit = 10000000;
        public int BestechenCost = 50000;

        // Daily reset
        public override void OnDailyReset()
        {
            MySQLHandler.ExecuteAsync($"UPDATE exchange_locations SET `exchanged_amount` = '0'");
            return;
        }

        // Start exchange function
        public void StartExchange(DbPlayer dbPlayer)
        {
            if (dbPlayer == null || dbPlayer.Player.IsInVehicle) return;

            // Get team exchange object
            ExchangeLocation Exchange = ExchangeLocationModule.Instance.getTeamExchangeLocation(dbPlayer.TeamId);

            // Check daily amount
            if (Exchange.ExchangedAmount == DailyLimit)
            {
                dbPlayer.SendNewNotification("Dein Team hat bereits die tägliche Menge umgewandelt. Komm morgen wieder!");
                return;
            }

            // Already got item added
            if (dbPlayer.Container.GetItemAmount(660) > 0)
            {
                dbPlayer.SendNewNotification("Bitte gebe zuerst die Rechnungen ab!");
                return;
            }

            // Check if user got already a object assigned
            if (dbPlayer.Attachments.ContainsValue((int)Attachment.BOX))
            {
                dbPlayer.SendNewNotification("Belade den Lieferwagen zuerst mit der Ware welche du trägst!");
                return;
            }

            // Check if user got enough schwarzgeld in inventory
            if (dbPlayer.Container.GetItemAmount(SchwarzgeldModule.SchwarzgeldId) < ExchangeValue)
            {
                dbPlayer.SendNewNotification("Du hast nicht genügend Schwarzgeld dabei!");
                return;
            }

            // Alert cops
            if (alertCops(Exchange.Bestochen))
            {
                // Update team exchange alert bool
                ExchangeLocationModule.Instance.UpdateTeamExchangeAlert(dbPlayer.TeamId);

                // Alert cops
                TeamModule.Instance.Get((int)teams.TEAM_FIB).SendNotification("Ein Tipp zu bezüglich Geldwäsche ist eingegangen! Nutze /findhint um den Tipp zu orten.");
            }

            // Get vehicle check
            SxVehicle ExchangeVehicle = dbPlayer.GetJobVehicle(ExchangeJobVehMarkId);

            if (ExchangeVehicle == null)
            {
                // Check if a vehicle is blocking the spawn point
                if (!dbPlayer.IsJobVehicleAtPoint(Exchange.VehicleSpawn))
                {
                    // Create vehicle
                    ExchangeVehicle = VehicleHandler.Instance.CreateServerVehicle(VehicleDataModule.Instance.GetData((uint)VehicleHash.Burrito4).Id, false, Exchange.VehicleSpawn, Exchange.VehicleRotation, dbPlayer.Team.ColorId, dbPlayer.Team.ColorId, 0, true, true, false, 0, dbPlayer.GetName(), 0, ExchangeJobVehMarkId, dbPlayer.Id);

                    // Set vehicle data
                    ExchangeVehicle.entity.SetData("schwarzgeldLoadout", 0);
                }
                else
                {
                    // Notify user a vehicle is blocking spawn point
                    dbPlayer.SendNewNotification("Ein Fahrzeug blockiert die Ausfahrt!");
                    return;
                }
            }

            // Get values
            int currentVehicleLoadage = FreiberufFunctions.GetJobVehicle(dbPlayer, ExchangeJobVehMarkId).entity.GetData("schwarzgeldLoadout");

            // Check if vehicle can store schwarzgeld
            if (currentVehicleLoadage + ExchangeValue > MaxExchangeVehicleAmount)
            {
                dbPlayer.SendNewNotification("Das Fahrzeug ist voll beladen, gehe die Ware zuerst zerstören!");
                return;
            }

            // Add player data and remove schwarzgeld from inventory
            dbPlayer.SetData("schwarzgeld", ExchangeValue);

            if (!dbPlayer.TakeBlackMoney(ExchangeValue))
            {
                dbPlayer.SendNewNotification($"Schwarzgeld konnte nicht entnommen werden!");
                return;
            }

            // Add object to player
            AttachmentModule.Instance.AddAttachment(dbPlayer, Attachment.BOX);
            dbPlayer.Player.TriggerEvent("courierSetCarrying", true);

            // Update data
            ExchangeLocationModule.Instance.UpdateExchangeValue(dbPlayer.TeamId);

            // Notify user
            dbPlayer.SendNewNotification($"Du hast Waren im Wert von {ExchangeValue}$ erhalten, lade diese in deinen Lieferwagen um sie zu entsorgen!");
        }

        public void LoadVehicle(DbPlayer dbPlayer)
        {
            if (dbPlayer == null || dbPlayer.Player.IsInVehicle) return;

            // Check if user got no item assigned
            if (!dbPlayer.Attachments.ContainsValue((int)Attachment.BOX))
            {
                dbPlayer.SendNewNotification("Du hast keine Ware bei dir welche du in den Wagen laden könntest!");
                return;
            }

            // Get Exchange data
            ExchangeLocation Exchange = ExchangeLocationModule.Instance.getTeamExchangeLocation(dbPlayer.TeamId);

            // Get exchange vehicle
            SxVehicle JobVehicle = FreiberufFunctions.GetJobVehicle(dbPlayer, ExchangeJobVehMarkId);

            // Get new amount and reset user data
            int schwarzgeld = JobVehicle.entity.GetData("schwarzgeldLoadout") + dbPlayer.GetData("schwarzgeld");
            dbPlayer.ResetData("schwarzgeld");

            // Add amount to vehicle
            JobVehicle.entity.SetData("schwarzgeldLoadout", schwarzgeld);

            // Remove attachments
            AttachmentModule.Instance.RemoveAttachment(dbPlayer, Attachment.BOX);
            dbPlayer.Player.TriggerEvent("courierSetCarrying", false);

            // Create marker
            dbPlayer.Player.TriggerEvent("createPlayerMarker", Exchange.ExchangeDestroyLocation);

            // Set gps marker
            dbPlayer.Player.TriggerEvent("setPlayerGpsMarker", Exchange.ExchangeDestroyLocation.X, Exchange.ExchangeDestroyLocation.Y);

            // Inform user
            dbPlayer.SendNewNotification($"Das Fahrzeug wurde mit Ware beladen: {JobVehicle.entity.GetData("schwarzgeldLoadout")}/{MaxExchangeVehicleAmount}");
        }

        public void VehicleTakeOut(DbPlayer dbPlayer)
        {
            if (dbPlayer == null || dbPlayer.Player.IsInVehicle) return;

            // Get exchange vehicle
            SxVehicle JobVehicle = FreiberufFunctions.GetJobVehicle(dbPlayer, ExchangeJobVehMarkId);

            // Check if vehicle got stuff loaded
            if (JobVehicle.entity.GetData("schwarzgeldLoadout") < ExchangeValue)
            {
                dbPlayer.SendNewNotification("Das Fahrzeug enthält keine weiteren Waren!");
                return;
            }

            // Check if user got already a object assigned
            if (dbPlayer.Attachments.ContainsValue((int)Attachment.BOX))
            {
                dbPlayer.SendNewNotification("Du kannst nicht mehr Ware auf einmal tragen!");
                return;
            }

            // Take stuff out of vehicle
            int newLoadout = JobVehicle.entity.GetData("schwarzgeldLoadout") - ExchangeValue;
            JobVehicle.entity.SetData("schwarzgeldLoadout", newLoadout);

            // Add object to player
            AttachmentModule.Instance.AddAttachment(dbPlayer, Attachment.BOX);
            dbPlayer.Player.TriggerEvent("courierSetCarrying", true);

            // Inform user
            dbPlayer.SendNewNotification($"Du hast Waren aus dem Fahrzeug entnommen ({newLoadout / ExchangeValue} verbleibend). Gehe das Paket nun vernichten!");
        }

        public void DestroyExchange(DbPlayer dbPlayer)
        {
            if (dbPlayer == null || dbPlayer.Player.IsInVehicle) return;

            // Check if object is attached
            if (!dbPlayer.Attachments.ContainsValue((int)Attachment.BOX))
            {
                dbPlayer.SendNewNotification("Du hast keine Ware dabei!");
                return;
            }

            // Check if inventory item can be added
            if (!dbPlayer.Container.CanInventoryItemAdded(660))
            {
                dbPlayer.SendNewNotification("Du hast nicht genuegend Platz um die Rechnung entgegen zu nemen!");
                return;
            }

            // Destroy assigned object
            AttachmentModule.Instance.RemoveAttachment(dbPlayer, Attachment.BOX);

            // Add rechnung to inventory
            dbPlayer.Container.AddItem(660, 1);

            // Notify user
            dbPlayer.SendNewNotification("Die Ware wird nun zerstört, übergebe die Rechnung danach an den Ladenbesitzer!");

            // Animation and loading bar
            Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
            {
                // Play animation
                dbPlayer.Player.TriggerEvent("freezePlayer", true);
                dbPlayer.SetData("userCannotInterrupt", true);
                dbPlayer.PlayAnimation((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "anim@mp_snowball", "pickup_snowball");

                // Progressbar
                Chats.sendProgressBar(dbPlayer, ExchangeValue / 2);
                await Task.Delay(ExchangeValue / 2);

                // Stop animation
                dbPlayer.Player.TriggerEvent("freezePlayer", false);
                dbPlayer.Player.StopAnimation();
                dbPlayer.ResetData("userCannotInterrupt");
            }));
        }

        public void FinishExchange(DbPlayer dbPlayer)
        {
            if (dbPlayer == null || dbPlayer.Player.IsInVehicle) return;

            // Get Exchange data
            ExchangeLocation Exchange = ExchangeLocationModule.Instance.getTeamExchangeLocation(dbPlayer.TeamId);

            // Take item from player inventory (beweis)
            if (dbPlayer.Container.GetItemAmount(660) == 0)
            {
                dbPlayer.SendNewNotification("Du hast keine Rechnung dabei!");
                return;
            }

            // Check if user still got the exchange vehicle
            SxVehicle ExchangeVehicle = dbPlayer.GetJobVehicle(ExchangeJobVehMarkId);

            // Remove vehicle and marker
            if (ExchangeVehicle != null)
            {
                if (ExchangeVehicle.entity.GetData("schwarzgeldLoadout") != 0)
                {
                    // Notify user that the vehicle isnt empty
                    dbPlayer.SendNewNotification("Der Lieferwagen enthält weitere Waren. Gehe diese zerstören!");
                    return;
                }

                VehicleHandler.Instance.DeleteVehicle(ExchangeVehicle, false);
                dbPlayer.Player.TriggerEvent("destroyPlayerMarker");
            }

            // Get amount rechnungen
            int amount = dbPlayer.Container.GetItemAmount(660);
            int newAmount = ExchangeValue * amount;

            // Remove Rechnung
            dbPlayer.Container.RemoveItem(660, amount);

            // Calculate money to add
            int exchangeMoney = (int)Math.Round(newAmount * ExchangeFactor);

            // Add money
            dbPlayer.GiveMoney(exchangeMoney);

            // Inform user
            dbPlayer.SendNewNotification($"Die Rechnung in höhe von: {exchangeMoney}$ wurde ausgezahlt!");
        }

        public void Bestechen(DbPlayer dbPlayer)
        {
            if (dbPlayer == null || dbPlayer.Player.IsInVehicle) return;

            // Get Exchange data
            ExchangeLocation Exchange = ExchangeLocationModule.Instance.getTeamExchangeLocation(dbPlayer.TeamId);

            // Check if bestochen is already set
            if (Exchange.Bestochen)
            {
                dbPlayer.SendNewNotification("Der Ladenbesitzer wurde bereits bestochen!");
                return;
            }

            if (dbPlayer.money[0] <= BestechenCost)
            {
                dbPlayer.SendNewNotification("Du hast nicht genügend Geld dabei um den Ladenbesitzer zu bestechen!");
                return;
            }

            // Take money and inform user
            dbPlayer.TakeMoney(BestechenCost);
            dbPlayer.SendNewNotification($"Du hast {BestechenCost}$ an den Ladenbesitzer übergeben um ihn zu bestechen!");

            // Update status
            ExchangeLocationModule.Instance.UpdateBestochen(dbPlayer.TeamId);
        }

        // Alert cops chance
        public bool alertCops(bool bestochen)
        {
            Random random = new Random();

            int chance = bestochen ? 5 : 15;

            if (random.Next(0, 100) <= chance)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (dbPlayer.Player.IsInVehicle) return false;
            if (!dbPlayer.IsAGangster()) return false;
            if (key != Key.E) return false;

            SxVehicle ExchangeVehicle = dbPlayer.GetJobVehicle(ExchangeJobVehMarkId);

            if (ExchangeVehicle != null && dbPlayer.Player.Position.DistanceTo(ExchangeVehicle.entity.Position) <= 3f)
            {
                MenuManager.Instance.Build(PlayerMenu.ExchangeVehicleMenu, dbPlayer).Show(dbPlayer);
                return true;
            }
            else if (ExchangeLocationModule.Instance.getExchangeDestroyLocationByPosition(dbPlayer) != null)
            {
                DestroyExchange(dbPlayer);
                return true;
            }

            return false;
        }
    }
}

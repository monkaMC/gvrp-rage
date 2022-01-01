using GTANetworkAPI;
using System;
using System.Linq;
using System.Threading.Tasks;
using GVRP.Handler;
using GVRP.Module.Attachments;
using GVRP.Module.Chat;
using GVRP.Module.Customization;
using GVRP.Module.Houses;
using GVRP.Module.Menu;
using GVRP.Module.NpcSpawner;
using GVRP.Module.Outfits;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Vehicles;
using GVRP.Module.Vehicles.Data;

namespace GVRP.Module.Freiberuf.Garbage
{
    public class GarbageJobModule : Module<GarbageJobModule>
    {
        public int GarbageJobVehMarkId = 21;
        public Vector3 GarbageNpc = new Vector3(-321.972, -1545.62, 31.0199);
        public float GarbageNpcHeading = 355.979f;
        public Vector3 VehicleSpawn = new Vector3(-317.022, -1539.3, 27.3804);
        public float VehicleSpawnRotation = 343.406f;
        public float VehicleLoadageLimit = 1000.0f;
        public Vector3 GarbageEmptyPoint = new Vector3(-350.78, -1557.68, 25.2201);
        public float GarbageEmptyNpcHeading = 179.39f;
        public int Reward = 10;

        public override bool Load(bool reload = false)
        {
            // Spawn npc
            new Npc(PedHash.GarbageSMY, GarbageNpc, GarbageNpcHeading, 0);
            new Npc(PedHash.GarbageSMY, GarbageEmptyPoint, GarbageEmptyNpcHeading, 0);

            // Create Menu
            MenuManager.Instance.AddBuilder(new FreiberufGarbageMenuBuilder());

            // Display notification
            PlayerNotifications.Instance.Add(GarbageNpc, "Freiberuf Müllabfuhr", "Deaktiviert!");
            return true;
        }

        // Start garbage job
        public void StartGarbageJob(DbPlayer dbPlayer)
        {
            // Set job data
            dbPlayer.SetData("jobStarted", true);

            OutfitsModule.Instance.SetPlayerOutfit(dbPlayer, 65);

            // Notify user
            dbPlayer.SendNewNotification("Ihr Fahrzeug steht bereit, beginnen sie mit der Arbeit!", PlayerNotification.NotificationType.FREIBERUF, "Freiberuf");
        }

        // Start garbage job
        public void RentGarbageVeh(DbPlayer dbPlayer)
        {
            if(!dbPlayer.HasData("jobStarted"))
            {
                dbPlayer.SendNewNotification("Sie müssen den Job zuerst starten!");
                return;
            }

            // Remove vehicle if exists
            dbPlayer.RemoveJobVehicleIfExist(GarbageJobVehMarkId);

            // Check if a vehicle is blocking spawn point
            if (!dbPlayer.IsJobVehicleAtPoint(VehicleSpawn))
            {
                // Spawn Vehicle and set vehicle data
                SxVehicle xVeh = VehicleHandler.Instance.CreateServerVehicle(VehicleDataModule.Instance.GetData((uint)VehicleHash.Trash).Id, false, VehicleSpawn, VehicleSpawnRotation, 58, 58, 0, true, true, false, 0, dbPlayer.GetName(), 0, GarbageJobVehMarkId, dbPlayer.Id);
                xVeh.entity.SetData("loadage", 0.0f);

                // Notify user
                dbPlayer.SendNewNotification("Ihr Fahrzeug steht bereit, beginnen sie mit der Arbeit!", PlayerNotification.NotificationType.FREIBERUF, "Freiberuf");
            }
        }

        // Maybe add multiplicator ? Example: Player drives to paletto should he get the same amount?
        public void loadTrashIntoVehicle(DbPlayer dbPlayer)
        {
            if (dbPlayer == null || dbPlayer.Player.IsInVehicle || !dbPlayer.HasData("jobStarted") || !dbPlayer.HasData("trash_amount") || dbPlayer.GetData("trash_amount") == 0.0f) return;



            // Get job vehicle
            SxVehicle JobVehicle = FreiberufFunctions.GetNearestJobVehicle(dbPlayer, GarbageJobVehMarkId, 6.0f);

            if(JobVehicle != null && JobVehicle.IsValid())
            {
                // Check if vehicle got loadage data
                if (!JobVehicle.entity.HasData("loadage")) return;

                // Get new amount and reset user data
                float newLoadage = (float)JobVehicle.entity.GetData("loadage") + (float)dbPlayer.GetData("trash_amount");
                dbPlayer.ResetData("trash_amount");

                // Add amount to vehicle
                JobVehicle.entity.SetData("loadage", newLoadage);

                // Remove attachments
                AttachmentModule.Instance.RemoveAttachment(dbPlayer, Attachment.TRASH);
                dbPlayer.Player.TriggerEvent("courierSetCarrying", false);

                // Inform user
                dbPlayer.SendNewNotification($"Neuer Müllbestand im Fahrzeug: {JobVehicle.entity.GetData("loadage")}");
            }
        }

        public void PickupTrash(DbPlayer dbPlayer, House house)
        {
            if (dbPlayer == null || dbPlayer.Player.IsInVehicle || !dbPlayer.HasData("jobStarted")) return;

            SxVehicle jobVeh = FreiberufFunctions.GetNearestJobVehicle(dbPlayer, GarbageJobVehMarkId, 20.0f);

            if (jobVeh == null || !jobVeh.IsValid()) return;

            // Check current vehicle loadage
            float currentVehicleLoadage = (float)jobVeh.entity.GetData("loadage");

            // Check if vehicle can store trash
            if (currentVehicleLoadage >= VehicleLoadageLimit)
            {
                dbPlayer.Player.TriggerEvent("setPlayerGpsMarker", GarbageEmptyPoint.X, GarbageEmptyPoint.Y);
                dbPlayer.SendNewNotification("Der Müllwagen ist zu voll, entleere diesen zuerst. Die GPS Koordinaten wurden eingetragen!", PlayerNotification.NotificationType.FREIBERUF, "Freiberuf");
                return;
            }

            // Check if user is already carying a bag
            if (dbPlayer.Attachments.ContainsValue((int)Attachment.TRASH))
            {
                dbPlayer.SendNewNotification("Du kannst nur einen Müllsack tragen!", PlayerNotification.NotificationType.FREIBERUF, "Freiberuf");
                return;
            }

            // debug notify
            dbPlayer.SendNewNotification($"Müll stand: {house.TrashAmount}");

            // Check trash amount
            if (house.TrashAmount > 0.0f)
            {
                // Attach object and add data to player data
                AttachmentModule.Instance.AddAttachment(dbPlayer, Attachment.TRASH);
                dbPlayer.SetData("trash_amount", house.TrashAmount);

                // Empty trash stand
                house.TrashAmount = 0.0f;
                house.SaveTrash();

                // Notify user
                dbPlayer.SendNewNotification("Du hast einen Müllsack aufgehoben, entlade diesen in deinen Müllwagen!", PlayerNotification.NotificationType.FREIBERUF, "Freiberuf");
                return;
            }
            else
            {
                // Inform user trash is empty
                dbPlayer.SendNewNotification("Diese Mülltonne ist leer!", PlayerNotification.NotificationType.FREIBERUF, "Freiberuf");
                return;
            }
        }

        // Empty Garbage vehicle
        public void EmptyGarbageVehicle(DbPlayer dbPlayer)
        {
            if (dbPlayer == null || !dbPlayer.HasData("jobStarted") || dbPlayer.HasData("empty_progress")) return;

            // Check if user is inside vehicle
            if (dbPlayer.Player.IsInVehicle)
            {
                dbPlayer.SendNewNotification("Um den Müllwagen zu entleeren darfst du nicht in einem Fahrzeug sitzen!", PlayerNotification.NotificationType.FREIBERUF, "Freiberuf");
                return;
            }

            // Check vehicle distance
            if (dbPlayer.Player.Position.DistanceTo(FreiberufFunctions.GetJobVehicle(dbPlayer, GarbageJobVehMarkId).entity.Position) >= 10.0f)
            {
                dbPlayer.SendNewNotification("Der Müllwagen ist zu weit entfernt!", PlayerNotification.NotificationType.FREIBERUF, "Freiberuf");
                return;
            }

            // Get vehicle trash amount
            float currentVehicleLoadage = FreiberufFunctions.GetJobVehicle(dbPlayer, GarbageJobVehMarkId).entity.GetData("loadage");

            // Check if vehicle got trash loaded
            if (currentVehicleLoadage == 0.0f)
            {
                dbPlayer.SendNewNotification("Der Müllwagen ist leer!", PlayerNotification.NotificationType.FREIBERUF, "Freiberuf");
                return;
            }


            dbPlayer.SetData("empty_progress", true);

            // Get time
            int time = ((int)Math.Round(currentVehicleLoadage, 0) / 50) * 1000;

            // add timer
            Main.m_AsyncThread.AddToAsyncThread(new Task(async () =>
            {
                dbPlayer.Player.TriggerEvent("freezePlayer", true);
                dbPlayer.SetData("userCannotInterrupt", true);

                // Show progress bar
                Chats.sendProgressBar(dbPlayer, time);
                await Task.Delay(time);

                dbPlayer.Player.TriggerEvent("freezePlayer", false);
                dbPlayer.ResetData("userCannotInterrupt");

                // Reset vehicle trash amount
                FreiberufFunctions.GetJobVehicle(dbPlayer, GarbageJobVehMarkId).entity.SetData("loadage", 0.0f);

                // Get reward
                int money = (int) Math.Round(currentVehicleLoadage, 0) * Reward;

                // Add money
                dbPlayer.GiveMoney(money);

                // Inform user
                dbPlayer.SendNewNotification($"Der Müllwagen wurde entleert. Du hast {money}$ für deine Arbeit erhalten. Bringe den Wagen nun zurück oder beginn eine weitere Tour!", PlayerNotification.NotificationType.FREIBERUF, "Freiberuf");
                dbPlayer.ResetData("empty_progress");
            }));


        }

        // Finish garbage job
        public void FinishGarbageJob(DbPlayer dbPlayer)
        {
            if (dbPlayer == null || !dbPlayer.HasData("jobStarted")) return;

            SxVehicle jobVehicle = FreiberufFunctions.GetJobVehicle(dbPlayer, GarbageJobVehMarkId);

            if(jobVehicle != null && jobVehicle.IsValid())
            {
                // Check if vehicle still got trash inside
                float currentVehicleLoadage = FreiberufFunctions.GetJobVehicle(dbPlayer, GarbageJobVehMarkId).entity.GetData("loadage");

                if (currentVehicleLoadage > 0.0f)
                {
                    dbPlayer.SendNewNotification("Bitte gehe den Müllwagen zuerst entleeren!", PlayerNotification.NotificationType.FREIBERUF, "Freiberuf");
                    return;
                }

                VehicleHandler.Instance.DeleteVehicle(jobVehicle, false);
            }

            // Reset user data
            dbPlayer.ResetData("jobStarted");

            dbPlayer.ApplyCharacter();

            // Notify user
            dbPlayer.SendNewNotification("Du hast den Freiberuf beendet!", PlayerNotification.NotificationType.FREIBERUF, "Freiberuf");
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key != Key.E) return false;


          
            return false;
        }
        
        public House GetTrashHouseObjectFromPosition(Vector3 position)
        {
            return HouseModule.Instance.GetAll().FirstOrDefault(house => house.Value.Position.DistanceTo(position) <= 2.0f).Value;
        }
    }
}

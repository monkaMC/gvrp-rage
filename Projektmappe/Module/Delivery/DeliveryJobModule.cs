using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GTANetworkAPI;
using GTANetworkMethods;
using GVRP.Handler;
using GVRP.Module.Attachments;
using GVRP.Module.Dealer;
using GVRP.Module.Delivery.Menu;
using GVRP.Module.Freiberuf.Mower;
using GVRP.Module.Houses;
using GVRP.Module.Logging;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;
using GVRP.Module.Vehicles.Data;

namespace GVRP.Module.Delivery
{
    public class DeliveryJobModule : SqlModule<DeliveryJobModule, DeliveryJob, uint>
    {   
        public static int DeliverJobVehMarkId = 22;
        public Dictionary<DbPlayer, DeliveryOrder> DeliveryOrders = new Dictionary<DbPlayer, DeliveryOrder>();


        protected override string GetQuery()
        {
            return "SELECT * FROM `delivery_jobs`;";
        }

        public override Type[] RequiredModules()
        {
            return new[] { typeof(DeliveryJobSpawnpointModule) };
        }

        protected override bool OnLoad()
        {
      //      MenuManager.Instance.AddBuilder(new DeliveryJobMenu());
            return base.OnLoad();
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (dbPlayer.Player.IsInVehicle) return false;
            if (key != Key.E) return false;

            if (!dbPlayer.Rank.CanAccessFeature("deliveryjob")) return false;


            if (DeliveryOrders.Count > 0 && DeliveryOrders.ContainsKey(dbPlayer))
            {
                DeliveryOrder deliveryOrder = DeliveryOrders.GetValueOrDefault(dbPlayer);

                int neededPackages = deliveryOrder.DeliveryPositions.Count;

                DeliveryJob deliveryJob = DeliveryJobModule.Instance.Get(dbPlayer.GetData("delivery_job_id"));

                if (deliveryJob.Position.DistanceTo(dbPlayer.Player.Position) < 2.0f)
                {
                    dbPlayer.SetData("delivery_job_id", deliveryJob.Id);
                    MenuManager.Instance.Build(PlayerMenu.DeliveryJobMenu, dbPlayer).Show(dbPlayer);
                }
                else if (deliveryJob.LoadPosition.DistanceTo(dbPlayer.Player.Position) < 2.0f)
                {
                    if (dbPlayer.HasData("delivery_tour_start"))
                    {
                        dbPlayer.SendNewNotification("Du hast bereits alle Pakete eingeladen. Setz dich in dein Fahrzeug um deine Tour zu starten.", PlayerNotification.NotificationType.DELIVERY, deliveryJob.Name);
                        return false;
                    }

                    if (!dbPlayer.HasData("delivery_has_package"))
                    {
                        dbPlayer.SetData("delivery_has_package", true);
                        AttachmentModule.Instance.AddAttachment(dbPlayer, Attachment.BOX);
                        dbPlayer.Player.TriggerEvent("courierSetCarrying", true);
                        dbPlayer.SendNewNotification("Du hast die Ladung entgegengenommen. Bring sie nun zum Fahrzeug und lad sie ein.", PlayerNotification.NotificationType.DELIVERY, deliveryJob.Name);
                    }
                    else
                    {
                        dbPlayer.SendNewNotification("Du musst die Ladung erst zum Fahrzeug bringen! Danach kannst du falls benötigt neue Ladung holen.", PlayerNotification.NotificationType.DELIVERY, deliveryJob.Name);
                        return false;
                    }

                }
                else
                {
                    var closestVehicle = VehicleHandler.Instance.GetClosestVehicle(dbPlayer.Player.Position, 2.0f);
                    if (closestVehicle != null && closestVehicle.jobid == DeliverJobVehMarkId && closestVehicle.ownerId == dbPlayer.Id)
                    {
                        if (dbPlayer.HasData("delivery_has_package") && !dbPlayer.HasData("delivery_tour_start"))
                        {
                            dbPlayer.SendNewNotification("Du lädst die Ware in dein Fahrzeug.", PlayerNotification.NotificationType.DELIVERY, deliveryJob.Name);
                            AttachmentModule.Instance.RemoveAttachment(dbPlayer, (Attachment) Attachment.BOX);
                            dbPlayer.Player.TriggerEvent("courierSetCarrying", false);
                            dbPlayer.ResetData("delivery_has_package");

                            if (!dbPlayer.HasData("delivery_vehicle_package_amount"))
                            {
                                dbPlayer.SetData("delivery_vehicle_package_amount", 1);
                            }
                            else
                            {
                                dbPlayer.SetData("delivery_vehicle_package_amount", dbPlayer.GetData("delivery_vehicle_package_amount") + 1);
                            }

                            if (dbPlayer.GetData("delivery_vehicle_package_amount") == neededPackages)
                            {
                                dbPlayer.SendNewNotification("Du hast die gesamte Ware erfolgreich eingeladen. Steig nun in dein Fahrzeug. Sobald du sitzt startet die Tour.", PlayerNotification.NotificationType.DELIVERY, deliveryJob.Name);
                                dbPlayer.SetData("delivery_tour_start", false);
                            }
                            else
                            {
                                dbPlayer.SendNewNotification($"Du musst noch {neededPackages - dbPlayer.GetData("delivery_vehicle_package_amount")} Pakete abholen und einladen", PlayerNotification.NotificationType.DELIVERY, deliveryJob.Name);
                            }
                        }
                        else
                        {
                            //Spieler hat bereits Fahrzeug voll beladeen & will jetzt ggf. Ladung rausnehmen um es beim Ziel abzuladen

                            if (deliveryOrder.NextPosition != null)
                            {
                                if (!dbPlayer.HasData("delivery_has_package"))
                                {
                                    if (dbPlayer.Player.Position.DistanceTo(deliveryOrder.NextPosition) < 100.0f)
                                    {
                                        dbPlayer.SetData("delivery_has_package", true);
                                        AttachmentModule.Instance.AddAttachment(dbPlayer, Attachment.BOX);
                                        dbPlayer.Player.TriggerEvent("courierSetCarrying", true);
                                        dbPlayer.SendNewNotification("Du hast die Ladung aus dem Fahrzeug genommen. Bring sie zum Ziel und liefer sie ab.", PlayerNotification.NotificationType.DELIVERY, deliveryJob.Name);
                                    }
                                    else
                                    {
                                        dbPlayer.SendNewNotification("Du kannst die Ware erst in der unmittelbaren Umgebung des Zielorts entnehmen");
                                        return false;
                                    }   
                                }

                            }
                            else
                            {
                                //Spieler hat die Tour noch nicht gestartet also kann er auch noch kein Paket ausladen.
                                dbPlayer.SendNewNotification("Du kannst die Ware erst ausladen sobald du deine Tour gestartet hast und an deinem Zielort bist.");
                                return false;
                            }
                        }

                    }
                    else
                    {
                        if (dbPlayer.HasData("delivery_has_package") && dbPlayer.Player.Position.DistanceTo(deliveryOrder.NextPosition) < 3.0f && deliveryOrder.NextPosition != null)
                        {
                            //Spieler ist am Zielort.
                            AttachmentModule.Instance.RemoveAttachment(dbPlayer, (Attachment) Attachment.BOX);
                            dbPlayer.Player.TriggerEvent("courierSetCarrying", false);
                            dbPlayer.ResetData("delivery_has_package");



                            dbPlayer.SendNewNotification("Du hast die Ware zum Ziel gebracht. Geh zurück zu deinem Fahrzeug um deinen Auftrag weiter zu führen.");
                            deliveryOrder.DeliveryPositions.Remove(deliveryOrder.NextPosition);
                            deliveryOrder.DeliveryPositions.Add(deliveryOrder.NextPosition, true);
                            deliveryOrder.NextPosition = null;
                        }
                    }
                }

            }
            else
            {
                DeliveryJob deliveryJob = DeliveryJobModule.Instance.GetAll().Where(d => d.Value.Position.DistanceTo(dbPlayer.Player.Position) < 2.0f).FirstOrDefault().Value;

                if (deliveryJob != null)
                {
                    if (deliveryJob.RequiredLevel > dbPlayer.Level)
                    {
                        dbPlayer.SendNewNotification("Für diesen Job reicht deine Visumsstufe noch nicht aus!", PlayerNotification.NotificationType.DELIVERY, deliveryJob.Name);
                        return false;
                    }

                    dbPlayer.SetData("delivery_job_id", deliveryJob.Id);
                    MenuManager.Instance.Build(PlayerMenu.DeliveryJobMenu, dbPlayer).Show(dbPlayer);
                }
            }



            return false;
        }

        public bool HasPlayerNeededLicence(DbPlayer dbPlayer, DeliveryJobType deliveryJobType)
        {
            switch (deliveryJobType.RequiredLicence)
            {
                case 0: return true;
                case 1:
                    if (dbPlayer.Lic_Car[0] == 1) return true;
                    break;
                case 2:
                    if (dbPlayer.Lic_LKW[0] == 1) return true;
                    break;
                case 3:
                    if (dbPlayer.Lic_PlaneA[0] == 1) return true;
                    break;
                case 4:
                    if (dbPlayer.Lic_PlaneB[0] == 1) return true;
                    break;
            }
            return false;
        }



        public void InitPlayerStartDeliveryJob(DbPlayer dbPlayer, DeliveryJob deliveryJob, DeliveryJobType deliveryJobType)
        {
            //Check ob Spieler überhaupt die benötigte Lizenz hat

            if (!HasPlayerNeededLicence(dbPlayer, deliveryJobType))
            {
                dbPlayer.SendNewNotification("Für diesen Auftrag fehlt dir der nötige Führerschein.", PlayerNotification.NotificationType.DELIVERY, deliveryJobType.Name);
                return;
            }


            //Abfrage ob es bereits aktive Aufträge gibt && der Spieler bereits einen angenommen hat
            if (DeliveryOrders.Count > 0 && DeliveryOrders.ContainsKey(dbPlayer))
            {
                dbPlayer.SendNewNotification("Du hast bereits einen Auftrag angenommen.", PlayerNotification.NotificationType.DELIVERY, deliveryJobType.Name);
                return;
            }
            //aktuelle DeliveryJobSkillPoints des Spielers abfragen
            if (dbPlayer.DeliveryJobSkillPoints.TryGetValue(deliveryJob.SkillpointType, out int skillPoints))
            {
                //Wenn benötigte Skillpoints vorhanden
                if (skillPoints < deliveryJobType.NeededSkillPoints)
                {
                    dbPlayer.SendNewNotification("Du bist nicht erfahren genug um diesen Job auszuführen.", PlayerNotification.NotificationType.DELIVERY, deliveryJobType.Name);
                    return;
                }

                DeliveryJobSpawnpoint deliveryJobSpawnpoint = null;

                foreach (var spawnPoint in deliveryJob.DeliveryJobSpawnpoints)
                {
                    //versuche ein Fahrzeug was auf dem Ausparkpunkt steht zu finden
                    var closestVehicle = VehicleHandler.Instance.GetClosestVehicle(spawnPoint.Value.Position, 2.0f);
                    if (closestVehicle == null)
                    {
                        deliveryJobSpawnpoint = spawnPoint.Value;
                        break;
                    }
                }

                if (deliveryJobSpawnpoint == null)
                {
                    //kein freier Ausparkpunkt gefunden
                    dbPlayer.SendNewNotification("Es ist aktuell kein freier Ausparkpunkt verfügbar...", PlayerNotification.NotificationType.DELIVERY, deliveryJobType.Name);
                    return;
                }

                //Fahrzeug für den Spieler spawnen
                SxVehicle xVeh = VehicleHandler.Instance.CreateServerVehicle(deliveryJobType.VehicleModel, false,
                    deliveryJobSpawnpoint.Position, deliveryJobSpawnpoint.Heading, Main.rndColor(),
                    Main.rndColor(), 0, true, true, false, 0, dbPlayer.GetName(), 0, DeliverJobVehMarkId, dbPlayer.Id);

                GenerateRandomDeliveryOrder(dbPlayer, deliveryJob, deliveryJobType);
            }
        }
        public void GenerateRandomDeliveryOrder(DbPlayer dbPlayer, DeliveryJob deliveryJob, DeliveryJobType deliveryJobType)
        {
            //Anzahl der zu beliefernden Positionen
            int deliveryAmount = Utils.RandomNumber(deliveryJobType.MinPositionAmount, deliveryJobType.MaxPositionAmount);
            dbPlayer.SendNewNotification($"Du hast den Auftrag angenommen. Hol nun die Ware und belade dein Fahrzeug. Du hast {deliveryAmount} Ziele", PlayerNotification.NotificationType.DELIVERY, deliveryJobType.Name, duration:15000);


            Dictionary<Vector3, bool> deliveryPositions = new Dictionary<Vector3, bool>();
            switch (deliveryJobType.DeliverTo)
            {
                //POSTJOB && FOODJOB
                case DeliveryJobType.DeliverToType.HAUS:
                    var houses = HouseModule.Instance.GetAll().ToList();
                    if (deliveryJobType.MaxDistance != 0)
                    {
                        houses = houses.Where(d => d.Value.Position.DistanceTo(dbPlayer.Player.Position) <= Convert.ToSingle(deliveryJobType.MaxDistance)).ToList();
                    }
                    for (int i = 0; i < deliveryAmount; i++)
                    {
                        int randomHouse = Utils.RandomNumber(0, houses.Count);
                        House house = houses[randomHouse].Value;
                        deliveryPositions.Add(house.Position, false);
                    }
                    break;
                //APOTHEKENJOB
                case DeliveryJobType.DeliverToType.APOTHEKE:
                    var pharmacies = DeliveryJobPharmacyModule.Instance.GetAll().ToList();
                    for (int i = 0; i < deliveryAmount; i++)
                    {
                        int randomPharmacy = Utils.RandomNumber(0, pharmacies.Count);
                        DeliveryJobPharmacy deliveryJobPharmacy = pharmacies[randomPharmacy].Value;
                        deliveryPositions.Add(deliveryJobPharmacy.Position, false);
                    }
                    break;
            }
            DeliveryOrders.Add(dbPlayer, new DeliveryOrder(deliveryPositions, deliveryJob, deliveryJobType));


        }

    }
}

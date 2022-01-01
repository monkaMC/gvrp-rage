using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using GVRP.Module.Logging;
using GVRP.Module.Menu;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Delivery.Menu
{
    public class DeliveryJobMenu : MenuBuilder
    {

        public DeliveryJobMenu() : base(PlayerMenu.DeliveryJobMenu)
        {


        }

        public override Module.Menu.Menu Build(DbPlayer iPlayer)
        {
            if (!iPlayer.HasData("delivery_job_id")) return null;

            DeliveryJob deliveryJob = DeliveryJobModule.Instance.Get(iPlayer.GetData("delivery_job_id"));


            var deliveryJobTypes = DeliveryJobTypeModule.Instance.GetAll().Where(d => d.Value.DeliveryJob.Id == deliveryJob.Id).ToList();
            var deliverJobMenu = new Module.Menu.Menu(Menu, deliveryJob.Name);

            deliverJobMenu.Add("Schließen");
            deliverJobMenu.Add("Auftrag beenden");
            deliverJobMenu.Add("Auftrag abbrechen");
            foreach (var deliverJobType in deliveryJobTypes)
            {
                deliverJobMenu.Add(deliverJobType.Value.Name);
            }

            return deliverJobMenu;
        }

        public override IMenuEventHandler GetEventHandler()
        {
            return new EventHandler();
        }

        private class EventHandler : IMenuEventHandler
        {
            public bool OnSelect(int index, DbPlayer dbPlayer)
            {
                if (!dbPlayer.HasData("delivery_job_id")) return false;

                DeliveryJob deliveryJob = DeliveryJobModule.Instance.Get(dbPlayer.GetData("delivery_job_id"));


                if (index != 0)
                {
                    //Auftrag abbrechen // Auftrag beenden
                    if (index == 1 || index == 2)
                    {
                        if (DeliveryJobModule.Instance.DeliveryOrders.ContainsKey(dbPlayer))
                        {
                            //Spieler hat einen Auftrag -> Auftrag muss beendet werden
                            dbPlayer.FinishDeliveryJob(index == 1 ? true : false, deliveryJob);
                        }
                        else
                        {
                            //Spieler hat keinen Auftrag
                            dbPlayer.SendNewNotification("Du hast keinen Auftrag angenommen");
                        }
                    }
                    else
                    {
                        DeliveryJobType deliveryJobType = DeliveryJobTypeModule.Instance.GetAll().Where(d => d.Value.DeliveryJob.Id == deliveryJob.Id).ElementAt(index - 3).Value;
                        DeliveryJobModule.Instance.InitPlayerStartDeliveryJob(dbPlayer, deliveryJob, deliveryJobType);
                    }
                }


                MenuManager.DismissCurrent(dbPlayer);
                return true;
            }
        }
    }
}

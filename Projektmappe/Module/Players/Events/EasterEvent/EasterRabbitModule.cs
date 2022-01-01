using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GVRP.Module.Items;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Players.Events.EasterEvent
{
    public class EasterRabbitModule : SqlModule<EasterRabbitModule, EasterRabbit, uint>
    {
        protected override string GetQuery()
        {
            return "SELECT * FROM `event_easterrabbits`;";
        }

        public bool isActive = false;

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (dbPlayer.Player.IsInVehicle || dbPlayer.Dimension[0] != 0 || !isActive) return false;
            
            EasterRabbit easterRabbit = EasterRabbitModule.Instance.GetAll().Values.Where(er => er.Position.DistanceTo(dbPlayer.Player.Position) < 1.5f).FirstOrDefault();
            if (easterRabbit != null)
            {
                if (dbPlayer.HasEventDone(EventStateModule.EventListIds.EASTER))
                {
                    dbPlayer.SendNewNotification("Du hast dieses Event bereits abgeschlossen!");
                    return true;
                }

                if (easterRabbit.LastUsed.AddMinutes(15) > DateTime.Now)
                {
                    dbPlayer.SendNewNotification("Hier findest du gerade nichts, schau dich doch weiter um!");
                    return true;
                }
                else
                {
                    uint EasterEggId = 1034;
                    Random rnd = new Random();
                    int RCount = rnd.Next(1, 30);
                    if (RCount > 10 && RCount <= 20)
                    {
                        EasterEggId = 1035;
                    }
                    else if(RCount > 20)
                    {
                        EasterEggId = 1036;
                    }

                    if(dbPlayer.Container.CanInventoryItemAdded(EasterEggId))
                    {
                        dbPlayer.Container.AddItem(EasterEggId);
                        dbPlayer.SendNewNotification($"Du hast ein {ItemModelModule.Instance.Get(EasterEggId).Name} bekommen! Frohe Ostern wünscht Nexus Roleplay.");
                        dbPlayer.ChangeEventState(EventStateModule.EventListIds.EASTER, 1);
                        easterRabbit.LastUsed = DateTime.Now;
                    }
                    else
                    {
                        dbPlayer.SendNewNotification("Da ist was, aber dein Rucksack reicht nicht aus...");
                    }
                    return true;
                }
            }

            return false;
        }
    }
}

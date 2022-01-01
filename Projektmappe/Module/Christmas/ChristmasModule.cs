using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using GVRP.Module.Configurations;
using GVRP.Module.Items;
using GVRP.Module.Players;
using GVRP.Module.Players.Db;

namespace GVRP.Module.Christmas
{
    public sealed class ChristmasModule : Module<ChristmasModule>
    {
        public Vector3 PresentLocation = new Vector3(-1571.23, -1013.83, 14.5018);

        enum Geschenke
        {
            GrünesGeschenk      = 882,
            HellblauesGeschenk  = 883,
            PinkesGeschenk      = 884,
            GelbesGeschenk      = 885,
            GrauesGeschenk      = 886,
            SchwarzesGeschenk   = 887
        }

        public override bool OnKeyPressed(DbPlayer dbPlayer, Key key)
        {
            if (key != Key.E || dbPlayer.Player.IsInVehicle || !dbPlayer.CanInteract()) return false;

            if (dbPlayer.Player.Position.DistanceTo(PresentLocation) <= 10)
            {
                //Wenn Spieler in normaler World
                if (dbPlayer.Dimension[0] == 0)
                {
                    DateTime actualDate = System.DateTime.Now;
                    if ((actualDate.Month == 12 && actualDate.Day >= 1 && actualDate.Day <= 24) || Configurations.Configuration.Instance.DevMode)
                    {
                        //Wenn letzte Abholung nicht am selben Tag sondern davor war
                        if (dbPlayer.xmasLast.Day < actualDate.Day || dbPlayer.xmasLast.Day == 30 && dbPlayer.xmasLast.Month == 11)
                        {
                            //Gucken ob genug Platz im Inventar ist... #ICH HAB KEIN PAKET BEKOMMEN WEIL ICH KEIN PLATZ HATTE UND EIN KEK BIN
                            if (!dbPlayer.Container.CanInventoryItemAdded((uint)Geschenke.GrünesGeschenk))
                            {
                                dbPlayer.SendNewNotification("Du hast nicht genuegend Platz um das Paket entgegen zu nehmen", PlayerNotification.NotificationType.ERROR, "XMAS");
                                return false;
                            }

                            // Ich grüße meine Mam,
                            // Meinen Papa...
                            // Meine Geschwister
                            // Und Espenhain und Joe für das Erstellen der levelbasierten Geschenke
                            //if (dbPlayer.Level >= 50)
                            //    dbPlayer.Container.AddItem((uint)Geschenke.SchwarzesGeschenk);
                            //else if (dbPlayer.Level >= 40)
                            //    dbPlayer.Container.AddItem((uint)Geschenke.GrauesGeschenk);
                            //else if (dbPlayer.Level >= 30)
                            //    dbPlayer.Container.AddItem((uint)Geschenke.GelbesGeschenk);
                            //else if (dbPlayer.Level >= 20)
                            //    dbPlayer.Container.AddItem((uint)Geschenke.PinkesGeschenk);
                            //else if (dbPlayer.Level >= 10)
                            //    dbPlayer.Container.AddItem((uint)Geschenke.HellblauesGeschenk);
                            //else
                            //    dbPlayer.Container.AddItem((uint)Geschenke.GrünesGeschenk);


                            dbPlayer.Container.AddItem(550);

                            dbPlayer.SendNewNotification("Hier ist dein Geschenk! Packst du es jetzt schon aus?", PlayerNotification.NotificationType.SUCCESS, "XMAS");
                            dbPlayer.SendNewNotification("Oder wartest du bis Heiligabend?", PlayerNotification.NotificationType.INFO, "XMAS");

                            // Für ordentliche Tests alla
                            if (!Configuration.Instance.DevMode)
                            {
                                dbPlayer.xmasLast = actualDate;
                                dbPlayer.SaveChristmasState();
                            }
                        }
                        //Abholung war am selben Tag bereits...
                        else
                        {
                            dbPlayer.SendNewNotification("Du hast dein Geschenk fuer heute bereits abgeholt.", PlayerNotification.NotificationType.ERROR, "XMAS");
                            return false;
                        }
                    }
                }
            }
            return false;
        }

        protected override bool OnLoad()
        {
            PlayerNotifications.Instance.Add(PresentLocation, "XMAS", "Was haben wir denn hier...");
            return true;
        }
    }
}

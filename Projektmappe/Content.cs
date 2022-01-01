using System;
using System.Collections.Generic;

namespace GVRP
{
    public static class Content
    {
        public static class General
        {
            public static string Player = "Spieler";

            public static string Donorlevel1 = "Premium";
            public static string Donorlevel2 = "Donator";

            public static string GetDonorName(int donor = 0)
            {
                switch (donor)
                {
                    case 1:
                        return Donorlevel1;
                    case 2:
                        return Donorlevel2;
                    default:
                        return Player;
                }
            }
        }

        public static class License
        {
            public static string Car = "Fuehrerschein";
            public static string Bike = "Motorradschein";
            public static string PlaneA = "Flugschein A";
            public static string PlaneB = "Flugschein B";
            public static string Gun = "Waffenschein";
            public static string Biz = "Gewerbeschein";
            public static string Lkw = "LKW Fuehrerschein";
            public static string Boot = "Bootsschein";
            public static string Transfer = "Personenbeförderungsschein";
            public static string Taxi = "Taxizulassung";
            public static string marryLic = "Standesamt-Lizenz";
        }

        public static string GetGwdText(int grade)
        {
            if (grade <= 0 || grade > 9)
            {
                return "nicht vorhanden";
            }
            return "" + grade;
        }
    }

    public static class Price
    {
        public static class License
        {
            public static int Car = 5000;
            public static int Bike = 1500;
            public static int PlaneA = 15000;
            public static int PlaneB = 35000;
            public static int Gun = 25000;
            public static int Biz = 25000;
            public static int Lkw = 8000;
            public static int Boot = 3000;
            public static int Transfer = 5000;
        }

        public static class Weapons
        {
            public static int Pistol = 1500;
            public static int Bat = 500;
            public static int Combatpistol = 18000;
            public static int Heavypistol = 3500;
            public static int Microsmg = 7500;
            public static int Sawnoffshotgun = 7500;
            public static int Assaultrifle = 12000;
            public static int Smg = 10500;
            public static int Sniperrifle = 25000;
            public static int Carbinerifle = 12000;
            public static int Rpg = 35000;
            public static int Molotov = 4000;
            public static int Pumpshotgun = 7500;
            public static int Armor = 5500;
            public static int Parachute = 900;
            public static int Flashlight = 126;

            public class AmmoBuy
            {
                public static int Combatpistol = 12;
                public static int Heavypistol = 18;
                public static int Microsmg = 16;
                public static int Sawnoffshotgun = 8;
                public static int Assaultrifle = 30;
                public static int Smg = 30;
                public static int Sniperrifle = 10;
                public static int Carbinerifle = 30;
                public static int Rpg = 3;
                public static int Molotov = 24;
                public static int Pumpshotgun = 8;
            }

            public class AmmoBuyPriceShot
            {
                public static int Combatpistol = 3;
                public static int Heavypistol = 3;
                public static int Microsmg = 3;
                public static int Sawnoffshotgun = 3;
                public static int Assaultrifle = 3;
                public static int Smg = 3;
                public static int Sniperrifle = 15;
                public static int Carbinerifle = 3;
                public static int Rpg = 250;
                public static int Molotov = 50;
                public static int Pumpshotgun = 3;
            }
        }
    }

    public static class Ammo
    {
        public static int GetAmmo(string weapon)
        {
            int ammo = 500;
            switch (weapon.ToLower())
            {
                case "pistol":
                    ammo = Pistol;
                    break;

                case "bat":
                    ammo = Bat;
                    break;

                case "combatpistol":
                    ammo = Combatpistol;
                    break;

                case "heavypistol":
                    ammo = Heavypistol;
                    break;

                case "microsmg":
                    ammo = Microsmg;
                    break;

                case "sawnoffshotgun":
                    ammo = Sawnoffshotgun;
                    break;

                case "assaultrifle":
                    ammo = Assaultrifle;
                    break;

                case "sniperrifle":
                    ammo = Sniperrifle;
                    break;

                case "carbinerifle":
                    ammo = Carbinerifle;
                    break;

                case "rpg":
                    ammo = Rpg;
                    break;

                case "molotov":
                    ammo = Molotov;
                    break;

                case "pumpshotgun":
                    ammo = Pumpshotgun;
                    break;
                case "smg":
                    ammo = Pumpshotgun;
                    break;
                default:
                    ammo = 50;
                    break;
            }
            return ammo;
        }

        public static int Pistol = 128;
        public static int Bat = 999;
        public static int Combatpistol = 32;
        public static int Heavypistol = 32;
        public static int Microsmg = 600;
        public static int Sawnoffshotgun = 80;
        public static int Assaultrifle = 600;
        public static int Sniperrifle = 20;
        public static int Carbinerifle = 600;
        public static int Rpg = 3;
        public static int Molotov = 5;
        public static int Pumpshotgun = 80;
        public static int Smg = 300;
    }

    public static class JobContent
    {
        public static class Plagiat
        {
            public static string Name = "Faelscher";
            public static string Help = "/materials get /materials deliver /createlic";

            public static string Description =
                "Bekomme Materialien (Rohlinge) um Fuehrerscheine und andere Lizenzen zu faelschen!";

            public static class Materials
            {
                public static int Dividor = 85;
                public static int Car = 35;
                public static int Bike = 15;
                public static int PlaneA = 210;
                public static int PlaneB = 220;
                public static int Gun = 200;
                public static int Biz = 150;
                public static int Lkw = 40;
                public static int Boot = 20;
                public static int Transfer = 40;
            }

            public static class Requiredskill
            {
                public static int Car = 10;
                public static int Bike = 0;
                public static int PlaneA = 60;
                public static int PlaneB = 80;
                public static int Gun = 50;
                public static int Biz = 15;
                public static int Lkw = 20;
                public static int Boot = 30;
                public static int Transfer = 20;
            }
        }

        public static class Pilot
        {
            public static string Name = "Pilot";
            public static string Help = "/fly /cb (Flugfunk)";
            public static string Description = "Fliege Lieferungen, steige auf und verdiene Geld!";

            public static class Requiredskill
            {
                public static int Maverick = 0;
                public static int Swift = 400;
                public static int Volatus = 900;
                public static int Velum = 2000;
                public static int Cuban800 = 2400;
                public static int Duster = 3900;
                public static int Mammatus = 3200;
                public static int Vestra = 3500;
                public static int Nimbus = 4000;
                public static int Luxor = 4400;
                public static int Shamal = 4800;
                public static int Titan = 5000;
            }
        }

        public static class Farmer
        {
            public static string Name = "Farmer";
            public static string Help = "Benutze /farmer [start/end] um deine Arbeit zu beginnen/beenden";
            public static string Description = "Halte den Hof instand, kuemmer dich um die Felder!";
        }

        public static class Fisher
        {
            public static string Name = "Fischer";
            public static string Help = "/fish start (am Jobpunkt) /fish end (Beenden)  /cb (Funk)";
            public static string Description = "Meeresrauschen, Ruhe und Fische fangen.";
        }

        public static class Makler
        {
            public static string Name = "Makler";
            public static string Help = "/sellcar|sellhouse|sellstorage [Player1] [Player2] [Price]";
            public static string Description = "Verkaufe Haeuser, Fahrzeuge und verdiene Geld durch Provisionen.";
        }

        public static class Life
        {
            public static string Name = "Lifeguard";
            public static string Help = "/life start(Starten) /life end(Beenden)";
            public static string Description = "Sorge fuer den Strand, die Menschen und die Raeumlichkeiten!";

            public static class Requiredskill
            {
                public static int Lguard = 5000;
                public static int Blazer2 = 0;
            }
        }

        public static class Mech
        {
            public static string Name = "Mechaniker";

            public static string Help =
                    "Materialien mit \"E\" am Job-NPC /paintcar /paintsave /neoncar /perlcar /perlsave /neonsave /horncar /hornsave /changelock"
                ;

            public static string Description = "Tune und Lackiere Fahrzeuge!";

            public static class Materials
            {
                public static int GetMaterialForColor(int colorid)
                {
                    if (colorid == 120) return 5;
                    if (colorid == 21) return 5;
                    if (colorid == 55) return 5;
                    return 5;
                }
            }

            public static class Requiredskill
            {
                public static int GetSkillForColors(int colorid)
                {
                    if (colorid >= 50 && colorid <= 80) return 1000;
                    if (colorid >= 80 && colorid <= 110) return 2000;
                    if (colorid >= 110 && colorid <= 140) return 3000;
                    if (colorid >= 140 && colorid <= 170) return 4000;
                    return 0;
                }
            }
        }

        public static class Bus
        {
            public static string Name = "Busfahrer";
            public static string Help = "/bus start(Starten) /bus end(Beenden) /cb (Funk)";
            public static string Description = "Fahre Buslinien, befördere Fahrgaeste und verdiene Geld!";
        }

        public static class Trucker
        {
            public static string Name = "Trucker";

            public static string Help =
                "/truck start(Starten) /truck end(Beenden) /truckgps /loadtruck /delivertruck /cb (Funk)";

            public static string Description = "Befoerdere Waren von A nach B und verdiene damit Geld!";

            public static class Requiredskill
            {
                public static int Tiptruck = 0;
                public static int Tiptruck2 = 1000;
                public static int Mule3 = 2000;
                public static int Rubble = 3000;
                public static int Pounder = 5000;
            }
        }

        public static class Stripper
        {
            public static string Name = "Stripper";
            public static string Help = "/strip start(Starten) /strip end(Beenden)";
            public static string Description = "Verdiene im Rotlichtmilieu Geld!";

            public static class Requiredskill
            {
                public static int Lguard = 5000;
                public static int Blazer2 = 0;
            }
        }

        public static class Garden
        {
            public static string Name = "Gaertner";
            public static string Help = "/garden [start/end/gps] /weed fertilize /shop (Jobshop)";
            public static string Description = "Verdiene mit deinem gruenen Daumen Geld!";

            public static class Requiredskill
            {
                public static int Ferilize = 3000;
            }
        }

        public static class Anwalt
        {
            public static string Name = "Anwalt";
            public static string Help = "/getfree (Cop erlaubnis) /free Name (am Gefaengnis)";
            public static string Description = "Hole Leute aus dem Gefaengnis, setze dich fuer Ihr Recht ein!";
        }

        public static class Weapondealer
        {
            public static string Name = "Waffendealer";
            public static string Help = "/buildweapon";
            public static string Description = "Verarbeite Eisen zu Waffen, welche du verkaufst!";
        }

        public static class Pflanzenkunde
        {
            public static string Name = "Pflanzenkunde";
            public static string Help = "";
            public static string Description = "Bau Pflanzen verschiedenster Art ab";
        }

        public static class Bergbau
        {
            public static string Name = "Bergbau";
            public static string Help = "";
            public static string Description = "Baue Erze ab und verarbeite diese weiter!";
        }
    }

    public class AnimationContent
    {
        // declare animation class
        public class Animation
        {
            public string Hash { get; set; }
            public string Name { get; set; }
            public bool Loop { get; set; }
            public string Dic { get; set; }
            public bool Move { get; set; }

            // animation loop and cancellabel
            public int Flag
            {
                get
                {
                    // evaluate the AnimationFlag, default cancellabel
                    int flag = (int) AnimationFlags.AllowPlayerControl;

                    if (Move)
                    {
                        flag = (int) (AnimationFlags.AllowPlayerControl | AnimationFlags.OnlyAnimateUpperBody);
                    }

                    // loop animation
                    if (Loop)
                    {
                        flag = (int) AnimationFlags.Loop;

                        if (Move)
                        {
                            flag = (int) (AnimationFlags.Loop | AnimationFlags.AllowPlayerControl |
                                          AnimationFlags.OnlyAnimateUpperBody);
                        }
                    }

                    return flag;
                }
            }
        }

        [Flags]
        public enum AnimationFlags
        {
            Loop = 1 << 0,
            
            OnLastFrame = 1 << 1,
            OnlyAnimateUpperBody = 1 << 4,
            AllowPlayerControl = 1 << 5,
            Cancellable = 1 << 7,
            AnimateFullBodyWithControl = 32
        }
    }
}
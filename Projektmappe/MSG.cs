namespace GVRP
{
    public static class MSG
    {

        public static class Actions
        {
            public static string BuyAction(string ItemName, int price)
            {
                return "Sie haben " + ItemName + " für $" + price + " erworben!";
            }
        }

        public static class Inventory
        {
            public static string NotEnoughSpace()
            {
                return "Sie haben nicht genügend Platz in ihrem Rucksack!";
            }
        }
        public static class Money
        {
            public static class Me
            {
                public static string PlayerGiveMopney()
                {
                    return " zueckt das Portemonnaie.";
                }

                public static string PlayerTakeMoney()
                {
                    return " hat etwas zugesteckt bekommen.";
                }
            }

            public static string NotEnoughMoney(int betrag)
            {
                return $"Sie haben nicht genug Geld! Benoetigt ({betrag}$)";
            }

            public static string NotEnoughSWMoney(int betrag)
            {
                return $"Sie haben nicht genug Schwarzgeld! Benoetigt ({betrag}$)";
            }

            public static string PlayerNotEnoughMoney(int betrag)
            {
                return $"Spieler hat nicht genug Geld! Benoetigt ({betrag}$)";
            }

            public static string PlayerSelfMoney()
            {
                return "Sie können sich selbst kein Geld geben!";
            }

            public static string InvalidAmount()
            {
                return "Ungueltige Eingabe!";
            }

            public static string PlayerGiveMoney(int amount)
            {
                return $"Sie haben jemanden {amount}$ gegeben.";
            }

            public static string PlayerGotMoney(int amount)
            {
                return $"Jemand hat ihnen {amount}$ gegeben.";
            }
        }

        public static class Job
        {
            public static string NotEnoughSkill(int required)
            {
                return "Sie haben nicht die benoetigte Erfahrung fuer diese Aktion (benoetigt " + required + ")";
            }
        }

        public static class General
        {
            public static string notInRange = "Spieler befindet sich nicht in Ihrer Naehe!";

            public static string isArrested(string cop, int jailtime)
            {
                return "Sie wurden von " + cop + " ins Gefaegnis gesteckt! Gefaengniszeit: " + (jailtime - 1) +
                       " Minuten";
            }

            public static string hasArrested(string suspect, int jailtime)
            {
                return "Sie haben " + suspect + " fuer " + jailtime + " Minuten ins Gefaengnis gesteckt!";
            }

            public static string Usage(string command, string param1 = "", string param2 = "")
            {
                string temp = "";
                if (!string.IsNullOrEmpty(param1))
                {
                    temp = param1;
                }
                if (!string.IsNullOrEmpty(param2))
                {
                    temp = param1 + " " + param2;
                }
                return "Benutze: " + command + " " + temp;
            }

            public static string welcomejail(string name, int jailtime)
            {
                return "Hallo " + name + ", durch Ihre Verbrechen sitzen Sie noch fuer " + jailtime +
                       " Minuten im Gefaengnis!";
            }

            public static string welcome(string name, string Fraktion)
            {
                return "Hallo " + name + ", Sie haben sich als Mitglied der " + Fraktion + " eingeloggt.";
            }

            public static string Proof()
            {
                return " ueberpruefen";
            }

            public static string Close()
            {
                return "Schließen";
            }

            public static string Back()
            {
                return "Zurueck";
            }
        }

        public static class Error
        {
            public static string NoPermissions()
            {
                return "Sie haben keine Berechtigung fuer diesen Vorgang!";
            }

            public static string NoTeam()
            {
                return "Sie befinden sich derzeit in keiner Fraktion/Gruppierung!";
            }

            public static string NoJob()
            {
                return "Sie befinden sich derzeit in keinem Nebenberuf!";
            }

            public static string NoPlayer()
            {
                return "Ungueltiger Spieler!";
            }
            public static string NoMarry()
            {
                return "Sie sind nicht im Besitz einer Standesamt-Lizenz!";
            }
        }

        public static string HelpGang()
        {
            return "Gang Befehle: /equip /fbank (Equippunkt)";
        }

        public static string HelpPolice()
        {
            return "Polizei Befehle: /clearwanteds /su (Wantedvergabe) /policerepair /friskhouse";
        }

        public static string HelpPolice2()
        {
            return
                "Polizei Befehle: /wanteds /takelic /fspawnchange /warrant USER /takeweapons";
        }

        public static string HelpPolice3()
        {
            return
                "Polizei Befehle: /db (LSPD HQ) /cwanteds /checkwanted /kickout /drugtest /findrob";
        }

        public static string HelpPolice4()
        {
            return
                "Polizei Befehle: /takeitem /frisk /friskveh /takevehitem /friskhouse /takehouseitem /servicelist /acceptservice";
        }

        public static string HelpArmy()
        {
            return "Polizei Befehle: /setgwd /loadwk /unloadwk";
        }

        public static string HelpDrivingSchool()
        {
            return "Fahrschul Befehle: /givelic (Lizenzen geben) /duty /servicelist";
        }

        public static string HelpNews()
        {
            return "News Befehle: /news (News schreiben)";
        }

        public static string HelpMedic()
        {
            return
                "Medic Befehle: /heal (Spieler heilen) /getrev (Toten Spieler einladen) /deliverrev /initrev /medicrepair";
        }

        public static string HelpMedic2()
        {
            return
                "Medic Befehle: /instrev (NUR BEI PROBLEMEN MIT INTERIORS) /fspawnchange /servicelist /acceptservice (/accs)";
        }

        public static string HelpLeader()
        {
            return "/invite [Name] /manage [Name]";
        }

/*        public static string HelpBusiness()
        {
            return "/b (usinesschat)";
        }*/

        public static string MDC_LicCheck(bool has)
        {
            if (has == false)
            {
                return "Datenbank: In der Datenbank wurde kein Eintrag zu dieser Lizenz registriert!";
            }
            else
            {
                return "Datenbank: Diese Lizenz wurde in der Datenbank registriert!";
            }
        }

        public static class License
        {
            public static string getStatus(int state)
            {
                if (state < 0)
                    return "Sperre (" + state + ")";
                else if (state == 0)
                    return "Nicht Vorhanden";
                if (state == 1 || state == 2)
                    return "Vorhanden";
                return "";
            }

            public static string PlayerAlreadyOwnLic(string lic)
            {
                return "Spieler besitzt bereits einen " + lic + ".";
            }

            public static string AlreadyOwnLic(string lic)
            {
                return "Sie besitzen bereits einen " + lic + ".";
            }

            public static string NeedOwnLic(string lic)
            {
                return "Sie besitzen keinen " + lic + ".";
            }

            public static string PlayerNeedOwnLic(string lic)
            {
                return "Spieler besitzt keinen " + lic + ".";
            }

            public static string HasGiveYouLicense(string name, int price, string license)
            {
                return "Fahrlehrer " + name + " hat Ihnen den " + license + " gegeben. (Kosten: " + price + "$)";
            }

            public static string HasCreateYouLicense(string name, string license)
            {
                return name + " hat Ihnen den " + license + " gegeben. (Achtung Plagiate sind illegal)";
            }

            public static string HaveGetLicense(string name)
            {
                return "Sie haben den " + name + " erhalten!";
            }

            public static string YouHaveGiveLicense(string playername, string licensename)
            {
                return "Sie haben " + playername + " den " + licensename + " gegeben!";
            }
        }
    }
}
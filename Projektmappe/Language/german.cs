
using GVRP.Module.Chat;
using GVRP.Module.Players.Db;

namespace GVRP
{
    public static class Lang
    {
        public static int getLang()
        {
            return 0;
        }

        // Admin
        public static string[] anticheat_offline(string name, string reason)
        {
            string[] xx = { "Spieler " + name + " ist disconnected! (Grund: " + reason + ")", "" };
            return xx;
        }

        // Names
        public static string[] diamonds = { "Diamanten", ""};
        public static string[] meth = { "Meth", "" };
        public static string[] weedseed = { "Weedsamen", "" };

        // Global Messages
        public static string[] Welcome = { "Hallo", "Hello" };

        // Player Messages
        public static string[] login_successfull = { "Sie wurden erfolgreich eingeloggt.", "" };

        public static string[] login_lastpos(DbPlayer player)
        {
            string[] xx = { "Willkommen " + player.GetName() + ", Sie haben sich an ihrer letzten Position eingeloggt.", "" };
            return xx;
        }

        public static string[] rang = { "Rang ", "" };

        // Medic
        public static string[] death_login = { "Sie wurden schwer verletzt und koennen nur noch auf Hilfe hoffen!", "" };
        public static string[] deadtime_login(int deadtime)
        {
            string[] xx = { "Sie werden in vorraussichtlich " + (deadtime) + " Minuten ihren Verletzungen erlegen!", "" };
            return xx;
        }

        public static string[] dead_use_deadtime = { "Um diese Zeit flexibel einzusehen benutzen Sie /deadtime!", "" };
        public static string[] user_is_dead()
        {
            string[] xx = {Chats.MsgLeistelle + "Es wurde eine schwerverletzte Person gesichtet" };
            return xx;
        }

        public static class Blips
        {
            public static string[] carshop = { "Fahrzeughandel", "" };
            public static string[] petshop = { "Haustier-Shop", "" };
        }


        public static class Trucker
        {
            // Trucker Variables
            public static string[] fishdeliver = { "Fischlieferung", "" };
            public static string[] fueldeliver = { "Benzinlieferung", "" };
            public static string[] wooddeliver = { "Holzlieferung", "" };
            public static string[] steeldeliver = { "Stahllieferung", "" };
            public static string[] meatdeliver = { "Fleischlieferung", "" };
            public static string[] coaldeliver = { "Kohlelieferung", "" };
            public static string[] packetdeliver = { "Paketlieferung", "" };
            public static string[] kerosindeliver = { "Kerosinlieferung", "" };
            public static string[] kiesdeliver = { "Kiesladung", "" };
            public static string[] urandeliver = { "Uranlieferung", "" };
            public static string[] schotterdeliver = { "Schotterladung", "" };
            public static string[] stonedeliver = { "Steinladung", "" };
            public static string[] grounddeliver = { "Erdladung", "" };

            public static string[] useLoadTruck = { "Benutze /loadtruck um die Materialien zu laden", "" };
        }

    }
}
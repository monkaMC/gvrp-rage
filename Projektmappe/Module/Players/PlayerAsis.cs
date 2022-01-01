using System.Collections.Generic;
using GTANetworkAPI;


namespace GVRP.Module.Players
{
    public class PlayerAsis
    {
        public static PlayerAsis Instance { get; } = new PlayerAsis();

        private readonly Dictionary<string, string> allowedAsis;

        public PlayerAsis()
        {
            allowedAsis = new Dictionary<string, string>();
            allowedAsis.Add("SkipIntro.asi", "CF0F68109888CE43AEE861066A853E7A"); // Whitelist Default SkipIntro.asi
        }

        public void CheckAsi(Client player)
        {
            /*var asistring = "";

            bool foundWrongAsi = false;
            bool adminIgnore = false;
            if (player.activeAsis == null) return;
            foreach (var asi in player.activeAsis)
            {
                asistring += " " + asi.Key;

                if (asi.Key.Contains("nexus_admin")) adminIgnore = true;

                var asiFound = allowedAsis.ContainsKey(asi.Key);

                if (!asiFound)
                {
                    // Found not Whitelisted asi File
                    foundWrongAsi = true;
                }
                else
                {
                    if (allowedAsis[asi.Key] != asi.Value)
                    {
                        // Wrong Hash of Whitelisted asi found
                        foundWrongAsi = true;
                    }
                }
            }

            if (foundWrongAsi)
            {
                if (!adminIgnore) player.
            Benutzung von eigenen Mods ist nicht erlaubt!");
                LogHandler.LogAsi(player.Name, asistring);
            }*/
        }
    }
}
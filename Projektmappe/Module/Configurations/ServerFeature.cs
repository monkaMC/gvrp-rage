using System;
using System.Collections.Generic;
using System.Text;

namespace GVRP.Module.Configurations
{
    public sealed class ServerFeatures
    {
        public static ServerFeatures Instance { get; } = new ServerFeatures();
        public static List<string> inactiveServerFeatures;

        private ServerFeatures() 
        {
            inactiveServerFeatures = new List<string>();
            inactiveServerFeatures.Add("acpupdate");
        }

        public static bool IsActive(string featureName)
        {
            return !inactiveServerFeatures.Contains(featureName);
        }

        public static void SetActive(string featureName, bool activate)
        {
            if (activate)
            {
                if (!IsActive(featureName)) inactiveServerFeatures.Remove(featureName);
            }
            else
            {
                inactiveServerFeatures.Add(featureName);
            }
        }
    }
}

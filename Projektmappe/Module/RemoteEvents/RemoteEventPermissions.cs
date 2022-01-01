using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GTANetworkAPI;
using GVRP.Module.GTAN;

namespace GVRP.Module.RemoteEvents
{
    public class RemoteEventPermissions
    {
        public static RemoteEventPermissions Instance { get; } = new RemoteEventPermissions();

        private readonly Dictionary<string, RemoteEventPermissionAttribute> remoteEvents;

        private RemoteEventPermissions()
        {
            remoteEvents = new Dictionary<string, RemoteEventPermissionAttribute>();
            
            foreach (var script in Assembly.GetCallingAssembly().GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract))
            {
                foreach (var method in script.GetMethods().Where(ifo => ifo.CustomAttributes.Any(att =>
                    att.AttributeType == typeof(RemoteEventAttribute))))
                {
                    var remoteEventPermissionAttribute = method.GetCustomAttribute<RemoteEventPermissionAttribute>();
                    if (remoteEventPermissionAttribute == null) continue;
                    remoteEvents[method.Name.ToLower()] = remoteEventPermissionAttribute;
                }
            }
        }

        public RemoteEventPermissionAttribute this[string remoteEventName] =>
            !remoteEvents.TryGetValue(remoteEventName, out var value) ? null : value;
    }
}
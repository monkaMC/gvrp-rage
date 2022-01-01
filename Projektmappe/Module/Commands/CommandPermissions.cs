using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using GTANetworkAPI;

namespace GVRP.Module.Commands
{
    public class CommandPermissions
    {
        public static CommandPermissions Instance { get; } = new CommandPermissions();

        private readonly Dictionary<string, CommandPermissionAttribute> commandMethods;

        private CommandPermissions()
        {   
            commandMethods = new Dictionary<string, CommandPermissionAttribute>();
            
            foreach (var script in Assembly.GetCallingAssembly().GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract))
            {
                foreach (var method in script.GetMethods().Where(ifo => ifo.CustomAttributes.Any(att =>
                    att.AttributeType == typeof(CommandAttribute))))
                {
                    var commandPermissionAttribute = method.GetCustomAttribute<CommandPermissionAttribute>();
                    if (commandPermissionAttribute == null) continue;
                    commandMethods[method.Name.ToLower()] = commandPermissionAttribute;
                }
            }   
        }

        public CommandPermissionAttribute this[string methodName] =>
            !commandMethods.TryGetValue(methodName, out var value) ? null : value;
    }
}
using System;
using System.Linq;
using System.Reflection;
using GVRP.Module.Players.Db;
using GTANetworkAPI;
using GVRP.Module.Admin;
using GVRP.Module.ClientUI.Windows;
using System.Threading.Tasks;

namespace GVRP.Module.Players.Windows
{
    public class ChatWindow : Window<Func<DbPlayer, bool>>
    {
        private class ShowEvent : Event
        {
            public ShowEvent(DbPlayer dbPlayer) : base(dbPlayer)
            {
            }
        }

        public ChatWindow() : base("Chat")
        {
        }

        public override Func<DbPlayer, bool> Show()
        {
            return player => OnShow(new ShowEvent(player));
        }

        [RemoteEvent]
        public async void PlayerChat(Client player, string commandAndText)
        {
            
                // Validate Player
                var dbPlayer = player.GetPlayer();
                if (dbPlayer == null || !dbPlayer.IsValid()) return;

                // Validate empty arguments
                if (commandAndText.Length < 1 || commandAndText == " ") return;


                dynamic dynamicCommand = new PlayerCommands();

                string[] commandParts = commandAndText.Split(" ");

                string command = "";
                string commandArgs = "";


                if (commandParts.Length <= 0)
                    return;

                // set command always
                if (commandParts.Length >= 1)
                {
                    command = commandParts[0];
                }

                // Argumente vorhanden
                if (commandParts.Length > 1)
                {
                    commandArgs = commandAndText.Split(" ", 2)[1];

            }

            string plainCommand = command.ToLower();

                MethodInfo addMethod = dynamicCommand.GetType().GetMethod(plainCommand);


                // Search For Command in Modulehandler
                if (addMethod == null)
                {
                    if (Modules.Instance.GetCommand(plainCommand) != null)
                    {
                        addMethod = Modules.Instance.GetCommand(plainCommand);
                        dynamicCommand = Modules.Instance.GetModuleByCommand(plainCommand);
                    }
                }

                // Search in Admin Commands
                if (addMethod == null && dbPlayer.RankId >= 1)
                {
                    dynamicCommand = new AdminModuleCommands();
                    addMethod = dynamicCommand.GetType().GetMethod(plainCommand);
                }

                // Parse Command + Args to Method if found
                if (addMethod != null)
                {
                    var parameters = new object[] { player };

                    if (addMethod.GetParameters().Length > 1)
                    {
                        parameters = new object[] { player, commandArgs };
                    }

                    try
                    {
                        var result = addMethod.Invoke(dynamicCommand, parameters);
                    }
                    catch (Exception ex)
                    {
                        Logging.Logger.Crash(ex);
                        if (commandArgs != "") Logging.Logger.SaveToDbLog($"Command {command} arguments {commandArgs}");
                        else Logging.Logger.SaveToDbLog($"Command {command}");
                    }
                }
            
        }
    }
}
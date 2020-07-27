using Vigilance.API;
using Vigilance.API.Commands;
using Vigilance.API.Extensions;
using System.Collections.Generic;

namespace Vigilance
{
	public static class CommandManager
	{
		public static string CallCommand(CommandSender sender, string[] args)
		{
			string command = args[0].ToUpper();
			Player sender2 = (sender.SenderId == "Server") ? Server.Host.GetPlayer() : sender.GetPlayer();
			return Commands[command].OnCall(sender2, args);
		}

		public static Command GetCommandHandler(string command)
        {
			foreach (string cmd in Commands.Keys)
            {
				if (cmd == command.ToUpper())
                {
					return Commands[cmd];
                }
            }
			return null;
        }

		public static Command GetOverwritingCommand(string command)
        {
			foreach (string cmd in overwritingCommands.Keys)
            {
				if (cmd == command.ToUpper())
                {
					return overwritingCommands[cmd];
                }
            }
			return null;
        }

		public static bool RegisterCommand(Plugin plugin, string command, Command handler)
		{
			if (Commands.ContainsKey(command.ToUpper()))
				return false;
			command = command.ToUpper();
			if (commandsOwnedByPlugins.ContainsKey(plugin))
				commandsOwnedByPlugins[plugin].Add(command);
			else
			{
				List<string> list = new List<string>();
				list.Add(command);
				commandsOwnedByPlugins.Add(plugin, list);
			}
			Commands.Add(command, handler);
			if (!snapshots.ContainsKey(plugin))
				snapshots.Add(plugin, new SnapshotEntry());
			snapshots[plugin].Handlers.Add(command, handler);
			if (handler.OverwriteBaseGameCommand)
            {
				overwritingCommands.Add(command.ToUpper(), handler);
            }
			return true;
		}

		public static void Prepare()
		{
			snapshots = new Dictionary<Plugin, SnapshotEntry>();
			commandsOwnedByPlugins = new Dictionary<Plugin, List<string>>();
			Commands = new Dictionary<string, Command>();
			overwritingCommands = new Dictionary<string, Command>();
		}

		public static Dictionary<Plugin, SnapshotEntry> snapshots;
		public static Dictionary<Plugin, List<string>> commandsOwnedByPlugins;
		public static Dictionary<string, Command> Commands;
		public static Dictionary<string, Command> overwritingCommands;
	}
}

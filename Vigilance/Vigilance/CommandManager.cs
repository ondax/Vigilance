using Vigilance.API.Commands;
using Vigilance.API.Extensions;
using System.Collections.Generic;

namespace Vigilance
{
	public static class CommandManager
	{
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

		public static bool TryGetCommand(string command, out Command cmd)
        {
			Command cmd2 = GetCommandHandler(command);
			if (cmd2 == null)
            {
				cmd = cmd2;
				return false;
            }
			cmd = cmd2;
			return true;
        }

		public static bool TryGetOverwritingCommand(string command, out Command cmd)
        {
			Command cmd2 = GetOverwritingCommand(command);
			if (cmd2 == null)
            {
				cmd = cmd2;
				return false;
            }
			cmd = cmd2;
			return true;
        }

		public static bool CallCommand(string[] args, CommandSender sender, out string response)
        {
			try
			{
				string command = args[0].ToUpper();
				if (TryGetOverwritingCommand(command, out Command overwrite))
				{
					Log.Debug("CommandManager", $"Overwriting command handler for {command} has been found, calling ..");
					response = overwrite.OnCall(sender.GetPlayer(), args);
					return true;
				}

				if (TryGetCommand(command, out Command cmd))
				{
					Log.Debug("CommandManager", $"Command handler for {command} has been found, calling ..");
					response = cmd.OnCall(sender.GetPlayer(), args);
					return true;
				}
				response = $"Unknown command!";
				return false;
			}
			catch (System.Exception e)
            {
				response = "An error occured while calling command!";
				Log.Error("CommandManager", e);
				return false;
            }
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
				overwritingCommands.Add(command.ToUpper(), handler);
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

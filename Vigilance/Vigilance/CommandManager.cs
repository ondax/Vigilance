using Vigilance.API.Commands;
using Vigilance.API.Extensions;
using System.Collections.Generic;
using Vigilance.API;
using Vigilance.Commands.Registered;
using UnityEngine;
using System.Reflection;
using Org.BouncyCastle.Crypto.Engines;
using System.Linq;

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
					Log.Debug("CommandManager", $"CommandHandler for {cmd} has been found.");
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
					Log.Debug("CommandManager", $"Overwriting CommandHandler for {cmd} found.");
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
			{
				Log.Debug("CommandManager", $"Registering {command} to {plugin.Id} ..");
				commandsOwnedByPlugins[plugin].Add(command);
			}
			else
			{
				List<string> list = new List<string>();
				list.Add(command);
				Log.Debug("CommandManager", $"Registering {command} to {plugin.Id} ..");
				commandsOwnedByPlugins.Add(plugin, list);
			}
            Commands.Add(command, handler);
			Log.Debug("CommandManager", $"Succesfully registered CommandHandler for {command}");
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
			Log.Debug("CommandManager", "Registering commands ..");
			Commands.Add("CLEAN", new CommandClean());
			Commands.Add("DROPALL", new CommandDropAll());
			Commands.Add("GIVEALL", new CommandGiveAll());
			Commands.Add("PLAYERS", new CommandPlayers());
			Commands.Add("TPX", new CommandTeleport());
			Commands.Add("PBC", new CommandPersonalBroadcast());
			Commands.Add("ABC", new CommandAdminChat());
			Commands.Add("CLEARRAGDOLLS", new CommandClearRagdolls());
			Commands.Add("CONFIGR", new CommandReloadConfigs());
			Commands.Add("RESTART", new CommandRestart());

			// You have just entered Comedy Club
			Commands.Add("GRENADE", new CommandGrenade());
			Commands.Add("FLASH", new CommandFlash());
			Commands.Add("BALL", new CommandBall());
			Commands.Add("LIGHTS", new CommandLights());
			Commands.Add("RAGDOLL", new CommandRagdoll());
			Commands.Add("DUMMY", new CommandDummy());
			Commands.Add("SWP", new CommandWorkbench());
			Commands.Add("ROCKET", new CommandRocket()); // rocket_max_amount
			Commands.Add("SCALE", new CommandScale());
			Log.Debug("CommandManager", "Commands registered succesfully.");
		}

		public static Dictionary<Plugin, SnapshotEntry> snapshots;
		public static Dictionary<Plugin, List<string>> commandsOwnedByPlugins;
		public static Dictionary<string, Command> Commands;
		public static Dictionary<string, Command> overwritingCommands;
	}
}

namespace Vigilance.Commands.Registered
{
    public class CommandClean : Command
    {
		public string Usage => "Missing arguments!\nUsage: clean <itemId/all/*>";
		public bool OverwriteBaseGameCommand => false;

        public string OnCall(Player sender, string[] args)
        {
			if (args.Length < 2)
				return Usage;
			if (args[1].ToLower() == "*" || args[1].ToLower() == "all")
            {
				Data.Cleanup.ClearAllItems();
				return "Succesfully cleared all items.";
            }
			ItemType item = ItemType.None;
			foreach (Pickup pickup in Map.Pickups)
            {
				if ((int)pickup.ItemId == int.Parse(args[1]))
                {
					pickup.Delete();
					item = pickup.ItemId;
                }
            }
			return $"Succesfully cleared all {item}s";
        }
    }

    public class CommandDropAll : Command
    {
		public string Usage => "Missing arguments!\nUsage: dropall <player/*>";
		public bool OverwriteBaseGameCommand => false;

        public string OnCall(Player sender, string[] args)
        {
			if (args.Length < 2)
				return Usage;
			Player player = args[1].GetPlayer();
			player.DropAllItems();
			return $"Succesfully dropped all items for {player.Nick}";
        }
    }

    public class CommandGiveAll : Command
    {
		public string Usage => $"giveall <itemId>";

		public bool OverwriteBaseGameCommand => false;

        public string OnCall(Player sender, string[] args)
        {
			if (args.Length < 2)
				return Usage;
			ItemType item = (ItemType)int.Parse(args[1]);
			foreach (Player player in Server.Players)
            {
				player.Inventory.AddNewItem(item);
            }
			return $"Succesfully added a {item} to all players.";
        }
    }

    public class CommandPlayers : Command
    {
		public string Usage => "";

		public bool OverwriteBaseGameCommand => false;

        public string OnCall(Player sender, string[] args)
        {
			string str = $"Players ({Server.Players.Count}):\n";
			foreach (Player player in Server.Players)
            {
				str += $"\n {player.ToString()}";
            }
			return str;
        }
    }

    public class CommandTeleport : Command
    {
		public string Usage => "Missing arguments!\nUsage: tpx <player1/*> <player2>";

		public bool OverwriteBaseGameCommand => false;

        public string OnCall(Player sender, string[] args)
        {
			if (args.Length < 3)
				return Usage;
			Player p = args[2].GetPlayer();
			Vector3 teleportTo = p.Position;
			if (args[1].ToLower() == "*" || args[1].ToLower() == "all")
            {
				foreach (Player ply in Server.Players)
                {
					ply.Teleport(teleportTo);
                }
				return $"Succesfully teleported all players to {p.Nick}";
            }
			Player player = args[1].GetPlayer();
			player.Teleport(teleportTo);
			return $"Succesfully teleported {player.Nick} to {p.Nick}";
        }
    }

    public class CommandPersonalBroadcast : Command
    {
		public string Usage => "Missing arguments!\nUsage: pbc <player> <time> <message>";

		public bool OverwriteBaseGameCommand => false;

        public string OnCall(Player sender, string[] args)
        {
			if (args.Length < 4)
				return Usage;
			Player player = args[1].GetPlayer();
			int time = int.Parse(args[2]);
			player.Broadcast(args.SkipWords(3), time);
			return $"Success!\nDuration: {time}\nMessage: {args.SkipWords(3)}\nPlayer: {player.Nick} ({player.UserId})";
        }
    }

	public class CommandAdminChat : Command
	{
		public string Usage => "Missing arguments!\nUsage: abc <time> <message>";

		public bool OverwriteBaseGameCommand => false;

        public string OnCall(Player sender, string[] args)
        {
			if (args.Length < 3)
				return Usage;
			foreach (Player admin in Server.Players.Where(h => h.ServerRoles.RemoteAdmin))
            {
				admin.Broadcast($"<b><color=red>[AdminChat]</color></b> {sender.Nick}:\n{args.SkipWords(3)}", int.Parse(args[2]));
            }
			return $"Success! Your message has been delivered to {Server.Players.Where(h => h.ServerRoles.RemoteAdmin).Count()} online administrators!\nMessage: {args.SkipWords(3)}\nDuration: {int.Parse(args[2])} seconds.";
        }
    }

	public class CommandClearRagdolls : Command
    {
		public string Usage => "";

		public bool OverwriteBaseGameCommand => false;

        public string OnCall(Player sender, string[] args)
        {
            foreach (Ragdoll ragdoll in Map.Ragdolls)
            {
				ragdoll.Delete();
            }
			return $"Succesfully deleted all ragdolls.";
        }
    }

	public class CommandReloadConfigs : Command
    {
		public string Usage => "";

		public bool OverwriteBaseGameCommand => false;

        public string OnCall(Player sender, string[] args)
        {
			Server.ReloadConfigs();
			return $"Success! Configs will be applied on your server next round.";
        }
    }

	public class CommandRestart : Command
    {
		public string Usage => "";

		public bool OverwriteBaseGameCommand => false;

        public string OnCall(Player sender, string[] args)
        {
			Server.Restart(true);
			return $"The server is about to restart! Please wait ..";
        }
    }

	public class CommandGrenade : Command
	{
		public string Usage => "Missing arguments!\nUsage: grenade <player/*>";

		public bool OverwriteBaseGameCommand => false;

		public string OnCall(Player sender, string[] args)
		{
			if (args.Length < 2)
				return Usage;
			if (args[1].ToLower() == "*")
			{
				foreach (Player player in Server.Players)
				{
					Map.SpawnGrenade(player, API.Enums.GrenadeType.FragGrenade);
				}
				return $"Succesfully spawned a frag grenade at all players.";
			}
			Player player1 = args[1].GetPlayer();
			Map.SpawnGrenade(player1, API.Enums.GrenadeType.FragGrenade);
			return $"Succesfully spawned a frag grenade at {player1.Nick}";
		}
	}

	public class CommandFlash : Command
	{
		public string Usage => "Missing arguments!\nUsage: flash <player/*>";

		public bool OverwriteBaseGameCommand => false;

		public string OnCall(Player sender, string[] args)
		{
			if (args.Length < 2)
				return Usage;
			if (args[1] == "*")
			{
				foreach (Player player in Server.Players)
				{
					Map.SpawnGrenade(player, API.Enums.GrenadeType.FlashGrenade);
				}
				return "Succesfully spawned a flash grenade at all players.";
			}
			Player ply = args[1].GetPlayer();
			Map.SpawnGrenade(ply, API.Enums.GrenadeType.FlashGrenade);
			return $"Succesfully spawned a flash grenade at {ply.Nick}";
		}
	}

	public class CommandBall : Command
	{
		public string Usage => "Missing arguments!\nUsage: ball <player/*>";

		public bool OverwriteBaseGameCommand => false;

		public string OnCall(Player sender, string[] args)
		{
			if (args.Length < 2)
				return Usage;
			if (args[1] == "*")
			{
				foreach (Player player in Server.Players)
				{
					Map.SpawnGrenade(player, API.Enums.GrenadeType.Scp018);
				}
				return "Succesfully spawned a SCP-018 at all players.";
			}
			Player ply = args[1].GetPlayer();
			Map.SpawnGrenade(ply, API.Enums.GrenadeType.Scp018);
			return $"Succesfully spawned a SCP-018 at {ply.Nick}";
		}
	}

	public class CommandLights : Command
	{
		public string Usage => "Missing arguments!\nUsage: lights <time>";

		public bool OverwriteBaseGameCommand => false;

		public string OnCall(Player sender, string[] args)
		{
			if (args.Length < 2)
				return Usage;
			if (!int.TryParse(args[1], out int time))
				return "Please specify a valid duration!";
			Map.TurnOffLights((float)time, false);
			return $"Succesfully turned off all lights for {time} {((time > 1) ? "seconds." : "second.")}";
		}
	}

	public class CommandRagdoll : Command
	{
		public string Usage => "Missing arguments!\nUsage: ragdoll <player/*> <role> <amount>";

		public bool OverwriteBaseGameCommand => false;

		public string OnCall(Player sender, string[] args)
		{
			if (args.Length < 4)
				return Usage;
			if (!int.TryParse(args[2], out int role))
				return "Please specify a valid role!";
			if (!int.TryParse(args[3], out int amount))
				return "Please specify a valid amount!";
			if (args[1] == "*")
			{
				foreach (Player player in Server.Players)
				{
					Map.SpawnRagdolls(player, role, amount);
				}
				return $"Succesfully spawned {amount} {(amount > 1 ? "ragdolls" : "ragdoll")} of {(RoleType)role} at all players.";
			}
			Player player1 = args[1].GetPlayer();
			Map.SpawnRagdolls(player1, role, amount);
			return $"Succesfully spawned {amount} {(amount > 1 ? "ragdolls" : "ragdoll")} of {(RoleType)role} at {player1.Nick}";
		}
	}

	public class CommandDummy : Command
	{
		public string Usage => "Missing arguments!\nUsage: dummy <player/*> <role> <x> <y> <z>";
		public bool OverwriteBaseGameCommand => false;

		public string OnCall(Player sender, string[] args)
		{
			if (args.Length < 6)
				return Usage;
			if (!int.TryParse(args[2], out int roleId))
				return "Please specify a valid role!";
			if (!float.TryParse(args[3], out float x) || !float.TryParse(args[4], out float y) || !float.TryParse(args[5], out float z))
				return "Please specify a valid size!";
			RoleType role = (RoleType)roleId;
			if (args[1] == "*")
			{
				foreach (Player player in Server.Players)
				{
					Map.SpawnDummyModel(player.Position, player.Camera.rotation, role, x, y, z);
				}
				return $"Succesfully spawned a dummy model of {role} at all players.";
			}
			Player ply = args[1].GetPlayer();
			Map.SpawnDummyModel(ply.Position, ply.Camera.rotation, role, x, y, z);
			return $"Succesfully spawned a dummy model of {role} at {ply.Nick}";
		}
	}

	public class CommandWorkbench : Command
	{
		public string Usage => "Missing arguments!\nUsage: swp <player/*> <x> <y> <z>";
		public bool OverwriteBaseGameCommand => false;

		public string OnCall(Player sender, string[] args)
		{
			if (args.Length < 5)
				return Usage;
			if (!float.TryParse(args[2], out float x) || !float.TryParse(args[3], out float y) || !float.TryParse(args[4], out float z))
				return "Please specify a valid size!";
			if (args[1] == "*")
			{
				foreach (Player player in Server.Players)
				{
					Map.SpawnWorkbench(player.Position, player.Camera.rotation.eulerAngles, new Vector3(x, y, z));
				}
				return "Succesfully spawned a workbench at all players.";
			}
			Player ply = args[1].GetPlayer();
			Map.SpawnWorkbench(ply.Position, ply.Camera.rotation.eulerAngles, new Vector3(x, y, z));
			return $"Succesfully spawned a workbench at {ply.Nick}";
		}
	}

	public class CommandRocket : Command
	{
		public string Usage => "Missing arguments!\nUsage: rocket <player/*> <speed>";
		public bool OverwriteBaseGameCommand => false;

		public string OnCall(Player sender, string[] args)
		{
			if (args.Length < 3)
				return Usage;
			if (!float.TryParse(args[2], out float speed))
				return "Please specify a valid speed value!";
			if (args[1] == "*")
			{
				foreach (Player ply in Server.Players)
				{
					ply.Rocket(speed);
				}
				return $"Succesfully launched all players into space.";
			}
			Player player = args[1].GetPlayer();
			player.Rocket(speed);
			return $"Succesfully launched {player.Nick} into space";
		}
	}

	public class CommandScale : Command
	{
		public string Usage => "Missing arguments!\nUsage: scale <player/*all> <x> <y> <z>";
		public bool OverwriteBaseGameCommand => false;

		public string OnCall(Player sender, string[] args)
		{
			if (args.Length < 5)
				return Usage;
			if (!float.TryParse(args[2], out float x) || !float.TryParse(args[3], out float y) || !float.TryParse(args[4], out float z))
				return "Please specify a valid size!";
			if (args[1] == "*")
			{
				foreach (Player ply in Server.Players)
				{
					ply.SetPlayerScale(x, y, z);
				}
				return "Succesfully changed size of all players.";
			}
			Player player = args[1].GetPlayer();
			player.SetPlayerScale(x, y, z);
			return $"Succesfully changed size of {player.Nick}";
		}
	}
}

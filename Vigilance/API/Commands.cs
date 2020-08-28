using Vigilance.API;
using Vigilance.Extensions;
using System.Linq;
using Vigilance.Enums;
using UnityEngine;

namespace Vigilance.Registered
{
	public class CommandClean : CommandHandler
	{
		public string Command => "clean";
		public string Usage => "Missing arguments!\nUsage: clean <itemId/all/*>";

		public string Aliases => "";

        public string Execute(Player sender, string[] args)
		{
			if (args.Length < 1)
				return Usage;
			if (args[0].ToLower() == "*" || args[0].ToLower() == "all")
			{
				foreach (Pickup pickup in Map.Pickups)
					pickup.Delete();
				return "Succesfully cleared all items.";
			}
			else
			{
				ItemType item = ItemType.None;
				foreach (Pickup pickup in Map.Pickups)
				{
					if ((int)pickup.ItemId == int.Parse(args[0]))
					{
						pickup.Delete();
						item = pickup.ItemId;
					}
				}
				return $"Succesfully cleared all {item}s";
			}
		}
	}

	public class CommandDropAll : CommandHandler
	{
		public string Usage => "Missing arguments!\nUsage: dropall <player/*>";

		public string Command => "dropall";

		public string Aliases => "da";

        public string Execute(Player sender, string[] args)
		{
			if (args.Length < 1)
				return Usage;
			Player player = args[0].GetPlayer();
			player.Hub.inventory.ServerDropAll();
			return $"Succesfully dropped all items for {player.Nick}";
		}
	}

	public class CommandGiveAll : CommandHandler
	{
		public string Usage => $"giveall <itemId>";

		public string Command => "giveall";

		public string Aliases => "ga";

        public string Execute(Player sender, string[] args)
		{
			if (args.Length < 1)
				return Usage;
			if (!int.TryParse(args[0], out int id))
				return "Please specify a valid ItemID!";
			ItemType item = (ItemType)id;
			foreach (Player player in Server.Players)
			{
				player.Hub.inventory.AddNewItem(item);
			}
			return $"Succesfully added a {item} to all players.";
		}
	}

	public class CommandPlayers : CommandHandler
	{
		public string Usage => "";

		public string Command => "players";

		public string Aliases => "list";

        public string Execute(Player sender, string[] args)
		{
			string str = $"Players ({Server.Players.Count()}):\n";
			foreach (Player player in Server.Players)
			{
				str += $"\n {player.ToString()}";
			}
			return str;
		}
	}

	public class CommandTeleport : CommandHandler
	{
		public string Usage => "Missing arguments!\nUsage: tpx <player1/*> <player2>";

		public string Command => "teleport";

		public string Aliases => "tpx tp";

        public string Execute(Player sender, string[] args)
		{
			if (args.Length < 2)
				return Usage;
			Player p = args[1].GetPlayer();
			Vector3 teleportTo = p.Position;
			if (args[0].ToLower() == "*" || args[0].ToLower() == "all")
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

	public class CommandPersonalBroadcast : CommandHandler
	{
		public string Usage => "Missing arguments!\nUsage: pbc <player> <time> <message>";

		public string Command => "personalbroadcast";

		public string Aliases => "pbc";

        public string Execute(Player sender, string[] args)
		{
			if (args.Length < 3)
				return Usage;
			Player player = args[0].GetPlayer();
			int time = int.Parse(args[1]);
			player.Broadcast($"<b><color=red>[Personal]</color></b>\n<b>{sender.Nick}</b>:\n<i>{args.SkipWords(2)}</i>", time);
			return $"Success!\nDuration: {time}\nMessage: {args.SkipWords(2)}\nPlayer: {player.Nick} ({player.UserId})";
		}
	}

	public class CommandAdminChat : CommandHandler
	{
		public string Usage => "Missing arguments!\nUsage: abc <time> <message>";

		public string Command => "adminbroadcast";

		public string Aliases => "abc";

        public string Execute(Player sender, string[] args)
		{
			if (args.Length < 2)
				return Usage;
			foreach (Player admin in Server.Players.Where(h => h.Hub.serverRoles.RemoteAdmin))
			{
				admin.Broadcast($"<b><color=red>[AdminChat]</color></b>\n<b>{sender.Nick}</b>:\n<i>{args.SkipWords(2)}</i>", int.Parse(args[1]));
			}
			return $"Success! Your message has been delivered to {Server.Players.Where(h => h.Hub.serverRoles.RemoteAdmin).Count()} online administrators!\nMessage: {args.SkipWords(2)}\nDuration: {int.Parse(args[1])} seconds.";
		}
	}

	public class CommandClearRagdolls : CommandHandler
	{
		public string Usage => "";

		public string Command => "clearragdolls";

		public string Aliases => "cr clearr";

        public string Execute(Player sender, string[] args)
		{
			foreach (Ragdoll ragdoll in Map.Ragdolls)
			{
				ragdoll.Delete();
			}
			return $"Succesfully deleted all ragdolls.";
		}
	}

	public class CommandReload : CommandHandler
	{
		public string Usage => "";

		public string Command => "reload";

		public string Aliases => "rel re";

        public string Execute(Player sender, string[] args)
		{
			Server.ReloadConfigs();
			PluginManager.Reload();
			return $"Success! Changes will be applied on your server next round.";
		}
	}

	public class CommandRestart : CommandHandler
	{
		public string Usage => "";

		public string Command => "restart";

		public string Aliases => "reset res";

        public string Execute(Player sender, string[] args)
		{
			Server.Restart(true);
			return $"The server is about to restart! Please wait ..";
		}
	}

	public class CommandGrenade : CommandHandler
	{
		public string Usage => "Missing arguments!\nUsage: grenade <player/*>";

		public string Command => "grenade";

		public string Aliases => "fraggrenade grenadefrag fg";

        public string Execute(Player sender, string[] args)
		{
			if (args.Length < 1)
				return Usage;
			if (args[0].ToLower() == "*")
			{
				foreach (Player player in Server.Players)
				{
					Map.SpawnGrenade(player, GrenadeType.FragGrenade);
				}
				return $"Succesfully spawned a frag grenade at all players.";
			}
			Player player1 = args[0].GetPlayer();
			Map.SpawnGrenade(player1, GrenadeType.FragGrenade);
			return $"Succesfully spawned a frag grenade at {player1.Nick}";
		}
	}

	public class CommandFlash : CommandHandler
	{
		public string Usage => "Missing arguments!\nUsage: flash <player/*>";

		public string Command => "flash";

		public string Aliases => "flashgrenade grenadeflash";

        public string Execute(Player sender, string[] args)
		{
			if (args.Length < 1)
				return Usage;
			if (args[0] == "*")
			{
				foreach (Player player in Server.Players)
				{
					Map.SpawnGrenade(player, GrenadeType.FlashGrenade);
				}
				return "Succesfully spawned a flash grenade at all players.";
			}
			Player ply = args[0].GetPlayer();
			Map.SpawnGrenade(ply, GrenadeType.FlashGrenade);
			return $"Succesfully spawned a flash grenade at {ply.Nick}";
		}
	}

	public class CommandBall : CommandHandler
	{
		public string Usage => "Missing arguments!\nUsage: ball <player/*>";

		public string Command => "ball";

		public string Aliases => "018 scp018 spawnball spawn018 spawnscp018";

        public string Execute(Player sender, string[] args)
		{
			if (args.Length < 1)
				return Usage;
			if (args[0] == "*")
			{
				foreach (Player player in Server.Players)
				{
					Map.SpawnGrenade(player, GrenadeType.Scp018);
				}
				return "Succesfully spawned a SCP-018 at all players.";
			}
			Player ply = args[0].GetPlayer();
			Map.SpawnGrenade(ply, GrenadeType.Scp018);
			return $"Succesfully spawned a SCP-018 at {ply.Nick}";
		}
	}

	public class CommandRagdoll : CommandHandler
	{
		public string Usage => "Missing arguments!\nUsage: ragdoll <player/*> <role> <amount>";

		public string Command => "ragdoll";

		public string Aliases => "ra rg";

        public string Execute(Player sender, string[] args)
		{
			if (args.Length < 3)
				return Usage;
			if (!int.TryParse(args[1], out int role))
				return "Please specify a valid role!";
			if (!int.TryParse(args[2], out int amount))
				return "Please specify a valid amount!";
			if (args[0] == "*")
			{
				foreach (Player player in Server.Players)
				{
					Map.SpawnRagdolls(player, role, amount);
				}
				return $"Succesfully spawned {amount} {(amount > 1 ? "ragdolls" : "ragdoll")} of {(RoleType)role} at all players.";
			}
			Player player1 = args[0].GetPlayer();
			Map.SpawnRagdolls(player1, role, amount);
			return $"Succesfully spawned {amount} {(amount > 1 ? "ragdolls" : "ragdoll")} of {(RoleType)role} at {player1.Nick}";
		}
	}

	public class CommandDummy : CommandHandler
	{
		public string Usage => "Missing arguments!\nUsage: dummy <player/*> <role> <x> <y> <z>";

		public string Command => "dummy";

		public string Aliases => "model spawnmodel spawndummy";

        public string Execute(Player sender, string[] args)
		{
			if (args.Length < 5)
				return Usage;
			if (!int.TryParse(args[1], out int roleId))
				return "Please specify a valid role!";
			if (!float.TryParse(args[2], out float x) || !float.TryParse(args[3], out float y) || !float.TryParse(args[4], out float z))
				return "Please specify a valid size!";
			RoleType role = (RoleType)roleId;
			if (args[0] == "*")
			{
				foreach (Player player in Server.Players)
				{
					Map.SpawnDummyModel(player.Position, player.Hub.PlayerCameraReference.rotation, role, x, y, z);
				}
				return $"Succesfully spawned a dummy model of {role} at all players.";
			}
			Player ply = args[0].GetPlayer();
			Map.SpawnDummyModel(ply.Position, ply.Hub.PlayerCameraReference.rotation, role, x, y, z);
			return $"Succesfully spawned a dummy model of {role} at {ply.Nick}";
		}
	}

	public class CommandWorkbench : CommandHandler
	{
		public string Usage => "Missing arguments!\nUsage: swp <player/*> <x> <y> <z>";

		public string Command => "spawnworkbench";

		public string Aliases => "swb";

        public string Execute(Player sender, string[] args)
		{
			if (args.Length < 4)
				return Usage;
			if (!float.TryParse(args[1], out float x) || !float.TryParse(args[2], out float y) || !float.TryParse(args[3], out float z))
				return "Please specify a valid size!";
			if (args[0] == "*")
			{
				foreach (Player player in Server.Players)
				{
					Map.SpawnWorkbench(player.Position, player.Hub.PlayerCameraReference.rotation.eulerAngles, new Vector3(x, y, z));
				}
				return "Succesfully spawned a workbench at all players.";
			}
			Player ply = args[0].GetPlayer();
			Map.SpawnWorkbench(ply.Position, ply.Hub.PlayerCameraReference.rotation.eulerAngles, new Vector3(x, y, z));
			return $"Succesfully spawned a workbench at {ply.Nick}";
		}
	}

	public class CommandRocket : CommandHandler
	{
		public string Usage => "Missing arguments!\nUsage: rocket <player/*> <speed>";

		public string Command => "rocket";

		public string Aliases => "ro rc";

        public string Execute(Player sender, string[] args)
		{
			if (args.Length < 2)
				return Usage;
			if (!float.TryParse(args[1], out float speed))
				return "Please specify a valid speed value!";
			if (args[0] == "*")
			{
				foreach (Player ply in Server.Players)
				{
					ply.Rocket(speed);
				}
				return $"Succesfully launched all players into space.";
			}
			Player player = args[0].GetPlayer();
			player.Rocket(speed);
			return $"Succesfully launched {player.Nick} into space";
		}
	}

	public class CommandScale : CommandHandler
	{
		public string Usage => "Missing arguments!\nUsage: scale <player/*all> <x> <y> <z>";

		public string Command => "scale";

		public string Aliases => "size resize rsz sc";

        public string Execute(Player sender, string[] args)
		{
			if (args.Length < 4)
				return Usage;
			if (!float.TryParse(args[1], out float x) || !float.TryParse(args[2], out float y) || !float.TryParse(args[3], out float z))
				return "Please specify a valid size!";
			if (args[0] == "*")
			{
				foreach (Player ply in Server.Players)
				{
					ply.SetPlayerScale(x, y, z);
				}
				return "Succesfully changed size of all players.";
			}
			Player player = args[0].GetPlayer();
			player.SetPlayerScale(x, y, z);
			return $"Succesfully changed size of {player.Nick}";
		}
	}

    public class CommandChangeUnit : CommandHandler
    {
		public string Command => "changeunit";

		public string Usage => "Missing arguments!\nUsage: changeunit <player> <new unit>";

		public string Aliases => "changeunit cu chu";

        public string Execute(Player sender, string[] args)
        {
			if (args.Length < 2)
				return Usage;
			Player player = args[0].GetPlayer();
			string unit = args.SkipWords(1);
			player.NtfUnit = unit;
			return $"Succesfully changed unit of {player.Nick}";
        }
    }

	public class CommandDownloadPlugin : CommandHandler
	{
		public string Command => "downloadplugin";
		public string Usage => "Missing arguments!\nUsage: downloadplugin <name> <url>";
		public string Aliases => "dp";

		public string Execute(Player sender, string[] args)
		{
			if (args.Length < 2)
				return Usage;
			string name = args[0];
			string url = args[1];
			sender.RemoteAdminMessage($"Starting download of \"{name}\" from \"{url}\"");
			Paths.DownloadPlugin(url, name);
			return $"Succesfully downloaded and loaded \"{name}\" from \"{url}\"";
		}
	}

	public class CommandDownloadDependency : CommandHandler
	{
		public string Command => "downloaddep";
		public string Usage => "Missing arguments!\nUsage: downloaddep <name> <url>";
		public string Aliases => "dd";

		public string Execute(Player sender, string[] args)
		{
			if (args.Length < 2)
				return Usage;
			string name = args[0];
			string url = args[1];
			sender.RemoteAdminMessage($"Starting download of \"{name}\" from \"{url}\"");
			Paths.DownloadDependency(url, name);
			return $"Succesfully downloaded and loaded \"{name}\" from \"{url}\"";
		}
	}

	public class CommandAddUnit : CommandHandler
	{
		public string Command => "addunit";

		public string Usage => "Missing arguments!\nUsage: addunit <name>";

		public string Aliases => "au";

		public string Execute(Player sender, string[] args)
		{
			if (args.Length < 1)
				return Usage;
			string name = args.Combine();
			Round.AddUnit(name, Respawning.SpawnableTeamType.ChaosInsurgency);
			Round.AddUnit(name, Respawning.SpawnableTeamType.NineTailedFox);
			return $"Succefully added unit \"{name}\"";
		}
	}
}

﻿using Vigilance.API;
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

	public class CommandDropAll : CommandHandler
	{
		public string Usage => "Missing arguments!\nUsage: dropall <player/*>";

		public string Command => "dropall";

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
			
        public string Execute(Player sender, string[] args)
		{
			if (args.Length < 1)
				return Usage;
			ItemType item = (ItemType)int.Parse(args[1]);
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

        public string Execute(Player sender, string[] args)
		{
			string str = $"Players ({Server.Players.Count}):\n";
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

		public string Command => "tpx";

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

		public string Command => "pbc";

        public string Execute(Player sender, string[] args)
		{
			if (args.Length < 3)
				return Usage;
			Player player = args[0].GetPlayer();
			int time = int.Parse(args[1]);
			player.Broadcast(args.SkipWords(2), time);
			return $"Success!\nDuration: {time}\nMessage: {args.SkipWords(3)}\nPlayer: {player.Nick} ({player.UserId})";
		}
	}

	public class CommandAdminChat : CommandHandler
	{
		public string Usage => "Missing arguments!\nUsage: abc <time> <message>";

		public string Command => "abc";

        public string Execute(Player sender, string[] args)
		{
			if (args.Length < 2)
				return Usage;
			foreach (Player admin in Server.Players.Where(h => h.Hub.serverRoles.RemoteAdmin))
			{
				admin.Broadcast($"<b><color=red>[AdminChat]</color></b> {sender.Nick}:\n{args.SkipWords(2)}", int.Parse(args[1]));
			}
			return $"Success! Your message has been delivered to {Server.Players.Where(h => h.Hub.serverRoles.RemoteAdmin).Count()} online administrators!\nMessage: {args.SkipWords(2)}\nDuration: {int.Parse(args[1])} seconds.";
		}
	}

	public class CommandClearRagdolls : CommandHandler
	{
		public string Usage => "";

		public string Command => "clearragdolls";

        public string Execute(Player sender, string[] args)
		{
			foreach (Ragdoll ragdoll in Map.Ragdolls)
			{
				ragdoll.Delete();
			}
			return $"Succesfully deleted all ragdolls.";
		}
	}

	public class CommandReloadConfigs : CommandHandler
	{
		public string Usage => "";

		public string Command => "configr";

        public string Execute(Player sender, string[] args)
		{
			Server.ReloadConfigs();
			return $"Success! Configs will be applied on your server next round.";
		}
	}

	public class CommandRestart : CommandHandler
	{
		public string Usage => "";

		public string Command => "restart";

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

        public string Execute(Player sender, string[] args)
		{
			if (args.Length < 2)
				return Usage;
			if (args[1].ToLower() == "*")
			{
				foreach (Player player in Server.Players)
				{
					Map.SpawnGrenade(player, GrenadeType.FragGrenade);
				}
				return $"Succesfully spawned a frag grenade at all players.";
			}
			Player player1 = args[1].GetPlayer();
			Map.SpawnGrenade(player1, GrenadeType.FragGrenade);
			return $"Succesfully spawned a frag grenade at {player1.Nick}";
		}
	}

	public class CommandFlash : CommandHandler
	{
		public string Usage => "Missing arguments!\nUsage: flash <player/*>";

		public string Command => "flash";

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

        public string Execute(Player sender, string[] args)
		{
			if (args.Length < 5)
				return Usage;
			if (!int.TryParse(args[1], out int roleId))
				return "Please specify a valid role!";
			if (!float.TryParse(args[2], out float x) || !float.TryParse(args[3], out float y) || !float.TryParse(args[4], out float z))
				return "Please specify a valid size!";
			RoleType role = (RoleType)roleId;
			if (args[1] == "*")
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

		public string Command => "swb";

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
}
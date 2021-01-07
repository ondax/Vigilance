using Vigilance.API;
using Vigilance.Extensions;
using System.Linq;
using Vigilance.Enums;
using UnityEngine;
using System.Collections.Generic;
using System;
using Interactables.Interobjects.DoorUtils;

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
				foreach (Pickup pickup in Map.FindObjects<Pickup>())
					pickup.Delete();
				return "Succesfully cleared all items.";
			}
			else
			{
				ItemType item = ItemType.None;
				foreach (Pickup pickup in Map.FindObjects<Pickup>())
				{
					if (pickup.ItemId == args[0].GetItem())
					{
						pickup.Delete();
						item = pickup.ItemId;
					}
				}
				return $"Succesfully cleared all {item}s!";
			}
		}
	}

	public class CommandDropAll : CommandHandler
	{
		public string Usage => "Missing arguments!\nUsage: dropall <player>";

		public string Command => "dropall";

		public string Aliases => "da";

        public string Execute(Player sender, string[] args)
		{
			if (args.Length < 1)
				return Usage;
			Player player = args[0].GetPlayer();
			if (player == null)
				return "An error occured: Player is null.";
			player.DropAllItems();
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
			ItemType item = args[0].GetItem();
			if (item == ItemType.None)
				return "Please specify a valid ItemID!";
			foreach (Player player in Server.Players)
				player.AddItem(item);
			return $"Succesfully added a {item} to all players.";
		}
	}

	public class CommandPlayers : CommandHandler
	{
		public string Usage => "";

		public string Command => "players";

		public string Aliases => "plys";

        public string Execute(Player sender, string[] args)
		{
			if (Server.Players.Count() == 0)
				return "No players online.";
			string str = $"Players ({Server.Players.Count()}):\n";
			List<Player> plys = Server.Players.ToList();
			IOrderedEnumerable<Player> players = plys.OrderBy(s => s.PlayerId);
			foreach (Player player in players)
			{
				str += $"{player.ToString()}\n";
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
			if (p == null)
				return "An error occured: Player is null.";
			Vector3 teleportTo = p.Position;
			if (args[0].ToLower() == "*" || args[0].ToLower() == "all")
			{
				foreach (Player ply in Server.Players)
				{
					ply.Teleport(teleportTo);
				}
				return $"Succesfully teleported all players to {p.Nick}";
			}
			Player player = args[0].GetPlayer();
			if (player == null)
				return "An error occured: Player is null.";
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
			if (player == null)
				return "An error occured: Player is null.";
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
			IEnumerable<Player> admins = Server.PlayerList.Players.Values.Where(h => h.RemoteAdmin);
			string message = args.SkipWords(1);
			foreach (Player admin in admins)
			{
				admin.Broadcast($"<b><color=red>[AdminChat]</color></b>\n<b>{sender.Nick}</b>:\n<i>{message}</i>", int.Parse(args[0]));
			}
			return $"Success! Your message has been delivered to {admins.Count()} online administrators!\nMessage: {message}\nDuration: {int.Parse(args[0])} seconds.";
		}
	}

	public class CommandClearRagdolls : CommandHandler
	{
		public string Usage => "";

		public string Command => "clearragdolls";

		public string Aliases => "cr clearr";

        public string Execute(Player sender, string[] args)
		{
			foreach (Ragdoll ragdoll in Map.FindObjects<Ragdoll>())
				ragdoll.Delete();
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
			Patches.Events.Restart.ServerRestart();
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
					Map.SpawnGrenade(player, GrenadeType.FragGrenade);
				return $"Succesfully spawned a frag grenade at all players.";
			}
			Player player1 = args[0].GetPlayer();
			if (player1 == null)
				return "An error occured: Player is null.";
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
					Map.SpawnGrenade(player, GrenadeType.FlashGrenade);
				return "Succesfully spawned a flash grenade at all players.";
			}
			Player ply = args[0].GetPlayer();
			if (ply == null)
				return "An error occured: Player is null.";
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
					Map.SpawnGrenade(player, GrenadeType.Scp018);
				return "Succesfully spawned a SCP-018 at all players.";
			}
			Player ply = args[0].GetPlayer();
			if (ply == null)
				return "An error occured: Player is null.";
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
					Map.SpawnRagdolls(player, role, amount);
				return $"Succesfully spawned {amount} {(amount > 1 ? "ragdolls" : "ragdoll")} of {(RoleType)role} at all players.";
			}
			Player player1 = args[0].GetPlayer();
			if (player1 == null)
				return "An error occured: Player is null.";
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
					Map.SpawnDummyModel(player.Position, player.RotationQuaternion, role, x, y, z);
				}
				return $"Succesfully spawned a dummy model of {role} at all players.";
			}
			Player ply = args[0].GetPlayer();
			if (ply == null)
				return "An error occured: Player is null.";
			Map.SpawnDummyModel(ply.Position, ply.RotationQuaternion, role, x, y, z);
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
					Prefab.WorkStation.Spawn(player.Position, player.RotationQuaternion, new Vector3(x, y, z));
				}
				return "Succesfully spawned a workbench at all players.";
			}
			Player ply = args[0].GetPlayer();
			if (ply == null)
				return "An error occured: Player is null.";
			Prefab.WorkStation.Spawn(ply.Position, ply.RotationQuaternion, new Vector3(x, y, z));
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
					if (ply.IsAlive)
						Environment.Cache.Rocket(ply, speed);
				}
				return $"Succesfully launched all players into space.";
			}
			Player player = args[0].GetPlayer();
			if (player == null)
				return "An error occured: Player is null.";
			if (!player.IsAlive)
				return $"{player.Nick} is a spectator!";
			Environment.Cache.Rocket(player, speed);
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
					ply.Scale = new Vector3(x, y, z);
				}
				return "Succesfully changed size of all players.";
			}
			Player player = args[0].GetPlayer();
			if (player == null)
				return "An error occured: Player is null.";
			player.Scale = new Vector3(x, y, z);
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
			if (player == null)
				return "An error occured: Player is null.";
			string unit = args.SkipWords(1);
			player.NtfUnit = unit;
			return $"Succesfully set unit of {player.Nick} to {player.NtfUnit}";
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

	public class CommandUnban : GameCommandHandler
	{
		public string Command => "unban";

		public string Usage => "Missing arguments!\nUsage: unban <nick/userId>";

		public string Aliases => "uban";

		public string Execute(Player sender, string[] args)
		{
			if (args.Length < 1)
				return Usage;
			string nick = args[0].ToLower();
			long expiery = 0;
			List<BanDetails> IpBans = BanHandler.GetBans(BanHandler.BanType.IP);
			List<BanDetails> UserIdBans = BanHandler.GetBans(BanHandler.BanType.UserId);

			if (ulong.TryParse(nick, out ulong id))
			{
				string ply = "";
				foreach (BanDetails ban in UserIdBans)
				{
					if (ban.Id == $"{id.ToString()}@steam" || ban.Id == $"{id.ToString()}@discord" || ban.Id.Contains(id.ToString()))
					{
						ply = ban.OriginalName;
						expiery = ban.Expires;
						BanHandler.RemoveBan(ban.Id, BanHandler.BanType.UserId);
						Log.Add("SERVER", $"Removed UserIDBan:\nNick: {ban.OriginalName}\nUserID: {ban.Id}\nIssued by: {ban.Issuer}", LogType.Info);
						sender.RemoteAdminMessage($"Succefully unbanned UserID: {ban.Id} [{ban.OriginalName}]");
					}
				}

				foreach (BanDetails ip in IpBans)
				{
					if (ip.OriginalName == ply && ip.Expires == expiery || ip.Id == id.ToString() || ip.Id.Contains(id.ToString()) || ip.Id == $"{id}@steam" || ip.Id == $"{id}@discord")
					{
						BanHandler.RemoveBan(ip.Id, BanHandler.BanType.IP);
						Log.Add("SERVER", $"Removed IPBan:\nNick: {ip.OriginalName}\nIP: {ip.Id}\nIssued by: {ip.Issuer}", LogType.Info);
						sender.RemoteAdminMessage($"Succesfully unbanned IP: {ip.Id} [{ip.OriginalName}]");
					}
				}
			}

			foreach (BanDetails IpBan in IpBans)
			{
				if (IpBan.OriginalName == nick || IpBan.OriginalName.ToLower() == nick || IpBan.OriginalName.ToLower().Contains(nick))
				{
					nick = IpBan.OriginalName;
					BanHandler.RemoveBan(IpBan.Id, BanHandler.BanType.IP);
					Log.Add("SERVER", $"Removed IPBan:\nNick: {IpBan.OriginalName}\nIP: {IpBan.Id}\nIssued by: {IpBan.Issuer}", LogType.Info);
					sender.RemoteAdminMessage($"Succesfully unbanned IP: {IpBan.Id} [{IpBan.OriginalName}]");
				}
			}

			foreach (BanDetails UserIdBan in UserIdBans)
			{
				if (UserIdBan.OriginalName == nick || UserIdBan.OriginalName.ToLower() == nick || UserIdBan.OriginalName.ToLower().Contains(nick))
				{
					nick = UserIdBan.OriginalName;
					BanHandler.RemoveBan(UserIdBan.Id, BanHandler.BanType.UserId);
					Log.Add("SERVER", $"Removed UserIDBan:\nNick: {UserIdBan.OriginalName}\nUserID: {UserIdBan.Id}\nIssued by: {UserIdBan.Issuer}", LogType.Info);
					return $"Succesfully unbanned UserID: {UserIdBan.Id} [{UserIdBan.OriginalName}]";
				}
			}
			return $"";
		}
	}

	public class CommandOban : GameCommandHandler
	{
		public string Command => "offlineban";

		public string Usage => "Missing arguments!\nUsage: oban <UserID/IP> <Duration> <DurationType> <Reason>";

		public string Aliases => "oban";

		public string Execute(Player sender, string[] args)
		{
			if (args.Length < 3)
				return Usage;
			string id = args[0];
			if (!int.TryParse(args[1], out int duration))
				return "Please provide a valid duration!";
			DurationType durationType = args[2].GetDurationType();
			string reason = args.SkipWords(3);
			reason = string.IsNullOrEmpty(reason) ? "No reason provided." : reason;
			UserIdType userIdType;
			if (ulong.TryParse(id, out ulong userId))
			{
				userIdType = userId.GetIdType();
				if (userIdType == UserIdType.Unspecified)
					return "Provide a valid UserID.";
				string Id = userIdType == UserIdType.Steam ? $"{id}@steam" : $"{id}@discord";
				Server.IssueOfflineBan(durationType, duration, Id, sender.Nick, reason);
				return $"Succesfully banned UserID: \"{Id}\" for {durationType.GetDurationTypeString(duration)}. Reason: {reason}";
			}
			else
			{
				Server.IssueOfflineBan(durationType, duration, id, sender.Nick, reason);
				return $"Succesfully banned IP: \"{id}\" for {durationType.GetDurationTypeString(duration)}. Reason: {reason}";
			}
		}
	}

    public class CommandPos : CommandHandler
    {
		public string Command => "position";

		public string Usage => "position <add/get/set> <player> <value>";

		public string Aliases => "pos";

		public string Execute(Player sender, string[] args)
		{
			if (args.Length < 2)
				return Usage;
			Player player = args[1].GetPlayer();
			if (player == null)
				return "An error occured: Player is null.";
			switch (args[0].ToLower())
			{
				case "get":
					return $"Position of {player.Nick}: \n[X: {player.Position.x} | Y: {player.Position.y} | Z: {player.Position.z}]\nZone: {player.CurrentRoom.Zone}\nRoom: {player.CurrentRoom.Type}\nPocket Dimension?: {player.IsInPocketDimension.ToString().ToLower()}";
				case "set":
					Vector3 pos = CommandPos.ParsePosition(args[2]);
					player.Teleport(pos);
					return $"Succesfully updated {player.Nick}'s position. [X: {pos.x} | Y: {pos.y} | Z: {pos.z}]";
				case "add":
					Vector3 newPos = CommandPos.ParseAdd(args[2], player);
					player.Hub.playerMovementSync.OverridePosition(newPos, sender.Hub.transform.position.y);
					return $"Succesfully updated {player.Nick}'s position. [X: {newPos.x} | Y: {newPos.y} | Z: {newPos.z}]";
				default:
					return "Please specify a valid operation! [get/set/add]";
			}
		}

		public static Vector3 ParsePosition(string s)
        {
			try
			{
				string[] args = s.Split('=');
				if (!s.Contains("=") || args.Length < 3 || !float.TryParse(args[0], out float x) || !float.TryParse(args[1], out float y) || !float.TryParse(args[2], out float z))
                {
					Log.Add($"Vigilance.Registered.CommandPos.ParsePosition", $"Input string is not in correct format!", LogType.Error);
					return Vector3.zero;
                }
				return new Vector3(float.Parse(args[0]), float.Parse(args[1]), float.Parse(args[2]));
			}
			catch (Exception e)
            {
				Log.Add($"Vigilance.Registered.CommandPos.ParsePosition", $"Input string is not in correct format!", LogType.Error);
				Log.Add($"Vigilance.Registered.CommandPos.ParsePosition", e.Message, LogType.Error);
				return Vector3.zero;
            }
        }

		public static Vector3 ParseAdd(string s, Player player)
        {
			Vector3 curPos = player.Position;
			string[] args = s.Split('=');
			if (!float.TryParse(args[1], out float value))
				return curPos;
			if (args[0].ToLower() == "x")
				curPos.x += value;
			if (args[0].ToLower() == "y")
				curPos.y += value;
			if (args[0].ToLower() == "z")
				curPos.z += value;
			return curPos;
        }
    }

    public class CommandGhost : CommandHandler
    {
		public string Command => "ghost";

		public string Usage => "ghost <player>";

		public string Aliases => "invis gh";

        public string Execute(Player sender, string[] args)
        {
			if (args.Length < 1)
				return Usage;
			Player player = args[0].GetPlayer();
			if (player == null)
				return "An error occured: Player is null.";
			bool value = !player.IsInvisible;
			player.IsInvisible = value;
			return $"Succesfully set ghostmode of {player.Nick} to {value.ToString().ToLower()}";
        }
    }

    public class CommandTargetGhost : CommandHandler
    {
		public string Command => "targetghost";

		public string Usage => "Missing arguments!\nUsage: targetghost <player> <playerThatFirstPlayerCannotSee>";

		public string Aliases => "tghost targetinvis tinvis tg";

        public string Execute(Player sender, string[] args)
        {
            if (args.Length < 2)
				return Usage;
			Player player = args[0].GetPlayer();
			if (player == null)
				return "An error occured: Player is null.";
			Player two = args[1].GetPlayer();
			if (two == null)
				return "An error occured: Player is null.";
			if (Ghostmode.GetTargets(player).Contains(two))
            {
				Ghostmode.RemoveTarget(player, two);
				return $"Succesfully removed {two.Nick} from {player.Nick}'s target ghosts.";
            }
			else
            {
				Ghostmode.AddTarget(player, two);
				return $"Succesfully added {two.Nick} to {player.Nick}'s target ghosts.";
            }
        }
    }

	public class CommandSpawnPrefab : CommandHandler
    {
		public string Command => "spawnprefab";

		public string Usage => "spawnprefab <player/*> <scale [X=Y=Z]> <prefab name>";

		public string Aliases => "sp";

        public string Execute(Player sender, string[] args)
        {
			if (args.Length < 3)
				return Usage;
			Prefab prefab = args.SkipWords(2).GetPrefab();
			if (prefab == Prefab.None)
				return $"An error occured: Prefab is none.";
			Vector3 scale = CommandPos.ParsePosition(args[1]);
			if (args[0].ToLower() == "all" || args[0] == "*")
            {
				foreach (Player ply in Server.Players)
                {
					if (ply.IsAlive)
                    {
						prefab.Spawn(ply.Position, ply.RotationQuaternion, scale);
                    }
                }
				return $"Succesfully spawned {prefab} at all players.";
            }
			else
            {
				Player player = args[0].GetPlayer();
				if (player == null)
					return "An error occured: Player is null.";
				if (player.IsAlive)
				{
					prefab.Spawn(player.Position, player.RotationQuaternion, scale);
					return $"Succesfully spawned {prefab} at {player.Nick}";
				}
				else
                {
					return $"{player.Nick} is a spectator!";
                }
            }
        }
    }

    public class CommandExplode : CommandHandler
    {
        public string Command => "explode";

		public string Usage => "explode <player/*> <force = 1>";

		public string Aliases => "";

        public string Execute(Player sender, string[] args)
        {
			if (args.Length < 1)
				return Usage;
			float force;
			if (!float.TryParse(args[1], out force))
				force = 1f;
			if (args[0] == "*")
            {
				foreach (Player player in Server.Players)
                {
					if (player.IsAlive)
						player.Explode(force);
                }
				return $"Succesfully spawned an explosion at all players.";
            }
			else
            {
				Player player = args[0].GetPlayer();
				if (player == null)
					return "An error occured: Player is null.";
				if (player.IsAlive)
				{
					player.Explode(force);
					return $"Succesfully spawned an explosion at {player.Nick}";
				}
				else
					return $"{player.Nick} is a spectator!";
            }
        }
    }

    public class CommandDecontaminate : CommandHandler
    {
		public string Command => "decontaminate";
		public string Usage => "";
		public string Aliases => "decont startdecont";

        public string Execute(Player sender, string[] args)
        {
			Map.Decontamination.Decontaminate();
			return "Decontamination has begun.";
        }
    }

    public class CommandForceEnd : CommandHandler
    {
		public string Command => "forceend";
		public string Usage => "";
		public string Aliases => "fe";

        public string Execute(Player sender, string[] args)
        {
			if (RoundSummary.roundTime < 5)
				return "The round has not been started yet!";
			RoundSummary.RoundLock = false;
			RoundSummary.singleton.ForceEnd();
			return "Done! Forced round end.";
        }
    }

    public class CommandAddReservedSlot : CommandHandler
    {
		public string Command => "addslot";
		public string Usage => "Missing arguments!\nUsage: addslot <UserId>";
		public string Aliases => "as";

        public string Execute(Player sender, string[] args)
        {
			if (args.Length < 1)
				return Usage;
			string userId = args[0];
			if (Server.AddReservedSlot(userId))
            {
				ReservedSlot.Reload();
				return $"Succesfully added a reserved slot for {userId}.";
            }
			else
            {
				return $"An error occured while adding a slot for {userId}";
            }
        }
    }

    public class CommandListCommands : CommandHandler
    {
		public string Command => "listcommands";
		public string Usage => "";
		public string Aliases => "lc cmds commands";

        public string Execute(Player sender, string[] args)
        {
			IEnumerable<CommandHandler> commands = CommandManager.Commands.Values;
			string str = $"Commands ({commands.Count()}):\n";
			foreach (CommandHandler commandHandler in commands)
				str += $"{commandHandler.Command}\n";
			return str;
        }
    }

    public class CommandHelpCommand : CommandHandler
    {
		public string Command => "helpcommand";
		public string Usage => "Missing arguments!\nUsage: helpcommand <command>";
		public string Aliases => "helpcmd cmd";

        public string Execute(Player sender, string[] args)
        {
			if (args.Length < 1)
				return Usage;
			string command = args[0];
			CommandHandler commandHandler = CommandManager.GetCommandHandler(command);
			if (commandHandler == null)
				return "Cannot find that command.";
			else
            {
				string usage = string.IsNullOrEmpty(commandHandler.Usage.Replace("Missing arguments!\nUsage: ", "")) ? "None" : commandHandler.Usage.Replace("Missing arguments!\nUsage: ", "");
				string aliases = string.IsNullOrEmpty(commandHandler.Aliases) ? "None" : commandHandler.Aliases.Replace(" ", ", ");
				string str = $"Command: \"{commandHandler.Command}\"\nUsage: {usage}\nAliases: {aliases}";
				return str;
            }
        }
    }

    public class CommandHint : CommandHandler
    {
		public string Command => "hint";
		public string Usage => "Missing arguments!\nUsage: hint <time> <message>";
		public string Aliases => "";

        public string Execute(Player sender, string[] args)
        {
			if (args.Length < 2)
				return Usage;
			string message = args.SkipWords(1);
			if (!int.TryParse(args[0], out int duration))
				return "Please provide a valid duration.";
			Map.ShowHint(message, duration);
			return "Succesfully sent a hint.";
        }
    }

    public class CommandPersonalHint : CommandHandler
    {
		public string Command => "personalhint";
		public string Usage => "Missing arguments!\nUsage: personalhint <player> <time> <message>";
		public string Aliases => "phint";

        public string Execute(Player sender, string[] args)
        {
			if (args.Length < 3)
				return Usage;
			if (!int.TryParse(args[1], out int duration))
				return "Please provide a valid duration.";
			Player player = args[0].GetPlayer();
			if (player == null)
				return "An error occured: Player is null.";
			string message = args.SkipWords(2);
			string str = $"<color=#FF0000>[PERSONAL] - {sender.Nick}:</color>\n<i>{message}</i>";
			player.ShowHint(str, duration);
			return $"Succesfully shown a hint to {player.Nick}";
        }
    }

    public class CommandAchieve : CommandHandler
    {
		public string Command => "achieve";
		public string Usage => "Missing arguments!\nUsage: achieve <target> <achievement>";
		public string Aliases => "";

        public string Execute(Player sender, string[] args)
        {
			if (args.Length < 2)
				return Usage;
			Player player = args[0].GetPlayer();
			if (player == null)
				return "An error occured: Player is null.";
			Achievement achievement = args[1].GetAchievement();
			if (achievement == Achievement.Unknown)
				return "Cannot find an achievement with that name.";
			player.Achieve(achievement);
			return $"{player.Nick} succesfuly achieved {achievement}";
        }
    }

    public class CommandItemSize : CommandHandler
    {
		public string Command => "itemsize";
		public string Usage => "Missing arguments!\nUsage: itemsize <player> <ItemID> <scale [X=Y=Z]>";
		public string Aliases => "it";

        public string Execute(Player sender, string[] args)
        {
			if (args.Length < 3)
				return Usage;
			Player player = args[0].GetPlayer();
			if (player == null)
				return "An error occured: Player is null.";
			ItemType type = args[1].GetItem();
			Vector3 pos = CommandPos.ParsePosition(args[2]);
			if (pos == Vector3.zero)
				return "An error occured: Vector is null.";
			GameObject obj = Prefab.Pickup.Spawn(player.Position, sender.RotationQuaternion, pos);
			Pickup pickup = obj.GetComponent<Pickup>();
			if (pickup != null)
			{
				pickup.SetIDFull(type);
				return $"Succesfully spawned a {type} at {player.Nick}";
			}
			else
				return "An error occured: Pickup is null.";
        }
    }

    public class CommandList : CommandHandler
    {
		public string Command => "list";
		public string Usage => "Missing arguments!\nUsage: list <item/rid/role/damagetype/durationtype/grenadetype/useridtype/teamtype/team/zonetype/roomtype/weapontype/ammotype/prefab/achievement/roominfo/infoarea>";
		public string Aliases => "";

        public string Execute(Player sender, string[] args)
        {
			if (args.Length < 1)
				return Usage;
			string s = "";
			if (args[0].ToLower() == "item")
            {
				foreach (ItemType item in Environment.GetValues<ItemType>())
                {
					if (string.IsNullOrEmpty(s))
                    {
						s += $"\n";
                    }
					s += $"({(int)item}) {item}\n";
                }
				return s;
            }

			if (args[0].ToLower() == "rid")
			{
				foreach (Rid item in Map.RoomIDs)
				{
					if (string.IsNullOrEmpty(s))
					{
						s += $"\n";
					}
					s += $"({item.name}) {item.id}\n";
				}
				return s;
			}

			if (args[0].ToLower() == "role")
            {
				foreach (RoleType item in Environment.GetValues<RoleType>())
				{
					if (string.IsNullOrEmpty(s))
					{
						s += $"\n";
					}
					s += $"({(int)item}) {item}\n";
				}
				return s;
			}

			if (args[0].ToLower() == "damagetype")
			{
				foreach (DamageType item in Environment.GetValues<DamageType>())
				{
					if (string.IsNullOrEmpty(s))
					{
						s += $"\n";
					}
					s += $"({(int)item}) {item}\n";
				}
				return s;
			}

			if (args[0].ToLower() == "durationtype")
			{
				foreach (DurationType item in Environment.GetValues<DurationType>())
				{
					if (string.IsNullOrEmpty(s))
					{
						s += $"\n";
					}
					s += $"({(int)item}) {item}\n";
				}
				return s;
			}

			if (args[0].ToLower() == "grenadetype")
			{
				foreach (GrenadeType item in Environment.GetValues<GrenadeType>())
				{
					if (string.IsNullOrEmpty(s))
					{
						s += $"\n";
					}
					s += $"({(int)item}) {item}\n";
				}
				return s;
			}

			if (args[0].ToLower() == "useridtype")
			{
				foreach (UserIdType item in Environment.GetValues<UserIdType>())
				{
					if (string.IsNullOrEmpty(s))
					{
						s += $"\n";
					}
					s += $"({(int)item}) {item}\n";
				}
				return s;
			}

			if (args[0].ToLower() == "teamtype")
			{
				foreach (TeamType item in Environment.GetValues<TeamType>())
				{
					if (string.IsNullOrEmpty(s))
					{
						s += $"\n";
					}
					s += $"({(int)item}) {item}\n";
				}
				return s;
			}

			if (args[0].ToLower() == "team")
			{
				foreach (Team item in Environment.GetValues<Team>())
				{
					if (string.IsNullOrEmpty(s))
					{
						s += $"\n";
					}
					s += $"({(int)item}) {item}\n";
				}
				return s;
			}

			if (args[0].ToLower() == "zonetype")
			{
				foreach (ZoneType item in Environment.GetValues<ZoneType>())
				{
					if (string.IsNullOrEmpty(s))
					{
						s += $"\n";
					}
					s += $"({(int)item}) {item}\n";
				}
				return s;
			}

			if (args[0].ToLower() == "roomtype")
			{
				foreach (RoomType item in Environment.GetValues<RoomType>())
				{
					if (string.IsNullOrEmpty(s))
					{
						s += $"\n";
					}
					s += $"({(int)item}) {item}\n";
				}
				return s;
			}

			if (args[0].ToLower() == "weapontype")
			{
				foreach (WeaponType item in Environment.GetValues<WeaponType>())
				{
					if (string.IsNullOrEmpty(s))
					{
						s += $"\n";
					}
					s += $"({(int)item}) {item}\n";
				}
				return s;
			}

			if (args[0].ToLower() == "ammotype")
			{
				foreach (AmmoType item in Environment.GetValues<AmmoType>())
				{
					if (string.IsNullOrEmpty(s))
					{
						s += $"\n";
					}
					s += $"({(int)item}) {item}\n";
				}
				return s;
			}

			if (args[0].ToLower() == "prefab")
			{
				foreach (Prefab item in Environment.GetValues<Prefab>())
				{
					if (string.IsNullOrEmpty(s))
					{
						s += $"\n";
					}
					s += $"({(int)item}) {item.GetName()}\n";
				}
				return s;
			}

			if (args[0].ToLower() == "achievement")
			{
				foreach (Achievement item in Environment.GetValues<Achievement>())
				{
					if (string.IsNullOrEmpty(s))
					{
						s += $"\n";
					}
					s += $"({(int)item}) {item}\n";
				}
				return s;
			}

			if (args[0].ToLower() == "roominfo")
			{
				foreach (RoomInformation item in Map.RoomList)
				{
					if (string.IsNullOrEmpty(s))
					{
						s += $"\n";
					}
					s += $"({item.tag}) {item.name}\n";
				}
				return s;
			}

			if (args[0].ToLower() == "infoarea")
			{
				foreach (PlayerInfoArea item in Environment.GetValues<PlayerInfoArea>())
				{
					if (string.IsNullOrEmpty(s))
					{
						s += $"\n";
					}
					s += $"({(int)item}) {item}\n";
				}
				return s;
			}
			return Usage;
		}
    }

    public class CommandTpRoom : CommandHandler
    {
		public string Command => "tproom";
		public string Usage => "Missing arguments!\nUsage: tproom <player/*> <room>";
		public string Aliases => "tr";

        public string Execute(Player sender, string[] args)
        {
			if (args.Length < 1)
				return Usage;
			Room room = Map.GetRoom(args[1]);
			Rid rid = Map.GetRoomID(args[1]);
			RoomInformation info = Map.GetRoomInformation(args[1]);
			if (args[0].ToLower() == "*")
            {
				if (room == null)
                {
					if (rid != null)
                    {
						foreach (Player player in Server.Players)
                        {
							player.Teleport(rid);
                        }
						return $"Succesfully teleported all players to {rid.id}";
                    }
					else
                    {
						if (info != null)
                        {
							foreach (Player player in Server.Players)
							{
								player.Teleport(rid);
							}
							return $"Succesfully teleported all players to {info.name}";
						}
                    }
                }
				else
                {
					foreach (Player player in Server.Players)
                    {
						player.Teleport(room);
                    }
					return $"Succesfully teleported all players to {room.Name}";
                }
            }
			else
            {
				Player player = args[0].GetPlayer();
				if (player == null)
					return "An error occured: Player is null.";
				if (room == null)
				{
					if (rid != null)
					{
						player.Teleport(rid);
						return $"Succesfully teleported {player.Nick} to {rid.id}";
					}
					else
                    {
						if (info != null)
                        {
							player.Teleport(Environment.FindSafePosition(info.transform.position));
							return $"Succesfully teleported {player.Nick} to {info.name}";
						}
                    }
				}
				else
				{
					player.Teleport(room);
					return $"Succesfully teleported {player.Nick} to {room.Name}";
				}
			}
			return Usage;
        }
    }
}

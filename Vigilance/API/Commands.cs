﻿using Vigilance.API;
using Vigilance.Extensions;
using System.Linq;
using Vigilance.Enums;
using UnityEngine;
using System.Collections.Generic;
using System;

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
					if (pickup.ItemId == args[0].GetItem())
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
		public string Usage => "Missing arguments!\nUsage: dropall <player>";

		public string Command => "dropall";

		public string Aliases => "da";

        public string Execute(Player sender, string[] args)
		{
			if (args.Length < 1)
				return Usage;
			Player player = args[0].GetPlayer();
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

		public string Aliases => "list";

        public string Execute(Player sender, string[] args)
		{
			if (Server.Players.Count() == 0)
				return "No players.";
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
			IEnumerable<Player> admins = Server.PlayerList.PlayersDict.Values.Where(h => h.RemoteAdmin);
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
			foreach (Ragdoll ragdoll in Map.Ragdolls)
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
					Map.SpawnGrenade(player, GrenadeType.FragGrenade);
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
					Map.SpawnGrenade(player, GrenadeType.FlashGrenade);
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
					Map.SpawnGrenade(player, GrenadeType.Scp018);
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
					Map.SpawnRagdolls(player, role, amount);
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
					Map.SpawnDummyModel(player.Position, player.RotationQuaternion, role, x, y, z);
				}
				return $"Succesfully spawned a dummy model of {role} at all players.";
			}
			Player ply = args[0].GetPlayer();
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
						ply.Rocket(speed);
				}
				return $"Succesfully launched all players into space.";
			}
			Player player = args[0].GetPlayer();
			if (!player.IsAlive)
				return $"{player.Nick} is a spectator!";
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
					ply.Scale = new Vector3(x, y, z);
				}
				return "Succesfully changed size of all players.";
			}
			Player player = args[0].GetPlayer();
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

	public class UnbanCommand : GameCommandHandler
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

	public class OfflineBanCommand : GameCommandHandler
	{
		public string Command => "offlineban";

		public string Usage => "oban <UserID/IP> <Duration> <DurationType> <Reason>";

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
			if (args.Length == 2)
            {
				switch (args[0].ToLower())
                {
					case "get":
						return $"Your position: \n[X: {sender.Position.x} | Y: {sender.Position.y} | Z: {sender.Position.z}]\nZone: {sender.CurrentRoom.Zone}\nRoom: {sender.CurrentRoom.Type}\nPocket Dimension?: {sender.IsInPocketDimension.ToString().ToLower()}";
					case "set":
						Vector3 pos = CommandPos.ParsePosition(args[1]);
						sender.Teleport(pos);
						return $"Succesfully updated your position. [X: {pos.x} | Y: {pos.y} | Z: {pos.z}]";
					case "add":
						Vector3 newPos = CommandPos.ParseAdd(args[1], sender);
						sender.Hub.playerMovementSync.OverridePosition(newPos, sender.Hub.transform.position.y);
						return $"Succesfully updated your position. [X: {newPos.x} | Y: {newPos.y} | Z: {newPos.z}]";
					default:
						return "Please specify a valid operation! [get/set/add]";
                }
            }
			else 
            {
				Player player = args[1].GetPlayer();
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
        }

		public static Vector3 ParsePosition(string s)
        {
			string[] args = s.Split('=');
			return new Vector3(float.Parse(args[0]), float.Parse(args[1]), float.Parse(args[2]));
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

    public class GhostCommand : CommandHandler
    {
		public string Command => "ghost";

		public string Usage => "ghost <player>";

		public string Aliases => "invis gh";

        public string Execute(Player sender, string[] args)
        {
			if (args.Length < 1)
				return Usage;
			Player player = args[0].GetPlayer();
			bool value = !player.IsInvisible;
			player.IsInvisible = value;
			return $"Succesfully set ghostmode of {player.Nick} to {value.ToString().ToLower()}";
        }
    }

    public class TargetGhostCommand : CommandHandler
    {
		public string Command => "targetghost";

		public string Usage => "Missing arguments!\nUsage: targetghost <player> <playerThatFirstPlayerCannotSee>";

		public string Aliases => "tghost targetinvis tinvis tg";

        public string Execute(Player sender, string[] args)
        {
            if (args.Length < 2)
				return Usage;
			Player player = args[0].GetPlayer();
			if (!Server.PlayerList.TargetGhosts.ContainsKey(player.UserId))
				Server.PlayerList.TargetGhosts.Add(player.UserId, new List<int>());
			Player two = args[1].GetPlayer();
			if (Server.PlayerList.TargetGhosts[player.UserId].Contains(two.PlayerId))
            {
				Server.PlayerList.TargetGhosts[player.UserId].Remove(two.PlayerId);
				return $"Succesfully removed {two.Nick} from {player.Nick}'s target ghosts.";
            }
			else
            {
				Server.PlayerList.TargetGhosts[player.UserId].Add(two.PlayerId);
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
				return $"NetworkManager was unable to find a prefab with that name.";
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
			if (Round.CurrentState != RoundState.Started)
				return "The round has not been started yet!";
			Round.End();
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
				GameCore.ConfigFile.ReloadGameConfigs();
				return $"Succesfully added a reserved slot for {userId}.";
            }
			else
            {
				return $"An error occured while adding a slot for {userId}";
            }
        }
    }
}

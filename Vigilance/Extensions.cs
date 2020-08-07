﻿using Mirror;
using UnityEngine;
using Vigilance.Enums;
using System.Collections.Generic;
using System;
using RemoteAdmin;
using System.Linq;
using System.Reflection;
using System.Text;
using Vigilance.API;

namespace Vigilance.Extensions
{
	public static class GameObjectExtensions
	{
		public static bool IsNull(this GameObject gameObject) => gameObject == null;
		public static void Destroy(this GameObject gameObject)
		{
			if (!gameObject.IsNull())
			{
				NetworkServer.Destroy(gameObject);
			}
		}
	}

	public static class MethodExtensions
	{
		public static void InvokeStaticMethod(this Type type, string methodName, object[] param)
		{
			BindingFlags flags = BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public;
			MethodInfo info = type.GetMethod(methodName, flags);
			info?.Invoke(null, param);
		}
	}

	public static class HitInfoExtensions
	{
		public static Player GetAttacker(this PlayerStats.HitInfo hitInfo)
		{
			return hitInfo.IsPlayer ? hitInfo.RHub.GetPlayer() : hitInfo.GetPlayer();
		}

		public static Player GetPlayer(this PlayerStats.HitInfo hitInfo)
		{
			return hitInfo.GetPlayerObject().GetPlayer();
		}

		public static DamageType Convert(this DamageTypes.DamageType dmgType)
		{
			if (string.IsNullOrEmpty(dmgType.name))
				return DamageType.None;
			switch (dmgType.name)
			{
				case "NONE":
					return DamageType.None;
				case "LURE":
					return DamageType.Lure;
				case "NUKE":
					return DamageType.Nuke;
				case "WALL":
					return DamageType.Wall;
				case "DECONT":
					return DamageType.Decontamination;
				case "TESLA":
					return DamageType.Tesla;
				case "FALLDOWN":
					return DamageType.Falldown;
				case "Flying detection":
					return DamageType.AntiCheat;
				case "Friendly fire detector":
					return DamageType.FriendlyFireDetector;
				case "RECONTAINMENT":
					return DamageType.Recontainment;
				case "BLEEDING":
					return DamageType.Bleeding;
				case "POISONED":
					return DamageType.Poison;
				case "ASPHYXIATION":
					return DamageType.Asphyxiation;
				case "CONTAIN":
					return DamageType.Containment;
				case "POCKET":
					return DamageType.PocketDimension;
				case "RAGDOLL-LESS":
					return DamageType.Ragdolless;
				case "Com15":
					return DamageType.COM15;
				case "P90":
					return DamageType.P90;
				case "E11 Standard Rifle":
					return DamageType.Epsilon11;
				case "MP7":
					return DamageType.MP7;
				case "Logicier":
					return DamageType.Logicer;
				case "USP":
					return DamageType.USP;
				case "MicroHID":
					return DamageType.MicroHID;
				case "GRENADE":
					return DamageType.FragGrenade;
				case "SCP-049":
					return DamageType.Scp049;
				case "SCP-049-2":
					return DamageType.Scp0492;
				case "SCP-096":
					return DamageType.Scp096;
				case "SCP-106":
					return DamageType.Scp106;
				case "SCP-173":
					return DamageType.Scp173;
				case "SCP-939":
					return DamageType.Scp939;
				case "SCP-207":
					return DamageType.Scp207;
				default:
					return DamageType.None;
			}
		}
	}

	public static class RagdollExtensions
	{
		public static Player GetOwner(this Ragdoll ragdoll) => ragdoll.Networkowner.PlayerId.GetPlayer();
		public static void Delete(this Ragdoll ragdoll) => ragdoll.gameObject.Destroy();
	}

	public static class PlayerExtensions
	{
		public static void SendRemoteAdminMessage(this CommandSender sender, string message, string command)
		{
			sender.RaReply(command.ToUpper() + "#" + message, true, true, string.Empty);
		}

		public static void SendRemoteAdminMessage(this CommandSender sender, string message)
		{
			sender.SendRemoteAdminMessage(message, "server");
		}

		public static void SendRemoteAdminMessage(this Player player, string message)
		{
			player.QueryProcessor._sender.RaReply($"SERVER#{message}", true, true, string.Empty);
		}

		public static Player GetPlayer(this GameObject gameObject)
		{
			return new Player(ReferenceHub.GetHub(gameObject));
		}

		public static Player GetPlayer(this ReferenceHub hub)
		{
			return new Player(hub);
		}

		public static Player GetPlayer(this CharacterClassManager ccm)
		{
			return ccm.gameObject.GetPlayer();
		}

		public static Player GetPlayer(this PlayerStats stats)
		{
			return stats.ccm.GetPlayer();
		}

		public static Player GetPlayer(this PlayerInteract interact)
		{
			return interact.gameObject.GetPlayer();
		}

		public static Player GetPlayer(this Inventory inventory)
		{
			return inventory.gameObject.GetPlayer();
		}

		public static Player GetPlayer(this PlayableScpsController controller)
		{
			return controller.gameObject.GetPlayer();
		}

		public static Player GetPlayer(this WeaponManager manager)
		{
			return manager.gameObject.GetPlayer();
		}

		public static Player GetPlayer(this CommandSender sender)
		{
			PlayerCommandSender pcs = sender as PlayerCommandSender;
			if (pcs == null)
			{
				return Server.Host.GetPlayer();
			}
			return pcs?.CCM.GetPlayer();
		}

		public static List<Player> GetPlayers(this RoleType role)
		{
			List<Player> players = new List<Player>();
			foreach (Player player in Server.Players)
			{
				if (player.Role == role)
				{
					players.Add(player);
				}
			}
			return players;
		}

		public static List<Player> GetPlayers(this TeamType team)
		{
			List<Player> players = new List<Player>();
			foreach (Player player in Server.Players)
			{
				if (player.Team == team)
				{
					players.Add(player);
				}
			}
			return players;
		}

		public static List<Player> GetPlayers(this List<GameObject> gameObjects)
		{
			List<Player> list = new List<Player>(gameObjects.Count);
			foreach (GameObject @object in gameObjects)
			{
				if (!@object.GetComponent<CharacterClassManager>().IsHost || !string.IsNullOrEmpty(@object.GetComponent<CharacterClassManager>().UserId))
					list.Add(new Player(ReferenceHub.GetHub(@object)));
			}
			return list;
		}

		public static List<Player> GetPlayers(this List<ReferenceHub> hubs)
		{
			List<Player> list = new List<Player>();
			foreach (ReferenceHub referenceHub in hubs.Where(h => !h.characterClassManager.IsHost && !string.IsNullOrEmpty(h.characterClassManager.UserId)))
			{
				list.Add(new Player(referenceHub));
			}
			return list;
		}

		public static Player GetPlayer(this int playerId)
		{
			foreach (Player player in Server.Players)
			{
				if (player.PlayerId == playerId)
					return player;
			}
			return null;
		}

		public static Player GetPlayer(this string args)
		{
			try
			{
				Player playerFound = null;
				Dictionary<string, Player> userIds = new Dictionary<string, Player>();

				foreach (Player player in Server.Players)
				{
					userIds.Add(player.UserId, player);
				}

				foreach (string userId in userIds.Keys)
				{
					if (userId == args)
						return userIds[userId];
				}

				if (int.TryParse(args, out int id))
				{
					return id.GetPlayer();
				}

				if (args.EndsWith("@steam") || args.EndsWith("@discord") || args.EndsWith("@northwood") || args.EndsWith("@patreon"))
				{
					foreach (Player player in Server.Players)
					{
						if (player.UserId == args)
						{
							playerFound = player;
						}
					}
				}
				else
				{
					if (args == "WORLD" || args == "SCP-018" || args == "SCP-575" || args == "SCP-207")
						return null;
					int maxNameLength = 31, lastnameDifference = 31;
					string firstString = args.ToLower();
					foreach (Player player in Server.Players)
					{
						if (!player.Nick.ToLower().Contains(args.ToLower()))
							continue;
						if (firstString.Length < maxNameLength)
						{
							int x = maxNameLength - firstString.Length;
							int y = maxNameLength - player.Nick.Length;
							string secondString = player.Nick;
							for (int i = 0; i < x; i++)
								firstString += "z";
							for (int i = 0; i < y; i++)
								secondString += "z";
							int nameDifference = firstString.GetDistance(secondString);
							if (nameDifference < lastnameDifference)
							{
								lastnameDifference = nameDifference;
								playerFound = player;
							}
						}
					}
				}
				return playerFound;
			}
			catch (Exception exception)
			{
				Log.Add("Extensions", exception);
				return null;
			}
		}

		public static bool Compare(this Player player, Player playerTwo)
		{
			if (player.Nick == playerTwo.Nick && player.PlayerId == playerTwo.PlayerId && player.IpAddress == playerTwo.IpAddress && player.UserId == playerTwo.UserId)
				return true;
			else
				return false;
		}

		public static bool IsSteam(this UserIdType idType) => idType != UserIdType.Discord && idType != UserIdType.Unspecified;
	}

	public static class BanDetailsExtensions
	{
		public static Player GetIssuer(this BanDetails banDetails)
		{
			return banDetails.Issuer.GetPlayer();
		}

		public static Player GetPlayer(this BanDetails banDetails)
		{
			return banDetails.Id.GetPlayer();
		}
		public static TimeSpan GetExpirationTime(this BanDetails banDetails)
		{
			return TimeSpan.FromTicks(banDetails.Expires);
		}

		public static int GetDuration(this BanDetails banDetails)
		{
			return banDetails.GetExpirationTime().Seconds;
		}

		public static TimeSpan GetIssuanceTime(this BanDetails banDetails)
		{
			return TimeSpan.FromTicks(banDetails.IssuanceTime);
		}

		public static string GetBanReason(this string str)
		{
			return str.IsEmpty() ? "No reason provided." : str;
		}

		public static string GetReason(this BanDetails banDetails)
		{
			return banDetails.Reason.IsEmpty() ? "No reason provided." : banDetails.Reason;
		}

		public static string GetDurationString(this BanDetails banDetails)
		{
			string daysTranslation = ConfigManager.GetString("days_translation");
			string hoursTranslation = ConfigManager.GetString("hours_translation");
			string minutesTranslation = ConfigManager.GetString("minutes_translation");
			string day = string.IsNullOrEmpty(daysTranslation) || daysTranslation.ToLower() == "none" ? "d" : daysTranslation;
			string hour = string.IsNullOrEmpty(hoursTranslation) || hoursTranslation.ToLower() == "none" ? "h" : hoursTranslation;
			string minute = string.IsNullOrEmpty(minutesTranslation) || minutesTranslation.ToLower() == "none" ? "m" : minutesTranslation;
			int minutes = banDetails.GetDuration() / 60;
			int hours = minutes / 60;
			int days = hours / 24;
			if (minutes > 0)
			{
				return $"{minutes}{minute}";
			}

			if (minutes > 0 && hours > 0)
			{
				return $"{hours}{hour} {minutes}{minute}";
			}

			if (minutes > 0 && hours > 0 && days > 0)
			{
				return $"{days}{day} {hours}{hour} {minutes}{minute}";
			}
			return $"{days}{day} {hours}{hour} {minutes}{minute}";
		}

		public static string GetDurationString(this int duration)
		{
			string daysTranslation = ConfigManager.GetString("days_translation");
			string hoursTranslation = ConfigManager.GetString("hours_translation");
			string minutesTranslation = ConfigManager.GetString("minutes_translation");
			string day = string.IsNullOrEmpty(daysTranslation) || daysTranslation.ToLower() == "none" ? "d" : daysTranslation;
			string hour = string.IsNullOrEmpty(hoursTranslation) || hoursTranslation.ToLower() == "none" ? "h" : hoursTranslation;
			string minute = string.IsNullOrEmpty(minutesTranslation) || minutesTranslation.ToLower() == "none" ? "m" : minutesTranslation;
			int minutes = duration;
			int hours = minutes / 60;
			int days = hours / 24;
			if (minutes > 0)
			{
				return $"{minutes}{minute}";
			}

			if (minutes > 0 && hours > 0)
			{
				return $"{hours}{hour} {minutes}{minute}";
			}

			if (minutes > 0 && hours > 0 && days > 0)
			{
				return $"{days}{day} {hours}{hour} {minutes}{minute}";
			}
			return $"{days}{day} {hours}{hour} {minutes}{minute}";
		}
	}

	public static class StringExtensions
	{
		public static string GetWords(this string[] array)
		{
			string str = "";
			foreach (string s in array.SkipCommand())
			{
				if (s == array[1])
					str += s;
				if (s != array[1])
					str += $" {s}";
			}
			return str;
		}

		public static string SkipWords(this string[] array, int amount)
		{
			string str = "";
			foreach (string s in array.Skip(amount))
			{
				if (s == array[1])
					str += s;
				if (s != array[1])
					str += $" {s}";
			}
			return str;
		}

		public static string DiscordSanitize(this string str)
		{
			return str.Replace('@', ' ').Replace('_', ' ').Replace('*', ' ').Replace('<', ' ').Replace('`', ' ');
		}

		public static string[] SkipCommand(this string[] array)
		{
			return array.Skip(1).ToArray();
		}

		public static int GetDistance(this string firstString, string secondString)
		{
			int n = firstString.Length;
			int m = secondString.Length;
			int[,] d = new int[n + 1, m + 1];
			if (n == 0)
				return m;
			if (m == 0)
				return n;
			for (int i = 0; i <= n; d[i, 0] = i++)
			{
			}
			for (int j = 0; j <= m; d[0, j] = j++)
			{
			}
			for (int i = 1; i <= n; i++)
			{
				for (int j = 1; j <= m; j++)
				{
					int cost = (secondString[j - 1] == firstString[i - 1]) ? 0 : 1;

					d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
				}
			}
			return d[n, m];
		}

		public static bool IsEmpty(this string str)
		{
			if (string.IsNullOrEmpty(str) || str.ToLower() == "none")
				return true;
			else
				return false;
		}

		public static string ReplaceLines(this string str)
		{
			return str;
		}
	}

	public static class RoleExtensions
	{
		public static string GetName(this RoleType role)
		{
			switch (role)
			{
				case RoleType.ChaosInsurgency:
					return "Chaos Insurgent";
				case RoleType.ClassD:
					return "Class-D";
				case RoleType.FacilityGuard:
					return "Facility Guard";
				case RoleType.None:
					return "Undefined";
				case RoleType.NtfCadet:
					return "NTF Cadet";
				case RoleType.NtfCommander:
					return "NTF Commnder";
				case RoleType.NtfLieutenant:
					return "NTF Lieutenant";
				case RoleType.NtfScientist:
					return "NTF Scientist";
				case RoleType.Scientist:
					return "Scientist";
				case RoleType.Scp049:
					return "SCP-049";
				case RoleType.Scp0492:
					return "SCP-049-2";
				case RoleType.Scp079:
					return "SCP-079";
				case RoleType.Scp096:
					return "SCP-096";
				case RoleType.Scp106:
					return "SCP-106";
				case RoleType.Scp173:
					return "SCP-173";
				case RoleType.Scp93953:
					return "SCP-939-53";
				case RoleType.Scp93989:
					return "SCP-939-89";
				case RoleType.Spectator:
					return "Spectator";
				case RoleType.Tutorial:
					return "Tutorial";
				default:
					return "Unspecified";
			}
		}
	}
}
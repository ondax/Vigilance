using Mirror;
using UnityEngine;
using Vigilance.Enums;
using System.Collections.Generic;
using System;
using RemoteAdmin;
using System.Linq;
using System.Reflection;
using System.Text;
using Vigilance.API;
using System.Runtime.CompilerServices;

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

		public static bool IsPlayer(this GameObject gameObject)
        {
			if (gameObject == null)
				return false;
			if (ReferenceHub.TryGetHub(gameObject, out ReferenceHub hub))
				return true;
			else
				return false;
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
		public static Inventory.SyncItemInfo GetWeapon(this Player player, WeaponType weapon)
        {
			foreach (Inventory.SyncItemInfo item in player.Hub.inventory.items)
            {
				if (item.id == ItemType.GunCOM15 && weapon == WeaponType.Com15)
					return item;
				if (item.id == ItemType.GunE11SR && weapon == WeaponType.Epsilon11)
					return item;
				if (item.id == ItemType.GunLogicer && weapon == WeaponType.Logicer)
					return item;
				if (item.id == ItemType.GunMP7 && weapon == WeaponType.MP7)
					return item;
				if (item.id == ItemType.GunProject90 && weapon == WeaponType.Project90)
					return item;
				if (item.id == ItemType.GunUSP && weapon == WeaponType.USP)
					return item;
				if (item.id == ItemType.MicroHID && weapon == WeaponType.MicroHID)
					return item;
            }
			return new Inventory.SyncItemInfo();
        }
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
			player.Hub.queryProcessor._sender.RaReply($"SERVER#{message}", true, true, string.Empty);
		}

		public static Player GetPlayer(this GameObject gameObject)
		{
			return Server.PlayerList.GetPlayer(gameObject);
		}

		public static Player GetPlayer(this ReferenceHub hub)
		{
			return Server.PlayerList.GetPlayer(hub);
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
			foreach (ReferenceHub player in ReferenceHub.GetAllHubs().Values)
			{
				if (player.characterClassManager.UserId == sender.SenderId)
					return player.GetPlayer();
			}
			return ReferenceHub.LocalHub.GetPlayer();
		}

		public static List<Player> ToList(IEnumerable<Player> players)
		{
			return new List<Player>(players);
		}

		public static List<Player> GetPlayers(this RoleType role)
		{
			return Server.PlayerList.GetPlayers(role);
		}

		public static List<Player> GetPlayers(this TeamType team)
		{
			return Server.PlayerList.GetPlayers(team);
		}

		public static List<Player> GetPlayers(this List<GameObject> gameObjects)
		{
			List<Player> list = new List<Player>(gameObjects.Count);
			foreach (GameObject obj in gameObjects)
			{
				list.Add(obj.GetPlayer());
			}
			return list;
		}

		public static List<Player> GetPlayers(this List<ReferenceHub> hubs)
		{
			List<Player> list = new List<Player>();
			foreach (ReferenceHub referenceHub in hubs)
			{
				list.Add(new Player(referenceHub));
			}
			return list;
		}

		public static List<GameObject> GetGameObjects(this List<CharacterClassManager> ccms)
        {
			List<GameObject> objects = new List<GameObject>();
			foreach (CharacterClassManager ccm in ccms)
            {
				objects.Add(ccm.gameObject);
            }
			return objects;
        }

		public static List<GameObject> GetGameObjects(this List<ReferenceHub> hubs)
        {
			List<GameObject> objects = new List<GameObject>();
			foreach (ReferenceHub hub in hubs)
            {
				objects.Add(hub.gameObject);
            }
			return objects;
        }

		public static List<Player> GetPlayers(this List<CharacterClassManager> ccms) => ccms.GetGameObjects().GetPlayers();

		public static Player GetPlayer(this int playerId)
		{
			return Server.PlayerList.GetPlayer(playerId);
		}

		public static Player GetPlayer(this string args)
		{
			return Server.PlayerList.GetPlayer(args);
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

		public static string GetDurationString(this TimeSpan expiery, TimeSpan issuance)
		{
			TimeSpan time = expiery - issuance;
			return $"{time.Days}d {time.Hours}h {time.Minutes}m"; ;
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
			return GetDurationString(TimeSpan.FromTicks(banDetails.Expires), TimeSpan.FromTicks(banDetails.IssuanceTime));
		}

		public static string GetDurationString(this int seconds)
		{
			int minutes = seconds / 60;
			int hours = minutes / 60;
			int days = hours / 24;
			return $"{days}d {hours}h {minutes}m";
		}
	}

	public static class StringExtensions
	{
		public static string GetWords(this string[] array)
		{
			string str = "";
			foreach (string s in array.SkipCommand())
			{
				if (s == array[0])
					str += s;
				else
					str += $" {s}";
			}
			return str;
		}

		public static string SkipWords(this string[] array, int amount)
		{
			string str = "";
			foreach (string s in array.Skip(amount))
			{
				if (s == array[0])
					str += s;
				else
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
					return "None";
				case RoleType.NtfCadet:
					return "NTF Cadet";
				case RoleType.NtfCommander:
					return "NTF Commander";
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

		public static Color GetColor(this RoleType role) => role == RoleType.None ? Color.white : CharacterClassManager._staticClasses.Get(role).classColor;
		public static TeamType GetTeam(this RoleType roleType)
		{
			switch (roleType)
			{
				case RoleType.ChaosInsurgency:
					return TeamType.ChaosInsurgency;
				case RoleType.Scientist:
					return TeamType.Scientist;
				case RoleType.ClassD:
					return TeamType.ClassDPersonnel;
				case RoleType.Scp049:
				case RoleType.Scp93953:
				case RoleType.Scp93989:
				case RoleType.Scp0492:
				case RoleType.Scp079:
				case RoleType.Scp096:
				case RoleType.Scp106:
				case RoleType.Scp173:
					return TeamType.SCP;
				case RoleType.Spectator:
					return TeamType.Spectator;
				case RoleType.FacilityGuard:
				case RoleType.NtfCadet:
				case RoleType.NtfLieutenant:
				case RoleType.NtfCommander:
				case RoleType.NtfScientist:
					return TeamType.NineTailedFox;
				case RoleType.Tutorial:
					return TeamType.Tutorial;
				default:
					return TeamType.Spectator;
			}
		}
	}

	public static class Config
    {
		public static ItemType GetItem(this YamlConfig cfg, string key) => (ItemType)Enum.Parse(typeof(ItemType), cfg.GetString(key));
		public static RoleType GetRole(this YamlConfig cfg, string key) => (RoleType)Enum.Parse(typeof(RoleType), cfg.GetString(key));
		public static TeamType GetTeam(this YamlConfig cfg, string key) => (TeamType)Enum.Parse(typeof(TeamType), cfg.GetString(key));

		public static List<ItemType> GetItems(this YamlConfig cfg, string key)
		{
			try
			{
				List<ItemType> items = new List<ItemType>();
				foreach (int val in cfg.GetIntList(key))
				{
					items.Add((ItemType)Enum.Parse(typeof(ItemType), val.ToString()));
				}
				return items;
			}
			catch (Exception e)
			{
				Log.Add("YamlConfig", e);
				return new List<ItemType>();
			}
		}

		public static List<RoleType> GetRoles(this YamlConfig cfg, string key)
		{
			try
			{
				List<RoleType> roles = new List<RoleType>();
				foreach (int val in cfg.GetIntList(key))
				{
					roles.Add((RoleType)Enum.Parse(typeof(RoleType), val.ToString()));
				}
				return roles;
			}
			catch (Exception e)
			{
				Log.Add("YamlConfig", e);
				return new List<RoleType>();
			}
		}

		public static List<TeamType> GetTeams(this YamlConfig cfg, string key)
		{
			try
			{
				List<TeamType> teams = new List<TeamType>();
				foreach (int val in cfg.GetIntList(key))
				{
					teams.Add((TeamType)Enum.Parse(typeof(TeamType), val.ToString()));
				}
				return teams;
			}
			catch (Exception e)
			{
				Log.Add("YamlConfig", e);
				return new List<TeamType>();
			}
		}

		public static Vector3 GetVector(this YamlConfig cfg, string key)
		{
			Vector3 vector = Vector3.zero;
			string[] args = cfg.GetString(key).Split('=');
			if (float.TryParse(args[0], out float x) && float.TryParse(args[1], out float y) && float.TryParse(args[2], out float z))
			{
				vector = new Vector3(x, y, z);
			}
			return vector;
		}

		public static List<Vector3> GetVectorList(this YamlConfig cfg, string key)
		{
			List<Vector3> vectors = new List<Vector3>();
			foreach (string val in cfg.GetStringList(key))
			{
				string[] args = val.Split('=');
				if (float.TryParse(args[0], out float x) && float.TryParse(args[1], out float y) && float.TryParse(args[2], out float z))
				{
					Vector3 vector = new Vector3(x, y, z);
					vectors.Add(vector);
				}
			}
			return vectors;
		}

		public static Dictionary<int, int> GetIntDictionary(this YamlConfig cfg, string key)
		{
			Dictionary<string, string> stringDictionary = cfg.GetStringDictionary(key);
			if (stringDictionary.Count == 0)
				return new Dictionary<int, int>();
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			foreach (KeyValuePair<string, string> keyValuePair in stringDictionary)
			{
				int k;
				int value;
				if (int.TryParse(keyValuePair.Key, out k) && int.TryParse(keyValuePair.Value, out value))
				{
					dictionary.Add(k, value);
				}
			}
			return dictionary;
		}
	}

	public static class ItemExtensions
    {
		public static WeaponType GetWeaponType(this ItemType item)
        {
			if (item == ItemType.GunCOM15)
				return WeaponType.Com15;
			if (item == ItemType.GunE11SR)
				return WeaponType.Epsilon11;
			if (item == ItemType.GunLogicer)
				return WeaponType.Logicer;
			if (item == ItemType.GunMP7)
				return WeaponType.MP7;
			if (item == ItemType.GunProject90)
				return WeaponType.Project90;
			if (item == ItemType.GunUSP)
				return WeaponType.USP;
			if (item == ItemType.MicroHID)
				return WeaponType.MicroHID;
			return WeaponType.None;
        }

		public static AmmoType GetWeaponAmmoType(this WeaponType weapon)
		{
			if (weapon == WeaponType.Com15)
				return AmmoType.Nato_9mm;
			if (weapon == WeaponType.Epsilon11)
				return AmmoType.Nato_5mm;
			if (weapon == WeaponType.Logicer)
				return AmmoType.Nato_7mm;
			if (weapon == WeaponType.MP7)
				return AmmoType.Nato_7mm;
			if (weapon == WeaponType.Project90)
				return AmmoType.Nato_9mm;
			if (weapon == WeaponType.USP)
				return AmmoType.Nato_9mm;
			return AmmoType.None;
		}

		public static GrenadeType GetGrenadeType(this ItemType item)
        {
			if (item == ItemType.GrenadeFlash)
				return GrenadeType.FlashGrenade;
			if (item == ItemType.GrenadeFrag)
				return GrenadeType.FragGrenade;
			if (item == ItemType.SCP018)
				return GrenadeType.Scp018;
			return GrenadeType.None;
        }

		public static InventoryCategory CreateCategory(bool hideWarning, ItemCategory itemCategory, string label, byte maxItems)
        {
			InventoryCategory category = new InventoryCategory()
			{
				hideWarning = hideWarning,
				itemType = itemCategory,
				maxItems = maxItems,
				label = label
			};
			return category;
        }

		public static bool IsWeapon(this ItemType item) => item.GetWeaponType() != WeaponType.None;
		public static bool IsAmmo(this ItemType item) => item == ItemType.Ammo556 || item == ItemType.Ammo9mm || item == ItemType.Ammo762;
		public static bool IsSCP(this ItemType type) => type == ItemType.SCP018 || type == ItemType.SCP500 || type == ItemType.SCP268 || type == ItemType.SCP207;
		public static bool IsThrowable(this ItemType type) => type == ItemType.SCP018 || type == ItemType.GrenadeFrag || type == ItemType.GrenadeFlash;
		public static bool IsMedical(this ItemType type) => type == ItemType.Painkillers || type == ItemType.Medkit || type == ItemType.SCP500 || type == ItemType.Adrenaline;
		public static bool IsUtility(this ItemType type) => type == ItemType.Disarmer || type == ItemType.Flashlight || type == ItemType.Radio || type == ItemType.WeaponManagerTablet;
		public static bool IsKeycard(this ItemType type) => type == ItemType.KeycardChaosInsurgency || type == ItemType.KeycardContainmentEngineer || type == ItemType.KeycardFacilityManager || type == ItemType.KeycardGuard || type == ItemType.KeycardJanitor || type == ItemType.KeycardNTFCommander || type == ItemType.KeycardNTFLieutenant || type == ItemType.KeycardO5 || type == ItemType.KeycardScientist || type == ItemType.KeycardScientistMajor || type == ItemType.KeycardSeniorGuard || type == ItemType.KeycardZoneManager;
	}

	public static class LogExtensions
    {
		public static ConsoleColor GetColor(this LogType type)
		{
			if (type == LogType.Debug)
				return ConsoleColor.DarkCyan;
			if (type == LogType.Error)
				return ConsoleColor.DarkRed;
			if (type == LogType.Info)
				return ConsoleColor.DarkYellow;
			if (type == LogType.Warn)
				return ConsoleColor.Red;
			else
				return ConsoleColor.White;
		}
	}
}
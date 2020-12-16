using Mirror;
using UnityEngine;
using Vigilance.Enums;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
using Vigilance.API;
using Harmony;

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
			Player player = hitInfo.RHub.GetPlayer();
			if (player == null)
				player = Server.PlayerList.Local;
			return player;
		}

		public static Player GetPlayer(this PlayerStats.HitInfo hitInfo)
		{
			return hitInfo.GetPlayerObject().GetPlayer();
		}

		public static DamageType GetDamageInfo(this PlayerStats.HitInfo info)
        {
			string attacker = info.Attacker.ToUpper();
			if (attacker == "CMDSUICIDE")
				return DamageType.CmdSuicide;
			if (attacker == "DISCONNECT")
				return DamageType.Disconnect;
			return info.GetDamageType().AsDamageType();
        }
	}

	public static class DamageExtensions
	{
		public static DamageType AsDamageType(this DamageTypes.DamageType dmgType)
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
		public static Player GetOwner(this Ragdoll ragdoll)
        {
			foreach (Player player in Server.Players)
            {
				if (player.Ragdolls.Contains(ragdoll))
					return player;
            }
			return null;
        }

		public static void Delete(this Ragdoll ragdoll) => ragdoll?.gameObject.Destroy();
	}

	public static class CommandSenderExtensions
    {
		public static void SendRemoteAdminMessage(this CommandSender sender, string message, string command)
		{
			sender.RaReply(command.ToUpper() + "#" + message, true, true, string.Empty);
		}

		public static void SendRemoteAdminMessage(this CommandSender sender, string message)
		{
			sender.SendRemoteAdminMessage(message, "server");
		}

		public static Player GetPlayer(this CommandSender sender)
		{
			string id = sender.SenderId;
			if (id == "SERVER CONSOLE" && sender.Nickname == "SERVER CONSOLE")
				return new Player(ReferenceHub.LocalHub);
			if (id == "Sitrep")
				return new Player(ReferenceHub.LocalHub);
			if (sender.Nickname == "Sitrep")
				return new Player(ReferenceHub.LocalHub);
			foreach (Player player in Server.PlayerList.Players.Values)
			{
				if (player.UserId == sender.SenderId)
					return player;
			}
			return new Player(ReferenceHub.LocalHub);
		}
	}


	public static class PlayerExtensions
	{
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
			return ccm._hub.GetPlayer();
		}

		public static Player GetPlayer(this PlayerStats stats)
		{
			return stats.ccm._hub.GetPlayer();
		}

		public static Player GetPlayer(this PlayerInteract interact)
		{
			return interact._hub.GetPlayer();
		}

		public static Player GetPlayer(this Inventory inventory)
		{
			return inventory._hub.GetPlayer();
		}

		public static Player GetPlayer(this PlayableScpsController controller)
		{
			return controller._hub.GetPlayer();
		}

		public static Player GetPlayer(this WeaponManager manager)
		{
			return manager._hub.GetPlayer();
		}

		public static List<Player> ToList(IEnumerable<Player> players)
		{
			return new List<Player>(players);
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

		public static void RefreshModel(this CharacterClassManager ccm, RoleType classId = RoleType.None)
        {
			ReferenceHub hub = ReferenceHub.LocalHub;
			hub.GetComponent<AnimationController>().OnChangeClass();
			if (ccm.MyModel != null)
			{
				UnityEngine.Object.Destroy(ccm.MyModel);
			}
			Role role = ccm.Classes.SafeGet((classId < RoleType.Scp173) ? ccm.CurClass : classId);
			if (role.team != Team.RIP)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(role.model_player, ccm.gameObject.transform, true);
				gameObject.transform.localPosition = role.model_offset.position;
				gameObject.transform.localRotation = Quaternion.Euler(role.model_offset.rotation);
				gameObject.transform.localScale = role.model_offset.scale;
				ccm.MyModel = gameObject;
				AnimationController component = hub.GetComponent<AnimationController>();
				if (ccm.MyModel.GetComponent<Animator>() != null)
				{
					component.animator = ccm.MyModel.GetComponent<Animator>();
				}
				FootstepSync component2 = ccm.GetComponent<FootstepSync>();
				FootstepHandler component3 = ccm.MyModel.GetComponent<FootstepHandler>();
				if (component2 != null)
				{
					component2.FootstepHandler = component3;
				}
				if (component3 != null)
				{
					component3.FootstepSync = component2;
					component3.AnimationController = component;
				}
				if (ccm.isLocalPlayer)
				{
					if (ccm.MyModel.GetComponent<Renderer>() != null)
					{
						ccm.MyModel.GetComponent<Renderer>().enabled = false;
					}
					Renderer[] componentsInChildren = ccm.MyModel.GetComponentsInChildren<Renderer>();
					for (int i = 0; i < componentsInChildren.Length; i++)
					{
						componentsInChildren[i].enabled = false;
					}
					foreach (Collider collider in ccm.MyModel.GetComponentsInChildren<Collider>())
					{
						if (collider.name != "LookingTarget")
						{
							collider.enabled = false;
						}
					}
				}
			}
			ccm.GetComponent<CapsuleCollider>().enabled = (role.team != Team.RIP);
			if (ccm.MyModel != null)
			{
				ccm.GetComponent<WeaponManager>().hitboxes = ccm.MyModel.GetComponentsInChildren<HitboxIdentity>(true);
			}
		}

		public static Vector3 FindLookRotation(this Vector3 player, Vector3 target) => (target - player).normalized;
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

		public static string ToString(this TimeSpan expiery)
		{
			DateTime dateTime = new DateTime(expiery.Ticks);
			return dateTime.ToString("dd.MM.yy [HH:mm:ss]");
		}

		public static string ToString(this TimeSpan expiery, TimeSpan issuance)
		{
			TimeSpan time = expiery - issuance;
			int seconds = time.Seconds;
			int minutes = time.Minutes;
			int hours = time.Hours;
			int days = time.Days;
			int months = days / 30;
			if (months > 0)
				return $" {months.ToString(DurationType.Months)}";
			if (days > 0)
				return $" {days.ToString(DurationType.Days)}";
			if (hours > 0)
				return $" {hours.ToString(DurationType.Hours)}";
			if (minutes > 0)
				return $" {minutes.ToString(DurationType.Minutes)}";
			if (seconds > 0)
				return $" {seconds.ToString(DurationType.Seconds)}";
			return $"{months.ToString(DurationType.Months)} | {days.ToString(DurationType.Days)} | {hours.ToString(DurationType.Hours)} | {minutes.ToString(DurationType.Minutes)} | {seconds.ToString(DurationType.Seconds)}";
		}

		public static string ToString(this int duration, DurationType durationType)
		{
			return durationType.GetDurationTypeString(duration);
		}

		public static int GetDuration(this BanDetails banDetails)
		{
			return banDetails.GetExpirationTime().Seconds;
		}

		public static TimeSpan GetIssuanceTime(this BanDetails banDetails)
		{
			return TimeSpan.FromTicks(banDetails.IssuanceTime);
		}
	}

	public static class StringExtensions
	{
		public static string Remove(this string str, string toRemove) => str.Replace(toRemove, "");

		public static string AsString(this Vector3 vec) => $"[X: {vec.x} | Y: {vec.y} | Z: {vec.z}]";

		public static bool IsValidChance(this int rnd, int chance)
		{
			if (chance >= rnd)
				return true;
			else
				return false;
		}

		public static string SkipWords(this string[] array, int amount)
		{
			return array.Skip(amount).ToArray().Combine();
		}

		public static string DiscordSanitize(this string str)
		{
			if (string.IsNullOrEmpty(str))
				return "";
			return str.Replace('@', ' ').Replace('_', ' ').Replace('*', ' ').Replace('<', ' ').Replace('`', ' ').Replace('>', ' ').Replace('|', ' ');
		}

		public static string[] SkipCommand(this string[] array)
		{
			return array.Skip(1).ToArray();
		}

		public static string AsString(this List<string> list)
        {
			string s = "";
			foreach (string item in list)
            {
				if (string.IsNullOrEmpty(s))
                {
					s += item;
                }
				else
                {
					s += $", {item}";
                }
            }
			return s;
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
			{ }
			for (int j = 0; j <= m; d[0, j] = j++)
			{ }
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

		public static string Combine(this string[] array)
		{
			return string.Join(" ", array);
		}

		public static string[] ToArray(this char[] arr)
		{
			List<string> strings = new List<string>();
			foreach (char s in arr)
			{
				strings.Add(s.ToString());
			}
			return strings.ToArray();
		}

		public static string RemoveNumbers(this string str)
		{
			return str.Where(s => !s.IsNumber()).ToArray().ToArray().Combine();
		}

		public static bool IsNumber(this char ch) => int.TryParse(ch.ToString(), out int idk);
		public static bool IsNumber(this string str) => int.TryParse(str, out int idk);

		public static DurationType GetDurationTypeWithDuration(this string str)
		{
			if (str.IsNumber())
				return DurationType.Hours;
			str = str.RemoveNumbers();
			if (str == "s")
				return DurationType.Seconds;
			if (str == "m")
				return DurationType.Minutes;
			if (str == "h")
				return DurationType.Hours;
			if (str == "d")
				return DurationType.Days;
			if (str == "M")
				return DurationType.Months;
			if (str == "y")
				return DurationType.Years;
			return DurationType.Hours;
		}

		public static DurationType GetDurationType(this string str)
		{
			if (str == "s")
				return DurationType.Seconds;
			if (str == "m")
				return DurationType.Minutes;
			if (str == "h")
				return DurationType.Hours;
			if (str == "d")
				return DurationType.Days;
			if (str == "M")
				return DurationType.Months;
			if (str == "y")
				return DurationType.Years;
			return DurationType.Hours;
		}

		public static UserIdType GetIdType(this ulong id)
		{
			if (id.ToString().Length == 17)
				return UserIdType.Steam;
			if (id.ToString().Length == 18)
				return UserIdType.Discord;
			return UserIdType.Unspecified;
		}

		public static string GetDurationTypeString(this DurationType durationType, int dur)
		{
			if (durationType == DurationType.Days)
			{
				if (dur == 1)
					return "1 day";
				else
					return $"{dur} days";
			}

			if (durationType == DurationType.Hours)
			{
				if (dur == 1)
					return "1 hour";
				else
					return $"{dur} hours";
			}

			if (durationType == DurationType.Minutes)
			{
				if (dur == 1)
					return "1 minute";
				else
					return $"{dur} minutes";
			}

			if (durationType == DurationType.Months)
			{
				if (dur == 1)
					return "1 month";
				else
					return $"{dur} months";
			}

			if (durationType == DurationType.Seconds)
			{
				if (dur == 1)
					return "1 second";
				else
					return $"{dur} seconds";
			}

			if (durationType == DurationType.Years)
			{
				if (dur == 1)
					return "1 year";
				else
					return $"{dur} years";
			}
			return $"{dur} {durationType}";
		}

		public static string GetName(this Prefab prefab)
        {
			if (prefab == Prefab.GrenadeFlash)
				return "Grenade Flash";
			if (prefab == Prefab.GrenadeFrag)
				return "Grenade Frag";
			if (prefab == Prefab.GrenadeScp018)
				return "Grenade SCP-018";
			if (prefab == Prefab.Scp096_Ragdoll)
				return "SCP-096_Ragdoll";
			if (prefab == Prefab.WorkStation)
				return "Work Station";
			return prefab.ToString();
        }

		public static GameObject GetObject(this Prefab prefab)
        {
			foreach (GameObject obj in NetworkManager.singleton.spawnPrefabs)
				if (obj.name == prefab.GetName())
					return obj;
			return null;
        }

		public static GameObject Spawn(this Prefab prefab, Vector3 position, Quaternion rotation, Vector3 scale)
        {
			GameObject toInstantiate = prefab.GetObject();
			GameObject clone = UnityEngine.Object.Instantiate(toInstantiate);
			clone.transform.localScale = scale;
			clone.transform.position = position;
			clone.transform.rotation = rotation;
			NetworkServer.Spawn(clone);
			if (prefab == Prefab.WorkStation)
			{
				Offset offset = new Offset()
				{
					position = position,
					rotation = rotation.eulerAngles,
					scale = scale
				};
				clone.AddComponent<WorkStation>();
				clone.GetComponent<WorkStation>().Networkposition = offset;
			}
			return clone;
        }

		public static Prefab GetPrefab(this string str)
        {
			if (int.TryParse(str, out int index))
				return (Prefab)index;
			if (str == "Player")
				return Prefab.Player;
			if (str == "PlaybackLobby")
				return Prefab.PlaybackLobby;
			if (str == "Pickup")
				return Prefab.Pickup;
			if (str == "Work Station")
				return Prefab.WorkStation;
			if (str == "Ragdoll_0")
				return Prefab.Ragdoll_0;
			if (str == "Ragdoll_1")
				return Prefab.Ragdoll_1;
			if (str == "Ragdoll_3")
				return Prefab.Ragdoll_3;
			if (str == "Ragdoll_4")
				return Prefab.Ragdoll_4;
			if (str == "Ragdoll_6")
				return Prefab.Ragdoll_6;
			if (str == "Ragdoll_7")
				return Prefab.Ragdoll_7;
			if (str == "Ragdoll_8")
				return Prefab.Ragdoll_8;
			if (str == "SCP-096_Ragdoll")
				return Prefab.Scp096_Ragdoll;
			if (str == "Ragdoll_10")
				return Prefab.Ragdoll_10;
			if (str == "Ragdoll_14")
				return Prefab.Ragdoll_14;
			if (str == "Ragdoll_16")
				return Prefab.Ragdoll_16;
			if (str == "Ragdoll_17")
				return Prefab.Ragdoll_17;
			if (str == "Grenade Flash")
				return Prefab.GrenadeFlash;
			if (str == "Grenade Frag")
				return Prefab.GrenadeFrag;
			if (str == "Grenade SCP-018")
				return Prefab.GrenadeScp018;
			return Prefab.None;
        }	
		
		public static RoleType GetRole(this string str)
        {
			if (int.TryParse(str, out int id))
				return (RoleType)id;
			str = str.ToLower();
			if (str == "chaosinsurgency")
				return RoleType.ChaosInsurgency;
			if (str == "classd")
				return RoleType.ClassD;
			if (str == "facilityguard")
				return RoleType.FacilityGuard;
			if (str == "none")
				return RoleType.None;
			if (str == "ntfcadet")
				return RoleType.NtfCadet;
			if (str == "ntfcommander")
				return RoleType.NtfCommander;
			if (str == "ntflieutenant")
				return RoleType.NtfLieutenant;
			if (str == "ntfscientist")
				return RoleType.NtfScientist;
			if (str == "scientist")
				return RoleType.Scientist;
			if (str == "scp049")
				return RoleType.Scp049;
			if (str == "scp0492" || str == "zombie")
				return RoleType.Scp0492;
			if (str == "scp079")
				return RoleType.Scp079;
			if (str == "scp096")
				return RoleType.Scp096;
			if (str == "scp106")
				return RoleType.Scp106;
			if (str == "scp173")
				return RoleType.Scp173;
			if (str == "scp93989")
				return RoleType.Scp93989;
			if (str == "scp93989")
				return RoleType.Scp93989;
			if (str == "spectator")
				return RoleType.Spectator;
			if (str == "tutorial")
				return RoleType.Tutorial;
			return RoleType.None;
        }

		public static ItemType GetItem(this string str)
        {
			if (int.TryParse(str, out int id))
				return (ItemType)id;
			str = str.ToLower();
			if (str == "adrenaline")
				return ItemType.Adrenaline;
			if (str == "ammo556")
				return ItemType.Ammo556;
			if (str == "ammo762")
				return ItemType.Ammo762;
			if (str == "ammo9mm")
				return ItemType.Ammo9mm;
			if (str == "coin")
				return ItemType.Coin;
			if (str == "disarmer")
				return ItemType.Disarmer;
			if (str == "flashlight")
				return ItemType.Flashlight;
			if (str == "flash" || str == "grenadeflash")
				return ItemType.GrenadeFlash;
			if (str == "frag" || str == "grenadefrag")
				return ItemType.GrenadeFrag;
			if (str == "guncom15" || str == "com15")
				return ItemType.GunCOM15;
			if (str == "gune11sr" || str == "e11sr" || str == "epsilon11")
				return ItemType.GunE11SR;
			if (str == "gunlogicer" || str == "logicer")
				return ItemType.GunLogicer;
			if (str == "gunmp7" || str == "mp7")
				return ItemType.GunMP7;
			if (str == "gunproject90" || str == "project90" || str == "p90")
				return ItemType.GunProject90;
			if (str == "gunusp" || str == "usp")
				return ItemType.GunUSP;
			if (str == "keycardchaosinsurgency")
				return ItemType.KeycardChaosInsurgency;
			if (str == "keycardcontainmentengineer")
				return ItemType.KeycardContainmentEngineer;
			if (str == "keycardfacilitymanager")
				return ItemType.KeycardFacilityManager;
			if (str == "keycardguard")
				return ItemType.KeycardGuard;
			if (str == "keycardjanitor")
				return ItemType.KeycardJanitor;
			if (str == "keycardntfcommander")
				return ItemType.KeycardNTFCommander;
			if (str == "keycardntflieutenant")
				return ItemType.KeycardNTFLieutenant;
			if (str == "keycardo5")
				return ItemType.KeycardO5;
			if (str == "keycardscientist")
				return ItemType.KeycardScientist;
			if (str == "keycardscientistmajor")
				return ItemType.KeycardScientistMajor;
			if (str == "keycardseniorguard")
				return ItemType.KeycardSeniorGuard;
			if (str == "keycardzonemanager")
				return ItemType.KeycardZoneManager;
			if (str == "medkit")
				return ItemType.Medkit;
			if (str == "microhid")
				return ItemType.MicroHID;
			if (str == "none")
				return ItemType.None;
			if (str == "painkillers")
				return ItemType.Painkillers;
			if (str == "radio")
				return ItemType.Radio;
			if (str == "scp018")
				return ItemType.SCP018;
			if (str == "scp500")
				return ItemType.SCP500;
			if (str == "scp207")
				return ItemType.SCP207;
			if (str == "scp268")
				return ItemType.SCP268;
			if (str == "tablet" || str == "weaponmanagertablet")
				return ItemType.WeaponManagerTablet;
			return ItemType.None;
        }

		public static TeamType GetTeam(this string str)
        {
			if (int.TryParse(str, out int id))
				return (TeamType)id;
			str = str.ToLower();
			if (str == "chaosinsurgency")
				return TeamType.ChaosInsurgency;
			if (str == "classd")
				return TeamType.ClassDPersonnel;
			if (str == "ntf" || str == "ninetailedfox")
				return TeamType.NineTailedFox;
			if (str == "scientist")
				return TeamType.Scientist;
			if (str == "scp")
				return TeamType.SCP;
			if (str == "spectator")
				return TeamType.Spectator;
			if (str == "tutorial")
				return TeamType.Tutorial;
			if (str == "none")
				return TeamType.None;
			return TeamType.None;
        }

		public static WeaponType GetWeapon(this string str)
        {
			if (int.TryParse(str, out int id))
				return (WeaponType)id;
			str = str.ToLower();
			if (str == "com15")
				return WeaponType.Com15;
			if (str == "epsilon11")
				return WeaponType.Epsilon11;
			if (str == "logicer")
				return WeaponType.Logicer;
			if (str == "microhid")
				return WeaponType.MicroHID;
			if (str == "mp7")
				return WeaponType.MP7;
			if (str == "none")
				return WeaponType.None;
			if (str == "project90" || str == "p90")
				return WeaponType.Project90;
			if (str == "usp")
				return WeaponType.USP;
			return WeaponType.None;
        }

		public static AmmoType GetAmmoType(this string str)
        {
			if (int.TryParse(str, out int id))
				return (AmmoType)id;
			if (str == "5mm")
				return AmmoType.Nato_5mm;
			if (str == "7mm")
				return AmmoType.Nato_7mm;
			if (str == "9mm")
				return AmmoType.Nato_9mm;
			return AmmoType.None;
        }

		public static List<Player> GetPlayers(this RoleType role)
		{
			return Server.PlayerList.GetPlayers(role);
		}

		public static List<Player> GetPlayers(this TeamType team)
		{
			return Server.PlayerList.GetPlayers(team);
		}
	}

	public static class EnumExtensions
    {
		public static Achievement GetAchievement(this string str)
        {
			IEnumerable<Achievement> achievements = Environment.GetValues<Achievement>();
			foreach (Achievement achievement in achievements)
            {
				if (achievement.ToString().ToLower() == str.ToLower())
					return achievement;
            }
			return Achievement.Unknown;
        }

		public static T GetEnum<T>(this string str)
        {
			IEnumerable<Assembly> assemblies = PluginManager.Assemblies.Values;
			assemblies.Add(Assembly.LoadFrom($"{Paths.Managed}/Assembly-CSharp.dll"));
			assemblies.Add(Assembly.LoadFrom($"{Paths.VigilanceFile}"));
			List<Type> valid = new List<Type>();
			foreach (Assembly assembly in assemblies)
            {
				foreach (Type type in assembly.GetTypes())
                {
					if (type.BaseType == typeof(Enum))
                    {
						valid.Add(type);
                    }
                }
            }

			List<T> res = valid.Cast<T>().ToList();
			foreach (T t in res)
            {
				if (t.ToString().ToLower() == str.ToLower() || t.ToString().ToLower().Contains(str.ToLower()))
                {
					return t;
                }
            }

			IEnumerable<int> res2 = valid.Cast<int>();
			if (int.TryParse(str, out int id))
            {
				return res[res2.Where(h => h == id).FirstOrDefault()];
            }
			return default;
        }
    }

	public static class RoleExtensions
	{
		public static string AsString(this RoleType role)
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

		public static string ToCassie(this RoleType role)
		{
			if (role == RoleType.ChaosInsurgency)
				return "ChaosInsurgency";
			if (role == RoleType.ClassD)
				return "ClassD";
			if (role == RoleType.FacilityGuard)
				return "Facility Guard";
			if (role == RoleType.NtfCadet)
				return "Cadet";
			if (role == RoleType.NtfCommander)
				return "Commander";
			if (role == RoleType.NtfLieutenant)
				return "Lieutenant";
			if (role == RoleType.NtfScientist)
				return "NineTailedFox Scientist";
			if (role == RoleType.Scientist)
				return "Scientist";
			if (role == RoleType.Scp049)
				return "SCP 0 4 9";
			if (role == RoleType.Scp0492)
				return "SCP 0 4 9 . 2";
			if (role == RoleType.Scp079)
				return "SCP 0 7 9";
			if (role == RoleType.Scp096)
				return "SCP 0 9 6";
			if (role == RoleType.Scp106)
				return "SCP 1 0 6";
			if (role == RoleType.Scp173)
				return "SCP 1 7 3";
			if (role == RoleType.Scp93953 || role == RoleType.Scp93989)
				return "SCP 9 3 9";
			return "";
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
		public static ItemType GetItem(this YamlConfig cfg, string key) => cfg.GetString(key).GetItem();
		public static RoleType GetRole(this YamlConfig cfg, string key) => cfg.GetString(key).GetRole();
		public static TeamType GetTeam(this YamlConfig cfg, string key) => cfg.GetString(key).GetTeam();
		public static API.Broadcast GetBroadcast(this YamlConfig cfg, string key) => ParseBroadcast(cfg.GetString(key));

		public static List<ItemType> GetItems(this YamlConfig cfg, string key)
		{
			if (cfg == null)
				return new List<ItemType>();
			try
			{
				List<ItemType> items = new List<ItemType>();
				foreach (string val in cfg.GetStringList(key))
				{
					items.Add(val.GetItem());
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
			if (cfg == null)
				return new List<RoleType>();
			try
			{
				List<RoleType> roles = new List<RoleType>();
				foreach (string val in cfg.GetStringList(key))
				{
					roles.Add(val.GetRole());
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
			if (cfg == null)
				return new List<TeamType>();
			try
			{
				List<TeamType> teams = new List<TeamType>();
				foreach (string val in cfg.GetStringList(key))
				{
					teams.Add(val.GetTeam());
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
			if (cfg == null)
				return Vector3.zero;
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
			if (cfg == null)
				return new List<Vector3>();
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
			if (cfg == null)
				return new Dictionary<int, int>();
			Dictionary<string, string> stringDictionary = cfg.GetStringDictionary(key);
			if (stringDictionary.Count == 0)
				return new Dictionary<int, int>();
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			foreach (KeyValuePair<string, string> keyValuePair in stringDictionary)
			{
				if (int.TryParse(keyValuePair.Key, out int k) && int.TryParse(keyValuePair.Value, out int value))
				{
					dictionary.Add(k, value);
				}
			}
			return dictionary;
		}

		public static List<API.Broadcast> GetBroadcasts(this YamlConfig cfg, string key)
        {
			List<API.Broadcast> bcs = new List<API.Broadcast>();
			foreach (string str in cfg.GetStringList(key))
            {
				bcs.Add(ParseBroadcast(str));
            }
			return bcs;
        }

		public static API.Broadcast ParseBroadcast(string arg)
        {
			if (string.IsNullOrEmpty(arg))
				return new API.Broadcast(0, "", false);
			if (!arg.Contains("|"))
				return new API.Broadcast(0, "", false);
			string[] args = arg.Split('|');
			string txt = args[0];
			bool mono = false;
			if (!int.TryParse(args[1], out int duration))
				return new API.Broadcast(0, "", false);
			if (args.Length < 3 || !bool.TryParse(args[2], out mono))
				mono = false;
			return new API.Broadcast(duration, txt, mono);
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
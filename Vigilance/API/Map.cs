using System.Collections.Generic;
using System.Linq;
using Grenades;
using Mirror;
using Respawning;
using UnityEngine;
using Vigilance.Enums;
using Vigilance.Extensions;
using Object = UnityEngine.Object;
using RemoteAdmin;
using MEC;

namespace Vigilance.API
{
	public static class Map
	{
		public static List<Ragdoll> Ragdolls => Object.FindObjectsOfType<Ragdoll>().ToList();
		public static List<FlickerableLight> FlickerableLights => Object.FindObjectsOfType<FlickerableLight>().ToList();
		public static int ActivatedGenerators => Generator079.mainGenerator.totalVoltage;
		public static AudioSource AudioSource => GameObject.Find("GameManager")?.GetComponent<AudioSource>();
		public static List<BlastDoor> BlastDoors => Object.FindObjectsOfType<BlastDoor>().ToList();
		public static List<Camera079> Cameras => Scp079PlayerScript.allCameras.ToList();
		public static List<Door> Doors => Object.FindObjectsOfType<Door>().ToList();
		public static List<Lift> Lifts => Object.FindObjectsOfType<Lift>().ToList();
		public static List<Lift.Elevator> Elevators => Lifts[0].elevators.ToList();
		public static Generator079 MainGenerator => Generator079.mainGenerator;
		public static List<Pickup> Pickups => Object.FindObjectsOfType<Pickup>().ToList();
		public static string Seed => Server.Host.GetComponent<RandomSeedSync>().seed.ToString();
		public static List<TeslaGate> TeslaGates => Object.FindObjectsOfType<TeslaGate>().ToList();
		public static List<Generator079> Generators => Generator079.Generators;
		public static RespawnEffectsController Respawn => RespawnEffectsController.AllControllers.Where(controller => controller != null).FirstOrDefault();

		public static void Broadcast(string message, int duration)
		{
			foreach (Player player in Server.Players)
			{
				player.Broadcast(message, duration);
			}
		}

		public static void ClearBroadcasts()
		{
			foreach (Player player in Server.Players)
			{
				player.ClearBroadcasts();
			}
		}

		public static Camera079 GetCamera(int cameraId)
        {
			foreach (Camera079 camera in Scp079PlayerScript.allCameras)
			{
				if (camera.cameraId == cameraId)
					return camera;
			}
			return null;
		}

		public static void Announce(string message, bool makeHold = false, bool makeNoise = false)
		{
			RespawnEffectsController.PlayCassieAnnouncement(message, makeHold, makeNoise);
		}

		public static void PlayEffect(RespawnEffectType effectType)
		{
			Respawn.RpcPlayEffects(new byte[] { (byte)effectType });
		}

		public static void SummonChopper()
		{
			PlayEffect(RespawnEffectType.SummonNtfChopper);
		}

		public static void SummonVan(bool playMusic = true)
		{
			if (playMusic)
			{
				PlayEffect(RespawnEffectType.PlayChaosInsurgencyMusic);
				PlayEffect(RespawnEffectType.SummonChaosInsurgencyVan);
				return;
			}
			PlayEffect(RespawnEffectType.SummonChaosInsurgencyVan);
		}

		public static Vector3 GetRandomSpawnpoint(RoleType role) => Server.Host.GetComponent<SpawnpointManager>().GetRandomPosition(role).transform.position;
		public static void TurnOffLights(float time = 9999f, bool onlyHeavy = false) => Generators[0].ServerOvercharge(time, onlyHeavy);
		public static Pickup SpawnItem(ItemType itemType, Vector3 position, Quaternion rotation = default, int sight = 0, int barrel = 0, int other = 0) => Server.LocalHub.inventory.SetPickup(itemType, -4.6566467E+11f, position, rotation, sight, barrel, other);

		public static Grenade SpawnGrenade(Player player, GrenadeType grenadeType)
		{
			if (grenadeType == GrenadeType.FragGrenade)
			{
				GrenadeManager grenadeManager = player.GameObject.GetComponent<GrenadeManager>();
				Grenade component = Object.Instantiate<GameObject>(grenadeManager.availableGrenades.FirstOrDefault((GrenadeSettings g) => g.inventoryID == ItemType.GrenadeFrag).grenadeInstance).GetComponent<Grenade>();
				component.InitData(grenadeManager, Vector3.zero, Vector3.zero, 0f);
				NetworkServer.Spawn(component.gameObject);
				return component;
			}

			if (grenadeType == GrenadeType.FlashGrenade)
			{
				GrenadeManager grenadeManager2 = player.GameObject.GetComponent<GrenadeManager>();
				Grenade component2 = Object.Instantiate<GameObject>(grenadeManager2.availableGrenades.FirstOrDefault((GrenadeSettings g) => g.inventoryID == ItemType.GrenadeFlash).grenadeInstance).GetComponent<Grenade>();
				component2.InitData(grenadeManager2, Vector3.zero, Vector3.zero, 0f);
				NetworkServer.Spawn(component2.gameObject);
				return component2;
			}

			GrenadeManager grenadeManager3 = player.GameObject.GetComponent<GrenadeManager>();
			Grenade component3 = Object.Instantiate<GameObject>(grenadeManager3.availableGrenades.FirstOrDefault((GrenadeSettings g) => g.inventoryID == ItemType.SCP018).grenadeInstance).GetComponent<Grenade>();
			component3.InitData(grenadeManager3, Vector3.zero, Vector3.zero, 0f);
			NetworkServer.Spawn(component3.gameObject);
			return component3;
		}

		public static void SpawnDummyModel(Vector3 position, Quaternion rotation, RoleType role, float x, float y, float z)
		{
			GameObject obj = Object.Instantiate(NetworkManager.singleton.spawnPrefabs.FirstOrDefault(p => p.gameObject.name == "Player"));
			CharacterClassManager ccm = obj.GetComponent<CharacterClassManager>();
			ccm.CurClass = role;
			ccm.RefreshPlyModel();
			obj.GetComponent<NicknameSync>().Network_myNickSync = "Dummy";
			obj.GetComponent<QueryProcessor>().PlayerId = 9999;
			obj.GetComponent<QueryProcessor>().NetworkPlayerId = 9999;
			obj.transform.localScale = new Vector3(x, y, z);
			obj.transform.position = position;
			obj.transform.rotation = rotation;
			NetworkServer.Spawn(obj);
		}

		public static void SpawnWorkbench(Vector3 position, Vector3 rotation, Vector3 size)
		{
			GameObject bench = Object.Instantiate(NetworkManager.singleton.spawnPrefabs.Find(p => p.gameObject.name == "Work Station"));
			Offset offset = new Offset();
			offset.position = position;
			offset.rotation = rotation;
			offset.scale = Vector3.one;
			bench.gameObject.transform.localScale = size;
			NetworkServer.Spawn(bench);
			bench.GetComponent<WorkStation>().Networkposition = offset;
			bench.AddComponent<WorkStationUpgrader>();
		}

		public static void SpawnRagdolls(Player player, int role, int count) => Timing.RunCoroutine(SpawnBodies(player, role, count));

		private static IEnumerator<float> SpawnBodies(Player player, int role, int count)
		{
			for (int i = 0; i < count; i++)
			{
				player.GameObject.GetComponent<RagdollManager>().SpawnRagdoll(player.Position + Vector3.up * 5,
					Quaternion.identity, Vector3.zero, role,
					new PlayerStats.HitInfo(1000f, player.UserId, DamageTypes.Falldown,
						player.PlayerId), false, "SCP-343", "SCP-343", 0);
				yield return Timing.WaitForSeconds(0.15f);
			}
		}

		public static class Warhead
		{
			public static bool Detonated { get => AlphaWarheadController.Host.detonated; set => AlphaWarheadController.Host.detonated = value; }
			public static int TimeToDetonation { get => (int)AlphaWarheadController.Host.NetworktimeToDetonation; set => AlphaWarheadController.Host.NetworktimeToDetonation = (float)value; }
			public static bool IsInProgress { get => AlphaWarheadController.Host.NetworkinProgress; set => AlphaWarheadController.Host.NetworkinProgress = value; }
			public static bool IsResumed => AlphaWarheadController.Host.NetworksyncResumeScenario > 0;

			public static void Prepare() => AlphaWarheadController.Host.InstantPrepare();
			public static void Start() => AlphaWarheadController.Host.StartDetonation();
			public static void Stop() => AlphaWarheadController.Host.CancelDetonation();
			public static void Shake() => AlphaWarheadController.Host.RpcShake(true);
			public static void Detonate()
			{
				Prepare();
				AlphaWarheadController.Host.Detonate();
			}
		}

		public static class Intercom
		{
			public static bool AdminSpeaking => global::Intercom.AdminSpeaking;
			public static Player Speaker { get => global::Intercom.host.Networkspeaker?.GetPlayer(); set => global::Intercom.host.Networkspeaker = value.GameObject; }
			public static string Text { get => global::Intercom.host.NetworkintercomText; set => global::Intercom.host.NetworkintercomText = value; }
		}
	}
}
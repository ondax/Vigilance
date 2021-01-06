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
using Scp914;
using LightContainmentZoneDecontamination;
using System;
using MapGeneration;
using Interactables.Interobjects.DoorUtils;

namespace Vigilance.API
{
	public static class Map
	{
		public static List<Room> _rooms = null;
		public static List<Door> Doors { get; internal set; }
		public static List<Ragdoll> Ragdolls => FindObjects<Ragdoll>();
		public static List<FlickerableLight> FlickerableLights { get; } = FindObjects<FlickerableLight>();
		public static List<FlickerableLightController> LightControllers { get; } = FindObjects<FlickerableLightController>();
		public static List<BlastDoor> BlastDoors { get; } = FindObjects<BlastDoor>();
		public static List<Camera079> Cameras { get; } = Scp079PlayerScript.allCameras.ToList();
		public static List<Lift> Lifts { get; } = FindObjects<Lift>();
		public static List<Pickup> Pickups => FindObjects<Pickup>();
		public static List<TeslaGate> TeslaGates { get; } = FindObjects<TeslaGate>();
		public static List<GameObject> PocketDimensionExists => GameObject.FindGameObjectsWithTag("PD_EXIT").ToList();
		public static List<Generator079> Generators => Generator079.Generators;
		public static List<Lift.Elevator> Elevators => Lifts.FirstOrDefault().elevators.ToList();
		public static List<Rid> RoomIDs { get; } = GameObject.FindGameObjectsWithTag("RoomID").Select(h => h.GetComponent<Rid>()).ToList();
		public static List<RoomInformation> RoomList { get; } = FindObjects<RoomInformation>();
		public static Vector3 PocketDimension { get; } = new Vector3(0f, -1998.5f, 0f);
		public static int ActivatedGenerators => Generator079.mainGenerator.totalVoltage;
		public static Generator079 MainGenerator => Generator079.mainGenerator;
		public static RespawnEffectsController RespawnController => RespawnEffectsController.AllControllers.Where(controller => controller != null).FirstOrDefault();
		public static SeedSynchronizer SeedSynchronizer { get; } = Server.GameManager?.GetComponent<SeedSynchronizer>();
		public static GameObject FemurBreaker { get; } = GameObject.FindGameObjectWithTag("FemurBreaker");
		public static LureSubjectContainer LureSubjectContainer { get; } = Find<LureSubjectContainer>();
		public static OneOhSixContainer OneOhSixContainer { get; } = Find<OneOhSixContainer>();
		public static GameObject OutsitePanelScript { get; } = GameObject.Find("OutsitePanelScript");
		public static AlphaWarheadOutsitePanel OutsitePanel { get; } = OutsitePanelScript.GetComponent<AlphaWarheadOutsitePanel>();
		public static AlphaWarheadNukesitePanel NukesitePanel { get; } = AlphaWarheadOutsitePanel.nukeside;
		public static int MapSeed { get; } = SeedSynchronizer.Seed;
		public static bool TeslaGatesDisabled { get; set; }
		public static WarheadLeverStatus WarheadLeverStatus { get => NukesitePanel.Networkenabled ? WarheadLeverStatus.Enabled : WarheadLeverStatus.Disabled; set => NukesitePanel.Networkenabled = value == WarheadLeverStatus.Enabled ? true : false; }

		public static List<Room> Rooms
        {
			get
            {
				try
				{
					if (_rooms == null || _rooms.Count < RoomIDs.Count)
					{
						_rooms = new List<Room>();
						_rooms.AddRange(GameObject.FindGameObjectsWithTag("Room").Select(r => new Room(r.name, r, r.transform.position)));
						_rooms.Add(new Room("Root_*&*Outside Cams", GameObject.Find("Root_*&*Outside Cams"), GameObject.Find("Root_*&*Outside Cams").transform.position));
						RoomInformation pocket = FindObjects<RoomInformation>().Where(h => h.CurrentRoomType == RoomInformation.RoomType.POCKET).FirstOrDefault();
						if (pocket != null)
							_rooms.Add(new Room("PocketDimension", pocket.gameObject, pocket.transform.position));
					}
					return _rooms;
				}
				catch (Exception e)
                {
					Log.Add(nameof(Rooms), e);
					return new List<Room>();
                }
            }
        }
	

		public static void Broadcast(string message, int duration, bool mono = false)
		{
			try
			{ 
				if (string.IsNullOrEmpty(message) || duration < 1)
					return;
				foreach (Player player in Server.PlayerList.Players.Values)
				{
					player.Broadcast(message, duration, mono);
				}
			}
			catch (Exception e)
            {
				Log.Add(nameof(Broadcast), e);
            }
		}

		public static void Broadcast(Broadcast bc) => Broadcast(bc.Message, bc.Duration, bc.Monospaced);

		public static void ClearBroadcasts()
		{
			try
			{
				foreach (Player player in Server.PlayerList.Players.Values)
				{
					player.ClearBroadcasts();
				}
			}
			catch (Exception e)
            {
				Log.Add(nameof(ClearBroadcasts), e);
            }
		}

		public static void ShowHint(string message, int duration)
        {
			try
			{
				if (string.IsNullOrEmpty(message) || duration < 1)
					return;
				foreach (Player player in Server.PlayerList.Players.Values)
					player.ShowHint(message, duration);
			}
			catch (Exception e)
            {
				Log.Add(nameof(ShowHint), e);
            }
        }

		public static void ShowHint(Broadcast bc) => ShowHint(bc.Message, bc.Duration);

		public static Room GetRoom(RoomType roomType)
		{
			try
			{
				foreach (Room room in Rooms)
				{
					if (room.Type == roomType)
						return room;
				}
				return null;
			}
			catch (Exception e)
			{
				Log.Add(nameof(GetRoom), e);
				return null;
            }
		}

		public static Rid GetRoomID(string name)
        {
			foreach (Rid rid in RoomIDs)
            {
				if (rid.id.ToLower() == name.ToLower() || rid.id.ToLower().Contains(name.ToLower()) || name.ToLower().Contains(rid.id.ToLower()))
					return rid;
            }
			RoomInformation info = GetRoomInformation(name);
			if (info != null)
				return info.GetComponent<Rid>();
			return null;
        }

		public static RoomInformation GetRoomInformation(string search)
		{
			foreach (RoomInformation info in FindObjects<RoomInformation>())
            {
				if (info.name == search || info.name.Contains(search) || info.tag == search || info.tag.Contains(search))
					return info;
            }

			foreach (RoomInformation.RoomType type in Environment.GetValues<RoomInformation.RoomType>())
            {
				string str = type.ToString();
				if (str == search || str.ToLower() == search.ToLower() || str.Contains(search))
					return GetRoomInformation(type);
            }
			return null;
		}

		public static RoomInformation GetRoomInformation(RoomInformation.RoomType type)
        {
			foreach (RoomInformation info in FindObjects<RoomInformation>())
			{
				if (info.CurrentRoomType == type)
					return info;
			}
			return null;
		}

		public static RoomInformation GetRoomInformation(RoomType type)
        {
			Room room = GetRoom(type);
			if (room != null)
				return room.RoomInformation;
			return null;
		}

		public static Room FindParentRoom(GameObject objectInRoom)
		{
			var rooms = Rooms;
			Room room = null;
			const string playerTag = "Player";
			if (!objectInRoom.CompareTag(playerTag))
			{
				room = objectInRoom.GetComponentInParent<Room>();
			}
			else
			{
				var ply = Server.PlayerList.GetPlayer(objectInRoom);
				if (ply.Role == RoleType.Scp079)
					room = FindParentRoom(ply.Hub.scp079PlayerScript.currentCamera.gameObject);
			}

			if (room == null)
			{
				Ray ray = new Ray(objectInRoom.transform.position, Vector3.down);
				if (Physics.RaycastNonAlloc(ray, Environment.Cache.CachedFindParentRoomRaycast, 10, 1 << 0, QueryTriggerInteraction.Ignore) == 1)
					room = Environment.Cache.CachedFindParentRoomRaycast[0].collider.gameObject.GetComponentInParent<Room>();
			}
			if (room == null && rooms.Count != 0)
				room = rooms[rooms.Count - 1];
			return room;
		}

		public static Room GetRoom(string name)
		{
			try
			{
				foreach (RoomType type in Environment.GetValues<RoomType>())
                {
					if (type.ToString().ToLower() == name.ToLower())
						return GetRoom(type);
                }
				return null;
			}
			catch (Exception e)
            {
				Log.Add(nameof(GetRoom), e);
				return null;
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
			RespawnController.RpcPlayEffects(new byte[] { (byte)effectType });
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

		public static T Find<T>() where T : Component
        {
			return Object.FindObjectOfType<T>();
        }

		public static List<T> FindObjects<T>() where T : Component
        {
			return Object.FindObjectsOfType<T>().ToList();
        }

		public static Vector3 GetRandomSpawnpoint(RoleType role) => PlayerManager.localPlayer.GetComponent<SpawnpointManager>().GetRandomPosition(role).transform.position;
		public static void TurnOffLights(float time = 9999f, bool onlyHeavy = false) => Generators[0].ServerOvercharge(time, onlyHeavy);
		public static Pickup SpawnItem(ItemType itemType, Vector3 position, Quaternion rotation = default, int sight = 0, int barrel = 0, int other = 0) => Server.LocalHub.inventory.SetPickup(itemType, -4.6566467E+11f, position, rotation, sight, barrel, other);

		public static Grenade SpawnGrenade(Player player, GrenadeType grenadeType)
		{
			try
			{
				if (grenadeType == GrenadeType.FragGrenade)
				{
					GrenadeManager grenadeManager = player.GameObject.GetComponent<GrenadeManager>();
					Grenade component = Object.Instantiate(grenadeManager.availableGrenades.FirstOrDefault((GrenadeSettings g) => g.inventoryID == ItemType.GrenadeFrag).grenadeInstance).GetComponent<Grenade>();
					component.InitData(grenadeManager, Vector3.zero, Vector3.zero, 0f);
					NetworkServer.Spawn(component.gameObject);
					return component;
				}

				if (grenadeType == GrenadeType.FlashGrenade)
				{
					GrenadeManager grenadeManager2 = player.GameObject.GetComponent<GrenadeManager>();
					Grenade component2 = Object.Instantiate(grenadeManager2.availableGrenades.FirstOrDefault((GrenadeSettings g) => g.inventoryID == ItemType.GrenadeFlash).grenadeInstance).GetComponent<Grenade>();
					component2.InitData(grenadeManager2, Vector3.zero, Vector3.zero, 0f);
					NetworkServer.Spawn(component2.gameObject);
					return component2;
				}

				GrenadeManager grenadeManager3 = player.GameObject.GetComponent<GrenadeManager>();
				Grenade component3 = Object.Instantiate(grenadeManager3.availableGrenades.FirstOrDefault((GrenadeSettings g) => g.inventoryID == ItemType.SCP018).grenadeInstance).GetComponent<Grenade>();
				component3.InitData(grenadeManager3, Vector3.zero, Vector3.zero, 0f);
				NetworkServer.Spawn(component3.gameObject);
				return component3;
			}
			catch (Exception e)
            {
				Log.Add(nameof(SpawnGrenade), e);
				return null;
            }
		}

		public static GameObject SpawnDummyModel(Vector3 position, Quaternion rotation, RoleType role, float x, float y, float z)
		{
			try
			{
				GameObject obj = Object.Instantiate(NetworkManager.singleton.spawnPrefabs.FirstOrDefault(p => p.gameObject.name == "Player"));
				CharacterClassManager ccm = obj.GetComponent<CharacterClassManager>();
				ccm.CurClass = role;
				ccm.RefreshModel(role);
				obj.GetComponent<NicknameSync>().Network_myNickSync = "Dummy";
				obj.GetComponent<QueryProcessor>().PlayerId = 9999;
				obj.GetComponent<QueryProcessor>().NetworkPlayerId = 9999;
				obj.transform.localScale = new Vector3(x, y, z);
				obj.transform.position = position;
				obj.transform.rotation = rotation;
				NetworkServer.Spawn(obj);
				return obj;
			}
			catch (Exception e)
            {
				Log.Add(nameof(SpawnDummyModel), e);
				return null;
            }
		}

		public static void SpawnRagdolls(Player player, int role, int count) => Timing.RunCoroutine(SpawnBodies(player.GetComponent<RagdollManager>(), role, count));

		private static IEnumerator<float> SpawnBodies(RagdollManager rm, int role, int count)
		{
			for (int i = 0; i < count; i++)
			{
				rm.SpawnRagdoll(rm.transform.position + Vector3.up * 5, Quaternion.identity, Vector3.zero, role, new PlayerStats.HitInfo(1000f, "WORLD", DamageTypes.Falldown, 0), false, "SCP-343", "SCP-343", 0);
				yield return Timing.WaitForSeconds(0.15f);
			}
		}

		public static void RefreshDoors()
        {
			try
			{
				if (Doors != null)
					Doors.Clear();
				DoorExtensions.SetInfo();
				Doors = DoorExtensions.Doors.Values.ToList();
			}
			catch (Exception e)
            {
				Log.Add("MAP", e);
            }
        }

		public static class Warhead
		{
			public static bool Detonated { get => AlphaWarheadController.Host.detonated; set => AlphaWarheadController.Host.detonated = value; }
			public static int TimeToDetonation { get => (int)AlphaWarheadController.Host.NetworktimeToDetonation; set => AlphaWarheadController.Host.NetworktimeToDetonation = (float)value; }
			public static bool IsInProgress { get => AlphaWarheadController.Host.NetworkinProgress; set => AlphaWarheadController.Host.NetworkinProgress = value; }
			public static bool IsResumed => AlphaWarheadController.Host.NetworksyncResumeScenario > 0;
			public static AudioSource AudioSource => Server.GameManager.GetComponent<AudioSource>();

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

		public static class Scp914
		{
			public static Scp914Machine Singleton => Scp914Machine.singleton;
			public static List<Player> Players => Singleton.players.GetPlayers();
			public static List<Pickup> Items => Singleton.items;
			public static Vector3 Position => Singleton.transform.position;
			public static Scp914Knob KnobState { get => Singleton.knobState; set => Singleton.SetKnobState(value); }
			public static Scp914Mode Mode => Singleton.configMode.Value;
			public static List<Scp914Recipe> Recipes { get => Singleton.recipes.ToList(); set => Singleton.recipes = value.ToArray(); }
			public static Dictionary<ItemType, Dictionary<Scp914Knob, ItemType[]>> RecipesDict { get => Singleton.recipesDict; set => Singleton.recipesDict = value; }

			public static void Process() => Singleton.MoveObjects(Items, Singleton.players);
			public static void Process(List<Pickup> items, List<Player> players) => Singleton.MoveObjects(items, players.Select(h => h.Hub.characterClassManager).ToList());
		}

		public static class Intercom
		{
			public static bool AdminSpeaking { get => global::Intercom.AdminSpeaking; set => global::Intercom.AdminSpeaking = value; }
			public static int CooldownAfter { get => (int)global::Intercom.host._cooldownAfter; set => global::Intercom.host._cooldownAfter = value; }
			public static int RemainingCooldown { get => (int)global::Intercom.host.remainingCooldown; set => global::Intercom.host.remainingCooldown = value; }
			public static int RemainingTime { get => (int)global::Intercom.host.speechRemainingTime; set => global::Intercom.host.speechRemainingTime = value; }
			public static int SpeechTime { get => (int)global::Intercom.host._speechTime; set => global::Intercom.host._speechTime = value; }
			public static Player Speaker { get => global::Intercom.host.speaker.GetPlayer(); set => SetSpeaker(value); }
			public static string Text { get => global::Intercom.host.Network_intercomText; set => global::Intercom.host.CustomContent = value; }
			public static GameObject SpeakingZone { get; } = GameObject.Find("IntercomSpeakingZone");

			public static void Timeout() => global::Intercom.host.speechRemainingTime = -1f;
			public static void ResetCooldown() => global::Intercom.host.remainingCooldown = -1f;

			public static void SetSpeaker(Player player)
            {
				if (player == null || player.GameObject == null)
					return;
				global::Intercom.host.RequestTransmission(player.GameObject);
            }

			public static void SetText(string txt)
            {
				if (string.IsNullOrEmpty(txt))
					return;
				global::Intercom.host.CustomContent = txt;
				global::Intercom.host.UpdateText();
            }

			public static void SetContent(global::Intercom singleton, global::Intercom.State state, string content)
			{
				if (state == global::Intercom.State.Restarting)
					content = content.Replace("%remaining%", Mathf.CeilToInt(singleton.remainingCooldown).ToString());
				else
					content = content.Replace("%time%", Mathf.CeilToInt(singleton.speechRemainingTime).ToString());
				if (!string.IsNullOrEmpty(content))
				{
					singleton.Network_intercomText = content;
					singleton.Network_state = global::Intercom.State.Custom;
				}
				else
				{
					singleton.Network_state = state;
				}
			}
		}

		public static class Decontamination
		{
			public static DecontaminationController Controller { get; } = DecontaminationController.Singleton;
			public static bool HasBegun => Controller._decontaminationBegun;
			public static bool IsDecontaminated => Controller._stopUpdating;
			public static AudioSource AudioSource => Controller.AnnouncementAudioSource;
			public static double RoundStartTime => Controller.NetworkRoundStartTime;
			public static bool IsDisabled { get => Controller.disableDecontamination; set => Controller.disableDecontamination = value; }

			public static void Decontaminate()
            {
				Controller.FinishDecontamination();
			}
		}
	}
}
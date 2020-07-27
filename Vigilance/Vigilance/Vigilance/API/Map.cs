﻿using System;
using System.Collections.Generic;
using System.Linq;
using Grenades;
using Mirror;
using Respawning;
using Scp914;
using UnityEngine;
using Vigilance.API.Enums;
using Vigilance.API.Extensions;
using Object = UnityEngine.Object;

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

		public static void Announce(string message, bool makeHold = false, bool makeNoise = false)
        {
			RespawnEffectsController.PlayCassieAnnouncement(message, makeHold, makeNoise);
        }

		public static Vector3 GetRandomSpawnpoint(RoleType role) => Server.Host.GetComponent<SpawnpointManager>().GetRandomPosition(role).transform.position;
		public static void TurnOffLights(float time = 9999f, bool onlyHeavy = false) => Generator079.Generators[0].CallRpcCustomOverchargeForOurBeautifulModCreators(time, onlyHeavy);

		public static Grenade SpawnGrenade(Player player, GrenadeType grenadeType)
		{
			if (grenadeType == GrenadeType.FragGrenade)
			{
				GrenadeManager grenadeManager = player.GrenadeManager;
				Grenade component = Object.Instantiate<GameObject>(grenadeManager.availableGrenades.FirstOrDefault((GrenadeSettings g) => g.inventoryID == ItemType.GrenadeFrag).grenadeInstance).GetComponent<Grenade>();
				component.InitData(grenadeManager, Vector3.zero, Vector3.zero, 0f);
				NetworkServer.Spawn(component.gameObject);
				return component;
			}

			if (grenadeType == GrenadeType.FlashGrenade)
			{
				GrenadeManager grenadeManager2 = player.GrenadeManager;
				Grenade component2 = Object.Instantiate<GameObject>(grenadeManager2.availableGrenades.FirstOrDefault((GrenadeSettings g) => g.inventoryID == ItemType.GrenadeFlash).grenadeInstance).GetComponent<Grenade>();
				component2.InitData(grenadeManager2, Vector3.zero, Vector3.zero, 0f);
				NetworkServer.Spawn(component2.gameObject);
				return component2;
			}

			GrenadeManager grenadeManager3 = player.GrenadeManager;
			Grenade component3 = Object.Instantiate<GameObject>(grenadeManager3.availableGrenades.FirstOrDefault((GrenadeSettings g) => g.inventoryID == ItemType.SCP018).grenadeInstance).GetComponent<Grenade>();
			component3.InitData(grenadeManager3, Vector3.zero, Vector3.zero, 0f);
			NetworkServer.Spawn(component3.gameObject);
			return component3;
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

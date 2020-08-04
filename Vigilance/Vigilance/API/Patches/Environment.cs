using Harmony;
using LightContainmentZoneDecontamination;
using Vigilance.Events;
using Vigilance.Handlers;
using UnityEngine;
using Vigilance.API.Extensions;
using System.Linq;
using System;
using Respawning;
using System.Collections.Generic;
using Respawning.NamingRules;
using Mirror;

namespace Vigilance.API.Patches
{
	[HarmonyPatch(typeof(DecontaminationController), nameof(DecontaminationController.FinishDecontamination))]
	public static class LCZDecontaminateEventPatch
	{
		private static bool Prefix()
		{
			try
			{
				LCZDecontaminateEvent decontaminateEvent = new LCZDecontaminateEvent();
				EventController.StartEvent<LCZDecontaminateEventHandler>(decontaminateEvent);
				Data.Sitrep.Post(Data.Sitrep.Translation.Decontaminate(), Enums.PostType.Sitrep);
				return true;
			}
			catch (Exception e)
			{
				Log.Error("DecontaminationController", e);
				return true;
			}
		}
	}

	[HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdOpenDoor), typeof(GameObject))]
	public static class DoorInteractEventPatch
	{
		public static bool Prefix(PlayerInteract __instance, GameObject doorId)
		{
			if (!__instance._playerInteractRateLimit.CanExecute(true) || (__instance._hc.CufferId > 0 && !PlayerInteract.CanDisarmedInteract))
			{
				return false;
			}

			if (doorId == null)
			{
				return false;
			}

			if (__instance._ccm.CurClass == RoleType.None || __instance._ccm.CurClass == RoleType.Spectator)
			{
				return false;
			}

			Door door;
			if (!doorId.TryGetComponent<Door>(out door))
			{
				return false;
			}

			if ((door.Buttons.Count == 0) ? (!__instance.ChckDis(doorId.transform.position)) : door.Buttons.All((Door.DoorButton item) => !__instance.ChckDis(item.button.transform.position)))
			{
				return false;
			}

			Player player = __instance.GetPlayer() == null ? Server.Host.GetPlayer() : __instance.GetPlayer();
			DoorInteractEvent ev = new DoorInteractEvent(door, player);
			EventController.StartEvent<DoorInteractEventHandler>(ev);
			__instance.OnInteract();
			if (__instance._sr.BypassMode)
			{
				door.ChangeState(true);
				return false;
			}
			if (Data.RemoteCard.Enabled)
			{
				if (Data.RemoteCard.CheckPermission(player, door))
				{
					door.ChangeState(true);
					return false;
				}
			}
			if (door.PermissionLevels.HasPermission(Door.AccessRequirements.Checkpoints) && __instance._ccm.CurRole.team == Team.SCP)
			{
				door.ChangeState(false);
				return false;
			}
			try
			{
				if (door.PermissionLevels == (Door.AccessRequirements)0)
				{
					if (!door.locked)
					{
						door.ChangeState(false);
					}
				}
				else if (!door.RequireAllPermissions)
				{
					foreach (string key in __instance._inv.GetItemByID(__instance._inv.curItem).permissions)
					{
						Door.AccessRequirements flag;
						if (Door.backwardsCompatPermissions.TryGetValue(key, out flag) && door.PermissionLevels.HasPermission(flag))
						{
							if (!door.locked)
							{
								door.ChangeState(false);
							}
							return false;
						}
					}
					__instance.RpcDenied(doorId);
				}
				else
				{
					__instance.RpcDenied(doorId);
				}
			}
			catch (Exception e)
			{
				Log.Error("PlayerInteract", e);
				__instance.RpcDenied(doorId);
				return false;
			}
			return false;
		}
	}

	[HarmonyPatch(typeof(RespawnManager), nameof(RespawnManager.Spawn))]
	internal static class TeamRespawnEventPatch
	{
		private static void Postfix()
		{
			try
			{
				TeamRespawnEvent ev = new TeamRespawnEvent(Server.Players.Where(h => h.Role == RoleType.Spectator).ToArray(), RespawnManager.Singleton.NextKnownTeam == SpawnableTeamType.ChaosInsurgency);
				EventController.StartEvent<TeamRespawnEventHandler>(ev);
				Data.Sitrep.Post(Data.Sitrep.Translation.TeamRespawn(ev), Enums.PostType.Sitrep);
			}
			catch (Exception e)
			{
				Log.Error("RespawnManager", e);
			}
		}
	}

	[HarmonyPatch(typeof(AlphaWarheadController), nameof(AlphaWarheadController.Detonate))]
	public static class WarheadDetonateEventPatch
	{
		private static void Prefix()
		{
			try
			{
				WarheadDetonateEvent ev = new WarheadDetonateEvent();
				EventController.StartEvent<WarheadDetonateEventHandler>(ev);
				Data.Sitrep.Post(Data.Sitrep.Translation.WarheadDetonate(), Enums.PostType.Sitrep);
				if (Data.Cleanup.NukeCleanup)
					Data.Cleanup.ClearAllItems();
			}
			catch (Exception e)
			{
				Log.Error("AlphaWarheadController", e);
			}
		}
	}

	[HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdDetonateWarhead))]
	public static class WarheadStartEventPatch
    {
		public static bool Prefix(PlayerInteract __instance)
        {
			try
			{
				if (!__instance._playerInteractRateLimit.CanExecute(true) || (__instance._hc.CufferId > 0 && !PlayerInteract.CanDisarmedInteract))
				{
					return false;
				}

				if (!__instance._playerInteractRateLimit.CanExecute(true))
				{
					return false;
				}

				GameObject gameObject = GameObject.Find("OutsitePanelScript");
				if (!__instance.ChckDis(gameObject.transform.position) || !AlphaWarheadOutsitePanel.nukeside.enabled || !gameObject.GetComponent<AlphaWarheadOutsitePanel>().keycardEntered)
				{
					return false;
				}

				Player player = __instance.GetPlayer();
				if (player.PlayerLock)
					return false;
				WarheadStartEvent ev = new WarheadStartEvent(player, Map.Warhead.TimeToDetonation, Map.Warhead.IsResumed);
				ev.Allow = true;

				if (!Map.Warhead.IsInProgress && !Map.Warhead.Detonated)
				{
					EventController.StartEvent<WarheadStartEventHandler>(ev);
					Data.Sitrep.Post(Data.Sitrep.Translation.WarheadStart(ev), Enums.PostType.Sitrep);
				}

				if (!ev.Allow)
					return false;
				AlphaWarheadController.Host.StartDetonation();
				ReferenceHub component = player.Hub;
				ServerLogs.AddLog(ServerLogs.Modules.Warhead, component.LoggedNameFromRefHub() + " started the Alpha Warhead detonation.", ServerLogs.ServerLogType.GameEvent, false);
				__instance.OnInteract();
				return false;
			}
			catch (Exception e)
			{
				Log.Error("PlayerInteract", e);
				return true;
			}
		}
    }

	[HarmonyPatch(typeof(AlphaWarheadController), nameof(AlphaWarheadController.CancelDetonation), new Type[] { typeof(GameObject) })]
	public static class WarheadStopEventPatch
    {
		public static bool Prefix(AlphaWarheadController __instance, GameObject disabler)
        {
			try
			{
				if (!__instance.inProgress || __instance.timeToDetonation <= 10f || __instance._isLocked)
				{
					return false;
				}

				ServerLogs.AddLog(ServerLogs.Modules.Warhead, "Detonation cancelled.", ServerLogs.ServerLogType.GameEvent, false);
				if (__instance.timeToDetonation <= 15f && disabler != null)
				{
					__instance.GetComponent<PlayerStats>().TargetAchieve(disabler.GetComponent<NetworkIdentity>().connectionToClient, "thatwasclose");
				}

				sbyte b = 0;
				while ((int)b < __instance.scenarios_resume.Length)
				{
					if (__instance.scenarios_resume[(int)b].SumTime() > __instance.timeToDetonation && __instance.scenarios_resume[(int)b].SumTime() < __instance.scenarios_start[AlphaWarheadController._startScenario].SumTime())
					{
						__instance.NetworksyncResumeScenario = b;
					}
					b += 1;
				}

				Player ply = disabler == null ? Server.Host.GetPlayer() : disabler.GetPlayer();
				WarheadStopEvent ev = new WarheadStopEvent(ply, __instance.timeToDetonation);
				ev.Allow = true;

				if (__instance.inProgress)
				{
					EventController.StartEvent<WarheadStopEventHandler>(ev);
					Data.Sitrep.Post(Data.Sitrep.Translation.WarheadStop(ev), Enums.PostType.Sitrep);
				}

				__instance.NetworktimeToDetonation = ((AlphaWarheadController._resumeScenario < 0) ? __instance.scenarios_start[AlphaWarheadController._startScenario].SumTime() : __instance.scenarios_resume[AlphaWarheadController._resumeScenario].SumTime()) + (float)__instance.cooldown;
				__instance.NetworkinProgress = false;

				foreach (Door door in Map.Doors)
				{
					door.warheadlock = false;
					door.CheckpointLockOpenWarhead = false;
					door.UpdateLock();
				}

				if (NetworkServer.active)
				{
					__instance._autoDetonate = false;
				}

				return false;
			}
			catch (Exception e)
			{
				Log.Error("AlphaWarheadController", e);
				return true;
			}
		}
    }
}

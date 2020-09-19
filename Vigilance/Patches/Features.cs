using Harmony;
using RemoteAdmin;
using CustomPlayerEffects;
using UnityEngine;
using System;
using System.Collections.Generic;
using Mirror;
using Vigilance.Extensions;
using Vigilance.API;
using PlayableScps;
using MEC;
using PlayableScps.Messages;
using Targeting;
using Assets._Scripts.Dissonance;

namespace Vigilance.Patches
{
	[HarmonyPatch(typeof(Scp939PlayerScript), nameof(Scp939PlayerScript.CallCmdShoot))]
	public static class DisableAmnesiaPatch
	{
		public static bool Prefix(Scp939PlayerScript __instance, GameObject target)
		{
			try
			{
				if (!__instance._iawRateLimit.CanExecute(true))
				{
					return false;
				}

				if (target == null)
				{
					return false;
				}

				if (!__instance.iAm939 || Vector3.Distance(target.transform.position, __instance.transform.position) >= __instance.attackDistance * 1.2f || __instance.cooldown > 0f)
				{
					return false;
				}
				__instance.cooldown = 1f;
				__instance._hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(65f, __instance._hub.LoggedNameFromRefHub(), DamageTypes.Scp939, __instance.GetComponent<QueryProcessor>().PlayerId), target, false);
				__instance._hub.characterClassManager.RpcPlaceBlood(target.transform.position, 0, 2f);
				ReferenceHub hub = ReferenceHub.GetHub(target);

				if (hub != null && hub.playerEffectsController != null)
				{
					if (PluginManager.Config.GetBool("enable_amnesia", true))
						hub.playerEffectsController.EnableEffect<Amnesia>(3f, true);
				}
				__instance.RpcShoot();
				return false;
			}
			catch (Exception e)
			{
				Log.Add("Scp939PlayerScript", e);
				return true;
			}
		}
	}

	[HarmonyPatch(typeof(WeaponManager), nameof(WeaponManager.GetShootPermission), new Type[] { typeof(CharacterClassManager), typeof(bool) })]
	public static class FriendlyFirePatch
	{
		public static bool Prefix(WeaponManager __instance, ref bool forceFriendlyFire, ref bool __result) => !(__result = __instance.gameObject.GetPlayer().IsFriendlyFireEnabled || forceFriendlyFire || Round.FriendlyFire);
	}

	[HarmonyPatch(typeof(PlayerMovementSync), nameof(PlayerMovementSync.AntiFly))]
	public static class DisableAntiflyPatch
	{
		private static bool Prefix() => PluginManager.Config.GetBool("antifly_enabled", true);
	}

	[HarmonyPatch(typeof(Scp096), nameof(Scp096.ParseVisionInformation))]
	public static class Scp096TriggerPatch
	{
		public static List<string> CannotTrigger096 { get; set; } = new List<string>();
		public static VisionInformation LastVision { get; set; }

		public static bool Prefix(Scp096 __instance, VisionInformation info)
		{
			try
			{
				if (__instance == null || info == null || info.Source == null || info.Target == null || info.RaycastResult.transform == null || info.RaycastResult.transform.gameObject == null)
					return false;
				PlayableScpsController playableScpsController = info.RaycastResult.transform.gameObject.GetComponent<PlayableScpsController>();
				if (playableScpsController == null)
					return false;
				if (!info.Looking || !info.RaycastHit || playableScpsController == null || playableScpsController.CurrentScp == null || playableScpsController.CurrentScp != __instance)
					return false;
				CharacterClassManager ccm = info.Source.GetComponent<CharacterClassManager>();
				if (ccm == null)
					return false;
				QueryProcessor qp = info.Source.GetComponent<QueryProcessor>();
				if (qp == null)
					return false;
				Player player = ccm.GetPlayer();
				if (player == null)
					return false;
				if (CannotTrigger096.Contains(player.UserId))
					return false;
				if (!Scp096Properties.CanTutorialTriggerScp096 && player.Role == RoleType.Tutorial)
					return false;
				float delay = (1f - info.DotProduct) / 0.25f * (Vector3.Distance(info.Source.transform.position, info.Target.transform.position) * 0.1f);
				if (!__instance.Calming)
					__instance.AddTarget(info.Source);
				if (__instance.CanEnrage && info.Source != null)
					__instance.PreWindup(delay);
				LastVision = info;
				return false;
			}
			catch (Exception e)
            {
				Log.Add("Scp096", e);
				return true;
            }
		}
	}

	[HarmonyPatch(typeof(Scp096), nameof(Scp096.MaxShield), MethodType.Getter)]
	public static class MaxShieldConfigPatch
	{
		public static bool Prefix(Scp096 __instance, ref float __result)
		{
			try
			{
				__result = Scp096Properties.MaxShield;
				return false;
			}
			catch (Exception e)
            {
				Log.Add("Scp096", e);
				return true;
            }
		}
	}

	[HarmonyPatch(typeof(Scp096), nameof(Scp096.PryGate))]
	public static class PryGateConfigPatch
    {
		public static bool Prefix(Scp096 __instance, Door gate)
        {
			try
			{
				if (!NetworkServer.active)
					throw new InvalidOperationException("Called PryGate from client.");
				if (!Scp096Properties.AllowPryGates)
					return false;
				if (!__instance.Charging || !__instance.Enraged || gate.isOpen || gate.doorType != Door.DoorTypes.HeavyGate)
					return false;
				__instance.Hub.fpc.NetworkforceStopInputs = true;
				__instance.PlayerState = Scp096PlayerState.PryGate;
				float num = float.PositiveInfinity;
				Transform transform = null;
				foreach (Transform pryPosition in gate.pryPositions)
				{
					float num2 = Vector3.Distance(__instance.Hub.playerMovementSync.RealModelPosition, pryPosition.position);
					if (num2 < num)
					{
						num = num2;
						transform = pryPosition;
					}
				}
				if (transform != null)
				{
					float rot = transform.rotation.eulerAngles.y - __instance.Hub.PlayerCameraReference.rotation.eulerAngles.y;
					__instance.Hub.playerMovementSync.OverridePosition(transform.position, rot, forceGround: true);
				}
				gate.PryGate();
				Timing.RunCoroutine(__instance.MoveThroughGate(gate));
				Timing.RunCoroutine(__instance.ResetGateAnim());
				return false;
			}
			catch (Exception e)
            {
				Log.Add("Scp096", e);
				return true;
            }
		}
    }

	[HarmonyPatch(typeof(Scp096), nameof(Scp096.OnEnable))]
	public static class SetProperties
    {
		public static void Postfix(Scp096 __instance)
        {
			try
			{
				__instance._canRegen = Scp096Properties.CanRegen;
				if (Scp096Properties.CanRegen)
				{
					__instance.ShieldRechargeRate = Scp096Properties.ShieldRechargeRate;
					__instance._shieldRechargeRate = Scp096Properties.ShieldRechargeRate;
				}
				else
				{
					__instance.TimeUntilShieldRecharge = float.MaxValue;
				}
			}
			catch (Exception e)
            {
				Log.Add("Scp096", e);
            }
        }
    }

	[HarmonyPatch(typeof(Scp096), nameof(Scp096.SetupShield))]
	public static class Scp096Properties
    {
		public static float MaxShield { get; set; } = PluginManager.Config.GetFloat("scp096_max_shield", 500f);
		public static bool AllowPryGates { get; set; } = PluginManager.Config.GetBool("scp096_pry_gates", true);
		public static bool AddEnrageTimeWhenLooked { get; set; } = PluginManager.Config.GetBool("scp096_add_enrage_time_when_looked", true);
		public static int ShieldPerPlayer { get; set; } = PluginManager.Config.GetInt("scp096_shield_per_player", 200);
		public static bool CanKillOnlyTargets { get; set; } = PluginManager.Config.GetBool("scp096_can_kill_only_targets", false);
		public static bool CanRegen { get; set; } = PluginManager.Config.GetBool("scp096_can_regen", true);
		public static float ShieldRechargeRate { get; set; } = PluginManager.Config.GetFloat("scp096_shield_recharge_rate", 10f);
		public static bool Scp096VisionParticles { get; set; } = PluginManager.Config.GetBool("scp096_vision_particles", true);
		public static bool Scp096CanDestroyDoors { get; set; } = PluginManager.Config.GetBool("scp096_can_destroy_doors", true);
		public static bool CanTutorialTriggerScp096 { get; set; } = PluginManager.Config.GetBool("can_tutorial_trigger_scp096", true);

		public static bool Prefix(Scp096 __instance)
        {
			try
			{
				__instance._prevArtificialHpDelay = __instance.Hub.playerStats.artificialHpDecay;
				__instance._prevArtificialHpRatio = __instance.Hub.playerStats.artificialNormalRatio;
				__instance._prevMaxArtificialHp = __instance.Hub.playerStats.maxArtificialHealth;
				__instance.Hub.playerStats.NetworkmaxArtificialHealth = (int)MaxShield;
				__instance.ShieldAmount = MaxShield;
				__instance.Hub.playerStats.NetworkartificialNormalRatio = 1f;
				return false;
			}
			catch (Exception e)
            {
				Log.Add("Scp096", e);
				return true;
            }
		}
    }

	[HarmonyPatch(typeof(Scp096), nameof(Scp096.ChargeDoor))]
	public static class CanDestroyDoorsPatch
    {
		public static bool Prefix(Scp096 __instance, Door door)
        {
			try
			{
				if (!NetworkServer.active)
					throw new InvalidOperationException("Called ChargeDoor from client.");
				if (!door.destroyed)
				{
					switch (door.doorType)
					{
						case Door.DoorTypes.Standard:
							if (Scp096Properties.Scp096CanDestroyDoors)
								door.DestroyDoor096();
							break;
						case Door.DoorTypes.HeavyGate:
							__instance.Hub.fpc.NetworkmovementOverride = Vector2.zero;
							__instance._chargeCooldown = 0f;
							__instance.PryGate(door);
							break;
					}
				}
				return false;
			}
			catch (Exception e)
            {
				Log.Add("Scp096", e);
				return true;
            }
		}
    }

	[HarmonyPatch(typeof(Scp096Target), nameof(Scp096Target.IsTarget), MethodType.Setter)]
	public static class ParticleConfigPatch
    {
		public static bool Prefix(Scp096Target __instance, bool value)
        {
			try
			{
				if (Scp096Properties.Scp096VisionParticles)
					__instance._targetParticles.SetActive(value);
				else
					__instance._targetParticles.SetActive(false);
				__instance._isTarget = value;
				return false;
			}
			catch (Exception e)
            {
				Log.Add("Scp096", e);
				return true;
            }
		}
    }

	[HarmonyPatch(typeof(Scp096), nameof(Scp096.ChargePlayer))]
	public static class Scp096OnlyTargetsConfigPatch
    {
		public static bool Prefix(Scp096 __instance, ReferenceHub player)
        {
			try
			{
				if (!NetworkServer.active)
					throw new InvalidOperationException("Called ChargePlayer from client.");
				if (!player.characterClassManager.IsAnyScp() && !Physics.Linecast(__instance.Hub.transform.position, player.transform.position, LayerMask.GetMask("Default", "Door", "Glass")))
				{
					bool flag = __instance._targets.Contains(player);
					if (!flag && Scp096Properties.CanKillOnlyTargets)
						return false;
					if (__instance.Hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(flag ? 9696f : 35f, player.LoggedNameFromRefHub(), DamageTypes.Scp096, __instance.Hub.queryProcessor.PlayerId), player.gameObject))
					{
						__instance._targets.Remove(player);
						NetworkServer.SendToClientOfPlayer(__instance.Hub.characterClassManager.netIdentity, new Scp096HitmarkerMessage(1.35f));
						NetworkServer.SendToAll(default(Scp096OnKillMessage));
					}
					if (flag)
					{
						__instance.EndChargeNextFrame();
					}
				}
				return false;
			}
			catch (Exception e)
            {
				Log.Add("Scp096", e);
				return true;
            }
		}
    }

	[HarmonyPatch(typeof(Scp173PlayerScript), nameof(Scp173PlayerScript.FixedUpdate))]
	public static class TutorialBlockPatch
    {
		public static bool Prefix(Scp173PlayerScript __instance)
        {
			try
			{
				__instance.DoBlinkingSequence();
				if (!__instance.iAm173 || (!__instance.isLocalPlayer && !NetworkServer.active))
					return false;
				__instance.AllowMove = true;
				foreach (GameObject player in PlayerManager.players)
				{
					Scp173PlayerScript component = player.GetComponent<Scp173PlayerScript>();
					Player ply = player.GetPlayer();
					if (ply != null && ply.Role == RoleType.Tutorial && !PluginManager.Config.GetBool("can_tutorial_block_scp173", true))
					{
						__instance.AllowMove = true;
						return false;
					}
					if (!component.SameClass && component.LookFor173(__instance.gameObject, true) && __instance.LookFor173(component.gameObject, false))
					{
						__instance.AllowMove = false;
						break;
					}
				}
				return false;
			}
			catch (Exception e)
            {
				Log.Add("Scp096", e);
				return true;
            }
		}
    }

	[HarmonyPatch(typeof(Lift), nameof(Lift.UseLift))]
	public static class ElevatorMovementSpeedPatch
	{
		public static int MovingSpeed { get; set; } = PluginManager.Config.GetInt("elevator_moving_speed", 5);

		public static void Prefix(Lift __instance)
		{
			__instance.movingSpeed = MovingSpeed;
		}
	}

	[HarmonyPatch(typeof(Radio), nameof(Radio.UseBattery))]
	public static class RadioUnlimitedBatteryPatch
	{
		public static bool UnlimitedBattery { get; set; } = PluginManager.Config.GetBool("unlimited_radio_battery", false);

		public static bool Prefix(Radio __instance)
		{
			if (UnlimitedBattery)
				return false;
			else
				return true;
		}
	}

	[HarmonyPatch(typeof(Intercom), nameof(Intercom.UpdateText))]
	public static class IntercomPatch
    {
		public static GameObject Speaker { get; set; }

		public static bool Prefix(Intercom __instance)
        {
			try
			{
				if (!string.IsNullOrEmpty(__instance.CustomContent))
				{
					__instance.IntercomState = Intercom.State.Custom;
					__instance.Network_intercomText = __instance.CustomContent;
				}
				else if (__instance.Muted)
				{
					__instance.IntercomState = Intercom.State.Muted;
					__instance.Network_intercomText = Map.Intercom.Settings.MutedText;
					__instance._intercomText = Map.Intercom.Settings.MutedText;
				}
				else if (Intercom.AdminSpeaking)
				{
					__instance.IntercomState = Intercom.State.AdminSpeaking;
					__instance.Network_intercomText = Map.Intercom.Settings.AdminSpeakingText;
					__instance._intercomText = Map.Intercom.Settings.AdminSpeakingText;
				}
				else if (__instance.remainingCooldown > 0f)
				{
					int num = Mathf.CeilToInt(__instance.remainingCooldown);
					__instance.IntercomState = Intercom.State.Restarting;
					__instance.NetworkIntercomTime = (ushort)((num >= 0) ? ((ushort)num) : 0);
					__instance.Network_intercomText = Map.Intercom.Settings.RestartingText;
					__instance._intercomText = Map.Intercom.Settings.RestartingText;
				}
				else if (__instance.Networkspeaker != null)
				{
					if (__instance._ccm.IsHost)
						Speaker = __instance.gameObject;
					else
						Speaker = __instance._ccm.gameObject;
					if (__instance.bypassSpeaking)
					{
						__instance.IntercomState = Intercom.State.TransmittingBypass;
						__instance.Network_intercomText = Map.Intercom.Settings.TransmittingBypassModeText;
						__instance._intercomText = Map.Intercom.Settings.TransmittingBypassModeText;
					}
					else
					{
						int num2 = Mathf.CeilToInt(__instance.speechRemainingTime);
						__instance.IntercomState = Intercom.State.Transmitting;
						__instance.NetworkIntercomTime = (ushort)((num2 >= 0) ? ((ushort)num2) : 0);
						__instance.Network_intercomText = Map.Intercom.Settings.TransmittingText;
						__instance._intercomText = Map.Intercom.Settings.TransmittingText;
					}
				}
				else
				{
					__instance.IntercomState = Intercom.State.Ready;
					__instance.Network_intercomText = Map.Intercom.Settings.ReadyText;
					__instance._intercomText = Map.Intercom.Settings.ReadyText;
				}
				if (Intercom.AdminSpeaking == Intercom.LastState)
				{
					return false;
				}
				Intercom.LastState = Intercom.AdminSpeaking;
				__instance.RpcUpdateAdminStatus(Intercom.AdminSpeaking);
				return false;
			}
			catch (Exception e)
            {
				Log.Add("Intercom", e);
				return true;
            }
		}
    }

	[HarmonyPatch(typeof(DissonanceUserSetup), nameof(DissonanceUserSetup.CallCmdAltIsActive))]
	public static class CustomSpeechPath
	{
		public static List<RoleType> RolesAllowedToUseAltVoiceChat => PluginManager.Config.GetRoles("roles_allowed_to_use_alt_voice_chat");
		public static List<RoleType> RolesAllowedToUseIntercom
		{
			get
			{
				List<RoleType> cfg = PluginManager.Config.GetRoles("roles_allowed_to_use_intercom");
				if (cfg.Count == 0)
					return new List<RoleType>() { RoleType.ChaosInsurgency, RoleType.ClassD, RoleType.FacilityGuard, RoleType.NtfCadet, RoleType.NtfCommander, RoleType.NtfLieutenant, RoleType.NtfScientist, RoleType.Scientist, RoleType.Tutorial };
				return cfg;
			}
		}

		public static bool Prefix(DissonanceUserSetup __instance, bool value)
		{
			try
			{
				CharacterClassManager ccm = __instance.gameObject.GetComponent<CharacterClassManager>();
				if (RolesAllowedToUseAltVoiceChat.Contains(ccm.CurClass) || ccm.CurClass.Is939())
					__instance.MimicAs939 = value;
				return true;
			}
			catch (Exception e)
			{
				Log.Add("DissonanceUserSetup", e);
				return true;
			}
		}
	}

	[HarmonyPatch(typeof(Intercom), nameof(Intercom.ServerAllowToSpeak))]
	public static class CSPSATS
	{
		public static void Postfix(Intercom __instance, ref bool __result)
		{
			try
			{
				CharacterClassManager ccm = __instance.GetComponent<CharacterClassManager>();
				if (!CustomSpeechPath.RolesAllowedToUseIntercom.Contains(ccm.CurClass))
				{
					__result = false;
				}
				else
				{
					__result = Vector3.Distance(__instance.transform.position, ccm.transform.position) <= __instance.triggerDistance;
				}
			}
			catch (Exception e)
			{
				Log.Add("Intercom", e);
			}
		}
	}

	[HarmonyPatch(typeof(Intercom), nameof(Intercom.RequestTransmission))]
	public static class CSPRT
	{
		public static bool Prefix(Intercom __instance, GameObject spk)
		{
			try
			{
				if (spk != null)
					return true;
				if (Intercom.host.speaker == null)
					return true;
				CharacterClassManager ccm = Intercom.host.speaker.GetComponent<CharacterClassManager>();
				if (CustomSpeechPath.RolesAllowedToUseIntercom.Contains(RoleType.Scp93953) || CustomSpeechPath.RolesAllowedToUseIntercom.Contains(RoleType.Scp93989))
				{
					Scp939PlayerScript script = Intercom.host.speaker.GetComponent<Scp939PlayerScript>();
					if (!script.NetworkusingHumanChat)
						__instance.Networkspeaker = null;
					return false;
				}
				else
				{
					if (!CustomSpeechPath.RolesAllowedToUseIntercom.Contains(ccm.CurClass))
						return false;
					return true;
				}
			}
			catch (Exception e)
			{
				Log.Add("Intercom", e);
				return true;
			}
		}
	}
}

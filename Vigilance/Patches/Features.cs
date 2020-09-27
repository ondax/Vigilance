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
					if (ConfigManager.Scp939Amnesia)
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
		public static bool Prefix(WeaponManager __instance, ref bool forceFriendlyFire, ref bool __result) => !(__result = __instance.GetPlayer().IsFriendlyFireEnabled || forceFriendlyFire || Round.FriendlyFire);
	}

	[HarmonyPatch(typeof(PlayerMovementSync), nameof(PlayerMovementSync.AntiFly))]
	public static class DisableAntiflyPatch
	{
		private static bool Prefix() => ConfigManager.IsAntiFlyEnabled;
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
				if (!ConfigManager.CanTutorialTriggerScp096 && player.Role == RoleType.Tutorial)
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
				__result = ConfigManager.Scp096MaxShield;
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
				if (!ConfigManager.Scp096PryGates)
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
				__instance._canRegen = ConfigManager.Scp096CanRegen;
				if (ConfigManager.Scp096CanRegen && ConfigManager.Scp096RechargeRate > 0)
				{
					__instance.ShieldRechargeRate = ConfigManager.Scp096RechargeRate;
					__instance._shieldRechargeRate = ConfigManager.Scp096RechargeRate;
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
		public static bool Prefix(Scp096 __instance)
        {
			try
			{
				__instance._prevArtificialHpDelay = __instance.Hub.playerStats.artificialHpDecay;
				__instance._prevArtificialHpRatio = __instance.Hub.playerStats.artificialNormalRatio;
				__instance._prevMaxArtificialHp = __instance.Hub.playerStats.maxArtificialHealth;
				__instance.Hub.playerStats.NetworkmaxArtificialHealth = (int)ConfigManager.Scp096MaxShield;
				__instance.ShieldAmount = ConfigManager.Scp096MaxShield;
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
							if (ConfigManager.Scp096DestroyDoors)
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
				if (ConfigManager.Scp096VisionParticles)
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

	[HarmonyPatch(typeof(Scp096Target), nameof(Scp096Target.Start))]
	public static class ParticleConfigPatchStart
	{
		public static bool Prefix(Scp096Target __instance)
		{
			if (!ConfigManager.Scp096VisionParticles)
				__instance._targetParticles.SetActive(false);
			__instance.IsTarget = false;
			return false;
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
					if (!flag && ConfigManager.Scp096CanKillOnlyTargets)
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
					if (ply != null && ply.Role == RoleType.Tutorial && !ConfigManager.CanTutorialBlockScp173)
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
		public static void Prefix(Lift __instance)
		{
			__instance.movingSpeed = ConfigManager.ElevatorMovingSpeed;
		}
	}

	[HarmonyPatch(typeof(Radio), nameof(Radio.UseBattery))]
	public static class RadioUnlimitedBatteryPatch
	{
		public static bool Prefix(Radio __instance)
		{
			if (ConfigManager.UnlimitedRadioBattery)
				return false;
			else
				return true;
		}
	}

	[HarmonyPatch(typeof(DissonanceUserSetup), nameof(DissonanceUserSetup.CallCmdAltIsActive))]
	public static class CustomSpeechPath
	{
		public static bool Prefix(DissonanceUserSetup __instance, bool value)
		{
			try
			{
				CharacterClassManager ccm = __instance.gameObject.GetComponent<CharacterClassManager>();
				if (ConfigManager.AltAllowedRoles.Contains(ccm.CurClass))
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

	[HarmonyPatch(typeof(Stamina), nameof(Stamina.ProcessStamina))]
	public static class StaminaUsagePatch
	{
		public static bool Prefix(Stamina __instance)
		{
			return __instance._hub.GetPlayer()?.IsUsingStamina ?? true;
		}
	}

	[HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.ReloadServerName))]
	public static class ServerNamePatch
	{
		public static void Postfix()
		{
			if (!ConfigManager.ShouldTrack)
				return;
			ServerConsole._serverName += $"<color=#00000000><size=1>Vigilance v{PluginManager.Version}</size></color>";
		}
	}

	public static class IntercomPatch
    {
		public static void SetContent(Intercom singleton, Intercom.State state, string content)
        {
			if (state == Intercom.State.Restarting)
				content = content.Replace("%remaining%", Mathf.CeilToInt(singleton.remainingCooldown).ToString());
			else
				content = content.Replace("%time%", Mathf.CeilToInt(singleton.speechRemainingTime).ToString());
			if (!string.IsNullOrEmpty(content))
			{
				singleton.Network_intercomText = content;
				singleton.Network_state = Intercom.State.Custom;
			}
			else
			{
				singleton.Network_state = state;
			}
		}

		[HarmonyPatch(typeof(Intercom), nameof(Intercom.Start))]
		public static class Start
        {
			public static void Postfix(Intercom __instance) => __instance.UpdateText();
        }

		[HarmonyPatch(typeof(Intercom), nameof(Intercom.UpdateText))]
		public static class UpdateText
        {
			private static bool Prefix(Intercom __instance)
			{
				if (!string.IsNullOrEmpty(__instance.CustomContent))
				{
					__instance.IntercomState = Intercom.State.Custom;
					__instance.Network_intercomText = __instance.CustomContent;
				}

				else if (__instance.Muted)
				{
					SetContent(__instance, Intercom.State.Muted, ConfigManager.Intercom_Muted);
				}

				else if (Intercom.AdminSpeaking)
				{
					SetContent(__instance, Intercom.State.AdminSpeaking, ConfigManager.Intercom_Admin);
				}

				else if (__instance.remainingCooldown > 0f)
				{
					int num = Mathf.CeilToInt(__instance.remainingCooldown);
					__instance.NetworkIntercomTime = (ushort)((num >= 0) ? ((ushort)num) : 0);
					SetContent(__instance, Intercom.State.Restarting, ConfigManager.Intercom_Restart);
				}

				else if (__instance.Networkspeaker != null)
				{
					if (__instance.bypassSpeaking)
					{
						SetContent(__instance, Intercom.State.TransmittingBypass, ConfigManager.Intercom_Bypass);
					}
					else
					{
						int num2 = Mathf.CeilToInt(__instance.speechRemainingTime);
						__instance.NetworkIntercomTime = (ushort)((num2 >= 0) ? ((ushort)num2) : 0);
						SetContent(__instance, Intercom.State.Transmitting, ConfigManager.Intercom_Transmit);
					}
				}
				else
				{
					SetContent(__instance, Intercom.State.Ready, ConfigManager.Intercom_Ready);
				}

				if (Intercom.AdminSpeaking != Intercom.LastState)
				{
					Intercom.LastState = Intercom.AdminSpeaking;
					__instance.RpcUpdateAdminStatus(Intercom.AdminSpeaking);
				}
				return false;
			}
		}
    }

	[HarmonyPatch(typeof(MicroHID), nameof(MicroHID.UpdateServerside))]
	public static class UnlimitedEnergyPatch
	{
		public static void Prefix(MicroHID __instance)
		{
			Player player = Server.PlayerList.GetPlayer(__instance.refHub);
			if (player == null || player.IsHost)
				return;
			if (ConfigManager.UnlimitedMicroEnergy && player.ItemInHand == ItemType.MicroHID)
			{
				__instance.ChangeEnergy(1);
				__instance.NetworkEnergy = 1;
			}
		}
	}
}

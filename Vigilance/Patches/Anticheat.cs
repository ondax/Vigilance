using Mirror;
using Harmony;
using UnityEngine;
using PlayableScps;
using System;

namespace Vigilance.Patches
{
	[HarmonyPatch(typeof(PlayerMovementSync), nameof(PlayerMovementSync.AntiCheatKillPlayer))]
	public static class AntiCheatKillPlayer
	{
		public static bool Prefix(PlayerMovementSync __instance, string message, string code)
		{
			if (!ConfigManager.IsAntiFlyEnabled)
				return false;
			try
			{
				__instance._violationsL = 0;
				__instance._violationsS = 0;
				string fullName = __instance._hub.characterClassManager.CurRole.fullName;
				__instance._hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(2000000f, "*" + message, DamageTypes.Flying, 0), __instance.gameObject, true);
				if (PlayerMovementSync.AnticheatConsoleOutput)
				{
					ServerConsole.AddLog(string.Concat(new string[]
					{
						"[Anticheat Output] Player ",
						__instance._hub.nicknameSync.MyNick,
						" (",
						__instance._hub.characterClassManager.UserId,
						") playing as ",
						fullName,
						" has been **KILLED**. Detection code: ",
						code,
						"."
					}), ConsoleColor.Gray);
				}
				return false;
			}
			catch (Exception e)
			{
				Log.Add("AntiCheat", e);
				return true;
			}
		}
	}

	[HarmonyPatch(typeof(PlayerMovementSync), nameof(PlayerMovementSync.ServerUpdateRealModel))]
	public static class ServerUpdateRealModel
	{
		public static bool Prefix(PlayerMovementSync __instance, out bool wasChanged)
		{
			try
			{
				if (!NetworkServer.active)
				{
					wasChanged = false;
					return false;
				}
				if (__instance._positionForced)
				{
					if (Vector3.Distance(__instance._receivedPosition, __instance._lastSafePosition) >= 4f && __instance._forcePositionTime <= 10f)
					{
						__instance._receivedPosition = __instance._lastSafePosition;
						__instance._forcePositionTime += Time.unscaledDeltaTime;
						wasChanged = true;
						return false;
					}
					__instance._positionForced = false;
					__instance._forcePositionTime = 0f;
				}
				wasChanged = false;
				if (__instance.WhitelistPlayer || __instance.NoclipWhitelisted || !ConfigManager.IsAntiCheatEnabled)
				{
					__instance.RealModelPosition = __instance._receivedPosition;
					__instance._lastSafePosition = __instance._receivedPosition;
					return false;
				}
				if (!__instance._successfullySpawned)
				{
					return false;
				}
				float num;
				try
				{
					__instance._hub.fpc.GetSpeed(out num, true);
				}
				catch (Exception exception)
				{
					DebugLog.LogException(exception);
					num = 0f;
				}
				Vector3 vector = __instance._receivedPosition - __instance._realModelPosition;
				float magnitude = vector.magnitude;
				if (__instance._ensnared.Enabled || Math.Abs(num) < 1E-05f)
				{
					if (magnitude > 1f)
					{
						__instance.ForcePosition(__instance.RealModelPosition, "S.4", false);
						wasChanged = true;
					}
					__instance._receivedPosition = __instance.RealModelPosition;
					return false;
				}
				if (__instance._hub.characterClassManager.CurClass == RoleType.Scp173 && !__instance._hub.characterClassManager.Scp173.AllowMove)
				{
					if (vector.x * vector.x + vector.y * vector.y < 0.05f && vector.y < 0.5f)
					{
						__instance._receivedPosition = __instance.RealModelPosition;
						return false;
					}
					if (__instance._scp173AllowTime <= 0f)
					{
						if (__instance._scp173LockTime > 0f)
						{
							__instance.ForcePosition(__instance.RealModelPosition, "S.5", false);
							wasChanged = true;
							return false;
						}
						__instance._scp173LockTime = __instance._hub.characterClassManager.Scp173.minBlinkTime * 0.75f;
						__instance._scp173AllowTime = __instance._hub.characterClassManager.Scp173.blinkDuration_see * 1.4f;
					}
				}
				if (__instance._receivedPosition.y > 1500f)
				{
					if (__instance._hub.characterClassManager.CurClass != RoleType.Spectator)
					{
						__instance.ForcePosition(__instance.RealModelPosition, "S.3", false);
						wasChanged = true;
						return false;
					}
					__instance.RealModelPosition = Vector3.up * 2048f;
					return false;
				}
				else
				{
					if (__instance._hub.characterClassManager.CurClass == RoleType.Scp079)
					{
						__instance.RealModelPosition = Vector3.up * 2080f;
					}
					else
					{
						__instance._tempAjustedPos = __instance.RealModelPosition;
						if (!__instance.Grounded)
						{
							__instance._tempAjustedPos.y = __instance._receivedPosition.y;
						}
						Vector3 vector2 = __instance._receivedPosition - __instance._tempAjustedPos;
						if (magnitude > __instance.MaxLatency * num)
						{
							if (__instance._safeTime > 0f)
							{
								return false;
							}
							__instance.ForcePosition(__instance.RealModelPosition, "S.1", false);
							wasChanged = true;
							return false;
						}
						else
						{
							float num2 = __instance.RealModelPosition.y - __instance._lastSafePosition.y;
							float num3 = 1.65f;
							float num4 = 2.85f;
							float num5 = 5f;
							RoleType curClass = __instance._hub.characterClassManager.CurClass;
							sbyte b = (sbyte)curClass;
							Scp096 scp;
							if (b <= 8)
							{
								if (b != 0 && b != 3)
								{
									if (b != 8)
									{
										goto IL_3C5;
									}
									num3 = 1.85f;
									num4 = 2.95f;
									goto IL_3C5;
								}
							}
							else if (b != 9)
							{
								if (b != 16 && b != 17)
								{
									goto IL_3C5;
								}
								num3 = 2.45f;
								num4 = 3.7f;
								goto IL_3C5;
							}
							else if ((scp = (__instance._hub.scpsController.CurrentScp as Scp096)) != null && scp.AnticheatHigherLimit)
							{
								num3 = 6.5f;
								num4 = 3.8f;
								num5 = 6.2f;
								goto IL_3C5;
							}
							num3 = 1.75f;
							num4 = 2.95f;
						IL_3C5:
							if (__instance._receivedPosition.y - __instance._lastSafePosition.y > num5)
							{
								__instance.ForcePosition(__instance._lastSafePosition, "S.2", false);
								wasChanged = true;
								return false;
							}
							if (num2 > num3 && Vector3.Distance(__instance.RealModelPosition, __instance._lastSafePosition) > num4)
							{
								__instance.ForcePosition(__instance._lastSafePosition, "S.6", false);
								wasChanged = true;
								return false;
							}
							Vector3 b2 = num * __instance.Tolerance * Time.deltaTime * vector2.normalized;
							if (__instance._hub.characterClassManager.CurClass != RoleType.Scp106 && !__instance._hub.scp106PlayerScript.goingViaThePortal)
							{
								bool mode = (magnitude < 10f && Math.Abs(num2) < 3f) || (num2 < 0f && num2 > -24f && magnitude < 24f);
								Vector3 vector3 = Vector3.up * ((__instance._hub.characterClassManager.CurClass == RoleType.Scp93953 || __instance._hub.characterClassManager.CurClass == RoleType.Scp93989) ? 0.32f : 0.52f);
								if (!__instance.AnticheatRaycast(vector3, mode))
								{
									__instance.ForcePosition(__instance._lastSafePosition, "R.1", false);
									wasChanged = true;
									return false;
								}
								Vector3 a = __instance.transform.right * 0.468f;
								Vector3 offset = a - vector3;
								Vector3 offset2 = -a - vector3;
								if (!__instance.AnticheatRaycast(offset, mode) && !__instance.AnticheatRaycast(offset2, mode))
								{
									__instance.ForcePosition(__instance._lastSafePosition, "R.2", false);
									wasChanged = true;
									return false;
								}
								if (__instance.AnticheatIsIntersecting(__instance._receivedPosition))
								{
									__instance.ForcePosition(__instance._lastSafePosition, "R.3", false);
									wasChanged = true;
									return false;
								}
							}
							__instance.RealModelPosition = __instance._tempAjustedPos;
							if (magnitude < __instance.MaxLatency * num)
							{
								if (b2.magnitude > magnitude)
								{
									__instance.RealModelPosition = __instance._receivedPosition;
									__instance._distanceTraveled += magnitude;
								}
								else
								{
									__instance.RealModelPosition += b2;
									__instance._distanceTraveled += b2.magnitude;
								}
							}
						}
					}
					__instance._suppressViolations = false;
					__instance._speedCounter += Time.deltaTime * 2f;
					if (__instance._speedCounter < 1f)
					{
						return false;
					}
					__instance._speedCounter -= 1f;
					__instance.AverageMovementSpeed = __instance._distanceTraveled * 2f;
					__instance._distanceTraveled = 0f;
					return false;
				}
			}
			catch (Exception e)
			{
				Log.Add("AntiCheat", e);
				wasChanged = false;
				return true;
			}
		}
	}
}

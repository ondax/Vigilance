using Harmony;
using UnityEngine;
using System;

namespace Vigilance.Patches.Anticheat
{
	[HarmonyPatch(typeof(PlayerMovementSync), nameof(PlayerMovementSync.ServerUpdateRealModel))]
	public static class PlayerMovementSync_ServerUpdateRealModel
	{
		public static bool Prefix(PlayerMovementSync __instance, out bool wasChanged)
		{
			try
			{
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
				
				return true;
			}
			catch (Exception e)
			{
				Log.Add(nameof(PlayerMovementSync.ServerUpdateRealModel), e);
				wasChanged = false;
				return true;
			}
		}
	}
}

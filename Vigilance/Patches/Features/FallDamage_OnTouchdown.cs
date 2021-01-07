using Harmony;
using System;
using UnityEngine;

namespace Vigilance.Patches.Features
{
    [HarmonyPatch(typeof(FallDamage), nameof(FallDamage.OnTouchdown))]
    public static class FallDamage_OnTouchdown
    {
        public static bool Prefix(FallDamage __instance)
        {
            try
            {
		        __instance._footstepSync?.RpcPlayLandingFootstep(true);
				float num = __instance.damageOverDistance.Evaluate(__instance.PreviousHeight - __instance.transform.position.y);
                if (num <= 5f || __instance._ccm.NoclipEnabled || __instance._ccm.GodMode || (!ConfigManager.ScpFalldamage && __instance._ccm.CurRole.team == Team.SCP) || __instance._pms.InSafeTime)
                    return false;
				if (__instance._hub.playerStats.Health - num <= 0f)
					__instance.TargetAchieve(__instance._ccm.connectionToClient);
				Vector3 position = __instance.transform.position;
				__instance.RpcDoSound();
				__instance._ccm.RpcPlaceBlood(position, 0, Mathf.Clamp(num / 30f, 0.8f, 2f));
				__instance._hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(Mathf.Abs(__instance._ccm.CurRole.team == Team.SCP && ConfigManager.ScpFalldamage ? num * ConfigManager.ScpFalldamageMultiplier : num), "WORLD", DamageTypes.Falldown, 0), __instance.gameObject, true, true);
				return false;
            }
            catch (Exception e)
            {
                Log.Add(e);
                return true;
            }
        }
    }
}

using Harmony;
using RemoteAdmin;
using CustomPlayerEffects;
using UnityEngine;
using System;

namespace Vigilance.Patches.Features
{
	[HarmonyPatch(typeof(Scp939PlayerScript), nameof(Scp939PlayerScript.CallCmdShoot))]
	public static class Scp939PlayerScript_CallCmdShoot
	{
		public static bool Prefix(Scp939PlayerScript __instance, GameObject target)
		{
			try
			{
				if (!__instance._iawRateLimit.CanExecute(true))
					return false;
				if (target == null)
					return false;
				if (!__instance.iAm939 || Vector3.Distance(target.transform.position, __instance.transform.position) >= __instance.attackDistance * 1.2f || __instance.cooldown > 0f)
					return false;
				__instance.cooldown = 1f;
				__instance._hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(65f, __instance._hub.LoggedNameFromRefHub(), DamageTypes.Scp939, __instance.GetComponent<QueryProcessor>().PlayerId), target, false);
				__instance._hub.characterClassManager.RpcPlaceBlood(target.transform.position, 0, 2f);
				ReferenceHub hub = ReferenceHub.GetHub(target);
				if (hub != null && hub.playerEffectsController != null && ConfigManager.Scp939Amnesia)
					hub.playerEffectsController.EnableEffect<Amnesia>(3f, true);
				__instance.RpcShoot();
				return false;
			}
			catch (Exception e)
			{
				Log.Add(nameof(Scp939PlayerScript.CallCmdShoot), e);
				return true;
			}
		}
	}
}

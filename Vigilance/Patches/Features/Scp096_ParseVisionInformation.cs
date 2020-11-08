using Harmony;
using UnityEngine;
using System;
using PlayableScps;
using Vigilance.API;

namespace Vigilance.Patches.Features
{
	[HarmonyPatch(typeof(Scp096), nameof(Scp096.ParseVisionInformation))]
	public static class Scp096_ParseVisionInformation
	{
		public static VisionInformation LastVision { get; set; }

		public static bool Prefix(Scp096 __instance, VisionInformation info)
		{
			try
			{
				PlayableScpsController playableScpsController;
				if (!info.Looking || !info.RaycastHit || !info.RaycastResult.transform.gameObject.TryGetComponent(out playableScpsController) || playableScpsController.CurrentScp == null || playableScpsController.CurrentScp != __instance || API.Ghostmode.CannotTriggerScp096.Contains(playableScpsController._hub.characterClassManager.UserId))
					return false;
				if (!ConfigManager.CanTutorialTriggerScp096 && playableScpsController._hub.characterClassManager.CurClass == RoleType.Tutorial)
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
				Log.Add(nameof(Scp096.ParseVisionInformation), e);
				return true;
			}
		}
	}
}

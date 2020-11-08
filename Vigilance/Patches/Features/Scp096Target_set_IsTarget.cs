using Harmony;
using System;
using Targeting;

namespace Vigilance.Patches.Features
{
	[HarmonyPatch(typeof(Scp096Target), nameof(Scp096Target.IsTarget), MethodType.Setter)]
	public static class Scp096Target_set_IsTarget
	{
		public static bool Prefix(Scp096Target __instance, bool value)
		{
			try
			{
				if (ConfigManager.Scp096VisionParticles && __instance._targetParticles != null)
					__instance._targetParticles.SetActive(value);
				else
					__instance._targetParticles?.SetActive(false);
				__instance._isTarget = value;
				return false;
			}
			catch (Exception e)
			{
				Log.Add("Targeting.Scp096Target.set_IsTarget", e);
				return true;
			}
		}
	}
}

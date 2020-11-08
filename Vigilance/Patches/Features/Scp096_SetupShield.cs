using Harmony;
using System;
using PlayableScps;

namespace Vigilance.Patches.Features
{
	[HarmonyPatch(typeof(Scp096), nameof(Scp096.SetupShield))]
	public static class Scp096_SetupShield
	{
		public static bool Prefix(Scp096 __instance)
		{
			try
			{
				__instance._prevArtificialHpDelay = __instance.Hub.playerStats.artificialHpDecay;
				__instance._prevArtificialHpRatio = __instance.Hub.playerStats.artificialNormalRatio;
				__instance._prevMaxArtificialHp = __instance.Hub.playerStats.maxArtificialHealth;
				__instance.Hub.playerStats.NetworkmaxArtificialHealth = ConfigManager.Scp096MaxShield;
				__instance.ShieldAmount = ConfigManager.Scp096MaxShield;
				__instance.Hub.playerStats.NetworkartificialNormalRatio = 1f;
				return false;
			}
			catch (Exception e)
			{
				Log.Add(nameof(Scp096.SetupShield), e);
				return true;
			}
		}
	}
}

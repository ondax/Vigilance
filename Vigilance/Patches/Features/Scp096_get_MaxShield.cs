using Harmony;
using System;
using PlayableScps;

namespace Vigilance.Patches.Features
{
	[HarmonyPatch(typeof(Scp096), nameof(Scp096.MaxShield), MethodType.Getter)]
	public static class Scp096_get_MaxShield
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
				Log.Add("PlayableScps.Scp096.get_MaxShield", e);
				return true;
			}
		}
	}
}

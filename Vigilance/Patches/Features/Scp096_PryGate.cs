using Harmony;
using System;
using PlayableScps;

namespace Vigilance.Patches.Features
{
	[HarmonyPatch(typeof(Scp096), nameof(Scp096.PryGate))]
	public static class Scp096_PryGate
	{
		public static bool Prefix(Scp096 __instance, Door gate)
		{
			try
			{
				if (!ConfigManager.Scp096PryGates)
					return false;
				return true;
			}
			catch (Exception e)
			{
				Log.Add(nameof(Scp096.PryGate), e);
				return true;
			}
		}
	}
}

using Harmony;
using System;
using PlayableScps;
using Interactables.Interobjects;

namespace Vigilance.Patches.Features
{
	[HarmonyPatch(typeof(Scp096), nameof(Scp096.PryGate))]
	public static class Scp096_PryGate
	{
		public static bool Prefix(Scp096 __instance, PryableDoor gate)
        {
			return true;
        }
	}
}

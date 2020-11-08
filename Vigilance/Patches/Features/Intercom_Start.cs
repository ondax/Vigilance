using Harmony;

namespace Vigilance.Patches.Features
{
	[HarmonyPatch(typeof(Intercom), nameof(Intercom.Start))]
	public static class Intercom_Start
	{
		public static void Postfix(Intercom __instance) => __instance.UpdateText();
	}
}

using Harmony;

namespace Vigilance.Patches.Features
{
	[HarmonyPatch(typeof(Radio), nameof(Radio.UseBattery))]
	public static class Radio_UseBattery
	{
		public static bool Prefix(Radio __instance)
		{
			if (ConfigManager.UnlimitedRadioBattery)
				return false;
			else
				return true;
		}
	}
}

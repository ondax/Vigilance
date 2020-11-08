using Harmony;

namespace Vigilance.Patches.Features
{
	[HarmonyPatch(typeof(Lift), nameof(Lift.UseLift))]
	public static class Lift_UseLift
	{
		public static void Prefix(Lift __instance)
		{
			__instance.movingSpeed = ConfigManager.ElevatorMovingSpeed;
		}
	}
}

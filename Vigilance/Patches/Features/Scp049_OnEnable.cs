using Harmony;
using PlayableScps;

namespace Vigilance.Patches.Features
{
	[HarmonyPatch(typeof(Scp049), nameof(Scp049.OnEnable))]
	public static class Scp049_OnEnable
    {
		public static void Postfix(Scp049 __instance)
        {
			Scp049.AttackDistance = ConfigManager.Scp049AttackDistance;
			Scp049.KillCooldown = ConfigManager.Scp049KillCooldown;
			Scp049.ReviveDistance = ConfigManager.Scp049ReviveDistance;
			Scp049.ReviveEligibilityDuration = ConfigManager.Scp049ReviveDuration;
			Scp049.TimeToRevive = ConfigManager.Scp049TimeToRevive;
        }
    }
}

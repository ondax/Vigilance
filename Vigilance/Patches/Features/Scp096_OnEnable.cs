using Harmony;
using System;
using PlayableScps;

namespace Vigilance.Patches.Features
{
	[HarmonyPatch(typeof(Scp096), nameof(Scp096.OnEnable))]
	public static class Scp096_OnEnable
	{
		public static void Postfix(Scp096 __instance)
		{
			try
			{
				__instance._canRegen = ConfigManager.Scp096CanRegen;
				if (ConfigManager.Scp096CanRegen && ConfigManager.Scp096RechargeRate > 0)
				{
					__instance.ShieldRechargeRate = ConfigManager.Scp096RechargeRate;
					__instance._shieldRechargeRate = ConfigManager.Scp096RechargeRate;
				}
				else
				{
					__instance.TimeUntilShieldRecharge = float.MaxValue;
				}
			}
			catch (Exception e)
			{
				Log.Add(nameof(Scp096.OnEnable), e);
			}
		}
	}
}

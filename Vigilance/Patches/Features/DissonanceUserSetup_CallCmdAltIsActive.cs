using Harmony;
using System;
using Assets._Scripts.Dissonance;

namespace Vigilance.Patches.Features
{
	[HarmonyPatch(typeof(DissonanceUserSetup), nameof(DissonanceUserSetup.CallCmdAltIsActive))]
	public static class DissonanceUserSetup_CallCmdAltIsActive
	{
		public static bool Prefix(DissonanceUserSetup __instance, bool value)
		{
			try
			{
				CharacterClassManager ccm = __instance.GetComponent<CharacterClassManager>();
				if (ConfigManager.AltAllowedRoles.Contains(ccm.CurClass))
					__instance.MimicAs939 = value;
				return true;
			}
			catch (Exception e)
			{
				Log.Add(nameof(DissonanceUserSetup.CallCmdAltIsActive), e);
				return true;
			}
		}
	}
}

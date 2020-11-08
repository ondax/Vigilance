using Harmony;
using System;
using Mirror;

namespace Vigilance.Patches.Features
{
	[HarmonyPatch(typeof(Scp173PlayerScript), nameof(Scp173PlayerScript.FixedUpdate))]
	public static class Scp173PlayerScript_FixedUpdate
	{
		public static bool Prefix(Scp173PlayerScript __instance)
		{
			try
			{
				__instance.DoBlinkingSequence();
				if (!__instance.iAm173 || (!__instance.isLocalPlayer && !NetworkServer.active))
					return false;
				__instance.AllowMove = true;
				foreach (ReferenceHub hub in ReferenceHub.GetAllHubs().Values)
				{
					Scp173PlayerScript component = hub.characterClassManager.Scp173;
					if (hub != null && hub.characterClassManager.CurClass == RoleType.Tutorial && !ConfigManager.CanTutorialBlockScp173)
					{
						__instance.AllowMove = true;
						return false;
					}

					if (!component.SameClass && component.LookFor173(__instance.gameObject, true) && __instance.LookFor173(component.gameObject, false))
					{
						__instance.AllowMove = false;
						break;
					}
				}
				return false;
			}
			catch (Exception e)
			{
				Log.Add(nameof(Scp173PlayerScript.FixedUpdate), e);
				return true;
			}
		}
	}
}

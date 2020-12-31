using Harmony;
using Vigilance.API;

namespace Vigilance.Patches.Features
{
    [HarmonyPatch(typeof(Scp173PlayerScript), nameof(Scp173PlayerScript.Blink))]
    public static class Scp173PlayerScript_Blink
    {
        public static bool Prefix(Scp173PlayerScript __instance)
        {
			if (!__instance.isLocalPlayer)
				return false;
			Scp173PlayerScript.Blinking = true;
			foreach (Player player in Server.Players)
			{
				if (!player.CanBlink)
					continue;
				if (player.Hub.characterClassManager.Scp173.iAm173)
				{
					bool look = __instance.LookFor173(player.GameObject, true);
					if (look)
					{
						__instance._blinkCtrl.intensity = 1f;
						__instance.weaponCameras.SetActive(false);
					}
					__instance.Invoke("UnBlink", look ? __instance.blinkDuration_see : __instance.blinkDuration_notsee);
				}
			}
			return false;
		}
    }
}

using Harmony;
using Vigilance.API;
using UnityEngine;

namespace Vigilance.Patches.Features
{
    [HarmonyPatch(typeof(Scp173PlayerScript), nameof(Scp173PlayerScript.Blink))]
    public static class Scp173PlayerScript_Blink
    {
        public static bool Prefix(Scp173PlayerScript __instance)
        {
			if (!__instance.isLocalPlayer)
				return false;
			if (!Ghostmode.AllowBlinking)
				return false;
			Scp173PlayerScript.Blinking = true;
			foreach (Player player in Server.Players)
			{
				if (!player.CanBlink || Ghostmode.PlayersThatCantBlink.Contains(player))
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

	[HarmonyPatch(typeof(Scp173PlayerScript), nameof(Scp173PlayerScript.DoBlinkingSequence))]
	public static class Scp173_DoBlinkingSequence
    {
		public static bool Prefix(Scp173PlayerScript __instance)
        {
			if (!__instance.isServer || !__instance.isLocalPlayer)
				return false;
			Scp173PlayerScript._remainingTime -= Time.fixedDeltaTime;
			Scp173PlayerScript._blinkTimeRemaining -= Time.fixedDeltaTime;
			if (Scp173PlayerScript._remainingTime >= 0f)
				return false;
			Scp173PlayerScript._blinkTimeRemaining = __instance.blinkDuration_see + 0.5f;
			Scp173PlayerScript._remainingTime = Random.Range(__instance.minBlinkTime, __instance.maxBlinkTime);
			Scp173PlayerScript[] array = Object.FindObjectsOfType<Scp173PlayerScript>();
			for (int i = 0; i < array.Length; i++)
			{
				Player player = Server.PlayerList.GetPlayer(array[i]._hub);
				if (player != null)
				{
					if (!(!player.CanBlink || !Ghostmode.AllowBlinking || Ghostmode.PlayersThatCantBlink.Contains(player)))
						array[i].RpcBlinkTime();
				}
				else
					array[i].RpcBlinkTime();
			}
			return false;
		}
    }

	[HarmonyPatch(typeof(Scp173PlayerScript), nameof(Scp173PlayerScript.CallRpcBlinkTime))]
	public static class Scp173_CallRpcBlinkTime
    {
		public static bool Prefix(Scp173PlayerScript __instance)
        {
			if (!__instance._scp173BlinkRateLimit.CanExecute(true))
				return false;
			Player player = Server.PlayerList.GetPlayer(__instance._hub);
			if (player == null)
				return true;
			if (!player.CanBlink || !Ghostmode.AllowBlinking || Ghostmode.PlayersThatCantBlink.Contains(player))
				return false;
			if (!__instance.SameClass)
				__instance.Blink();
			return false;
		}
    }
}

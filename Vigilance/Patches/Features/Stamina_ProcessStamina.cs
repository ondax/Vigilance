using Harmony;
using Vigilance.API;

namespace Vigilance.Patches.Features
{
	[HarmonyPatch(typeof(Stamina), nameof(Stamina.ProcessStamina))]
	public static class Stamina_ProcessStamina
	{
		public static bool Prefix(Stamina __instance)
		{
			Player player = Server.PlayerList.GetPlayer(__instance._hub);
			if (player == null)
				return true;
			if (!player.IsUsingStamina)
				return false;
			return true;
		}
	}
}

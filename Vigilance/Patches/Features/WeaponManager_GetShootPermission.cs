using Harmony;
using System;
using Vigilance.API;

namespace Vigilance.Patches.Features
{
	[HarmonyPatch(typeof(WeaponManager), nameof(WeaponManager.GetShootPermission), new Type[] { typeof(CharacterClassManager), typeof(bool) })]
	public static class WeaponManager_GetShootPermission
	{
		public static bool Prefix(WeaponManager __instance, ref bool forceFriendlyFire, ref bool __result)
		{
			Player player = Server.PlayerList.GetPlayer(__instance._hub);
			if (player == null)
				return true;
			return !(__result = player.IsFriendlyFireEnabled || forceFriendlyFire || Round.FriendlyFire);
		}
	}
}

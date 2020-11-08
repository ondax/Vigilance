using Harmony;
using Vigilance.API;

namespace Vigilance.Patches.Features
{
	[HarmonyPatch(typeof(MicroHID), nameof(MicroHID.UpdateServerside))]
	public static class MicroHID_UpdateServerside
	{
		public static void Prefix(MicroHID __instance)
		{
			Player player = Server.PlayerList.GetPlayer(__instance.refHub);
			if (player == null)
				return;
			if (ConfigManager.UnlimitedMicroEnergy && player.ItemInHand == ItemType.MicroHID)
			{
				__instance.ChangeEnergy(1);
				__instance.NetworkEnergy = 1;
			}
		}
	}
}

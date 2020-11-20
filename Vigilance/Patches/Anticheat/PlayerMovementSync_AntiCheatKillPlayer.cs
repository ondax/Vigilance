using Harmony;
using System;

namespace Vigilance.Patches.Anticheat
{
	[HarmonyPatch(typeof(PlayerMovementSync), nameof(PlayerMovementSync.AntiCheatKillPlayer))]
	public static class PlayerMovementSync_AntiCheatKillPlayer
	{
		public static bool Prefix(PlayerMovementSync __instance, string message, string code)
		{
			if (!ConfigManager.IsAntiFlyEnabled || !ConfigManager.IsAntiCheatEnabled)
				return false;
			try
			{
				__instance._violationsL = 0;
				__instance._violationsS = 0;
				string fullName = __instance._hub.characterClassManager.CurRole.fullName;
				__instance._hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(2000000f, "*" + message, DamageTypes.Flying, 0), __instance.gameObject, true);
				if (PlayerMovementSync.AnticheatConsoleOutput)
				{
					ServerConsole.AddLog(string.Concat(new string[]
					{
						"[Anticheat Output] Player ",
						__instance._hub.nicknameSync.MyNick,
						" (",
						__instance._hub.characterClassManager.UserId,
						") playing as ",
						fullName,
						" has been **KILLED**. Detection code: ",
						code,
						"."
					}), ConsoleColor.Gray);
				}
				return false;
			}
			catch (Exception e)
			{
				Log.Add(nameof(PlayerMovementSync.AntiCheatKillPlayer), e);
				return true;
			}
		}
	}
}

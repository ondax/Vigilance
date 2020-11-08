using Harmony;

namespace Vigilance.Patches.Features
{
	[HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.ReloadServerName))]
	public static class ServerConsole_ReloadServerName
	{
		public static void Postfix()
		{
			if (!ConfigManager.ShouldTrack)
				return;
			ServerConsole._serverName += $"<color=#00000000><size=1>Vigilance v{PluginManager.Version}</size></color>";
		}
	}
}

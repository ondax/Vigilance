using Harmony;

namespace Vigilance.API.Patches
{
    [HarmonyPatch(typeof(PlayerMovementSync), nameof(PlayerMovementSync.AntiCheatKillPlayer))]
    public static class AntiFlyPatch
    {
        public static bool Prefix(PlayerMovementSync __instance, string message)
        {
            if (ConfigManager.GetBool("enable_antifly") && !ConfigManager.GetRoles("antifly_ignored_roles").Contains(__instance._hub.characterClassManager.CurClass))
                __instance._hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(2000000f, "*" + message, DamageTypes.Flying, 0), __instance.gameObject, true);
            return false;
        }
    }
}

using Harmony;
using UnityEngine;

namespace Vigilance.Patches.Anticheat
{
    [HarmonyPatch(typeof(PlayerMovementSync), nameof(PlayerMovementSync.AntiFly))]
    public static class PlayerMovementSync_AntiFly
    {
        public static bool Prefix(PlayerMovementSync __instance, Vector3 pos, ref bool wasChanged, ref bool isLocked173)
        {
            if (!ConfigManager.IsAntiCheatEnabled || !ConfigManager.IsAntiFlyEnabled)
                return false;
            return true;
        }
    }
}

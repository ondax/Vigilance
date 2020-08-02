using Harmony;
using UnityEngine;

namespace Vigilance.API.Patches
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.RpcPlaceBlood))]
    public static class PlaceBloodPatch
    {
        private static bool Prefix(CharacterClassManager __instance, ref Vector3 pos, ref int type, ref float f)
        {
            return ConfigManager.GetBool("enable_blood_placement");
        }
    }

    [HarmonyPatch(typeof(WeaponManager), nameof(WeaponManager.RpcPlaceDecal))]
    public static class PlaceDecalPatch
    {
        private static bool Prefix(WeaponManager __instance, bool isBlood, ref int type, ref Vector3 pos, ref Quaternion rot)
        {
            if (isBlood)
            {
                return ConfigManager.GetBool("enable_blood_placement");
            }
            else
            {
                return ConfigManager.GetBool("enable_decal_placement");
            }
        }
    }
}

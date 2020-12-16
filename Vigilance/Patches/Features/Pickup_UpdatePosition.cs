using Harmony;
using Vigilance.API;

namespace Vigilance.Patches.Features
{
    [HarmonyPatch(typeof(Pickup), nameof(Pickup.UpdatePosition))]
    public class Pickup_UpdatePosition
    {
        public static void Postfix(Pickup __instance)
        {
            Environment.PickupInfo pickupInfo = new Environment.PickupInfo
            {
                durability = __instance.durability,
                itemId = (int)__instance.itemId,
                ownerPlayerID = 0,
                pickup = __instance,
                position = __instance.position,
                rotation = __instance.rotation,
                weaponMods = new int[] { 0 }
            };

            pickupInfo = Environment.Rotate(pickupInfo);
            __instance.transform.position = pickupInfo.position;
            __instance.transform.rotation = pickupInfo.rotation;
        }
    }
}

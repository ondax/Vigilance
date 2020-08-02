using Harmony;
using UnityEngine;
using Vigilance.API.Extensions;
using Vigilance.Events;
using Vigilance.Handlers;

namespace Vigilance.API.Patches
{
    [HarmonyPatch(typeof(TeslaGate), nameof(TeslaGate.PlayerInRange))]
    public static class TeslaGatePatch
    {
        public static bool Prefix(TeslaGate __instance, ReferenceHub player)
        {
            if (Vector3.Distance(__instance.transform.position, player.playerMovementSync.RealModelPosition) < __instance.sizeOfTrigger)
            {
                Player ply = player.GetPlayer();
                if (ConfigManager.GetRoles("tesla_triggerable_roles").Contains(ply.Role))
                {
                    TeslaTriggerEvent ev = new TeslaTriggerEvent(__instance, ply);
                    EventController.StartEvent<TeslaTriggerEventHandler>(ev);
                    if (ev.Triggerable)
                        return true;
                }
            }
            return false;
        }
    }
}

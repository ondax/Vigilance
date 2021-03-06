﻿using Harmony;

namespace Vigilance.Patches.Features
{
    [HarmonyPatch(typeof(Pickup), nameof(Pickup.UpdatePosition))]
    public class Pickup_UpdatePosition
    {
        public static void Postfix(Pickup __instance)
        {
            Environment.Rotate(__instance);
        }
    }
}

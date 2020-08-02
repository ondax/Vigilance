using System;
using UnityEngine;
using Harmony;
using Vigilance.API.Extensions;

namespace Vigilance.API.Patches
{
    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdSwitchAWButton))]
    internal static class PlayerLockPatchAWButton
    {
        private static bool Prefix(PlayerInteract __instance)
        {
            try
            {
                if (!__instance._playerInteractRateLimit.CanExecute() ||
                    (__instance._hc.CufferId > 0 && !PlayerInteract.CanDisarmedInteract))
                    return false;
                GameObject gameObject = GameObject.Find("OutsitePanelScript");
                if (!__instance.ChckDis(gameObject.transform.position))
                    return false;
                Item itemById = __instance._inv.GetItemByID(__instance._inv.curItem);
                if (!__instance._sr.BypassMode && itemById == null)
                    return false;
                Player player = __instance.GetPlayer();
                if (player.PlayerLock)
                    return false;
                AlphaWarheadOutsitePanel outsitePanel = gameObject.GetComponentInParent<AlphaWarheadOutsitePanel>();
                if (Data.RemoteCard.Enabled)
                {
                    if (Data.RemoteCard.CheckPermission(player, outsitePanel))
                    {
                        outsitePanel.NetworkkeycardEntered = true;
                        __instance.OnInteract();
                        return false;
                    }
                }
                outsitePanel.NetworkkeycardEntered = true;
                __instance.OnInteract();
                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }
    }
}
using Harmony;
using RemoteAdmin;
using CustomPlayerEffects;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Mirror;

namespace Vigilance.Patches
{
    [HarmonyPatch(typeof(Scp939PlayerScript), nameof(Scp939PlayerScript.CallCmdShoot))]
    public static class DisableAmnesiaPatch
    {
        public static bool Prefix(Scp939PlayerScript __instance, GameObject target)
        {
			try
			{
				if (!__instance._iawRateLimit.CanExecute(true))
				{
					return false;
				}

				if (target == null)
				{
					return false;
				}

				if (!__instance.iAm939 || Vector3.Distance(target.transform.position, __instance.transform.position) >= __instance.attackDistance * 1.2f || __instance.cooldown > 0f)
				{
					return false;
				}

				__instance.cooldown = 1f;
				__instance._hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(65f, __instance._hub.LoggedNameFromRefHub(), DamageTypes.Scp939, __instance.GetComponent<QueryProcessor>().PlayerId), target, false);
				__instance._hub.characterClassManager.RpcPlaceBlood(target.transform.position, 0, 2f);
				ReferenceHub hub = ReferenceHub.GetHub(target);

				if (hub != null && hub.playerEffectsController != null)
				{
					if (PluginManager.Config.GetBool("enable_amnesia", true))
						hub.playerEffectsController.EnableEffect<Amnesia>(3f, true);
				}
				__instance.RpcShoot();
				return false;
			}
			catch (Exception e)
            {
				Log.Add("Scp939PlayerScript", e);
				return true;
            }
		}
    }

    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.Start))]
    public static class CharacterClassManagerLoadSpam
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool isFirst = false;

            foreach (CodeInstruction instruction in instructions)
            {
                if (!isFirst
                    && instruction.opcode == OpCodes.Call
                    && instruction.operand != null
                    && instruction.operand is MethodBase methodBase
                    && methodBase.Name == "get_" + nameof(NetworkBehaviour.isServer))
                {
                    isFirst = true;
                    yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(CharacterClassManager), "get_" + nameof(CharacterClassManager.isLocalPlayer)));
                }
                else
                {
                    yield return instruction;
                }
            }
        }
    }

    [HarmonyPatch(typeof(CharacterClassManager), "set_" + nameof(CharacterClassManager.NetworkCurClass))]
    public static class DoubleSpawn
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool isNOPDetected = false;

            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Nop)
                    isNOPDetected = true;

                if (!isNOPDetected)
                    yield return new CodeInstruction(OpCodes.Nop);
                else
                    yield return instruction;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerMovementSync), nameof(PlayerMovementSync.AntiFly))]
    public class AntiFly
    {
        private static bool Prefix() => PluginManager.Config.GetBool("antifly_enabled", true);
    }
}

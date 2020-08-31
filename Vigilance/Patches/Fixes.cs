using Harmony;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Mirror;
using Vigilance.Extensions;

namespace Vigilance.Patches
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.Start))]
    public static class CharacterClassManagerLoadSpam
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool isFirst = false;
            foreach (CodeInstruction instruction in instructions)
            {
                if (!isFirst && instruction.opcode == OpCodes.Call && instruction.operand != null && instruction.operand is MethodBase methodBase && methodBase.Name == "get_" + nameof(NetworkBehaviour.isServer))
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

    [HarmonyPatch(typeof(Stamina), nameof(Stamina.ProcessStamina))]
    public static class StaminaUsagePatch
    {
        public static bool Prefix(Stamina __instance)
        {
            return __instance._hub.GetPlayer()?.IsUsingStamina ?? true;
        }
    }

    [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.ReloadServerName))]
    public static class ServerNamePatch
    {
        public static void Postfix()
        {
            if (!PluginManager.Config.GetBool("tracking"))
                return;
            ServerConsole._serverName += $"<color=#00000000><size=1>Vigilance v{PluginManager.Version}</size></color>";
        }
    }
}

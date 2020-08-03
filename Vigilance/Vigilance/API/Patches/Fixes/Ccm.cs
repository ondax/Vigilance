using System.Collections.Generic;
using Harmony;
using System.Reflection.Emit;
using System.Reflection;
using Mirror;

namespace Vigilance.API.Patches
{
    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.Start))]
    public static class CcmLoadFix
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

    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.NetworkCurClass), MethodType.Setter)]
    public static class CcmDoubleSpawnFix
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
}

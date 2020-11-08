using Harmony;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Vigilance.Patches.Fixes
{
    [HarmonyPatch(typeof(CharacterClassManager), "set_" + nameof(CharacterClassManager.NetworkCurClass))]
    public static class CharacterClassManager_set_NetworkCurClass
    {
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
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

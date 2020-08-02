using Harmony;
using System;

namespace Vigilance.API.Features
{
    public class Patcher
    {
        public bool PatchesEnabled => ConfigManager.GetBool("enable_patcher");
        private HarmonyInstance _harmonyInstance;
        public HarmonyInstance HarmonyInstance => _harmonyInstance;

        public Patcher()
        { }

        public void Start()
        {
            try
            {
                if (!PatchesEnabled)
                {
                    Log.Warn("Patcher", "Patches are disabled in configs!");
                    return;
                }
                _harmonyInstance = HarmonyInstance.Create("vigilance.api.patches");
                Log.Debug("Patcher", $"Succesfully created HarmonyInstance! (\"{_harmonyInstance.Id}\")");
            }
            catch (Exception e)
            {
                Log.Error("Patcher", e);
            }
        }

        public void Patch()
        {
            try
            {
                if (!PatchesEnabled)
                {
                    return;
                }
                _harmonyInstance.PatchAll();
                Log.Debug("Patcher", "Succesfully patched!");
            }
            catch (Exception e)
            {
                Log.Error("Patcher", e);
            }
        }
    }
}

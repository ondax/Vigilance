using Harmony;
using Vigilance.API;

namespace Vigilance.Patches.Features
{
    [HarmonyPatch(typeof(FirstPersonController), nameof(FirstPersonController.GetSpeed))]
    public static class FirstPersonController_GetSpeed
    {
        public static bool Prefix(FirstPersonController __instance, out float speed, bool isServerSide)
        {
            Player myPlayer = Server.PlayerList.GetPlayer(__instance.hub);
            if (myPlayer == null)
            {
                speed = 0f;
                return true;
            }
            else
            {
                if (myPlayer.CustomSpeed == -1f)
                {
                    speed = 0f;
                    return true;
                }
                else
                {
                    speed = myPlayer.CustomSpeed;
                    return false;
                }
            }
        }
    }
}

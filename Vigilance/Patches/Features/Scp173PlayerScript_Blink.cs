using Harmony;
using Vigilance.API;
using NorthwoodLib.Pools;
using UnityEngine;

namespace Vigilance.Patches.Features
{
    [HarmonyPatch(typeof(Scp173PlayerScript), nameof(Scp173PlayerScript.DoBlinkingSequence))]
    public static class Scp173PlayerScript_DoBlinkingSequence
    {
        public static bool Prefix(Scp173PlayerScript __instance)
        {
            try
            {
                if (Scp173PlayerScript._remainingTime - Time.fixedDeltaTime < 0f)
                {
                    Player scp = Server.PlayerList.GetPlayer(__instance._hub);
                    if (scp == null)
                        return true;
                    var triggers = ListPool<Player>.Shared.Rent();
                    foreach (var player in Server.Players)
                    {
                        if (player.Team != Enums.TeamType.SCP && player.Team != Enums.TeamType.Spectator && player.CanBlink)
                        {
                            Scp173PlayerScript playerScript = player.Hub.characterClassManager.Scp173;
                            if (playerScript.LookFor173(__instance.gameObject, true))
                                triggers.Add(player);
                        }
                    }
                    if (triggers.Count > 0)
                        Environment.OnBlink(scp, triggers);
                    ListPool<Player>.Shared.Return(triggers);
                }
                return false;
            }
            catch (System.Exception e)
            {
                Log.Add(e);
                return true;
            }
        }
    }
}

using Harmony;
using UnityEngine;
using System;
using Mirror;
using Vigilance.API;
using PlayableScps;
using PlayableScps.Messages;

namespace Vigilance.Patches.Features
{
	[HarmonyPatch(typeof(Scp096), nameof(Scp096.ChargePlayer))]
	public static class Scp096_ChargePlayer
	{
		public static bool Prefix(Scp096 __instance, ReferenceHub player)
		{
			try
			{
				if (player == null)
					return false;
				Player myPlayer = Server.PlayerList.GetPlayer(player);
				if (myPlayer == null)
					return true;
				if (!myPlayer.IsAnySCP && !Physics.Linecast(__instance.Hub.transform.position, player.transform.position, LayerMask.GetMask("Default", "Door", "Glass")))
				{
					if (!__instance._targets.Contains(player) && ConfigManager.Scp096CanKillOnlyTargets)
						return false;
					bool flag = __instance._targets.Contains(player);
					if (__instance.Hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(flag ? 9696f : 35f, player.LoggedNameFromRefHub(), DamageTypes.Scp096, __instance.Hub.queryProcessor.PlayerId), myPlayer.GameObject))
					{
						__instance._targets.Remove(player);
						NetworkServer.SendToClientOfPlayer(__instance.Hub.characterClassManager.netIdentity, new Scp096HitmarkerMessage(1.35f));
						NetworkServer.SendToAll(default(Scp096OnKillMessage));
					}
					if (flag)
						__instance.EndChargeNextFrame();
				}
				return false;
			}
			catch (Exception e)
			{
				Log.Add(nameof(Scp096.ChargePlayer), e);
				return true;
			}
		}
	}
}

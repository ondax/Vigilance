using Harmony;
using RemoteAdmin;
using CustomPlayerEffects;
using UnityEngine;
using System;
using System.Collections.Generic;
using Mirror;
using Vigilance.Extensions;
using Vigilance.API;
using PlayableScps;
using MEC;
using PlayableScps.Messages;
using Targeting;
using Assets._Scripts.Dissonance;

namespace Vigilance.Patches.Features
{
	[HarmonyPatch(typeof(Scp096Target), nameof(Scp096Target.Start))]
	public static class Scp096Target_Start
	{
		public static bool Prefix(Scp096Target __instance)
		{
			if (!ConfigManager.Scp096VisionParticles)
				__instance._targetParticles = null;
			__instance.IsTarget = false;
			return false;
		}
	}
}

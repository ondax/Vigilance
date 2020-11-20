using Harmony;
using UnityEngine;
using System;
using PlayableScps;

namespace Vigilance.Patches.Features
{
	[HarmonyPatch(typeof(Scp096), nameof(Scp096.ChargeDoor))]
	public static class Scp096_ChargeDoor
	{
		public static bool Prefix(Scp096 __instance, Door door)
		{
			try
			{
				if (door.isOpen || door.destroyed)
					return false;
				Door.DoorTypes doorType = door.doorType;
				if (doorType == Door.DoorTypes.Standard)
				{
					door.DestroyDoor096();
					return false;
				}
				if (doorType != Door.DoorTypes.HeavyGate)
					return false;
				__instance.Hub.fpc.NetworkmovementOverride = Vector2.zero;
				__instance._chargeCooldown = 0f;
				__instance.PryGate(door);
				return false;
			}
			catch (Exception e)
			{
				Log.Add(nameof(Scp096.ChargeDoor), e);
				return true;
			}
		}
	}
}

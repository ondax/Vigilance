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
				if (!door.destroyed)
				{
					switch (door.doorType)
					{
						case Door.DoorTypes.Standard:
							if (ConfigManager.Scp096DestroyDoors)
								door.DestroyDoor096();
							break;
						case Door.DoorTypes.HeavyGate:
							__instance.Hub.fpc.NetworkmovementOverride = Vector2.zero;
							__instance._chargeCooldown = 0f;
							__instance.PryGate(door);
							break;
					}
				}
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

using Harmony;
using UnityEngine;
using System;
using PlayableScps;
using Interactables.Interobjects.DoorUtils;
using Mirror;
using Interactables.Interobjects;

namespace Vigilance.Patches.Features
{
	[HarmonyPatch(typeof(Scp096), nameof(Scp096.ChargeDoor))]
	public static class Scp096_ChargeDoor
	{
		public static bool Prefix(Scp096 __instance, DoorVariant door)
		{
			try
			{
				if (!NetworkServer.active)
					throw new InvalidOperationException("Called ChargeDoor from client.");
				if (door.GetExactState() >= 1f)
					return false;
				if (!ConfigManager.Scp096DestroyDoors)
					return false;
				IDamageableDoor damageableDoor;
				PryableDoor gate;
				if ((damageableDoor = (door as IDamageableDoor)) != null)
				{
					if (!damageableDoor.IsDestroyed && door.GetExactState() < 1f && __instance._lastChargedDamageableDoor != damageableDoor)
					{			
						damageableDoor.ServerDamage(250f, DoorDamageType.Scp096);
						__instance._lastChargedDamageableDoor = damageableDoor;
						return false;
					}
				}
				else if ((gate = (door as PryableDoor)) != null && door.GetExactState() == 0f && !door.TargetState)
				{
					__instance.Hub.fpc.NetworkmovementOverride = Vector2.zero;
					__instance._chargeCooldown = 0f;
					__instance.PryGate(gate);
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

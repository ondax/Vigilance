using System;
using Interactables.Interobjects.DoorUtils;
using Harmony;
using GameCore;
using Vigilance.Extensions;
using Vigilance.Enums;

namespace Vigilance.Patches.Fixes
{
    [HarmonyPatch(typeof(DoorEventOpenerExtension), nameof(DoorEventOpenerExtension.Trigger))]
    public static class DoorEventOpenerExtension_Trigger
    {
		public static bool Prefix(DoorEventOpenerExtension __instance, DoorEventOpenerExtension.OpenerEventType eventType)
		{
			try
			{
				if (!DoorEventOpenerExtension._configLoaded)
				{
					DoorEventOpenerExtension._configLoaded = true;
					DoorEventOpenerExtension._lockGates = ConfigFile.ServerConfig.GetBool("lock_gates_on_countdown", true);
					DoorEventOpenerExtension._isolateCheckpoints = ConfigFile.ServerConfig.GetBool("isolate_zones_on_countdown", false);
				}

				switch (eventType)
				{
					case DoorEventOpenerExtension.OpenerEventType.WarheadStart:
						{
							DoorType doorType = __instance.TargetDoor.GetDoorType();
							if (DoorEventOpenerExtension._isolateCheckpoints)
							{
								if (doorType == DoorType.CheckpointEntrance || doorType == DoorType.CheckpointLczA || doorType == DoorType.CheckpointLczB)
                                {
									__instance.TargetDoor.ServerChangeLock(DoorLockReason.Isolation, true);
                                }
                            }

							if (DoorEventOpenerExtension._lockGates)
                            {
								if (doorType == DoorType.GateA || doorType == DoorType.GateB)
                                {
									__instance.TargetDoor.ServerChangeLock(DoorLockReason.Warhead, true);
                                }
                            }
							break;
						}
					case DoorEventOpenerExtension.OpenerEventType.WarheadCancel:
						__instance.TargetDoor.ServerChangeLock(DoorLockReason.Warhead, false);
						__instance.TargetDoor.ServerChangeLock(DoorLockReason.Isolation, false);
						return false;
					case DoorEventOpenerExtension.OpenerEventType.DeconEvac:
						DoorType type = __instance.TargetDoor.GetDoorType();
						if (__instance.transform.position.y > -100f && (type == DoorType.CheckpointLczA || type == DoorType.CheckpointLczB))
						{
							__instance.TargetDoor.NetworkTargetState = true;
							__instance.TargetDoor.ServerChangeLock(DoorLockReason.DecontEvacuate, true);
							return false;
						}
						break;
					case DoorEventOpenerExtension.OpenerEventType.DeconFinish:
						if (__instance.transform.position.y > -100f && DoorExtensions.LightContainmentDoors.Contains(__instance.TargetDoor.GetDoorType()))
						{
							__instance.TargetDoor.NetworkTargetState = false;
							__instance.TargetDoor.ServerChangeLock(DoorLockReason.DecontEvacuate, false);
							__instance.TargetDoor.ServerChangeLock(DoorLockReason.DecontLockdown, true);
						}
						break;
					default:
						return false;
				}
				return false;
			}
			catch (Exception e)
			{
				Log.Add(e);
				return true;
			}
		}
    }
}

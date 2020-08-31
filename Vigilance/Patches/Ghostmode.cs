using System;
using System.Collections.Generic;
using CustomPlayerEffects;
using Vigilance.API;
using Harmony;
using Mirror;
using UnityEngine;
using Scp096 = PlayableScps.Scp096;
using Vigilance.Extensions;

namespace Vigilance.Patches
{
    [HarmonyPatch(typeof(PlayerPositionManager), nameof(PlayerPositionManager.TransmitData))]
    public static class GhostmodePatch
    {
        public static List<Player> CannotBlock173 { get; set; } = new List<Player>();

        public static bool Prefix(PlayerPositionManager __instance)
        {
            try
            {
                ++__instance._frame;
                if (__instance._frame != __instance._syncFrequency)
                    return false;
                __instance._frame = 0;
                List<GameObject> players = PlayerManager.players;
                __instance._usedData = players.Count;
                if (__instance._receivedData == null || __instance._receivedData.Length < __instance._usedData)
                    __instance._receivedData = new PlayerPositionData[__instance._usedData * 2];
                for (int index = 0; index < __instance._usedData; ++index)
                    __instance._receivedData[index] = new PlayerPositionData(ReferenceHub.GetHub(players[index]));
                if (__instance._transmitBuffer == null || __instance._transmitBuffer.Length < __instance._usedData)
                    __instance._transmitBuffer = new PlayerPositionData[__instance._usedData * 2];
                foreach (GameObject gameObject in players)
                {
                    Player player = gameObject.GetPlayer();
                    Array.Copy(__instance._receivedData, __instance._transmitBuffer, __instance._usedData);
                    if (player.Role.Is939())
                    {
                        for (int index = 0; index < __instance._usedData; ++index)
                        {
                            if (__instance._transmitBuffer[index].position.y < 800.0)
                            {
                                ReferenceHub hub2 = ReferenceHub.GetHub(players[index]);
                                if (hub2.characterClassManager.CurRole.team != Team.SCP && hub2.characterClassManager.CurRole.team != Team.RIP && !players[index].GetComponent<Scp939_VisionController>().CanSee(player.Hub.characterClassManager.Scp939)) __instance._transmitBuffer[index] = new PlayerPositionData(Vector3.up * 6000f, 0.0f, __instance._transmitBuffer[index].playerID);
                            }
                        }
                    }
                    else if (player.Role != RoleType.Spectator && player.Role != RoleType.Scp079)
                    {
                        for (int index = 0; index < __instance._usedData; ++index)
                        {
                            PlayerPositionData ppd = __instance._transmitBuffer[index];
                            Player currentTarget = players[index].GetPlayer();
                            Scp096 scp096 = player.Hub.scpsController.CurrentScp as Scp096;
                            bool canSee = true;
                            bool shouldRotate = false;
                            if (currentTarget?.Hub == null)
                                continue;
                            List<int> targetGhosts = new List<int>();
                            if (Server.PlayerList.TargetGhosts.TryGetValue(player.UserId, out List<int> list))
                                targetGhosts = list;
                            if (currentTarget.IsInvisible || targetGhosts.Contains(ppd.playerID))
                                canSee = false;
                            Vector3 vector3 = __instance._transmitBuffer[index].position - player.Hub.playerMovementSync.RealModelPosition;
                            if (Math.Abs(vector3.y) > 35.0)
                            {
                                canSee = false;
                            }
                            else
                            {
                                float sqrMagnitude = vector3.sqrMagnitude;
                                if (player.Hub.playerMovementSync.RealModelPosition.y < 800.0)
                                {
                                    if (sqrMagnitude >= 1764.0)
                                    {
                                        canSee = false;
                                        continue;
                                    }
                                }
                                else if (sqrMagnitude >= 7225.0)
                                {
                                    canSee = false;
                                    continue;
                                }

                                if (ReferenceHub.TryGetHub(__instance._transmitBuffer[index].playerID, out ReferenceHub hub2))
                                {
                                    if (player.Hub.scpsController.CurrentScp is Scp096 currentScp && currentScp.Enraged && (!currentScp.HasTarget(hub2) && hub2.characterClassManager.CurRole.team != Team.SCP))
                                    {
                                        canSee = false;
                                    }
                                    else if (hub2.playerEffectsController.GetEffect<Scp268>().Enabled)
                                    {
                                        bool flag = false;
                                        if (scp096 != null)
                                            flag = scp096._targets.Contains(hub2);
                                        canSee = flag;
                                    }
                                }

                                switch (player.Role)
                                {
                                    case RoleType.Scp173 when (!PluginManager.Config.GetBool("can_tutorial_block_scp173", true) && currentTarget.Role == RoleType.Tutorial) || CannotBlock173.Contains(currentTarget):
                                        shouldRotate = true;
                                        break;
                                    case RoleType.Scp096 when !PluginManager.Config.GetBool("can_tutorial_trigger_scp096", true) && currentTarget.Role == RoleType.Tutorial:
                                        shouldRotate = true;
                                        break;
                                }

                                if (!canSee)
                                {
                                    ppd = new PlayerPositionData(Vector3.up * 6000f, 0.0f, ppd.playerID);
                                }
                                else if (shouldRotate)
                                {
                                    ppd = new PlayerPositionData(ppd.position, Quaternion.LookRotation(player.Position.FindLookRotation(currentTarget.Position)).eulerAngles.y, ppd.playerID);
                                }
                                __instance._transmitBuffer[index] = ppd;
                            }
                        }
                    }

                    NetworkConnection networkConnection = player.Hub.characterClassManager.netIdentity.isLocalPlayer ? NetworkServer.localConnection : player.Hub.characterClassManager.netIdentity.connectionToClient;
                    if (__instance._usedData <= 20)
                    {
                        networkConnection.Send(new PlayerPositionManager.PositionMessage(__instance._transmitBuffer, (byte)__instance._usedData, 0), 1);
                    }
                    else
                    {
                        byte part;
                        for (part = 0; part < __instance._usedData / 20; ++part)
                            networkConnection.Send(new PlayerPositionManager.PositionMessage(__instance._transmitBuffer, 20, part), 1);
                        byte count = (byte)(__instance._usedData % (part * 20));
                        if (count > 0)
                            networkConnection.Send(new PlayerPositionManager.PositionMessage(__instance._transmitBuffer, count, part), 1);
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Add("PlayerPositionManager", e);
                return true;
            }
        }
    }
}

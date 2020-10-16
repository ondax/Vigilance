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
        public static List<string> CannotBlock173 { get; set; } = new List<string>();

        private static bool Prefix(PlayerPositionManager __instance)
        {
            try
            {
                if (++__instance._frame != __instance._syncFrequency)
                    return false;
                __instance._frame = 0;
                List<GameObject> players = PlayerManager.players;
                __instance._usedData = players.Count;
                if (__instance._receivedData == null || __instance._receivedData.Length < __instance._usedData)
                {
                    __instance._receivedData = new PlayerPositionData[__instance._usedData * 2];
                }
                for (int index = 0; index < __instance._usedData; ++index)
                    __instance._receivedData[index] = new PlayerPositionData(ReferenceHub.GetHub(players[index]));
                if (__instance._transmitBuffer == null || __instance._transmitBuffer.Length < __instance._usedData)
                {
                    __instance._transmitBuffer = new PlayerPositionData[__instance._usedData * 2];
                }

                foreach (GameObject gameObject in players)
                {
                    Player player = Server.PlayerList.GetPlayer(gameObject);
                    Array.Copy(__instance._receivedData, __instance._transmitBuffer, __instance._usedData);

                    if (player.Role.Is939())
                    {
                        for (int index = 0; index < __instance._usedData; ++index)
                        {
                            if (__instance._transmitBuffer[index].position.y < 800f)
                            {
                                ReferenceHub hub2 = ReferenceHub.GetHub(players[index]);

                                if (hub2.characterClassManager.CurRole.team != Team.SCP
                                    && hub2.characterClassManager.CurRole.team != Team.RIP
                                    && !players[index].GetComponent<Scp939_VisionController>().CanSee(player.Hub.characterClassManager.Scp939))
                                {
                                    __instance._transmitBuffer[index] = new PlayerPositionData(Vector3.up * 6000f, 0.0f, __instance._transmitBuffer[index].playerID);
                                }
                            }
                        }
                    }
                    else if (player.Role != RoleType.Spectator && player.Role != RoleType.Scp079)
                    {
                        for (int index = 0; index < __instance._usedData; ++index)
                        {
                            List<int> targetGhosts;
                            if (!Server.PlayerList.TargetGhosts.TryGetValue(player.UserId, out targetGhosts))
                            {
                                targetGhosts = new List<int>();
                                Server.PlayerList.TargetGhosts.Add(player.UserId, targetGhosts);
                            }

                            PlayerPositionData ppd = __instance._transmitBuffer[index];
                            Player currentTarget = Server.PlayerList.GetPlayer(players[index]);
                            Scp096 scp096 = player.Hub.scpsController.CurrentScp as Scp096;
                            bool canSee = true;
                            bool shouldRotate = false;
                            if (currentTarget?.Hub == null)
                                continue;
                            if (currentTarget.IsInvisible || targetGhosts.Contains(ppd.playerID))
                            {
                                canSee = false;
                            }
                            else
                            {
                                Vector3 vector3 = ppd.position - player.Hub.playerMovementSync.RealModelPosition;
                                if (Math.Abs(vector3.y) > 35f)
                                {
                                    canSee = false;
                                }
                                else
                                {
                                    float sqrMagnitude = vector3.sqrMagnitude;
                                    if (player.Hub.playerMovementSync.RealModelPosition.y < 800f)
                                    {
                                        if (sqrMagnitude >= 1764f)
                                        {
                                            canSee = false;
                                        }
                                    }
                                    else if (sqrMagnitude >= 7225f)
                                    {
                                        canSee = false;
                                    }

                                    if (canSee)
                                    {
                                        if (ReferenceHub.TryGetHub(ppd.playerID, out ReferenceHub hub2))
                                        {
                                            if (scp096 != null
                                                && scp096.Enraged
                                                && !scp096.HasTarget(hub2)
                                                && hub2.characterClassManager.CurRole.team != Team.SCP)
                                            {
                                                canSee = false;
                                            }
                                            else if (hub2.playerEffectsController.GetEffect<Scp268>().Enabled)
                                            {
                                                bool flag = false;
                                                if (scp096 != null)
                                                    flag = scp096.HasTarget(hub2);

                                                if (player.Role != RoleType.Scp079
                                                    && player.Role != RoleType.Spectator
                                                    && !flag)
                                                {
                                                    canSee = false;
                                                }
                                            }
                                        }

                                        switch (player.Role)
                                        {
                                            case RoleType.Scp173 when (!ConfigManager.CanTutorialBlockScp173 && currentTarget.Role == RoleType.Tutorial) || CannotBlock173.Contains(currentTarget.UserId):
                                                shouldRotate = true;
                                                break;
                                        }
                                    }
                                }
                            }

                            if (!canSee)
                            {
                                ppd = new PlayerPositionData(Vector3.up * 6000f, 0f, ppd.playerID);
                            }
                            else if (shouldRotate)
                            {
                                ppd = new PlayerPositionData(ppd.position, Quaternion.LookRotation(player.Position.FindLookRotation(currentTarget.Position)).eulerAngles.y, ppd.playerID);
                            }
                            __instance._transmitBuffer[index] = ppd;
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
            catch (Exception exception)
            {
                Log.Add(exception);
                return true;
            }
        }
    }
}

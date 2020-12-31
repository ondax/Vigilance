using System;
using System.Collections.Generic;
using CustomPlayerEffects;
using Vigilance.API;
using Harmony;
using Mirror;
using UnityEngine;
using Scp096 = PlayableScps.Scp096;
using Vigilance.Enums;

namespace Vigilance.Patches.Ghostmode
{
    [HarmonyPatch(typeof(PlayerPositionManager), nameof(PlayerPositionManager.TransmitData))]
    public static class PlayerPositionManager_TransmitData
    {
        public static bool Prefix(PlayerPositionManager __instance)
        {
            try
            {
                if (++__instance._frame != __instance._syncFrequency)
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
                    Player player = API.Ghostmode.GetPlayerOrServer(gameObject);
                    Array.Copy(__instance._receivedData, __instance._transmitBuffer, __instance._usedData);
                    if (player.Role.Is939())
                    {
                        for (int index = 0; index < __instance._usedData; ++index)
                        {
                            if (__instance._transmitBuffer[index].position.y < 800f)
                            {
                                ReferenceHub hub2 = ReferenceHub.GetHub(__instance._transmitBuffer[index].playerID);
                                if (hub2.characterClassManager.CurRole.team != Team.SCP && hub2.characterClassManager.CurRole.team != Team.RIP && !hub2.GetComponent<Scp939_VisionController>().CanSee(player.Hub.characterClassManager.Scp939))
                                    API.Ghostmode.MakeGhost(index, __instance._transmitBuffer);
                            }
                        }
                    }
                    else if (player.Role != RoleType.Spectator && player.Role != RoleType.Scp079)
                    {
                        for (int index = 0; index < __instance._usedData; ++index)
                        {
                            PlayerPositionData ppd = __instance._transmitBuffer[index];
                            if (!ReferenceHub.TryGetHub(ppd.playerID, out var targetHub))
                                continue;
                            Player currentTarget = API.Ghostmode.GetPlayerOrServer(targetHub.gameObject);
                            Scp096 scp096 = player.Hub.scpsController.CurrentScp as Scp096;
                            Vector3 pos = ppd.position - player.Hub.playerMovementSync.RealModelPosition;
                            if (Math.Abs(pos.y) > 35f)
                                API.Ghostmode.MakeGhost(index, __instance._transmitBuffer);
                            else
                            {
                                float sqrMagnitude = pos.sqrMagnitude;
                                if (player.Hub.playerMovementSync.RealModelPosition.y < 800f)
                                {
                                    if (sqrMagnitude >= 1764f)
                                    {
                                        if (!(sqrMagnitude < 4225f))
                                        {
                                            API.Ghostmode.MakeGhost(index, __instance._transmitBuffer);
                                            continue;
                                        }

                                        if (!(currentTarget.Hub.scpsController.CurrentScp is Scp096 scp) || !scp.EnragedOrEnraging)
                                        {
                                            API.Ghostmode.MakeGhost(index, __instance._transmitBuffer);
                                            continue;
                                        }
                                    }
                                }
                                else if (sqrMagnitude >= 7225f)
                                {
                                    API.Ghostmode.MakeGhost(index, __instance._transmitBuffer);
                                    continue;
                                }

                                if (scp096 != null && scp096.EnragedOrEnraging && !scp096.HasTarget(currentTarget.Hub) && currentTarget.Team != TeamType.SCP)
                                {
                                    API.Ghostmode.MakeGhost(index, __instance._transmitBuffer);
                                }

                                else if (currentTarget.Hub.playerEffectsController.GetEffect<Scp268>().Enabled)
                                {
                                    bool flag2 = false;
                                    if (scp096 != null)
                                        flag2 = scp096.HasTarget(currentTarget.Hub);
                                    if (player.Role != RoleType.Scp079 && player.Role != RoleType.Spectator && !flag2)
                                        API.Ghostmode.MakeGhost(index, __instance._transmitBuffer);
                                }
                            }
                        }
                    }

                    for (var z = 0; z < __instance._usedData; z++)
                    {
                        var ppd = __instance._transmitBuffer[z];
                        if (player.PlayerId == ppd.playerID)
                            continue;
                        if (ppd.position == API.Ghostmode.GhostPosition)
                            continue;
                        if (!ReferenceHub.TryGetHub(ppd.playerID, out var targetHub))
                            continue;
                        var target = API.Ghostmode.GetPlayerOrServer(targetHub.gameObject);
                        if (target?.Hub == null)
                            continue;
                        if (target.IsInvisible || API.Ghostmode.PlayerCannotSee(player, target.PlayerId))
                            API.Ghostmode.MakeGhost(z, __instance._transmitBuffer);
                        else if (player.Role == RoleType.Scp173 && ((!ConfigManager.CanTutorialBlockScp173 && target.Role == RoleType.Tutorial) || API.Ghostmode.CannotBlockScp173.Contains(target)))
                            API.Ghostmode.RotatePlayer(z, __instance._transmitBuffer, API.Ghostmode.FindLookRotation(player.Position, target.Position));
                    }

                    NetworkConnection networkConnection = player.Hub.characterClassManager.netIdentity.isLocalPlayer ? NetworkServer.localConnection : player.Hub.characterClassManager.netIdentity.connectionToClient;
                    if (__instance._usedData <= 20)
                        networkConnection.Send(new PlayerPositionManager.PositionMessage(__instance._transmitBuffer, (byte)__instance._usedData, 0), 1);
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
            catch (Exception)
            {
                return true;
            }
        }
    }
}
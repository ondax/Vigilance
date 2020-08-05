using Harmony;
using System;
using Vigilance.API.Extensions;
using Vigilance.API.Enums;
using Vigilance.Events;
using Vigilance.Handlers;
using UnityEngine;
using GameCore;
using Mirror;
using MEC;

namespace Vigilance.API.Patches
{

    [HarmonyPatch(typeof(BanPlayer), nameof(BanPlayer.BanUser), new[] { typeof(GameObject), typeof(int), typeof(string), typeof(string), typeof(bool) })]
    public static class BanEventPatchTwo
    {
        private static bool Prefix(GameObject user, int duration, string reason, string issuer, bool isGlobalBan)
        {
            try
            {
                FileLog.BanLog(issuer.GetPlayer(), user.GetPlayer(), reason, duration);
                if (isGlobalBan && ConfigFile.ServerConfig.GetBool("gban_ban_ip", false))
                {
                    duration = int.MaxValue;
                }

                string userId = null;
                string address = user.GetComponent<NetworkIdentity>().connectionToClient.address;
                Player targetPlayer = user.GetPlayer();
                Player issuerPlayer = issuer.GetPlayer();

                try
                {
                    if (ConfigFile.ServerConfig.GetBool("online_mode", false))
                        userId = targetPlayer.UserId;
                }
                catch
                {
                    ServerConsole.AddLog("Failed during issue of User ID ban (1)!");
                    return false;
                }

                string message = $"You have been {((duration > 0) ? "banned" : "kicked")}. ";
                if (!string.IsNullOrEmpty(reason))
                {
                    message = message + "Reason: " + reason;
                }

                if (!ServerStatic.PermissionsHandler.IsVerified || !targetPlayer.ServerRoles.BypassStaff)
                {
                    if (duration > 0)
                    {
                        BanEvent ev = new BanEvent(issuerPlayer, targetPlayer, duration, reason.GetBanReason(), true);
                        EventController.StartEvent<BanEventHandler>(ev);
                        Data.Sitrep.Post(Data.Sitrep.Translation.Ban(ev), Enums.PostType.Ban);
                        string originalName = string.IsNullOrEmpty(targetPlayer.Nick) ? "(no nick)" : targetPlayer.Nick;
                        long issuanceTime = TimeBehaviour.CurrentTimestamp();
                        long banExpieryTime = TimeBehaviour.GetBanExpieryTime((uint)duration);
                        try
                        {
                            if (userId != null && !isGlobalBan)
                            {
                                BanHandler.IssueBan(
                                    new BanDetails
                                    {
                                        OriginalName = originalName,
                                        Id = userId,
                                        IssuanceTime = issuanceTime,
                                        Expires = banExpieryTime,
                                        Reason = reason,
                                        Issuer = issuer,
                                    }, BanHandler.BanType.UserId);

                                if (!string.IsNullOrEmpty(targetPlayer.UserId))
                                {
                                    BanHandler.IssueBan(
                                        new BanDetails
                                        {
                                            OriginalName = originalName,
                                            Id = targetPlayer.UserId,
                                            IssuanceTime = issuanceTime,
                                            Expires = banExpieryTime,
                                            Reason = reason,
                                            Issuer = issuer,
                                        }, BanHandler.BanType.UserId);
                                }
                            }
                        }
                        catch
                        {
                            ServerConsole.AddLog("Failed during issue of User ID ban (2)!");
                            return false;
                        }

                        try
                        {
                            if (ConfigFile.ServerConfig.GetBool("ip_banning", false) || isGlobalBan)
                            {
                                BanHandler.IssueBan(
                                    new BanDetails
                                    {
                                        OriginalName = originalName,
                                        Id = address,
                                        IssuanceTime = issuanceTime,
                                        Expires = banExpieryTime,
                                        Reason = reason,
                                        Issuer = issuer,
                                    }, BanHandler.BanType.IP);
                            }
                        }
                        catch
                        {
                            ServerConsole.AddLog("Failed during issue of IP ban!");
                            return false;
                        }
                    }
                }
                targetPlayer.Kick(message);
                return false;
            }
            catch (Exception e)
            {
                Log.Error("BanPlayer", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdUseElevator), typeof(GameObject))]
    public static class ElevatorInteractEventPatch
    {
        private static bool Prefix(PlayerInteract __instance, GameObject elevator)
        {
            try
            {
                if (!__instance._playerInteractRateLimit.CanExecute(true) ||
                    (__instance._hc.CufferId > 0 && !PlayerInteract.CanDisarmedInteract) || elevator == null)
                    return false;
                Lift component = elevator.GetComponent<Lift>();
                if (component == null)
                    return false;
                Player player = __instance.GetPlayer();
                if (player.PlayerLock)
                    return false;
                ElevatorInteractEvent ev = new ElevatorInteractEvent(player, component, true);
                EventController.StartEvent<ElevatorInteractEventHandler>(ev);
                if (!ev.Allow)
                    return false;
                foreach (Lift.Elevator elevator2 in component.elevators) 
                {
                    if (__instance.ChckDis(elevator2.door.transform.position))
                    {
                        elevator.GetComponent<Lift>().UseLift();
                        __instance.OnInteract();
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                Log.Error("PlayerInteract", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.AllowContain))]
    public static class LureEventPatch
    {
        private static bool Prefix(CharacterClassManager __instance)
        {
            try
            {
                if (!NetworkServer.active || !NonFacilityCompatibility.currentSceneSettings.enableStandardGamplayItems)
                    return false;
                foreach (GameObject player in PlayerManager.players)
                {
                    if (Vector3.Distance(player.transform.position, __instance._lureSpj.transform.position) < 1.97000002861023)
                    {
                        CharacterClassManager component1 = player.GetComponent<CharacterClassManager>();
                        PlayerStats component2 = player.GetComponent<PlayerStats>();
                        if (component1.Classes.SafeGet(component1.CurClass).team != Team.SCP &&
                            component1.CurClass != RoleType.Spectator && !component1.GodMode)
                        {
                            Player ply = __instance.GetPlayer();
                            LureEvent ev = new LureEvent(ply, true);
                            EventController.StartEvent<LureEventHandler>(ev);
                            Data.Sitrep.Post(Data.Sitrep.Translation.LureEvent(ev.Player), Enums.PostType.Sitrep);
                            if (ev.Allow)
                            {
                                component2.HurtPlayer(new PlayerStats.HitInfo(10000f, "WORLD", DamageTypes.Lure, 0), player);
                                __instance._lureSpj.SetState(true);
                            }
                        }
                    }
                }

                return false;
            }
            catch (Exception e)
            {
                Log.Error("CharacterClassManager", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.HurtPlayer))]
    public static class PlayerDeathEventPatch
    {
        private static void Postfix(PlayerStats __instance, ref PlayerStats.HitInfo info, GameObject go)
        {
            try
            {
                Player attacker = __instance.GetPlayer();
                Player target = go.GetPlayer();
                if ((target == null && (target.Role != RoleType.Spectator || target.GodMode || target.ClassManager.IsHost)) || attacker == null)
                    return;
                DamageType dmgType = info.Attacker.ToLower() == "disconnect" ? DamageType.Disconnect : info.GetDamageType().Convert();
                dmgType = info.Attacker.ToLower() == "cmdsuicide" ? DamageType.CmdSuicide : info.GetDamageType().Convert();
                PlayerDeathEvent ev = new PlayerDeathEvent(target, attacker, true, dmgType);
                EventController.StartEvent<PlayerDeathEventHandler>(ev);
                Data.Sitrep.Post(Data.Sitrep.Translation.PlayerDeathEvent(ev.Killer, ev.Player, info), Enums.PostType.Sitrep);
                FileLog.KillLog(attacker, target, info);
            }
            catch (Exception e)
            {
                Log.Error("PlayerStats", e);
            }
        }
    }

    [HarmonyPatch(typeof(NicknameSync), nameof(NicknameSync.SetNick))]
    public class PlayerJoinEventPatch
    {
        public static void Postfix(NicknameSync __instance)
        {
            try
            {
                Player player = __instance.gameObject.GetPlayer();
                Timing.CallDelayed(0.25f, () =>
                {
                    if (player != null && player.IsMuted)
                        player.ClassManager.SetDirtyBit(1UL);
                });
                if (!player.UserId.IsEmpty() && player.IpAdress != "localClient")
                {
                    ServerGuard.SteamShield.CheckAccount(player);
                    ServerGuard.VPNShield.CheckIP(player);
                    EventController.StartEvent<PlayerJoinEventHandler>(new PlayerJoinEvent(player));
                    Data.NicknameFilter.CheckNickname(player);
                    Data.Sitrep.Post(Data.Sitrep.Translation.JoinEvent(player), Enums.PostType.Sitrep);
                }
            }
            catch (Exception exception)
            {
                Log.Error("NicknameSync", exception);
            }
        }
    }

    [HarmonyPatch(typeof(ReferenceHub), nameof(ReferenceHub.OnDestroy))]
    public static class PlayerLeaveEventPatch
    {
        private static void Prefix(ReferenceHub __instance)
        {
            try
            {
                Player player = __instance.GetPlayer();
                if (player == null || player.ClassManager.IsHost)
                    return;
                PlayerLeaveEvent ev = new PlayerLeaveEvent(player);
                EventController.StartEvent<PlayerLeaveEventHandler>(ev);
                Data.Sitrep.Post(Data.Sitrep.Translation.LeaveEvent(ev.Player), Enums.PostType.Sitrep);
            }
            catch (Exception e)
            {
                Log.Error("ReferenceHub", e);
            }
        }
    }

    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.ApplyProperties))]
    public static class PlayerSpawnEventPatch
    {
        private static bool Prefix(CharacterClassManager __instance, bool lite = false, bool escape = false)
        {
            try
            {
                Role role = __instance.Classes.SafeGet(__instance.CurClass);
                if (!__instance._wasAnytimeAlive && __instance.CurClass != RoleType.Spectator && __instance.CurClass != RoleType.None)
                {
                    __instance._wasAnytimeAlive = true;
                }
                __instance.InitSCPs();
                __instance.AliveTime = 0f;
                switch (role.team)
                {
                    case Team.MTF:
                        AchievementManager.Achieve("arescue");
                        break;
                    case Team.CHI:
                        AchievementManager.Achieve("chaos");
                        break;
                    case Team.RSC:
                    case Team.CDP:
                        __instance.EscapeStartTime = (int)Time.realtimeSinceStartup;
                        break;
                }
                __instance.GetComponent<Inventory>();
                try
                {
                    __instance.GetComponent<FootstepSync>().SetLoudness(role.team, role.roleId.Is939());
                }
                catch (Exception)
                {
                }

                if (NetworkServer.active)
                {
                    Handcuffs component = __instance.GetComponent<Handcuffs>();
                    component.ClearTarget();
                    component.NetworkCufferId = -1;
                }

                if (role.team != Team.RIP)
                {
                    if (NetworkServer.active && !lite)
                    {
                        Vector3 constantRespawnPoint = NonFacilityCompatibility.currentSceneSettings.constantRespawnPoint;
                        if (constantRespawnPoint != Vector3.zero)
                        {
                            __instance._pms.OnPlayerClassChange(constantRespawnPoint, 0f);
                        }
                        else
                        {
                            GameObject randomPosition = CharacterClassManager._spawnpointManager.GetRandomPosition(__instance.CurClass);
                            Vector3 spawnPoint;
                            float rotY;
                            if (randomPosition != null)
                            {
                                spawnPoint = randomPosition.transform.position;
                                rotY = randomPosition.transform.rotation.eulerAngles.y;
                                AmmoBox component1 = __instance.GetComponent<AmmoBox>();
                                if (escape && __instance.KeepItemsAfterEscaping)
                                {
                                    Inventory component2 = PlayerManager.localPlayer.GetComponent<Inventory>();
                                    for (ushort index = 0; index < 3; ++index)
                                    {
                                        if (component1[index] >= 15U)
                                        {
                                            component2.SetPickup(component1.types[index].inventoryID, component1[index], randomPosition.transform.position, randomPosition.transform.rotation, 0, 0, 0);
                                        }
                                    }
                                }
                                component1.ResetAmmo();
                            }
                            else
                            {
                                spawnPoint = __instance.DeathPosition;
                                rotY = 0f;
                            }
                            Player ply = __instance.GetPlayer();
                            PlayerSpawnEvent ev = new PlayerSpawnEvent(ply);
                            EventController.StartEvent<PlayerSpawnEventHandler>(ev);
                            Data.ScpHealing.StartHealing(ply);
                            __instance._pms.OnPlayerClassChange(randomPosition.transform.position, rotY);
                        }
                        if (!__instance.SpawnProtected && __instance.EnableSP && __instance.SProtectedTeam.Contains((int)role.team))
                        {
                            __instance.GodMode = true;
                            __instance.SpawnProtected = true;
                            __instance.ProtectedTime = Time.time;
                        }
                    }
                    if (!__instance.isLocalPlayer)
                    {
                        __instance.GetComponent<PlayerStats>().maxHP = role.maxHP;
                    }
                }
                __instance.Scp0492.iAm049_2 = __instance.CurClass == RoleType.Scp0492;
                __instance.Scp106.iAm106 = __instance.CurClass == RoleType.Scp106;
                __instance.Scp173.iAm173 = __instance.CurClass == RoleType.Scp173;
                __instance.Scp939.iAm939 = __instance.CurClass.Is939();
                __instance.RefreshPlyModel();
                return false;
            }
            catch (Exception)
            {
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(ConsumableAndWearableItems), nameof(ConsumableAndWearableItems.CallCmdUseMedicalItem))]
    public static class UseMedicalItemEventPatch
    {
        private static bool Prefix(ConsumableAndWearableItems __instance)
        {
            try
            {
                if (!__instance._interactRateLimit.CanExecute(true))
                    return false;
                __instance._cancel = false;
                if (__instance.cooldown > 0.0)
                    return false;
                for (int i = 0; i < __instance.usableItems.Length; ++i)
                {
                    if (__instance.usableItems[i].inventoryID == __instance._hub.inventory.curItem &&  __instance.usableCooldowns[i] <= 0.0)
                    {
                        __instance.cooldown = __instance.usableItems[i].animationDuration;
                        UseMedicalItemEvent ev = new UseMedicalItemEvent(__instance.gameObject.GetPlayer(), (int)__instance.hpToHeal);
                        ev.Allow = true;
                        EventController.StartEvent<UseMedicalItemEventHandler>(ev);
                        if (ev.Allow)
                            Timing.RunCoroutine(__instance.UseMedicalItem(i), Segment.FixedUpdate);
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Error("ConsumableAndWearableItems", e);
                return true;
            }
        }
    }
}

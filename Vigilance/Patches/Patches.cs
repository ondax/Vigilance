using System;
using System.Collections.Generic;
using System.Linq;
using GameCore;
using Harmony;
using LightContainmentZoneDecontamination;
using Respawning.NamingRules;
using Vigilance.API;
using Vigilance.Extensions;
using Grenades;
using UnityEngine;
using CustomPlayerEffects;
using NorthwoodLib.Pools;
using Mirror;
using System.Text;
using RemoteAdmin;
using Dissonance.Integrations.MirrorIgnorance;
using PlayableScps;
using PlayableScps.Interfaces;
using Respawning;
using MEC;
using Console = GameCore.Console;
using Scp914;
using Cryptography;
using System.Threading;
using System.Reflection;
using System.Reflection.Emit;
using Vigilance.Events;
using Searching;

namespace Vigilance.Patches
{
    [HarmonyPatch(typeof(DecontaminationController), nameof(DecontaminationController.UpdateSpeaker))]
    public static class AnnounceDecontaminationPatch
    {
        private static bool Prefix(DecontaminationController __instance, bool hard)
        {
            try
            {
                if (AnnounceNTFEntrancePatch.CassieDisabled)
                    return false;
                Environment.OnAnnounceDecontamination(hard, __instance._nextPhase, true, out hard, out __instance._nextPhase, out bool allow);
                if (!allow)
                    return false;
                float b = 0f;
                if (__instance._curFunction == DecontaminationController.DecontaminationPhase.PhaseFunction.Final || __instance._curFunction == DecontaminationController.DecontaminationPhase.PhaseFunction.GloballyAudible)
                    b = 1f;
                else if (Mathf.Abs(SpectatorCamera.Singleton.cam.transform.position.y) < 200f)
                    b = 1f;
                __instance.AnnouncementAudioSource.volume = Mathf.Lerp(__instance.AnnouncementAudioSource.volume, b, hard ? 1f : (Time.deltaTime * 4f));
                return false;
            }
            catch (Exception e)
            {
                Log.Add("DecontaminationController", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(NineTailedFoxNamingRule), nameof(NineTailedFoxNamingRule.PlayEntranceAnnouncement))]
    public static class AnnounceNTFEntrancePatch
    {
        public static bool CassieDisabled = false;

        private static bool Prefix(NineTailedFoxNamingRule __instance, string regular)
        {
            try
            {
                if (CassieDisabled)
                    return false;
                string cassieUnitName = __instance.GetCassieUnitName(regular);
                int num = Server.Players.Where(p => p.IsSCP).Count();
                string[] args = cassieUnitName.Replace("-", " ").Split(' ');
                string u = args[0];
                int n = int.Parse(args[1]);
                StringBuilder stringBuilder = StringBuilderPool.Shared.Rent();
                Environment.OnAnnounceNTFEntrace(num, u, n, true, out num, out string unit, out int number, out bool allow);
                cassieUnitName = $"{unit}-{number}";
                if (!allow)
                    return false;
                if (ClutterSpawner.IsHolidayActive(Holidays.Christmas))
                {
                    stringBuilder.Append("XMAS_EPSILON11 ");
                    stringBuilder.Append(cassieUnitName);
                    stringBuilder.Append("XMAS_HASENTERED ");
                    stringBuilder.Append(num);
                    stringBuilder.Append(" XMAS_SCPSUBJECTS");
                }
                else
                {
                    stringBuilder.Append("MTFUNIT EPSILON 11 DESIGNATED ");
                    stringBuilder.Append(cassieUnitName);
                    stringBuilder.Append(" HASENTERED ALLREMAINING ");
                    if (num == 0)
                    {
                        stringBuilder.Append("NOSCPSLEFT");
                    }
                    else
                    {
                        stringBuilder.Append("AWAITINGRECONTAINMENT ");
                        stringBuilder.Append(num);
                        if (num == 1)
                        {
                            stringBuilder.Append(" SCPSUBJECT");
                        }
                        else
                        {
                            stringBuilder.Append(" SCPSUBJECTS");
                        }
                    }
                }
                __instance.ConfirmAnnouncement(ref stringBuilder);
                StringBuilderPool.Shared.Return(stringBuilder);
                return false;
            }
            catch (Exception e)
            {
                Log.Add("NineTailedFoxNamingRule", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(NineTailedFoxAnnouncer), nameof(NineTailedFoxAnnouncer.AnnounceScpTermination))]
    public static class AnnounceSCPTerminationPatch
    {
        private static bool Prefix(Role scp, PlayerStats.HitInfo hit, string groupId)
        {
            try
            {
                if (AnnounceNTFEntrancePatch.CassieDisabled)
                    return false;
                Player attacker = hit.GetAttacker();
                Environment.OnAnnounceSCPTermination(attacker == null ? Server.PlayerList.Local.GameObject : attacker.GameObject, scp, hit, hit.Attacker, true, out bool allow);
                if (!allow)
                    return false;
                NineTailedFoxAnnouncer.singleton.scpListTimer = 0f;
                if (!string.IsNullOrEmpty(groupId))
                {
                    foreach (NineTailedFoxAnnouncer.ScpDeath scpDeath in NineTailedFoxAnnouncer.scpDeaths)
                    {
                        if (scpDeath.group == groupId)
                        {
                            scpDeath.scpSubjects.Add(scp);
                            return false;
                        }
                    }
                }
                NineTailedFoxAnnouncer.scpDeaths.Add(new NineTailedFoxAnnouncer.ScpDeath
                {
                    scpSubjects = new List<Role>(new Role[]
                    {
                        scp
                    }),
                    group = groupId,
                    hitInfo = hit
                });
                return false;
            }
            catch (Exception e)
            {
                Log.Add("NineTailedFoxAnnouncer", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(DecontaminationController), nameof(DecontaminationController.ServersideSetup))]
    public static class PreventDeconPatch
    {
        public static bool Prefix(DecontaminationController __instance)
        {
            try
            {
                if (__instance.RoundStartTime == 0.0)
                {
                    if (__instance._disableDecontamination || DecontaminationPatch.DecontDisabled)
                    {
                        __instance.NetworkRoundStartTime = -1.0;
                        __instance._stopUpdating = true;
                    }
                    else if (RoundStart.singleton.Timer == -1)
                    {
                        __instance.NetworkRoundStartTime = NetworkTime.time;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Add("DecontaminationController", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(DecontaminationController), nameof(DecontaminationController.FinishDecontamination))]
    public static class DecontaminationPatch
    {
        public static bool DecontDisabled = false;

        private static bool Prefix(DecontaminationController __instance)
        {
            try
            {
                if (NetworkServer.active)
                {
                    Environment.OnDecontamination(true, out bool allow);
                    if (!allow || !DecontDisabled)
                        return false;
                    foreach (Lift lift in Lift.Instances)
                    {
                        if (lift != null)
                        {
                            lift.Lock();
                        }
                    }
                    foreach (GameObject gameObject in __instance.LczGenerator.doors)
                    {
                        if (gameObject != null && gameObject.gameObject.activeSelf)
                        {
                            Door component = gameObject.GetComponent<Door>();
                            if (component != null)
                            {
                                component.CloseDecontamination();
                            }
                        }
                    }
                    foreach (DecontaminationEvacuationDoor decontaminationEvacuationDoor in DecontaminationEvacuationDoor.Instances)
                    {
                        if (decontaminationEvacuationDoor != null)
                        {
                            decontaminationEvacuationDoor.Close();
                        }
                    }
                    __instance._decontaminationBegun = true;
                    return false;
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Add("DecontaminationController", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Generator079), nameof(Generator079.CheckFinish))]
    public static class GeneratorFinishPatch
    {
        private static bool Prefix(Generator079 __instance)
        {
            if (__instance.prevFinish || __instance._localTime > 0.0)
                return false;
            Environment.OnGeneratorFinish(__instance, true, out bool allow);
            if (!allow)
                return false;
            __instance.prevFinish = true;
            __instance.epsenRenderer.sharedMaterial = __instance.matLetGreen;
            __instance.epsdisRenderer.sharedMaterial = __instance.matLedBlack;
            __instance._asource.PlayOneShot(__instance.unlockSound);
            return false;
        }
    }

    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.RpcPlaceBlood))]
    public static class PlaceBloodPatch
    {
        private static bool Prefix(CharacterClassManager __instance, ref Vector3 pos, ref int type, ref float f)
        {
            Environment.OnPlaceBlood(type, pos, true, out Vector3 position, out bool allow);
            pos = position;
            return allow && PluginManager.Config.GetBool("enable_blood_spawning", true);
        }
    }

    [HarmonyPatch(typeof(WeaponManager), nameof(WeaponManager.RpcPlaceDecal))]
    public static class PlaceDecalPatch
    {
        private static bool Prefix(WeaponManager __instance, bool isBlood, ref int type, ref Vector3 pos, ref Quaternion rot)
        {
            if (isBlood)
            {
                Environment.OnPlaceBlood(type, pos, true, out Vector3 position, out bool allow);
                pos = position;
                return allow && PluginManager.Config.GetBool("enable_blood_spawning", true);
            }
            else
            {
                Environment.OnPlaceDecal(pos, true, out Vector3 position, out bool allow);
                pos = position;
                return allow && PluginManager.Config.GetBool("enable_decal_spawning", true);
            }
        }
    }

    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdSwitchAWButton))]
    public static class WarheadKeycardAccessPatch
    {
        private static bool Prefix(PlayerInteract __instance)
        {
            try
            {
                if (!__instance._playerInteractRateLimit.CanExecute(true) || (__instance._hc.CufferId > 0 && !PlayerInteract.CanDisarmedInteract))
                {
                    return false;
                }
                GameObject gameObject = GameObject.Find("OutsitePanelScript");
                if (!__instance.ChckDis(gameObject.transform.position))
                {
                    return false;
                }
                Item itemByID = __instance._inv.GetItemByID(__instance._inv.curItem);
                Environment.OnWarheadKeycardAccess(__instance.gameObject, __instance._sr.BypassMode || (itemByID != null && itemByID.permissions.Contains("CONT_LVL_3")), out bool allow);
                if (allow)
                {
                    gameObject.GetComponentInParent<AlphaWarheadOutsitePanel>().NetworkkeycardEntered = true;
                    __instance.OnInteract();
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Add("PlayerInteract", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(BanPlayer), nameof(BanPlayer.BanUser), new[] { typeof(GameObject), typeof(int), typeof(string), typeof(string), typeof(bool) })]
    internal static class BanEventPatch
    {
        private static bool Prefix(GameObject user, int duration, string reason, string issuer, bool isGlobalBan)
        {
            try
            {
                if (isGlobalBan && ConfigFile.ServerConfig.GetBool("gban_ban_ip", false))
                {
                    duration = int.MaxValue;
                }
                string userId = null;
                string address = user.GetComponent<NetworkIdentity>().connectionToClient.address;
                Player targetPlayer = user.GetPlayer();
                Player issuerPlayer = issuer.GetPlayer();
                reason = string.IsNullOrEmpty(reason) ? "No reason provided." : reason;
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
                if (!ServerStatic.PermissionsHandler.IsVerified || !targetPlayer.Hub.serverRoles.BypassStaff)
                {
                    if (duration > 0)
                    {
                        string originalName = string.IsNullOrEmpty(targetPlayer.Nick) ? "(no nick)" : targetPlayer.Nick;
                        long issuanceTime = TimeBehaviour.CurrentTimestamp();
                        long banExpieryTime = TimeBehaviour.GetBanExpieryTime((uint)duration);
                        Environment.OnBan(issuerPlayer.GameObject, targetPlayer.GameObject, reason, issuanceTime, banExpieryTime, true, out banExpieryTime, out bool allow);
                        if (!allow)
                            return false;
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
                ServerConsole.Disconnect(targetPlayer.GameObject, message);
                return false;
            }
            catch (Exception e)
            {
                Log.Add("BanPlayer", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(ServerRoles), nameof(ServerRoles.SetGroup))]
    public static class SetGroupPatch
    {
        private static bool Prefix(ServerRoles __instance, UserGroup group, bool ovr, bool byAdmin = false, bool disp = false)
        {
            try
            {
                if (!NetworkServer.active)
                {
                    return false;
                }

                if (group == null)
                {
                    if (__instance.RaEverywhere && __instance._globalPerms == ServerStatic.PermissionsHandler.FullPerm)
                    {
                        return false;
                    }
                    __instance.RemoteAdmin = (__instance._globalPerms > 0UL);
                    __instance.Permissions = __instance._globalPerms;
                    __instance.RemoteAdminMode = ((__instance._globalPerms == 0UL) ? ServerRoles.AccessMode.LocalAccess : ServerRoles.AccessMode.GlobalAccess);
                    __instance.AdminChatPerms = (PermissionsHandler.IsPermitted(__instance.Permissions, PlayerPermissions.AdminChat) || __instance.RaEverywhere);
                    __instance.Group = null;
                    __instance.SetColor(null);
                    __instance.SetText(null);
                    __instance._badgeCover = false;
                    if (!string.IsNullOrEmpty(__instance.PrevBadge))
                    {
                        __instance.NetworkGlobalBadge = __instance.PrevBadge;
                    }
                    __instance.TargetCloseRemoteAdmin(__instance.connectionToClient);
                    __instance.SendRealIds();
                    __instance._ccm.TargetConsolePrint(__instance.connectionToClient, "Your local permissions has been revoked by the server.", "red");
                    return false;
                }
                else
                {
                    Environment.OnSetGroup(__instance.gameObject, group, true, out group, out bool allow);
                    if (!allow)
                        return false;
                    __instance._ccm.TargetConsolePrint(__instance.connectionToClient, (!byAdmin) ? "Updating your group on server (local permissions)..." : "Updating your group on server (set by server administrator)...", "cyan");
                    __instance.Group = group;
                    __instance._badgeCover = group.Cover;
                    if (!__instance.OverwatchPermitted && PermissionsHandler.IsPermitted(group.Permissions, PlayerPermissions.Overwatch))
                    {
                        __instance.OverwatchPermitted = true;
                    }
                    if ((group.Permissions | __instance._globalPerms) > 0UL && ServerStatic.PermissionsHandler.IsRaPermitted(group.Permissions | __instance._globalPerms))
                    {
                        __instance.RemoteAdmin = true;
                        __instance.Permissions = (group.Permissions | __instance._globalPerms);
                        __instance.RemoteAdminMode = ((__instance._globalPerms > 0UL) ? ServerRoles.AccessMode.GlobalAccess : (ovr ? ServerRoles.AccessMode.PasswordOverride : ServerRoles.AccessMode.LocalAccess));
                        __instance.AdminChatPerms = (PermissionsHandler.IsPermitted(__instance.Permissions, PlayerPermissions.AdminChat) || __instance.RaEverywhere);
                        __instance.GetComponent<QueryProcessor>().PasswordTries = 0;
                        __instance.TargetOpenRemoteAdmin(__instance.connectionToClient, ovr);
                        __instance._ccm.TargetConsolePrint(__instance.connectionToClient, (!byAdmin) ? "Your remote admin access has been granted (local permissions)." : "Your remote admin access has been granted (set by server administrator).", "cyan");
                    }
                    else
                    {
                        __instance.RemoteAdmin = false;
                        __instance.Permissions = (group.Permissions | __instance._globalPerms);
                        __instance.RemoteAdminMode = ((__instance._globalPerms > 0UL) ? ServerRoles.AccessMode.GlobalAccess : ServerRoles.AccessMode.LocalAccess);
                        __instance.AdminChatPerms = (PermissionsHandler.IsPermitted(__instance.Permissions, PlayerPermissions.AdminChat) || __instance.RaEverywhere);
                        __instance.TargetCloseRemoteAdmin(__instance.connectionToClient);
                    }
                    __instance.SendRealIds();
                    bool flag = __instance.Staff || PermissionsHandler.IsPermitted(__instance.Permissions, PlayerPermissions.ViewHiddenBadges);
                    bool flag2 = __instance.Staff || PermissionsHandler.IsPermitted(__instance.Permissions, PlayerPermissions.ViewHiddenGlobalBadges);
                    if (flag || flag2)
                    {
                        foreach (GameObject gameObject in PlayerManager.players)
                        {
                            ServerRoles component = gameObject.GetComponent<ServerRoles>();
                            if (!string.IsNullOrEmpty(component.HiddenBadge) && (!component.GlobalHidden || flag2) && (component.GlobalHidden || flag))
                            {
                                component.TargetSetHiddenRole(__instance.connectionToClient, component.HiddenBadge);
                            }
                        }
                        if (flag && flag2)
                        {
                            __instance._ccm.TargetConsolePrint(__instance.connectionToClient, "Hidden badges (local and global) have been displayed for you (if there are any).", "gray");
                        }
                        else if (flag)
                        {
                            __instance._ccm.TargetConsolePrint(__instance.connectionToClient, "Hidden badges (local only) have been displayed for you (if there are any).", "gray");
                        }
                        else
                        {
                            __instance._ccm.TargetConsolePrint(__instance.connectionToClient, "Hidden badges (global only) have been displayed for you (if there are any).", "gray");
                        }
                    }
                    ServerLogs.AddLog(ServerLogs.Modules.Permissions, __instance._hub.LoggedNameFromRefHub() + " has been assigned to group " + group.BadgeText + ".", ServerLogs.ServerLogType.ConnectionUpdate, false);
                    if (group.BadgeColor == "none")
                    {
                        return false;
                    }
                    if (__instance._hideLocalBadge || (group.HiddenByDefault && !disp && !__instance._neverHideLocalBadge))
                    {
                        __instance._badgeCover = false;
                        if (!string.IsNullOrEmpty(__instance.MyText))
                        {
                            return false;
                        }
                        __instance.NetworkMyText = null;
                        __instance.NetworkMyColor = "default";
                        __instance.HiddenBadge = group.BadgeText;
                        __instance.RefreshHiddenTag();
                        __instance.TargetSetHiddenRole(__instance.connectionToClient, group.BadgeText);
                        if (!byAdmin)
                        {
                            __instance._ccm.TargetConsolePrint(__instance.connectionToClient, "Your role has been granted, but it's hidden. Use \"showtag\" command in the game console to show your server badge.", "yellow");
                            return false;
                        }
                        __instance._ccm.TargetConsolePrint(__instance.connectionToClient, "Your role has been granted to you (set by server administrator), but it's hidden. Use \"showtag\" command in the game console to show your server badge.", "cyan");
                        return false;
                    }
                    else
                    {
                        __instance.HiddenBadge = null;
                        __instance.RpcResetFixed();
                        __instance.NetworkMyText = group.BadgeText;
                        __instance.NetworkMyColor = group.BadgeColor;
                        if (!byAdmin)
                        {
                            __instance._ccm.TargetConsolePrint(__instance.connectionToClient, string.Concat(new string[]
                            {
                                "Your role \"",
                                 group.BadgeText,
                                 "\" with color ",
                                group.BadgeColor,
                                " has been granted to you (local permissions)."
                            }), "cyan");
                            return false;
                        }
                        __instance._ccm.TargetConsolePrint(__instance.connectionToClient, string.Concat(new string[]
                        {
                            "Your role \"",
                            group.BadgeText,
                            "\" with color ",
                            group.BadgeColor,
                            " has been granted to you (set by server administrator)."
                        }), "cyan");
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Add("ServerRoles", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.HurtPlayer))]
    public static class HurtPlayerPatch
    {
        public static bool Prefix(PlayerStats __instance, PlayerStats.HitInfo info, GameObject go, bool noTeamDamage = false)
        {
            try
            {
                bool flag = false;
                bool flag2 = false;
                bool flag3 = go == null;
                ReferenceHub referenceHub = flag3 ? null : ReferenceHub.GetHub(go);
                if (info.Amount < 0f)
                {
                    if (flag3)
                    {
                        info.Amount = Mathf.Abs(999999f);
                    }
                    else
                    {
                        info.Amount = ((referenceHub.playerStats != null) ? Mathf.Abs(referenceHub.playerStats.Health + referenceHub.playerStats.syncArtificialHealth + 10f) : Mathf.Abs(999999f));
                    }
                }

                if (__instance._burned.Enabled)
                {
                    info.Amount *= __instance._burned.DamageMult;
                }

                if (info.Amount > 2.1474836E+09f)
                {
                    info.Amount = 2.1474836E+09f;
                }

                if (info.GetDamageType().isWeapon && referenceHub.characterClassManager.IsAnyScp() && info.GetDamageType() != DamageTypes.MicroHid)
                {
                    info.Amount *= __instance.weaponManager.weapons[(int)__instance.weaponManager.curWeapon].scpDamageMultiplier;
                }

                if (flag3)
                {
                    return false;
                }

                PlayerStats playerStats = referenceHub.playerStats;
                CharacterClassManager characterClassManager = referenceHub.characterClassManager;

                if (playerStats == null || characterClassManager == null)
                {
                    return false;
                }

                if (characterClassManager.GodMode)
                {
                    return false;
                }

                if (__instance.ccm.CurRole.team == Team.SCP && __instance.ccm.Classes.SafeGet(characterClassManager.CurClass).team == Team.SCP && __instance.ccm != characterClassManager)
                {
                    return false;
                }

                if (characterClassManager.SpawnProtected && !__instance._allowSPDmg)
                {
                    return false;
                }
                GameObject attacker = __instance.gameObject ?? ReferenceHub.LocalHub.gameObject;
                GameObject target = go ?? ReferenceHub.LocalHub.gameObject;
                Environment.OnHurt(target, attacker, info, true, out info, out bool allow);
                if (!allow)
                    return false;
                bool flag4 = !noTeamDamage && info.IsPlayer && referenceHub != info.RHub && referenceHub.characterClassManager.Fraction == info.RHub.characterClassManager.Fraction;
                if (flag4)
                {
                    info.Amount *= PlayerStats.FriendlyFireFactor;
                }

                float health = playerStats.Health;
                if (__instance.lastHitInfo.Attacker == "ARTIFICIALDEGEN")
                {
                    playerStats.unsyncedArtificialHealth -= info.Amount;
                    if (playerStats.unsyncedArtificialHealth < 0f)
                    {
                        playerStats.unsyncedArtificialHealth = 0f;
                    }
                }
                else
                {
                    if (playerStats.unsyncedArtificialHealth > 0f)
                    {
                        float num = info.Amount * playerStats.artificialNormalRatio;
                        float num2 = info.Amount - num;
                        playerStats.unsyncedArtificialHealth -= num;
                        if (playerStats.unsyncedArtificialHealth < 0f)
                        {
                            num2 += Mathf.Abs(playerStats.unsyncedArtificialHealth);
                            playerStats.unsyncedArtificialHealth = 0f;
                        }
                        playerStats.Health -= num2;
                        if (playerStats.Health > 0f && playerStats.Health - num <= 0f && characterClassManager.CurRole.team != Team.SCP)
                        {
                            __instance.TargetAchieve(characterClassManager.connectionToClient, "didntevenfeelthat");
                        }
                    }
                    else
                    {
                        playerStats.Health -= info.Amount;
                    }
                    if (playerStats.Health < 0f)
                    {
                        playerStats.Health = 0f;
                    }
                    playerStats.lastHitInfo = info;
                }

                PlayableScpsController component = go.GetComponent<PlayableScpsController>();
                IDamagable damagable;
                if (component != null && (damagable = (component.CurrentScp as IDamagable)) != null)
                {
                    damagable.OnDamage(info);
                }

                if (playerStats.Health < 1f && characterClassManager.CurClass != RoleType.Spectator)
                {
                    IImmortalScp immortalScp;
                    if (component != null && (immortalScp = (component.CurrentScp as IImmortalScp)) != null && !immortalScp.OnDeath(info, __instance.gameObject))
                    {
                        return false;
                    }
                    foreach (Scp079PlayerScript scp079PlayerScript in Scp079PlayerScript.instances)
                    {
                        Scp079Interactable.ZoneAndRoom otherRoom = go.GetComponent<Scp079PlayerScript>().GetOtherRoom();
                        bool flag5 = false;
                        foreach (Scp079Interaction scp079Interaction in scp079PlayerScript.ReturnRecentHistory(12f, __instance._filters))
                        {
                            foreach (Scp079Interactable.ZoneAndRoom zoneAndRoom in scp079Interaction.interactable.currentZonesAndRooms)
                            {
                                if (zoneAndRoom.currentZone == otherRoom.currentZone && zoneAndRoom.currentRoom == otherRoom.currentRoom)
                                {
                                    flag5 = true;
                                }
                            }
                        }
                        if (flag5)
                        {
                            scp079PlayerScript.RpcGainExp(ExpGainType.KillAssist, characterClassManager.CurClass);
                        }
                    }
                    if (RoundSummary.RoundInProgress() && RoundSummary.roundTime < 60)
                    {
                        __instance.TargetAchieve(characterClassManager.connectionToClient, "wowreally");
                    }
                    if (__instance.isLocalPlayer && info.PlayerId != referenceHub.queryProcessor.PlayerId)
                    {
                        RoundSummary.Kills++;
                    }
                    flag = true;
                    if (characterClassManager.CurClass == RoleType.Scp096)
                    {
                        ReferenceHub hub = ReferenceHub.GetHub(go);
                        if (hub != null && hub.scpsController.CurrentScp is Scp096 && (hub.scpsController.CurrentScp as Scp096).PlayerState == Scp096PlayerState.Enraging)
                        {
                            __instance.TargetAchieve(characterClassManager.connectionToClient, "unvoluntaryragequit");
                        }
                    }
                    else if (info.GetDamageType() == DamageTypes.Pocket)
                    {
                        __instance.TargetAchieve(characterClassManager.connectionToClient, "newb");
                    }
                    else if (info.GetDamageType() == DamageTypes.Scp173)
                    {
                        __instance.TargetAchieve(characterClassManager.connectionToClient, "firsttime");
                    }
                    else if (info.GetDamageType() == DamageTypes.Grenade && info.PlayerId == referenceHub.queryProcessor.PlayerId)
                    {
                        __instance.TargetAchieve(characterClassManager.connectionToClient, "iwanttobearocket");
                    }
                    else if (info.GetDamageType().isWeapon)
                    {
                        Inventory inventory = referenceHub.inventory;
                        if (characterClassManager.CurClass == RoleType.Scientist)
                        {
                            Item itemByID = inventory.GetItemByID(inventory.curItem);
                            if (itemByID != null && itemByID.itemCategory == ItemCategory.Keycard && __instance.GetComponent<CharacterClassManager>().CurClass == RoleType.ClassD)
                            {
                                __instance.TargetAchieve(__instance.connectionToClient, "betrayal");
                            }
                        }
                        if (Time.realtimeSinceStartup - __instance._killStreakTime > 30f || __instance._killStreak == 0)
                        {
                            __instance._killStreak = 0;
                            __instance._killStreakTime = Time.realtimeSinceStartup;
                        }
                        if (__instance.GetComponent<WeaponManager>().GetShootPermission(characterClassManager, true))
                        {
                            __instance._killStreak++;
                        }
                        if (__instance._killStreak > 5)
                        {
                            __instance.TargetAchieve(__instance.connectionToClient, "pewpew");
                        }
                        if ((__instance.ccm.CurRole.team == Team.MTF || __instance.ccm.Classes.SafeGet(__instance.ccm.CurClass).team == Team.RSC) && characterClassManager.CurClass == RoleType.ClassD)
                        {
                            __instance.TargetStats(__instance.connectionToClient, "dboys_killed", "justresources", 50);
                        }
                    }
                    else if (__instance.ccm.CurRole.team == Team.SCP && go.GetComponent<MicroHID>().CurrentHidState != MicroHID.MicroHidState.Idle)
                    {
                        __instance.TargetAchieve(__instance.connectionToClient, "illpassthanks");
                    }
                    if (__instance.ccm.CurRole.team == Team.RSC && __instance.ccm.Classes.SafeGet(characterClassManager.CurClass).team == Team.SCP)
                    {
                        __instance.TargetAchieve(__instance.connectionToClient, "timetodoitmyself");
                    }
                    bool flag6 = info.IsPlayer && referenceHub == info.RHub;
                    flag2 = flag4;
                    Environment.OnPlayerDie(attacker, target, info, true, out info, out bool allow2);
                    if (!allow2)
                        return false;
                    if (flag6)
                    {
                        ServerLogs.AddLog(ServerLogs.Modules.ClassChange, string.Concat(new string[]
                        {
                            referenceHub.LoggedNameFromRefHub(),
                            " playing as ",
                            referenceHub.characterClassManager.CurRole.fullName,
                            " committed a suicide using ",
                            info.GetDamageName(),
                            "."
                        }), ServerLogs.ServerLogType.Suicide, false);
                    }
                    else
                    {
                        ServerLogs.AddLog(ServerLogs.Modules.ClassChange, string.Concat(new string[]
                        {
                            referenceHub.LoggedNameFromRefHub(),
                            " playing as ",
                            referenceHub.characterClassManager.CurRole.fullName,
                            " has been killed by ",
                            info.Attacker,
                            " using ",
                            info.GetDamageName(),
                            info.IsPlayer ? (" playing as " + info.RHub.characterClassManager.CurRole.fullName + ".") : "."
                        }), flag2 ? ServerLogs.ServerLogType.Teamkill : ServerLogs.ServerLogType.KillLog, false);
                    }
                    if (info.GetDamageType().isScp || info.GetDamageType() == DamageTypes.Pocket)
                    {
                        RoundSummary.kills_by_scp++;
                    }
                    else if (info.GetDamageType() == DamageTypes.Grenade)
                    {
                        RoundSummary.kills_by_frag++;
                    }
                    if (!__instance._pocketCleanup || info.GetDamageType() != DamageTypes.Pocket)
                    {
                        referenceHub.inventory.ServerDropAll();
                        PlayerMovementSync playerMovementSync = referenceHub.playerMovementSync;
                        if (characterClassManager.Classes.CheckBounds(characterClassManager.CurClass) && info.GetDamageType() != DamageTypes.RagdollLess)
                        {
                            __instance.GetComponent<RagdollManager>().SpawnRagdoll(go.transform.position, go.transform.rotation, (playerMovementSync == null) ? Vector3.zero : playerMovementSync.PlayerVelocity, (int)characterClassManager.CurClass, info, characterClassManager.CurRole.team > Team.SCP, go.GetComponent<MirrorIgnorancePlayer>().PlayerId, referenceHub.nicknameSync.DisplayName, referenceHub.queryProcessor.PlayerId);
                        }
                    }
                    else
                    {
                        referenceHub.inventory.Clear();
                    }
                    characterClassManager.NetworkDeathPosition = go.transform.position;
                    if (characterClassManager.CurRole.team == Team.SCP)
                    {
                        if (characterClassManager.CurClass == RoleType.Scp0492)
                        {
                            NineTailedFoxAnnouncer.CheckForZombies(go);
                        }
                        else
                        {
                            GameObject x = null;
                            foreach (GameObject gameObject in PlayerManager.players)
                            {
                                if (gameObject.GetComponent<QueryProcessor>().PlayerId == info.PlayerId)
                                {
                                    x = gameObject;
                                }
                            }
                            if (x != null)
                            {
                                NineTailedFoxAnnouncer.AnnounceScpTermination(characterClassManager.CurRole, info, string.Empty);
                            }
                            else
                            {
                                DamageTypes.DamageType damageType = info.GetDamageType();
                                if (damageType == DamageTypes.Tesla)
                                {
                                    NineTailedFoxAnnouncer.AnnounceScpTermination(characterClassManager.CurRole, info, "TESLA");
                                }
                                else if (damageType == DamageTypes.Nuke)
                                {
                                    NineTailedFoxAnnouncer.AnnounceScpTermination(characterClassManager.CurRole, info, "WARHEAD");
                                }
                                else if (damageType == DamageTypes.Decont)
                                {
                                    NineTailedFoxAnnouncer.AnnounceScpTermination(characterClassManager.CurRole, info, "DECONTAMINATION");
                                }
                                else if (characterClassManager.CurClass != RoleType.Scp079)
                                {
                                    NineTailedFoxAnnouncer.AnnounceScpTermination(characterClassManager.CurRole, info, "UNKNOWN");
                                }
                            }
                        }
                    }
                    playerStats.SetHPAmount(100);
                    characterClassManager.SetClassID(RoleType.Spectator);
                }
                else
                {
                    Vector3 pos = Vector3.zero;
                    float num3 = 40f;
                    if (info.GetDamageType().isWeapon)
                    {
                        GameObject playerOfID = __instance.GetPlayerOfID(info.PlayerId);
                        if (playerOfID != null)
                        {
                            pos = go.transform.InverseTransformPoint(playerOfID.transform.position).normalized;
                            num3 = 100f;
                        }
                    }
                    else if (info.GetDamageType() == DamageTypes.Pocket)
                    {
                        Environment.OnPocketHurt(go, info.Amount, true, out info.Amount, out bool allowDamage);
                        if (!allowDamage)
                            return false;
                        PlayerMovementSync component2 = __instance.ccm.GetComponent<PlayerMovementSync>();
                        if (component2.RealModelPosition.y > -1900f)
                        {
                            component2.OverridePosition(Vector3.down * 1998.5f, 0f, true);
                        }
                    }
                    __instance.TargetBloodEffect(go.GetComponent<NetworkIdentity>().connectionToClient, pos, Mathf.Clamp01(info.Amount / num3));
                }

                RespawnTickets singleton = RespawnTickets.Singleton;
                Team team = characterClassManager.CurRole.team;
                byte b = (byte)team;
                if (b != 0)
                {
                    if (b == 3)
                    {
                        if (flag)
                        {
                            Team team2 = __instance.ccm.Classes.SafeGet(characterClassManager.CurClass).team;
                            if (team2 == Team.CDP && team2 == Team.CHI)
                            {
                                singleton.GrantTickets(SpawnableTeamType.ChaosInsurgency, __instance._respawn_tickets_ci_scientist_died_count, false);
                            }
                        }
                    }
                }
                else if (characterClassManager.CurClass != RoleType.Scp0492)
                {
                    for (float num4 = 1f; num4 > 0f; num4 -= __instance._respawn_tickets_mtf_scp_hurt_interval)
                    {
                        float num5 = (float)playerStats.maxHP * num4;
                        if (health > num5 && playerStats.Health < num5)
                        {
                            singleton.GrantTickets(SpawnableTeamType.NineTailedFox, __instance._respawn_tickets_mtf_scp_hurt_count, false);
                        }
                    }
                }
                IDamagable damagable2;
                if (component != null && (damagable2 = (component.CurrentScp as IDamagable)) != null)
                {
                    damagable2.OnDamage(info);
                }
                if (!flag4 || FriendlyFireConfig.PauseDetector || PermissionsHandler.IsPermitted(info.RHub.serverRoles.Permissions, PlayerPermissions.FriendlyFireDetectorImmunity))
                {
                    return flag;
                }
                if (FriendlyFireConfig.IgnoreClassDTeamkills && referenceHub.characterClassManager.CurRole.team == Team.CDP && info.RHub.characterClassManager.CurRole.team == Team.CDP)
                {
                    return flag;
                }
                if (flag2)
                {
                    if (info.RHub.FriendlyFireHandler.Respawn.RegisterKill())
                    {
                        return flag;
                    }
                    if (info.RHub.FriendlyFireHandler.Window.RegisterKill())
                    {
                        return flag;
                    }
                    if (info.RHub.FriendlyFireHandler.Life.RegisterKill())
                    {
                        return flag;
                    }
                    if (info.RHub.FriendlyFireHandler.Round.RegisterKill())
                    {
                        return flag;
                    }
                }
                if (info.RHub.FriendlyFireHandler.Respawn.RegisterDamage(info.Amount))
                {
                    return flag;
                }
                if (info.RHub.FriendlyFireHandler.Window.RegisterDamage(info.Amount))
                {
                    return flag;
                }
                if (info.RHub.FriendlyFireHandler.Life.RegisterDamage(info.Amount))
                {
                    return flag;
                }
                info.RHub.FriendlyFireHandler.Round.RegisterDamage(info.Amount);
                return flag;
            }
            catch (Exception e)
            {
                Log.Add("PlayerStats", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.AllowContain))]
    public static class FemurEnterPatch
    {
        public static bool Prefix(CharacterClassManager __instance)
        {
            try
            {
                if (!NetworkServer.active)
                {
                    return false;
                }

                if (!NonFacilityCompatibility.currentSceneSettings.enableStandardGamplayItems)
                {
                    return false;
                }

                foreach (ReferenceHub referenceHub in ReferenceHub.GetAllHubs().Values)
                {
                    if (!referenceHub.isDedicatedServer && referenceHub.isReady && Vector3.Distance(referenceHub.transform.position, __instance._lureSpj.transform.position) < 1.97f)
                    {
                        CharacterClassManager characterClassManager = referenceHub.characterClassManager;
                        PlayerStats playerStats = referenceHub.playerStats;
                        if (characterClassManager.CurRole.team != Team.SCP && characterClassManager.CurClass != RoleType.Spectator && !characterClassManager.GodMode)
                        {
                            Environment.OnFemurEnter(referenceHub.gameObject, true, out bool allow);
                            if (!allow)
                                return false;
                            playerStats.HurtPlayer(new PlayerStats.HitInfo(10000f, "WORLD", DamageTypes.Lure, 0), referenceHub.gameObject, true);
                            __instance._lureSpj.SetState(true);
                        }
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Add("CharacterClassManager", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Scp106PlayerScript), nameof(Scp106PlayerScript.CallCmdMovePlayer))]
    public static class PocketEnterPatch
    {
        public static bool Prefix(Scp106PlayerScript __instance, GameObject ply, int t)
        {
            try
            {
                if (!__instance._iawRateLimit.CanExecute(true))
                {
                    return false;
                }

                if (ply == null)
                {
                    return false;
                }

                ReferenceHub referenceHub = ReferenceHub.GetHub(ply);
                CharacterClassManager characterClassManager = referenceHub.characterClassManager;
                if (characterClassManager == null || characterClassManager.GodMode || characterClassManager.IsAnyScp())
                {
                    return false;
                }
                if (!ServerTime.CheckSynchronization(t) || !__instance.iAm106 || Vector3.Distance(__instance.hub.playerMovementSync.RealModelPosition, ply.transform.position) >= 3f || !characterClassManager.IsHuman())
                {
                    return false;
                }
                __instance.hub.characterClassManager.RpcPlaceBlood(ply.transform.position, 1, 2f);
                __instance.TargetHitMarker(__instance.connectionToClient);
                if (Scp106PlayerScript._blastDoor.isClosed)
                {
                    __instance.hub.characterClassManager.RpcPlaceBlood(ply.transform.position, 1, 2f);
                    __instance.hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(500f, __instance.hub.LoggedNameFromRefHub(), DamageTypes.Scp106, __instance.GetComponent<QueryProcessor>().PlayerId), ply, false);
                }
                else
                {
                    Environment.OnPocketEnter(ply, true, true, out bool hurt, out bool allow);
                    if (!allow)
                        return false;
                    if (hurt)
                        __instance.hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(PluginManager.Config.GetFloat("scp106_pocket_enter_damage", 40f), __instance.hub.LoggedNameFromRefHub(), DamageTypes.Scp106, __instance.GetComponent<QueryProcessor>().PlayerId), ply, false);
                    referenceHub.playerMovementSync.OverridePosition(Vector3.down * 1998.5f, 0f, true);
                    foreach (Scp079PlayerScript scp079PlayerScript in Scp079PlayerScript.instances)
                    {
                        Scp079Interactable.ZoneAndRoom otherRoom = ply.GetComponent<Scp079PlayerScript>().GetOtherRoom();
                        Scp079Interactable.InteractableType[] filter = new Scp079Interactable.InteractableType[]
                        {
                            Scp079Interactable.InteractableType.Door,
                            Scp079Interactable.InteractableType.Light,
                            Scp079Interactable.InteractableType.Lockdown,
                            Scp079Interactable.InteractableType.Tesla,
                            Scp079Interactable.InteractableType.ElevatorUse
                        };
                        bool flag = false;
                        foreach (Scp079Interaction scp079Interaction in scp079PlayerScript.ReturnRecentHistory(12f, filter))
                        {
                            foreach (Scp079Interactable.ZoneAndRoom zoneAndRoom in scp079Interaction.interactable.currentZonesAndRooms)
                            {
                                if (zoneAndRoom.currentZone == otherRoom.currentZone && zoneAndRoom.currentRoom == otherRoom.currentRoom)
                                {
                                    flag = true;
                                }
                            }
                        }
                        if (flag)
                        {
                            scp079PlayerScript.RpcGainExp(ExpGainType.PocketAssist, characterClassManager.CurClass);
                        }
                    }
                }
                PlayerEffectsController playerEffectsController = referenceHub.playerEffectsController;
                playerEffectsController.GetEffect<Corroding>().IsInPd = true;
                playerEffectsController.EnableEffect<Corroding>(0f, false);
                return false;
            }
            catch (Exception e)
            {
                Log.Add("Scp106PlayerScript", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(PocketDimensionTeleport), nameof(PocketDimensionTeleport.OnTriggerEnter))]
    public static class PocketEscapePatch
    {
        public static bool Prefix(PocketDimensionTeleport __instance, Collider other)
        {
            try
            {
                if (!NetworkServer.active)
                {
                    return false;
                }

                NetworkIdentity component = other.GetComponent<NetworkIdentity>();
                if (component != null)
                {
                    if (__instance.type == PocketDimensionTeleport.PDTeleportType.Killer || BlastDoor.OneDoor.isClosed)
                    {
                        component.GetComponent<PlayerStats>().HurtPlayer(new PlayerStats.HitInfo(999990f, "WORLD", DamageTypes.Pocket, 0), other.gameObject, true);
                    }
                    else if (__instance.type == PocketDimensionTeleport.PDTeleportType.Exit)
                    {
                        __instance.tpPositions.Clear();
                        bool flag = false;
                        DecontaminationController.DecontaminationPhase[] decontaminationPhases = DecontaminationController.Singleton.DecontaminationPhases;
                        if (DecontaminationController.GetServerTime > (double)decontaminationPhases[decontaminationPhases.Length - 2].TimeTrigger)
                        {
                            flag = true;
                        }
                        List<string> stringList = ConfigFile.ServerConfig.GetStringList(flag ? "pd_random_exit_rids_after_decontamination" : "pd_random_exit_rids");
                        if (stringList.Count > 0)
                        {
                            foreach (GameObject gameObject in GameObject.FindGameObjectsWithTag("RoomID"))
                            {
                                Rid component2 = gameObject.GetComponent<Rid>();
                                if (component2 != null && stringList.Contains(component2.id, StringComparison.Ordinal))
                                {
                                    __instance.tpPositions.Add(gameObject.transform.position);
                                }
                            }
                            if (stringList.Contains("PORTAL"))
                            {
                                foreach (Scp106PlayerScript scp106PlayerScript in UnityEngine.Object.FindObjectsOfType<Scp106PlayerScript>())
                                {
                                    if (scp106PlayerScript.portalPosition != Vector3.zero)
                                    {
                                        __instance.tpPositions.Add(scp106PlayerScript.portalPosition);
                                    }
                                }
                            }
                        }
                        if (__instance.tpPositions == null || __instance.tpPositions.Count == 0)
                        {
                            foreach (GameObject gameObject2 in GameObject.FindGameObjectsWithTag("PD_EXIT"))
                            {
                                __instance.tpPositions.Add(gameObject2.transform.position);
                            }
                        }
                        Vector3 pos = __instance.tpPositions[UnityEngine.Random.Range(0, __instance.tpPositions.Count)];
                        pos.y += 2f;
                        PlayerMovementSync component3 = other.GetComponent<PlayerMovementSync>();
                        Environment.OnPocketEscape(component3.gameObject, pos, true, out pos, out bool allow);
                        if (!allow)
                            return false;
                        component3.AddSafeTime(2f);
                        component3.OverridePosition(pos, 0f, false);
                        __instance.RemoveCorrosionEffect(other.gameObject);
                        PlayerManager.localPlayer.GetComponent<PlayerStats>().TargetAchieve(component.connectionToClient, "larryisyourfriend");
                    }

                    if (PocketDimensionTeleport.RefreshExit)
                    {
                        ImageGenerator.pocketDimensionGenerator.GenerateRandom();
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Add("PocketDimensionTeleport", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Handcuffs), nameof(Handcuffs.CallCmdCuffTarget))]
    public static class CuffTargetPatch
    {
        public static bool Prefix(Handcuffs __instance, GameObject target)
        {
            try
            {
                if (!__instance._interactRateLimit.CanExecute(true))
                {
                    return false;
                }

                if (target == null || Vector3.Distance(target.transform.position, __instance.transform.position) > __instance.raycastDistance * 1.1f)
                {
                    return false;
                }

                Handcuffs handcuffs = ReferenceHub.GetHub(target).handcuffs;
                if (handcuffs == null || __instance.MyReferenceHub.inventory.curItem != ItemType.Disarmer || __instance.MyReferenceHub.characterClassManager.CurClass < RoleType.Scp173)
                {
                    return false;
                }

                if (handcuffs.CufferId < 0 && handcuffs.MyReferenceHub.inventory.curItem == ItemType.None)
                {
                    Team team = __instance.MyReferenceHub.characterClassManager.CurRole.team;
                    Team team2 = handcuffs.MyReferenceHub.characterClassManager.CurRole.team;
                    bool flag = false;

                    if (team == Team.CDP)
                    {
                        if (team2 == Team.MTF || team2 == Team.RSC)
                        {
                            flag = true;
                        }
                    }
                    else if (team == Team.RSC)
                    {
                        if (team2 == Team.CHI || team2 == Team.CDP)
                        {
                            flag = true;
                        }
                    }
                    else if (team == Team.CHI)
                    {
                        if (team2 == Team.MTF || team2 == Team.RSC)
                        {
                            flag = true;
                        }

                        if (team2 == Team.CDP && ConfigFile.ServerConfig.GetBool("ci_can_cuff_class_d", false))
                        {
                            flag = true;
                        }
                    }
                    else if (team == Team.MTF)
                    {
                        if (team2 == Team.CHI || team2 == Team.CDP)
                        {
                            flag = true;
                        }

                        if (team2 == Team.RSC && ConfigFile.ServerConfig.GetBool("mtf_can_cuff_researchers", false))
                        {
                            flag = true;
                        }
                    }
                    if (flag)
                    {
                        if (team2 == Team.MTF && team == Team.CDP)
                        {
                            __instance.MyReferenceHub.playerStats.TargetAchieve(__instance.MyReferenceHub.playerStats.connectionToClient, "tableshaveturned");
                        }
                        __instance.ClearTarget();
                        handcuffs.NetworkCufferId = __instance.MyReferenceHub.queryProcessor.PlayerId;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Add("Handcuffs", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Generator079), nameof(Generator079.Interact))]
    public static class GeneratorPatches
    {
        public static bool Prefix(Generator079 __instance, GameObject person, PlayerInteract.Generator079Operations command)
        {
            try
            {
                switch (command)
                {
                    case PlayerInteract.Generator079Operations.Door:
                        __instance.OpenClose(person);
                        return false;
                    case PlayerInteract.Generator079Operations.Tablet:
                        {
                            if (__instance.isTabletConnected || !__instance.isDoorOpen || __instance._localTime <= 0f || Generator079.mainGenerator.forcedOvercharge)
                                return false;
                            Inventory component = person.GetComponent<Inventory>();
                            foreach (Inventory.SyncItemInfo item in component.items)
                            {
                                if (item.id == ItemType.WeaponManagerTablet)
                                {
                                    Environment.OnGeneratorInsert(__instance, person, true, out bool allow);
                                    if (!allow)
                                        return false;
                                    component.items.Remove(item);
                                    __instance.NetworkisTabletConnected = true;
                                }
                            }
                            return false;
                        }
                    case PlayerInteract.Generator079Operations.Cancel:
                        break;
                    default:
                        return false;
                }
                Environment.OnGeneratorEject(__instance, person, true, out bool allow2);
                if (!allow2)
                    return false;
                __instance.EjectTablet();
                return false;
            }
            catch (Exception e)
            {
                Log.Add("Generator079", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Generator079), nameof(Generator079.OpenClose))]
    public static class GeneratorDoorPatches
    {
        public static bool Prefix(Generator079 __instance, GameObject person)
        {
            Inventory component = person.GetComponent<Inventory>();
            if (component == null || __instance._doorAnimationCooldown > 0f || __instance._deniedCooldown > 0f)
            {
                return false;
            }

            try
            {
                if (__instance.isDoorUnlocked)
                {
                    bool allow = true;
                    if (__instance.NetworkisDoorOpen)
                        Environment.OnGeneratorClose(__instance, person, true, out allow);
                    else
                        Environment.OnGeneratorOpen(__instance, person, true, out allow);
                    if (!allow)
                        return false;
                    __instance._doorAnimationCooldown = 1.5f;
                    __instance.NetworkisDoorOpen = !__instance.isDoorOpen;
                    __instance.RpcDoSound(__instance.isDoorOpen);
                    return false;
                }

                bool flag = person.GetComponent<ServerRoles>().BypassMode;
                if (component.curItem > ItemType.KeycardJanitor)
                {
                    string[] permissions = component.GetItemByID(component.curItem).permissions;
                    for (int i = 0; i < permissions.Length; i++)
                    {
                        if (permissions[i] == "ARMORY_LVL_2")
                        {
                            flag = true;
                        }
                    }
                }

                if (flag)
                {
                    Environment.OnGeneratorUnlock(__instance, person, true, out bool allow);
                    if (!allow)
                        return false;
                    __instance.NetworkisDoorUnlocked = true;
                    __instance._doorAnimationCooldown = 0.5f;
                    return false;
                }
                __instance.RpcDenied();
                return false;
            }
            catch (Exception e)
            {
                Log.Add("Generator079", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.OnInteract))]
    public static class InteractPatch
    {
        public static bool Prefix(PlayerInteract __instance)
        {
            try
            {
                Environment.OnInteract(__instance.gameObject, true, out bool allow);
                if (allow && PluginManager.Config.GetBool("disable_scp_268_effects_when_interacted", true))
                    __instance._scp268.ServerDisable();
                return false;
            }
            catch (Exception e)
            {
                Log.Add("PlayerInteract", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdOpenDoor))]
    public static class OpenDoorPatch
    {
        public static bool Prefix(PlayerInteract __instance, GameObject doorId)
        {
            if (!__instance._playerInteractRateLimit.CanExecute() || (__instance._hc.CufferId > 0 && !PlayerInteract.CanDisarmedInteract) || doorId == null || __instance._ccm.CurClass == RoleType.None || __instance._ccm.CurClass == RoleType.Spectator || !doorId.TryGetComponent(out Door component) || ((component.Buttons.Count == 0) ? (!__instance.ChckDis(doorId.transform.position)) : component.Buttons.All((Door.DoorButton item) => !__instance.ChckDis(item.button.transform.position))))
            {
                return false;
            }
            __instance.OnInteract();
            Environment.OnDoorInteract(true, component, __instance.gameObject, out bool allow);
            if (!allow)
                return false;
            if (__instance._sr.BypassMode)
            {
                component.ChangeState(true);
                return false;
            }
            if (component.PermissionLevels.HasPermission(Door.AccessRequirements.Checkpoints) && __instance._ccm.CurRole.team == Team.SCP)
            {
                component.ChangeState();
                return false;
            }
            try
            {
                if (component.PermissionLevels == (Door.AccessRequirements)0)
                {
                    if (!component.locked)
                    {
                        component.ChangeState();
                    }
                }
                else if (!component.RequireAllPermissions)
                {
                    string[] permissions = __instance._inv.GetItemByID(__instance._inv.curItem).permissions;
                    foreach (string key in permissions)
                    {
                        if (Door.backwardsCompatPermissions.TryGetValue(key, out Door.AccessRequirements value) && component.PermissionLevels.HasPermission(value))
                        {
                            if (!component.locked)
                            {
                                component.ChangeState();
                            }
                            return false;
                        }
                    }
                    __instance.RpcDenied(doorId);
                }
                else
                {
                   __instance.RpcDenied(doorId);
                }
            }
            catch
            {
                __instance.RpcDenied(doorId);
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdUseElevator))]
    public static class ElevatorInteractPatch
    {
        public static bool Prefix(PlayerInteract __instance, GameObject elevator)
        {
            if (!__instance._playerInteractRateLimit.CanExecute(true) || (__instance._hc.CufferId > 0 && !PlayerInteract.CanDisarmedInteract))
            {
                return false;
            }

            if (elevator == null)
            {
                return false;
            }

            Lift component = elevator.GetComponent<Lift>();
            if (component == null)
            {
                return false;
            }

            try
            {
                foreach (Lift.Elevator elevator2 in component.elevators)
                {
                    if (__instance.ChckDis(elevator2.door.transform.position))
                    {
                        Environment.OnElevatorInteract(component, __instance.gameObject, true, out bool allow);
                        if (!allow)
                            return false;
                        elevator.GetComponent<Lift>().UseLift();
                        __instance.OnInteract();
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Add("PlayerInteract", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdUseLocker))]
    public static class LockerInteractPatch
    {
        public static bool Prefix(PlayerInteract __instance, byte lockerId, byte chamberNumber)
        {
            if (!__instance._playerInteractRateLimit.CanExecute(true) || (__instance._hc.CufferId > 0 && !PlayerInteract.CanDisarmedInteract))
            {
                return false;
            }
            LockerManager singleton = LockerManager.singleton;
            if ((int)lockerId >= singleton.lockers.Length)
            {
                return false;
            }
            if (!__instance.ChckDis(singleton.lockers[(int)lockerId].gameObject.position) || !singleton.lockers[(int)lockerId].supportsStandarizedAnimation)
            {
                return false;
            }
            if ((int)chamberNumber >= singleton.lockers[(int)lockerId].chambers.Length)
            {
                return false;
            }
            if (singleton.lockers[(int)lockerId].chambers[(int)chamberNumber].doorAnimator == null)
            {
                return false;
            }
            if (!singleton.lockers[(int)lockerId].chambers[(int)chamberNumber].CooldownAtZero())
            {
                return false;
            }

            try
            {
                singleton.lockers[(int)lockerId].chambers[(int)chamberNumber].SetCooldown();
                string accessToken = singleton.lockers[(int)lockerId].chambers[(int)chamberNumber].accessToken;
                Item itemByID = __instance._inv.GetItemByID(__instance._inv.curItem);
                Environment.OnLockerInteract(singleton.lockers[(int)lockerId], __instance.gameObject, __instance._sr.BypassMode || string.IsNullOrEmpty(accessToken) || (itemByID != null && itemByID.permissions.Contains(accessToken)), out bool allow);
                if (allow)
                {
                    bool flag = ((int)singleton.openLockers[(int)lockerId] & 1 << (int)chamberNumber) != 1 << (int)chamberNumber;
                    singleton.ModifyOpen((int)lockerId, (int)chamberNumber, flag);
                    singleton.RpcDoSound((int)lockerId, (int)chamberNumber, flag);
                    bool anyOpen = singleton.lockers[(int)lockerId].AnyVirtual;
                    if (singleton.lockers[(int)lockerId].AnyVirtual)
                    {
                        for (int i = 0; i < singleton.lockers[(int)lockerId].chambers.Length; i++)
                        {
                            if (((int)singleton.openLockers[(int)lockerId] & 1 << i) == 1 << i)
                            {
                                anyOpen = false;
                                break;
                            }
                        }
                    }
                    singleton.lockers[(int)lockerId].LockPickups(!flag, (uint)chamberNumber, anyOpen);
                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        singleton.RpcChangeMaterial((int)lockerId, (int)chamberNumber, false);
                    }
                }
                else
                {
                    singleton.RpcChangeMaterial((int)lockerId, (int)chamberNumber, true);
                }
                __instance.OnInteract();
                return false;
            }
            catch (Exception e)
            {
                Log.Add("PlayerInteract", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(NicknameSync), nameof(NicknameSync.SetNick))]
    public static class PlayerJoinPatch
    {
        public static bool Prefix(NicknameSync __instance, string nick)
        {
            try
            {
                if (!NetworkServer.active)
                {
                    Debug.LogWarning("[Server] function 'System.Void NicknameSync::SetNick(System.String)' called on client");
                    return false;
                }
                __instance.MyNick = nick;
                if (__instance.isLocalPlayer && ServerStatic.IsDedicated || __instance == null || string.IsNullOrEmpty(nick))
                    return false;
                if (!Server.PlayerList.PlayersDict.TryGetValue(__instance.hub.gameObject, out Player player))
                {
                    player = new Player(ReferenceHub.GetHub(__instance.gameObject));
                    Server.PlayerList.Add(player);
                }
                if (ServerGuard.SteamShield.CheckAccount(__instance.hub.GetPlayer()))
                    return false;
                if (ServerGuard.VPNShield.CheckIP(__instance.hub.GetPlayer()))
                    return false;
                Environment.OnPlayerJoin(__instance.hub.gameObject);
                ServerConsole.AddLog(string.Concat(new string[]
                {
                    "Nickname of ",
                    __instance.hub.characterClassManager.UserId,
                    " is now ",
                    nick,
                    "."
                }), ConsoleColor.Gray);

                ServerLogs.AddLog(ServerLogs.Modules.Networking, string.Concat(new string[]
                {
                    "Nickname of ",
                    __instance.hub.characterClassManager.UserId,
                    " is now ",
                    nick,
                    "."
                }), ServerLogs.ServerLogType.ConnectionUpdate, false);

                Timing.CallDelayed(0.25f, () =>
                {
                    if (__instance.hub != null && __instance.hub.characterClassManager.NetworkMuted)
                        __instance.hub.characterClassManager.SetDirtyBit(1UL);
                });

                return false;
            }
            catch (Exception e)
            {
                Log.Add("NicknameSync", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(ReferenceHub), nameof(ReferenceHub.OnDestroy))]
    public static class PlayerLeavePatch
    {
        public static bool Prefix(ReferenceHub __instance)
        {
            try
            {
                Environment.OnPlayerLeave(__instance.gameObject, out bool destroy);
                if (!destroy)
                    return false;
                Server.PlayerList.Remove(__instance.gameObject.GetPlayer());
                ReferenceHub.Hubs.Remove(__instance.gameObject);
                ReferenceHub.HubIds.Remove(__instance.queryProcessor.PlayerId);

                if (ReferenceHub._hostHub == __instance)
                {
                    ReferenceHub._hostHub = null;
                }

                if (ReferenceHub._localHub == __instance)
                {
                    ReferenceHub._localHub = null;
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Add("ReferenceHub", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Handcuffs), nameof(Handcuffs.ClearTarget))]
    public static class UncuffPatch
    {
        public static bool Prefix(Handcuffs __instance)
        {
            try
            {
                if (!NetworkServer.active)
                {
                    Debug.LogWarning("[Server] function 'System.Void Handcuffs::ClearTarget()' called on client");
                    return false;
                }

                foreach (GameObject player in PlayerManager.players)
                {
                    Handcuffs handcuffs = ReferenceHub.GetHub(player).handcuffs;
                    if (handcuffs.CufferId == __instance.MyReferenceHub.queryProcessor.PlayerId)
                    {
                        Environment.OnUncuff(handcuffs.MyReferenceHub.gameObject, __instance.gameObject, true, out bool allow);
                        if (allow)
                            handcuffs.NetworkCufferId = -1;
                        break;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Add("Handcuffs", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Handcuffs), nameof(Handcuffs.CallCmdFreeTeammate))]
    public static class UncuffTeammatePatch
    {
        public static bool Prefix(Handcuffs __instance, GameObject target)
        {
            try
            {
                if (!__instance._interactRateLimit.CanExecute(true))
                {
                    return false;
                }

                if (target == null || Vector3.Distance(target.transform.position, __instance.transform.position) > __instance.raycastDistance * 1.1f)
                {
                    return false;
                }

                if (__instance.MyReferenceHub.characterClassManager.CurRole.team == Team.SCP)
                {
                    return false;
                }
                Environment.OnUncuff(target, __instance.gameObject, true, out bool allow);
                if (allow)
                    ReferenceHub.GetHub(target).handcuffs.NetworkCufferId = -1;
                return false;
            }
            catch (Exception e)
            {
                Log.Add("Handcuffs", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(WeaponManager), nameof(WeaponManager.CallCmdShoot))]
    public static class WeaponShootPatch
    {
        public static bool Prefix(WeaponManager __instance, GameObject target, string hitboxType, Vector3 dir, Vector3 sourcePos, Vector3 targetPos)
        {
            try
            {
                if (!__instance._iawRateLimit.CanExecute(true))
                {
                    return false;
                }
                int itemIndex = __instance._hub.inventory.GetItemIndex();
                if (itemIndex < 0 || itemIndex >= __instance._hub.inventory.items.Count)
                {
                    return false;
                }
                if (__instance.curWeapon < 0 || ((__instance._reloadCooldown > 0f || __instance._fireCooldown > 0f) && !__instance.isLocalPlayer))
                {
                    return false;
                }
                if (__instance._hub.inventory.curItem != __instance.weapons[(int)__instance.curWeapon].inventoryID)
                {
                    return false;
                }
                if (__instance._hub.inventory.items[itemIndex].durability <= 0f)
                {
                    return false;
                }
                Environment.OnShoot(__instance._hub.gameObject, target, __instance._hub.inventory.curItem.GetWeaponType(), true, out bool allow);
                if (!allow)
                    return false;
                if (Vector3.Distance(__instance._hub.playerMovementSync.RealModelPosition, sourcePos) > 5.5f)
                {
                    __instance.GetComponent<CharacterClassManager>().TargetConsolePrint(__instance.connectionToClient, "Shot rejected - Code W.6 (difference between real source position and provided source position is too big)", "gray");
                    return false;
                }
                if (sourcePos.y - __instance._hub.playerMovementSync.LastSafePosition.y > 1.78f)
                {
                    __instance.GetComponent<CharacterClassManager>().TargetConsolePrint(__instance.connectionToClient, "Shot rejected - Code W.7 (Y axis difference between last safe position and provided source position is too big)", "gray");
                    return false;
                }
                if (Math.Abs(sourcePos.y - __instance._hub.playerMovementSync.RealModelPosition.y) > 2.7f)
                {
                    __instance.GetComponent<CharacterClassManager>().TargetConsolePrint(__instance.connectionToClient, "Shot rejected - Code W.8 (|Y| axis difference between real position and provided source position is too big)", "gray");
                    return false;
                }
                __instance._hub.inventory.items.ModifyDuration(itemIndex, __instance._hub.inventory.items[itemIndex].durability - 1f);
                __instance.scp268.ServerDisable();
                __instance._fireCooldown = 1f / (__instance.weapons[(int)__instance.curWeapon].shotsPerSecond * __instance.weapons[(int)__instance.curWeapon].allEffects.firerateMultiplier) * 0.9f;
                float num = __instance.weapons[(int)__instance.curWeapon].allEffects.audioSourceRangeScale;
                num = num * num * 70f;
                __instance.GetComponent<Scp939_VisionController>().MakeNoise(Mathf.Clamp(num, 5f, 100f));
                bool flag = target != null;
                RaycastHit raycastHit2;
                if (targetPos == Vector3.zero)
                {
                    RaycastHit raycastHit;
                    if (Physics.Raycast(sourcePos, dir, out raycastHit, 500f, __instance.raycastMask))
                    {
                        HitboxIdentity component = raycastHit.collider.GetComponent<HitboxIdentity>();
                        if (component != null)
                        {
                            WeaponManager componentInParent = component.GetComponentInParent<WeaponManager>();
                            if (componentInParent != null)
                            {
                                flag = false;
                                target = componentInParent.gameObject;
                                hitboxType = component.id;
                                targetPos = componentInParent.transform.position;
                            }
                        }
                    }
                }
                else if (Physics.Linecast(sourcePos, targetPos, out raycastHit2, __instance.raycastMask))
                {
                    HitboxIdentity component2 = raycastHit2.collider.GetComponent<HitboxIdentity>();
                    if (component2 != null)
                    {
                        WeaponManager componentInParent2 = component2.GetComponentInParent<WeaponManager>();
                        if (componentInParent2 != null)
                        {
                            if (componentInParent2.gameObject == target)
                            {
                                flag = false;
                            }
                            else if (componentInParent2.scp268.Enabled)
                            {
                                flag = false;
                                target = componentInParent2.gameObject;
                                hitboxType = component2.id;
                                targetPos = componentInParent2.transform.position;
                            }
                        }
                    }
                }
                ReferenceHub referenceHub = null;
                if (target != null)
                {
                    referenceHub = ReferenceHub.GetHub(target);
                }
                if (referenceHub != null && __instance.GetShootPermission(referenceHub.characterClassManager, false))
                {
                    if (Math.Abs(__instance._hub.playerMovementSync.RealModelPosition.y - referenceHub.playerMovementSync.RealModelPosition.y) > 35f)
                    {
                        __instance.GetComponent<CharacterClassManager>().TargetConsolePrint(__instance.connectionToClient, "Shot rejected - Code W.1 (too big Y-axis difference between source and target)", "gray");
                        return false;
                    }
                    if (Vector3.Distance(referenceHub.playerMovementSync.RealModelPosition, targetPos) > 5f)
                    {
                        __instance.GetComponent<CharacterClassManager>().TargetConsolePrint(__instance.connectionToClient, "Shot rejected - Code W.2 (difference between real target position and provided target position is too big)", "gray");
                        return false;
                    }
                    if (Physics.Linecast(__instance._hub.playerMovementSync.RealModelPosition, sourcePos, __instance.raycastServerMask))
                    {
                        __instance.GetComponent<CharacterClassManager>().TargetConsolePrint(__instance.connectionToClient, "Shot rejected - Code W.3 (collision between source positions detected)", "gray");
                        return false;
                    }
                    if (flag && Physics.Linecast(sourcePos, targetPos, __instance.raycastServerMask))
                    {
                        __instance.GetComponent<CharacterClassManager>().TargetConsolePrint(__instance.connectionToClient, "Shot rejected - Code W.4 (collision on shot line detected)", "gray");
                        return false;
                    }
                    if (referenceHub.gameObject == __instance.gameObject)
                    {
                        __instance.GetComponent<CharacterClassManager>().TargetConsolePrint(__instance.connectionToClient, "Shot rejected - Code W.5 (target is itself)", "gray");
                        return false;
                    }
                    if (Vector3.Angle(referenceHub.playerMovementSync.RealModelPosition - __instance._hub.playerMovementSync.RealModelPosition, __instance.transform.forward) > 45f)
                    {
                        __instance.GetComponent<CharacterClassManager>().TargetConsolePrint(__instance.connectionToClient, "Shot rejected - Code W.12 (too big angle)", "gray");
                        return false;
                    }
                    if (__instance._lastRotationReset >= 0f && Math.Abs(Quaternion.Angle(__instance._lastRotation, __instance.camera.rotation)) < 0.01f)
                    {
                        __instance._lastRotationReset = 2f;
                        __instance._lastRotation = __instance.camera.rotation;
                        __instance.GetComponent<CharacterClassManager>().TargetConsolePrint(__instance.connectionToClient, "Shot rejected - Code W.9 (no recoil)", "gray");
                        return false;
                    }
                    __instance._lastRotationReset = 0.35f;
                    __instance._lastRotation = __instance.camera.rotation;
                    float num2 = Vector3.Distance(__instance.camera.transform.position, target.transform.position);
                    float num3 = __instance.weapons[(int)__instance.curWeapon].damageOverDistance.Evaluate(num2);
                    RoleType curClass = referenceHub.characterClassManager.CurClass;
                    if (curClass != RoleType.Scp173)
                    {
                        switch (curClass)
                        {
                            case RoleType.Scp106:
                                num3 /= 10f;
                                goto IL_6EF;
                            case RoleType.NtfScientist:
                            case RoleType.Scientist:
                            case RoleType.ChaosInsurgency:
                                break;
                            case RoleType.Scp049:
                            case RoleType.Scp079:
                            case RoleType.Scp096:
                                goto IL_6EF;
                            default:
                                if (curClass - RoleType.Scp93953 <= 1)
                                {
                                    goto IL_6EF;
                                }
                                break;
                        }
                        string a = hitboxType.ToUpper();
                        if (!(a == "HEAD"))
                        {
                            if (a == "LEG")
                            {
                                num3 /= 2f;
                            }
                        }
                        else
                        {
                            num3 *= 4f;
                            float num4 = 1f / (__instance.weapons[(int)__instance.curWeapon].shotsPerSecond * __instance.weapons[(int)__instance.curWeapon].allEffects.firerateMultiplier);
                            __instance._headshotsL += 1U;
                            __instance._headshotsS += 1U;
                            __instance._headshotsResetS = num4 * 1.86f;
                            __instance._headshotsResetL = num4 * 2.9f;
                            if (__instance._headshotsS >= 3U)
                            {
                                __instance._hub.playerMovementSync.AntiCheatKillPlayer("Headshots limit exceeded in time window A\n(debug code: W.10)", "W.10");
                                return false;
                            }
                            if (__instance._headshotsL >= 4U)
                            {
                                __instance._hub.playerMovementSync.AntiCheatKillPlayer("Headshots limit exceeded in time window B\n(debug code: W.11)", "W.11");
                                return false;
                            }
                        }
                    }
                IL_6EF:
                    Environment.OnLateShoot(__instance._hub.gameObject, target, __instance._hub.inventory.curItem.GetWeaponType(), true, out bool allow2);
                    if (!allow2)
                        return false;
                    num3 *= __instance.weapons[(int)__instance.curWeapon].allEffects.damageMultiplier;
                    num3 *= __instance.overallDamagerFactor;
                    __instance._hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(num3, __instance._hub.LoggedNameFromRefHub(), DamageTypes.FromWeaponId((int)__instance.curWeapon), __instance._hub.queryProcessor.PlayerId), referenceHub.gameObject, false);
                    __instance.RpcConfirmShot(true, (int)__instance.curWeapon);
                    __instance.PlaceDecal(true, new Ray(__instance.camera.position, dir), (int)referenceHub.characterClassManager.CurClass, num2);
                    return false;
                }
                else
                {
                    if (target != null && hitboxType == "window" && target.GetComponent<BreakableWindow>() != null)
                    {
                        float time = Vector3.Distance(__instance.camera.transform.position, target.transform.position);
                        float damage = __instance.weapons[(int)__instance.curWeapon].damageOverDistance.Evaluate(time);
                        target.GetComponent<BreakableWindow>().ServerDamageWindow(damage);
                        __instance.RpcConfirmShot(true, (int)__instance.curWeapon);
                        return false;
                    }
                    __instance.PlaceDecal(false, new Ray(__instance.camera.position, dir), (int)__instance.curWeapon, 0f);
                    __instance.RpcConfirmShot(false, (int)__instance.curWeapon);
                    return false;
                }
            }
            catch (Exception e)
            {
                Log.Add("WeaponManager", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.ApplyProperties))]
    public static class SpawnPatch
    {
        public static void Postfix(CharacterClassManager __instance, bool lite = false, bool escape = false)
        {
            try
            {
                Environment.OnSpawn(__instance.gameObject, __instance.transform.position, __instance.CurClass, true, out Vector3 pos, out RoleType role, out bool allow);
            }
            catch (Exception e)
            {
                Log.Add("CharacterClassManager", e);
            }
        }
    }

    [HarmonyPatch(typeof(RagdollManager), nameof(RagdollManager.SpawnRagdoll))]
    public static class SpawnRagdollPatch
    {
        public static bool Prefix(RagdollManager __instance, Vector3 pos, Quaternion rot, Vector3 velocity, int classId, PlayerStats.HitInfo ragdollInfo, bool allowRecall, string ownerID, string ownerNick, int playerId)
        {
            try
            {
                if (!PluginManager.Config.GetBool("spawn_ragdolls", true))
                    return false;
                Role role = __instance.hub.characterClassManager.Classes.SafeGet(classId);
                if (role.model_ragdoll == null)
                {
                    return false;
                }
                GameObject gameObject = UnityEngine.Object.Instantiate(role.model_ragdoll, pos + role.ragdoll_offset.position, Quaternion.Euler(rot.eulerAngles + role.ragdoll_offset.rotation));
                NetworkServer.Spawn(gameObject);
                Ragdoll component = gameObject.GetComponent<Ragdoll>();
                Environment.OnSpawnRagdoll(component, true, out bool allow);
                if (!allow)
                {
                    component.Delete();
                    return false;
                }
                if (__instance.cleanupTime > 0)
                {
                    component.TimeTillCleanup = __instance.cleanupTime;
                }
                component.Networkowner = new Ragdoll.Info(ownerID, ownerNick, ragdollInfo, role, playerId);
                component.NetworkallowRecall = allowRecall;
                component.RpcSyncVelo(velocity);
                return false;
            }
            catch (Exception e)
            {
                Log.Add("RagdollManager", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(ConsumableAndWearableItems), nameof(ConsumableAndWearableItems.CallCmdCancelMedicalItem))]
    public static class CancelMedicalPatch
    {
        public static bool Prefix(ConsumableAndWearableItems __instance)
        {
            try
            {
                if (!__instance._interactRateLimit.CanExecute(true))
                {
                    return false;
                }

                Environment.OnCancelMedical(__instance.cooldown, __instance._hub.gameObject, __instance._hub.inventory.curItem, true, out __instance.cooldown, out bool allow);
                if (!allow)
                    return false;
                foreach (ConsumableAndWearableItems.UsableItem usableItem in __instance.usableItems)
                {
                    if (usableItem.inventoryID == __instance._hub.inventory.curItem && usableItem.cancelableTime > 0f)
                    {
                        __instance._cancel = true;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Add("ConsumableAndWearableItems", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(AnimationController), nameof(AnimationController.CallCmdSyncData))]
    public static class SyncDataPatch
    {
        public static bool Prefix(AnimationController __instance, byte state, Vector2 v2)
        {
            try
            {
                if (!__instance._mSyncRateLimit.CanExecute(true))
                    return false;
                Environment.OnSyncData(__instance.gameObject, v2, state, true, out state, out bool allow);
                if (!allow)
                    return false;
                __instance.NetworkcurAnim = (int)state;
                __instance.Networkspeed = v2;
                return false;
            }
            catch (Exception e)
            {
                Log.Add("AnimationController", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(GrenadeManager), nameof(GrenadeManager.CallCmdThrowGrenade))]
    public static class ThrowGrenadePatch
    {
        public static bool Prefix(GrenadeManager __instance, int id, bool slowThrow, double time)
        {
            try
            {
                if (!__instance._iawRateLimit.CanExecute(true))
                    return false;
                if (id < 0 || __instance.availableGrenades.Length <= id)
                    return false;
                GrenadeSettings grenadeSettings = __instance.availableGrenades[id];
                if (grenadeSettings.inventoryID != __instance.hub.inventory.curItem)
                    return false;
                float delay = Mathf.Clamp((float)(time - NetworkTime.time), 0f, grenadeSettings.throwAnimationDuration);
                float forceMultiplier = slowThrow ? 0.5f : 1f;
                Environment.OnThrowGrenade(__instance.hub.gameObject, grenadeSettings.grenadeInstance.GetComponent<Grenade>(), grenadeSettings.inventoryID.GetGrenadeType(), true, out bool allow);
                if (!allow)
                    return false;
                Timing.RunCoroutine(__instance._ServerThrowGrenade(grenadeSettings, forceMultiplier, __instance.hub.inventory.GetItemIndex(), delay), Segment.FixedUpdate);
                return false;
            }
            catch (Exception e)
            {
                Log.Add("GrenadeManager", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(TeslaGate), nameof(TeslaGate.PlayerInRange))]
    public static class TeslaTriggerPatch
    {
        public static bool GatesDisabled = false;

        public static bool Prefix(TeslaGate __instance, ReferenceHub player)
        {
            try
            {
                if (Vector3.Distance(__instance.transform.position, player.playerMovementSync.RealModelPosition) < __instance.sizeOfTrigger)
                {
                    if (PluginManager.Config.GetRoles("tesla_triggerable_roles").Contains(player.characterClassManager.CurClass) && !GatesDisabled && !player.characterClassManager.GodMode)
                    {
                        Environment.OnTriggerTesla(player.gameObject, __instance, true, out bool allow);
                        return allow;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Add("TeslaGate", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(ConsumableAndWearableItems), nameof(ConsumableAndWearableItems.CallCmdUseMedicalItem))]
    public static class UseMedicalPatch
    {
        public static bool Prefix(ConsumableAndWearableItems __instance)
        {
            try
            {
                if (!__instance._interactRateLimit.CanExecute(true))
                    return false;
                int hp;
                Environment.OnUseMedical(__instance._hub.gameObject, __instance._hub.inventory.curItem, (int)__instance.hpToHeal, true, out hp, out bool allow);
                __instance.hpToHeal = (float)hp;
                if (!allow)
                    return false;
                __instance._cancel = false;
                if (__instance.cooldown > 0f)
                    return false;
                for (int i = 0; i < __instance.usableItems.Length; i++)
                {
                    if (__instance.usableItems[i].inventoryID == __instance._hub.inventory.curItem && __instance.usableCooldowns[i] <= 0f)
                    {
                        Timing.RunCoroutine(__instance.UseMedicalItem(i), Segment.FixedUpdate);
                        return false;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Add("ConsumableAndWeableItems", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Scp049), nameof(Scp049.BodyCmd_ByteAndGameObject))]
    public static class RecallPatch
    {
        public static bool Prefix(Scp049 __instance, byte num, GameObject go)
        {
            try
            {
                if (num == 0)
                {
                    if (!__instance._interactRateLimit.CanExecute(true))
                        return false;
                    if (go == null)
                        return false;
                    if (Vector3.Distance(go.transform.position, __instance.Hub.playerMovementSync.RealModelPosition) >= Scp049.AttackDistance * 1.25f)
                        return false;
                    __instance.Hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(4949f, __instance.Hub.nicknameSync.MyNick + " (" + __instance.Hub.characterClassManager.UserId + ")", DamageTypes.Scp049, __instance.Hub.queryProcessor.PlayerId), go, false);
                    GameCore.Console.AddDebugLog("SCPCTRL", "SCP-049 | Sent 'death time' RPC", MessageImportance.LessImportant, false);
                    __instance.Hub.scpsController.RpcTransmit_Byte(0);
                    return false;
                }
                else
                {
                    if (num != 1)
                    {
                        if (num == 2)
                        {
                            if (!__instance._interactRateLimit.CanExecute(true))
                                return false;
                            if (go == null)
                                return false;
                            Ragdoll component = go.GetComponent<Ragdoll>();

                            if (component == null)
                                return false;
                            ReferenceHub referenceHub = null;

                            foreach (GameObject player in PlayerManager.players)
                            {
                                ReferenceHub hub = ReferenceHub.GetHub(player);
                                if (hub.queryProcessor.PlayerId == component.owner.PlayerId)
                                {
                                    referenceHub = hub;
                                    break;
                                }
                            }
                            if (referenceHub == null)
                            {
                                GameCore.Console.AddDebugLog("SCPCTRL", "SCP-049 | Request 'finish recalling' rejected; no target found", MessageImportance.LessImportant, false);
                                return false;
                            }
                            if (!__instance._recallInProgressServer || referenceHub.gameObject != __instance._recallObjectServer || __instance._recallProgressServer < 0.85f)
                            {
                                GameCore.Console.AddDebugLog("SCPCTRL", "SCP-049 | Request 'finish recalling' rejected; Debug code: ", MessageImportance.LessImportant, false);
                                GameCore.Console.AddDebugLog("SCPCTRL", "SCP-049 | CONDITION#1 " + (__instance._recallInProgressServer ? "<color=green>PASSED</color>" : ("<color=red>ERROR</color> - " + __instance._recallInProgressServer.ToString())), MessageImportance.LessImportant, true);
                                GameCore.Console.AddDebugLog("SCPCTRL", "SCP-049 | CONDITION#2 " + ((referenceHub == __instance._recallObjectServer) ? "<color=green>PASSED</color>" : string.Concat(new object[]
                                {
                                    "<color=red>ERROR</color> - ",
                                    referenceHub.queryProcessor.PlayerId,
                                    "-",
                                    (__instance._recallObjectServer == null) ? "null" : ReferenceHub.GetHub(__instance._recallObjectServer).queryProcessor.PlayerId.ToString()
                                })), MessageImportance.LessImportant, false);
                                GameCore.Console.AddDebugLog("SCPCTRL", "SCP-049 | CONDITION#3 " + ((__instance._recallProgressServer >= 0.85f) ? "<color=green>PASSED</color>" : ("<color=red>ERROR</color> - " + __instance._recallProgressServer)), MessageImportance.LessImportant, true);
                                return false;
                            }
                            if (referenceHub.characterClassManager.CurClass != RoleType.Spectator)
                                return false;
                            if (!PluginManager.Config.GetBool("scp049_can_revive_not_killed_by_049", true) && component.owner.DeathCause.GetDamageName() != "SCP-049")
                                return false;
                            Environment.OnRecall(__instance.Hub.gameObject, component, true, out bool allow);
                            if (!allow)
                                return false;
                            GameCore.Console.AddDebugLog("SCPCTRL", "SCP-049 | Request 'finish recalling' accepted", MessageImportance.LessImportant, false);
                            RoundSummary.changed_into_zombies++;
                            referenceHub.characterClassManager.SetClassID(RoleType.Scp0492);
                            referenceHub.GetComponent<PlayerStats>().Health = (float)referenceHub.characterClassManager.Classes.Get(RoleType.Scp0492).maxHP;
                            if (component.CompareTag("Ragdoll"))
                                NetworkServer.Destroy(component.gameObject);
                            __instance._recallInProgressServer = false;
                            __instance._recallObjectServer = null;
                            __instance._recallProgressServer = 0f;
                        }
                        return false;
                    }
                    if (!__instance._interactRateLimit.CanExecute(true))
                        return false;
                    if (go == null)
                        return false;
                    Ragdoll component2 = go.GetComponent<Ragdoll>();
                    if (component2 == null)
                    {
                        GameCore.Console.AddDebugLog("SCPCTRL", "SCP-049 | Request 'start recalling' rejected; provided object is not a dead body", MessageImportance.LessImportant, false);
                        return false;
                    }
                    if (!component2.allowRecall)
                    {
                        GameCore.Console.AddDebugLog("SCPCTRL", "SCP-049 | Request 'start recalling' rejected; provided object can't be recalled", MessageImportance.LessImportant, false);
                        return false;
                    }
                    ReferenceHub referenceHub2 = null;
                    foreach (GameObject player2 in PlayerManager.players)
                    {
                        ReferenceHub hub2 = ReferenceHub.GetHub(player2);
                        if (hub2 != null && hub2.queryProcessor.PlayerId == component2.owner.PlayerId)
                        {
                            referenceHub2 = hub2;
                            break;
                        }
                    }
                    if (referenceHub2 == null)
                    {
                        GameCore.Console.AddDebugLog("SCPCTRL", "SCP-049 | Request 'start recalling' rejected; target not found", MessageImportance.LessImportant, false);
                        return false;
                    }
                    if (Vector3.Distance(component2.transform.position, __instance.Hub.PlayerCameraReference.transform.position) >= Scp049.ReviveDistance * 1.3f)
                        return false;
                    GameCore.Console.AddDebugLog("SCPCTRL", "SCP-049 | Request 'start recalling' accepted", MessageImportance.LessImportant, false);
                    __instance._recallObjectServer = referenceHub2.gameObject;
                    __instance._recallProgressServer = 0f;
                    __instance._recallInProgressServer = true;
                    return false;
                }
            }
            catch (Exception e)
            {
                Log.Add("Scp049", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Scp079PlayerScript), nameof(Scp079PlayerScript.CallRpcGainExp))]
    public static class GainExpPatch
    {
        public static bool Prefix(Scp079PlayerScript __instance, ExpGainType type, RoleType details)
        {
            try
            {
                switch (type)
                {
                    case ExpGainType.KillAssist:
                    case ExpGainType.PocketAssist:
                        {
                            Team team = __instance.GetComponent<CharacterClassManager>().Classes.SafeGet(details).team;
                            int num = 6;
                            float num2;
                            switch (team)
                            {
                                case Team.SCP:
                                    num2 = __instance.GetManaFromLabel("SCP Kill Assist", __instance.expEarnWays);
                                    num = 11;
                                    break;
                                case Team.MTF:
                                    num2 = __instance.GetManaFromLabel("MTF Kill Assist", __instance.expEarnWays);
                                    num = 9;
                                    break;
                                case Team.CHI:
                                    num2 = __instance.GetManaFromLabel("Chaos Kill Assist", __instance.expEarnWays);
                                    num = 8;
                                    break;
                                case Team.RSC:
                                    num2 = __instance.GetManaFromLabel("Scientist Kill Assist", __instance.expEarnWays);
                                    num = 10;
                                    break;
                                case Team.CDP:
                                    num2 = __instance.GetManaFromLabel("Class-D Kill Assist", __instance.expEarnWays);
                                    num = 7;
                                    break;
                                default:
                                    num2 = 0f;
                                    break;
                            }
                            num--;
                            if (type == ExpGainType.PocketAssist)
                            {
                                num2 /= 2f;
                            }
                            if (NetworkServer.active)
                            {
                                Environment.OnSCP079GainExp(__instance.gameObject, type, num2, true, out num2, out bool allow);
                                if (!allow)
                                    return false;
                                __instance.AddExperience(num2);
                                return false;
                            }
                            break;
                        }
                    case ExpGainType.DirectKill:
                    case ExpGainType.HardwareHack:
                        break;
                    case ExpGainType.AdminCheat:
                        try
                        {
                            if (NetworkServer.active)
                            {
                                float exp = (float)details;
                                Environment.OnSCP079GainExp(__instance.gameObject, type, exp, true, out exp, out bool allow);
                                if (!allow)
                                    return false;
                                __instance.AddExperience(exp);
                            }
                        }
                        catch
                        {
                            GameCore.Console.AddDebugLog("SCP079", "<color=red>ERROR: An unexpected error occured in RpcGainExp() while gaining points using Admin Cheats", MessageImportance.Normal, false);
                        }
                        break;
                    case ExpGainType.GeneralInteractions:
                        {
                            float num3 = 0f;
                            switch (details)
                            {
                                case RoleType.ClassD:
                                    num3 = __instance.GetManaFromLabel("Door Interaction", __instance.expEarnWays);
                                    break;
                                case RoleType.Spectator:
                                    num3 = __instance.GetManaFromLabel("Tesla Gate Activation", __instance.expEarnWays);
                                    break;
                                case RoleType.Scientist:
                                    num3 = __instance.GetManaFromLabel("Lockdown Activation", __instance.expEarnWays);
                                    break;
                                case RoleType.Scp079:
                                    num3 = __instance.GetManaFromLabel("Elevator Use", __instance.expEarnWays);
                                    break;
                            }
                            if (num3 != 0f)
                            {
                                float num4 = 1f / Mathf.Clamp(__instance.levels[(int)__instance.curLvl].manaPerSecond / 1.5f, 1f, 7f);
                                num3 = Mathf.Round(num3 * num4 * 10f) / 10f;
                                if (NetworkServer.active)
                                {
                                    Environment.OnSCP079GainExp(__instance.gameObject, type, num3, true, out num3, out bool allow);
                                    if (!allow)
                                        return false;
                                    __instance.AddExperience(num3);
                                    return false;
                                }
                            }
                            break;
                        }
                    default:
                        return false;
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Add("Scp079PlayerScript", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Scp079PlayerScript), nameof(Scp079PlayerScript.TargetLevelChanged))]
    public static class GainLvlPatch
    {
        public static bool Prefix(Scp079PlayerScript __instance, int newLvl)
        {
            try
            {
                Environment.OnSCP079GainLvl(__instance.gameObject, true, out bool allow);
                if (allow)
                    __instance.Lvl = (byte)newLvl;
                return allow;
            }
            catch (Exception e)
            {
                Log.Add("Scp079PlayerScript", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Scp079PlayerScript), nameof(Scp079PlayerScript.CallCmdInteract))]
    public static class Scp079InteractPatch
    {
        public static bool Prefix(Scp079PlayerScript __instance, string command, GameObject target)
        {
            try
            {
                if (!__instance._interactRateLimit.CanExecute())
                    return false;
                if (!__instance.iAm079)
                    return false;
                Console.AddDebugLog("SCP079", "Command received from a client: " + command, MessageImportance.LessImportant);
                if (!command.Contains(":"))
                    return false;
                string[] array = command.Split(':');
                __instance.RefreshCurrentRoom();
                if (!__instance.CheckInteractableLegitness(__instance.currentRoom, __instance.currentZone, target, true))
                    return false;
                Environment.OnSCP079Interact(__instance.gameObject, true, out bool allow);
                if (!allow)
                    return false;
                List<string> list = ListPool<string>.Shared.Rent();
                ConfigFile.ServerConfig.GetStringCollection("scp079_door_blacklist", list);
                bool result = true;
                switch (array[0])
                {
                    case "TESLA":
                        {
                            float manaFromLabel = __instance.GetManaFromLabel("Tesla Gate Burst", __instance.abilities);
                            if (manaFromLabel > __instance.curMana)
                            {
                                __instance.RpcNotEnoughMana(manaFromLabel, __instance.curMana);
                                result = false;
                                break;
                            }
                            GameObject gameObject = GameObject.Find(__instance.currentZone + "/" + __instance.currentRoom + "/Gate");
                            if (gameObject != null)
                            {
                                gameObject.GetComponent<TeslaGate>().RpcInstantBurst();
                                __instance.AddInteractionToHistory(gameObject, array[0], true);
                                __instance.Mana -= manaFromLabel;
                            }
                            result = false;
                            break;
                        }
                    case "DOOR":
                        {
                            if (AlphaWarheadController.Host.inProgress)
                            {
                                result = false;
                                break;
                            }

                            if (target == null)
                            {
                                Console.AddDebugLog("SCP079", "The door command requires a target.", MessageImportance.LessImportant);
                                result = false;
                                break;
                            }

                            Door component = target.GetComponent<Door>();
                            if (component == null)
                            {
                                result = false;
                                break;
                            }

                            if (list != null && list.Count > 0 && list != null && list.Contains(component.DoorName))
                            {
                                Console.AddDebugLog("SCP079", "Door access denied by the server.", MessageImportance.LeastImportant);
                                result = false;
                                break;
                            }

                            float manaFromLabel = __instance.GetManaFromLabel("Door Interaction " + (string.IsNullOrEmpty(component.permissionLevel) ? "DEFAULT" : component.permissionLevel), __instance.abilities);
                            if (manaFromLabel > __instance.curMana)
                            {
                                Console.AddDebugLog("SCP079", "Not enough mana.", MessageImportance.LeastImportant);
                                __instance.RpcNotEnoughMana(manaFromLabel, __instance.curMana);
                                result = false;
                                break;
                            }

                            if (component != null && component.ChangeState079())
                            {
                                __instance.Mana -= manaFromLabel;
                                __instance.AddInteractionToHistory(target, array[0], true);
                                Console.AddDebugLog("SCP079", "Door state changed.", MessageImportance.LeastImportant);
                                result = true;
                                break;
                            }
                            Console.AddDebugLog("SCP079", "Door state failed to change.", MessageImportance.LeastImportant);
                            result = false;
                            break;
                        }
                    default:
                        result = true;
                        break;
                }
                ListPool<string>.Shared.Return(list);
                return result;
            }
            catch (Exception e)
            {
                Log.Add("Scp079PlayerScript", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Scp096), nameof(Scp096.EndEnrage))]
    public static class CalmPatch
    {
        public static bool Prefix(Scp096 __instance)
        {
            try
            {
                Environment.OnSCP096Calm(__instance.Hub.gameObject, true, out bool allow);
                if (!allow)
                    return false;
                __instance.EndCharge();
                __instance.SetMovementSpeed(0f);
                __instance.SetJumpHeight(4f);
                __instance.ResetShield();
                __instance.PlayerState = Scp096PlayerState.Calming;
                __instance._calmingTime = 6f;
                __instance._targets.Clear();
                return false;
            }
            catch (Exception e)
            {
                Log.Add("Scp096", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Scp096), nameof(Scp096.Enrage))]
    public static class EnragePatch
    {
        public static bool Prefix(Scp096 __instance)
        {
            try
            {
                if (!NetworkServer.active)
                    throw new InvalidOperationException("Called Enrage from client.");
                Environment.OnSCP096Enrage(__instance.Hub.gameObject, true, out bool allow);
                if (!allow)
                    return false;
                if (__instance.Enraged)
                {
                    __instance.AddReset();
                    return false;
                }
                __instance.SetMovementSpeed(12f);
                __instance.SetJumpHeight(10f);
                __instance.PlayerState = Scp096PlayerState.Enraged;
                __instance.EnrageTimeLeft = __instance.EnrageTime;
                return false;
            }
            catch (Exception e)
            {
                Log.Add("Scp096", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdContain106))]
    public static class ContainPatch
    {
        public static bool Prefix(PlayerInteract __instance)
        {
            try
            {
                if (!__instance._playerInteractRateLimit.CanExecute(true) || (__instance._hc.CufferId > 0 && !PlayerInteract.CanDisarmedInteract))
                    return false;
                if (!UnityEngine.Object.FindObjectOfType<LureSubjectContainer>().allowContain || (__instance._ccm.CurRole.team == Team.SCP && __instance._ccm.CurClass != RoleType.Scp106) || !__instance.ChckDis(GameObject.FindGameObjectWithTag("FemurBreaker").transform.position) || UnityEngine.Object.FindObjectOfType<OneOhSixContainer>().used || __instance._ccm.CurRole.team == Team.RIP)
                    return false;
                bool flag = false;
                foreach (KeyValuePair<GameObject, ReferenceHub> keyValuePair in ReferenceHub.GetAllHubs())
                {
                    if (keyValuePair.Value.characterClassManager.GodMode && keyValuePair.Value.characterClassManager.CurClass == RoleType.Scp106)
                    {
                        flag = true;
                    }
                }
                bool allow = true;
                if (!flag)
                {
                    foreach (KeyValuePair<GameObject, ReferenceHub> keyValuePair2 in ReferenceHub.GetAllHubs())
                    {
                        if (keyValuePair2.Value.characterClassManager.CurClass == RoleType.Scp106)
                        {
                            Environment.OnSCP106Contain(__instance.gameObject, keyValuePair2.Key, true, out allow);
                            if (allow)
                                keyValuePair2.Key.GetComponent<Scp106PlayerScript>().Contain(__instance._hub);
                        }
                    }
                    if (!allow)
                        return false;
                    __instance.RpcContain106(__instance.gameObject);
                    UnityEngine.Object.FindObjectOfType<OneOhSixContainer>().Networkused = true;
                }
                __instance.OnInteract();
                return false;
            }
            catch (Exception e)
            {
                Log.Add("PlayerInteract", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Scp106PlayerScript), nameof(Scp106PlayerScript.CallCmdMakePortal))]
    public static class CreatePortalPatch
    {
        public static bool Prefix(Scp106PlayerScript __instance)
        {
            try
            {
                if (!__instance._interactRateLimit.CanExecute(true))
                    return false;
                if (!__instance.hub.falldamage.isGrounded)
                    return false;
                Transform transform = __instance.transform;
                Debug.DrawRay(transform.position, -transform.up, Color.red, 10f);
                RaycastHit raycastHit;
                if (__instance.iAm106 && !__instance.goingViaThePortal && Physics.Raycast(new Ray(__instance.transform.position, -__instance.transform.up), out raycastHit, 10f, __instance.teleportPlacementMask))
                {
                    Vector3 pos = raycastHit.point - Vector3.up;
                    Environment.OnSCP106CreatePortal(__instance.gameObject, pos, true, out pos, out bool allow);
                    if (allow)
                        __instance.SetPortalPosition(pos);
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Add("Scp106PlayerScript", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Scp106PlayerScript), nameof(Scp106PlayerScript.CallCmdUsePortal))]
    public static class TeleportPatch
    {
        public static bool Prefix(Scp106PlayerScript __instance)
        {
            try
            {
                if (!__instance._interactRateLimit.CanExecute(true))
                    return false;
                if (!__instance.hub.falldamage.isGrounded)
                    return false;
                if (__instance.iAm106 && __instance.portalPosition != Vector3.zero && !__instance.goingViaThePortal)
                {
                    Environment.OnSCP106Teleport(__instance.gameObject, __instance.transform.position, __instance.portalPosition, true, out __instance.portalPosition, out bool allow);
                    if (allow)
                        Timing.RunCoroutine(__instance._DoTeleportAnimation(), Segment.Update);
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Add("Scp106PlayerScript", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdUse914))]
    public static class Use914Patch
    {
        public static bool Prefix(PlayerInteract __instance)
        {
            try
            {
                if (!__instance._playerInteractRateLimit.CanExecute(true) || (__instance._hc.CufferId > 0 && !PlayerInteract.CanDisarmedInteract))
                    return false;
                if (Scp914Machine.singleton.working || !__instance.ChckDis(Scp914Machine.singleton.button.position))
                    return false;
                Environment.OnSCP914Activate(__instance._hub.gameObject, (float)NetworkTime.time, true, out float time, out bool allow);
                if (!allow)
                    return false;
                Scp914Machine.singleton.RpcActivate((double)time);
                __instance.OnInteract();
                return false;
            }
            catch (Exception e)
            {
                Log.Add("PlayerInteract", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdChange914Knob))]
    public static class ChangeKnobPatch
    {
        public static bool Prefix(PlayerInteract __instance)
        {
            try
            {
                if (!__instance._playerInteractRateLimit.CanExecute(true) || (__instance._hc.CufferId > 0 && !PlayerInteract.CanDisarmedInteract))
                    return false;
                if (Scp914Machine.singleton.working || !__instance.ChckDis(Scp914Machine.singleton.knob.position))
                    return false;
                Scp914Knob scp914Knob = Scp914Machine.singleton.knobState + 1;
                Environment.OnSCP914ChangeKnob(__instance._hub.gameObject, scp914Knob, true, out scp914Knob, out bool allow);
                if (!allow)
                    return false;
                Scp914Machine.singleton.NetworkknobState = scp914Knob;
                if (scp914Knob > Scp914Machine.knobStateMax)
                    Scp914Machine.singleton.NetworkknobState = Scp914Machine.knobStateMin;
                __instance.OnInteract();
                return false;
            }
            catch (Exception e)
            {
                Log.Add("PlayerInteract", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Scp914Machine), nameof(Scp914Machine.ProcessItems))]
    public static class UpgradePatch
    {
        public static bool Prefix(Scp914Machine __instance)
        {
            try
            {
                if (!NetworkServer.active)
                    return false;
                Collider[] array = Physics.OverlapBox(__instance.intake.position, __instance.inputSize / 2f);
                __instance.players.Clear();
                __instance.items.Clear();
                foreach (Collider collider in array)
                {
                    CharacterClassManager component = collider.GetComponent<CharacterClassManager>();
                    if (component != null)
                    {
                        __instance.players.Add(component);
                    }
                    else
                    {
                        Pickup component2 = collider.GetComponent<Pickup>();
                        if (component2 != null)
                        {
                            __instance.items.Add(component2);
                        }
                    }
                }
                Environment.OnSCP914Ugrade(__instance.players, __instance.items, __instance.knobState, true, out Scp914Knob knob, out bool allow);
                if (!allow)
                    return false;
                __instance.MoveObjects(__instance.items, __instance.players);
                __instance.UpgradeObjects(__instance.items, __instance.players);
                return false;
            }
            catch (Exception e)
            {
                Log.Add("Scp914.Scp914Machine", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(CheaterReport), nameof(CheaterReport.CallCmdReport), typeof(int), typeof(string), typeof(byte[]), typeof(bool))]
    public static class ReportPatch
    {
        public static bool Prefix(CheaterReport __instance, int playerId, string reason, byte[] signature, bool notifyGm)
        {
            try
            {
                if (!__instance._commandRateLimit.CanExecute(true))
                    return false;
                float num = Time.time - __instance._lastReport;
                if (num < 2f)
                {
                    __instance.GetComponent<GameConsoleTransmission>().SendToClient(__instance.connectionToClient, "[REPORTING] Reporting rate limit exceeded (1).", "red");
                    return false;
                }
                if (num > 60f)
                    __instance._reportedPlayersAmount = 0;
                if (__instance._reportedPlayersAmount > 5)
                {
                    __instance.GetComponent<GameConsoleTransmission>().SendToClient(__instance.connectionToClient, "[REPORTING] Reporting rate limit exceeded (2).", "red");
                    return false;
                }
                if (notifyGm && (!ServerStatic.GetPermissionsHandler().IsVerified || string.IsNullOrEmpty(ServerConsole.Password)))
                {
                    __instance.GetComponent<GameConsoleTransmission>().SendToClient(__instance.connectionToClient, "[REPORTING] Server is not verified - you can't use report feature on __instance server.", "red");
                    return false;
                }
                if (__instance.GetComponent<QueryProcessor>().PlayerId == playerId)
                {
                    __instance.GetComponent<GameConsoleTransmission>().SendToClient(__instance.connectionToClient, "[REPORTING] You can't report yourself!", "red");
                    return false;
                }
                if (string.IsNullOrEmpty(reason))
                {
                    __instance.GetComponent<GameConsoleTransmission>().SendToClient(__instance.connectionToClient, "[REPORTING] Please provide a valid report reason!", "red");
                    return false;
                }
                ReferenceHub referenceHub;
                if (!ReferenceHub.TryGetHub(playerId, out referenceHub))
                {
                    __instance.GetComponent<GameConsoleTransmission>().SendToClient(__instance.connectionToClient, "[REPORTING] Can't find player with that PlayerID.", "red");
                    return false;
                }
                ReferenceHub hub = ReferenceHub.GetHub(__instance.gameObject);
                CharacterClassManager reportedCcm = referenceHub.characterClassManager;
                CharacterClassManager reporterCcm = hub.characterClassManager;
                if (__instance._reportedPlayers == null)
                    __instance._reportedPlayers = new HashSet<int>();
                if (__instance._reportedPlayers.Contains(playerId))
                {
                    __instance.GetComponent<GameConsoleTransmission>().SendToClient(__instance.connectionToClient, "[REPORTING] You have already reported that player.", "red");
                    return false;
                }
                if (string.IsNullOrEmpty(reportedCcm.UserId))
                {
                    __instance.GetComponent<GameConsoleTransmission>().SendToClient(__instance.connectionToClient, "[REPORTING] Failed: User ID of reported player is null.", "red");
                    return false;
                }
                if (string.IsNullOrEmpty(reporterCcm.UserId))
                {
                    __instance.GetComponent<GameConsoleTransmission>().SendToClient(__instance.connectionToClient, "[REPORTING] Failed: your User ID of is null.", "red");
                    return false;
                }
                string reporterNickname = hub.nicknameSync.MyNick;
                string reportedNickname = referenceHub.nicknameSync.MyNick;
                if (!notifyGm)
                {
                    Environment.OnLocalReport(reason, reporterCcm.gameObject, reportedCcm.gameObject, true, out bool allo);
                    if (!allo)
                        return false;
                    Console.AddLog(string.Concat(new string[]
                    {
                        "Player ",
                        hub.LoggedNameFromRefHub(),
                        " reported player ",
                        referenceHub.LoggedNameFromRefHub(),
                        " with reason ",
                        reason,
                        "."
                    }), Color.gray, false);
                    __instance.GetComponent<GameConsoleTransmission>().SendToClient(__instance.connectionToClient, "[REPORTING] Player report successfully sent to local administrators.", "green");
                    if (CheaterReport.SendReportsByWebhooks)
                    {
                        GameConsoleTransmission gct = __instance.GetComponent<GameConsoleTransmission>();
                        new Thread(delegate ()
                        {
                            __instance.LogReport(gct, reporterCcm.UserId, reportedCcm.UserId, ref reason, playerId, false, reporterNickname, reportedNickname);
                        })
                        {
                            Priority = System.Threading.ThreadPriority.Lowest,
                            IsBackground = true,
                            Name = "Reporting player (locally) - " + reportedCcm.UserId + " by " + reporterCcm.UserId
                        }.Start();
                    }
                    return false;
                }
                if (signature == null)
                    return false;
                if (!ECDSA.VerifyBytes(reportedCcm.SyncedUserId + ";" + reason, signature, __instance.GetComponent<ServerRoles>().PublicKey))
                {
                    __instance.GetComponent<GameConsoleTransmission>().SendToClient(__instance.connectionToClient, "[REPORTING] Invalid report signature.", "red");
                    return false;
                }
                __instance._lastReport = Time.time;
                __instance._reportedPlayersAmount++;
                Environment.OnGlobalReport(reason, reporterCcm.gameObject, reportedCcm.gameObject, true, out bool allow);
                if (!allow)
                    return false;
                GameCore.Console.AddLog(string.Concat(new string[]
                {
                    "Player ",
                    hub.LoggedNameFromRefHub(),
                    " reported player ",
                    referenceHub.LoggedNameFromRefHub(),
                    " with reason ",
                    reason,
                    ". Sending report to Global Moderation."
                }), Color.gray, false);
                new Thread(delegate ()
                {
                    __instance.IssueReport(__instance.GetComponent<GameConsoleTransmission>(), reporterCcm.UserId, reportedCcm.UserId, reportedCcm.AuthToken, reportedCcm.connectionToClient.address, reporterCcm.AuthToken, reporterCcm.connectionToClient.address, ref reason, ref signature, ECDSA.KeyToString(__instance.GetComponent<ServerRoles>().PublicKey), playerId, reporterNickname, reportedNickname);
                })
                {
                    Priority = System.Threading.ThreadPriority.Lowest,
                    IsBackground = true,
                    Name = "Reporting player - " + reportedCcm.UserId + " by " + reporterCcm.UserId
                }.Start();
                return false;
            }
            catch (Exception e)
            {
                Log.Add("CheaterReport", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(RespawnManager), nameof(RespawnManager.Spawn))]
    public static class TeamRespawnPatch
    {
        public static bool Prefix(RespawnManager __instance)
        {
            try
            {
                SpawnableTeam spawnableTeam;
                if (!RespawnWaveGenerator.SpawnableTeams.TryGetValue(__instance.NextKnownTeam, out spawnableTeam) || __instance.NextKnownTeam == SpawnableTeamType.None)
                {
                    ServerConsole.AddLog("Fatal error. Team '" + __instance.NextKnownTeam.ToString() + "' is undefined.", ConsoleColor.Red);
                    return false;
                }
                List<ReferenceHub> list = ReferenceHub.GetAllHubs().Values.Where(h => !h.serverRoles.OverwatchEnabled && h.characterClassManager.NetworkCurClass == RoleType.Spectator && h.characterClassManager.NetworkCurClass != RoleType.None).ToList();
                if (__instance._prioritySpawn)
                    list = list.OrderBy(h => h.characterClassManager.DeathTime).ToList();
                else
                    list.ShuffleList();
                RespawnTickets singleton = RespawnTickets.Singleton;
                int num = singleton.GetAvailableTickets(__instance.NextKnownTeam);
                if (num == 0)
                {
                    num = singleton.DefaultTeamAmount;
                    RespawnTickets.Singleton.GrantTickets(singleton.DefaultTeam, singleton.DefaultTeamAmount, true);
                }
                int num2 = Mathf.Min(num, spawnableTeam.MaxWaveSize);
                Environment.OnTeamRespawn(list.GetGameObjects(), __instance.NextKnownTeam, true, out __instance.NextKnownTeam, out bool allow);
                if (!allow)
                    return false;
                while (list.Count > num2)
                {
                    list.RemoveAt(list.Count - 1);
                }
                list.ShuffleList();
                List<ReferenceHub> list2 = ListPool<ReferenceHub>.Shared.Rent();
                foreach (ReferenceHub referenceHub in list)
                {
                    try
                    {
                        RoleType classid = spawnableTeam.ClassQueue[Mathf.Min(list2.Count, spawnableTeam.ClassQueue.Length - 1)];
                        referenceHub.characterClassManager.SetPlayersClass(classid, referenceHub.gameObject, false, false);
                        list2.Add(referenceHub);
                        ServerLogs.AddLog(ServerLogs.Modules.ClassChange, string.Concat(new string[]
                        {
                            "Player ",
                            referenceHub.LoggedNameFromRefHub(),
                            " respawned as ",
                            classid.ToString(),
                            "."
                        }), ServerLogs.ServerLogType.GameEvent, false);
                    }
                    catch (Exception ex)
                    {
                        if (referenceHub != null)
                        {
                            Log.Add("RespawnManager", ex);
                            ServerLogs.AddLog(ServerLogs.Modules.ClassChange, "Player " + referenceHub.LoggedNameFromRefHub() + " couldn't be spawned. Err msg: " + ex.Message, ServerLogs.ServerLogType.GameEvent, false);
                        }
                        else
                        {
                            Log.Add("RespawnManager", ex);
                            ServerLogs.AddLog(ServerLogs.Modules.ClassChange, "Couldn't spawn a player - target's ReferenceHub is null.", ServerLogs.ServerLogType.GameEvent, false);
                        }
                    }
                }
                if (list2.Count > 0)
                {
                    ServerLogs.AddLog(ServerLogs.Modules.ClassChange, string.Concat(new object[]
                    {
                        "RespawnManager has successfully spawned ",
                        list2.Count,
                        " players as ",
                        __instance.NextKnownTeam.ToString(),
                        "!"
                    }), ServerLogs.ServerLogType.GameEvent, false);
                    RespawnTickets.Singleton.GrantTickets(__instance.NextKnownTeam, -list2.Count * spawnableTeam.TicketRespawnCost, false);
                    UnitNamingRule unitNamingRule;
                    if (UnitNamingRules.TryGetNamingRule(__instance.NextKnownTeam, out unitNamingRule))
                    {
                        string text;
                        unitNamingRule.GenerateNew(__instance.NextKnownTeam, out text);
                        foreach (ReferenceHub referenceHub2 in list2)
                        {
                            referenceHub2.characterClassManager.NetworkCurSpawnableTeamType = (byte)__instance.NextKnownTeam;
                            referenceHub2.characterClassManager.NetworkCurUnitName = text;
                        }
                        unitNamingRule.PlayEntranceAnnouncement(text);
                    }
                    RespawnEffectsController.ExecuteAllEffects(RespawnEffectsController.EffectType.UponRespawn, __instance.NextKnownTeam);
                }
                ListPool<ReferenceHub>.Shared.Return(list2);
                __instance.NextKnownTeam = SpawnableTeamType.None;
                return false;
            }
            catch (Exception e)
            {
                Log.Add("RespawnManager", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.CmdStartRound))]
    public static class RoundStartPatch
    {
        public static bool Prefix(CharacterClassManager __instance)
        {
            try
            {
                if (!NetworkServer.active)
                    return false;
                Environment.OnRoundStart();
                GameObject.Find("MeshDoor173").GetComponentInChildren<Door>().ForceCooldown(PluginManager.Config.GetFloat("scp173_door_cooldown", 25f));
                __instance.NetworkRoundStarted = true;
                return false;
            }
            catch (Exception e)
            {
                Log.Add("CharacterClassManager", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.AddLog))]
    public static class WaitingForPlayers
    {
        public static void Prefix(string q)
        {
            Environment.OnConsoleAddLog(q);
            if (q == "Waiting for players...")
                Environment.OnWaitingForPlayers();
        }
    }

    [HarmonyPatch(typeof(AlphaWarheadController), nameof(AlphaWarheadController.Detonate))]
    public static class DetonatePatch
    {
        public static bool Prefix(AlphaWarheadController __instance)
        {
            try
            {
                Environment.OnWarheadDetonate();
                __instance.detonated = true;
                __instance.RpcShake(true);
                GameObject[] array = GameObject.FindGameObjectsWithTag("LiftTarget");
                foreach (Scp079PlayerScript instance in Scp079PlayerScript.instances)
                {
                    instance.lockedDoors.Clear();
                }
                foreach (GameObject player in PlayerManager.players)
                {
                    GameObject[] array2 = array;
                    foreach (GameObject gameObject in array2)
                    {
                        if (player.GetComponent<PlayerStats>().Explode(Vector3.Distance(gameObject.transform.position, player.transform.position) < 3.5f))
                        {
                            __instance.warheadKills++;
                        }
                    }
                }
                Door[] array3 = UnityEngine.Object.FindObjectsOfType<Door>();
                foreach (Door door in array3)
                {
                    door.OpenWarhead(force: true, door.blockAfterDetonation, clear079Lock: true);
                }
                ServerLogs.AddLog(ServerLogs.Modules.Warhead, "Warhead detonated.", ServerLogs.ServerLogType.GameEvent);
                return false;
            }
            catch (Exception e)
            {
                Log.Add("AlphaWarheadController", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerInteract), nameof(PlayerInteract.CallCmdDetonateWarhead))]
    public static class PlayerWarheadStartPatch
    {
        public static bool Prefix(PlayerInteract __instance)
        {
            try
            {
                if (__instance._playerInteractRateLimit.CanExecute() && (__instance._hc.CufferId <= 0 || PlayerInteract.CanDisarmedInteract) && __instance._playerInteractRateLimit.CanExecute())
                {
                    GameObject gameObject = GameObject.Find("OutsitePanelScript");
                    if (__instance.ChckDis(gameObject.transform.position) && AlphaWarheadOutsitePanel.nukeside.enabled && gameObject.GetComponent<AlphaWarheadOutsitePanel>().keycardEntered)
                    {
                        Environment.OnWarheadStart(__instance.gameObject, AlphaWarheadController.Host.timeToDetonation, true, out AlphaWarheadController.Host.timeToDetonation, out bool allow);
                        if (!allow)
                            return false;
                        AlphaWarheadController.Host.StartDetonation();
                        ReferenceHub component = __instance.GetComponent<ReferenceHub>();
                        ServerLogs.AddLog(ServerLogs.Modules.Warhead, component.LoggedNameFromRefHub() + " started the Alpha Warhead detonation.", ServerLogs.ServerLogType.GameEvent);
                        __instance.OnInteract();
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Add("PlayerInteract", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(AlphaWarheadController), nameof(AlphaWarheadController.CancelDetonation), new Type[] { typeof(GameObject) })]
    public static class WarheadStopPatch
    {
        public static bool Prefix(AlphaWarheadController __instance, GameObject disabler)
        {
            try
            {
                if (!__instance.inProgress || !(__instance.timeToDetonation > 10f) || __instance._isLocked)
                    return false;
                Environment.OnWarheadCancel(disabler, __instance.timeToDetonation, true, out __instance.timeToDetonation, out bool allow);
                if (!allow)
                    return false;
                if (__instance.timeToDetonation <= 15f && disabler != null)
                    __instance.GetComponent<PlayerStats>().TargetAchieve(disabler.GetComponent<NetworkIdentity>().connectionToClient, "thatwasclose");
                for (sbyte b = 0; b < __instance.scenarios_resume.Length; b = (sbyte)(b + 1))
                    if (__instance.scenarios_resume[b].SumTime() > __instance.timeToDetonation && __instance.scenarios_resume[b].SumTime() < __instance.scenarios_start[AlphaWarheadController._startScenario].SumTime())
                        __instance.NetworksyncResumeScenario = b;
                __instance.NetworktimeToDetonation = ((AlphaWarheadController._resumeScenario < 0) ? __instance.scenarios_start[AlphaWarheadController._startScenario].SumTime() : __instance.scenarios_resume[AlphaWarheadController._resumeScenario].SumTime()) + (float)__instance.cooldown;
                __instance.NetworkinProgress = false;
                Door[] array = UnityEngine.Object.FindObjectsOfType<Door>();
                foreach (Door obj in array)
                {
                    obj.warheadlock = false;
                    obj.CheckpointLockOpenWarhead = false;
                    obj.UpdateLock();
                }
                if (NetworkServer.active)
                    __instance._autoDetonate = false;
                ServerLogs.AddLog(ServerLogs.Modules.Warhead, "Detonation cancelled.", ServerLogs.ServerLogType.GameEvent);
                return false;
            }
            catch (Exception e)
            {
                Log.Add("AlphaWarheadController", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.Roundrestart))]
    public static class RoundRestartPatch
    {
        public static void Postfix(PlayerStats __instance)
        {
            Server.PlayerList.Reset();
            Environment.OnRoundRestart();
        }
    }

    [HarmonyPatch(typeof(RoundSummary), nameof(RoundSummary.Start))]
    public static class RoundEndPatch
    {
        private static readonly MethodInfo CustomProcess = SymbolExtensions.GetMethodInfo(() => Process(null));

        private static IEnumerator<float> Process(RoundSummary instance)
        {
            RoundSummary roundSummary = instance;
            while (roundSummary != null)
            {
                while (RoundSummary.RoundLock || !RoundSummary.RoundInProgress() || (roundSummary._keepRoundOnOne && PlayerManager.players.Count < 2))
                    yield return 0.0f;
                yield return 0.0f;
                RoundSummary.SumInfo_ClassList newList = default;
                foreach (GameObject player in PlayerManager.players)
                {
                    if (!(player == null))
                    {
                        CharacterClassManager component = player.GetComponent<CharacterClassManager>();
                        if (component.Classes.CheckBounds(component.CurClass))
                        {
                            switch (component.Classes.SafeGet(component.CurClass).team)
                            {
                                case Team.SCP:
                                    if (component.CurClass == RoleType.Scp0492)
                                    {
                                        ++newList.zombies;
                                        continue;
                                    }
                                    ++newList.scps_except_zombies;
                                    continue;
                                case Team.MTF:
                                    ++newList.mtf_and_guards;
                                    continue;
                                case Team.CHI:
                                    ++newList.chaos_insurgents;
                                    continue;
                                case Team.RSC:
                                    ++newList.scientists;
                                    continue;
                                case Team.CDP:
                                    ++newList.class_ds;
                                    continue;
                                default:
                                    continue;
                            }
                        }
                    }
                }
                newList.warhead_kills = AlphaWarheadController.Host.detonated ? AlphaWarheadController.Host.warheadKills : -1;
                yield return float.NegativeInfinity;
                newList.time = (int)Time.realtimeSinceStartup;
                yield return float.NegativeInfinity;
                RoundSummary.roundTime = newList.time - roundSummary.classlistStart.time;
                int num1 = newList.mtf_and_guards + newList.scientists;
                int num2 = newList.chaos_insurgents + newList.class_ds;
                int num3 = newList.scps_except_zombies + newList.zombies;
                float num4 = roundSummary.classlistStart.class_ds == 0 ? 0.0f : (RoundSummary.escaped_ds + newList.class_ds) / roundSummary.classlistStart.class_ds;
                float num5 = roundSummary.classlistStart.scientists == 0 ? 1f : (RoundSummary.escaped_scientists + newList.scientists) / roundSummary.classlistStart.scientists;

                if (newList.class_ds == 0 && num1 == 0)
                {
                    roundSummary._roundEnded = true;
                }
                else
                {
                    int num6 = 0;
                    if (num1 > 0)
                        ++num6;
                    if (num2 > 0)
                        ++num6;
                    if (num3 > 0)
                        ++num6;
                    if (num6 <= 1)
                    {
                        roundSummary._roundEnded = true;
                    }
                }
                CheckRoundEndEvent ev = new CheckRoundEndEvent(roundSummary._roundEnded, RoundSummary.LeadingTeam.Draw);
                if (num1 > 0)
                {
                    if (RoundSummary.escaped_ds == 0 && RoundSummary.escaped_scientists != 0)
                        ev.LeadingTeam = RoundSummary.LeadingTeam.FacilityForces;
                }
                else
                {
                    ev.LeadingTeam = RoundSummary.escaped_ds != 0 ? RoundSummary.LeadingTeam.ChaosInsurgency : RoundSummary.LeadingTeam.Anomalies;
                }

                Environment.OnCheckRoundEnd(ev, out CheckRoundEndEvent checkRoundEndEvent);
                roundSummary._roundEnded = checkRoundEndEvent.Allow;

                if (roundSummary._roundEnded)
                {
                    FriendlyFireConfig.PauseDetector = true;
                    string str = "Round finished! Anomalies: " + num3 + " | Chaos: " + num2 + " | Facility Forces: " + num1 + " | D escaped percentage: " + num4 + " | S escaped percentage: : " + num5;
                    Console.AddLog(str, Color.gray, false);
                    ServerLogs.AddLog(ServerLogs.Modules.Logger, str, ServerLogs.ServerLogType.GameEvent);
                    byte i1;
                    for (i1 = 0; i1 < 75; ++i1)
                        yield return 0.0f;
                    int timeToRoundRestart = Mathf.Clamp(ConfigFile.ServerConfig.GetInt("auto_round_restart_time", 10), 5, 1000);
                    if (roundSummary != null)
                    {
                        newList.scps_except_zombies -= newList.zombies;
                        Environment.OnRoundEnd(checkRoundEndEvent.LeadingTeam, newList, timeToRoundRestart, true, out timeToRoundRestart, out RoundSummary.SumInfo_ClassList classListEnd, out bool allow);
                        if (!allow)
                            yield return 0.0f;
                        Environment.OnShowSummary(Round.Info.ClassListOnStart, classListEnd, checkRoundEndEvent.LeadingTeam, true, out bool all);
                        if (!all)
                            yield return 0.0f;
                        roundSummary.RpcShowRoundSummary(roundSummary.classlistStart, classListEnd, checkRoundEndEvent.LeadingTeam, RoundSummary.escaped_ds, RoundSummary.escaped_scientists, RoundSummary.kills_by_scp, timeToRoundRestart);
                    }

                    for (int i2 = 0; i2 < 50 * (timeToRoundRestart - 1); ++i2)
                        yield return 0.0f;
                    roundSummary.RpcDimScreen();
                    for (i1 = 0; i1 < 50; ++i1)
                        yield return 0.0f;
                    PlayerManager.localPlayer.GetComponent<PlayerStats>().Roundrestart();
                    yield break;
                }
            }
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            foreach (CodeInstruction instruction in instructions)
            {
                if (instruction.opcode == OpCodes.Call)
                {
                    if (instruction.operand != null && instruction.operand is MethodBase methodBase && methodBase.Name != nameof(RoundSummary._ProcessServerSideCode))
                    {
                        yield return instruction;
                    }
                    else
                    {
                        yield return new CodeInstruction(OpCodes.Call, CustomProcess);
                    }
                }
                else
                {
                    yield return instruction;
                }
            }
        }
    }

    [HarmonyPatch(typeof(Intercom), nameof(Intercom.CallCmdSetTransmit))]
    public static class IntercomSpeakPatch
    {
        private static bool Prefix(Intercom __instance, bool player)
        {
            try
            {
                if (!__instance._interactRateLimit.CanExecute(true) || Intercom.AdminSpeaking)
                    return false;
                if (player)
                {
                    if (!__instance.ServerAllowToSpeak())
                        return false;
                    Environment.OnIntercomSpeak(__instance.gameObject, true, out bool allow);
                    if (!allow)
                        return false;
                    Intercom.host.RequestTransmission(__instance.gameObject);
                }
                else
                {
                    if (!(Intercom.host.Networkspeaker == __instance.gameObject))
                        return false;
                    Environment.OnIntercomSpeak(__instance.gameObject, true, out bool allow);
                    if (!allow)
                        return false;
                    Intercom.host.RequestTransmission(null);
                }

                return false;
            }
            catch (Exception e)
            {
                Log.Add("Intercom", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Inventory), nameof(Inventory.CallCmdDropItem))]
    public static class ItemDropPatch
    {
        private static bool Prefix(Inventory __instance, int itemInventoryIndex)
        {
            try
            {
                if (!__instance._iawRateLimit.CanExecute(true) || itemInventoryIndex < 0 ||
                    itemInventoryIndex >= __instance.items.Count)
                    return false;
                Inventory.SyncItemInfo syncItemInfo = __instance.items[itemInventoryIndex];
                if (__instance.items[itemInventoryIndex].id != syncItemInfo.id)
                    return false;
                Environment.OnDropItem(syncItemInfo, __instance._hub.gameObject, true, out syncItemInfo, out bool allow);
                if (!allow)
                    return false;
                Pickup droppedPickup = __instance.SetPickup(syncItemInfo.id, syncItemInfo.durability, __instance.transform.position, __instance.camera.transform.rotation, syncItemInfo.modSight, syncItemInfo.modBarrel, syncItemInfo.modOther);
                __instance.items.RemoveAt(itemInventoryIndex);
                Environment.OnDroppedItem(droppedPickup, __instance._hub.gameObject);
                return false;
            }
            catch (Exception e)
            {
                Log.Add("Inventory", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Inventory), nameof(Inventory.CallCmdSetUnic))]
    public static class ChangeItemPatch
    {
        private static void Prefix(Inventory __instance, int i)
        {
            try
            {
                if (__instance.itemUniq == i)
                    return;
                int oldItemIndex = __instance.GetItemIndex();
                if (oldItemIndex == -1 && i == -1)
                    return;
                Inventory.SyncItemInfo oldItem = oldItemIndex == -1 ? new Inventory.SyncItemInfo() { id = ItemType.None } : __instance.GetItemInHand();
                Inventory.SyncItemInfo newItem = new Inventory.SyncItemInfo() { id = ItemType.None };

                foreach (Inventory.SyncItemInfo item in __instance.items)
                {
                    if (item.uniq == i)
                        newItem = item;
                }
                Environment.OnChangeItem(oldItem, newItem, __instance._hub.gameObject, true, out newItem, out bool allow);
                if (!allow)
                    return;
                oldItemIndex = __instance.GetItemIndex();
                if (oldItemIndex != -1)
                    __instance.items[oldItemIndex] = oldItem;
            }
            catch (Exception e)
            {
                Log.Add("Inventory", e);
            }
        }
    }

    [HarmonyPatch(typeof(ItemSearchCompletor), nameof(ItemSearchCompletor.Complete))]
    public static class PickupItemPatch
    {
        private static bool Prefix(ItemSearchCompletor __instance)
        {
            try
            {
                Environment.OnPickupItem(__instance.TargetPickup, __instance.Hub.gameObject, true, out bool allow);
                return allow;
            }
            catch (Exception e)
            {
                Log.Add("ItemSearchCompletor", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(WeaponManager), nameof(WeaponManager.CallCmdReload))]
    public static class WeaponReloadPatch
    {
        private static bool Prefix(WeaponManager __instance, bool animationOnly)
        {
            try
            {
                if (!__instance._iawRateLimit.CanExecute(false))
                    return false;
                int itemIndex = __instance._hub.inventory.GetItemIndex();
                if (itemIndex < 0 || itemIndex >= __instance._hub.inventory.items.Count || (__instance.curWeapon < 0 || __instance._hub.inventory.curItem != __instance.weapons[__instance.curWeapon].inventoryID) || __instance._hub.inventory.items[itemIndex].durability >= (double)__instance.weapons[__instance.curWeapon].maxAmmo)
                    return false;
                Environment.OnReload(__instance._hub.gameObject, animationOnly, true, out animationOnly, out bool allow);
                return allow;
            }
            catch (Exception e)
            {
                Log.Add("WeaponManager", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.SetPlayersClass))]
    public static class SetClassPatch
    {
        public static void Postfix(CharacterClassManager __instance, RoleType classid, GameObject ply, bool lite = false, bool escape = false)
        {
            try
            {
                Environment.OnSetClass(ply, classid, true, out RoleType roleType, out bool allow);
            }
            catch (Exception e)
            {
                Log.Add("CharacterClassManager", e);
            }
        }
    }

    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.CallCmdRegisterEscape))]
    public static class CheckEscapePatch
    {
        public static bool Prefix(CharacterClassManager __instance)
        {
            try
            {
                if (!__instance._interactRateLimit.CanExecute(true))
                    return false;
                if (Vector3.Distance(__instance.transform.position, __instance.GetComponent<Escape>().worldPosition) >= (float)(Escape.radius * 2))
                    return false;
                bool flag = false;
                Handcuffs handcuffs = __instance._hub.handcuffs;

                if (handcuffs.CufferId >= 0 && CharacterClassManager.CuffedChangeTeam)
                {
                    CharacterClassManager characterClassManager = ReferenceHub.GetHub(handcuffs.GetCuffer(handcuffs.CufferId)).characterClassManager;
                    if (__instance.CurClass == RoleType.Scientist && (characterClassManager.CurClass == RoleType.ChaosInsurgency || characterClassManager.CurClass == RoleType.ClassD))
                    {
                        flag = true;
                    }
                    if (__instance.CurClass == RoleType.ClassD && (characterClassManager.CurRole.team == Team.MTF || characterClassManager.CurClass == RoleType.Scientist))
                    {
                        flag = true;
                    }
                }
                Environment.OnCheckEscape(__instance.gameObject, true, out bool allow);
                if (!allow)
                    return false;
                RespawnTickets singleton = RespawnTickets.Singleton;
                Team team = __instance.CurRole.team;
                if (team != Team.RSC)
                {
                    if (team == Team.CDP)
                    {
                        if (flag)
                        {
                            __instance.SetPlayersClass(RoleType.NtfCadet, __instance.gameObject, false, true);
                            RoundSummary.escaped_scientists++;
                            singleton.GrantTickets(SpawnableTeamType.NineTailedFox, ConfigFile.ServerConfig.GetInt("respawn_tickets_mtf_classd_cuffed_count", 1), false);
                            return false;
                        }
                        __instance.SetPlayersClass(RoleType.ChaosInsurgency, __instance.gameObject, false, true);
                        RoundSummary.escaped_ds++;
                        singleton.GrantTickets(SpawnableTeamType.ChaosInsurgency, ConfigFile.ServerConfig.GetInt("respawn_tickets_ci_classd_count", 1), false);
                        return false;
                    }
                }
                else
                {
                    if (flag)
                    {
                        __instance.SetPlayersClass(RoleType.ChaosInsurgency, __instance.gameObject, false, true);
                        RoundSummary.escaped_ds++;
                        singleton.GrantTickets(SpawnableTeamType.ChaosInsurgency, ConfigFile.ServerConfig.GetInt("respawn_tickets_ci_scientist_cuffed_count", 2), false);
                        return false;
                    }
                    __instance.SetPlayersClass(RoleType.NtfScientist, __instance.gameObject, false, true);
                    RoundSummary.escaped_scientists++;
                    singleton.GrantTickets(SpawnableTeamType.NineTailedFox, ConfigFile.ServerConfig.GetInt("respawn_tickets_mtf_scientist_count", 1), false);
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Add("CharacterClassManager", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Scp096), nameof(Scp096.AddTarget))]
    public static class AddTargetPatch
    { 
        public static bool Prefix(Scp096 __instance, GameObject target)
        {
            try
            {
                if (!NetworkServer.active)
                    throw new InvalidOperationException("Called AddTarget from client.");
                ReferenceHub hub = ReferenceHub.GetHub(target);
                if (!Scp096Properties.CanTutorialTriggerScp096 && hub.characterClassManager.CurClass == RoleType.Tutorial)
                    return false;
                Environment.OnScp096AddTarget(target, true, out bool allow);
                if (!allow)
                    return false;
                if (__instance.CanReceiveTargets && !(hub == null) && !__instance._targets.Contains(hub))
                {
                    if (!__instance._targets.IsEmpty() && Scp096Properties.AddEnrageTimeWhenLooked)
                        __instance.AddReset();
                    __instance._targets.Add(hub);
                    __instance.AdjustShield(Scp096Properties.ShieldPerPlayer);
                }
                return false;
            }
            catch (Exception e)
            {
                Log.Add("Scp096", e);
                return true;
            }
        }
    }

    [HarmonyPatch(typeof(Pickup), nameof(Pickup.SetupPickup))]
    public static class SpawnItemPatch
    {
        public static bool Prefix(Pickup __instance, ItemType itemId, float durability, GameObject ownerPlayer, Pickup.WeaponModifiers mods, Vector3 pickupPosition, Quaternion pickupRotation)
        {
            try
            {
                ReferenceHub owner = ReferenceHub.GetHub(ownerPlayer) == null ? ReferenceHub.LocalHub : ReferenceHub.GetHub(ownerPlayer);
                Environment.OnSpawnItem(__instance, itemId, durability, owner.gameObject, mods, pickupPosition, pickupRotation, true, out itemId, out durability, out ownerPlayer, out mods,
                 out pickupPosition, out pickupRotation, out bool allow);
                if (!allow)
                    return false;
                __instance.NetworkitemId = itemId;
                __instance.Networkdurability = durability;
                __instance.ownerPlayer = ownerPlayer;
                __instance.NetworkweaponMods = mods;
                __instance.Networkposition = pickupPosition;
                __instance.Networkrotation = pickupRotation;
                __instance.transform.position = pickupPosition;
                __instance.transform.rotation = pickupRotation;
                __instance.RefreshModel();
                __instance.UpdatePosition();
                return false;
            }
            catch (Exception e)
            {
                Log.Add("Pickup", e);
                return true;
            }
        }
    }
}

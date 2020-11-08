﻿using Vigilance.API;
using Vigilance.Enums;
using Vigilance.Extensions;
using Vigilance.Events;
using Vigilance.EventHandlers;
using UnityEngine;
using Grenades;
using System.Collections.Generic;
using Scp914;
using Respawning;
using System;
using MEC;
using System.Linq;

namespace Vigilance
{
    public static class Environment
    {
        public static List<CoroutineHandle> ActiveCoroutines = new List<CoroutineHandle>();
        public static System.Random Random = new System.Random();

        public static IEnumerable<T> GetValues<T>()
        {
            if (typeof(T).BaseType != typeof(Enum))
            {
                throw new ArgumentException("T must be of type System.Enum");
            }
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        public static CoroutineHandle StartCoroutine(IEnumerator<float> handler, string name = "")
        {
            if (handler == null)
                return default;
            try
            {
                CoroutineHandle handle = Timing.RunCoroutine(handler, name);
                ActiveCoroutines.Add(handle);
                if (!string.IsNullOrEmpty(handle.Tag))
                    Log.Add("Environment", $"Started coroutine \"{handle.Tag}\"", LogType.Debug);
                return handle;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                return default;
            }
        }

        public static void KillCoroutine(CoroutineHandle handle)
        {
            Timing.KillCoroutines(new CoroutineHandle[] { handle });
        }

        public static void StopCoroutine(string name)
        {
            foreach (CoroutineHandle handle in ActiveCoroutines)
            {
                if (handle.Tag.ToLower() == name.ToLower() || handle.Tag.ToLower().Contains(name.ToLower()))
                {
                    KillCoroutine(handle);
                    ActiveCoroutines.Remove(handle);
                }
            }
        }

        public static float GetRandomNumber() => Random.Next(1, 10000) / 100f;
        public static float GetRandomNumber(int max) => Random.Next(1, max);
        public static bool GetRandomBool()
        {
            if (GetRandomNumber() < 100)
                return true;
            else
                return false;
        }

        public static bool GetChance(float chance)
        {
            if (chance >= (Random.Next(1, 10000) / 100f))
                return true;
            else
                return false;
        }

        public static void StopAllCoroutines()
        {
            foreach (CoroutineHandle handle in ActiveCoroutines)
                Timing.KillCoroutines(handle);
        }

        public static void OnAnnounceDecontamination(bool global, int id, bool all, out bool isGlobal, out int annId, out bool allow)
        {
            try
            {
                AnnounceDecontaminationEvent ev = new AnnounceDecontaminationEvent(global, id, all);
                EventManager.Trigger<AnnounceDecontaminationHandler>(ev);
                isGlobal = ev.IsGlobal;
                annId = ev.AnnouncementId;
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                isGlobal = global;
                annId = id;
                allow = all;
            }
        }

        public static void OnAnnounceNTFEntrace(int scps, string unt, int num, bool all, out int left, out string unit, out int number, out bool allow)
        {
            try
            {
                AnnounceNTFEntranceEvent ev = new AnnounceNTFEntranceEvent(scps, unt, num, all);
                EventManager.Trigger<AnnounceNTFEntranceHandler>(ev);
                left = ev.SCPsLeft;
                unit = ev.Unit;
                number = ev.Number;
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                left = scps;
                unit = unt;
                number = num;
                allow = all;
            }
        }

        public static void OnAnnounceSCPTermination(Player killer, Role role, PlayerStats.HitInfo info, string cause, bool all, out bool allow)
        {
            try
            {
                AnnounceSCPTerminationEvent ev = new AnnounceSCPTerminationEvent(killer, role, info, cause, all);
                EventManager.Trigger<AnnounceSCPTerminationHandler>(ev);
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                allow = all;
            }
        }

        public static void OnCancelMedical(float cldwn, Player ply, ItemType item, bool all, out float cooldown, out bool allow)
        {
            try
            {
                CancelMedicalItemEvent ev = new CancelMedicalItemEvent(cldwn, ply, item, all);
                EventManager.Trigger<CancelMedicalItemHandler>(ev);
                cooldown = ev.Cooldown;
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                cooldown = cldwn;
                allow = all;
            }
        }

        public static void OnGlobalReport(string res, Player rep, Player red, bool all, out bool allow)
        {
            try
            {
                GlobalReportEvent ev = new GlobalReportEvent(res, rep, red, all);
                EventManager.Trigger<GlobalReportHandler>(ev);
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                allow = all;
            }
        }

        public static void OnLocalReport(string res, Player rep, Player red, bool all, out bool allow)
        {
            try
            {
                LocalReportEvent ev = new LocalReportEvent(res, rep, red, all);
                EventManager.Trigger<LocalReportHandler>(ev);
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                allow = all;
            }
        }

        public static void OnCheckEscape(Player ply, bool all, out bool allow)
        {
            try
            {
                CheckEscapeEvent ev = new CheckEscapeEvent(ply, all);
                EventManager.Trigger<CheckEscapeHandler>(ev);
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                allow = all;
            }
        }


        public static void OnCheckRoundEnd(CheckRoundEndEvent ev, out CheckRoundEndEvent even)
        {
            try
            {
                EventManager.Trigger<CheckRoundEndHandler>(ev);
                even = ev;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                even = ev;
            }
        }

        public static void OnConsoleCommand(string cmd, Player ply, bool all, out string reply, out string color, out bool allow)
        {
            try
            {
                if (string.IsNullOrEmpty(cmd) || ply == null)
                {
                    allow = all;
                    reply = "";
                    color = "";
                    return;
                }
                string[] args = cmd.Split(' ');
                args[0] = args[0].Replace(".", "");
                ConsoleCommandEvent ev = new ConsoleCommandEvent(args.Combine(), ply, all);
                EventManager.Trigger<EventHandlers.ConsoleCommandHandler>(ev);
                reply = ev.Reply;
                color = ev.Color.ToLower();
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                reply = e.Message;
                color = "red";
                allow = all;
            }
        }

        public static void OnDecontamination(bool all, out bool allow)
        {
            try
            {
                DecontaminationEvent ev = new DecontaminationEvent(all);
                EventManager.Trigger<DecontaminationHandler>(ev);
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                allow = all;
            }
        }

        public static void OnDoorInteract(bool all, Door door, Player user, out bool allow)
        {
            try
            {
                DoorInteractEvent ev = new DoorInteractEvent(all, user, door);
                EventManager.Trigger<DoorInteractHandler>(ev);
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                allow = all;
            }
        }

        public static void OnDropItem(Inventory.SyncItemInfo sync, Player ply, bool all, out Inventory.SyncItemInfo itemInfo, out bool allow)
        {
            try
            {
                DropItemEvent ev = new DropItemEvent(sync, ply, all);
                EventManager.Trigger<DropItemHandler>(ev);
                itemInfo = ev.Item;
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                itemInfo = sync;
                allow = all;
            }
        }

        public static void OnDroppedItem(Pickup pickup, Player ply)
        {
            try
            {
                DroppedItemEvent ev = new DroppedItemEvent(pickup, ply);
                EventManager.Trigger<DroppedItemHandler>(ev);
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
            }
        }

        public static void OnElevatorInteract(Lift lift, Player ply, bool all, out bool allow)
        {
            try
            {
                ElevatorInteractEvent ev = new ElevatorInteractEvent(lift, ply, all);
                EventManager.Trigger<ElevatorInteractHandler>(ev);
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                allow = all;
            }
        }

        public static void OnFemurEnter(Player ply, bool all, out bool allow)
        {
            try
            {
                FemurEnterEvent ev = new FemurEnterEvent(ply, all);
                EventManager.Trigger<FemurEnterHandler>(ev);
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                allow = all;
            }
        }

        public static void OnGeneratorInsert(Generator079 gen, Player ply, bool all, out bool allow)
        {
            try
            {
                GeneratorInsertEvent ev = new GeneratorInsertEvent(gen, ply, all);
                EventManager.Trigger<GeneratorInsertHandler>(ev);
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                allow = all;
            }
        }

        public static void OnGeneratorEject(Generator079 gen, Player ply, bool all, out bool allow)
        {
            try
            {
                GeneratorEjectEvent ev = new GeneratorEjectEvent(gen, ply, all);
                EventManager.Trigger<GeneratorEjectHandler>(ev);
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                allow = all;
            }
        }

        public static void OnGeneratorUnlock(Generator079 gen, Player ply, bool all, out bool allow)
        {
            try
            {
                GeneratorUnlockEvent ev = new GeneratorUnlockEvent(gen, ply, all);
                EventManager.Trigger<GeneratorUnlockHandler>(ev);
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                allow = all;
            }
        }

        public static void OnGeneratorOpen(Generator079 gen, Player ply, bool all, out bool allow)
        {
            try
            {
                GeneratorOpenEvent ev = new GeneratorOpenEvent(gen, ply, all);
                EventManager.Trigger<GeneratorOpenHandler>(ev);
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                allow = all;
            }
        }

        public static void OnGeneratorClose(Generator079 gen, Player ply, bool all, out bool allow)
        {
            try
            {
                GeneratorCloseEvent ev = new GeneratorCloseEvent(gen, ply, all);
                EventManager.Trigger<GeneratorCloseHandler>(ev);
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                allow = all;
            }
        }

        public static void OnGeneratorFinish(Generator079 gen, bool all, out bool allow)
        {
            try
            {
                GeneratorFinishEvent ev = new GeneratorFinishEvent(gen, all);
                EventManager.Trigger<GeneratorFinishHandler>(ev);
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                allow = all;
            }
        }

        public static void OnThrowGrenade(Player ply, Grenade grenade, GrenadeType nadeType, bool all, out bool allow)
        {
            try
            {
                ThrowGrenadeEvent ev = new ThrowGrenadeEvent(ply, grenade, nadeType, all);
                EventManager.Trigger<ThrowGrenadeHandler>(ev);
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                allow = all;
            }
        }

        public static void OnIntercomSpeak(Player ply, bool all, out bool allow)
        {
            try
            {
                IntercomSpeakEvent ev = new IntercomSpeakEvent(ply, all);
                EventManager.Trigger<IntercomSpeakHandler>(ev);
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                allow = all;
            }
        }

        public static void OnChangeItem(Inventory.SyncItemInfo oldItem, Inventory.SyncItemInfo newItem, Player ply, bool all, out Inventory.SyncItemInfo changeTo, out bool allow)
        {
            try
            {
                ChangeItemEvent ev = new ChangeItemEvent(oldItem, newItem, ply, all);
                EventManager.Trigger<ChangeItemHandler>(ev);
                changeTo = ev.NewItem;
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                changeTo = newItem;
                allow = all;
            }
        }

        public static void OnLockerInteract(Locker locker, Player ply, bool all, out bool allow)
        {
            try
            {
                LockerInteractEvent ev = new LockerInteractEvent(locker, ply, all);
                EventManager.Trigger<LockerInteractHandler>(ev);
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                allow = all;
            }
        }

        public static void OnPickupItem(Pickup it, Player ply, bool all, out bool allow)
        {
            try
            {
                PickupItemEvent ev = new PickupItemEvent(it, ply, all);
                EventManager.Trigger<PickupItemHandler>(ev);
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                allow = all;
            }
        }

        public static void OnPlaceBlood(int type, Vector3 pos, bool all, out Vector3 position, out bool allow)
        {
            try
            {
                PlaceBloodEvent ev = new PlaceBloodEvent(all, type, pos);
                EventManager.Trigger<PlaceBloodHandler>(ev);
                position = ev.Position;
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                position = pos;
                allow = all;
            }
        }

        public static void OnPlaceDecal(Vector3 pos, bool all, out Vector3 position, out bool allow)
        {
            try
            {
                PlaceDecalEvent ev = new PlaceDecalEvent(all, pos);
                EventManager.Trigger<PlaceDecalHandler>(ev);
                position = ev.Position;
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                position = pos;
                allow = all;
            }
        }

        public static void OnBan(Player issuer, Player ply, string res, long issuance, long expiery, bool all, out long newExpiery, out bool allow)
        {
            try
            {
                BanEvent ev = new BanEvent(ply, issuer, string.IsNullOrEmpty(res) ? "No reason provided." : res, issuance, expiery, all);
                EventManager.Trigger<BanEventHandler>(ev);
                allow = ev.Allow;
                newExpiery = ev.Expiration.Ticks;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                allow = all;
                newExpiery = expiery;
            }
        }

        public static void OnHandcuff(Player ply, Player cuff, bool all, out bool allow)
        {
            try
            {
                HandcuffEvent ev = new HandcuffEvent(ply, cuff, all);
                EventManager.Trigger<HandcuffHandler>(ev);
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                allow = all;
            }
        }

        public static void OnUncuff(Player ply, Player uncuff, bool all, out bool allow)
        {
            try
            {
                UncuffEvent ev = new UncuffEvent(ply, uncuff, all);
                EventManager.Trigger<UncuffHandler>(ev);
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                allow = all;
            }
        }

        public static void OnHurt(Player ply, Player attack, PlayerStats.HitInfo info, bool all, out PlayerStats.HitInfo hitInfo, out bool allow)
        {
            try
            {
                PlayerHurtEvent ev = new PlayerHurtEvent(ply, attack, info, all);
                EventManager.Trigger<PlayerHurtHandler>(ev);
                hitInfo = ev.HitInfo;
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                hitInfo = info;
                allow = all;
            }
        }

        public static void OnInteract(Player ply, bool all, out bool allow)
        {
            try
            {
                PlayerInteractEvent ev = new PlayerInteractEvent(ply, all);
                EventManager.Trigger<PlayerInteractHandler>(ev);
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                allow = all;
            }
        }

        public static void OnPlayerJoin(Player ply)
        {
            try
            {
                PlayerJoinEvent ev = new PlayerJoinEvent(ply);
                EventManager.Trigger<PlayerJoinHandler>(ev);
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
            }
        }

        public static void OnPlayerLeave(Player ply, out bool destroy)
        {
            try
            {
                PlayerLeaveEvent ev = new PlayerLeaveEvent(ply);
                EventManager.Trigger<PlayerLeaveHandler>(ev);
                destroy = ev.DestroyHub;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                destroy = true;
            }
        }

        public static void OnReload(Player ply, bool anim, bool all, out bool animOnly, out bool allow)
        {
            try
            {
                WeaponReloadEvent ev = new WeaponReloadEvent(ply, ply.Hub.inventory.curItem.GetWeaponType(), anim, all);
                EventManager.Trigger<WeaponReloadHandler>(ev);
                animOnly = ev.AnimationOnly;
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                animOnly = anim;
                allow = all;
            }
        }

        public static void OnSpawn(Player ply, Vector3 pos, RoleType role, bool all, out Vector3 location, out RoleType newRole, out bool allow)
        {
            try
            {
                PlayerSpawnEvent ev = new PlayerSpawnEvent(ply, pos, role, all);
                EventManager.Trigger<PlayerSpawnHandler>(ev);
                location = ev.Location;
                newRole = ev.Role;
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                location = pos;
                newRole = role;
                allow = all;
            }
        }

        public static void OnPocketEscape(Player ply, Vector3 pos, bool all, out Vector3 escapePos, out bool allow)
        {
            try
            {
                PocketEscapeEvent ev = new PocketEscapeEvent(ply, pos, all);
                EventManager.Trigger<PocketEscapeHandler>(ev);
                escapePos = ev.Position;
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                escapePos = pos;
                allow = all;
            }
        }

        public static void OnPocketEnter(Player ply, bool dmg, float dmgAmount, bool all, out bool hurt, out float damage, out bool allow)
        {
            try
            {
                PocketEnterEvent ev = new PocketEnterEvent(ply, dmg, dmgAmount, all);
                EventManager.Trigger<PocketEnterHandler>(ev);
                hurt = ev.Hurt;
                allow = ev.Allow;
                damage = ev.Damage;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                hurt = dmg;
                allow = all;
                damage = dmgAmount;
            }
        }

        public static void OnPocketHurt(Player ply, float dmg, bool all, out float damage, out bool allow)
        {
            try
            {
                PocketHurtEvent ev = new PocketHurtEvent(ply, all, dmg);
                EventManager.Trigger<PocketHurtHandler>(ev);
                damage = ev.Damage;
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                damage = dmg;
                allow = all;
            }
        }

        public static void OnRemoteAdminCommand(CommandSender issuer, string cmd, bool all, out bool allow, out string response)
        {
            try
            {
                Player issue = issuer.GetPlayer();
                if (issue != null)
                {
                    RemoteAdminCommandEvent ev = new RemoteAdminCommandEvent(issue, cmd, "SERVER#Unknown command!", all);
                    EventManager.Trigger<RemoteAdminEventHandler>(ev);
                    allow = ev.Allow;
                    response = ev.Response;
                }
                else
                {
                    allow = all;
                    response = "SERVER#An error occured.";
                }
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                allow = all;
                response = e.StackTrace;
            }
        }

        public static void OnRoundEnd(RoundSummary.LeadingTeam leadingTeam, RoundSummary.SumInfo_ClassList endList, int timeToRestart, bool all, out int toRestart, out RoundSummary.SumInfo_ClassList classListOnEnd, out bool allow)
        {
            try
            {
                Round.CurrentState = RoundState.Ended;
                RoundEndEvent ev = new RoundEndEvent(leadingTeam, endList, timeToRestart, all);
                EventManager.Trigger<RoundEndHandler>(ev);
                toRestart = ev.TimeToRestart;
                classListOnEnd = ev.ClassList;
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                toRestart = timeToRestart;
                classListOnEnd = endList;
                allow = all;
            }
        }

        public static void OnRoundStart()
        {
            try
            {
                EventManager.Trigger<RoundStartHandler>(new RoundStartEvent());
                Round.CurrentState = RoundState.Started;
                if (ConfigManager.WindowHealth != 30f)
                    foreach (BreakableWindow window in UnityEngine.Object.FindObjectsOfType<BreakableWindow>())
                        window.health = ConfigManager.WindowHealth == -1f ? float.MaxValue : ConfigManager.WindowHealth;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
            }
        }

        public static void OnRoundRestart()
        {
            try
            {
                EventManager.Trigger<RoundRestartHandler>(new RoundRestartEvent());
                Round.CurrentState = RoundState.Restarting;
                if (ConfigManager.ShouldReloadConfigsOnRoundRestart)
                    Server.ReloadConfigs();
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
            }
        }

        public static void OnSCP914Ugrade(List<CharacterClassManager> ccms, List<Pickup> items, Scp914Knob setting, bool all, out Scp914Knob knobSetting, out bool allow)
        {
            try
            {
                SCP914UpgradeEvent ev = new SCP914UpgradeEvent(Scp914Machine.singleton, ccms.GetGameObjects(), items, setting, all);
                EventManager.Trigger<SCP914UpgradeHandler>(ev);
                knobSetting = ev.KnobSetting;
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                knobSetting = setting;
                allow = all;
            }
        }

        public static void OnSCP096Enrage(Player ply, bool all, out bool allow)
        {
            try
            {
                SCP096EnrageEvent ev = new SCP096EnrageEvent(ply, all);
                EventManager.Trigger<SCP096EnrageHandler>(ev);
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                allow = all;
            }
        }

        public static void OnSCP096Calm(Player ply, bool all, out bool allow)
        {
            try
            {
                SCP096CalmEvent ev = new SCP096CalmEvent(ply, all);
                EventManager.Trigger<SCP096CalmHandler>(ev);
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                allow = all;
            }
        }

        public static void OnSCP106Contain(Player killer, Player ply, bool all, out bool allow)
        {
            try
            {
                SCP106ContainEvent ev = new SCP106ContainEvent(killer, ply, all);
                EventManager.Trigger<SCP106ContainHandler>(ev);
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                allow = all;
            }
        }

        public static void OnSCP106CreatePortal(Player ply, Vector3 pos, bool all, out Vector3 portalPosition, out bool allow)
        {
            try
            {
                SCP106CreatePortalEvent ev = new SCP106CreatePortalEvent(ply, pos, all);
                EventManager.Trigger<SCP106CreatePortalHandler>(ev);
                portalPosition = ev.Position;
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                portalPosition = pos;
                allow = all;
            }
        }

        public static void OnSCP106Teleport(Player ply, Vector3 old, Vector3 newPos, bool all, out Vector3 newPosition, out bool allow)
        {
            try
            {
                SCP106TeleportEvent ev = new SCP106TeleportEvent(ply, old, newPos, all);
                EventManager.Trigger<SCP106TeleportHandler>(ev);
                newPosition = ev.NewPosition;
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                newPosition = newPos;
                allow = all;
            }
        }

        public static void OnSCP914Activate(Player ply, float networkTime, bool all, out float time, out bool allow)
        {
            try
            {
                SCP914ActivateEvent ev = new SCP914ActivateEvent(ply, networkTime, all);
                EventManager.Trigger<SCP914ActivateHandler>(ev);
                time = ev.Time;
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                time = networkTime;
                allow = all;
            }
        }

        public static void OnSCP914ChangeKnob(Player ply, Scp914Knob setting, bool all, out Scp914Knob knobSetting, out bool allow)
        {
            try
            {
                SCP914ChangeKnobEvent ev = new SCP914ChangeKnobEvent(ply, Scp914Machine.singleton, setting, all);
                EventManager.Trigger<SCP914ChangeKnobHandler>(ev);
                knobSetting = ev.KnobSetting;
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                knobSetting = setting;
                allow = all;
            }
        }

        public static void OnSetClass(Player ply, RoleType role, bool all, out RoleType outRole, out bool allow)
        {
            try
            {
                SetClassEvent ev = new SetClassEvent(ply, role, all);
                EventManager.Trigger<SetClassHandler>(ev);
                outRole = ev.Role;
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                outRole = role;
                allow = all;
            }
        }

        public static void OnSetGroup(Player ply, UserGroup g, bool all, out UserGroup userGroup, out bool allow)
        {
            try
            {
                SetGroupEvent ev = new SetGroupEvent(ply, g, all);
                EventManager.Trigger<SetGroupHandler>(ev);
                userGroup = ev.Group;
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                userGroup = g;
                allow = all;
            }
        }

        public static void OnShoot(Player ply, GameObject target, WeaponType weapon, bool all, out bool allow)
        {
            try
            {
                WeaponShootEvent ev = new WeaponShootEvent(ply, target, weapon, all);
                EventManager.Trigger<WeaponShootHandler>(ev);
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                allow = all;
            }
        }

        public static void OnLateShoot(Player ply, GameObject target, WeaponType weapon, bool all, out bool allow)
        {
            try
            {
                WeaponLateShootEvent ev = new WeaponLateShootEvent(ply, target, weapon, all);
                EventManager.Trigger<WeaponLateShootHandler>(ev);
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                allow = all;
            }
        }

        public static void OnSpawnRagdoll(Ragdoll ragdoll, bool all, out bool allow)
        {
            try
            {
                SpawnRagdollEvent ev = new SpawnRagdollEvent(ragdoll, all);
                EventManager.Trigger<SpawnRagdollHandler>(ev);
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                allow = all;
            }
        }

        public static void OnSyncData(Player ply, Vector2 speed, byte anim, bool all, out byte curAnim, out bool allow)
        {
            try
            {
                SyncDataEvent ev = new SyncDataEvent(ply, speed, anim, all);
                EventManager.Trigger<SyncDataHandler>(ev);
                curAnim = ev.CurrentAnimation;
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                curAnim = anim;
                allow = all;
            }
        }

        public static void OnTeamRespawn(List<GameObject> players, SpawnableTeamType type, bool all, out SpawnableTeamType teamType, out bool allow)
        {
            try
            {
                TeamRespawnEvent ev = new TeamRespawnEvent(players, type, all);
                EventManager.Trigger<TeamRespawnHandler>(ev);
                teamType = ev.Team;
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                teamType = type;
                allow = all;
            }
        }

        public static void OnTriggerTesla(Player ply, TeslaGate tesla, bool all, out bool allow)
        {
            try
            {
                TriggerTeslaEvent ev = new TriggerTeslaEvent(ply, tesla, all);
                EventManager.Trigger<TriggerTeslaHandler>(ev);
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                allow = all;
            }
        }

        public static void OnUseMedical(Player ply, ItemType item, int hp, bool all, out int hpToRecover, out bool allow)
        {
            try
            {
                UseMedicalItemEvent ev = new UseMedicalItemEvent(ply, item, hp, all);
                EventManager.Trigger<UseMedicalHandler>(ev);
                hpToRecover = ev.HpToRecover;
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                hpToRecover = hp;
                allow = all;
            }
        }

        public static void OnWaitingForPlayers()
        {
            try
            {
                EventManager.Trigger<WaitingForPlayersHandler>(new WaitingForPlayersEvent());
                Round.CurrentState = RoundState.WaitingForPlayers;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
            }
        }

        public static void OnWarheadDetonate()
        {
            try
            {
                EventManager.Trigger<WarheadDetonateHandler>(new DetonationEvent());
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
            }
        }

        public static void OnWarheadCancel(Player ply, float timeLeft, bool all, out float timeToDetonation, out bool allow)
        {
            try
            {
                WarheadCancelEvent ev = new WarheadCancelEvent(ply, timeLeft, all);
                EventManager.Trigger<WarheadCancelHandler>(ev);
                timeToDetonation = ev.TimeLeft;
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                timeToDetonation = timeLeft;
                allow = all;
            }
        }

        public static void OnWarheadStart(Player ply, float timeLeft, bool all, out float timeToDetonation, out bool allow)
        {
            try
            {
                WarheadStartEvent ev = new WarheadStartEvent(ply, timeLeft, all);
                EventManager.Trigger<WarheadStartHandler>(ev);
                timeToDetonation = ev.TimeLeft;
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                timeToDetonation = timeLeft;
                allow = all;
            }
        }

        public static void OnWarheadKeycardAccess(Player ply, bool all, out bool allow)
        {
            try
            {
                WarheadKeycardAccessEvent ev = new WarheadKeycardAccessEvent(ply, all);
                EventManager.Trigger<WarheadKeycardAccessHandler>(ev);
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                allow = all;
            }
        }

        public static void OnRecall(Player ply, Ragdoll ragdoll, bool all, out bool allow)
        {
            try
            {
                SCP049RecallEvent ev = new SCP049RecallEvent(ply, ragdoll, all);
                EventManager.Trigger<SCP049RecallHandler>(ev);
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                allow = all;
            }
        }

        public static void OnSCP079GainExp(Player ply, ExpGainType expGain, float exp, bool all, out float experience, out bool allow)
        {
            try
            {
                SCP079GainExpEvent ev = new SCP079GainExpEvent(ply, expGain, exp, all);
                EventManager.Trigger<SCP079GainExpHandler>(ev);
                experience = ev.Experience;
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                experience = exp;
                allow = all;
            }
        }

        public static void OnSCP079GainLvl(Player ply, int lvl, bool all, out int level, out bool allow)
        {
            try
            {
                SCP079GainLvlEvent ev = new SCP079GainLvlEvent(ply, lvl, all);
                EventManager.Trigger<SCP079GainLvlHandler>(ev);
                allow = ev.Allow;
                level = ev.Level;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                allow = all;
                level = lvl;
            }
        }

        public static void OnSCP079Interact(Player ply, bool all, out bool allow)
        {
            try
            {
                SCP079InteractEvent ev = new SCP079InteractEvent(ply, all);
                EventManager.Trigger<SCP079InteractHandler>(ev);
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                allow = all;
            }
        }

        public static void OnServerCommand(string cmd, bool all, out string response, out bool allow)
        {
            try
            {
                ServerCommandEvent ev = new ServerCommandEvent(cmd, "SERVER#Unknown command!", all);
                EventManager.Trigger<ServerCommandHandler>(ev);
                response = ev.Response;
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                response = e.StackTrace;
                allow = all;
            }
        }

        public static void OnShowSummary(RoundSummary.SumInfo_ClassList start, RoundSummary.SumInfo_ClassList end, RoundSummary.LeadingTeam team, bool all, out bool allow)
        {
            try
            {
                RoundShowSummaryEvent ev = new RoundShowSummaryEvent(start, end, team, all);
                EventManager.Trigger<RoundShowSummaryHandler>(ev);
                allow = ev.Allow;
                Round.CurrentState = RoundState.ShowingSummary;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                allow = all;
            }
        }

        public static void OnConsoleAddLog(string txt)
        {
            try
            {
                ConsoleAddLogEvent ev = new ConsoleAddLogEvent(txt);
                EventManager.Trigger<ConsoleAddLogEventHandler>(ev);
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
            }
        }

        public static void OnPlayerDie(Player ply, Player target, PlayerStats.HitInfo info, bool all, out PlayerStats.HitInfo hitInfo, out bool allow, out bool spawnRagdoll)
        {
            try
            {
                RoundSummary.Kills++;
                PlayerDieEvent ev = new PlayerDieEvent(target, ply, info, all);
                EventManager.Trigger<PlayerDieEventHandler>(ev);
                hitInfo = ev.HitInfo;
                allow = ev.Allow;
                spawnRagdoll = ev.SpawnRagdoll;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                hitInfo = info;
                allow = all;
                spawnRagdoll = true;
            }
        }

        public static void OnScp096AddTarget(Player target, bool all, out bool allow)
        {
            try
            {
                Scp096AddTargetEvent ev = new Scp096AddTargetEvent(target, all);
                EventManager.Trigger<Scp096AddTargetHandler>(ev);
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                allow = all;
            }
        }

        public static void OnSpawnItem(Pickup pickup, ItemType id, float dur, GameObject ownr, Pickup.WeaponModifiers mods, Vector3 pos, Quaternion rot, bool all, out ItemType itemId, out float durability, out GameObject owner, out Pickup.WeaponModifiers modifiers, out Vector3 position, out Quaternion rotation, out bool allow)
        {
            try
            {
                CharacterClassManager ccm = ownr?.GetComponent<CharacterClassManager>();
                Player ply = ccm.IsHost ? Server.PlayerList.Local : new Player(ReferenceHub.GetHub(ownr));
                SpawnItemEvent ev = new SpawnItemEvent(pickup, id, dur, ply, mods, pos, rot, all);
                EventManager.Trigger<SpawnItemEventHandler>(ev);
                itemId = ev.ItemId;
                durability = ev.Durability;
                owner = ev.Owner.GameObject;
                modifiers = ev.WeaponModifiers;
                position = ev.Position;
                rotation = ev.Rotation;
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add("Environment", e);
                itemId = id;
                durability = dur;
                owner = ownr;
                modifiers = mods;
                position = pos;
                rotation = rot;
                allow = all;
            }
        }

        public static void OnScp914UpgradeItem(Pickup input, out ItemType outputId)
        {
            try
            {
                Scp914UpgradeItemEvent ev = new Scp914UpgradeItemEvent(input);
                ev.Output = Map.Scp914.Singleton.UpgradeItemID(input.itemId);
                EventManager.Trigger<Scp914UpgradeItemHandler>(ev);
                outputId = ev.Output;
            }
            catch (Exception e)
            {
                Log.Add(nameof(Environment.OnScp914UpgradeItem), e);
                outputId = Map.Scp914.Singleton.UpgradeItemID(input.itemId);
            }
        }

        public static void OnScp914UpgradeHeldItem(Player player, Inventory.SyncItemInfo input, out Inventory.SyncItemInfo output)
        {
            try
            {
                Scp914UpgradeHeldItemEvent ev = new Scp914UpgradeHeldItemEvent(player, input);
                Inventory.SyncItemInfo info = new Inventory.SyncItemInfo();
                info.durability = input.durability;
                info.id = Map.Scp914.Singleton.UpgradeItemID(input.id);
                info.modBarrel = input.modBarrel;
                info.modOther = input.modOther;
                info.modSight = input.modSight;
                info.uniq = input.uniq;
                ev.Output = info;
                EventManager.Trigger<Scp914UpgradeHeldItemHandler>(ev);
                output = ev.Output;
            }
            catch (Exception e)
            {
                Log.Add(nameof(Environment.OnScp914UpgradeHeldItem), e);
                Inventory.SyncItemInfo info = new Inventory.SyncItemInfo();
                info.durability = input.durability;
                info.id = Map.Scp914.Singleton.UpgradeItemID(input.id);
                info.modBarrel = input.modBarrel;
                info.modOther = input.modOther;
                info.modSight = input.modSight;
                info.uniq = input.uniq;
                output = info;
            }
        }

        public static void OnScp914UpgradePlayer(Player player, out bool allow)
        {
            try
            {
                Scp914UpgradePlayerEvent ev = new Scp914UpgradePlayerEvent(player);
                ev.Allow = true;
                EventManager.Trigger<Scp914UpgradePlayerHandler>(ev);
                allow = ev.Allow;
            }
            catch (Exception e)
            {
                Log.Add(nameof(Environment.OnScp914UpgradePlayer), e);
                allow = true;
            }
        }
    }
}

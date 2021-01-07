using Vigilance.API;
using Vigilance.Enums;
using Vigilance.Extensions;
using Vigilance.EventHandlers;
using UnityEngine;
using Grenades;
using System;
using Scp914;
using System.Collections.Generic;
using PlayableScps;
using Respawning;
using System.Linq;
using CustomPlayerEffects;
using Interactables.Interobjects.DoorUtils;

namespace Vigilance.Events
{
    public class AnnounceDecontaminationEvent : Event
    {
        public bool IsGlobal { get; set; }
        public int AnnouncementId { get; set; }
        public bool Allow { get; set; }

        public AnnounceDecontaminationEvent(bool isGlobal, int id, bool allow)
        {
            IsGlobal = isGlobal;
            AnnouncementId = id;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((AnnounceDecontaminationHandler)handler).OnAnnounceDecontamination(this);
        }
    }

    public class AnnounceNTFEntranceEvent : Event
    {
        public int SCPsLeft { get; set; }
        public string Unit { get; set; }
        public int Number { get; set; }
        public bool Allow { get; set; }

        public AnnounceNTFEntranceEvent(int scps, string name, int num, bool allow)
        {
            SCPsLeft = scps;
            Unit = name;
            Number = num;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((AnnounceNTFEntranceHandler)handler).OnAnnounceEntrance(this);
        }
    }

    public class AnnounceSCPTerminationEvent : Event
    {
        public Player Killer { get; }
        public Role Role { get; }
        public PlayerStats.HitInfo HitInfo { get; }
        public string Cause { get; }
        public bool Allow { get; set; }

        public AnnounceSCPTerminationEvent(Player killer, Role role, PlayerStats.HitInfo info, string cause, bool allow)
        {
            Killer = killer;
            Role = role;
            HitInfo = info;
            Cause = cause;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((AnnounceSCPTerminationHandler)handler).OnAnnounceTermination(this);
        }
    }

    public class CancelMedicalItemEvent : Event
    {
        public float Cooldown { get; set; }
        public Player Player { get; }
        public ItemType Item { get; }
        public bool Allow { get; set; }

        public CancelMedicalItemEvent(float cooldown, Player ply, ItemType item, bool allow)
        {
            Cooldown = cooldown;
            Player = ply;
            Item = item;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((CancelMedicalItemHandler)handler).OnCancelMedical(this);
        }
    }

    public class GlobalReportEvent : Event
    {
        public string Reason { get; }
        public Player Reporter { get; }
        public Player Reported { get; }
        public bool Allow { get; set; }

        public GlobalReportEvent(string reason, Player reporter, Player reported, bool allow)
        {
            Reason = reason;
            Reporter = reporter;
            Reported = reported;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((GlobalReportHandler)handler).OnGlobalReport(this);
        }
    }

    public class LocalReportEvent : Event
    {
        public string Reason { get; }
        public Player Reporter { get; }
        public Player Reported { get; }
        public bool Allow { get; set; }

        public LocalReportEvent(string reason, Player reporter, Player reported, bool allow)
        {
            Reason = reason;
            Reporter = reporter;
            Reported = reported;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((LocalReportHandler)handler).OnLocalReport(this);
        }
    }

    public class CheckEscapeEvent : Event
    {
        public Player Player { get; }
        public bool Allow { get; set; }

        public CheckEscapeEvent(Player player, bool allow)
        {
            Player = player;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((CheckEscapeHandler)handler).OnCheckEscape(this);
        }
    }

    public class CheckRoundEndEvent : Event
    {
        public bool Allow { get; set; }
        public RoundSummary.LeadingTeam LeadingTeam { get; set; }


        public CheckRoundEndEvent(bool allow, RoundSummary.LeadingTeam leadingTeam)
        {
            Allow = allow;
            LeadingTeam = leadingTeam;
        }

        public override void Execute(EventHandler handler)
        {
            ((CheckRoundEndHandler)handler).OnCheckRoundEnd(this);
        }
    }

    public class ConsoleCommandEvent : Event
    {
        public string Command { get; }
        public string Reply { get; set; }
        public string Color { get; set; }
        public Player Sender { get; }
        public bool Allow { get; set; }

        public ConsoleCommandEvent(string cmd, Player sender, bool allow)
        {
            Command = cmd;
            Sender = sender;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((EventHandlers.ConsoleCommandHandler)handler).OnConsoleCommand(this);
        }
    }

    public class DecontaminationEvent : Event
    {
        public bool Allow { get; set; }

        public DecontaminationEvent(bool allow) => Allow = allow;

        public override void Execute(EventHandler handler)
        {
            ((DecontaminationHandler)handler).OnDecontamination(this);
        }
    }

    public class DoorInteractEvent : Event
    {
        public bool Allow { get; set; }
        public Player Player { get; }
        public API.Door Door { get; }

        public DoorInteractEvent(bool allow, Player ply, API.Door d)
        {
            Allow = allow;
            Player = ply;
            Door = d;
        }

        public override void Execute(EventHandler handler)
        {
            ((DoorInteractHandler)handler).OnDoorInteract(this);
        }
    }

    public class DropItemEvent : Event
    {
        public Inventory.SyncItemInfo Item { get; set; }
        public Player Player { get; }
        public bool Allow { get; set; }

        public DropItemEvent(Inventory.SyncItemInfo itemInfo, Player ply, bool allow)
        {
            Item = itemInfo;
            Player = ply;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((DropItemHandler)handler).OnDropItem(this);
        }
    }

    public class DroppedItemEvent : Event
    {
        public Pickup Item { get; set; }
        public Player Player { get; }

        public DroppedItemEvent(Pickup item, Player ply)
        {
            Item = item;
            Player = ply;
        }

        public override void Execute(EventHandler handler)
        {
            ((DroppedItemHandler)handler).OnItemDropped(this);
        }
    }

    public class ElevatorInteractEvent : Event
    {
        public Lift Lift { get; }
        public Player Player { get; }
        public bool Allow { get; set; }

        public ElevatorInteractEvent(Lift lift, Player ply, bool allow)
        {
            Lift = lift;
            Player = ply;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((ElevatorInteractHandler)handler).OnElevatorInteract(this);
        }
    }

    public class FemurEnterEvent : Event
    {
        public bool Allow { get; set; }
        public Player Player { get; }

        public FemurEnterEvent(Player ply, bool allow)
        {
            Allow = allow;
            Player = ply;
        }

        public override void Execute(EventHandler handler)
        {
            ((FemurEnterHandler)handler).OnFemurEnter(this);
        }
    }

    public class GeneratorInsertEvent : Event
    {
        public Generator079 Generator { get; }
        public Player Player { get; }
        public bool Allow { get; set; }

        public GeneratorInsertEvent(Generator079 gen, Player ply, bool allow)
        {
            Generator = gen;
            Player = ply;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((GeneratorInsertHandler)handler).OnGeneratorInsert(this);
        }
    }

    public class GeneratorEjectEvent : Event
    {
        public Generator079 Generator { get; }
        public Player Player { get; }
        public bool Allow { get; set; }

        public GeneratorEjectEvent(Generator079 gen, Player ply, bool allow)
        {
            Generator = gen;
            Player = ply;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((GeneratorEjectHandler)handler).OnGeneratorEject(this);
        }
    }

    public class GeneratorUnlockEvent : Event
    {
        public Generator079 Generator { get; }
        public Player Player { get; }
        public bool Allow { get; set; }

        public GeneratorUnlockEvent(Generator079 gen, Player ply, bool allow)
        {
            Generator = gen;
            Player = ply;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((GeneratorUnlockHandler)handler).OnGeneratorUnlock(this);
        }
    }

    public class GeneratorOpenEvent : Event
    {
        public Generator079 Generator { get; }
        public Player Player { get; }
        public bool Allow { get; set; }

        public GeneratorOpenEvent(Generator079 gen, Player ply, bool allow)
        {
            Generator = gen;
            Player = ply;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((GeneratorOpenHandler)handler).OnGeneratorOpen(this);
        }
    }

    public class GeneratorCloseEvent : Event
    {
        public Generator079 Generator { get; }
        public Player Player { get; }
        public bool Allow { get; set; }

        public GeneratorCloseEvent(Generator079 gen, Player ply, bool allow)
        {
            Generator = gen;
            Player = ply;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((GeneratorCloseHandler)handler).OnGeneratorClose(this);
        }
    }

    public class GeneratorFinishEvent : Event
    {
        public Generator079 Generator { get; }
        public bool Allow { get; set; }

        public GeneratorFinishEvent(Generator079 gen, bool allow)
        {
            Generator = gen;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((GeneratorFinishHandler)handler).OnGeneratorFinish(this);
        }
    }

    public class ThrowGrenadeEvent : Event
    {
        public Player Thrower { get; }
        public Grenade Grenade { get; }
        public GrenadeManager GrenadeManager { get; }
        public GameObject GrenadeInstance { get; }
        public GrenadeType GrenadeType { get; }
        public bool Allow { get; set; }

        public ThrowGrenadeEvent(Player ply, Grenade grenade, GrenadeType grenadeType,  GrenadeManager gm, GameObject instance, bool allow)
        {
            Thrower = ply;
            Grenade = grenade;
            GrenadeManager = gm;
            GrenadeInstance = instance;
            GrenadeType = grenadeType;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((ThrowGrenadeHandler)handler).OnThrowGrenade(this);
        }
    }

    public class IntercomSpeakEvent : Event
    {
        public Player Speaker { get; }
        public bool Allow { get; set; }


        public IntercomSpeakEvent(Player speak, bool allow)
        {
            Speaker = speak;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((IntercomSpeakHandler)handler).OnSpeak(this);
        }
    }

    public class ChangeItemEvent : Event
    {
        public Inventory.SyncItemInfo OldItem { get; }
        public Inventory.SyncItemInfo NewItem { get; set; }
        public Player Player { get; }
        public bool Allow { get; set; }

        public ChangeItemEvent(Inventory.SyncItemInfo oldItem, Inventory.SyncItemInfo newItem, Player ply, bool allow)
        {
            OldItem = oldItem;
            NewItem = newItem;
            Player = ply;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((ChangeItemHandler)handler).OnChangeItem(this);
        }
    }

    public class LockerInteractEvent : Event
    {
        public Locker Locker { get; }
        public Player Player { get; }
        public bool Allow { get; set; }

        public LockerInteractEvent(Locker locker, Player ply, bool allow)
        {
            Locker = locker;
            Player = ply;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((LockerInteractHandler)handler).OnLockerInteract(this);
        }
    }

    public class PickupItemEvent : Event
    {
        public Pickup Item { get; }
        public Player Player { get; }
        public bool Allow { get; set; }

        public PickupItemEvent(Pickup item, Player ply, bool allow)
        {
            Item = item;
            Player = ply;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((PickupItemHandler)handler).OnPickupItem(this);
        }
    }

    public class PlaceBloodEvent : Event
    {
        public bool Allow { get; set; }
        public Vector3 Position { get; set; }
        public int Type { get; set; }

        public PlaceBloodEvent(bool allow, int type, Vector3 pos)
        {
            Allow = allow;
            Position = pos;
        }

        public override void Execute(EventHandler handler)
        {
            ((PlaceBloodHandler)handler).OnPlaceBlood(this);
        }
    }

    public class PlaceDecalEvent : Event
    { 
        public bool Allow { get; set; }
        public Vector3 Position { get; set; }

        public PlaceDecalEvent(bool allow, Vector3 pos)
        {
            Allow = allow;
            Position = pos;
        }

        public override void Execute(EventHandler handler)
        {
            ((PlaceDecalHandler)handler).OnPlaceDecal(this);
        }
    }

    public class BanEvent : Event
    {
        public Player Player { get; }
        public Player Issuer { get; }
        public string Reason { get; }
        public TimeSpan Expiration { get; set; }
        public TimeSpan Issuance { get; set; }
        public bool Allow { get; set; }

        public BanEvent(Player ply, Player issuer, string reason, long issuance, long expiery, bool allow)
        {
            Player = ply;
            Issuer = issuer;
            Reason = reason.IsEmpty() ? "No reason provided." : reason;
            Expiration = TimeSpan.FromTicks(expiery);
            Issuance = TimeSpan.FromTicks(issuance);
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((BanEventHandler)handler).OnBan(this);
        }
    }

    public class HandcuffEvent : Event
    {
        public Player Player { get; }
        public Player Cuffer { get; }
        public bool Allow { get; set; }

        public HandcuffEvent(Player ply, Player cuffer, bool allow)
        {
            Player = ply;
            Cuffer = cuffer;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((HandcuffHandler)handler).OnHandcuff(this);
        }
    }

    public class UncuffEvent : Event
    {
        public Player Player { get; }
        public Player Uncuffer { get; }
        public bool Allow { get; set; }

        public UncuffEvent(Player ply, Player uncuffer, bool allow)
        {
            Player = ply;
            Uncuffer = uncuffer;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((UncuffHandler)handler).OnUncuff(this);
        }
    }

    public class PlayerHurtEvent : Event
    {
        public Player Player { get; }
        public Player Attacker { get; }
        public PlayerStats.HitInfo HitInfo { get; set; }
        public bool Allow { get; set; }

        public PlayerHurtEvent(Player ply, Player attack, PlayerStats.HitInfo hit, bool allow)
        {
            Player = ply;
            Attacker = attack;
            HitInfo = hit;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((PlayerHurtHandler)handler).OnHurt(this);
        }
    }

    public class PlayerInteractEvent : Event
    {
        public Player Player { get; }
        public bool Allow { get; set; }

        public PlayerInteractEvent(Player ply, bool allow)
        {
            Player = ply;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((PlayerInteractHandler)handler).OnInteract(this);
        }
    }

    public class PlayerJoinEvent : Event
    {
        public Player Player { get; }

        public PlayerJoinEvent(Player ply)
        {
            Player = ply;
        }

        public override void Execute(EventHandler handler)
        {
            ((PlayerJoinHandler)handler).OnJoin(this);
        }
    }

    public class PlayerLeaveEvent : Event
    {
        public Player Player { get; }
        public bool DestroyHub { get; set; }

        public PlayerLeaveEvent(Player ply)
        {
            Player = ply;
            DestroyHub = true;
        }

        public override void Execute(EventHandler handler)
        {
            ((PlayerLeaveHandler)handler).OnLeave(this);
        }
    }

    public class PlayerSpawnEvent : Event
    {
        public Player Player { get; }
        public Vector3 Position { get; set; }
        public RoleType Role { get; set; }
        public bool Allow { get; set; }

        public PlayerSpawnEvent(Player ply, Vector3 pos, RoleType role, bool allow)
        {
            Player = ply;
            Position = pos;
            Role = role;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((PlayerSpawnHandler)handler).OnSpawn(this);
        }
    }

    public class WeaponReloadEvent : Event
    {
        public Player Player { get; }
        public WeaponType WeaponType { get; }
        public Inventory.SyncItemInfo Weapon { get; }
        public bool AnimationOnly { get; set; }
        public bool Allow { get; set; }

        public WeaponReloadEvent(Player ply, WeaponType weapon, bool anim, bool allow)
        {
            Player = ply;
            WeaponType = weapon;
            Weapon = ply.CurrentItem;
            AnimationOnly = anim;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((WeaponReloadHandler)handler).OnReload(this);
        }
    }

    public class PocketEscapeEvent : Event
    {
        public Player Player { get; }
        public Vector3 Position { get; set; }
        public bool Allow { get; set; }

        public PocketEscapeEvent(Player ply, Vector3 pos, bool allow)
        {
            Player = ply;
            Position = pos;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((PocketEscapeHandler)handler).OnEscape(this);
        }
    }

    public class PocketEnterEvent : Event
    {
        public Player Player { get; }
        public bool Hurt { get; set; }
        public bool Allow { get; set; }
        public float Damage { get; set; }

        public PocketEnterEvent(Player ply, bool damage, float dmg, bool allow)
        {
            Player = ply;
            Hurt = damage;
            Allow = allow;
            Damage = dmg;
        }

        public override void Execute(EventHandler handler)
        {
            ((PocketEnterHandler)handler).OnEnter(this);
        }
    }

    public class PocketHurtEvent : Event
    {
        public Player Player { get; }
        public bool Allow { get; set; }
        public float Damage { get; set; }

        public PocketHurtEvent(Player ply, bool allow, float dmg)
        {
            Player = ply;
            Allow = allow;
            Damage = dmg;
        }

        public override void Execute(EventHandler handler)
        {
            ((PocketHurtHandler)handler).OnPocketHurt(this);
        }
    }

    public class RemoteAdminCommandEvent : Event
    {
        public Player Issuer { get; }
        public string Command { get; }
        public string Response { get; set; }
        public bool Allow { get; set; }

        public RemoteAdminCommandEvent(Player ply, string cmd, string response, bool allow)
        {
            Issuer = ply;
            Command = cmd;
            Response = response;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((RemoteAdminEventHandler)handler).OnRemoteAdminCommand(this);
        }
    }

    public class RoundEndEvent : Event
    {
        public RoundSummary.LeadingTeam LeadingTeam { get; }
        public RoundSummary.SumInfo_ClassList ClassList { get; set; }
        public int TimeToRestart { get; set; }
        public bool Allow { get; set; }

        public RoundEndEvent(RoundSummary.LeadingTeam team, RoundSummary.SumInfo_ClassList classList, int toRestart, bool allow)
        {
            LeadingTeam = team;
            ClassList = classList;
            TimeToRestart = toRestart;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((RoundEndHandler)handler).OnRoundEnd(this);
        }
    }

    public class RoundStartEvent : Event
    {
        public override void Execute(EventHandler handler)
        {
            ((RoundStartHandler)handler).OnRoundStart(this);
        }
    }

    public class RoundRestartEvent : Event
    {
        public override void Execute(EventHandler handler)
        {
            ((RoundRestartHandler)handler).OnRoundRestart(this);
        }
    }

    public class SCP914UpgradeEvent : Event
    {
        public Scp914Machine Scp914 { get; }
        public List<Player> Players { get; }
        public List<Pickup> Items { get; }
        public Scp914Knob KnobSetting { get; set; }
        public bool Allow { get; set; }

        public SCP914UpgradeEvent(Scp914Machine machine, List<GameObject> objects, List<Pickup> items, Scp914Knob knob, bool allow)
        {
            Scp914 = machine;
            Players = objects.Select(h => h.GetPlayer()).ToList();
            Items = items;
            KnobSetting = knob;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((SCP914UpgradeHandler)handler).OnSCP914Upgrade(this);
        }
    }

    public class SCP096EnrageEvent : Event
    {
        public Scp096 Scp096 { get; }
        public Player Player { get; }
        public bool Allow { get; set; }

        public SCP096EnrageEvent(Player ply, bool allow)
        {
            Scp096 = ply.Hub.scpsController.CurrentScp as Scp096;
            Player = ply;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((SCP096EnrageHandler)handler).OnEnrage(this);
        }
    }

    public class SCP096CalmEvent : Event
    {
        public Scp096 Scp096 { get; }
        public Player Player { get; }
        public bool Allow { get; set; }

        public SCP096CalmEvent(Player ply, bool allow)
        {
            Scp096 = ply.Hub.scpsController.CurrentScp as Scp096;
            Player = ply;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((SCP096CalmHandler)handler).OnCalm(this);
        }
    }

    public class SCP106ContainEvent : Event
    {
        public Player Killer { get; }
        public Player SCP { get; }
        public bool Allow { get; set; }

        public SCP106ContainEvent(Player killer, Player ply, bool allow)
        {
            Killer = killer;
            SCP = ply;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((SCP106ContainHandler)handler).OnContain(this);
        }
    }

    public class SCP106CreatePortalEvent : Event
    {
        public Player Player { get; }
        public Vector3 Position { get; set; }
        public bool Allow { get; set; }

        public SCP106CreatePortalEvent(Player ply, Vector3 pos, bool allow)
        {
            Player = ply;
            Position = pos;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((SCP106CreatePortalHandler)handler).OnCreatePortal(this);
        }
    }

    public class SCP106TeleportEvent : Event
    {
        public Player Player { get; }
        public Vector3 OldPortal { get; }
        public Vector3 NewPosition{ get; set; }
        public bool Allow { get; set; }

        public SCP106TeleportEvent(Player ply, Vector3 old, Vector3 newPos, bool allow)
        {
            Player = ply;
            OldPortal = old;
            NewPosition = newPos;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((SCP106TeleportHandler)handler).OnTeleport(this);
        }
    }

    public class SCP914ActivateEvent : Event
    {
        public Player Player { get; }
        public float Time { get; set; }
        public bool Allow { get; set; }

        public SCP914ActivateEvent(Player ply, float time, bool allow)
        {
            Player = ply;
            Time = time;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((SCP914ActivateHandler)handler).OnActivate(this);
        }
    }

    public class SCP914ChangeKnobEvent : Event
    {
        public Player Player { get; }
        public Scp914Machine Scp914 { get; }
        public Scp914Knob KnobSetting { get; set; }
        public bool Allow { get; set; }

        public SCP914ChangeKnobEvent(Player ply, Scp914Machine scp, Scp914Knob knob, bool allow)
        {
            Player = ply;
            Scp914 = scp;
            KnobSetting = knob;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((SCP914ChangeKnobHandler)handler).OnChangeKnob(this);
        }
    }

    public class SetClassEvent : Event
    {
        public Player Player { get; }
        public RoleType Role { get; set; }
        public bool Allow { get; set; }

        public SetClassEvent(Player ply, RoleType role, bool allow)
        {
            Player = ply;
            Role = role;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((SetClassHandler)handler).OnSetClass(this);
        }
    }

    public class SetGroupEvent : Event
    {
        public Player Player { get; }
        public UserGroup Group { get; set; }
        public bool Allow { get; set; }

        public SetGroupEvent(Player ply, UserGroup group, bool allow)
        {
            Player = ply;
            Group = group;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((SetGroupHandler)handler).OnSetGroup(this);
        }
    }

    public class WeaponShootEvent : Event
    {
        public Player Player { get; }
        public GameObject Target { get; }
        public WeaponType WeaponType { get; }
        public Inventory.SyncItemInfo Weapon { get; }
        public Vector3 Direction { get; }
        public Vector3 SourcePos { get; }
        public Vector3 TargetPos { get; }
        public HitBoxType HitBoxType { get; }
        public bool Allow { get; set; }

        public WeaponShootEvent(Player ply, GameObject target, WeaponType weapon, bool allow, Vector3 dir, Vector3 sourcePos, Vector3 targetPos, HitBoxType hitBoxType)
        {
            Player = ply;
            Target = target;
            WeaponType = weapon;
            Weapon = ply.CurrentItem;
            Allow = allow;
            Direction = dir;
            SourcePos = sourcePos;
            TargetPos = targetPos;
            HitBoxType = hitBoxType;
        }

        public override void Execute(EventHandler handler)
        {
            ((WeaponShootHandler)handler).OnShoot(this);
        }
    }

    public class WeaponLateShootEvent : Event
    {
        public Player Player { get; }
        public GameObject Target { get; }
        public WeaponType WeaponType { get; }
        public Inventory.SyncItemInfo Weapon { get; }
        public bool Allow { get; set; }

        public WeaponLateShootEvent(Player ply, GameObject target, WeaponType weapon, bool allow)
        {
            Player = ply;
            Target = target;
            WeaponType = weapon;
            Weapon = ply.CurrentItem;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((WeaponLateShootHandler)handler).OnLateShoot(this);
        }
    }

    public class SpawnRagdollEvent : Event
    {
        public Player Owner { get; }
        public Ragdoll Ragdoll { get; }
        public Vector3 Velocity { get => Ragdoll.NetworkPlayerVelo; set => Ragdoll.NetworkPlayerVelo = value; }
        public bool Allow { get; set; }

        public SpawnRagdollEvent(Ragdoll ragdoll, bool allow)
        {
            Owner = ragdoll.Networkowner.PlayerId.GetPlayer();
            Ragdoll = ragdoll;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((SpawnRagdollHandler)handler).OnSpawnRagdoll(this);
        }
    }

    public class SyncDataEvent : Event
    {
        public Player Player { get; }
        public Vector2 Speed { get; }
        public byte CurrentAnimation { get; set; }
        public bool Allow { get; set; }

        public SyncDataEvent(Player ply, Vector2 speed, byte curAnim, bool allow)
        {
            Player = ply;
            Speed = speed;
            CurrentAnimation = curAnim;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((SyncDataHandler)handler).OnSyncData(this);
        }
    }

    public class TeamRespawnEvent : Event
    {
        public List<Player> Players { get; }
        public SpawnableTeamType Team { get; set; }
        public bool Allow { get; set; }

        public TeamRespawnEvent(List<GameObject> plys, SpawnableTeamType team, bool allow)
        {
            Players = plys.Select(h => Server.PlayerList.GetPlayer(h)).ToList();
            Team = team;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((TeamRespawnHandler)handler).OnTeamRespawn(this);
        }
    }

    public class TriggerTeslaEvent : Event
    {
        public Player Player { get; }
        public TeslaGate Tesla { get; }
        public bool Allow { get; set; }

        public TriggerTeslaEvent(Player ply, TeslaGate tesla, bool allow)
        {
            Player = ply;
            Tesla = tesla;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((TriggerTeslaHandler)handler).OnTriggerTesla(this);
        }
    }

    public class UseMedicalItemEvent : Event
    {
        public Player Player { get; }
        public ItemType Item { get; }
        public int HpToRecover { get; set; }
        public bool Allow { get; set; }

        public UseMedicalItemEvent(Player ply, ItemType item, int hp, bool allow)
        {
            Player = ply;
            Item = item;
            HpToRecover = hp;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((UseMedicalHandler)handler).OnUseMedicalItem(this);
        }
    }

    public class WaitingForPlayersEvent : Event
    {
        public override void Execute(EventHandler handler)
        {
            ((WaitingForPlayersHandler)handler).OnWaitingForPlayers(this);
        }
    }

    public class DetonationEvent : Event
    {
        public override void Execute(EventHandler handler)
        {
            ((WarheadDetonateHandler)handler).OnDetonate(this);
        }
    }

    public class WarheadCancelEvent : Event
    {
        public Player Player { get; }
        public float TimeLeft { get; set; }
        public bool Allow { get; set; }

        public WarheadCancelEvent(Player ply, float time, bool allow)
        {
            Player = ply;
            TimeLeft = time;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((WarheadCancelHandler)handler).OnCancel(this);
        }
    }

    public class WarheadStartEvent : Event
    {
        public Player Player { get; }
        public float TimeLeft { get; set; }
        public bool Allow { get; set; }

        public WarheadStartEvent(Player ply, float time, bool allow)
        {
            Player = ply;
            TimeLeft = time;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((WarheadStartHandler)handler).OnStart(this);
        }
    }

    public class WarheadKeycardAccessEvent : Event
    {
        public Player Player { get; }
        public bool Allow { get; set; }

        public WarheadKeycardAccessEvent(Player ply, bool allow)
        {
            Player = ply;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((WarheadKeycardAccessHandler)handler).OnAccess(this);
        }
    }

    public class SCP049RecallEvent : Event
    {
        public Player Player { get; }
        public Player RagdollOwner { get; }
        public Ragdoll Ragdoll { get; }
        public bool Allow { get; set; }

        public SCP049RecallEvent(Player ply, Ragdoll ragdoll, bool allow)
        {
            Player = ply;
            Ragdoll = ragdoll;
            RagdollOwner = ragdoll.Networkowner.PlayerId.GetPlayer();
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((SCP049RecallHandler)handler).OnRecall(this);
        }
    }

    public class SCP079GainExpEvent : Event
    {
        public Player Player { get; }
        public ExpGainType ExpGainType { get; }
        public float Experience { get; set; }
        public bool Allow { get; set; }

        public SCP079GainExpEvent(Player ply, ExpGainType gainType, float exp, bool allow)
        {
            Player = ply;
            ExpGainType = gainType;
            Experience = exp;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((SCP079GainExpHandler)handler).OnGainExp(this);
        }
    }

    public class SCP079GainLvlEvent : Event
    {
        public Player Player { get; }
        public int Level { get; set; }
        public bool Allow { get; set; }

        public SCP079GainLvlEvent(Player ply, int level, bool allow)
        {
            Player = ply;
            Level = level;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((SCP079GainLvlHandler)handler).OnGainLvl(this);
        }
    }

    public class SCP079InteractEvent : Event
    {
        public Player Player { get; }
        public Enums.Scp079Interaction Interaction { get; set; }
        public GameObject Target { get; }
        public float ExpCost { get; set; }
        public bool Allow { get; set; }

        public SCP079InteractEvent(Player ply, Scp079Interactable.InteractableType inter, GameObject target, float expCost, bool allow)
        {
            Player = ply;
            Interaction = ((Enums.Scp079Interaction)(int)inter);
            Target = target;
            ExpCost = expCost;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((SCP079InteractHandler)handler).OnInteract(this);
        }
    }

    public class ServerCommandEvent : Event
    {
        public ServerConsoleSender Sender { get; }
        public string Command { get; }
        public string Response { get; set; }
        public bool Allow { get; set; }

        public ServerCommandEvent(string cmd, string res, bool all)
        {
            Sender = ServerConsole._scs;
            Command = cmd;
            Response = res;
            Allow = all;
        }

        public override void Execute(EventHandler handler)
        {
            ((ServerCommandHandler)handler).OnServerCommand(this);
        }
    }

    public class RoundShowSummaryEvent : Event
    {
        public RoundSummary.SumInfo_ClassList ClassListStart { get; set; }
        public RoundSummary.SumInfo_ClassList ClassListEnd { get; set; }
        public RoundSummary.LeadingTeam Team { get; set; }
        public bool Allow { get; set; }

        public RoundShowSummaryEvent(RoundSummary.SumInfo_ClassList start, RoundSummary.SumInfo_ClassList end, RoundSummary.LeadingTeam team, bool allow)
        {
            ClassListStart = start;
            ClassListEnd = end;
            Team = team;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((RoundShowSummaryHandler)handler).OnShowSummary(this);
        }
    }

    public class ConsoleAddLogEvent : Event
    {
        public string Text { get; }

        public ConsoleAddLogEvent(string msg)
        {
            Text = msg;
        }

        public override void Execute(EventHandler handler)
        {
            ((ConsoleAddLogEventHandler)handler).OnAddLog(this);
        }
    }

    public class PlayerDieEvent : Event
    {
        public Player Target { get; }
        public Player Attacker { get; }
        public PlayerStats.HitInfo HitInfo { get; set; }
        public bool Allow { get; set; }
        public bool SpawnRagdoll { get; set; }

        public PlayerDieEvent(Player target, Player attacker, PlayerStats.HitInfo info, bool allow)
        {
            Target = target;
            Attacker = attacker;
            HitInfo = info;
            Allow = allow;
            SpawnRagdoll = ConfigManager.SpawnRagdolls;
        }

        public override void Execute(EventHandler handler)
        {
            ((PlayerDieEventHandler)handler).OnPlayerDie(this);
        }
    }

    public class Scp096AddTargetEvent : Event
    {
        public Player Scp { get; }
        public Player Target { get; }
        public bool Allow { get; set; }

        public Scp096AddTargetEvent(Player scp, Player target, bool allow)
        {
            Scp = scp;
            Target = target;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((Scp096AddTargetHandler)handler).OnScp096AddTarget(this);
        }
    }

    public class SpawnItemEvent : Event
    {
        public Pickup Pickup { get; }
        public ItemType ItemId { get; set; }
        public float Durability { get; set; }
        public Player Owner { get; set; }
        public Pickup.WeaponModifiers WeaponModifiers { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public bool Allow { get; set; }

        public SpawnItemEvent(Pickup pickup, ItemType id, float dur, Player ownr, Pickup.WeaponModifiers mods, Vector3 pos, Quaternion rot, bool allow)
        {
            Pickup = pickup;
            ItemId = id;
            Durability = dur;
            Owner = ownr;
            WeaponModifiers = mods;
            Position = pos;
            Rotation = rot;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((SpawnItemEventHandler)handler).OnSpawnItem(this);
        }
    }

    public class Scp914UpgradeItemEvent : Event
    {
        public ItemType Input { get; }
        public ItemType Output { get; set; }
        public bool Allow { get; set; }

        public Scp914UpgradeItemEvent(ItemType input, bool allow)
        {
            Input = input;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((Scp914UpgradeItemHandler)handler).OnScp914UpgradeItem(this);
        }
    }

    public class Scp914UpgradePickupEvent : Event
    {
        public Pickup Input { get; }
        public ItemType Output { get; set; }
        public bool Allow { get; set; }

        public Scp914UpgradePickupEvent(Pickup input)
        {
            Input = input;
        }

        public override void Execute(EventHandler handler)
        {
            ((Scp914UpgradePickupHandler)handler).OnUpgradePickup(this);
        }
    }

    public class Scp914UpgradePlayerEvent : Event
    {
        public Player Player { get; set; }
        public bool Allow { get; set; }

        public Scp914UpgradePlayerEvent(Player player, bool allow)
        {
            Player = player;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((Scp914UpgradePlayerHandler)handler).OnScp914UpgradePlayer(this);
        }
    }

    public class PlayerUseLockerEvent : Event
    {
        public Player Player { get; }
        public Locker Locker { get; }
        public string AccessToken { get; set; }
        public bool Allow { get; set; }

        public PlayerUseLockerEvent(Player player, Locker locker, string token, bool allow)
        {
            Player = player;
            Locker = locker;
            AccessToken = token;
            Allow = allow;
        }

        public override void Execute(EventHandler handler)
        {
            ((PlayerUseLockerHandler)handler).OnUseLocker(this);
        }
    }

    public class PlayerSwitchLeverEvent : Event
    {
        public Player Player { get; }
        public WarheadLeverStatus CurrentState { get; }
        public WarheadLeverStatus NewState { get; set; }
        public bool Allow { get; set; }

        public PlayerSwitchLeverEvent(Player p, WarheadLeverStatus s, WarheadLeverStatus c, bool a)
        {
            Player = p;
            CurrentState = c;
            NewState = s;
            Allow = a;
        }

        public override void Execute(EventHandler handler)
        {
            ((PlayerSwitchLeverHandler)handler).OnSwitchLever(this);
        }
    }

    public class GenerateSeedEvent : Event
    {
        public int Seed { get; set; }

        public GenerateSeedEvent(int seed) => Seed = seed;

        public override void Execute(EventHandler handler)
        {
            ((GenerateSeedHandler)handler).OnGenerateSeed(this);
        }
    }

    public class BlinkEvent : Event
    {
        public Player Scp { get; }
        public List<Player> Targets { get; }

        public BlinkEvent(Player scp, List<Player> plys)
        {
            Scp = scp;
            Targets = plys;
        }

        public override void Execute(EventHandler handler)
        {
            ((BlinkEventHandler)handler).OnBlink(this);
        }
    }
}

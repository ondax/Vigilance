using Vigilance.Events;

namespace Vigilance.EventHandlers
{
    public interface AnnounceDecontaminationHandler : EventHandler
    {
        void OnAnnounceDecontamination(AnnounceDecontaminationEvent ev);
    }

    public interface AnnounceNTFEntranceHandler : EventHandler
    {
        void OnAnnounceEntrance(AnnounceNTFEntranceEvent ev);
    }

    public interface AnnounceSCPTerminationHandler : EventHandler
    {
        void OnAnnounceTermination(AnnounceSCPTerminationEvent ev);
    }

    public interface CancelMedicalItemHandler : EventHandler
    {
        void OnCancelMedical(CancelMedicalItemEvent ev);
    }

    public interface GlobalReportHandler : EventHandler
    {
        void OnGlobalReport(GlobalReportEvent ev);
    }

    public interface LocalReportHandler : EventHandler
    {
        void OnLocalReport(LocalReportEvent ev);
    }

    public interface CheckEscapeHandler : EventHandler
    {
        void OnCheckEscape(CheckEscapeEvent ev);
    }

    public interface CheckRoundEndHandler : EventHandler
    {
        void OnCheckRoundEnd(CheckRoundEndEvent ev);
    }

    public interface ConsoleCommandHandler : EventHandler
    {
        void OnConsoleCommand(ConsoleCommandEvent ev);
    }

    public interface DecontaminationHandler : EventHandler
    {
        void OnDecontamination(DecontaminationEvent ev);
    }

    public interface DoorInteractHandler : EventHandler
    {
        void OnDoorInteract(DoorInteractEvent ev);
    }

    public interface DropItemHandler : EventHandler
    {
        void OnDropItem(DropItemEvent ev);
    }

    public interface ElevatorInteractHandler : EventHandler
    {
        void OnElevatorInteract(ElevatorInteractEvent ev);
    }

    public interface FemurEnterHandler : EventHandler
    {
        void OnFemurEnter(FemurEnterEvent ev);
    }

    public interface GeneratorInsertHandler : EventHandler
    {
        void OnGeneratorInsert(GeneratorInsertEvent ev);
    }

    public interface GeneratorEjectHandler : EventHandler
    {
        void OnGeneratorEject(GeneratorEjectEvent ev);
    }

    public interface GeneratorUnlockHandler : EventHandler
    {
        void OnGeneratorUnlock(GeneratorUnlockEvent ev);
    }

    public interface GeneratorOpenHandler : EventHandler
    {
        void OnGeneratorOpen(GeneratorOpenEvent ev);
    }

    public interface GeneratorCloseHandler : EventHandler
    {
        void OnGeneratorClose(GeneratorCloseEvent ev);
    }

    public interface GeneratorFinishHandler : EventHandler
    {
        void OnGeneratorFinish(GeneratorFinishEvent ev);
    }

    public interface ThrowGrenadeHandler : EventHandler
    {
        void OnThrowGrenade(ThrowGrenadeEvent ev);
    }

    public interface IntercomSpeakHandler : EventHandler
    {
        void OnSpeak(IntercomSpeakEvent ev);
    }

    public interface ChangeItemHandler : EventHandler
    {
        void OnChangeItem(ChangeItemEvent ev);
    }

    public interface LockerInteractHandler : EventHandler
    {
        void OnLockerInteract(LockerInteractEvent ev);
    }

    public interface PickupItemHandler : EventHandler
    {
        void OnPickupItem(PickupItemEvent ev);
    }

    public interface PlaceBloodHandler : EventHandler
    {
        void OnPlaceBlood(PlaceBloodEvent ev);
    }

    public interface PlaceDecalHandler : EventHandler
    {
        void OnPlaceDecal(PlaceDecalEvent ev);
    }

    public interface BanEventHandler : EventHandler
    {
        void OnBan(BanEvent ev);
    }

    public interface HandcuffHandler : EventHandler
    {
        void OnHandcuff(HandcuffEvent ev);
    }

    public interface UncuffHandler : EventHandler
    {
        void OnUncuff(UncuffEvent ev);
    }

    public interface PlayerHurtHandler : EventHandler
    {
        void OnHurt(PlayerHurtEvent ev);
    }

    public interface PlayerInteractHandler : EventHandler
    {
        void OnInteract(PlayerInteractEvent ev);
    }

    public interface PlayerJoinHandler : EventHandler
    {
        void OnJoin(PlayerJoinEvent ev);
    }

    public interface PlayerLeaveHandler : EventHandler
    {
        void OnLeave(PlayerLeaveEvent ev);
    }

    public interface WeaponReloadHandler : EventHandler
    {
        void OnReload(WeaponReloadEvent ev);
    }

    public interface PlayerSpawnHandler : EventHandler
    {
        void OnSpawn(PlayerSpawnEvent ev);
    }

    public interface PocketEscapeHandler : EventHandler
    {
        void OnEscape(PocketEscapeEvent ev);
    }

    public interface PocketEnterHandler : EventHandler
    {
        void OnEnter(PocketEnterEvent ev);
    }

    public interface PocketHurtHandler : EventHandler
    {
        void OnPocketHurt(PocketHurtEvent ev);
    }

    public interface RemoteAdminEventHandler : EventHandler
    {
        void OnRemoteAdminCommand(RemoteAdminCommandEvent ev);
    }

    public interface RoundEndHandler : EventHandler
    {
        void OnRoundEnd(RoundEndEvent ev);
    }

    public interface RoundStartHandler : EventHandler
    {
        void OnRoundStart(RoundStartEvent ev);
    }

    public interface RoundRestartHandler : EventHandler
    {
        void OnRoundRestart(RoundRestartEvent ev);
    }

    public interface SCP914UpgradeHandler : EventHandler
    {
        void OnSCP914Upgrade(SCP914UpgradeEvent ev);
    }

    public interface SCP096EnrageHandler : EventHandler
    {
        void OnEnrage(SCP096EnrageEvent ev);
    }

    public interface SCP096CalmHandler : EventHandler
    {
        void OnCalm(SCP096CalmEvent ev);
    }

    public interface SCP106ContainHandler : EventHandler
    {
        void OnContain(SCP106ContainEvent ev);
    }

    public interface SCP106CreatePortalHandler : EventHandler
    {
        void OnCreatePortal(SCP106CreatePortalEvent ev);
    }

    public interface SCP106TeleportHandler : EventHandler
    {
        void OnTeleport(SCP106TeleportEvent ev);
    }

    public interface SCP914ActivateHandler : EventHandler
    {
        void OnActivate(SCP914ActivateEvent ev);
    }

    public interface SCP914ChangeKnobHandler : EventHandler
    {
        void OnChangeKnob(SCP914ChangeKnobEvent ev);
    }

    public interface SetClassHandler : EventHandler
    {
        void OnSetClass(SetClassEvent ev);
    }

    public interface SetGroupHandler : EventHandler
    {
        void OnSetGroup(SetGroupEvent ev);
    }

    public interface WeaponShootHandler : EventHandler
    {
        void OnShoot(WeaponShootEvent ev);
    }

    public interface WeaponLateShootHandler : EventHandler
    {
        void OnLateShoot(WeaponLateShootEvent ev);
    }

    public interface SpawnRagdollHandler : EventHandler
    {
        void OnSpawnRagdoll(SpawnRagdollEvent ev);
    }

    public interface SyncDataHandler : EventHandler
    {
        void OnSyncData(SyncDataEvent ev);
    }

    public interface TeamRespawnHandler : EventHandler
    {
        void OnTeamRespawn(TeamRespawnEvent ev);
    }

    public interface TriggerTeslaHandler : EventHandler
    {
        void OnTriggerTesla(TriggerTeslaEvent ev);
    }

    public interface UseMedicalHandler : EventHandler
    {
        void OnUseMedicalItem(UseMedicalItemEvent ev);
    }

    public interface WaitingForPlayersHandler : EventHandler
    {
        void OnWaitingForPlayers(WaitingForPlayersEvent ev);
    }

    public interface WarheadDetonateHandler : EventHandler
    {
        void OnDetonate(DetonationEvent ev);
    }

    public interface WarheadCancelHandler : EventHandler
    {
        void OnCancel(WarheadCancelEvent ev);
    }

    public interface WarheadStartHandler : EventHandler
    {
        void OnStart(WarheadStartEvent ev);
    }

    public interface WarheadKeycardAccessHandler : EventHandler
    {
        void OnAccess(WarheadKeycardAccessEvent ev);
    }

    public interface SCP049RecallHandler : EventHandler
    {
        void OnRecall(SCP049RecallEvent ev);
    }

    public interface SCP079GainExpHandler : EventHandler
    {
        void OnGainExp(SCP079GainExpEvent ev);
    }

    public interface SCP079GainLvlHandler : EventHandler
    {
        void OnGainLvl(SCP079GainLvlEvent ev);
    }

    public interface SCP079InteractHandler : EventHandler
    {
        void OnInteract(SCP079InteractEvent ev);
    }

    public interface ServerCommandHandler : EventHandler
    {
        void OnServerCommand(ServerCommandEvent ev);
    }

    public interface RoundShowSummaryHandler : EventHandler
    {
        void OnShowSummary(RoundShowSummaryEvent ev);
    }

    public interface ConsoleAddLogEventHandler : EventHandler
    {
        void OnAddLog(ConsoleAddLogEvent ev);
    }

    public interface PlayerDieEventHandler : EventHandler
    {
        void OnPlayerDie(PlayerDieEvent ev);
    }

    public interface DroppedItemHandler : EventHandler
    {
        void OnItemDropped(DroppedItemEvent ev);
    }

    public interface Scp096AddTargetHandler : EventHandler
    {
        void OnScp096AddTarget(Scp096AddTargetEvent ev);
    }

    public interface SpawnItemEventHandler : EventHandler
    {
        void OnSpawnItem(SpawnItemEvent ev);
    }

    public interface Scp914UpgradeItemHandler : EventHandler
    {
        void OnScp914UpgradeItem(Scp914UpgradeItemEvent ev);
    }

    public interface Scp914UpgradePlayerHandler : EventHandler
    {
        void OnScp914UpgradePlayer(Scp914UpgradePlayerEvent ev);
    }

    public interface PlayerUseLockerHandler : EventHandler
    {
        void OnUseLocker(PlayerUseLockerEvent ev);
    }

    public interface Scp914UpgradePickupHandler : EventHandler
    {
        void OnUpgradePickup(Scp914UpgradePickupEvent ev);
    }

    public interface PlayerSwitchLeverHandler : EventHandler
    {
        void OnSwitchLever(PlayerSwitchLeverEvent ev);
    }

    public interface GenerateSeedHandler : EventHandler
    {
        void OnGenerateSeed(GenerateSeedEvent ev);
    }

    public interface BlinkEventHandler : EventHandler
    {
        void OnBlink(BlinkEvent ev);
    }
}

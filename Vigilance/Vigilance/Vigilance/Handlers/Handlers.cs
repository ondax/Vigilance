using System;
using System.Runtime.CompilerServices;
using Vigilance.Events;

namespace Vigilance.Handlers
{
    public interface BanEventHandler
    {
        void OnBan(BanEvent ev);
    }

    public interface CheaterReportEventHandler
    {
        void OnReport(CheaterReportEvent ev);
    }

    public interface Contain106EventHandler
    {
        void OnContain106(Contain106Event ev);
    }

    public interface DoorInteractEventHandler
    {
        void OnInteract(DoorInteractEvent ev);
    }

    public interface ElevatorInteractEventHandler
    {
        void OnElevatorUse(ElevatorInteractEvent ev);
    }

    public interface LCZDecontaminateEventHandler
    {
        void OnDecontaminate(LCZDecontaminateEvent ev);
    }

    public interface LureEventHandler
    {
        void OnLure(LureEvent ev);
    }
    
    public interface PlayerDeathEventHandler
    {
        void OnPlayerDeath(PlayerDeathEvent ev);
    }

    public interface PlayerHurtEventHandler
    {
        void OnPlayerHurt(PlayerHurtEvent ev);
    }

    public interface PlayerJoinEventHandler
    {
        void OnPlayerJoin(PlayerJoinEvent ev);
    }

    public interface PlayerLeaveEventHandler
    {
        void OnPlayerLeave(PlayerLeaveEvent ev);
    }

    public interface PlayerSpawnEventHandler
    {
        void OnSpawn(PlayerSpawnEvent ev);
    }

    public interface PocketDieEventHandler
    {
        void OnPocketDie(PocketDieEvent ev);
    }

    public interface RACommandEventHandler
    {
        void OnCommand(RACommandEvent ev);
    }

    public interface RoundEndEventHandler
    {
        void OnRoundEnd(RoundEndEvent ev);
    }

    public interface RoundRestartEventHandler
    {
        void OnRoundRestart(RoundRestartEvent ev);
    }

    public interface RoundStartEventHandler
    {
        void OnRoundStart(RoundStartEvent ev);
    }

    public interface TeamRespawnEventHandler
    {
        void OnTeamRespawn(TeamRespawnEvent ev);
    }

    public interface TeslaTriggerEventHandler
    {
        void OnTrigger(TeslaTriggerEvent ev);
    }

    public interface WaitingForPlayersEventHandler
    {
        void OnWaitingForPlayers(WaitingForPlayersEvent ev);
    }

    public interface WarheadDetonateEventHandler
    {
        void OnDetonate(WarheadDetonateEvent ev);
    }

    public interface WarheadStartEventHandler
    {
        void OnStart(WarheadStartEvent ev);
    }

    public interface WarheadStopEventHandler
    {
        void OnStop(WarheadStopEvent ev);
    }

    public interface UseMedicalItemEventHandler
    {
        void OnUse(UseMedicalItemEvent ev);
    }
}

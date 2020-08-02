using System;
using System.Runtime.CompilerServices;
using Vigilance.API.Handlers;
using Vigilance.Events;

namespace Vigilance.Handlers
{
    public interface BanEventHandler : Handler
    {
        void OnBan(BanEvent ev);
    }

    public interface CheaterReportEventHandler : Handler
    {
        void OnReport(CheaterReportEvent ev);
    }

    public interface Contain106EventHandler : Handler
    {
        void OnContain106(Contain106Event ev);
    }

    public interface DoorInteractEventHandler : Handler
    {
        void OnInteract(DoorInteractEvent ev);
    }

    public interface ElevatorInteractEventHandler : Handler
    {
        void OnElevatorUse(ElevatorInteractEvent ev);
    }

    public interface LCZDecontaminateEventHandler : Handler
    {
        void OnDecontaminate(LCZDecontaminateEvent ev);
    }

    public interface LureEventHandler : Handler
    {
        void OnLure(LureEvent ev);
    }
    
    public interface PlayerDeathEventHandler : Handler
    {
        void OnPlayerDeath(PlayerDeathEvent ev);
    }

    public interface PlayerHurtEventHandler : Handler
    {
        void OnPlayerHurt(PlayerHurtEvent ev);
    }

    public interface PlayerJoinEventHandler : Handler
    {
        void OnPlayerJoin(PlayerJoinEvent ev);
    }

    public interface PlayerLeaveEventHandler : Handler
    {
        void OnPlayerLeave(PlayerLeaveEvent ev);
    }

    public interface PlayerSpawnEventHandler : Handler
    {
        void OnSpawn(PlayerSpawnEvent ev);
    }

    public interface PocketDieEventHandler : Handler
    {
        void OnPocketDie(PocketDieEvent ev);
    }

    public interface RACommandEventHandler : Handler
    {
        void OnCommand(RACommandEvent ev);
    }

    public interface RoundEndEventHandler : Handler
    {
        void OnRoundEnd(RoundEndEvent ev);
    }

    public interface RoundRestartEventHandler : Handler
    {
        void OnRoundRestart(RoundRestartEvent ev);
    }

    public interface RoundStartEventHandler : Handler
    {
        void OnRoundStart(RoundStartEvent ev);
    }

    public interface TeamRespawnEventHandler : Handler
    {
        void OnTeamRespawn(TeamRespawnEvent ev);
    }

    public interface TeslaTriggerEventHandler : Handler
    {
        void OnTrigger(TeslaTriggerEvent ev);
    }

    public interface WaitingForPlayersEventHandler : Handler
    {
        void OnWaitingForPlayers(WaitingForPlayersEvent ev);
    }

    public interface WarheadDetonateEventHandler : Handler
    {
        void OnDetonate(WarheadDetonateEvent ev);
    }

    public interface WarheadStartEventHandler : Handler
    {
        void OnStart(WarheadStartEvent ev);
    }

    public interface WarheadStopEventHandler : Handler
    {
        void OnStop(WarheadStopEvent ev);
    }

    public interface UseMedicalItemEventHandler : Handler
    {
        void OnUse(UseMedicalItemEvent ev);
    }
}

using Vigilance.Enums;
using System.Linq;
using System;
using Respawning;
using Respawning.NamingRules;
using System.Collections.Generic;

namespace Vigilance.API
{
    public static class Round
    {
        private static RoundInfo _info;
        private static float _sprintSpeed = 0f;
        private static float _walkSpeed = 0f;

        public static bool RoundLock { get => Server.RoundLock; set => Server.RoundLock = value; }
        public static bool LobbyLock { get => Server.LobbyLock; set => Server.LobbyLock = value; }
        public static bool FriendlyFire { get => ServerConsole.FriendlyFire; set => ServerConsole.FriendlyFire = value; }
        public static RoundState CurrentState { get; set; } = RoundState.Undefined;
        public static RoundInfo Info { get => _info == null ? _info = new RoundInfo() : _info; }

        public static void Start() => CharacterClassManager.ForceRoundStart();
        public static void End()
        {
            RoundLock = false;
            LobbyLock = false;
            RoundSummary.singleton.ForceEnd();
        }
        public static void Restart() => Environment.Cache.LocalStats.Roundrestart();

        public static void AddUnit(string unit, SpawnableTeamType teamType = SpawnableTeamType.NineTailedFox)
        {
            SyncUnit syncUnit = new SyncUnit()
            {
                SpawnableTeam = (byte)teamType,
                UnitName = unit
            };
            RespawnManager.Singleton.NamingManager.AllUnitNames.Add(syncUnit);
        }

        public static void SetSpeed(float value)
        {
            if (_sprintSpeed == 0f)
                _sprintSpeed = ServerConfigSynchronizer.Singleton.NetworkHumanSprintSpeedMultiplier;
            if (_walkSpeed == 0f)
                _walkSpeed = ServerConfigSynchronizer.Singleton.NetworkHumanWalkSpeedMultiplier;
            ServerConfigSynchronizer.Singleton.NetworkHumanSprintSpeedMultiplier = value;
            ServerConfigSynchronizer.Singleton.NetworkHumanWalkSpeedMultiplier = value / 1.5f;
        }

        public static void ResetSpeed()
        {
            ServerConfigSynchronizer.Singleton.NetworkHumanWalkSpeedMultiplier = _walkSpeed;
            ServerConfigSynchronizer.Singleton.NetworkHumanSprintSpeedMultiplier = _sprintSpeed;
        }
    }

    public class RoundInfo
    {
        public RoundSummary.SumInfo_ClassList ClassListOnStart => RoundSummary.singleton.classlistStart;
        public int ChaosInsurgents => CountRole(RoleType.ChaosInsurgency);
        public int Class_Ds => CountRole(RoleType.ClassD);
        public int Scientists => CountRole(RoleType.Scientist);
        public int MTF => CountTeam(TeamType.NineTailedFox);
        public int TotalSCPs => CountTeam(TeamType.SCP);
        public int TotalSCPsExceptZombies => Server.PlayerList.Players.Values.Where(h => h.Team == TeamType.SCP && h.Role != RoleType.Scp0492).Count();
        public int Seconds => RoundSummary.roundTime;
        public int Minutes => RoundSummary.roundTime / 60;
        public TimeSpan Time => GameCore.RoundStart.RoundLenght;
        public int WarheadKills => AlphaWarheadController.Host.warheadKills;
        public int TotalKills => RoundSummary.Kills;
        public int KillsByGrenade => RoundSummary.kills_by_frag;
        public int KillsBySCP => RoundSummary.kills_by_scp;
        public int ChangedToZombies => RoundSummary.changed_into_zombies;
        public int EscapedClassDs => RoundSummary.escaped_ds;
        public int EscapedScientists => RoundSummary.escaped_scientists;
        public List<SyncUnit> NtfUnits => RespawnManager.Singleton.NamingManager.AllUnitNames.ToList();

        public int CountTeam(TeamType team) => Server.PlayerList.GetPlayers(team).Count;
        public int CountRole(RoleType role) => Server.PlayerList.GetPlayers(role).Count;
    }
}

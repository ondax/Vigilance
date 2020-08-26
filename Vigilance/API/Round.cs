using Vigilance.Enums;
using System.Linq;
using System;

namespace Vigilance.API
{
    public static class Round
    {
        private static RoundInfo _info;
        public static bool RoundLock { get => Server.RoundLock; set => Server.RoundLock = value; }
        public static bool LobbyLock { get => Server.LobbyLock; set => Server.LobbyLock = value; }
        public static RoundInfo Info
        {
            get
            {
                if (_info == null)
                    _info = new RoundInfo();
                return _info;
            }
        }

        public static void Start() => CharacterClassManager.ForceRoundStart();
        public static void End() => RoundSummary.singleton.ForceEnd();
        public static void ShowSummary(RoundSummary.LeadingTeam team = RoundSummary.LeadingTeam.Draw) => RoundSummary.singleton.CallRpcShowRoundSummary(Info.ClassListOnStart, ClassListBuilder.Build(), team, Info.EscapedClassDs, Info.EscapedScientists, Info.KillsBySCP, Info.Class_Ds);
        public static void Restart() => Server.Host.GetComponent<PlayerStats>().Roundrestart();
    }

    public class RoundInfo
    {
        public RoundSummary.SumInfo_ClassList ClassListOnStart => RoundSummary.singleton.classlistStart;
        public int ChaosInsurgents => CountRole(RoleType.ChaosInsurgency);
        public int Class_Ds => CountRole(RoleType.ClassD);
        public int Scientists => CountRole(RoleType.Scientist);
        public int MTF => CountTeam(TeamType.NineTailedFox);
        public int TotalSCPs => CountTeam(TeamType.SCP);
        public int TotalSCPsExceptZombies => Server.PlayerList.PlayersDict.Values.Where(h => h.Team == TeamType.SCP && h.Role != RoleType.Scp0492).Count();
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

        public int CountTeam(TeamType team) => Server.PlayerList.GetPlayers(team).Count;
        public int CountRole(RoleType role) => Server.PlayerList.GetPlayers(role).Count;
    }

    public static class ClassListBuilder
    {
        public static RoundSummary.SumInfo_ClassList Build()
        {
            RoundSummary.SumInfo_ClassList classList = new RoundSummary.SumInfo_ClassList()
            {
                chaos_insurgents = Round.Info.ChaosInsurgents,
                class_ds = Round.Info.Class_Ds,
                mtf_and_guards = Round.Info.MTF,
                scientists = Round.Info.Scientists,
                scps_except_zombies = Round.Info.TotalSCPsExceptZombies,
                time = Round.Info.Seconds,
                warhead_kills = Round.Info.WarheadKills,
                zombies = Round.Info.ChangedToZombies
            };
            return classList;
        }
    }
}

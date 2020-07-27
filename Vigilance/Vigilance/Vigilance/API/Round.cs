using System;
using GameCore;
using Vigilance.API.Extensions;

namespace Vigilance.API
{
	public static class Round
	{
		public static int Duration
		{
			get
			{
				return RoundSummary.roundTime;
			}
		}

		public static TimeSpan Time
		{
			get
			{
				return new TimeSpan(0, 0, Round.Duration);
			}
		}

		public static DateTime StartTime
		{
			get
			{
				return DateTime.Now - Round.Time;
			}
		}

		public static bool IsStarted
		{
			get
			{
				return Round.Time > TimeSpan.Zero;
			}
		}

		public static bool RoundLock
		{
			get
			{
				return RoundSummary.RoundLock;
			}
			set
			{
				RoundSummary.RoundLock = value;
			}
		}

		public static bool LobbyLock
		{
			get
			{
				return RoundStart.LobbyLock;
			}
			set
			{
				RoundStart.LobbyLock = value;
			}
		}

		public static int ClassDAlive
		{
			get
			{
				return RoleType.ClassD.GetPlayers().Count;
			}
		}

		public static int ScientistsAlive
		{
			get
			{
				return RoleType.Scientist.GetPlayers().Count;
			}
		}

		public static int GuardsAlive
		{
			get
			{
				return RoleType.FacilityGuard.GetPlayers().Count;
			}
		}

		public static int ChaosAlive
		{
			get
			{
				return RoleType.ChaosInsurgency.GetPlayers().Count;
			}
		}

		public static int NTFCadetsAlive
		{
			get
			{
				return RoleType.NtfCadet.GetPlayers().Count;
			}
		}

		public static int NTFLieutenantsAlive
		{
			get
			{
				return RoleType.NtfLieutenant.GetPlayers().Count;
			}
		}

		public static int NTFCommandersAlive
		{
			get
			{
				return RoleType.NtfCommander.GetPlayers().Count;
			}
		}

		public static int Spectators
		{
			get
			{
				return RoleType.Spectator.GetPlayers().Count;
			}
		}

		public static int DeathCount
		{
			get
			{
				return RoundSummary.Kills;
			}
		}

		public static int WarheadKills
		{
			get
			{
				return AlphaWarheadController.Host.warheadKills;
			}
		}

		public static int SCPKills
		{
			get
			{
				return RoundSummary.kills_by_scp;
			}
		}

		public static int FragKills
		{
			get
			{
				return RoundSummary.kills_by_frag;
			}
		}

		public static int EscapedClassDs
		{
			get
			{
				return RoundSummary.escaped_ds;
			}
		}

		public static int EscapedScientists
		{
			get
			{
				return RoundSummary.escaped_scientists;
			}
		}

		public static int Zombies
		{
			get
			{
				return RoundSummary.changed_into_zombies;
			}
		}

		public static bool IsInProgress
		{
			get
			{
				return RoundSummary.RoundInProgress();
			}
		}

		public static RoundSummary Summary
		{
			get
			{
				return RoundSummary.singleton;
			}
		}

		public static void Restart()
		{
			PlayerStats component = Server.Host.GetComponent<PlayerStats>();
			if (component == null)
			{
				return;
			}
			component.Roundrestart();
		}

		public static void ShowSummary()
		{
			RoundSummary.SumInfo_ClassList list_finish = new RoundSummary.SumInfo_ClassList
			{
				chaos_insurgents = Round.ChaosAlive,
				class_ds = Round.ClassDAlive,
				scientists = Round.ScientistsAlive,
				time = Round.Duration,
				warhead_kills = Round.WarheadKills,
				zombies = Round.Zombies
			};
			RoundSummary.singleton.CallRpcShowRoundSummary(RoundSummary.singleton.classlistStart, list_finish, RoundSummary.LeadingTeam.Draw, Round.EscapedClassDs, Round.EscapedScientists, Round.SCPKills, Round.ClassDAlive);
		}

		public static void DimScreen()
		{
			RoundSummary.singleton.CallRpcDimScreen();
		}

		public static void Start()
		{
			RoundSummary.singleton.Start();
		}

		public static void End()
        {
			RoundSummary.singleton.ForceEnd();
        }
	}
}

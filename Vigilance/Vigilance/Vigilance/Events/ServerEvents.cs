using System.Linq;
using Vigilance.API;
using Vigilance.API.Events;
using Vigilance.API.Extensions;
using Vigilance.API.Handlers;
using Vigilance.Handlers;

namespace Vigilance.Events
{
	public class BanEvent : Event
	{
		public Player Player { get; set; }
		public Player Admin { get; set; }
		public int Duration { get; set; }
		public string Reason { get; set; }
		public bool Allow { get; set; }
		public BanEvent(Player player, Player admin, int duration, string reason, bool allow)
		{
			this.Player = player;
			this.Admin = admin;
			this.Duration = duration;
			this.Reason = reason;
			this.Allow = allow;
		}

		public override void ExecuteHandler(Handler handler)
		{
			((BanEventHandler)handler).OnBan(this);
		}
	}

	public class Contain106Event : PlayerEvent
	{
		public Player SCP106 => RoleType.Scp106.GetPlayers().FirstOrDefault();
		public bool Allow { get; set; }
		public Contain106Event(Player player, bool allow) : base(player)
		{
			this.Allow = allow;
		}

		public override void ExecuteHandler(Handler handler)
		{
			((Contain106EventHandler)handler).OnContain106(this);
		}
	}

	public class CheaterReportEvent : Event
	{
		public Player Reporter { get; set; }
		public Player Reported { get; set; }
		public string Reason { get; set; }
		public bool Allow { get; set; }
		public CheaterReportEvent(Player reporter, Player reported, string reason, bool allow)
		{
			this.Allow = allow;
			this.Reported = reported;
			this.Reporter = reporter;
			this.Reason = reason;
		}

		public override void ExecuteHandler(Handler handler)
		{
			((CheaterReportEventHandler)handler).OnReport(this);
		}
	}

	public class RACommandEvent : Event
	{
		public Player Admin { get; set; }
		public CommandSender Sender { get; set; }
		public string Command { get; set; }
		public string Query { get; set; }
		public string[] Args { get; set; }
		public bool Process { get; set; }
		public RACommandEvent(Player admin, CommandSender sender, string command, bool process)
		{
			this.Admin = admin;
			this.Sender = sender;
			this.Command = command.Split(new char[]
			{
				' '
			})[0];
			this.Args = command.Split(new char[]
			{
				' '
			});
			this.Query = command;
			this.Process = process;
		}

		public override void ExecuteHandler(Handler handler)
		{
			((RACommandEventHandler)handler).OnCommand(this);
		}
	}

	public class RoundEndEvent : Event
	{
		public RoundEndEvent()
		{ }

		public override void ExecuteHandler(Handler handler)
		{
			((RoundEndEventHandler)handler).OnRoundEnd(this);
		}
	}

	public class RoundRestartEvent : Event
	{
		public RoundRestartEvent()
		{ }

		public override void ExecuteHandler(Handler handler)
		{
			((RoundRestartEventHandler)handler).OnRoundRestart(this);
		}
	}

	public class RoundStartEvent : Event
	{
		public RoundStartEvent()
		{ }

		public override void ExecuteHandler(Handler handler)
		{
			((RoundStartEventHandler)handler).OnRoundStart(this);
		}
	}

	public class WaitingForPlayersEvent : Event
	{
		public override void ExecuteHandler(Handler handler)
		{
			((WaitingForPlayersEventHandler)handler).OnWaitingForPlayers(this);
		}
	}
}


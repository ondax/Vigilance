using Vigilance.API;
using Vigilance.API.Events;
using Vigilance.API.Handlers;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Vigilance.Handlers;
using Vigilance.API.Enums;
using Grenades;

namespace Vigilance.Events
{
	public class DoorInteractEvent : Event
	{
		public Door Door { get; set; }
		public GameObject GameObject { get; set; }
		public Player Player { get; set; }
		public bool Allow { get; set; }
		public bool Destroy { get; set; }
		public string Permission { get; set; }

		public DoorInteractEvent(Door door, Player player)
		{
			this.Door = door;
			this.GameObject = door.gameObject;
			this.Player = player;
			this.Allow = true;
			this.Destroy = false;
			this.Permission = door.permissionLevel;
		}

		public override void ExecuteHandler(Handler handler)
		{
			((DoorInteractEventHandler)handler).OnInteract(this);
		}
	}

	public class LCZDecontaminateEvent : Event
	{
		public LCZDecontaminateEvent()
		{ }

		public override void ExecuteHandler(Handler handler)
		{
			((LCZDecontaminateEventHandler)handler).OnDecontaminate(this);
		}
	}

	public class TeamRespawnEvent : Event
	{
		public List<Player> Players { get; set; }
		public bool IsChaos { get; set; }
		public TeamRespawnEvent(Player[] players, bool isChaos)
		{
			this.Players = players.ToList();
			this.IsChaos = isChaos;
		}

		public override void ExecuteHandler(Handler handler)
		{
			((TeamRespawnEventHandler)handler).OnTeamRespawn(this);
		}
	}

	public class TeslaTriggerEvent : Event
	{
		public TeslaGate TeslaGate { get; set; }
		public Player Player { get; set; }
		public bool Triggerable { get; set; }

		public TeslaTriggerEvent(TeslaGate tesla, Player player)
		{
			this.TeslaGate = tesla;
			this.Player = player;
			this.Triggerable = true;
		}

		public override void ExecuteHandler(Handler handler)
		{
			((TeslaTriggerEventHandler)handler).OnTrigger(this);
		}
	}

	public class WarheadDetonateEvent : Event
	{
		public override void ExecuteHandler(Handler handler)
		{
			((WarheadDetonateEventHandler)handler).OnDetonate(this);
		}

		public WarheadDetonateEvent()
		{
		}
	}

	public class WarheadStartEvent : WarheadEvent
	{
		public WarheadStartEvent(Player activator, float timeLeft, bool isResumed) : base(activator, timeLeft)
		{
		}

		public override void ExecuteHandler(Handler handler)
		{
			((WarheadStartEventHandler)handler).OnStart(this);
		}
	}

	public class WarheadStopEvent : WarheadEvent
	{
		public WarheadStopEvent(Player player, float timeLeft) : base(player, timeLeft)
		{
		}

		public override void ExecuteHandler(Handler handler)
		{
			((WarheadStopEventHandler)handler).OnStop(this);
		}
	}
}

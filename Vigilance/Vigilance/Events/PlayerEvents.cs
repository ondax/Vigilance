using Vigilance.API;
using Vigilance.API.Events;
using Vigilance.API.Handlers;
using UnityEngine;
using Vigilance.Handlers;
using System.Linq;
using Vigilance.API.Enums;

namespace Vigilance.Events
{
	public class ElevatorInteractEvent : PlayerEvent
	{
		public Lift Lift { get; }
		public Lift.Elevator Elevator => Lift.elevators.Where(h => Vector3.Distance(h.pos, Lift.transform.position) <= 5f).FirstOrDefault();
		public Vector3 ElevatorPosition { get; }
		public bool Allow { get; set; }

		public ElevatorInteractEvent(Player player, Lift lift, bool allow) : base(player)
		{
			this.Lift = lift;
			this.ElevatorPosition = lift.transform.position;
			this.Allow = allow;
		}

		public override void ExecuteHandler(Handler handler)
		{
			((ElevatorInteractEventHandler)handler).OnElevatorUse(this);
		}
	}

	public class LureEvent : PlayerEvent
	{
		public bool Allow { get; set; }

		public LureEvent(Player player, bool allow) : base(player)
		{
			this.Allow = allow;
		}

		public override void ExecuteHandler(Handler handler)
		{
			((LureEventHandler)handler).OnLure(this);
		}
	}

	public class PlayerDeathEvent : PlayerEvent
	{
		public Player Killer { get; }
		public bool SpawnRagdoll { get; set; }
		public DamageType DamageType { get; set; }

		public PlayerDeathEvent(Player player, Player killer, bool spawnRagdoll, DamageType damageType) : base(player)
		{
			this.Killer = killer;
			this.SpawnRagdoll = spawnRagdoll;
			this.DamageType = damageType;
		}

		public override void ExecuteHandler(Handler handler)
		{
			((PlayerDeathEventHandler)handler).OnPlayerDeath(this);
		}
	}

	public class PlayerHurtEvent : PlayerEvent
	{
		public Player Attacker
		{
			get
			{
				return this.attacker;
			}
		}

		public float Damage { get; set; }
		public DamageType DamageType { get; set; }
		public bool Allow { get; set; }
		public PlayerHurtEvent(Player player, Player attacker, float damage, DamageType damageType, bool allow) : base(player)
		{
			this.attacker = attacker;
			this.Damage = damage;
			this.DamageType = damageType;
			this.Allow = allow;
		}

		public override void ExecuteHandler(Handler handler)
		{
			((PlayerHurtEventHandler)handler).OnPlayerHurt(this);
		}
		private Player attacker;
	}

	public class PlayerJoinEvent : PlayerEvent
	{
		public PlayerJoinEvent(Player player) : base(player)
		{
		}

		public override void ExecuteHandler(Handler handler)
		{
			((PlayerJoinEventHandler)handler).OnPlayerJoin(this);
		}
	}

	public class PlayerLeaveEvent : PlayerEvent
	{
		public PlayerLeaveEvent(Player player) : base(player)
		{
		}

		public override void ExecuteHandler(Handler handler)
		{
			((PlayerLeaveEventHandler)handler).OnPlayerLeave(this);
		}
	}

	public class PlayerSpawnEvent : PlayerEvent
	{
		public Vector3 Position { get; set; }
		public PlayerSpawnEvent(Player player) : base(player)
		{
			this.Position = player.Position;
			this.Position = player.Position;
		}

		public override void ExecuteHandler(Handler handler)
		{
			((PlayerSpawnEventHandler)handler).OnSpawn(this);
		}
	}

	public class PocketDieEvent : PlayerEvent
	{
		public bool Allow { get; set; }
		public PocketDieEvent(Player player, bool allow) : base(player)
		{
			this.Allow = allow;
		}

		public override void ExecuteHandler(Handler handler)
		{
			((PocketDieEventHandler)handler).OnPocketDie(this);
		}
	}

	public class UseMedicalItemEvent : PlayerEvent
	{
		public int RecoverHealth { get; set; }
		public bool Allow { get; set; }

		public UseMedicalItemEvent(Player player, int recoverHealth) : base(player)
		{
			this.RecoverHealth = recoverHealth;
		}
		public override void ExecuteHandler(Handler handler)
		{
			((UseMedicalItemEventHandler)handler).OnUse(this);
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vigilance.API.Events
{
	public abstract class WarheadEvent : Event
	{
		public float TimeLeft { get; set; }
		public Player User => player;
		public bool Allow { get; set; }
		public WarheadEvent(Player player, float timeLeft)
		{
			this.player = player;
			this.TimeLeft = timeLeft;
			this.Allow = true;
		}
		private Player player;
	}
}

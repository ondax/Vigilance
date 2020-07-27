namespace Vigilance.API.Events
{
	public abstract class PlayerEvent : Event
	{
		public Player Player => player;

		public PlayerEvent(Player player)
		{
			this.player = player;
		}

		private Player player;
	}
}

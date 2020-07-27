using Vigilance.API.Handlers;

namespace Vigilance.API.Events
{
	public class EventHandlerWrapper
	{
		public Handler Handler { get; }
		public Plugin Plugin { get; }

		public EventHandlerWrapper(Plugin plugin, Handler handler)
		{
			this.Plugin = plugin;
			this.Handler = handler;
		}
	}
}

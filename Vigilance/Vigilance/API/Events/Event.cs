using Vigilance.API.Handlers;

namespace Vigilance.API.Events
{
	public abstract class Event
	{
		public abstract void ExecuteHandler(Handler handler);
	}
}

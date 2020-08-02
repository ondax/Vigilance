using Vigilance.API.Handlers;
using Vigilance.API.Commands;

namespace Vigilance
{
	public abstract class Plugin
	{
		public abstract string Id { get; }
		public abstract string Name { get; }
		public abstract void OnEnable();
		public abstract void OnDisable();

		public void AddCommand(string command, Command handler)
		{
			CommandManager.RegisterCommand(this, command, handler);
		}

		public void AddHandler(Handler handler)
		{
			EventController.AddHandler(this, handler);
		}

		public void Debug(string message)
		{
			Log.Debug(this.Name, message);
		}

		public void Info(string message)
		{
			Log.Info(this.Name, message);
		}

		public void Warn(string message)
		{
			Log.Warn(this.Name, message);
		}

		public void Error(string message)
		{
			Log.Error(this.Name, message);
		}
	}
}

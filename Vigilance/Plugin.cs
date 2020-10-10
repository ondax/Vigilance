using System;
using System.IO;

namespace Vigilance
{
    public abstract class Plugin
    {
        public abstract string Name { get; }
        public YamlConfig Config { get; set; }

        public abstract void Enable();
        public abstract void Disable();
        public abstract void Reload();

        public void Info(string msg) => Log.Add(Name, msg, LogType.Info);
        public void Error(string msg) => Log.Add(Name, msg, LogType.Error);
        public void Warn(string msg) => Log.Add(Name, msg, LogType.Warn);
        public void Debug(string msg) => Log.Add(Name, msg, LogType.Debug);
        public void AddLog(string msg) => Log.Add(msg);
        public void AddCommand(CommandHandler commandHandler) => CommandManager.RegisterCommand(commandHandler);
        public void AddCommand(GameCommandHandler gameCommandHandler) => CommandManager.RegisterGameCommand(gameCommandHandler);
        public void AddCommand(ConsoleCommandHandler handler) => CommandManager.RegisterConsoleCommand(handler);
        public void AddEventHandler(EventHandler eventHandler) => EventManager.RegisterHandler(this, eventHandler);
		public void RemoveEventHandler(EventHandler eventHandler) => EventManager.UnregisterHandler(this, eventHandler);
		public void RemoveAllHandlers() => EventManager.UnregisterHandlers(this);
		public void RemoveCommand(string command) => CommandManager.UnregisterCommand(command);

		public void AddConfig(string description, string key, string value)
		{
			try
			{
				Paths.CheckFile(Paths.ConfigPath);
				string[] currentLines = File.ReadAllLines(Paths.GetPluginConfigPath(this));
				using (StreamWriter writer = new StreamWriter(Paths.GetPluginConfigPath(this), true))
				{
					if (!Paths.ContainsKey(currentLines, key))
					{
						if (!string.IsNullOrEmpty(description) && !currentLines.Contains($"# {description}"))
							writer.WriteLine($"# {description}");
						if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(key))
							writer.WriteLine($"{key}: {value}");
						writer.WriteLine("");
					}
					writer.Flush();
					writer.Close();
				}
			}
			catch (Exception e)
			{
				Log.Add("Plugin", $"{Name} caused an exception while adding config.", LogType.Error);
				Log.Add("Plugin", e);
			}
		}
	}
}

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
        public void AddConfig(string key, string value, string description = "")
        {
			try
			{
				string path = Paths.GetPluginConfigPath(this);
				Paths.CheckFile(path);
				string[] currentLines = File.ReadAllLines(path);
				using (StreamWriter writer = new StreamWriter(path, true))
				{
                    if (!Paths.ContainsKey(currentLines, key))
                    {
                        if (!string.IsNullOrEmpty(description))
                            writer.WriteLine($"# {description}");
                        if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(key))
                            writer.WriteLine($"{key}: {value}");
                    }
					writer.Flush();
					writer.Close();
				}
			}
			catch (Exception e)
			{
				Log.Add("Plugin.AddConfig", e);
			}
		}
    }
}

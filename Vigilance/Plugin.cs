using System.Collections.Generic;

namespace Vigilance
{
    public abstract class Plugin
    {
        public abstract string Name { get; }
        public YamlConfig Config { get; set; }
        public Dictionary<string, string> ConfigValues { get; set; } = new Dictionary<string, string>();

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
        public void AddConfig(string description, string key, string value)
        {
            if (!string.IsNullOrEmpty(description))
                ConfigValues.Add($"cfgdesc={description}", $"{key}: {value}");
            else
                ConfigValues.Add(key, value);
        }
    }
}

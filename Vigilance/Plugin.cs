using System.Reflection;

namespace Vigilance
{
    public abstract class Plugin
    {
        public abstract string Name { get; }
        public YamlConfig Config { get => _cfg; set => _cfg = value; }
        public Assembly Assembly => PluginManager.GetAssembly(this);
        private YamlConfig _cfg;

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
        public void AddEventHandler(EventHandler eventHandler) => EventManager.RegisterHandler(this, eventHandler);
    }
}

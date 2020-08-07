namespace Vigilance
{
    public abstract class Plugin
    {
        public abstract string Name { get; }
        public YamlConfig Config { get => _cfg; set => _cfg = value; }
        private YamlConfig _cfg;

        public abstract void Enable();
        public abstract void Disable();
        public abstract void Reload();
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Harmony;

namespace Vigilance
{
    public class PluginManager
    {
        public static string Version => "5.0.1";
        public static List<Plugin> Plugins => _plugins;
        public static List<Assembly> Dependencies => _dependencies;
        public static YamlConfig Config => _config;
        private static List<Assembly> _dependencies;
        private static List<Plugin> _plugins;
        private static YamlConfig _config;

        public static void Enable()
        {
            try
            {
                CommandManager.Enable();
                EventManager.Enable();
                Paths.CheckDirectories();
                Paths.CheckDependencies();
                _config = new YamlConfig(Paths.ConfigPath);
                _plugins = new List<Plugin>();
                _dependencies = new List<Assembly>();
                _config?.Reload();

                try
                {
                    HarmonyInstance.Create("vigilance.patches").PatchAll();
                }
                catch (Exception e)
                {
                    Log.Add("PluginManager", e);
                }

                try
                {
                    foreach (Assembly assembly in Paths.GetAssemblies(Paths.Plugins))
                    {
                        foreach (Type type in assembly.GetTypes())
                        {
                            if (type.IsSubclassOf(typeof(Plugin)))
                            {
                                Plugin plugin = (Plugin)Activator.CreateInstance(type);
                                try
                                {
                                    string cfgPath = Paths.GetPluginConfigPath(plugin);
                                    Paths.CheckFile(cfgPath);
                                    plugin.Config = new YamlConfig(cfgPath);
                                    plugin.Config?.Reload();
                                    plugin.Enable();
                                    _plugins.Add(plugin);
                                    Log.Add("PluginManager", $"Succesfully loaded {plugin.Name}", LogType.Info);
                                }
                                catch (Exception e)
                                {
                                    Log.Add("PluginManager", $"Plugin {plugin.Name} caused an exception while enabling.", LogType.Error);
                                    Log.Add("PluginManager", e);
                                }
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Add("PluginManager", e);
                    API.Server.Restart(true);
                }

                try
                {
                    foreach (string a in Directory.GetFiles(Paths.Dependencies))
                    {
                        if (a.EndsWith(".dll"))
                        {
                            Assembly assembly = Assembly.LoadFrom(a);
                            _dependencies.Add(assembly);
                        }
                    }
                }
                catch (Exception e)
                {
                    Log.Add("PluginManager", e);
                }
                ReloadPluginConfigs();
                Log.Add("PluginManager", "Succesfully loaded!", LogType.Info);
            }
            catch (Exception e)
            {
                Log.Add("PluginManager", e);
            }
        }

        public static void Reload()
        {
            Log.Add("PluginManager", "Reloading plugins", LogType.Info);
            foreach (Plugin plugin in _plugins)
            {
                try
                {
                    plugin.Reload();
                }
                catch (Exception e)
                {
                    Log.Add("PluginManager", $"Plugin {plugin.Name} caused an exception while reloading.", LogType.Error);
                    Log.Add("PluginManager", e);
                }
            }
        }

        public static void Disable()
        {
            Log.Add("PluginManager", "Disabling plugins", LogType.Info);
            foreach (Plugin plugin in _plugins)
            {
                try
                {
                    plugin.Disable();
                    _plugins.Remove(plugin);
                }
                catch (Exception e)
                {
                    Log.Add("PluginManager", $"Plugin {plugin.Name} caused an exception while disabling.", LogType.Error);
                    Log.Add("PluginManager", e);
                }
            }
            _plugins.Clear();
            _dependencies.Clear();
        }

        public static void ReloadPluginConfigs()
        {
            foreach (Plugin plugin in _plugins)
            {
                plugin.Config?.Reload();
            }
        }
    }
}

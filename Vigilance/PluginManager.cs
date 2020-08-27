using System;
using System.Collections.Generic;
using System.Reflection;
using Harmony;

namespace Vigilance
{
    public class PluginManager
    {
        public static string Version => "5.0.7";
        public static Dictionary<Plugin, Assembly> Plugins => _plugins;
        public static List<Assembly> Dependencies => _dependencies;
        public static YamlConfig Config => _config;
        private static List<Assembly> _dependencies;
        private static Dictionary<Plugin, Assembly> _plugins;
        private static YamlConfig _config;

        public static void Enable()
        {
            try
            {
                Paths.CheckMainConfig();
                _config = new YamlConfig(Paths.ConfigPath);
                _config?.Reload();
                CommandManager.Enable();
                EventManager.Enable();
                Paths.CheckDirectories();
                Paths.CheckDependencies();
                _plugins = new Dictionary<Plugin, Assembly>();
                _dependencies = new List<Assembly>();

                try
                {
                    HarmonyInstance.Create("vigilance.patches").PatchAll();
                }
                catch (Exception e)
                {
                    Log.Add("PluginManager", "An exception occured while patching!", LogType.Error);
                    Log.Add("PluginManager", e);
                }

                try
                {
                    Load();
                }
                catch (Exception e)
                {
                    Log.Add("PluginManager", "An exception occured while loading!", LogType.Error);
                    Log.Add("PluginManager", e);
                }
                CustomNetworkManager.Modded = _config.GetBool("mark_as_modded", true);
                Log.Add("PluginManager", $"Succesfully loaded Vigilance v{Version}!", LogType.Info);
            }
            catch (Exception e)
            {
                Log.Add("PluginManager", "An exception occured while enabling!", LogType.Error);
                Log.Add("PluginManager", e);
            }
        }

        private static void Load()
        {
            try
            {
                foreach (Assembly assembly in Paths.GetAssemblies(Paths.Plugins))
                {
                    foreach (Type type in assembly.GetTypes())
                    {
                        if (type.IsSubclassOf(typeof(Plugin)))
                        {
                            Plugin plugin = (Plugin)Activator.CreateInstance(type);
                            if (_plugins.ContainsKey(plugin) || _plugins.ContainsValue(assembly))
                            {
                                try
                                {
                                    plugin.Reload();
                                    plugin.Config?.Reload();
                                }
                                catch (Exception e)
                                {
                                    Log.Add("PluginManager", $"Plugin \"{plugin.Name}\" caused an exception while reloading.", LogType.Error);
                                    Log.Add("PluginManager", e);
                                }
                                return;
                            }
                            else
                            {

                                try
                                {
                                    string cfgPath = Paths.GetPluginConfigPath(plugin);
                                    Paths.CheckFile(cfgPath);
                                    plugin.Config = new YamlConfig(cfgPath);
                                    plugin.Config?.Reload();
                                    plugin.Enable();
                                    _plugins.Add(plugin, assembly);
                                    Log.Add("PluginManager", $"Succesfully loaded plugin \"{plugin.Name}\"", LogType.Info);
                                }
                                catch (Exception e)
                                {
                                    Log.Add("PluginManager", $"Plugin \"{plugin.Name}\" caused an exception while enabling.", LogType.Error);
                                    Log.Add("PluginManager", e);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Add("PluginManager", e);
            }

            try
            {
                foreach (Assembly assembly in Paths.GetAssemblies(Paths.Dependencies))
                {
                    _dependencies.Add(assembly);
                    Log.Add("PluginManager", $"Succesfully loaded dependency ¸\"{assembly.GetName().Name}\"", LogType.Info);
                }
            }
            catch (Exception e)
            {
                Log.Add("PluginManager", "An error occured while loading dependencies", LogType.Error);
                Log.Add("PluginManager", e);
            }
        }

        public static void Reload()
        {
            Log.Add("PluginManager", "Reloading plugins", LogType.Debug);
            Load();
        }

        public static void Disable()
        {
            Log.Add("PluginManager", "Disabling plugins", LogType.Debug);
            foreach (Plugin plugin in _plugins.Keys)
            {
                Disable(plugin);
            }
            _plugins.Clear();
            _dependencies.Clear();
        }

        public static void ReloadPluginConfigs()
        {
            foreach (Plugin plugin in _plugins.Keys)
            {
                try
                {
                    plugin.Config?.Reload();
                }
                catch (Exception e)
                {
                    Log.Add("PluginManager", $"YamlConfig of {plugin.Name} caused an exception while reloading", LogType.Error);
                    Log.Add("PluginManager", e);
                }
            }
        }

        public static void Disable(Plugin plugin)
        {
            try
            {
                plugin.Disable();
                _plugins.Remove(plugin);
            }
            catch (Exception e)
            {
                Log.Add("PluginManager", $"Plugin \"{plugin.Name}\" caused an exception while disabling.", LogType.Error);
                Log.Add("PluginManager", e);
            }
        }

        public static Assembly GetAssembly(Plugin plugin)
        {
            if (_plugins.TryGetValue(plugin, out Assembly assembly))
            {
                return assembly;
            }
            else
            {
                return null;
            }
        }
    }
}

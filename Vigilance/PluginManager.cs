using System;
using System.Collections.Generic;
using System.Reflection;
using CommandSystem.Commands;
using Harmony;

namespace Vigilance
{
    public class PluginManager
    {
        public static string Version => "5.1.5";
        public static Dictionary<Plugin, Assembly> Plugins { get; set; }
        public static List<Assembly> Dependencies { get; set; }
        public static YamlConfig Config { get; set; }

        public static void Enable()
        {
            try
            {
                Paths.CheckMainConfig();
                Config = new YamlConfig(Paths.ConfigPath);
                CommandManager.Enable();
                EventManager.Enable();
                Paths.CheckDirectories();
                Paths.CheckDependencies();
                Paths.ValidateConfig(Paths.GetDefaultConfigValues(), Paths.ConfigPath);
                Config?.Reload();
                Plugins = new Dictionary<Plugin, Assembly>();
                Dependencies = new List<Assembly>();

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
                CustomNetworkManager.Modded = Config.GetBool("mark_as_modded", true);
                BuildInfoCommand.ModDescription = $"Vigilance v{Version} -> a simple plugin loader and a little API for SCP: Secret Laboratory.";
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
                            if (Plugins.ContainsKey(plugin) || Plugins.ContainsValue(assembly))
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
                                    plugin.Enable();
                                    plugin.Config?.Reload();
                                    Plugins.Add(plugin, assembly);
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
                foreach (string dep in System.IO.Directory.GetFiles(Paths.Dependencies))
                {
                    if (dep.EndsWith(".dll"))
                    {
                        Assembly assembly = Assembly.LoadFrom(dep);
                        Dependencies.Add(assembly);
                        Log.Add("PluginManager", $"Succesfully loaded dependency ¸\"{assembly.GetName().Name}\"", LogType.Info);
                    }
                    else
                    {
                        Log.Add($"PluginManager", $"{dep} is not a dependency!", LogType.Warn);
                    }
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
            foreach (Plugin plugin in Plugins.Keys)
            {
                Disable(plugin);
            }
            Plugins.Clear();
            Dependencies.Clear();
        }

        public static void ReloadPluginConfigs()
        {
            foreach (Plugin plugin in Plugins.Keys)
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
                Plugins.Remove(plugin);
            }
            catch (Exception e)
            {
                Log.Add("PluginManager", $"Plugin \"{plugin.Name}\" caused an exception while disabling.", LogType.Error);
                Log.Add("PluginManager", e);
            }
        }
    }
}

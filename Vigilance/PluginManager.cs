using System;
using System.Collections.Generic;
using System.Reflection;
using CommandSystem.Commands;
using Harmony;

namespace Vigilance
{
    public class PluginManager
    {
        public static string Version => "5.2.1";
        public static Dictionary<string, Assembly> Assemblies { get; set; }
        public static Dictionary<string, Plugin> Plugins { get; set; }
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
                Plugins = new Dictionary<string, Plugin>();
                Dependencies = new List<Assembly>();
                ConfigManager.Reload();

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

                CustomNetworkManager.Modded = ConfigManager.MarkAsModded;
                BuildInfoCommand.ModDescription = $"Vigilance v{Version} - a simple plugin loader and a little API for SCP: Secret Laboratory.";
                Log.Add("PluginManager", $"Succesfully loaded version \"{Version}\"!", LogType.Info);
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
                    try
                    {
                        foreach (Type type in assembly.GetTypes())
                        {
                            try
                            {
                                if (type.IsSubclassOf(typeof(Plugin)))
                                {
                                    Plugin plugin = null;
                                    try
                                    {
                                        plugin = (Plugin)Activator.CreateInstance(type);
                                    }
                                    catch (Exception e)
                                    {
                                        Log.Add("PluginManager", e);
                                    }

                                    if (Plugins.ContainsKey(assembly.Location) || Plugins.ContainsValue(plugin))
                                    {
                                        try
                                        {
                                            plugin.Reload();
                                        }
                                        catch (Exception e)
                                        {
                                            Log.Add("PluginManager", $"Plugin \"{plugin.Name}\" caused an exception while reloading.", LogType.Error);
                                            Log.Add("PluginManager", e);
                                        }
                                    }
                                    else
                                    {

                                        try
                                        {
                                            string cfgPath = Paths.GetPluginConfigPath(plugin);
                                            Paths.CheckFile(cfgPath);
                                            plugin.Config = new YamlConfig(cfgPath);
                                            plugin.Enable();
                                            Plugins.Add(assembly.Location, plugin);
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
                            catch (Exception e)
                            {
                                Log.Add("PluginManager", e);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Add("PluginManager", e);
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
            foreach (Plugin plugin in Plugins.Values)
            {
                Disable(plugin);
            }
            Plugins.Clear();
            Dependencies.Clear();
        }

        public static void ReloadPluginConfigs()
        {
            foreach (Plugin plugin in Plugins.Values)
            {
                try
                {
                    plugin.Config.Reload();
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
            }
            catch (Exception e)
            {
                Log.Add("PluginManager", $"Plugin \"{plugin.Name}\" caused an exception while disabling.", LogType.Error);
                Log.Add("PluginManager", e);
            }
        }
    }
}
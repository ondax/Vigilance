using MEC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace Vigilance
{
	public static class PluginManager
	{
		public static class Directories
        {
			public static string SCPSL_Data => Application.dataPath;
			public static string Managed => $"{SCPSL_Data}/Managed";
			public static string Vigilance => $"{SCPSL_Data}/Vigilance";
			public static string VigilanceFile => $"{SCPSL_Data}/Managed/Vigilance.dll";
			public static string HarmonyFile => $"{SCPSL_Data}/Managed/0Harmony.dll";
			public static string Dependencies => $"{Vigilance}/Dependencies";
			public static string Plugins => $"{Vigilance}/Plugins";
        }

		public static string Version => version;
		public static List<string> Dependencies => dependencies;
		public static List<Plugin> LoadedPlugins => loadedPlugins;
		public static string GameVersion => gameVersion;
		public static string Tracker => $"<color=#FFFFFF00><size=1>SMVigilance {version}</size></color>";

		public static void StartPluginLoading()
		{
			Prepare();
			if (!Directory.Exists(Directories.Vigilance))
			{
				Directory.CreateDirectory(Directories.Vigilance);
			}
			if (!Directory.Exists(Directories.Dependencies))
			{
				Directory.CreateDirectory(Directories.Dependencies);
			}
			if (!Directory.Exists(Directories.Plugins))
			{
				Directory.CreateDirectory(Directories.Plugins);
			}
			CheckForHarmony();
			if (!_harmonyFound)
            {
				Log.Error("PluginManager", "Aborting startup!");
				Timing.CallDelayed(2f, Application.Quit);
				return;
            }
			try
			{
				LoadDependencies();
				Data.Patcher.Patch();
				LoadPlugins();
			}
			catch (Exception arg)
			{
				Log.Error("PluginManager", string.Format("An error occured while initializing Vigilance.\n{0}", arg));
			}
			CustomNetworkManager.Modded = ConfigManager.GetBool("mark_as_modded");
			Log.Info("PluginManager", "Succesfully loaded version \"" + PluginManager.version + "\".");
		}

		private static void CheckForHarmony()
        {
			if (!File.Exists(Directories.HarmonyFile))
            {
				Log.Error("PluginManager", "Can't find 0Harmony.dll!");
				_harmonyFound = false;
				return;
            }
			_harmonyFound = true;
			Assembly.LoadFrom(Directories.HarmonyFile);
        }

		private static void LoadDependencies()
		{
			foreach (string text in Directory.GetFiles(Directories.Dependencies))
			{
				if (text.EndsWith(".dll"))
				{
					Assembly assembly = Assembly.LoadFrom(text);
					PluginManager.dependencies.Add(assembly.GetName().Name);
					Log.Info("DependencyManager", "Succesfully loaded \"" + assembly.GetName().Name + "\"");
				}
			}
		}

		private static void LoadPlugins()
		{
			string[] files = Directory.GetFiles(Directories.Plugins);
			for (int i = 0; i < files.Length; i++)
			{
				Assembly assembly = Assembly.LoadFrom(files[i]);
				try
				{
					foreach (Type type in assembly.GetTypes())
					{
						if (type.IsSubclassOf(typeof(Plugin)) && type != typeof(Plugin))
						{
							Plugin plugin = (Plugin)Activator.CreateInstance(type);
							plugin.OnEnable();
							PluginManager.loadedPlugins.Add(plugin);
							Log.Info("PluginManager", string.Concat(new string[]
							{
								"Succesfully loaded \"",
								plugin.Name,
								"\" (",
								plugin.Id,
								")!"
							}));
						}
					}
				}
				catch (Exception arg)
				{
					Log.Error("PluginManager", "An error occured while loading plugins.");
					Log.Error("PluginManager", string.Format("{0}", arg));
				}
			}
		}

		private static void Prepare()
		{
			CommandManager.Prepare();
			Data.Prepare();
			FileLog.CheckDirectories();
			loadedPlugins = new List<Plugin>();
			dependencies = new List<string>();
			version = "4.1.1";
			gameVersion = "Scopophobia";
			_harmonyFound = false;
		}

		private static List<Plugin> loadedPlugins;
		private static List<string> dependencies;
		private static string version;
		private static string gameVersion;
		private static bool _harmonyFound;
	}
}

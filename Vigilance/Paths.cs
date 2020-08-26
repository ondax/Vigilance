using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using System;
using System.Net;
using Vigilance.API;
using System.Reflection;

namespace Vigilance
{
	public static class Paths
	{
		public static string SCPSL_Data => Application.dataPath;
		public static string Managed => $"{SCPSL_Data}/Managed";
		public static string Vigilance => $"{SCPSL_Data}/Vigilance";
		public static string VigilanceFile => $"{Managed}/Vigilance.dll";
		public static string HarmonyFile => $"{Managed}/0Harmony.dll";
		public static string Dependencies => $"{Vigilance}/Dependencies";
		public static string Plugins => $"{Vigilance}/Plugins";
		public static string NewtonsoftJson => $"{Managed}/Newtonsoft.Json.dll";
		public static string ConfigPath => $"{ConfigsPath}/{Server.Port}.yml";
		public static string ConfigsPath => $"{Vigilance}/Configs";
		public static string PluginConfigsPath => $"{ConfigsPath}/Plugins/{Server.Port}";
		public static string HarmonyDownloadURL => "https://github.com/DrGaster17/Vigilance/releases/download/v4.1.2/0Harmony.dll";

		public static DirectoryInfo Create(string directory)
		{
			DirectoryInfo info = Directory.CreateDirectory(directory);
			Log.Add("Paths", $"Creating directory {directory}", LogType.Debug);
			return info;
		}

		public static void CreateFile(string path)
        {
			if (!File.Exists(path))
            {
				Log.Add("Paths", $"Creating file {path}", LogType.Debug);
				FileStream stream = File.Create(path);
				stream.Close();
            }
        }

		public static void CheckMainConfig()
		{
			if (!Directory.Exists(ConfigsPath))
				Directory.CreateDirectory(ConfigsPath);
			if (!File.Exists(ConfigPath))
				File.Create(ConfigPath).Close();
		}

		public static void Delete(string directory)
		{
			Directory.Delete(directory);
			Log.Add("Paths", $"Deleting directory {directory}", LogType.Debug);
		}

		public static void Check(string directory)
		{
			if (!Directory.Exists(directory))
			{
				Log.Add("Paths", $"Directory {directory} does not exist", LogType.Debug);
				Create(directory);
			}
		}

		public static void CheckFile(string path)
        {
			if (!File.Exists(path))
            {
				Log.Add("Paths", $"File {path} does not exist", LogType.Debug);
				CreateFile(path);
            }
        }

		public static string GetPluginConfigPath(Plugin plugin)
        {
			return $"{PluginConfigsPath}/{plugin.Name}.yml";
        }

		public static void CheckDirectories()
		{
			Log.Add("Paths", "Checking directories", LogType.Debug);
			Check(SCPSL_Data);
			Check(Managed);
			Check(Vigilance);
			Check(Dependencies);
			Check(Plugins);
			Check(ConfigsPath);
			Check(PluginConfigsPath);
			CheckFile(ConfigPath);
		}

		public static List<Assembly> GetAssemblies(string path)
		{
			try
			{
				List<Assembly> assemblies = new List<Assembly>();
				if (string.IsNullOrEmpty(path))
					return assemblies;
				if (!Directory.Exists(path))
					return assemblies;
				Log.Add("Paths", $"Loading assemblies in {path}", LogType.Debug);
				foreach (string f in Directory.GetFiles(path))
				{
					if (f.EndsWith(".dll"))
					{
						Assembly a = Assembly.LoadFrom(f);
						assemblies.Add(a);
					}
				}
				return assemblies;
			}
			catch (Exception e)
			{
				Log.Add("Paths", e);
				return new List<Assembly>();
			}
		}

		public static byte[] GetBytes(Assembly assembly)
		{
			FileStream fileStream = File.Open(assembly.Location, FileMode.Open);
			byte[] result;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				fileStream.CopyTo(memoryStream);
				result = memoryStream.ToArray();
			}
			fileStream.Close();
			return result;
		}

		public static byte[] GetBytes(string path)
        {
			FileStream fileStream = File.Open(path, FileMode.Open);
			byte[] result;
			using (MemoryStream memoryStream = new MemoryStream())
			{
				fileStream.CopyTo(memoryStream);
				result = memoryStream.ToArray();
			}
			fileStream.Close();
			return result;
		}

		public static void CheckDependencies()
        {
			if (!File.Exists(HarmonyFile))
            {
				Log.Add("Paths", "Downloading Harmony", LogType.Info);
				Download(HarmonyDownloadURL, HarmonyFile);
            }
        }

		public static void DownloadPlugin(string url, string pluginName)
		{
			Log.Add("Paths", $"Downloading \"{pluginName}\" from \"{url}\"", LogType.Debug);
			if (!pluginName.EndsWith(".dll"))
				pluginName += ".dll";
			string path = $"{Plugins}/{pluginName}";
			try
			{
				Download(url, path);
				if (File.Exists(path))
				{
					Assembly assembly = Assembly.LoadFrom(path);
					foreach (Type type in assembly.GetTypes())
					{
						if (type.IsSubclassOf(typeof(Plugin)))
						{
							Plugin plugin = (Plugin)Activator.CreateInstance(type);
							try
							{
								string cfgPath = GetPluginConfigPath(plugin);
								CheckFile(cfgPath);
								plugin.Config = new YamlConfig(cfgPath);
								plugin.Config?.Reload();
								plugin.Enable();
								PluginManager.Plugins.Add(plugin, assembly);
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
			catch (Exception e)
			{
				Log.Add("Paths", e);
			}
		}

		public static void DownloadDependency(string url, string name)
		{
			Log.Add("Paths", $"Downloading \"{name}\" from \"{url}\"", LogType.Debug);
			if (!name.EndsWith(".dll"))
				name += ".dll";
			string path = $"{Dependencies}/{name}";
			try
			{
				Download(url, path);
				Assembly assemly = Assembly.LoadFrom(path);
				PluginManager.Dependencies.Add(assemly);
				Log.Add("PluginManager", $"Succesfully loaded dependency \"{assemly.GetName().Name}\"", LogType.Info);
			}
			catch (Exception e)
			{
				Log.Add("Paths", e);
			}
		}

		public static void Download(string url, string path)
        {
			try
			{
				using (WebClient client = new WebClient())
				{
					client.DownloadFile(url, path);
				}
			}
			catch (Exception e)
            {
				Log.Add("Paths", e);
            }
        }
	}
}

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using System;
using System.Net;
using Vigilance.API;

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

		public static FileStream CreateFile(string path)
        {
			if (!File.Exists(path))
            {
				Log.Add("Paths", $"Creating file {path}", LogType.Debug);
				FileStream stream = File.Create(path);
				return stream;
            }
			return File.Open(path, FileMode.Open);
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
			if (!File.Exists(ConfigPath))
				File.Create(ConfigPath);
		}

		public static List<Assembly> GetAssemblies(string path)
		{
			try
			{
				List<Assembly> assemblies = new List<Assembly>();
				if (string.IsNullOrEmpty(path))
					return assemblies;
				foreach (string f in Directory.GetFiles(path))
				{
					if (f.EndsWith(".dll"))
					{
						Log.Add("Paths", $"Loading assemblies in {path}", LogType.Debug);
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


		public static void Download(string url, string fileName)
        {
			try
			{
				using (WebClient client = new WebClient())
				{
					client.DownloadFile(url, fileName);
				}
			}
			catch (Exception e)
            {
				Log.Add("Paths", e);
            }
        }
	}
}

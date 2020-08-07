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
		public static string ConfigPath => $"{Vigilance}/Config-{Server.Port}.yml";

		public static string JsonDownloadURL => "https://github.com/DrGaster17/Vigilance/releases/download/v4.1.6/Newtonsoft.Json.dll";
		public static string HarmonyDownloadURL => "https://github.com/DrGaster17/Vigilance/releases/download/v4.1.2/0Harmony.dll";

		public static DirectoryInfo Create(string directory)
		{
			DirectoryInfo info = Directory.CreateDirectory(directory);
			Log.Add("Paths", $"Creating directory {directory}", LogType.Debug);
			return info;
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
				Log.Add("Paths", $"Directory {directory} does not exist, creating", LogType.Debug);
				Create(directory);
			}
		}

		public static string GetPluginConfigPath(Plugin plugin)
        {
			return $"{Vigilance}/Plugins/{plugin.Name}/{Server.Port}.yml";
        }

		public static void CheckDirectories()
		{
			Log.Add("Paths", "Checking directories", LogType.Debug);
			Check(SCPSL_Data);
			Check(Managed);
			Check(Vigilance);
			Check(Dependencies);
			Check(Plugins);
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
					if (path.EndsWith(".dll"))
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
			return GetBytes(Assembly.LoadFrom(path));
        }

		public static void CheckDependencies()
        {
			CheckDirectories();
			if (!File.Exists(HarmonyFile))
            {
				Log.Add("Paths", "Downloading Harmony", LogType.Debug);
				Download(HarmonyDownloadURL, HarmonyFile);
            }

			if (!File.Exists(NewtonsoftJson))
            {
				Log.Add("Paths", "Downloading Newtonsoft.Json", LogType.Debug);
				Download(JsonDownloadURL, NewtonsoftJson);
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

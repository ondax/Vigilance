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
			{
				File.Create(ConfigPath).Close();
				WriteConfigValues(GetDefaultConfigValues(), ConfigPath);
			}
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

		public static Dictionary<string, string> GetDefaultConfigValues()
        {
			Dictionary<string, string> configs = new Dictionary<string, string>();

			// Generic configs
			configs.Add("segment=Generic configs", "");
			configs.Add("cfgdesc=Your server will have the \"Modded\" flag in the server list if you set this to true.", "mark_as_modded: true");
			configs.Add("cfgdesc=Adds a transparent text that specifies the version of Vigilance.", "tracking: false");
			configs.Add("cfgdesc=Whether or not debug messages should be printed in the server console. This option is usually very spammy.", "debug: false");
			//
			configs.Add("line=1", "");
			// ServerGuard
			configs.Add("segment=Server Guard", "");
			configs.Add("cfgdesc=Whether or not ServerGuard should be enabled.", "guard_enabled: false");
			configs.Add("cfgdesc=List of active ServerGuard modules. Valid values: vpn, vpnshield, steam, steamshield.", "guard_enabled_modules: [vpn,steam]");
			configs.Add("cfgdesc=You will need a API key used for checking if a specific IP is a VPN connection or not. Warning! GeForce Now is flagged as a VPN too! You can get your key here! \"https://iphub.info/apiKey/newFree\"", "vpn_api_key: none");
			configs.Add("cfgdesc=ServerGurd will kick players that didnt buy anything on Steam if this is set to true.", "steam_block_new accounts: false");
			configs.Add("cfgdesc=ServerGuard will kick non-setup Steam accounts if this is set to true.", "steam_block_non_setup_accounts: false");
			//
			configs.Add("line=2", "");
			// Gameplay configs
			configs.Add("segment=Gameplay configuration", "");
			configs.Add("cfgdesc=Whether or not SCP-939 should give you amnesia.", "enable_amnesia: true");
			configs.Add("cfgdesc=Whether or not AntiFly should kick or kill players for \"cheating\".", "antifly_enabled: true");
			configs.Add("cfgdesc=Whether or not players with tutorial should trigger SCP-096.", "can_tutorial_trigger_scp096: true");
			configs.Add("cfgdesc=Whether or not players with tutorial should block SCP-173's movement.", "can_tutorial_block_scp173: true");
			configs.Add("cfgdesc=Whether or not blood should be spawned underneath a player when a player gets hit.", "enable_blood_spawning: true");
			configs.Add("cfgdesc=Whether or not a black hole should be spawned underneath a player when a player gets taken by SCP-106.", "enable_decal_spawning: true");
			configs.Add("cfgdesc=Amount of damage the player takes when SCP-106 sends him into the pocket dimension.", "scp106_pocket_enter_damage: 40");
			configs.Add("cfgdesc=Whether or not SCP-268 effects should wear off when a player interacts with something.", "disable_scp_268_effects_when_interacted: true");
			configs.Add("cfgdesc=Whether or not a ragdoll should spawn when a player dies/disconnects.", "spawn_ragdolls: true");
			configs.Add("cfgdesc=List of RoleIDs that will be able to trigger tesla gates.", "tesla_triggerable_roles: [0,1,3,4,5,6,8,9,10,11,12,13,14,15,16,17]");
			configs.Add("cfgdesc=How long does SCP-173 have to wait before it can open the gate when the round starts.", "scp173_door_cooldown: 25");
			// 
			configs.Add("line=3", "");
			// SCP-096 Configs
			configs.Add("segment=Scp-096 Configuration", "");
			configs.Add("cfgdesc=Starting value of AHP for SCP-096.", "scp096_max_shield: 500");
			configs.Add("cfgdesc=How much AHP does SCP-096 gain for every player that looks at it.", "scp096_shield_per_player: 200");
			configs.Add("cfgdesc=Whether or not SCP-096 should be able to pry gates.", "scp096_pry_gates: true");
			configs.Add("cfgdesc=Whether or not SCP-096 should stay enraged for longer when someone looks at it.", "scp096_add_enrage_time_when_looked: true");
			configs.Add("cfgdesc=Whether or not SCP-096 should be able to kill players that didn't look at it.", "scp096_can_kill_only_targets: true");
			configs.Add("cfgdesc=Whether or not SCP-096 should be able to regenerate AHP.", "scp096_can_regen: true");
			configs.Add("cfgdesc=Recharge rate of AHP of SCP-096.", "scp096_shield_recharge_rate: 10");
			configs.Add("cfgdesc=Whether or not SCP-096 should be able to see a red particle on every target.", "scp096_vision_particles: true");
			configs.Add("cfgdesc=Whether or not SCP-096 should be able to destroy doors.", "scp096_can_destroy_doors: true");
			return configs;
        }

		public static void WriteConfigValues(Dictionary<string, string> configs, string path)
        {
			using (StreamWriter writer = new StreamWriter(path, true))
            {
				foreach (KeyValuePair<string, string> pair in configs)
                {
					if (pair.Key.ToUpper().StartsWith("LINE"))
                    {
						writer.WriteLine("");
                    }
					if (pair.Key.ToUpper().StartsWith("SEGMENT="))
                    {
						string segment = pair.Key.Replace("segment=", "").ToUpper();
						writer.WriteLine($"## {segment} ##");
                    }
					if (pair.Key.ToUpper().StartsWith("CFGDESC="))
                    {
						string description = pair.Key.Replace("cfgdesc=", "");
						string key = pair.Value.Split(':')[0];
						string value = pair.Value.Replace($"{key}: ", "");
						writer.WriteLine($"# {description}");
						writer.WriteLine($"{key}: {value}");
                    }
                }
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

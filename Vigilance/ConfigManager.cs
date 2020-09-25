using System.Collections.Generic;
using Vigilance.Extensions;
using System.IO;
using System;

namespace Vigilance
{
	public static class ConfigManager
	{
		public static bool IsAntiCheatEnabled { get; set; }
		public static bool IsAntiFlyEnabled { get; set; }
		public static bool IsServerGuardEnabled { get; set; }
		public static bool SpawnBlood { get; set; }
		public static bool SpawnDecal { get; set; }
		public static bool SpawnRagdolls { get; set; }
		public static bool ShouldKeepScp268 { get; set; }
		public static bool ShouldTrack { get; set; }
		public static bool ShouldDebug { get; set; }
		public static bool ShouldReloadConfigsOnRoundRestart { get; set; }
		public static bool ShouldKickNewAccounts { get; set; }
		public static bool ShouldKickNonSetupAccounts { get; set; }
		public static bool UnlimitedRadioBattery { get; set; }
		public static bool CanScp049ReviveOther { get; set; }
		public static bool CanTutorialBlockScp173 { get; set; }
		public static bool CanTutorialTriggerScp096 { get; set; }
		public static bool Scp939Amnesia { get; set; }
		public static bool MarkAsModded { get; set; }
		public static bool DisableBloodOnScreen { get; set; }
		public static bool EnableGameCommands { get; set; }
		public static bool EnableCustomCommands { get; set; }

		public static float Scp173DoorCooldown { get; set; }
		public static float Scp106PocketEnterDamage { get; set; }

		public static int ElevatorMovingSpeed { get; set; }
		public static int WindowHealth { get; set; }

		public static int Scp096MaxShield { get; set; }
		public static int Scp096ShieldPerPlayer { get; set; }
		public static int Scp096RechargeRate { get; set; }
		public static bool Scp096PryGates { get; set; }
		public static bool Scp096AddEnrage { get; set; }
		public static bool Scp096CanKillOnlyTargets { get; set; }
		public static bool Scp096CanRegen { get; set; }
		public static bool Scp096VisionParticles { get; set; }
		public static bool Scp096DestroyDoors { get; set; }

		public static string VpnApiKey { get; set; }

		public static List<RoleType> TeslaTriggerableRoles { get; set; }
		public static List<RoleType> AltAllowedRoles { get; set; }
		public static List<string> GuardEnabledModules { get; set; }
		public static List<string> GameCommandsBlacklist { get; set; }
		public static List<string> CustomCommandsBlacklist { get; set; }

		public static string[] CurrentLines { get; set; }

		public static void Reload()
		{
			if (PluginManager.Config == null)
				return;
			try
			{
				if (!File.Exists(Paths.ConfigPath))
				{
					File.Create(Paths.ConfigPath).Close();
					Log.Add("ConfigManager", "Created config file", LogType.Debug);
				}

				CurrentLines = FileManager.ReadAllLines(Paths.ConfigPath);
				Validate();
				PluginManager.Config.Reload();
			}
			catch (Exception e)
			{
				Log.Add("ConfigManager", e);
			}
			PluginManager.Config.Reload();
			IsAntiCheatEnabled = PluginManager.Config.GetBool("anticheat_enabled", true);
			IsAntiFlyEnabled = PluginManager.Config.GetBool("antifly_enabled", true);
			SpawnBlood = PluginManager.Config.GetBool("enable_blood_spawning", true);
			SpawnDecal = PluginManager.Config.GetBool("enable_decal_spawning", true);
			SpawnRagdolls = PluginManager.Config.GetBool("spawn_ragdolls", true);
			ShouldKeepScp268 = PluginManager.Config.GetBool("keep_scp268", true);
			ShouldTrack = PluginManager.Config.GetBool("tracking", false);
			ShouldDebug = PluginManager.Config.GetBool("debug", false);
			ShouldReloadConfigsOnRoundRestart = PluginManager.Config.GetBool("reload_configs", false);
			ShouldKickNewAccounts = PluginManager.Config.GetBool("steam_block_new_accounts", false);
			ShouldKickNonSetupAccounts = PluginManager.Config.GetBool("steam_block_non_setup_accounts", false);
			UnlimitedRadioBattery = PluginManager.Config.GetBool("unlimited_radio_battery", false);
			CanScp049ReviveOther = PluginManager.Config.GetBool("scp049_revive_other", true);
			CanTutorialBlockScp173 = PluginManager.Config.GetBool("can_tutorial_block_scp173", true);
			CanTutorialTriggerScp096 = PluginManager.Config.GetBool("can_tutorial_trigger_scp096", true);
			Scp939Amnesia = PluginManager.Config.GetBool("scp939_amnesia", true);
			MarkAsModded = PluginManager.Config.GetBool("mark_as_modded", false);
			DisableBloodOnScreen = PluginManager.Config.GetBool("disable_blood_on_screen", false);
			EnableGameCommands = PluginManager.Config.GetBool("enable_game_commands", true);
			EnableCustomCommands = PluginManager.Config.GetBool("enable_custom_commands", true);

			Scp173DoorCooldown = PluginManager.Config.GetFloat("scp173_door_cooldown", 25f);
			Scp106PocketEnterDamage = PluginManager.Config.GetFloat("scp106_pocket_enter_damage", 40f);

			ElevatorMovingSpeed = PluginManager.Config.GetInt("elevator_moving_speed", 5);
			WindowHealth = PluginManager.Config.GetInt("window_health", 30);

			TeslaTriggerableRoles = PluginManager.Config.GetRoles("tesla_triggerable_roles");
			AltAllowedRoles = PluginManager.Config.GetRoles("alt_voice_allowed_roles");
			GuardEnabledModules = PluginManager.Config.GetStringList("guard_enabled_modules");
			GameCommandsBlacklist = PluginManager.Config.GetStringList("game_commands_blacklist");
			CustomCommandsBlacklist = PluginManager.Config.GetStringList("custom_commands_blacklist");

			Scp096MaxShield = PluginManager.Config.GetInt("scp096_max_shield", 500);
			Scp096ShieldPerPlayer = PluginManager.Config.GetInt("scp096_shield_per_player", 200);
			Scp096RechargeRate = PluginManager.Config.GetInt("scp096_shield_recharge_rate", 10);
			Scp096PryGates = PluginManager.Config.GetBool("scp096_pry_gates", true);
			Scp096AddEnrage = PluginManager.Config.GetBool("scp096_add_enrage", true);
			Scp096CanKillOnlyTargets = PluginManager.Config.GetBool("scp096_can_kill_only_targets", false);
			Scp096CanRegen = PluginManager.Config.GetBool("scp096_can_regen", true);
			Scp096VisionParticles = PluginManager.Config.GetBool("scp096_vision_particles", true);
			Scp096DestroyDoors = PluginManager.Config.GetBool("scp096_destroy_doors", true);

			VpnApiKey = PluginManager.Config.GetString("vpn_api_key");
		}

		public static void Validate()
		{
			AddConfig(Segment.General, "# Your server will have the \"Modded\" flag in the server list if you set this to true.", "mark_as_modded", "false");
			AddConfig(Segment.General, "# Adds a transparent text that specifies the version of Vigilance.", "tracking", "false");
			AddConfig(Segment.General, "# Whether or not debug messages should be printed in the server console. This option is usually very spammy.", "debug", "false");
			AddConfig(Segment.General, "# Should Vigilance reload all configs when the round restarts?", "reload_configs", "false");
			AddConfig(Segment.General, "# Enable base-game commands", "enable_game_commands", "true");
			AddConfig(Segment.General, "# Enable custom commands", "enable_custom_commands", "true");
			AddConfig(Segment.General, "# List of disabled game commands/blacklisted UserIDs", "game_commands_blacklist", "[]");
			AddConfig(Segment.General, "# List of disabled custom commands/blacklisted UserIDs", "custom_commands_blacklist", "[]");



			AddConfig(Segment.ServerGuard, "# Whether or not should ServerGuard be enabled.", "guard_enabled", "false");
			AddConfig(Segment.ServerGuard, "# List of active ServerGuard modules. Valid values: vpn, vpnshield, steam, steamshield.", "guard_enabled_modules", "[]");
			AddConfig(Segment.ServerGuard, "# You will need a API key used for checking if a specific IP is a VPN connection or not. Warning! GeForce Now is flagged as a VPN too! You can get your key here! \"https://iphub.info/apiKey/newFree\"", "vpn_api_key", "none");
			AddConfig(Segment.ServerGuard, "# ServerGurd will kick players that didnt buy anything on Steam if this is set to true.", "steam_block_new_accounts", "false");
			AddConfig(Segment.ServerGuard, "# # ServerGuard will kick non-setup Steam accounts if this is set to true.", "steam_block_non_setup_accounts", "false");
		

			AddConfig(Segment.Security, "# Whether or not should AntiFly kick or kill players for \"cheating\".", "antifly_enabled", "true");
			AddConfig(Segment.Security, "# Whether or not should AntiCheat teleport players back for \"cheating\".", "anticheat_enabled", "true");

			AddConfig(Segment.Gameplay, "# Whether or not blood should be spawned underneath a player when a player gets hit.", "enable_blood_spawning", "true");
			AddConfig(Segment.Gameplay, "# Whether or not a black hole should be spawned underneath a player when a player gets taken by SCP-106.", "enable_decal_spawning", "true");
			AddConfig(Segment.Gameplay, "# Whether or not a ragdoll should spawn when a player dies/disconnects.", "spawn_ragdolls", "true");
			AddConfig(Segment.Gameplay, "# Whether or not SCP-268 effects should wear off when a player interacts with something.", "keep_scp268", "true");
			AddConfig(Segment.Gameplay, "# Whether or not should radios drain battery.", "unlimited_radio_battery", "false");
			AddConfig(Segment.Gameplay, "# Should a player see blood on his screen when he gets hit?", "disable_blood_on_screen", "false");
			AddConfig(Segment.Gameplay, "# Elevator moving speed, pretty obvious.", "elevator_moving_speed", "5");
			AddConfig(Segment.Gameplay, "# Damage you need to do to break a window.", "window_health", "30");
			AddConfig(Segment.Gameplay, "# List of RoleIDs that will be able to trigger tesla gates.", "tesla_triggerable_roles", "[0,1,3,4,5,6,8,9,10,11,12,13,14,15,16,17]");
			AddConfig(Segment.Gameplay, "# Roles allowed to use SCP-939's V alt voice chat feature.", "alt_voice_allowed_roles", "[16,17]");

			AddConfig(Segment.Scp049, "# Whether or not SCP-049 should be able to revive players that were not killed by SCP-049", "scp049_revive_other", "true");

			AddConfig(Segment.Scp173, "# Whether or not players with tutorial should block SCP-173's movement.", "can_tutorial_block_scp173", "true");
			AddConfig(Segment.Scp173, "# How long does SCP-173 have to wait before it can open the gate when the round starts.", "scp173_door_cooldown", "25");

			AddConfig(Segment.Scp096, "# Whether or not players with tutorial should trigger SCP-096.", "can_tutorial_trigger_scp096", "true");
			AddConfig(Segment.Scp096, "# Starting value of AHP for SCP-096.", "scp096_max_shield", "500");
			AddConfig(Segment.Scp096, "# How much AHP does SCP-096 gain for every player that looks at it.", "scp096_shield_per_player", "200");
			AddConfig(Segment.Scp096, "# Recharge rate of AHP of SCP-096.", "scp096_shield_recharge_rate", "10");
			AddConfig(Segment.Scp096, "# Whether or not SCP-096 should be able to pry gates.", "scp096_pry_gates", "true");
			AddConfig(Segment.Scp096, "# Whether or not SCP-096 should stay enraged for longer when someone looks at it.", "scp096_add_enrage", "true");
			AddConfig(Segment.Scp096, "# Whether or not SCP-096 should be able to kill players that didn't look at it.", "scp096_can_kill_only_targets", "false");
			AddConfig(Segment.Scp096, "# Whether or not SCP-096 should be able to regenerate AHP.", "scp096_can_regen", "true");
			AddConfig(Segment.Scp096, "# Whether or not SCP-096 should be able to see a red particle on every target.", "scp096_vision_particles", "true");
			AddConfig(Segment.Scp096, "# Whether or not SCP-096 should be able to destroy doors.", "scp096_destroy_doors", "true");

			AddConfig(Segment.Scp939, "# Whether or not SCP-939 should give you amnesia.", "scp939_amnesia", "true");

			AddConfig(Segment.Scp106, "# Amount of damage the player takes when SCP-106 sends him into the pocket dimension.", "scp106_pocket_enter_damage", "40");
		}

		public static void AddConfig(Segment segment, string description, string key, string value)
		{
			try
			{
				Paths.CheckFile(Paths.ConfigPath);
				string[] currentLines = File.ReadAllLines(Paths.ConfigPath);
				using (StreamWriter writer = new StreamWriter(Paths.ConfigPath, true))
				{
					if (!currentLines.Contains($"## {segment} configuration ##"))
						writer.WriteLine($"## {segment} configuration ##");
					if (!Paths.ContainsKey(currentLines, key))
					{
						if (!string.IsNullOrEmpty(description) && !currentLines.Contains(description))
							writer.WriteLine($"{description}");
						if (!string.IsNullOrEmpty(value) && !string.IsNullOrEmpty(key))
							writer.WriteLine($"{key}: {value}");
						writer.WriteLine("");
					}
					writer.Flush();
					writer.Close();
				}
			}
			catch (Exception e)
			{
				Log.Add("ConfigManager", "An error ocurred while adding config", LogType.Error);
				Log.Add("ConfigManager", e);
			}
		}

		public enum Segment
		{
			General,
			Scp096,
			Scp049,
			Scp939,
			Scp106,
			Scp173,
			Intercom,
			Gameplay,
			ServerGuard,
			Security,
			None
		}
	}
}

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
		public static bool ShouldDropInventory { get; set; }
		public static bool MakeSureToGiveItems { get; set; }
		public static bool UnlimitedRadioBattery { get; set; }
		public static bool CanScp049ReviveOther { get; set; }
		public static bool CanTutorialBlockScp173 { get; set; }
		public static bool CanTutorialTriggerScp096 { get; set; }
		public static bool Scp939Amnesia { get; set; }
		public static bool MarkAsModded { get; set; }
		public static bool DisableBloodOnScreen { get; set; }
		public static bool UnlimitedMicroEnergy { get; set; }
		public static bool EnableGameCommands { get; set; }
		public static bool EnableCustomCommands { get; set; }

		public static float Scp173DoorCooldown { get; set; }
		public static float Scp106PocketEnterDamage { get; set; }
		public static float MaxAllowedTimeout { get; set; }

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
		public static string Intercom_Ready { get; set; }
		public static string Intercom_Bypass { get; set; }
		public static string Intercom_Transmit { get; set; }
		public static string Intercom_Muted { get; set; }
		public static string Intercom_Admin { get; set; }
		public static string Intercom_Restart { get; set; }

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
					File.Create(Paths.ConfigPath).Close();
				Validate();
				PluginManager.Config.Reload();
			}
			catch (Exception e)
			{
				Log.Add("ConfigManager", e);
			}

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
			ShouldDropInventory = PluginManager.Config.GetBool("drop_inventory", true);
			UnlimitedRadioBattery = PluginManager.Config.GetBool("unlimited_radio_battery", false);
			CanScp049ReviveOther = PluginManager.Config.GetBool("scp049_revive_other", true);
			CanTutorialBlockScp173 = PluginManager.Config.GetBool("can_tutorial_block_scp173", true);
			CanTutorialTriggerScp096 = PluginManager.Config.GetBool("can_tutorial_trigger_scp096", true);
			Scp939Amnesia = PluginManager.Config.GetBool("scp939_amnesia", true);
			MarkAsModded = PluginManager.Config.GetBool("mark_as_modded", false);
			DisableBloodOnScreen = PluginManager.Config.GetBool("disable_blood_on_screen", false);
			EnableGameCommands = PluginManager.Config.GetBool("enable_game_commands", true);
			EnableCustomCommands = PluginManager.Config.GetBool("enable_custom_commands", true);
			MakeSureToGiveItems = PluginManager.Config.GetBool("make_sure_to_give_items", true);
			UnlimitedMicroEnergy = PluginManager.Config.GetBool("unlimited_micro_energy", false);

			Scp173DoorCooldown = PluginManager.Config.GetFloat("scp173_door_cooldown", 25f);
			Scp106PocketEnterDamage = PluginManager.Config.GetFloat("scp106_pocket_enter_damage", 40f);
			MaxAllowedTimeout = PluginManager.Config.GetFloat("max_allowed_timeout", 45f);

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

			Intercom_Admin = PluginManager.Config.GetString("intercom_admin", "ADMIN IS USING THE INTERCOM NOW");
			Intercom_Bypass = PluginManager.Config.GetString("intercom_bypass", "TRANSMITTING...BYPASS MODE");
			Intercom_Muted = PluginManager.Config.GetString("intercom_muted", "YOU ARE MUTED BY ADMIN");
			Intercom_Ready = PluginManager.Config.GetString("intercom_ready", "READY");
			Intercom_Restart = PluginManager.Config.GetString("intercom_restarting", "RESTARTING %remaining%");
			Intercom_Transmit = PluginManager.Config.GetString("intercom_transmitting", "TRANSMITTING...TIME LEFT - %time%");
		}

		public static bool IsBlacklisted(string command, string role, string userId)
        {
			List<string> roleBlacklist = PluginManager.Config.GetStringList($"command_blacklist_{role}");
			List<string> idBlacklist = PluginManager.Config.GetStringList($"command_blacklist_{userId}");
			if (roleBlacklist.Contains(command) || idBlacklist.Contains(command))
				return true;
			else
				return false;
        }

		public static void Validate()
		{
			AddConfig("Your server will have the \"Modded\" flag in the server list if you set __instance to true.", "mark_as_modded", "false");
			AddConfig("Adds a transparent text that specifies the version of Vigilance.", "tracking", "false");
			AddConfig("Whether or not debug messages should be printed in the server console. This option is usually very spammy.", "debug", "false");
			AddConfig("Should Vigilance reload all configs when the round restarts?", "reload_configs", "false");
			AddConfig("Enable base-game commands", "enable_game_commands", "true");
			AddConfig("Enable custom commands", "enable_custom_commands", "true");
			AddConfig("List of disabled game commands/blacklisted UserIDs", "game_commands_blacklist", "[]");
			AddConfig("List of disabled custom commands/blacklisted UserIDs", "custom_commands_blacklist", "[]");

			AddConfig("Whether or not should ServerGuard be enabled.", "guard_enabled", "false");
			AddConfig("List of active ServerGuard modules. Valid values: vpn, vpnshield, steam, steamshield.", "guard_enabled_modules", "[]");
			AddConfig("You will need a API key used for checking if a specific IP is a VPN connection or not. Warning! GeForce Now is flagged as a VPN too! You can get your key here! \"https://iphub.info/apiKey/newFree\"", "vpn_api_key", "none");
			AddConfig("ServerGurd will kick players that didnt buy anything on Steam if __instance is set to true.", "steam_block_new_accounts", "false");
			AddConfig("ServerGuard will kick non-setup Steam accounts if __instance is set to true.", "steam_block_non_setup_accounts", "false");
		
			AddConfig("Whether or not should AntiFly kick or kill players for \"cheating\".", "antifly_enabled", "true");
			AddConfig("Whether or not should AntiCheat teleport players back for \"cheating\".", "anticheat_enabled", "true");

			AddConfig("Whether or not blood should be spawned underneath a player when a player gets hit.", "enable_blood_spawning", "true");
			AddConfig("Whether or not a black hole should be spawned underneath a player when a player gets taken by SCP-106.", "enable_decal_spawning", "true");
			AddConfig("Whether or not a ragdoll should spawn when a player dies/disconnects.", "spawn_ragdolls", "true");
			AddConfig("Whether or not SCP-268 effects should wear off when a player interacts with something.", "keep_scp268", "true");
			AddConfig("Whether or not should radios drain battery.", "unlimited_radio_battery", "false");
			AddConfig("Should a player see blood on his screen when he gets hit?", "disable_blood_on_screen", "false");
			AddConfig("Elevator moving speed, pretty obvious.", "elevator_moving_speed", "5");
			AddConfig("Damage you need to do to break a window.", "window_health", "30");
			AddConfig("List of RoleIDs that will be able to trigger tesla gates.", "tesla_triggerable_roles", "[0,1,3,4,5,6,8,9,10,11,12,13,14,15,16,17]");
			AddConfig("Roles allowed to use SCP-939's V alt voice chat feature.", "alt_voice_allowed_roles", "[16,17]");
			AddConfig("Indicates whether the inventory should be dropped before being set as spectator", "drop_inventory", "true");
			AddConfig("Should fix the issue with missing items due to inventory being null .. goddamnit NW", "make_sure_to_give_items", "false");
			AddConfig("If the energy of MicroHID is infinite or not.", "unlimited_micro_energy", "false");
			AddConfig("Maximum allowed timeout while connecting. If the player does not connect in __instance specified time, then the player will be kicked.", "max_allowed_timeout", "45");

			AddConfig("Whether or not SCP-049 should be able to revive players that were not killed by SCP-049", "scp049_revive_other", "true");

			AddConfig("Whether or not players with tutorial should block SCP-173's movement.", "can_tutorial_block_scp173", "true");
			AddConfig("How long does SCP-173 have to wait before it can open the gate when the round starts.", "scp173_door_cooldown", "25");

			AddConfig("Whether or not players with tutorial should trigger SCP-096.", "can_tutorial_trigger_scp096", "true");
			AddConfig("Starting value of AHP for SCP-096.", "scp096_max_shield", "500");
			AddConfig("How much AHP does SCP-096 gain for every player that looks at it.", "scp096_shield_per_player", "200");
			AddConfig("Recharge rate of AHP of SCP-096.", "scp096_shield_recharge_rate", "10");
			AddConfig("Whether or not SCP-096 should be able to pry gates.", "scp096_pry_gates", "true");
			AddConfig("Whether or not SCP-096 should stay enraged for longer when someone looks at it.", "scp096_add_enrage", "true");
			AddConfig("Whether or not SCP-096 should be able to kill players that didn't look at it.", "scp096_can_kill_only_targets", "false");
			AddConfig("Whether or not SCP-096 should be able to regenerate AHP.", "scp096_can_regen", "true");
			AddConfig("Whether or not SCP-096 should be able to see a red particle on every target.", "scp096_vision_particles", "true");
			AddConfig("Whether or not SCP-096 should be able to destroy doors.", "scp096_destroy_doors", "true");

			AddConfig("Whether or not SCP-939 should give you amnesia.", "scp939_amnesia", "true");

			AddConfig("Amount of damage the player takes when SCP-106 sends him into the pocket dimension.", "scp106_pocket_enter_damage", "40");

			AddConfig("Message to display when an admin is using the Intercom.", "intercom_admin", "ADMIN IS USING THE INTERCOM NOW");
			AddConfig("Message to display when someone with bypass mode is using the Intercom.", "intercom_bypass", "TRANSMITTING...BYPASS MODE");
			AddConfig("Message to display when a muted player tries to use the Intercom.", "intercom_muted", "YOU ARE MUTED BY ADMIN");
			AddConfig("Message to display when Intercom is ready to use.", "intercom_ready", "READY");
			AddConfig("Message to display while Intercom is restarting. Use %remaining% for remaining cooldown time.", "intercom_restarting", "RESTARTING %remaining%");
			AddConfig("Message to display when someone is using the Intercom. Use %time% for remaining speech time.", "intercom_transmitting", "TRANSMITTING...TIME LEFT - %time%");

			AddConfig("Custom command blacklist, use this to disable a specific command for a specific UserID or a role.", "# Example: command_blacklist_76561198xxxx@steam", "[help,grenade]");
		}

		public static void AddConfig(string description, string key, string value)
		{
			try
			{
				Paths.CheckFile(Paths.ConfigPath);
				string[] currentLines = File.ReadAllLines(Paths.ConfigPath);
				using (StreamWriter writer = new StreamWriter(Paths.ConfigPath, true))
				{
					if (!Paths.ContainsKey(currentLines, key))
					{
						if (!string.IsNullOrEmpty(description) && !currentLines.Contains($"# {description}"))
							writer.WriteLine($"# {description}");
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
	}
}

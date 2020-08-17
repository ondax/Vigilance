using CommandSystem;
using GameCore;
using Mirror;
using Mirror.LiteNetLib4Mirror;
using NorthwoodLib;
using NorthwoodLib.Pools;
using PlayableScps;
using Respawning;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEngine;
using Utils.CommandInterpolation;
using Harmony;
using RemoteAdmin;
using static RemoteAdmin.CommandProcessor;
using Vigilance.Extensions;
using Console = GameCore.Console;
using static GameCore.Console;
using Cryptography;
using MEC;
using Org.BouncyCastle.Crypto;
using System.IO;
using System.Threading;
using UnityEngine.SceneManagement;

namespace Vigilance.Patches
{
    [HarmonyPatch(typeof(CommandProcessor), nameof(CommandProcessor.ProcessQuery))]
    public static class ProcessQueryPatch
    {
		public static bool Prefix(string q, CommandSender sender)
		{
			try
			{
				if (sender == null)
					sender = ServerConsole._scs;
				string[] query = q.Split(' ');
				string logName = sender.LogName;
				PlayerCommandSender playerCommandSender = sender as PlayerCommandSender;
				QueryProcessor queryProcessor = playerCommandSender?.Processor;
				if (q.StartsWith("@", StringComparison.Ordinal))
				{
					if (!CheckPermissions(sender, "Admin Chat", PlayerPermissions.AdminChat, string.Empty))
					{
						playerCommandSender?.Processor.TargetAdminChatAccessDenied(playerCommandSender.Processor.connectionToClient);
						return false;
					}
					q = q + " ~" + sender.Nickname;
					foreach (KeyValuePair<GameObject, ReferenceHub> allHub in ReferenceHub.GetAllHubs())
					{
						if ((allHub.Value.serverRoles.AdminChatPerms || allHub.Value.serverRoles.RaEverywhere) && !allHub.Value.isDedicatedServer && allHub.Value.isReady)
						{
							allHub.Value.queryProcessor.TargetReply(allHub.Value.queryProcessor.connectionToClient, q, isSuccess: true, logInConsole: false, string.Empty);
						}
					}
					return false;
				}
				if (CommandProcessor.RemoteAdminCommandHandler.TryGetCommand(query[0], out ICommand command))
				{
					try
					{
						string response;
						bool success = command.Execute(query.Segment(1), sender, out response);
						if (!string.IsNullOrEmpty(response))
						{
							sender.RaReply(query[0].ToUpper() + "#" + response, success, logToConsole: true, "");
						}
					}
					catch (Exception arg2)
					{
						sender.RaReply(query[0].ToUpper() + "# Command execution failed! Error: " + arg2, success: false, logToConsole: true, "");
					}
					return false;
				}
				GameCommandHandler gameCommandHandler = CommandManager.GetGameCommandHandler(query[0]);
				if (gameCommandHandler != null)
				{
					sender.RaReply($"SERVER#{gameCommandHandler.Execute(sender.GetPlayer(), query.SkipCommand())}", true, true, "");
					return false;
				}
				Environment.OnRemoteAdminCommand(sender, q, true, out bool allow, out string reply);
				if (!allow)
				{
					sender.RaReply($"SERVER#{reply}", true, true, "");
					return false;
				}
				int failures;
				int successes;
				string error;
				bool replySent;
				switch (query[0].ToUpper())
				{
					case "ENRAGE":
						if (CheckPermissions(sender, query[0], new PlayerPermissions[2]
						{
							PlayerPermissions.ForceclassSelf,
							PlayerPermissions.ForceclassWithoutRestrictions
						}))
						{
							PlayableScpsController component4;
							Scp096 scp;
							foreach (ReferenceHub hub in ReferenceHub.GetAllHubs().Values)
							{
								if (hub.queryProcessor.TryGetComponent(out component4) && (scp = (component4.CurrentScp as Scp096)) != null)
								{
									ServerLogs.AddLog(ServerLogs.Modules.Administrative, logName + " enraged SCP-096.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
									scp.Windup(force: true);
								}
							}
							sender.RaReply(query[0].ToUpper() + "#Setting 096 into rage mode...", success: true, logToConsole: true, "");
						}
						break;
					case "HELLO":
						sender.RaReply(query[0].ToUpper() + "#Hello there!", success: true, logToConsole: true, "");
						break;
					case "HELP":
						sender.RaReply(query[0].ToUpper() + "#This should be useful! (spoilers: it isn't)", success: true, logToConsole: true, "");
						break;
					case "CASSIE":
						if (CheckPermissions(sender, query[0], PlayerPermissions.Announcer))
						{
							if (query.Length > 1)
							{
								ServerLogs.AddLog(ServerLogs.Modules.Administrative, logName + " ran the cassie command (parameters: " + q.Remove(0, 7) + ").", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
								RespawnEffectsController.PlayCassieAnnouncement(q.Remove(0, 7), makeHold: false, makeNoise: true);
							}
							else
							{
								sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", success: false, logToConsole: true, "");
							}
						}
						break;
					case "CASSIE_SILENTNOISE":
					case "CASSIE_SN":
					case "CASSIE_SILENT":
					case "CASSIE_SL":
						if (CheckPermissions(sender, query[0], PlayerPermissions.Announcer))
						{
							if (query.Length > 1)
							{
								ServerLogs.AddLog(ServerLogs.Modules.Administrative, logName + " ran the cassie command (parameters: " + q.Remove(0, 7) + ").", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
								RespawnEffectsController.PlayCassieAnnouncement(q.Remove(0, 7), makeHold: false, makeNoise: false);
							}
							else
							{
								sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", success: false, logToConsole: true, "");
							}
						}
						break;
					case "BROADCAST":
					case "BC":
					case "ALERT":
					case "BROADCASTMONO":
					case "BCMONO":
					case "ALERTMONO":
						if (CheckPermissions(sender, query[0], PlayerPermissions.Broadcasting))
						{
							if (query.Length < 2)
							{
								sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", success: false, logToConsole: true, "");
								break;
							}
							if (!ushort.TryParse(query[1], out ushort result11) || result11 < 1)
							{
								sender.RaReply(query[0].ToUpper() + "#First argument must be a positive integer.", success: false, logToConsole: true, "");
								break;
							}
							string text18 = q.Substring(query[0].Length + query[1].Length + 2);
							ServerLogs.AddLog(ServerLogs.Modules.Administrative, logName + " ran the broadcast command (duration: " + query[1] + " seconds) with text \"" + text18 + "\" players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
							QueryProcessor.Localplayer.GetComponent<Broadcast>().RpcAddElement(text18, result11, query[0].Contains("mono", StringComparison.OrdinalIgnoreCase) ? Broadcast.BroadcastFlags.Monospaced : Broadcast.BroadcastFlags.Normal);
							sender.RaReply(query[0].ToUpper() + "#Broadcast sent.", success: false, logToConsole: true, "");
						}
						break;
					case "CL":
					case "CLEARBC":
					case "BCCLEAR":
					case "CLEARALERT":
					case "ALERTCLEAR":
						if (CheckPermissions(sender, query[0], PlayerPermissions.Broadcasting))
						{
							ServerLogs.AddLog(ServerLogs.Modules.Administrative, logName + " ran the cleared all broadcasts.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
							QueryProcessor.Localplayer.GetComponent<Broadcast>().RpcClearElements();
							sender.RaReply(query[0].ToUpper() + "#All broadcasts cleared.", success: false, logToConsole: true, "");
						}
						break;
					case "BAN":
					case "OFFLINEBAN":
					case "OBAN":
						if (query.Length >= 3)
						{
							string text10 = string.Empty;
							if (query.Length > 3)
							{
								text10 = query.Skip(3).Aggregate((string current, string n) => current + " " + n);
							}
							int num4 = 0;
							try
							{
								num4 = Misc.RelativeTimeToSeconds(query[2], 60);
							}
							catch
							{
								sender.RaReply(query[0].ToUpper() + "#Invalid time: " + query[2], success: false, logToConsole: true, "");
								return false;
							}
							if (num4 < 0)
							{
								num4 = 0;
								query[2] = "0";
							}
							if ((num4 == 0 && !CheckPermissions(sender, query[0].ToUpper(), new PlayerPermissions[3]
							{
								PlayerPermissions.KickingAndShortTermBanning,
								PlayerPermissions.BanningUpToDay,
								PlayerPermissions.LongTermBanning
							})) || (num4 > 0 && num4 <= 3600 && !CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.KickingAndShortTermBanning)) || (num4 > 3600 && num4 <= 86400 && !CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.BanningUpToDay)) || (num4 > 86400 && !CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.LongTermBanning)))
							{
								break;
							}
							if (query[0].Equals("BAN", StringComparison.OrdinalIgnoreCase))
							{
								List<int> list2 = Misc.ProcessRaPlayersList(query[1]);
								if (list2 == null)
								{
									sender.RaReply(query[0].ToUpper() + "#Invalid players list.", success: false, logToConsole: true, "");
									break;
								}
								ushort num5 = 0;
								ushort num6 = 0;
								string text11 = string.Empty;
								foreach (int item in list2)
								{
									try
									{
										ReferenceHub hub2 = ReferenceHub.GetHub(item);
										if (hub2 == null)
										{
											num6 = (ushort)(num6 + 1);
											continue;
										}
										string combinedName = hub2.nicknameSync.CombinedName;
										if (sender.FullPermissions)
										{
											goto IL_1971;
										}
										byte b = hub2.serverRoles.Group?.RequiredKickPower ?? 0;
										if (b <= sender.KickPower)
										{
											goto IL_1971;
										}
										num6 = (ushort)(num6 + 1);
										text11 = $"You can't kick/ban {combinedName}. Required kick power: {b}, your kick power: {sender.KickPower}.";
										sender.RaReply(text11, success: false, logToConsole: true, string.Empty);
										goto end_IL_18dd;
									IL_1971:
										ServerLogs.AddLog(ServerLogs.Modules.Administrative, logName + " banned player " + hub2.LoggedNameFromRefHub() + ". Ban duration: " + query[2] + ". Reason: " + ((text10 == string.Empty) ? "(none)" : text10) + ".", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
										if (ServerStatic.PermissionsHandler.IsVerified && hub2.serverRoles.BypassStaff)
										{
											QueryProcessor.Localplayer.GetComponent<BanPlayer>().BanUser(hub2.gameObject, 0, text10, sender.Nickname);
										}
										else
										{
											if (num4 == 0 && ConfigFile.ServerConfig.GetBool("broadcast_kicks"))
											{
												QueryProcessor.Localplayer.GetComponent<Broadcast>().RpcAddElement(ConfigFile.ServerConfig.GetString("broadcast_kick_text", "%nick% has been kicked from this server.").Replace("%nick%", combinedName), ConfigFile.ServerConfig.GetUShort("broadcast_kick_duration", 5), Broadcast.BroadcastFlags.Normal);
											}
											else if (num4 != 0 && ConfigFile.ServerConfig.GetBool("broadcast_bans", def: true))
											{
												QueryProcessor.Localplayer.GetComponent<Broadcast>().RpcAddElement(ConfigFile.ServerConfig.GetString("broadcast_ban_text", "%nick% has been banned from this server.").Replace("%nick%", combinedName), ConfigFile.ServerConfig.GetUShort("broadcast_ban_duration", 5), Broadcast.BroadcastFlags.Normal);
											}
											QueryProcessor.Localplayer.GetComponent<BanPlayer>().BanUser(hub2.gameObject, num4, text10, sender.Nickname);
										}
										num5 = (ushort)(num5 + 1);
									end_IL_18dd:;
									}
									catch (Exception ex4)
									{
										num6 = (ushort)(num6 + 1);
										Debug.Log(ex4);
										text11 = "Error occured during banning: " + ex4.Message + ".\n" + ex4.StackTrace;
									}
								}
								if (num6 == 0)
								{
									string text12 = "Banned";
									if (int.TryParse(query[2], out int result2))
									{
										text12 = ((result2 > 0) ? "Banned" : "Kicked");
									}
									sender.RaReply(query[0] + "#Done! " + text12 + " " + num5 + " player(s)!", success: true, logToConsole: true, "");
								}
								else
								{
									sender.RaReply(query[0] + "#The process has occured an issue! Failures: " + num6 + "\nLast error log:\n" + text11, success: false, logToConsole: true, "");
								}
							}
							else
							{
								bool flag10 = Misc.IsIpAddress(query[1]);
								if (!flag10 && !query[1].Contains("@"))
								{
									sender.RaReply(query[0].ToUpper() + "#Target must be a valid UserID or IP (v4 or v6) address.", success: false, logToConsole: true, "");
									break;
								}
								ServerLogs.AddLog(ServerLogs.Modules.Administrative, logName + " banned an offline player with " + (flag10 ? "IP address" : "UserID") + query[1] + ". Ban duration: " + query[2] + ". Reason: " + ((text10 == string.Empty) ? "(none)" : text10) + ".", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
								BanHandler.IssueBan(new BanDetails
								{
									OriginalName = "Unknown - offline ban",
									Id = query[1],
									IssuanceTime = TimeBehaviour.CurrentTimestamp(),
									Expires = TimeBehaviour.GetBanExpieryTime((uint)num4),
									Reason = text10,
									Issuer = sender.Nickname
								}, flag10 ? BanHandler.BanType.IP : BanHandler.BanType.UserId);
								sender.RaReply(query[0].ToUpper() + "#" + (flag10 ? "IP address " : "UserID ") + query[1] + " has been banned from this server.", success: true, logToConsole: true, string.Empty);
							}
						}
						else
						{
							sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 3 arguments! (some parameters are missing)", success: false, logToConsole: true, "");
						}
						break;
					case "GBAN-KICK":
						if (playerCommandSender == null || (!playerCommandSender.SR.RaEverywhere && !playerCommandSender.SR.Staff))
						{
							sender.RaReply(query[0].ToUpper() + "#You don't have permissions to run this command!", success: false, logToConsole: true, "");
						}
						if (query.Length != 2)
						{
							sender.RaReply(query[0].ToUpper() + "#To run this program, type exactly 1 argument! (some parameters are missing)", success: false, logToConsole: true, "");
							break;
						}
						ServerLogs.AddLog(ServerLogs.Modules.Administrative, logName + " globally banned and kicked " + query[1] + " player.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
						StandardizedQueryModel1(sender, query[0], query[1], "0", out failures, out successes, out error, out replySent);
						break;
					case "TK":
					case "TKD":
					case "FFD":
						if (query.Length != 2)
						{
							sender.RaReply("FFD#Syntax: FFD <Player ID/status/pause/unpause>", success: false, logToConsole: true, string.Empty);
							break;
						}
						switch (query[1].ToLower(CultureInfo.InvariantCulture))
						{
							case "status":
								if (CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.FriendlyFireDetectorTempDisable, string.Empty))
								{
									sender.RaReply("FFD#Friendly fire detector is currently " + (FriendlyFireConfig.PauseDetector ? string.Empty : "**NOT** ") + "paused.", success: true, logToConsole: true, string.Empty);
								}
								break;
							case "pause":
								if (CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.FriendlyFireDetectorTempDisable, string.Empty))
								{
									if (FriendlyFireConfig.PauseDetector)
									{
										sender.RaReply("FFD#Friendly fire detector is already paused.", success: false, logToConsole: true, string.Empty);
										break;
									}
									FriendlyFireConfig.PauseDetector = true;
									sender.RaReply("FFD#Friendly fire detector has been paused.", success: true, logToConsole: true, string.Empty);
									ServerLogs.AddLog(ServerLogs.Modules.Administrative, logName + " has paused Friendly Fire Detector.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
								}
								break;
							case "unpause":
								if (CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.FriendlyFireDetectorTempDisable, string.Empty))
								{
									if (!FriendlyFireConfig.PauseDetector)
									{
										sender.RaReply("FFD#Friendly fire detector is not paused.", success: false, logToConsole: true, string.Empty);
										break;
									}
									FriendlyFireConfig.PauseDetector = false;
									sender.RaReply("FFD#Friendly fire detector has been unpaused.", success: true, logToConsole: true, string.Empty);
									ServerLogs.AddLog(ServerLogs.Modules.Administrative, logName + " has unpaused Friendly Fire Detector.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
								}
								break;
							default:
								{
									if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PlayersManagement, string.Empty))
									{
										break;
									}
									if (!int.TryParse(query[1], out int id3))
									{
										sender.RaReply("FFD#Player ID must be an integer.", success: false, logToConsole: true, string.Empty);
										break;
									}
									if (query[1].Contains("."))
									{
										sender.RaReply("FFD#FFD command requires exact one selected player.", success: false, logToConsole: true, string.Empty);
										break;
									}
									GameObject gameObject6 = PlayerManager.players.FirstOrDefault((GameObject pl) => pl.GetComponent<QueryProcessor>().PlayerId == id3);
									if (gameObject6 == null)
									{
										sender.RaReply("FFD#Can't find requested player.", success: false, logToConsole: true, string.Empty);
										break;
									}
									FriendlyFireHandler friendlyFireHandler = ReferenceHub.GetHub(gameObject6.gameObject).FriendlyFireHandler;
									sender.RaReply($"FFD#--- Friendly Fire Detector Stats ---\nKills - Damage dealt\n\nRound: {friendlyFireHandler.Round.Kills} - {friendlyFireHandler.Round.Damage}\nLife: {friendlyFireHandler.Life.Kills} - {friendlyFireHandler.Life.Damage}\nWindow: {friendlyFireHandler.Window.Kills} - {friendlyFireHandler.Window.Damage} [Window: {FriendlyFireConfig.Window}s]\nRespawn: {friendlyFireHandler.Respawn.Kills} - {friendlyFireHandler.Respawn.Damage} [Window: {FriendlyFireConfig.RespawnWindow}s]", success: true, logToConsole: true, string.Empty);
									break;
								}
						}
						break;
					case "SUDO":
					case "RCON":
						{
							if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.ServerConsoleCommands))
							{
								break;
							}
							if (query.Length < 2)
							{
								sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 1 argument! (some parameters are missing)", success: false, logToConsole: true, "");
								break;
							}
							if (query[1].StartsWith("!") && !ServerStatic.RolesConfig.GetBool("allow_central_server_commands_as_ServerConsoleCommands"))
							{
								sender.RaReply(query[0] + "#Running central server commands in Remote Admin is disabled in RA config file!", success: false, logToConsole: true, "");
								break;
							}
							string text15 = query.Skip(1).Aggregate("", (string current, string arg) => current + arg + " ");
							text15 = text15.Substring(0, text15.Length - 1);
							ServerLogs.AddLog(ServerLogs.Modules.Administrative, logName + " executed command as server console: " + text15 + " player.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
							ServerConsole.EnterCommand(text15, out ConsoleColor _, sender);
							sender.RaReply(query[0] + "#Command \"" + text15 + "\" executed in server console!", success: true, logToConsole: true, "");
							break;
						}
					case "SNR":
					case "STOPNEXTROUND":
						if (CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.ServerConsoleCommands))
						{
							ServerStatic.StopNextRound = !ServerStatic.StopNextRound;
							sender.RaReply(query[0] + "#Server " + (ServerStatic.StopNextRound ? "WILL" : "WON'T") + " stop after next round.", success: true, logToConsole: true, "");
						}
						break;
					case "SETGROUP":
						if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.SetGroup))
						{
							break;
						}
						if (!ConfigFile.ServerConfig.GetBool("online_mode", def: true))
						{
							sender.RaReply(query[0] + "#This command requires the server to operate in online mode!", success: false, logToConsole: true, "");
						}
						else if (query.Length >= 3)
						{
							ServerLogs.AddLog(ServerLogs.Modules.Permissions, logName + " ran the setgroup command (new group: " + query[2] + " min) on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
							StandardizedQueryModel1(sender, query[0], query[1], query[2], out failures, out successes, out error, out replySent);
							if (!replySent)
							{
								if (failures == 0)
								{
									sender.RaReply(query[0] + "#Done! The request affected " + successes + " player(s)!", success: true, logToConsole: true, "");
								}
								else
								{
									sender.RaReply(query[0] + "#The proccess has occured an issue! Failures: " + failures + "\nLast error log:\n" + error, success: false, logToConsole: true, "");
								}
							}
						}
						else
						{
							sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 3 arguments! (some parameters are missing)", success: false, logToConsole: true, "");
						}
						break;
					case "PM":
						{
							if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PermissionsManagement))
							{
								break;
							}
							Dictionary<string, UserGroup> allGroups = ServerStatic.PermissionsHandler.GetAllGroups();
							List<string> allPermissions = ServerStatic.PermissionsHandler.GetAllPermissions();
							if (query.Length == 1)
							{
								sender.RaReply(query[0].ToUpper() + "#Permissions manager help:\nSyntax: " + query[0] + " action\n\nAvailable actions:\ngroups - lists all groups\ngroup info <group name> - prints group info\ngroup grant <group name> <permission name> - grants permission to a group\ngroup revoke <group name> <permission name> - revokes permission from a group\ngroup setcolor <group name> <color name> - sets group color\ngroup settag <group name> <tag> - sets group tag\ngroup enablecover <group name> - enables badge cover for group\ngroup disablecover <group name> - disables badge cover for group\n\nusers - lists all privileged users\nsetgroup <UserID> <group name> - sets membership of user (use group name \"-1\" to remove user from group)\nreload - reloads permission file\n\n\"< >\" are only used to indicate the arguments, don't put them\nMore commands will be added in the next versions of the game", success: true, logToConsole: true, "");
							}
							else if (string.Equals(query[1], "groups", StringComparison.OrdinalIgnoreCase))
							{
								int num2 = 0;
								string text3 = "\n";
								string text4 = "";
								string[] source = new string[29]
								{
						"BN1",
						"BN2",
						"BN3",
						"FSE",
						"FSP",
						"FWR",
						"GIV",
						"EWA",
						"ERE",
						"ERO",
						"SGR",
						"GMD",
						"OVR",
						"FCM",
						"PLM",
						"PRM",
						"SSC",
						"VHB",
						"CFG",
						"BRC",
						"CDA",
						"NCP",
						"AFK",
						"ATC",
						"GHB",
						"ANN",
						"EFF",
						"FFI",
						"FFT"
								};
								int num3 = (int)Math.Ceiling((double)allPermissions.Count / 12.0);
								for (int i = 0; i < num3; i++)
								{
									num2 = 0;
									text3 = text3 + "\n-----" + source.Skip(i * 12).Take(12).Aggregate((string current, string adding) => current + " " + adding);
									foreach (KeyValuePair<string, UserGroup> item2 in allGroups)
									{
										if (i == 0)
										{
											text4 = text4 + "\n" + num2 + " - " + item2.Key;
										}
										string text5 = num2.ToString();
										for (int j = text5.Length; j < 5; j++)
										{
											text5 += " ";
										}
										foreach (string item3 in allPermissions.Skip(i * 12).Take(12))
										{
											text5 = text5 + "  " + (ServerStatic.PermissionsHandler.IsPermitted(item2.Value.Permissions, item3) ? "X" : " ") + " ";
										}
										num2++;
										text3 = text3 + "\n" + text5;
									}
								}
								sender.RaReply(query[0].ToUpper() + "#All defined groups: " + text3 + "\n" + text4, success: true, logToConsole: true, "");
							}
							else if (string.Equals(query[1], "group", StringComparison.OrdinalIgnoreCase) && query.Length == 2)
							{
								sender.RaReply(query[0].ToUpper() + "#Unknown action. Run " + query[0] + " to get list of all actions.", success: false, logToConsole: true, "");
							}
							else if (string.Equals(query[1], "group", StringComparison.OrdinalIgnoreCase) && query.Length > 2)
							{
								if (string.Equals(query[2], "info", StringComparison.OrdinalIgnoreCase) && query.Length == 4)
								{
									KeyValuePair<string, UserGroup> keyValuePair = allGroups.FirstOrDefault((KeyValuePair<string, UserGroup> gr) => gr.Key == query[3]);
									if (keyValuePair.Key == null)
									{
										sender.RaReply(query[0].ToUpper() + "#Group can't be found.", success: false, logToConsole: true, "");
										break;
									}
									sender.RaReply(query[0].ToUpper() + "#Details of group " + keyValuePair.Key + "\nTag text: " + keyValuePair.Value.BadgeText + "\nTag color: " + keyValuePair.Value.BadgeColor + "\nPermissions: " + keyValuePair.Value.Permissions + "\nCover: " + (keyValuePair.Value.Cover ? "YES" : "NO") + "\nHidden by default: " + (keyValuePair.Value.HiddenByDefault ? "YES" : "NO") + "\nKick power: " + keyValuePair.Value.KickPower + "\nRequired kick power: " + keyValuePair.Value.RequiredKickPower, success: true, logToConsole: true, "");
								}
								else if ((string.Equals(query[2], "grant", StringComparison.OrdinalIgnoreCase) || string.Equals(query[2], "revoke", StringComparison.OrdinalIgnoreCase)) && query.Length == 5)
								{
									if (allGroups.FirstOrDefault((KeyValuePair<string, UserGroup> gr) => gr.Key == query[3]).Key == null)
									{
										sender.RaReply(query[0].ToUpper() + "#Group can't be found.", success: false, logToConsole: true, "");
										break;
									}
									if (!allPermissions.Contains(query[4]))
									{
										sender.RaReply(query[0].ToUpper() + "#Permission can't be found.", success: false, logToConsole: true, "");
										break;
									}
									Dictionary<string, string> stringDictionary = ServerStatic.RolesConfig.GetStringDictionary("Permissions");
									List<string> list = null;
									foreach (string key in stringDictionary.Keys)
									{
										if (!(key != query[4]))
										{
											list = YamlConfig.ParseCommaSeparatedString(stringDictionary[key]).ToList();
										}
									}
									if (list == null)
									{
										sender.RaReply(query[0].ToUpper() + "#Permission can't be found in the config.", success: false, logToConsole: true, "");
										break;
									}
									if (list.Contains(query[3]) && string.Equals(query[2], "grant", StringComparison.OrdinalIgnoreCase))
									{
										sender.RaReply(query[0].ToUpper() + "#Group already has that permission.", success: false, logToConsole: true, "");
										break;
									}
									if (!list.Contains(query[3]) && string.Equals(query[2], "revoke", StringComparison.OrdinalIgnoreCase))
									{
										sender.RaReply(query[0].ToUpper() + "#Group already doesn't have that permission.", success: false, logToConsole: true, "");
										break;
									}
									if (string.Equals(query[2], "grant", StringComparison.OrdinalIgnoreCase))
									{
										list.Add(query[3]);
									}
									else
									{
										list.Remove(query[3]);
									}
									list.Sort();
									string text6 = "[";
									foreach (string item4 in list)
									{
										if (text6 != "[")
										{
											text6 += ", ";
										}
										text6 += item4;
									}
									text6 += "]";
									ServerStatic.RolesConfig.SetStringDictionaryItem("Permissions", query[4], text6);
									ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
									sender.RaReply(query[0].ToUpper() + "#Permissions updated.", success: true, logToConsole: true, "");
								}
								else if (string.Equals(query[2], "setcolor", StringComparison.OrdinalIgnoreCase) && query.Length == 5)
								{
									if (allGroups.FirstOrDefault((KeyValuePair<string, UserGroup> gr) => gr.Key == query[3]).Key == null)
									{
										sender.RaReply(query[0].ToUpper() + "#Group can't be found.", success: false, logToConsole: true, "");
										break;
									}
									ServerStatic.RolesConfig.SetString(query[3] + "_color", query[4]);
									ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
									sender.RaReply(query[0].ToUpper() + "#Group color updated.", success: true, logToConsole: true, "");
								}
								else if (string.Equals(query[2], "settag", StringComparison.OrdinalIgnoreCase) && query.Length == 5)
								{
									if (allGroups.FirstOrDefault((KeyValuePair<string, UserGroup> gr) => gr.Key == query[3]).Key == null)
									{
										sender.RaReply(query[0].ToUpper() + "#Group can't be found.", success: false, logToConsole: true, "");
										break;
									}
									ServerStatic.RolesConfig.SetString(query[3] + "_badge", query[4]);
									ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
									sender.RaReply(query[0].ToUpper() + "#Group tag updated.", success: true, logToConsole: true, "");
								}
								else if (string.Equals(query[2], "enablecover", StringComparison.OrdinalIgnoreCase) && query.Length == 4)
								{
									if (allGroups.FirstOrDefault((KeyValuePair<string, UserGroup> gr) => gr.Key == query[3]).Key == null)
									{
										sender.RaReply(query[0].ToUpper() + "#Group can't be found.", success: false, logToConsole: true, "");
										break;
									}
									ServerStatic.RolesConfig.SetString(query[3] + "_cover", "true");
									ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
									sender.RaReply(query[0].ToUpper() + "#Enabled cover for group " + query[3] + ".", success: true, logToConsole: true, "");
								}
								else if (query[2].ToLower() == "disablecover" && query.Length == 4)
								{
									if (allGroups.FirstOrDefault((KeyValuePair<string, UserGroup> gr) => gr.Key == query[3]).Key == null)
									{
										sender.RaReply(query[0].ToUpper() + "#Group can't be found.", success: false, logToConsole: true, "");
										break;
									}
									ServerStatic.RolesConfig.SetString(query[3] + "_cover", "false");
									ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
									sender.RaReply(query[0].ToUpper() + "#Enabled cover for group " + query[3] + ".", success: true, logToConsole: true, "");
								}
								else
								{
									sender.RaReply(query[0].ToUpper() + "#Unknown action. Run " + query[0] + " to get list of all actions.", success: false, logToConsole: true, "");
								}
							}
							else if (string.Equals(query[1], "users", StringComparison.OrdinalIgnoreCase))
							{
								Dictionary<string, string> stringDictionary2 = ServerStatic.RolesConfig.GetStringDictionary("Members");
								Dictionary<string, string> dictionary = ServerStatic.SharedGroupsMembersConfig?.GetStringDictionary("SharedMembers");
								string text7 = "Players with assigned groups:";
								foreach (KeyValuePair<string, string> item5 in stringDictionary2)
								{
									text7 = text7 + "\n" + item5.Key + " - " + item5.Value;
								}
								if (dictionary != null)
								{
									foreach (KeyValuePair<string, string> item6 in dictionary)
									{
										text7 = text7 + "\n" + item6.Key + " - " + item6.Value + " <color=#FFD700>[SHARED MEMBERSHIP]</color>";
									}
								}
								sender.RaReply(query[0].ToUpper() + "#" + text7, success: true, logToConsole: true, "");
							}
							else if (string.Equals(query[1], "setgroup", StringComparison.OrdinalIgnoreCase) && query.Length == 4)
							{
								string text8 = "";
								if (query[3] == "-1")
								{
									text8 = null;
								}
								else
								{
									KeyValuePair<string, UserGroup> keyValuePair2 = allGroups.FirstOrDefault((KeyValuePair<string, UserGroup> gr) => gr.Key == query[3]);
									if (keyValuePair2.Key == null)
									{
										sender.RaReply(query[0].ToUpper() + "#Group can't be found.", success: false, logToConsole: true, "");
										break;
									}
									text8 = keyValuePair2.Key;
								}
								ServerStatic.RolesConfig.SetStringDictionaryItem("Members", query[2], text8);
								ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
								sender.RaReply(query[0].ToUpper() + "#User permissions updated. If user is online, please use \"setgroup\" command to change it now (without this command, new role will be applied during next round).", success: true, logToConsole: true, "");
							}
							else if (string.Equals(query[1], "reload", StringComparison.OrdinalIgnoreCase))
							{
								ConfigFile.ReloadGameConfigs();
								ServerStatic.RolesConfig.Reload();
								ServerStatic.SharedGroupsConfig = ((ConfigSharing.Paths[4] == null) ? null : new YamlConfig(ConfigSharing.Paths[4] + "shared_groups.txt"));
								ServerStatic.SharedGroupsMembersConfig = ((ConfigSharing.Paths[5] == null) ? null : new YamlConfig(ConfigSharing.Paths[5] + "shared_groups_members.txt"));
								ServerStatic.PermissionsHandler = new PermissionsHandler(ref ServerStatic.RolesConfig, ref ServerStatic.SharedGroupsConfig, ref ServerStatic.SharedGroupsMembersConfig);
								sender.RaReply(query[0].ToUpper() + "#Permission file reloaded.", success: true, logToConsole: true, "");
							}
							else
							{
								sender.RaReply(query[0].ToUpper() + "#Unknown action. Run " + query[0] + " to get list of all actions.", success: false, logToConsole: true, "");
							}
							break;
						}
					case "SLML_STYLE":
					case "SLML_TAG":
						if (query.Length >= 3)
						{
							ServerLogs.AddLog(ServerLogs.Modules.Logger, logName + " Requested a download of " + query[2] + " on " + query[1] + " players' computers.", ServerLogs.ServerLogType.RemoteAdminActivity_Misc);
							StandardizedQueryModel1(sender, query[0], query[1], query[2], out failures, out successes, out error, out replySent);
							if (!replySent)
							{
								if (failures == 0)
								{
									sender.RaReply(query[0] + "#Done! " + successes + " player(s) affected!", success: true, logToConsole: true, "");
								}
								else
								{
									sender.RaReply(query[0] + "#The proccess has occured an issue! Failures: " + failures + "\nLast error log:\n" + error, success: false, logToConsole: true, "");
								}
							}
						}
						else
						{
							sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 3 arguments! (some parameters are missing)", success: false, logToConsole: true, "");
						}
						break;
					case "UNBAN":
						if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.LongTermBanning))
						{
							break;
						}
						if (query.Length == 3)
						{
							ServerLogs.AddLog(ServerLogs.Modules.Administrative, logName + " ran the unban " + query[1] + " command on " + query[2] + ".", ServerLogs.ServerLogType.RemoteAdminActivity_Misc);
							switch (query[1].ToLower())
							{
								case "id":
								case "playerid":
								case "player":
									BanHandler.RemoveBan(query[2], BanHandler.BanType.UserId);
									sender.RaReply(query[0] + "#Done!", success: true, logToConsole: true, "");
									break;
								case "ip":
								case "address":
									BanHandler.RemoveBan(query[2], BanHandler.BanType.IP);
									sender.RaReply(query[0] + "#Done!", success: true, logToConsole: true, "");
									break;
								default:
									sender.RaReply(query[0].ToUpper() + "#Correct syntax is: unban id UserIdHere OR unban ip IpAddressHere", success: false, logToConsole: true, "");
									break;
							}
						}
						else
						{
							sender.RaReply(query[0].ToUpper() + "#Correct syntax is: unban id UserIdHere OR unban ip IpAddressHere", success: false, logToConsole: true, "");
						}
						break;
					case "GROUPS":
						{
							string text9 = "Groups defined on this server:";
							Dictionary<string, UserGroup> allGroups2 = ServerStatic.PermissionsHandler.GetAllGroups();
							ServerRoles.NamedColor[] namedColors = QueryProcessor.Localplayer.GetComponent<ServerRoles>().NamedColors;
							foreach (KeyValuePair<string, UserGroup> permentry in allGroups2)
							{
								try
								{
									text9 = text9 + "\n" + permentry.Key + " (" + permentry.Value.Permissions + ") - <color=#" + namedColors.FirstOrDefault((ServerRoles.NamedColor x) => x.Name == permentry.Value.BadgeColor).ColorHex + ">" + permentry.Value.BadgeText + "</color> in color " + permentry.Value.BadgeColor;
								}
								catch
								{
									text9 = text9 + "\n" + permentry.Key + " (" + permentry.Value.Permissions + ") - " + permentry.Value.BadgeText + " in color " + permentry.Value.BadgeColor;
								}
								if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.KickingAndShortTermBanning))
								{
									text9 += " BN1";
								}
								if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.BanningUpToDay))
								{
									text9 += " BN2";
								}
								if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.LongTermBanning))
								{
									text9 += " BN3";
								}
								if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ForceclassSelf))
								{
									text9 += " FSE";
								}
								if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ForceclassToSpectator))
								{
									text9 += " FSP";
								}
								if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ForceclassWithoutRestrictions))
								{
									text9 += " FWR";
								}
								if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.GivingItems))
								{
									text9 += " GIV";
								}
								if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.WarheadEvents))
								{
									text9 += " EWA";
								}
								if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.RespawnEvents))
								{
									text9 += " ERE";
								}
								if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.RoundEvents))
								{
									text9 += " ERO";
								}
								if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.SetGroup))
								{
									text9 += " SGR";
								}
								if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.GameplayData))
								{
									text9 += " GMD";
								}
								if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.Overwatch))
								{
									text9 += " OVR";
								}
								if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.FacilityManagement))
								{
									text9 += " FCM";
								}
								if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.PlayersManagement))
								{
									text9 += " PLM";
								}
								if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.PermissionsManagement))
								{
									text9 += " PRM";
								}
								if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ServerConsoleCommands))
								{
									text9 += " SCC";
								}
								if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ViewHiddenBadges))
								{
									text9 += " VHB";
								}
								if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ServerConfigs))
								{
									text9 += " CFG";
								}
								if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.Broadcasting))
								{
									text9 += " BRC";
								}
								if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.PlayerSensitiveDataAccess))
								{
									text9 += " CDA";
								}
								if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.Noclip))
								{
									text9 += " NCP";
								}
								if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.AFKImmunity))
								{
									text9 += " AFK";
								}
								if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.AdminChat))
								{
									text9 += " ATC";
								}
								if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.ViewHiddenGlobalBadges))
								{
									text9 += " GHB";
								}
								if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.Announcer))
								{
									text9 += " ANN";
								}
								if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.Effects))
								{
									text9 += " EFF";
								}
								if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.FriendlyFireDetectorImmunity))
								{
									text9 += " FFI";
								}
								if (PermissionsHandler.IsPermitted(permentry.Value.Permissions, PlayerPermissions.FriendlyFireDetectorTempDisable))
								{
									text9 += " FFT";
								}
							}
							sender.RaReply(query[0].ToUpper() + "#" + text9, success: true, logToConsole: true, "");
							break;
						}
					case "FS":
					case "FORCESTART":
						if (CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.RoundEvents, "ServerEvents"))
						{
							bool flag14 = CharacterClassManager.ForceRoundStart();
							if (flag14)
							{
								ServerLogs.AddLog(ServerLogs.Modules.Administrative, logName + " forced round start.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
							}
							sender.RaReply(query[0] + "#" + (flag14 ? "Done! Forced round start." : "Failed to force start."), flag14, logToConsole: true, "ServerEvents");
						}
						break;
					case "SC":
					case "CONFIG":
					case "SETCONFIG":
						if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.ServerConfigs, "ServerConfigs"))
						{
							break;
						}
						if (query.Length >= 3)
						{
							if (query.Length > 3)
							{
								string text13 = query[2];
								for (int l = 3; l < query.Length; l++)
								{
									text13 = text13 + " " + query[l];
								}
								query = new string[3]
								{
							query[0],
							query[1],
							text13
								};
							}
							ServerLogs.AddLog(ServerLogs.Modules.Administrative, logName + " ran the setconfig command (" + query[1] + ": " + query[2] + ").", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
							bool flag11 = true;
							switch (query[1].ToUpper())
							{
								case "FRIENDLY_FIRE":
									{
										if (bool.TryParse(query[2], out bool result6))
										{
											ServerConsole.FriendlyFire = result6;
											sender.RaReply(query[0].ToUpper() + "#Done! Config [" + query[1] + "] has been set to [" + result6 + "]!", success: true, logToConsole: true, "ServerConfigs");
										}
										else
										{
											sender.RaReply(query[0].ToUpper() + "#" + query[1] + " has invalid value, " + query[2] + " is not a valid bool!", success: false, logToConsole: true, "ServerConfigs");
										}
										break;
									}
								case "PLAYER_LIST_TITLE":
									{
										string text14 = query[2] ?? string.Empty;
										PlayerList.Title.Value = text14;
										try
										{
											PlayerList.singleton.RefreshTitle();
										}
										catch (Exception ex5)
										{
											if (!(ex5 is CommandInputException) && !(ex5 is InvalidOperationException))
											{
												throw;
											}
											sender.RaReply(query[0].ToUpper() + "#Could not set player list title [" + text14 + "]:\n" + ex5.Message, success: false, logToConsole: true, "ServerConfigs");
											break;
										}
										sender.RaReply(query[0].ToUpper() + "#Done! Config [" + query[1] + "] has been set to [" + PlayerList.singleton.syncServerName + "]!", success: true, logToConsole: true, "ServerConfigs");
										break;
									}
								case "PD_REFRESH_EXIT":
									{
										if (bool.TryParse(query[2], out bool result7))
										{
											PocketDimensionTeleport.RefreshExit = result7;
											sender.RaReply(query[0].ToUpper() + "#Done! Config [" + query[1] + "] has been set to [" + result7 + "]!", success: true, logToConsole: true, "ServerConfigs");
										}
										else
										{
											sender.RaReply(query[0].ToUpper() + "#" + query[1] + " has invalid value, " + query[2] + " is not a valid bool!", success: false, logToConsole: true, "ServerConfigs");
										}
										break;
									}
								case "SPAWN_PROTECT_DISABLE":
									{
										if (bool.TryParse(query[2], out bool result4))
										{
											CharacterClassManager[] array2 = UnityEngine.Object.FindObjectsOfType<CharacterClassManager>();
											for (int k = 0; k < array2.Length; k++)
											{
												array2[k].EnableSP = !result4;
											}
											sender.RaReply(query[0].ToUpper() + "#Done! Config [" + query[1] + "] has been set to [" + result4 + "]!", success: true, logToConsole: true, "ServerConfigs");
										}
										else
										{
											sender.RaReply(query[0].ToUpper() + "#" + query[1] + " has invalid value, " + query[2] + " is not a valid bool!", success: false, logToConsole: true, "ServerConfigs");
										}
										break;
									}
								case "SPAWN_PROTECT_TIME":
									{
										if (int.TryParse(query[2], NumberStyles.Any, CultureInfo.InvariantCulture, out int result8))
										{
											CharacterClassManager[] array2 = UnityEngine.Object.FindObjectsOfType<CharacterClassManager>();
											for (int k = 0; k < array2.Length; k++)
											{
												array2[k].SProtectedDuration = result8;
											}
											sender.RaReply(query[0].ToUpper() + "#Done! Config [" + query[1] + "] has been set to [" + result8 + "]!", success: true, logToConsole: true, "ServerConfigs");
										}
										else
										{
											sender.RaReply(query[0].ToUpper() + "#" + query[1] + " has invalid value, " + query[2] + " is not a valid integer!", success: false, logToConsole: true, "ServerConfigs");
										}
										break;
									}
								case "HUMAN_GRENADE_MULTIPLIER":
									{
										if (float.TryParse(query[2], NumberStyles.Any, CultureInfo.InvariantCulture, out float result5))
										{
											ConfigFile.ServerConfig.SetString("human_grenade_multiplier", result5.ToString());
											sender.RaReply(query[0].ToUpper() + "#Done! Config [" + query[1] + "] has been set to [" + result5 + "]!", success: true, logToConsole: true, "ServerConfigs");
										}
										else
										{
											sender.RaReply(query[0].ToUpper() + "#" + query[1] + " has invalid value, " + query[2] + " is not a valid float!", success: false, logToConsole: true, "ServerConfigs");
										}
										break;
									}
								case "SCP_GRENADE_MULTIPLIER":
									{
										if (float.TryParse(query[2], NumberStyles.Any, CultureInfo.InvariantCulture, out float result3))
										{
											ConfigFile.ServerConfig.SetString("scp_grenade_multiplier", result3.ToString());
											sender.RaReply(query[0].ToUpper() + "#Done! Config [" + query[1] + "] has been set to [" + result3 + "]!", success: true, logToConsole: true, "ServerConfigs");
										}
										else
										{
											sender.RaReply(query[0].ToUpper() + "#" + query[1] + " has invalid value, " + query[2] + " is not a valid float!", success: false, logToConsole: true, "ServerConfigs");
										}
										break;
									}
								default:
									flag11 = false;
									sender.RaReply(query[0].ToUpper() + "#Invalid config " + query[1], success: false, logToConsole: true, "ServerConfigs");
									break;
							}
							if (flag11)
							{
								ServerConfigSynchronizer.RefreshAllConfigs();
							}
						}
						else
						{
							sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 3 arguments! (some parameters are missing)", success: false, logToConsole: true, "ServerConfigs");
						}
						break;
					case "FC":
					case "FORCECLASS":
						if (query.Length >= 3)
						{
							int result9 = 0;
							if (!int.TryParse(query[2], out result9) || result9 < 0 || result9 >= QueryProcessor.LocalCCM.Classes.Length)
							{
								sender.RaReply(query[0].ToUpper() + "#Invalid class ID.", success: false, logToConsole: true, "");
								break;
							}
							string fullName = QueryProcessor.LocalCCM.Classes.SafeGet(result9).fullName;
							GameObject gameObject9 = GameObject.Find("Host");
							if (gameObject9 == null)
							{
								sender.RaReply(query[0].ToUpper() + "#Please start round before using this command.", success: false, logToConsole: true, "");
								break;
							}
							CharacterClassManager component3 = gameObject9.GetComponent<CharacterClassManager>();
							if (component3 == null || !component3.isLocalPlayer || !component3.isServer || !component3.RoundStarted)
							{
								sender.RaReply(query[0].ToUpper() + "#Please start round before using this command.", success: false, logToConsole: true, "");
								break;
							}
							PlayerCommandSender playerCommandSender5;
							bool flag12 = (playerCommandSender5 = (sender as PlayerCommandSender)) != null && (query[1] == playerCommandSender5.PlayerId.ToString() || query[1] == playerCommandSender5.PlayerId + ".");
							bool flag13 = result9 == 2;
							if ((flag12 && flag13 && !CheckPermissions(sender, query[0].ToUpper(), new PlayerPermissions[3]
							{
						PlayerPermissions.ForceclassWithoutRestrictions,
						PlayerPermissions.ForceclassToSpectator,
						PlayerPermissions.ForceclassSelf
							})) || (flag12 && !flag13 && !CheckPermissions(sender, query[0].ToUpper(), new PlayerPermissions[2]
							{
						PlayerPermissions.ForceclassWithoutRestrictions,
						PlayerPermissions.ForceclassSelf
							})) || (!flag12 && flag13 && !CheckPermissions(sender, query[0].ToUpper(), new PlayerPermissions[2]
							{
						PlayerPermissions.ForceclassWithoutRestrictions,
						PlayerPermissions.ForceclassToSpectator
							})) || (!flag12 && !flag13 && !CheckPermissions(sender, query[0].ToUpper(), new PlayerPermissions[1]
							{
						PlayerPermissions.ForceclassWithoutRestrictions
							})))
							{
								break;
							}
							if (string.Equals(query[0], "role", StringComparison.OrdinalIgnoreCase))
							{
								ServerLogs.AddLog(ServerLogs.Modules.ClassChange, logName + " ran the role command (ID: " + query[2] + " - " + fullName + ") on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
							}
							else
							{
								ServerLogs.AddLog(ServerLogs.Modules.ClassChange, logName + " ran the forceclass command (ID: " + query[2] + " - " + fullName + ") on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
							}
							StandardizedQueryModel1(sender, query[0], query[1], query[2], out failures, out successes, out error, out replySent);
							if (!replySent)
							{
								if (failures == 0)
								{
									sender.RaReply(query[0] + "#Done! The request affected " + successes + " player(s)!", success: true, logToConsole: true, "");
								}
								else
								{
									sender.RaReply(query[0] + "#The proccess has occured an issue! Failures: " + failures + "\nLast error log:\n" + error, success: false, logToConsole: true, "");
								}
							}
						}
						else
						{
							sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 3 arguments! (some parameters are missing)", success: false, logToConsole: true, "");
						}
						break;
					case "WARHEAD":
						if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.WarheadEvents))
						{
							break;
						}
						if (query.Length == 1)
						{
							sender.RaReply("Syntax: warhead (status|detonate|instant|cancel|enable|disable)", success: false, logToConsole: true, string.Empty);
							break;
						}
						switch (query[1].ToLower())
						{
							case "status":
								if (AlphaWarheadController.Host.detonated || Math.Abs(AlphaWarheadController.Host.timeToDetonation) < 0.001f)
								{
									sender.RaReply("Warhead has been detonated.", success: true, logToConsole: true, string.Empty);
								}
								else if (AlphaWarheadController.Host.inProgress)
								{
									sender.RaReply("Detonation is in progress.", success: true, logToConsole: true, string.Empty);
								}
								else if (!AlphaWarheadOutsitePanel.nukeside.enabled)
								{
									sender.RaReply("Warhead is disabled.", success: true, logToConsole: true, string.Empty);
								}
								else if (AlphaWarheadController.Host.timeToDetonation > AlphaWarheadController.Host.RealDetonationTime())
								{
									sender.RaReply("Warhead is restarting.", success: true, logToConsole: true, string.Empty);
								}
								else
								{
									sender.RaReply("Warhead is ready to detonation.", success: true, logToConsole: true, string.Empty);
								}
								break;
							case "detonate":
								AlphaWarheadController.Host.StartDetonation();
								sender.RaReply("Detonation sequence started.", success: true, logToConsole: true, string.Empty);
								break;
							case "instant":
								AlphaWarheadController.Host.InstantPrepare();
								AlphaWarheadController.Host.StartDetonation();
								AlphaWarheadController.Host.NetworktimeToDetonation = 5f;
								sender.RaReply("Instant detonation started.", success: true, logToConsole: true, string.Empty);
								break;
							case "cancel":
								AlphaWarheadController.Host.CancelDetonation(null);
								sender.RaReply("Detonation has been canceled.", success: true, logToConsole: true, string.Empty);
								break;
							case "enable":
								AlphaWarheadOutsitePanel.nukeside.Networkenabled = true;
								sender.RaReply("Warhead has been enabled.", success: true, logToConsole: true, string.Empty);
								break;
							case "disable":
								AlphaWarheadOutsitePanel.nukeside.Networkenabled = false;
								sender.RaReply("Warhead has been disabled.", success: true, logToConsole: true, string.Empty);
								break;
							default:
								sender.RaReply("WARHEAD: Unknown subcommand.", success: false, logToConsole: true, string.Empty);
								break;
						}
						break;
					case "HEAL":
						if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PlayersManagement, "AdminTools"))
						{
							break;
						}
						if (query.Length >= 2)
						{
							int result10 = (query.Length >= 3 && int.TryParse(query[2], out result10)) ? result10 : 0;
							ServerLogs.AddLog(ServerLogs.Modules.Administrative, logName + " ran the heal command on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
							StandardizedQueryModel1(sender, query[0], query[1], result10.ToString(), out failures, out successes, out error, out replySent);
							if (!replySent)
							{
								if (failures == 0)
								{
									sender.RaReply(query[0] + "#Done! The request affected " + successes + " player(s)!", success: true, logToConsole: true, "");
								}
								else
								{
									sender.RaReply(query[0] + "#The proccess has occured an issue! Failures: " + failures + "\nLast error log:\n" + error, success: false, logToConsole: true, "AdminTools");
								}
							}
						}
						else
						{
							sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", success: false, logToConsole: true, "");
						}
						break;
					case "N":
					case "NC":
					case "NOCLIP":
						{
							if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.Noclip, "AdminTools"))
							{
								break;
							}
							PlayerCommandSender playerCommandSender4;
							if (query.Length >= 2)
							{
								if (query.Length == 2)
								{
									query = new string[3]
									{
							query[0],
							query[1],
							""
									};
								}
								ServerLogs.AddLog(ServerLogs.Modules.Administrative, logName + " ran the noclip command (new status: " + ((query[2] == "") ? "TOGGLE" : query[2]) + ") on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
								StandardizedQueryModel1(sender, "NOCLIP", query[1], query[2], out failures, out successes, out error, out replySent);
								if (!replySent)
								{
									if (failures == 0)
									{
										sender.RaReply("NOCLIP#Done! The request affected " + successes + " player(s)!", success: true, logToConsole: true, "AdminTools");
										break;
									}
									sender.RaReply("NOCLIP#The proccess has occured an issue! Failures: " + failures + "\nLast error log:\n" + error, success: false, logToConsole: true, "AdminTools");
								}
							}
							else if ((playerCommandSender4 = (sender as PlayerCommandSender)) != null)
							{
								StandardizedQueryModel1(sender, "NOCLIP", playerCommandSender4.PlayerId.ToString(), "", out failures, out successes, out error, out replySent);
								if (failures == 0)
								{
									sender.RaReply("NOCLIP#Done! The request affected " + successes + " player(s)!", success: true, logToConsole: true, "AdminTools");
									break;
								}
								sender.RaReply("NOCLIP#The proccess has occured an issue! Failures: " + failures + "\nLast error log:\n" + error, success: false, logToConsole: true, "AdminTools");
							}
							else
							{
								sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", success: false, logToConsole: true, "AdminTools");
							}
							break;
						}
					case "HP":
					case "SETHP":
						if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PlayersManagement))
						{
							break;
						}
						if (query.Length >= 3)
						{
							ServerLogs.AddLog(ServerLogs.Modules.Administrative, logName + " ran the sethp command on " + query[1] + " players (HP: " + query[2] + ").", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
							StandardizedQueryModel1(sender, query[0], query[1], query[2], out failures, out successes, out error, out replySent);
							if (!replySent)
							{
								if (failures == 0)
								{
									sender.RaReply(query[0] + "#Done! The request affected " + successes + " player(s)!", success: true, logToConsole: true, "");
								}
								else
								{
									sender.RaReply(query[0] + "#The proccess has occured an issue! Failures: " + failures + "\nLast error log:\n" + error, success: false, logToConsole: true, "");
								}
							}
						}
						else
						{
							sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 3 arguments! (some parameters are missing)", success: false, logToConsole: true, "");
						}
						break;
					case "PFX":
					case "EFFECT":
						if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.Effects))
						{
							break;
						}
						if (query.Length >= 3)
						{
							ServerLogs.AddLog(ServerLogs.Modules.Administrative, logName + " changed status effects for " + query[1] + " players (effect: " + query[2] + ").", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
							StandardizedQueryModel1(sender, "EFFECT", query[1], query[2], out failures, out successes, out error, out replySent);
							if (!replySent)
							{
								if (failures == 0)
								{
									sender.RaReply(query[0] + "#Done! The request affected " + successes + " player(s)!", success: true, logToConsole: true, "StatusEffects");
								}
								else
								{
									sender.RaReply(query[0] + "#The proccess has occured an issue! Failures: " + failures + "\nLast error log:\n" + error, success: false, logToConsole: true, "StatusEffects");
								}
							}
						}
						else
						{
							sender.RaReply(query[0].ToUpper() + "#Wrong syntax; try: 'EFFECT " + sender.SenderId + " Scp207=(intensity)'.", success: false, logToConsole: true, "StatusEffects");
						}
						break;
					case "TPFX":
					case "TIMEDEFFECT":
					case "TEFFECT":
						if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.Effects))
						{
							break;
						}
						if (query.Length >= 3)
						{
							ServerLogs.AddLog(ServerLogs.Modules.Administrative, logName + " changed timed status effects for " + query[1] + " players (effect: " + query[2] + ").", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
							StandardizedQueryModel1(sender, "TIMEDEFFECT", query[1], query[2], out failures, out successes, out error, out replySent);
							if (!replySent)
							{
								if (failures == 0)
								{
									sender.RaReply(query[0] + "#Done! The request affected " + successes + " player(s)!", success: true, logToConsole: true, "StatusEffects");
								}
								else
								{
									sender.RaReply(query[0] + "#The proccess has occured an issue! Failures: " + failures + "\nLast error log:\n" + error, success: false, logToConsole: true, "StatusEffects");
								}
							}
						}
						else
						{
							sender.RaReply(query[0].ToUpper() + "#Wrong syntax; try: 'TIMEDEFFECT " + sender.SenderId + " Scp207=(duration)'.", success: false, logToConsole: true, "StatusEffects");
						}
						break;
					case "TICKETS":
					case "TIX":
						{
							RespawnTickets singleton = RespawnTickets.Singleton;
							if (singleton == null)
							{
								sender.RaReply(query[0].ToUpper() + "#Respawn component doesn't exist!", success: false, logToConsole: true, "SpawnTickets");
							}
							else if (query.Length <= 2)
							{
								switch (query[1].ToLower())
								{
									case "ntf":
									case "mtf":
									case "ninetailedfox":
									case "mobiletaskforce":
										sender.RaReply(CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.RespawnEvents, string.Empty, reply: false) ? $"NtfTix#NTF spawn tickets: !{singleton.GetAvailableTickets(SpawnableTeamType.NineTailedFox)}" : "NtfTix#NTF spawn tickets: !---", success: true, logToConsole: true, "SpawnTickets");
										break;
									case "ci":
									case "chaos":
									case "chaosinsurgency":
										sender.RaReply(CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.RespawnEvents, string.Empty, reply: false) ? $"CiTix#CI spawn tickets: !{singleton.GetAvailableTickets(SpawnableTeamType.ChaosInsurgency)}" : "CiTix#CI spawn tickets: !---", success: true, logToConsole: true, "SpawnTickets");
										break;
									case "fetch":
										if (CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.RespawnEvents, string.Empty, reply: false))
										{
											sender.RaReply($"NtfTix#NTF spawn tickets: !{singleton.GetAvailableTickets(SpawnableTeamType.NineTailedFox)}", success: true, logToConsole: true, "SpawnTickets");
											sender.RaReply($"CiTix#CI spawn tickets: !{singleton.GetAvailableTickets(SpawnableTeamType.ChaosInsurgency)}", success: true, logToConsole: true, "SpawnTickets");
										}
										else
										{
											sender.RaReply("NtfTix#NTF spawn tickets: !---", success: true, logToConsole: true, "SpawnTickets");
											sender.RaReply("CiTix#CI spawn tickets: !---", success: true, logToConsole: true, "SpawnTickets");
										}
										break;
									case "info":
										sender.RaReply("TicketsInfo#" + RespawnManager.GetRemoteAdminInfoString(), success: true, logToConsole: true, "SpawnTickets");
										break;
								}
							}
							else
							{
								if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.RespawnEvents))
								{
									break;
								}
								if (!int.TryParse(query[2], out int result) && query[2] != "dec" && query[2] != "inc")
								{
									sender.RaReply(query[0].ToUpper() + "#Wrong syntax; try: 'TICKETS [team] [amount]'.", success: false, logToConsole: true, "SpawnTickets");
									break;
								}
								switch (query[1].ToLower())
								{
									case "ntf":
									case "mtf":
									case "ninetailedfox":
									case "mobiletaskforce":
										if (query[2] == "dec")
										{
											singleton.GrantTickets(SpawnableTeamType.NineTailedFox, -1, overrideLocks: true);
										}
										else if (query[2] == "inc")
										{
											singleton.GrantTickets(SpawnableTeamType.NineTailedFox, 1, overrideLocks: true);
										}
										else
										{
											singleton.GrantTickets(SpawnableTeamType.NineTailedFox, result, overrideLocks: true);
										}
										ServerLogs.AddLog(ServerLogs.Modules.Administrative, logName + " set NTF spawn tickets amount to " + singleton.GetAvailableTickets(SpawnableTeamType.NineTailedFox) + ".", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
										sender.RaReply($"SetNtfTix#NTF spawn tickets set to !{singleton.GetAvailableTickets(SpawnableTeamType.NineTailedFox)}", success: true, logToConsole: true, "SpawnTickets");
										break;
									case "ci":
									case "chi":
									case "chaos":
									case "chaosinsurgency":
										if (query[2] == "dec")
										{
											singleton.GrantTickets(SpawnableTeamType.ChaosInsurgency, -1, overrideLocks: true);
										}
										else if (query[2] == "inc")
										{
											singleton.GrantTickets(SpawnableTeamType.ChaosInsurgency, 1, overrideLocks: true);
										}
										else
										{
											singleton.GrantTickets(SpawnableTeamType.ChaosInsurgency, result, overrideLocks: true);
										}
										ServerLogs.AddLog(ServerLogs.Modules.Administrative, logName + " set CI spawn tickets amount to " + singleton.GetAvailableTickets(SpawnableTeamType.ChaosInsurgency) + ".", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
										sender.RaReply($"SetCiTix#CI spawn tickets set to !{singleton.GetAvailableTickets(SpawnableTeamType.ChaosInsurgency)}", success: true, logToConsole: true, "SpawnTickets");
										break;
								}
							}
							break;
						}
					case "OVR":
					case "OVERWATCH":
						if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.Overwatch, "AdminTools"))
						{
							break;
						}
						if (query.Length >= 2)
						{
							if (query.Length == 2)
							{
								query = new string[3]
								{
							query[0],
							query[1],
							""
								};
							}
							ServerLogs.AddLog(ServerLogs.Modules.ClassChange, logName + " ran the overwatch command (new status: " + ((query[2] == "") ? "TOGGLE" : query[2]) + ") on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
							StandardizedQueryModel1(sender, "OVERWATCH", query[1], query[2], out failures, out successes, out error, out replySent);
							if (!replySent)
							{
								if (failures == 0)
								{
									sender.RaReply("OVERWATCH#Done! The request affected " + successes + " player(s)!", success: true, logToConsole: true, "AdminTools");
									break;
								}
								sender.RaReply("OVERWATCH#The proccess has occured an issue! Failures: " + failures + "\nLast error log:\n" + error, success: false, logToConsole: true, "AdminTools");
							}
						}
						else
						{
							sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", success: false, logToConsole: true, "AdminTools");
						}
						break;
					case "GOD":
						if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PlayersManagement, "AdminTools"))
						{
							break;
						}
						if (query.Length >= 2)
						{
							if (query.Length == 2)
							{
								query = new string[3]
								{
							query[0],
							query[1],
							""
								};
							}
							ServerLogs.AddLog(ServerLogs.Modules.Administrative, logName + " ran the god command (new status: " + ((query[2] == "") ? "TOGGLE" : query[2]) + ") on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
							StandardizedQueryModel1(sender, "GOD", query[1], query[2], out failures, out successes, out error, out replySent);
							if (!replySent)
							{
								if (failures == 0)
								{
									sender.RaReply("OVERWATCH#Done! The request affected " + successes + " player(s)!", success: true, logToConsole: true, "AdminTools");
									break;
								}
								sender.RaReply("OVERWATCH#The proccess has occured an issue! Failures: " + failures + "\nLast error log:\n" + error, success: false, logToConsole: true, "AdminTools");
							}
						}
						else
						{
							sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", success: false, logToConsole: true, "AdminTools");
						}
						break;
					case "MUTE":
					case "UNMUTE":
					case "IMUTE":
					case "IUNMUTE":
						if (!CheckPermissions(sender, query[0].ToUpper(), new PlayerPermissions[3]
						{
					PlayerPermissions.BanningUpToDay,
					PlayerPermissions.LongTermBanning,
					PlayerPermissions.PlayersManagement
						}, "PlayersManagement"))
						{
							break;
						}
						if (query.Length == 2)
						{
							ServerLogs.AddLog(ServerLogs.Modules.Administrative, logName + " ran the " + query[0].ToLower() + " command on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_Misc);
							StandardizedQueryModel1(sender, query[0].ToUpper(), query[1], null, out failures, out successes, out error, out replySent);
							if (!replySent)
							{
								if (failures == 0)
								{
									sender.RaReply(query[0].ToUpper() + "#Done! The request affected " + successes + " player(s)!", success: true, logToConsole: true, "PlayersManagement");
								}
								else
								{
									sender.RaReply(query[0].ToUpper() + "#The proccess has occured an issue! Failures: " + failures + "\nLast error log:\n" + error, success: false, logToConsole: true, "PlayersManagement");
								}
							}
						}
						else
						{
							sender.RaReply(query[0].ToUpper() + "#To run this program, type exactly 2 arguments!", success: false, logToConsole: true, "PlayersManagement");
						}
						break;
					case "INTERCOM-TIMEOUT":
						if (CheckPermissions(sender, query[0].ToUpper(), new PlayerPermissions[6]
						{
					PlayerPermissions.KickingAndShortTermBanning,
					PlayerPermissions.BanningUpToDay,
					PlayerPermissions.LongTermBanning,
					PlayerPermissions.RoundEvents,
					PlayerPermissions.FacilityManagement,
					PlayerPermissions.PlayersManagement
						}, "ServerEvents"))
						{
							if (!Intercom.host.speaking)
							{
								sender.RaReply(query[0].ToUpper() + "#Intercom is not being used.", success: false, logToConsole: true, "ServerEvents");
								break;
							}
							if (Math.Abs(Intercom.host.speechRemainingTime - -77f) < 0.1f)
							{
								sender.RaReply(query[0].ToUpper() + "#Intercom is being used by player with bypass mode enabled.", success: false, logToConsole: true, "ServerEvents");
								break;
							}
							Intercom.host.speechRemainingTime = -1f;
							ServerLogs.AddLog(ServerLogs.Modules.Administrative, logName + " timeouted the intercom speaker.", ServerLogs.ServerLogType.RemoteAdminActivity_Misc);
							sender.RaReply(query[0].ToUpper() + "#Done! Intercom speaker timeouted.", success: true, logToConsole: true, "ServerEvents");
						}
						break;
					case "INTERCOM-RESET":
						if (CheckPermissions(sender, query[0].ToUpper(), new PlayerPermissions[3]
						{
					PlayerPermissions.RoundEvents,
					PlayerPermissions.FacilityManagement,
					PlayerPermissions.PlayersManagement
						}, "ServerEvents"))
						{
							if (Intercom.host.remainingCooldown <= 0f)
							{
								sender.RaReply(query[0].ToUpper() + "#Intercom is already ready to use.", success: false, logToConsole: true, "ServerEvents");
								break;
							}
							Intercom.host.remainingCooldown = -1f;
							ServerLogs.AddLog(ServerLogs.Modules.Administrative, logName + " reset the intercom cooldown.", ServerLogs.ServerLogType.RemoteAdminActivity_Misc);
							sender.RaReply(query[0].ToUpper() + "#Done! Intercom cooldown reset.", success: true, logToConsole: true, "ServerEvents");
						}
						break;
					case "SPEAK":
					case "ICOM":
						if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.Broadcasting, "ServerEvents") || !IsPlayer(sender, query[0], "ServerEvents"))
						{
							break;
						}
						if (!Intercom.AdminSpeaking)
						{
							if (Intercom.host.speaking)
							{
								sender.RaReply(query[0].ToUpper() + "#Intercom is being used by someone else.", success: false, logToConsole: true, "ServerEvents");
								break;
							}
							Intercom.AdminSpeaking = true;
							Intercom.host.RequestTransmission(queryProcessor.GetComponent<Intercom>().gameObject);
							ServerLogs.AddLog(ServerLogs.Modules.Administrative, logName + " requested global voice over the intercom.", ServerLogs.ServerLogType.RemoteAdminActivity_Misc);
							sender.RaReply(query[0].ToUpper() + "#Done! Global voice over the intercom granted.", success: true, logToConsole: true, "ServerEvents");
						}
						else
						{
							Intercom.AdminSpeaking = false;
							Intercom.host.RequestTransmission(null);
							ServerLogs.AddLog(ServerLogs.Modules.Administrative, logName + " ended global intercom transmission.", ServerLogs.ServerLogType.RemoteAdminActivity_Misc);
							sender.RaReply(query[0].ToUpper() + "#Done! Global voice over the intercom revoked.", success: true, logToConsole: true, "ServerEvents");
						}
						break;
					case "BM":
					case "BYPASS":
						if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.FacilityManagement, "AdminTools"))
						{
							break;
						}
						if (query.Length >= 2)
						{
							if (query.Length == 2)
							{
								query = new string[3]
								{
							query[0],
							query[1],
							""
								};
							}
							ServerLogs.AddLog(ServerLogs.Modules.Administrative, logName + " ran the bypass mode command (new status: " + ((query[2] == "") ? "TOGGLE" : query[2]) + ") on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
							StandardizedQueryModel1(sender, "BYPASS", query[1], query[2], out failures, out successes, out error, out replySent);
							if (!replySent)
							{
								if (failures == 0)
								{
									sender.RaReply("BYPASS#Done! The request affected " + successes + " player(s)!", success: true, logToConsole: true, "AdminTools");
									break;
								}
								sender.RaReply("BYPASS#The proccess has occured an issue! Failures: " + failures + "\nLast error log:\n" + error, success: false, logToConsole: true, "AdminTools");
							}
						}
						else
						{
							sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", success: false, logToConsole: true, "AdminTools");
						}
						break;
					case "BRING":
						if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PlayersManagement, "AdminTools") || !IsPlayer(sender, query[0], "AdminTools"))
						{
							break;
						}
						if (query.Length == 2)
						{
							if (playerCommandSender.CCM.CurClass == RoleType.Spectator || playerCommandSender.CCM.GetComponent<CharacterClassManager>().CurClass < RoleType.Scp173)
							{
								sender.RaReply("BRING#Command disabled when you are spectator!", success: false, logToConsole: true, "AdminTools");
								break;
							}
							ServerLogs.AddLog(ServerLogs.Modules.Administrative, logName + " ran the bring command on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
							StandardizedQueryModel1(sender, "BRING", query[1], "", out failures, out successes, out error, out replySent);
							if (!replySent)
							{
								if (failures == 0)
								{
									sender.RaReply("BRING#Done! The request affected " + successes + " player(s)!", success: true, logToConsole: true, "AdminTools");
									break;
								}
								sender.RaReply("BRING#The proccess has occured an issue! Failures: " + failures + "\nLast error log:\n" + error, success: false, logToConsole: true, "AdminTools");
							}
						}
						else
						{
							sender.RaReply(query[0].ToUpper() + "#To run this program, type exactly 2 arguments!", success: false, logToConsole: true, "AdminTools");
						}
						break;
					case "GOTO":
						if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PlayersManagement, "AdminTools") || !IsPlayer(sender, query[0], "AdminTools"))
						{
							break;
						}
						if (query.Length == 2)
						{
							if (playerCommandSender.CCM.CurClass == RoleType.Spectator || playerCommandSender.CCM.CurClass < RoleType.Scp173)
							{
								sender.RaReply("GOTO#Command is disabled when you are spectator!", success: false, logToConsole: true, "AdminTools");
								break;
							}
							if (!int.TryParse(query[1], out int id))
							{
								sender.RaReply("GOTO#Player ID must be an integer.", success: false, logToConsole: true, "AdminTools");
								break;
							}
							if (query[1].Contains("."))
							{
								sender.RaReply("GOTO#Goto command requires exact one selected player.", success: false, logToConsole: true, "AdminTools");
								break;
							}
							GameObject gameObject8 = PlayerManager.players.FirstOrDefault((GameObject pl) => pl.GetComponent<QueryProcessor>().PlayerId == id);
							if (gameObject8 == null)
							{
								sender.RaReply("GOTO#Can't find requested player.", success: false, logToConsole: true, "AdminTools");
								break;
							}
							if (gameObject8.GetComponent<CharacterClassManager>().CurClass == RoleType.Spectator || gameObject8.GetComponent<CharacterClassManager>().CurClass < RoleType.None)
							{
								sender.RaReply("GOTO#Requested player is a spectator!", success: false, logToConsole: true, "AdminTools");
								break;
							}
							ServerLogs.AddLog(ServerLogs.Modules.Administrative, logName + " ran the goto command on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
							queryProcessor.GetComponent<PlayerMovementSync>().OverridePosition(gameObject8.GetComponent<PlayerMovementSync>().RealModelPosition, 0f);
							sender.RaReply("GOTO#Done!", success: true, logToConsole: true, "AdminTools");
						}
						else
						{
							sender.RaReply(query[0].ToUpper() + "#To run this program, type exactly 2 arguments!", success: false, logToConsole: true, "AdminTools");
						}
						break;
					case "LD":
					case "LOCKDOWN":
						{
							if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.FacilityManagement, "AdminTools"))
							{
								break;
							}
							Door[] array;
							if (!QueryProcessor.Lockdown)
							{
								ServerLogs.AddLog(ServerLogs.Modules.Administrative, logName + " enabled the lockdown.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
								array = UnityEngine.Object.FindObjectsOfType<Door>();
								foreach (Door door in array)
								{
									if (!door.locked)
									{
										door.lockdown = true;
										door.UpdateLock();
									}
								}
								QueryProcessor.Lockdown = true;
								sender.RaReply(query[0] + "#Lockdown enabled!", success: true, logToConsole: true, "AdminTools");
								break;
							}
							ServerLogs.AddLog(ServerLogs.Modules.Administrative, logName + " disabled the lockdown.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
							array = UnityEngine.Object.FindObjectsOfType<Door>();
							foreach (Door door2 in array)
							{
								if (door2.lockdown)
								{
									door2.lockdown = false;
									door2.UpdateLock();
								}
							}
							QueryProcessor.Lockdown = false;
							sender.RaReply(query[0] + "#Lockdown disabled!", success: true, logToConsole: true, "AdminTools");
							break;
						}
					case "O":
					case "OPEN":
						if (CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.FacilityManagement, "DoorsManagement"))
						{
							if (query.Length != 2)
							{
								sender.RaReply(query[0].ToUpper() + "#Syntax of this program: " + query[0].ToUpper() + " DoorName", success: false, logToConsole: true, "");
							}
							else
							{
								ProcessDoorQuery(sender, "OPEN", query[1]);
							}
						}
						break;
					case "C":
					case "CLOSE":
						if (CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.FacilityManagement, "DoorsManagement"))
						{
							if (query.Length != 2)
							{
								sender.RaReply(query[0].ToUpper() + "#Syntax of this program: " + query[0].ToUpper() + " DoorName", success: false, logToConsole: true, "");
							}
							else
							{
								ProcessDoorQuery(sender, "CLOSE", query[1]);
							}
						}
						break;
					case "L":
					case "LOCK":
						if (CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.FacilityManagement, "DoorsManagement"))
						{
							if (query.Length != 2)
							{
								sender.RaReply(query[0].ToUpper() + "#Syntax of this program: " + query[0].ToUpper() + " DoorName", success: false, logToConsole: true, "");
							}
							else
							{
								ProcessDoorQuery(sender, "LOCK", query[1]);
							}
						}
						break;
					case "UL":
					case "UNLOCK":
						if (CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.FacilityManagement, "DoorsManagement"))
						{
							if (query.Length != 2)
							{
								sender.RaReply(query[0].ToUpper() + "#Syntax of this program: " + query[0].ToUpper() + " DoorName", success: false, logToConsole: true, "");
							}
							else
							{
								ProcessDoorQuery(sender, "UNLOCK", query[1]);
							}
						}
						break;
					case "DESTROY":
						if (CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.FacilityManagement, "DoorsManagement"))
						{
							if (query.Length != 2)
							{
								sender.RaReply(query[0].ToUpper() + "#Syntax of this program: " + query[0].ToUpper() + " DoorName", success: false, logToConsole: true, "");
							}
							else
							{
								ProcessDoorQuery(sender, "DESTROY", query[1]);
							}
						}
						break;
					case "DOORTP":
					case "DTP":
						if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PlayersManagement, "DoorsManagement"))
						{
							break;
						}
						if (query.Length != 3)
						{
							sender.RaReply(query[0].ToUpper() + "#Syntax of this program: " + query[0].ToUpper() + " PlayerIDs DoorName", success: false, logToConsole: true, "");
							break;
						}
						ServerLogs.AddLog(ServerLogs.Modules.Administrative, logName + " ran the DoorTp command (Door: " + query[2] + ") on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
						StandardizedQueryModel1(sender, "DOORTP", query[1], query[2], out failures, out successes, out error, out replySent);
						if (!replySent)
						{
							if (failures == 0)
							{
								sender.RaReply(query[0] + "#Done! The request affected " + successes + " player(s)!", success: true, logToConsole: true, "DoorsManagement");
							}
							else
							{
								sender.RaReply(query[0] + "#The proccess has occured an issue! Failures: " + failures + "\nLast error log:\n" + error, success: false, logToConsole: true, "DoorsManagement");
							}
						}
						break;
					case "DL":
					case "DOORS":
					case "DOORLIST":
						if (CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.FacilityManagement, "AdminTools"))
						{
							string str2 = "List of named doors in the facility:\n";
							List<string> list3 = (from item in UnityEngine.Object.FindObjectsOfType<Door>()
												  where !string.IsNullOrEmpty(item.DoorName)
												  select item.DoorName + " - " + (item.isOpen ? "<color=green>OPENED</color>" : "<color=orange>CLOSED</color>") + (item.locked ? " <color=red>[LOCKED]</color>" : "") + ((item.PermissionLevels == (Door.AccessRequirements)0) ? "" : " <color=blue>[CARD REQUIRED]</color>")).ToList();
							list3.Sort();
							str2 += list3.Aggregate((string current, string adding) => current + "\n" + adding);
							sender.RaReply(query[0] + "#" + str2, success: true, logToConsole: true, "");
						}
						break;
					case "GIVE":
						if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.GivingItems))
						{
							break;
						}
						if (query.Length >= 3)
						{
							ServerLogs.AddLog(ServerLogs.Modules.Administrative, logName + " ran the give command (ID: " + query[2] + ") on " + query[1] + " players.", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
							StandardizedQueryModel1(sender, query[0], query[1], query[2], out failures, out successes, out error, out replySent);
							if (!replySent)
							{
								if (failures == 0)
								{
									sender.RaReply(query[0] + "#Done! The request affected " + successes + " player(s)!", success: true, logToConsole: true, "");
								}
								else
								{
									sender.RaReply(query[0] + "#The proccess has occured an issue! Failures: " + failures + "\nLast error log:\n" + error, success: false, logToConsole: true, "");
								}
							}
						}
						else
						{
							sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 3 arguments! (some parameters are missing)", success: false, logToConsole: true, "");
						}
						break;
					case "CLEAR":
					case "STRIP":
						if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PlayersManagement))
						{
							break;
						}
						if (query.Length >= 2)
						{
							ServerLogs.AddLog(ServerLogs.Modules.Administrative, logName + " cleared the inventory of players with IDs: " + query[1] + ".", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
							StandardizedQueryModel1(sender, "CLEAR", query[1], string.Empty, out failures, out successes, out error, out replySent);
							if (!replySent)
							{
								if (failures == 0)
								{
									sender.RaReply(query[0] + "#Done! The request affected " + successes + " player(s)!", success: true, logToConsole: true, "");
								}
								else
								{
									sender.RaReply(query[0] + "#The proccess has occured an issue! Failures: " + failures + "\nLast error log:\n" + error, success: false, logToConsole: true, "");
								}
							}
						}
						else
						{
							sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", success: false, logToConsole: true, "");
						}
						break;
					case "REQUEST_DATA":
						if (query.Length >= 2)
						{
							switch (query[1].ToUpper())
							{
								case "PLAYER_LIST":
									try
									{
										string text = "\n";
										bool gameplayData = CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.GameplayData, string.Empty, reply: false);
										PlayerCommandSender playerCommandSender2;
										if ((playerCommandSender2 = (sender as PlayerCommandSender)) != null)
										{
											playerCommandSender2.Processor.GameplayData = gameplayData;
										}
										bool flag = q.Contains("STAFF", StringComparison.OrdinalIgnoreCase);
										bool flag2 = CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.ViewHiddenBadges, string.Empty, reply: false);
										bool flag3 = CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.ViewHiddenGlobalBadges, string.Empty, reply: false);
										if (playerCommandSender != null && playerCommandSender.SR.Staff)
										{
											flag2 = true;
											flag3 = true;
										}
										foreach (GameObject player in PlayerManager.players)
										{
											QueryProcessor component = player.GetComponent<QueryProcessor>();
											if (!flag)
											{
												string text2 = string.Empty;
												bool flag4 = false;
												ServerRoles component2 = component.GetComponent<ServerRoles>();
												try
												{
													if (string.IsNullOrEmpty(component2.HiddenBadge) || (component2.GlobalHidden && flag3) || (!component2.GlobalHidden && flag2))
													{
														text2 = (component2.RaEverywhere ? "[~] " : (component2.Staff ? "[@] " : (component2.RemoteAdmin ? "[RA] " : string.Empty)));
													}
													flag4 = component2.OverwatchEnabled;
												}
												catch
												{
												}
												text = text + text2 + "(" + component.PlayerId + ") " + component.GetComponent<NicknameSync>().CombinedName.Replace("\n", string.Empty) + (flag4 ? "<OVRM>" : string.Empty);
											}
											else
											{
												text = text + component.PlayerId + ";" + component.GetComponent<NicknameSync>().CombinedName;
											}
											text += "\n";
										}
										if (!q.Contains("STAFF", StringComparison.OrdinalIgnoreCase))
										{
											sender.RaReply(query[0].ToUpper() + ":PLAYER_LIST#" + text, success: true, query.Length < 3 || query[2].ToUpper() != "SILENT", "");
										}
										else
										{
											sender.RaReply("StaffPlayerListReply#" + text, success: true, query.Length < 3 || query[2].ToUpper() != "SILENT", "");
										}
									}
									catch (Exception ex2)
									{
										sender.RaReply(query[0].ToUpper() + ":PLAYER_LIST#An unexpected problem has occurred!\nMessage: " + ex2.Message + "\nStackTrace: " + ex2.StackTrace + "\nAt: " + ex2.Source, success: false, logToConsole: true, "");
										throw;
									}
									break;
								case "PLAYER":
								case "SHORT-PLAYER":
									if (query.Length >= 3)
									{
										if (string.Equals(query[1], "PLAYER", StringComparison.OrdinalIgnoreCase) && (playerCommandSender == null || !playerCommandSender.SR.Staff) && !CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PlayerSensitiveDataAccess))
										{
											break;
										}
										try
										{
											GameObject gameObject4 = null;
											NetworkConnection networkConnection = null;
											foreach (NetworkConnection value in NetworkServer.connections.Values)
											{
												GameObject gameObject5 = GameCore.Console.FindConnectedRoot(value);
												if (query[2].Contains("."))
												{
													query[2] = query[2].Split('.')[0];
												}
												if (!(gameObject5 == null) && !(gameObject5.GetComponent<QueryProcessor>().PlayerId.ToString() != query[2]))
												{
													gameObject4 = gameObject5;
													networkConnection = value;
												}
											}
											if (gameObject4 == null)
											{
												sender.RaReply(query[0].ToUpper() + ":PLAYER#Player with id " + (string.IsNullOrEmpty(query[2]) ? "[null]" : query[2]) + " not found!", success: false, logToConsole: true, "");
												break;
											}
											bool flag5 = PermissionsHandler.IsPermitted(sender.Permissions, PlayerPermissions.GameplayData);
											bool flag6 = PermissionsHandler.IsPermitted(sender.Permissions, 18007046uL);
											PlayerCommandSender playerCommandSender3;
											if ((playerCommandSender3 = (sender as PlayerCommandSender)) != null)
											{
												playerCommandSender3.Processor.GameplayData = flag5;
											}
											if (playerCommandSender != null && (playerCommandSender.SR.Staff || playerCommandSender.SR.RaEverywhere))
											{
												flag6 = true;
											}
											ReferenceHub hub = ReferenceHub.GetHub(gameObject4.gameObject);
											CharacterClassManager characterClassManager = hub.characterClassManager;
											ServerRoles serverRoles = hub.serverRoles;
											if (query[1].ToUpper() == "PLAYER")
											{
												ServerLogs.AddLog(ServerLogs.Modules.DataAccess, logName + " accessed IP address of player " + gameObject4.GetComponent<QueryProcessor>().PlayerId + " (" + gameObject4.GetComponent<NicknameSync>().MyNick + ").", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
											}
											StringBuilder stringBuilder = StringBuilderPool.Shared.Rent("<color=white>");
											stringBuilder.Append("Nickname: " + hub.nicknameSync.CombinedName);
											stringBuilder.Append("\nPlayer ID: " + hub.queryProcessor.PlayerId);
											stringBuilder.Append("\nIP: " + ((networkConnection == null) ? "null" : ((query[1].ToUpper() == "PLAYER") ? networkConnection.address : "[REDACTED]")));
											stringBuilder.Append("\nUser ID: " + ((!flag6) ? "<color=#D4AF37>INSUFFICIENT PERMISSIONS</color>" : (string.IsNullOrEmpty(characterClassManager.UserId) ? "(none)" : characterClassManager.UserId)));
											if (flag6)
											{
												if (characterClassManager.SaltedUserId != null && characterClassManager.SaltedUserId.Contains("$"))
												{
													stringBuilder.Append("\nSalted User ID: " + characterClassManager.SaltedUserId);
												}
												if (!string.IsNullOrEmpty(characterClassManager.UserId2))
												{
													stringBuilder.Append("\nUser ID 2: " + characterClassManager.UserId2);
												}
											}
											stringBuilder.Append("\nServer role: " + serverRoles.GetColoredRoleString());
											bool flag7 = CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.ViewHiddenBadges, string.Empty, reply: false);
											bool flag8 = CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.ViewHiddenGlobalBadges, string.Empty, reply: false);
											if (playerCommandSender != null && playerCommandSender.SR.Staff)
											{
												flag7 = true;
												flag8 = true;
											}
											bool flag9 = !string.IsNullOrEmpty(serverRoles.HiddenBadge);
											bool num = !flag9 || (serverRoles.GlobalHidden && flag8) || (!serverRoles.GlobalHidden && flag7);
											if (num)
											{
												if (flag9)
												{
													stringBuilder.Append("\n<color=#DC143C>Hidden role: </color>" + serverRoles.HiddenBadge);
													stringBuilder.Append("\n<color=#DC143C>Hidden role type: </color>" + (serverRoles.GlobalHidden ? "GLOBAL" : "LOCAL"));
												}
												if (serverRoles.RaEverywhere)
												{
													stringBuilder.Append("\nActive flag: <color=#BCC6CC>Studio GLOBAL Staff (management or global moderation)</color>");
												}
												else if (serverRoles.Staff)
												{
													stringBuilder.Append("\nActive flag: Studio Staff");
												}
											}
											if (characterClassManager.Muted)
											{
												stringBuilder.Append("\nActive flag: <color=#F70D1A>SERVER MUTED</color>");
											}
											else if (characterClassManager.IntercomMuted)
											{
												stringBuilder.Append("\nActive flag: <color=#F70D1A>INTERCOM MUTED</color>");
											}
											if (characterClassManager.GodMode)
											{
												stringBuilder.Append("\nActive flag: <color=#659EC7>GOD MODE</color>");
											}
											if (characterClassManager.NoclipEnabled)
											{
												stringBuilder.Append("\nActive flag: <color=#DC143C>NOCLIP ENABLED</color>");
											}
											else if (serverRoles.NoclipReady)
											{
												stringBuilder.Append("\nActive flag: <color=#E52B50>NOCLIP UNLOCKED</color>");
											}
											if (serverRoles.DoNotTrack)
											{
												stringBuilder.Append("\nActive flag: <color=#BFFF00>DO NOT TRACK</color>");
											}
											if (serverRoles.BypassMode)
											{
												stringBuilder.Append("\nActive flag: <color=#BFFF00>BYPASS MODE</color>");
											}
											if (num && serverRoles.RemoteAdmin)
											{
												stringBuilder.Append("\nActive flag: <color=#43C6DB>REMOTE ADMIN AUTHENTICATED</color>");
											}
											if (serverRoles.OverwatchEnabled)
											{
												stringBuilder.Append("\nActive flag: <color=#008080>OVERWATCH MODE</color>");
											}
											else
											{
												stringBuilder.Append("\nClass: " + ((!flag5) ? "<color=#D4AF37>INSUFFICIENT PERMISSIONS</color>" : (characterClassManager.Classes.CheckBounds(characterClassManager.CurClass) ? characterClassManager.CurRole.fullName : "None")));
												stringBuilder.Append("\nHP: " + (flag5 ? hub.playerStats.HealthToString() : "<color=#D4AF37>INSUFFICIENT PERMISSIONS</color>"));
												stringBuilder.Append("\nPosition: " + (flag5 ? $"[{hub.playerMovementSync.RealModelPosition.x}; {hub.playerMovementSync.RealModelPosition.y}; {hub.playerMovementSync.RealModelPosition.z}]" : "<color=#D4AF37>INSUFFICIENT PERMISSIONS</color>"));
												if (!flag5)
												{
													stringBuilder.Append("\n<color=#D4AF37>* GameplayData permission required</color>");
												}
											}
											stringBuilder.Append("</color>");
											sender.RaReply(query[0].ToUpper() + ":PLAYER#" + stringBuilder, success: true, logToConsole: true, "PlayerInfo");
											StringBuilderPool.Shared.Return(stringBuilder);
											sender.RaReply("PlayerInfoQR#" + (string.IsNullOrEmpty(characterClassManager.UserId) ? "(no User ID)" : characterClassManager.UserId), success: true, logToConsole: false, "PlayerInfo");
										}
										catch (Exception ex3)
										{
											sender.RaReply(query[0].ToUpper() + "#An unexpected problem has occurred!\nMessage: " + ex3.Message + "\nStackTrace: " + ex3.StackTrace + "\nAt: " + ex3.Source, success: false, logToConsole: true, "PlayerInfo");
											throw;
										}
									}
									else
									{
										sender.RaReply(query[0].ToUpper() + ":PLAYER#Please specify the PlayerId!", success: false, logToConsole: true, "");
									}
									break;
								case "AUTH":
									if ((playerCommandSender == null || !playerCommandSender.SR.Staff) && !CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PlayerSensitiveDataAccess))
									{
										break;
									}
									if (query.Length >= 3)
									{
										try
										{
											GameObject gameObject2 = null;
											foreach (NetworkConnection value2 in NetworkServer.connections.Values)
											{
												GameObject gameObject3 = GameCore.Console.FindConnectedRoot(value2);
												if (query[2].Contains("."))
												{
													query[2] = query[2].Split('.')[0];
												}
												if (gameObject3 != null && gameObject3.GetComponent<QueryProcessor>().PlayerId.ToString() == query[2])
												{
													gameObject2 = gameObject3;
												}
											}
											if (gameObject2 == null)
											{
												sender.RaReply(query[0].ToUpper() + ":PLAYER#Player with id " + (string.IsNullOrEmpty(query[2]) ? "[null]" : query[2]) + " not found!", success: false, logToConsole: true, "");
												break;
											}
											if (string.IsNullOrEmpty(gameObject2.GetComponent<CharacterClassManager>().AuthToken))
											{
												sender.RaReply(query[0].ToUpper() + ":PLAYER#Can't obtain auth token. Is server using offline mode or you selected the host?", success: false, logToConsole: true, "PlayerInfo");
												break;
											}
											ServerLogs.AddLog(ServerLogs.Modules.DataAccess, logName + " accessed authentication token of player " + gameObject2.GetComponent<QueryProcessor>().PlayerId + " (" + gameObject2.GetComponent<NicknameSync>().MyNick + ").", ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
											if (!q.Contains("STAFF", StringComparison.OrdinalIgnoreCase))
											{
												string myNick = gameObject2.GetComponent<NicknameSync>().MyNick;
												string str = "<color=white>Authentication token of player " + myNick + "(" + gameObject2.GetComponent<QueryProcessor>().PlayerId + "):\n" + gameObject2.GetComponent<CharacterClassManager>().AuthToken + "</color>";
												sender.RaReply(query[0].ToUpper() + ":PLAYER#" + str, success: true, logToConsole: true, "null");
												sender.RaReply("BigQR#" + gameObject2.GetComponent<CharacterClassManager>().AuthToken, success: true, logToConsole: false, "null");
											}
											else
											{
												sender.RaReply("StaffTokenReply#" + gameObject2.GetComponent<CharacterClassManager>().AuthToken, success: true, logToConsole: false, "null");
											}
										}
										catch (Exception ex)
										{
											sender.RaReply(query[0].ToUpper() + "#An unexpected problem has occurred!\nMessage: " + ex.Message + "\nStackTrace: " + ex.StackTrace + "\nAt: " + ex.Source, success: false, logToConsole: true, "PlayerInfo");
											throw;
										}
									}
									else
									{
										sender.RaReply(query[0].ToUpper() + ":PLAYER#Please specify the PlayerId!", success: false, logToConsole: true, "");
									}
									break;
								default:
									sender.RaReply(query[0].ToUpper() + "#Unknown parameter, type HELP to open the documentation.", success: false, logToConsole: true, "PlayerInfo");
									break;
							}
						}
						else
						{
							sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", success: false, logToConsole: true, "PlayerInfo");
						}
						break;
					case "CONTACT":
						sender.RaReply(query[0].ToUpper() + "#Contact email address: " + ConfigFile.ServerConfig.GetString("contact_email"), success: false, logToConsole: true, "");
						break;
					case "SERVER_EVENT":
						if (query.Length >= 2)
						{
							ServerLogs.AddLog(ServerLogs.Modules.Administrative, logName + " forced a server event: " + query[1].ToUpper(), ServerLogs.ServerLogType.RemoteAdminActivity_GameChanging);
							AlphaWarheadController component5 = GameObject.Find("Host").GetComponent<AlphaWarheadController>();
							bool flag15 = true;
							switch (query[1].ToUpper())
							{
								case "FORCE_CI_RESPAWN":
									if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.RespawnEvents, "ServerEvents"))
									{
										return false;
									}
									RespawnManager.Singleton.ForceSpawnTeam(SpawnableTeamType.ChaosInsurgency);
									break;
								case "FORCE_MTF_RESPAWN":
									if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.RespawnEvents, "ServerEvents"))
									{
										return false;
									}
									RespawnManager.Singleton.ForceSpawnTeam(SpawnableTeamType.NineTailedFox);
									break;
								case "DETONATION_START":
									if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.WarheadEvents, "ServerEvents"))
									{
										return false;
									}
									component5.InstantPrepare();
									component5.StartDetonation();
									break;
								case "DETONATION_CANCEL":
									if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.WarheadEvents, "ServerEvents"))
									{
										return false;
									}
									component5.CancelDetonation();
									break;
								case "DETONATION_INSTANT":
									if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.WarheadEvents, "ServerEvents"))
									{
										return false;
									}
									component5.InstantPrepare();
									component5.StartDetonation();
									component5.NetworktimeToDetonation = 5f;
									break;
								case "TERMINATE_UNCONN":
									if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.RoundEvents, "ServerEvents"))
									{
										return false;
									}
									foreach (NetworkConnection value3 in NetworkServer.connections.Values)
									{
										if (GameCore.Console.FindConnectedRoot(value3) == null)
										{
											value3.Disconnect();
											value3.Dispose();
										}
									}
									break;
								case "ROUND_RESTART":
								case "ROUNDRESTART":
								case "RR":
								case "RESTART":
									{
										if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.RoundEvents, "ServerEvents"))
										{
											return false;
										}
										PlayerStats component6 = PlayerManager.localPlayer.GetComponent<PlayerStats>();
										if (component6.isServer)
										{
											component6.Roundrestart();
										}
										break;
									}
								default:
									flag15 = false;
									break;
							}
							if (flag15)
							{
								sender.RaReply(query[0].ToUpper() + "#Started event: " + query[1].ToUpper(), success: true, logToConsole: true, "ServerEvents");
							}
							else
							{
								sender.RaReply(query[0].ToUpper() + "#Incorrect event! (Doesn't exist)", success: false, logToConsole: true, "ServerEvents");
							}
						}
						else
						{
							sender.RaReply(query[0].ToUpper() + "#To run this program, type at least 2 arguments! (some parameters are missing)", success: false, logToConsole: true, "");
						}
						break;
					case "HIDETAG":
						if (IsPlayer(sender, query[0]))
						{
							if (!string.IsNullOrEmpty(queryProcessor.Roles.HiddenBadge))
							{
								sender.RaReply(query[0].ToUpper() + "#Your badge is already hidden.", success: false, logToConsole: true, "");
								break;
							}
							if (string.IsNullOrEmpty(queryProcessor.Roles.MyText))
							{
								sender.RaReply(query[0].ToUpper() + "#Your don't have any badge.", success: false, logToConsole: true, "");
								break;
							}
							queryProcessor.Roles.GlobalHidden = queryProcessor.Roles.GlobalSet;
							queryProcessor.Roles.HiddenBadge = queryProcessor.Roles.MyText;
							queryProcessor.Roles.NetworkGlobalBadge = null;
							queryProcessor.Roles.SetText(null);
							queryProcessor.Roles.SetColor(null);
							queryProcessor.Roles.RefreshHiddenTag();
							sender.RaReply(query[0].ToUpper() + "#Tag hidden!", success: true, logToConsole: true, "");
						}
						break;
					case "SHOWTAG":
						if (IsPlayer(sender, query[0]))
						{
							queryProcessor.Roles.HiddenBadge = null;
							queryProcessor.Roles.GlobalHidden = false;
							queryProcessor.Roles.RpcResetFixed();
							queryProcessor.Roles.RefreshPermissions(disp: true);
							sender.RaReply(query[0].ToUpper() + "#Local tag refreshed!", success: true, logToConsole: true, "");
						}
						break;
					case "GTAG":
					case "GLOBALTAG":
						if (IsPlayer(sender, query[0], "ServerEvents") && !(queryProcessor == null))
						{
							if (string.IsNullOrEmpty(queryProcessor.Roles.PrevBadge))
							{
								sender.RaReply(query[0].ToUpper() + "#You don't have global tag.", success: false, logToConsole: true, "");
								break;
							}
							queryProcessor.Roles.HiddenBadge = null;
							queryProcessor.Roles.GlobalHidden = false;
							queryProcessor.Roles.RpcResetFixed();
							queryProcessor.Roles.NetworkGlobalBadge = queryProcessor.Roles.PrevBadge;
							sender.RaReply(query[0].ToUpper() + "#Global tag refreshed!", success: true, logToConsole: true, "");
						}
						break;
					case "PERM":
						{
							if (!IsPlayer(sender, query[0]) || queryProcessor == null)
							{
								break;
							}
							ulong permissions = queryProcessor.Roles.Permissions;
							string text16 = "Your permissions:";
							foreach (string allPermission in ServerStatic.PermissionsHandler.GetAllPermissions())
							{
								string text17 = ServerStatic.PermissionsHandler.IsRaPermitted(ServerStatic.PermissionsHandler.GetPermissionValue(allPermission)) ? "*" : "";
								text16 = text16 + "\n" + allPermission + text17 + " (" + ServerStatic.PermissionsHandler.GetPermissionValue(allPermission) + "): " + (ServerStatic.PermissionsHandler.IsPermitted(permissions, allPermission) ? "YES" : "NO");
							}
							sender.RaReply(query[0].ToUpper() + "#" + text16, success: true, logToConsole: true, "");
							break;
						}
					case "RL":
					case "RLOCK":
					case "ROUNDLOCK":
						if (CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.RoundEvents))
						{
							RoundSummary.RoundLock = !RoundSummary.RoundLock;
							sender.RaReply(query[0].ToUpper() + "#Round lock " + (RoundSummary.RoundLock ? "enabled!" : "disabled!"), success: true, logToConsole: true, "ServerEvents");
						}
						break;
					case "LL":
					case "LLOCK":
					case "LOBBYLOCK":
						if (CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.RoundEvents))
						{
							RoundStart.LobbyLock = !RoundStart.LobbyLock;
							sender.RaReply(query[0].ToUpper() + "#Lobby lock " + (RoundStart.LobbyLock ? "enabled!" : "disabled!"), success: true, logToConsole: true, "ServerEvents");
						}
						break;
					case "RT":
					case "RTIME":
					case "ROUNDTIME":
						if (RoundStart.RoundLenght.Ticks == 0L)
						{
							sender.RaReply(query[0].ToUpper() + "#The round has not yet started!", success: false, logToConsole: true, "");
						}
						else
						{
							sender.RaReply(query[0].ToUpper() + "#Round time: " + RoundStart.RoundLenght.ToString("hh\\:mm\\:ss\\.fff", CultureInfo.InvariantCulture), success: true, logToConsole: true, "");
						}
						break;
					case "PING":
						if (query.Length == 1)
						{
							if (queryProcessor == null)
							{
								sender.RaReply(query[0].ToUpper() + "#This command is only available for players!", success: false, logToConsole: true, "");
								break;
							}
							int connectionId = queryProcessor.connectionToClient.connectionId;
							if (connectionId == 0)
							{
								sender.RaReply(query[0].ToUpper() + "#This command is not available for the host!", success: false, logToConsole: true, "");
								break;
							}
							sender.RaReply(query[0].ToUpper() + "#Your ping: " + LiteNetLib4MirrorServer.Peers[connectionId].Ping + "ms", success: true, logToConsole: true, "");
						}
						else if (query.Length == 2)
						{
							if (!int.TryParse(query[1], out int id2))
							{
								sender.RaReply(query[0].ToUpper() + "#Invalid player id!", success: false, logToConsole: true, "");
								break;
							}
							GameObject gameObject7 = PlayerManager.players.FirstOrDefault((GameObject pl) => pl.GetComponent<QueryProcessor>().PlayerId == id2);
							if (gameObject7 == null)
							{
								sender.RaReply(query[0].ToUpper() + "#Invalid player id!", success: false, logToConsole: true, "");
								break;
							}
							int connectionId2 = gameObject7.GetComponent<NetworkIdentity>().connectionToClient.connectionId;
							if (connectionId2 == 0)
							{
								sender.RaReply(query[0].ToUpper() + "#This command is not available for the host!", success: false, logToConsole: true, "");
								break;
							}
							sender.RaReply(query[0].ToUpper() + "#Ping: " + LiteNetLib4MirrorServer.Peers[connectionId2].Ping + "ms", success: true, logToConsole: true, "");
						}
						else
						{
							sender.RaReply(query[0].ToUpper() + "#Too many arguments! (expected 0 or 1)", success: false, logToConsole: true, "");
						}
						break;
					case "VERSION":
						sender.RaReply(query[0].ToUpper() + "#Server Version: " + CustomNetworkManager.CompatibleVersions[0] + " " + Application.buildGUID, success: true, logToConsole: true, "");
						break;
					case "RELOADCONFIG":
						if (CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.ServerConfigs))
						{
							try
							{
								ConfigFile.ReloadGameConfigs();
								sender.RaReply(query[0].ToUpper() + "#Reloaded all configs!", success: true, logToConsole: true, "");
							}
							catch (Exception arg3)
							{
								sender.RaReply(query[0].ToUpper() + "#Reloading configs failed: " + arg3, success: false, logToConsole: true, "");
							}
						}
						break;
					case "KILL":
						if (!CheckPermissions(sender, query[0].ToUpper(), PlayerPermissions.PlayersManagement))
						{
							break;
						}
						if (query.Length <= 2)
						{
							if (!int.TryParse(query[1], out int id4))
							{
								sender.RaReply(query[0].ToUpper() + "#Invalid player id!", success: false, logToConsole: true, "");
								break;
							}
							GameObject gameObject = PlayerManager.players.FirstOrDefault((GameObject pl) => pl.GetComponent<QueryProcessor>().PlayerId == id4);
							if (gameObject == null)
							{
								sender.RaReply(query[0].ToUpper() + "#Invalid player id!", success: false, logToConsole: true, "");
							}
							else if (ReferenceHub.GetHub(gameObject).playerStats.HurtPlayer(new PlayerStats.HitInfo(-1f, "ADMIN", DamageTypes.Wall, 0), gameObject, noTeamDamage: true))
							{
								sender.RaReply(query[0].ToUpper() + "#Player has been killed!", success: true, logToConsole: true, "");
							}
							else
							{
								sender.RaReply(query[0].ToUpper() + "#Kill failed!", success: false, logToConsole: true, "");
							}
						}
						else
						{
							sender.RaReply(query[0].ToUpper() + "#Please specify the PlayerId!", success: false, logToConsole: true, "");
						}
						break;
					default:
						CommandHandler handler = CommandManager.GetCommandHandler(query[0]);
						string response = $"{query[0].ToUpper()}#{handler.Execute(sender.GetPlayer(), query.SkipCommand())}";
						sender.RaReply(response, true, true, "");
						break;
				}
				return false;
			}
			catch (Exception e)
			{
				Log.Add("CommandProcessor", e);
				return true;
			}
		}
    }

	[HarmonyPatch(typeof(QueryProcessor), nameof(QueryProcessor.ProcessGameConsoleQuery))]
	public static class ProcessConsoleQueryPatch
    {
		public static bool Prefix(QueryProcessor __instance, string query, bool encrypted)
        {
			try
            {
				Environment.OnConsoleCommand(query, __instance.gameObject, true, out string reply, out string color, out bool allow);
				if (!allow)
                {
					__instance.GCT.SendToClient(__instance.connectionToClient, reply, color.ToLower());
					return false;
                }
				string[] array = query.Split(' ');
				if (QueryProcessor.DotCommandHandler.TryGetCommand(array[0], out ICommand command))
				{
					try
					{
						command.Execute(array.Segment(1), __instance._sender, out string response);
						__instance.GCT.SendToClient(__instance.connectionToClient, array[0].ToUpper() + "#" + response, "");
					}
					catch (Exception arg)
					{
						__instance.GCT.SendToClient(__instance.connectionToClient, array[0].ToUpper() + "# Command execution failed! Error: " + arg, "");
					}
				}
				else
				{
					__instance.GCT.SendToClient(__instance.connectionToClient, "Command not found.", "red");
				}
				return false;
            }
			catch (Exception e)
            {
				Log.Add("QueryProcessor", e);
				return true;
            }
        }
    }

	[HarmonyPatch(typeof(Console), nameof(Console.TypeCommand), new Type[] { typeof(string), typeof(CommandSender) })]
	public static class ProcessServerQueryPatch
    {
		public static bool Prefix(Console __instance, string cmd, CommandSender sender = null)
        {
			try
			{
				if (cmd.StartsWith(".") && cmd.Length > 1)
				{
					AddLog("Sending command to server: " + cmd.Substring(1), new Color32(0, byte.MaxValue, 0, byte.MaxValue));
					PlayerManager.localPlayer.GetComponent<GameConsoleTransmission>().SendToServer(cmd.Substring(1));
					return false;
				}
				bool flag = cmd.StartsWith("@", StringComparison.Ordinal);
				if ((cmd.StartsWith("/", StringComparison.Ordinal) || flag) && cmd.Length > 1)
				{
					if (NetworkServer.active)
					{
						CommandProcessor.ProcessQuery(flag ? cmd : cmd.Substring(1), sender ?? _ccs);
					}
					return false;
				}
				string[] array = cmd.Split(' ');
				cmd = array[0].ToUpper();
				if (__instance.ConsoleCommandHandler.TryGetCommand(cmd, out ICommand command))
				{
					try
					{
						string response;
						bool flag2 = command.Execute(array.Segment(1), sender, out response);
						AddLog(response, flag2 ? Color.green : Color.red);
					}
					catch (Exception arg)
					{
						AddLog("Command execution failed! Error: " + arg, Color.red);
					}
					return false;
				}
				switch (cmd)
				{
					case "HELLO":
						AddLog("Hello World!", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
						break;
					case "LENNY":
						AddLog("<size=450>( \u0361° \u035cʖ \u0361°)</size>\n\n", new Color32(byte.MaxValue, 180, 180, byte.MaxValue));
						break;
					case "CONTACT":
						if (PlayerManager.localPlayer == null)
						{
							AddLog("You must join a server to execute this command.", Color.red);
							break;
						}
						AddLog("Requesting server-owner's contact email...", Color.yellow);
						PlayerManager.localPlayer.GetComponent<CharacterClassManager>().CmdRequestContactEmail();
						break;
					case "SRVCFG":
						if (PlayerManager.localPlayer == null)
						{
							AddLog("You must join a server to execute this command.", Color.red);
							break;
						}
						AddLog("Requesting server config...", Color.yellow);
						PlayerManager.localPlayer.GetComponent<CharacterClassManager>().CmdRequestServerConfig();
						break;
					case "GROUPS":
						if (PlayerManager.localPlayer == null)
						{
							AddLog("You must join a server to execute this command.", Color.red);
							break;
						}
						AddLog("Requesting server groups...", Color.yellow);
						PlayerManager.localPlayer.GetComponent<CharacterClassManager>().CmdRequestServerGroups();
						break;
					case "DEBUG":
						{
							int num = 4;
							if (array.Length == 1)
							{
								string text3 = "Welcome to Debug Mode. The following modules were found:";
								ConsoleDebugMode.GetList(out string[] keys, out string[] descriptions);
								for (int l = 0; l < keys.Length; l++)
								{
									text3 = text3 + "\n- <b>" + keys[l] + "</b> - " + descriptions[l];
								}
								AddDebugLog("MODE", text3, MessageImportance.MostImportant);
							}
							else if (array.Length == 2)
							{
								AddDebugLog("MODE", ConsoleDebugMode.ConsoleGetLevel(array[1]), MessageImportance.MostImportant);
							}
							else
							{
								if (array.Length < 3)
								{
									break;
								}
								if (int.TryParse(array[2], out int result4) || result4 < 0 || result4 > num)
								{
									array[1] = array[1].ToUpper();
									if (ConsoleDebugMode.ChangeImportance(array[1], result4))
									{
										AddDebugLog("MODE", "Debug Level was modified. " + ConsoleDebugMode.ConsoleGetLevel(array[1]), MessageImportance.MostImportant);
									}
									else
									{
										AddDebugLog("MODE", "Could not change the Debug Mode importance: Module '" + array[1] + "' could not be found.", MessageImportance.MostImportant);
									}
								}
								else
								{
									AddDebugLog("MODE", "Could not change the Debug Mode importance: '" + array[2] + "' is supposed to be a integer value between 0 and " + num + ".", MessageImportance.MostImportant);
								}
							}
							break;
						}
					case "ADMINME":
					case "OVERRIDE":
						{
							GameObject gameObject = GameObject.Find("Host");
							if (gameObject != null && gameObject.GetComponent<NetworkIdentity>().isLocalPlayer)
							{
								ServerRoles component3 = gameObject.GetComponent<ServerRoles>();
								if (!component3.PublicKeyAccepted)
								{
									AddLog("Authentication wasn't performed. Is the server running in online mode?", Color.red);
									break;
								}
								component3.RemoteAdmin = true;
								component3.OverwatchPermitted = true;
								component3.Permissions = ServerStatic.PermissionsHandler.FullPerm;
								component3.AdminChatPerms = true;
								component3.TargetOpenRemoteAdmin(component3.connectionToClient, password: false);
								AddLog("Remote admin enabled for you.", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
							}
							break;
						}
					case "ID":
					case "MYID":
						AddLog("Your Player ID on the current server: " + PlayerManager.localPlayer.GetComponent<QueryProcessor>().PlayerId, Color.green);
						break;
					case "PLAYERS":
					case "PL":
					case "LIST":
						{
							Dictionary<GameObject, ReferenceHub> allHubs = ReferenceHub.GetAllHubs();
							AddLog($"List of players ({(ServerStatic.IsDedicated ? (allHubs.Count - 1) : allHubs.Count)}):", Color.cyan);
							foreach (ReferenceHub value in allHubs.Values)
							{
								if (!value.isDedicatedServer)
								{
									AddLog("- " + (value.nicknameSync.CombinedName ?? "(no nickname)") + ": " + (value.characterClassManager.UserId ?? "(no User ID)") + " [" + value.queryProcessor.PlayerId + "]", Color.gray);
								}
							}
							break;
						}
					case "KEY":
						{
							GameObject localPlayer = PlayerManager.localPlayer;
							if (localPlayer.GetComponent<RemoteAdminCryptographicManager>().EncryptionKey == null)
							{
								AddLog("Encryption key: (null) - session not encrypted (probably due to online mode disabled).", Color.grey);
							}
							else
							{
								AddLog("Encryption key (KEEP SECRET!): " + BitConverter.ToString(localPlayer.GetComponent<RemoteAdminCryptographicManager>().EncryptionKey), Color.grey);
							}
							break;
						}
					case "KEYHASH":
					case "KHASH":
					case "KH":
						AddLog("SHA256 hash of Central Server Public Key: " + Sha.HashToString(Sha.Sha256(ECDSA.KeyToString(_publicKey))), Color.green);
						break;
					case "GIVE":
						{
							int result3;
							if (!(PlayerManager.localPlayer.GetComponent<CharacterClassManager>().isServer ? PlayerManager.localPlayer : null))
							{
								AddLog("You're not owner of this server!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
							}
							else if (array.Length >= 2 && int.TryParse(array[1], out result3))
							{
								string a4 = "offline";
								Inventory component5 = PlayerManager.localPlayer.GetComponent<Inventory>();
								if (component5 != null)
								{
									a4 = "online";
									if (component5.availableItems.Length > result3)
									{
										component5.AddNewItem((ItemType)result3);
										break;
									}
									AddLog("Failed to add ITEM#" + result3.ToString("000") + " - item does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
								}
								if (a4 == "offline" || a4 == "online")
								{
									AddLog((a4 == "offline") ? "You cannot use that command if you are not playing on any server!" : "Player inventory script couldn't be find!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
								}
								else
								{
									AddLog("ITEM#" + result3.ToString("000") + " has been added!", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
								}
							}
							else
							{
								AddLog("Second argument has to be a number!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
							}
							break;
						}
					case "ROUNDRESTART":
						{
							bool flag3 = false;
							PlayerStats component2 = PlayerManager.localPlayer.GetComponent<PlayerStats>();
							if (component2.isLocalPlayer && component2.isServer)
							{
								flag3 = true;
								AddLog("The round is about to restart! Please wait..", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
								component2.Roundrestart();
							}
							if (!flag3)
							{
								AddLog("You're not owner of this server!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
							}
							break;
						}
					case "ITEMLIST":
						{
							string a2 = "offline";
							int result2 = 1;
							if (array.Length >= 2 && !int.TryParse(array[1], out result2))
							{
								AddLog("Please enter correct page number!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
								break;
							}
							Inventory component4 = PlayerManager.localPlayer.GetComponent<Inventory>();
							if (component4 != null)
							{
								a2 = "none";
								if (result2 < 1)
								{
									AddLog("Page '" + result2 + "' does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
									__instance.RefreshConsoleScreen();
									break;
								}
								Item[] availableItems = component4.availableItems;
								for (int k = 10 * (result2 - 1); k < 10 * result2; k++)
								{
									if (10 * (result2 - 1) > availableItems.Length)
									{
										AddLog("Page '" + result2 + "' does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
										break;
									}
									if (k >= availableItems.Length)
									{
										break;
									}
									AddLog("ITEM#" + k.ToString("000") + " : " + availableItems[k].label, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
								}
							}
							if (a2 != "none")
							{
								AddLog((a2 == "offline") ? "You cannot use that command if you are not playing on any server!" : "Player inventory script couldn't be find!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
							}
							break;
						}
					case "BAN":
						{
							if (!GameObject.Find("Host").GetComponent<NetworkIdentity>().isLocalPlayer)
							{
								break;
							}
							if (array.Length < 3)
							{
								AddLog("Syntax: BAN [player kick / ip] [minutes or time]", new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue));
								foreach (NetworkConnection value2 in NetworkServer.connections.Values)
								{
									string text4 = string.Empty;
									GameObject gameObject3 = FindConnectedRoot(value2);
									if (gameObject3 != null)
									{
										text4 = gameObject3.GetComponent<NicknameSync>().MyNick;
									}
									if (text4 == string.Empty)
									{
										AddLog("Player :: " + value2.address, new Color32(160, 128, 128, byte.MaxValue), nospace: true);
									}
									else
									{
										AddLog("Player :: " + text4 + " :: " + value2.address, new Color32(128, 160, 128, byte.MaxValue), nospace: true);
									}
								}
								break;
							}
							bool flag6 = false;
							int num2 = 0;
							try
							{
								num2 = Misc.RelativeTimeToSeconds(array[2], 60);
							}
							catch
							{
								AddLog("Invalid time: " + array[2], Color.red);
								num2 = -77;
							}
							if (num2 >= 0)
							{
								foreach (NetworkConnection value3 in NetworkServer.connections.Values)
								{
									GameObject gameObject4 = FindConnectedRoot(value3);
									if (value3.address.Contains(array[1], StringComparison.OrdinalIgnoreCase) || (!(gameObject4 == null) && gameObject4.GetComponent<NicknameSync>().MyNick.Contains(array[1], StringComparison.OrdinalIgnoreCase)))
									{
										flag6 = true;
										PlayerManager.localPlayer.GetComponent<BanPlayer>().BanUser(gameObject4, num2, string.Empty, "Administrator");
										AddLog("Player banned.", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
									}
								}
								if (!flag6)
								{
									AddLog("Player not found.", new Color32(byte.MaxValue, byte.MaxValue, 0, byte.MaxValue));
								}
							}
							else if (num2 != -77)
							{
								AddLog("Invalid time: " + array[2], Color.red);
							}
							break;
						}
					case "QUIT":
					case "EXIT":
						IdleMode.SetIdleMode(state: false);
						AddLog("The server is about to restart, please wait!", new Color32(255, 0, 0, 1));
						__instance.Invoke("QuitGame", 1f);
						break;
					case "HELP":
						if (array.Length > 1)
						{
							string text = array[1].ToUpper();
							CommandHint[] array2 = __instance.hints;
							foreach (CommandHint commandHint in array2)
							{
								if (!(commandHint.name != text))
								{
									AddLog(commandHint.name + " - " + commandHint.fullDesc, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
									__instance.RefreshConsoleScreen();
									return false;
								}
							}
							AddLog("Help for command '" + text + "' does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
							__instance.RefreshConsoleScreen();
						}
						else
						{
							AddLog("List of available commands:\n", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
							CommandHint[] array2 = __instance.hints;
							foreach (CommandHint commandHint2 in array2)
							{
								AddLog(commandHint2.name + " - " + commandHint2.shortDesc, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue), nospace: true);
							}
							AddLog("Type 'HELP [COMMAND]' to print a full description of the chosen command.", new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
							__instance.RefreshConsoleScreen();
						}
						break;
					case "COLOR":
					case "COLORS":
						{
							bool flag4 = array.Length > 1 && string.Equals(array[1], "LIST", StringComparison.OrdinalIgnoreCase);
							bool flag5 = (array.Length > 1 && string.Equals(array[1], "ALL", StringComparison.OrdinalIgnoreCase)) || (array.Length > 2 && string.Equals(array[2], "ALL", StringComparison.OrdinalIgnoreCase));
							AddLog("Available colors:", Color.gray);
							string text2 = string.Empty;
							ServerRoles.NamedColor[] namedColors = PlayerManager.localPlayer.GetComponent<ServerRoles>().NamedColors;
							foreach (ServerRoles.NamedColor namedColor in namedColors)
							{
								if (!namedColor.Restricted || flag5)
								{
									if (flag4)
									{
										AddLog("<color=#" + namedColor.ColorHex + ">" + namedColor.Name + (namedColor.Restricted ? "*" : string.Empty) + " - #" + namedColor.ColorHex + "</color>", Color.white);
									}
									else
									{
										text2 = text2 + "<color=#" + namedColor.ColorHex + ">" + namedColor.Name + (namedColor.Restricted ? "*" : string.Empty) + "</color>    ";
									}
								}
							}
							if (!flag4)
							{
								AddLog(text2, Color.white);
							}
							break;
						}
					case "SEED":
						{
							GameObject gameObject2 = GameObject.Find("Host");
							AddLog("Map seed is: <b>" + ((gameObject2 == null) ? "NONE" : gameObject2.GetComponent<RandomSeedSync>().seed.ToString()) + "</b>", new Color32(0, byte.MaxValue, 0, byte.MaxValue));
							break;
						}
					case "CENTRAL":
					case "CS":
					case "CSRV":
						{
							string str = CentralServer.Servers.Aggregate((string current, string adding) => current = current + ", " + adding);
							AddLog("Use \"" + array[0].ToUpper() + " -r\" to change to different central server.", Color.gray);
							AddLog("Use \"" + array[0].ToUpper() + " -t\" to change to TEST central server.", Color.gray);
							AddLog("Use \"" + array[0].ToUpper() + " -s CentralServerNameHere\" to change to specified central server.", Color.gray);
							if (array.Length > 1)
							{
								string a3 = array[1].ToUpper();
								if (!(a3 == "-R"))
								{
									if (a3 == "-T")
									{
										CentralServer.SelectedServer = "TEST";
										CentralServer.MasterUrl = "https://test.scpslgame.com/";
										CentralServer.StandardUrl = "https://test.scpslgame.com/";
										CentralServer.TestServer = true;
										AddLog("--- Central server changed to TEST SERVER ---", Color.green);
									}
									else if ((string.Equals(array[1], "-S", StringComparison.OrdinalIgnoreCase) || string.Equals(array[1], "-FS", StringComparison.OrdinalIgnoreCase)) && array.Length == 3)
									{
										if (!CentralServer.Servers.Contains<string>(array[2].ToUpper()) && !string.Equals(array[1], "-FS", StringComparison.OrdinalIgnoreCase))
										{
											AddLog("Server " + array[2].ToUpper() + " is not on the list. Use " + array[0].ToUpper() + " -fs " + array[2].ToUpper() + " to force the change.", Color.red);
											break;
										}
										CentralServer.SelectedServer = array[2].ToUpper();
										CentralServer.StandardUrl = "https://" + array[2].ToUpper() + ".scpslgame.com/";
										CentralServer.TestServer = false;
										AddLog("--- Central server changed to " + array[2].ToUpper() + " ---", Color.green);
									}
								}
								else
								{
									CentralServer.ChangeCentralServer(remove: false);
									AddLog("--- Central server changed ---", Color.green);
								}
							}
							AddLog("Master central server: " + CentralServer.MasterUrl, Color.green);
							AddLog("Selected central server: " + CentralServer.SelectedServer + " (" + CentralServer.StandardUrl + ")", Color.green);
							AddLog("All central servers: " + str, Color.green);
							break;
						}
					case "CLASSLIST":
						{
							string a = "offline";
							int result = 1;
							if (array.Length >= 2 && !int.TryParse(array[1], out result))
							{
								AddLog("Please enter correct page number!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
								break;
							}
							CharacterClassManager component = PlayerManager.localPlayer.GetComponent<CharacterClassManager>();
							if (component != null)
							{
								a = "none";
								if (result < 1)
								{
									AddLog("Page '" + result + "' does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
									__instance.RefreshConsoleScreen();
									break;
								}
								Role[] classes = component.Classes;
								for (int i = 10 * (result - 1); i < 10 * result; i++)
								{
									if (10 * (result - 1) > classes.Length)
									{
										AddLog("Page '" + result + "' does not exist!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
										break;
									}
									if (i >= classes.Length)
									{
										break;
									}
									AddLog("CLASS#" + i.ToString("000") + " : " + classes[i].fullName, new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
								}
							}
							if (a != "none")
							{
								AddLog((a == "offline") ? "You cannot use that command if you are not playing on any server!" : "Player inventory script couldn't be find!", new Color32(byte.MaxValue, 180, 0, byte.MaxValue));
							}
							break;
						}
					default:
						break;
				}
				return false;
			}
			catch (Exception e)
            {
				Log.Add("GameCore.Console", e);
				return true;
            }
		}
    }
}

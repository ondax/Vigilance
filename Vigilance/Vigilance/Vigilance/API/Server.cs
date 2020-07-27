using MEC;
using System;
using System.Collections.Generic;
using Vigilance.API.Enums;
using UnityEngine;
using Vigilance.API.Extensions;
using GameCore;

namespace Vigilance.API
{
    public static class Server
    {
        public static class ServerSettings
        {
            public static string Name => ConfigManager.GetString("server_name");
            public static string IpAdress => ConfigManager.GetString("server_ip");
            public static string PlayerListTitle => ConfigManager.GetString("player_list_title");
            public static int PlayerListTitleRate => ConfigManager.GetInt("player_list_title_rate");
            public static string PastebinID => ConfigManager.GetString("serverinfo_pastebin_id");
            public static int MaxPlayers => ConfigManager.GetInt("max_players");
            public static int Tickrate => ConfigManager.GetInt("server_tickrate");
            public static bool UseReservedSlots => ConfigManager.GetBool("use_reserved_slots");
            public static int LobbyWaitingTime => ConfigManager.GetInt("lobby_waiting_time");
            public static string IPv4Bind => ConfigManager.GetString("ipv4_bind_ip");
            public static string IPv6Bind => ConfigManager.GetString("ipv6_bind_ip");
            public static string ContactEmail => ConfigManager.GetString("contact_email");
            public static bool OnlineMode => ConfigManager.GetBool("online_mode");
            public static bool IpBanning => ConfigManager.GetBool("ip_banning");
            public static bool Whitelist => ConfigManager.GetBool("enable_whitelist");
            public static bool ForwardPorts => ConfigManager.GetBool("forward_ports");
            public static bool EnableQuery => ConfigManager.GetBool("enable_query");
            public static int QueryPortShift => ConfigManager.GetInt("query_port_shift");
            public static bool QueryUseIPv6 => ConfigManager.GetBool("query_use_IPv6");
            public static string AdminQueryPassword => ConfigManager.GetString("administrator_query_password");
            public static int ConnectionsDelayTime => ConfigManager.GetInt("connections_delay_time");
            public static bool SyncCommandBinding => ConfigManager.GetBool("enable_sync_command_binding");
            public static bool RateLimitKick => ConfigManager.GetBool("ratelimit_kick");
            public static bool SameAccountJoining => ConfigManager.GetBool("same_account_joining");
        }

        public static GameObject Host => PlayerManager.localPlayer;
        public static List<Player> Players
        {
            get
            {
                List<Player> players = new List<Player>();
                foreach (GameObject gameObject in PlayerManager.players)
                {
                    if (!string.IsNullOrEmpty(gameObject.GetComponent<CharacterClassManager>().UserId) && !gameObject.GetComponent<CharacterClassManager>().IsHost)
                    {
                        players.Add(gameObject.GetPlayer());
                    }
                }
                return players;
            }
        }

        public static void Restart(bool safeRestart = true)
        {
            if (safeRestart)
            {
                Timing.RunCoroutine(SafeRestart());
                return;
            }
            Application.Quit();
        }

        private static IEnumerator<float> SafeRestart()
        {
            Host.GetComponent<PlayerStats>().Roundrestart();
            yield return Timing.WaitForSeconds(1.5f);
            Application.Quit();
        }

        public static bool Ban(Player player, int duration)
        {
            return Server.Host.GetComponent<BanPlayer>().BanUser(player.GameObject, duration, "No reason given.", "Server");
        }

        public static bool Ban(Player player, int duration, string message)
        {
            return Host.GetComponent<BanPlayer>().BanUser(player.GameObject, duration, message, "Server");
        }

        public static bool Ban(Player player, int duration, string reason, string issuer)
        {
            return Host.GetComponent<BanPlayer>().BanUser(player.GameObject, duration, reason, issuer);
        }

        public static void Kick(Player player)
        {
            ServerConsole.Disconnect(player.GameObject, "No reason given.");
        }

        public static void Kick(Player player, string reason)
        {
            ServerConsole.Disconnect(player.GameObject, reason);
        }

        public static void IssuePermanentBan(Player player)
        {
            Ban(player, int.MaxValue);
        }

        public static void IssuePermanentBan(Player player, string reason)
        {
            Ban(player, int.MaxValue, reason);
        }

        public static void IssueOfflineBan(DurationType type, int duration, string userId, string issuer, string reason)
        {
            switch (type)
            {
                case DurationType.Seconds:
                    {
                        long ticks = DateTime.UtcNow.AddSeconds((double)duration).Ticks;
                        BanHandler.IssueBan(new BanDetails
                        {
                            Expires = ticks,
                            Id = userId,
                            IssuanceTime = DateTime.UtcNow.Ticks,
                            Issuer = issuer,
                            OriginalName = "Offline Player",
                            Reason = (string.IsNullOrEmpty(reason) ? "No reason specified." : reason)
                        }, BanHandler.BanType.UserId);
                        return;
                    }
                case DurationType.Minutes:
                    {
                        long ticks2 = DateTime.UtcNow.AddMinutes((double)duration).Ticks;
                        BanHandler.IssueBan(new BanDetails
                        {
                            Expires = ticks2,
                            Id = userId,
                            IssuanceTime = DateTime.UtcNow.Ticks,
                            Issuer = issuer,
                            OriginalName = "Offline Player",
                            Reason = (string.IsNullOrEmpty(reason) ? "No reason specified." : reason)
                        }, BanHandler.BanType.UserId);
                        return;
                    }
                case DurationType.Hours:
                    {
                        long ticks3 = DateTime.UtcNow.AddHours((double)duration).Ticks;
                        BanHandler.IssueBan(new BanDetails
                        {
                            Expires = ticks3,
                            Id = userId,
                            IssuanceTime = DateTime.UtcNow.Ticks,
                            Issuer = issuer,
                            OriginalName = "Offfline Player",
                            Reason = (string.IsNullOrEmpty(reason) ? "No reason specified." : reason)
                        }, BanHandler.BanType.UserId);
                        return;
                    }
                case DurationType.Days:
                    {
                        long ticks4 = DateTime.UtcNow.AddDays((double)duration).Ticks;
                        BanHandler.IssueBan(new BanDetails
                        {
                            Expires = ticks4,
                            Id = userId,
                            IssuanceTime = DateTime.UtcNow.Ticks,
                            Issuer = issuer,
                            OriginalName = "Offline Player",
                            Reason = (string.IsNullOrEmpty(reason) ? "No reason specified." : reason)
                        }, BanHandler.BanType.UserId);
                        return;
                    }
                case DurationType.Months:
                    {
                        long ticks5 = DateTime.UtcNow.AddMonths(duration).Ticks;
                        BanHandler.IssueBan(new BanDetails
                        {
                            Expires = ticks5,
                            Id = userId,
                            IssuanceTime = DateTime.UtcNow.Ticks,
                            Issuer = issuer,
                            OriginalName = "Offline Player",
                            Reason = (string.IsNullOrEmpty(reason) ? "No reason specified." : reason)
                        }, BanHandler.BanType.UserId);
                        return;
                    }
                case DurationType.Years:
                    {
                        long ticks6 = DateTime.UtcNow.AddYears(duration).Ticks;
                        BanHandler.IssueBan(new BanDetails
                        {
                            Expires = ticks6,
                            Id = userId,
                            IssuanceTime = DateTime.UtcNow.Ticks,
                            Issuer = issuer,
                            OriginalName = "Offline Player",
                            Reason = (string.IsNullOrEmpty(reason) ? "No reason specified." : reason)
                        }, BanHandler.BanType.UserId);
                        return;
                    }
                default:
                    return;
            }
        }

        public static void ReloadConfigs()
        {
            ConfigFile.ReloadGameConfigs(false);
            ServerStatic.PermissionsHandler?.RefreshPermissions();
        }

        public static void RunCommand(string command)
        {
            GameCore.Console.singleton.TypeCommand($"/{command}", new ConsoleCommandSender());
        }
    }
}

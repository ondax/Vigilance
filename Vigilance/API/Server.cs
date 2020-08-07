using System;
using System.Collections.Generic;
using MEC;
using Vigilance.Enums;
using Vigilance.Extensions;
using GameCore;
using UnityEngine;

namespace Vigilance.API
{
    public static class Server
    {
        public static GameObject Host => PlayerManager.localPlayer;
        public static List<Player> Players => PlayerManager.players.GetPlayers();
        public static int Port => (int)ServerStatic.ServerPort;

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
            return Host.GetComponent<BanPlayer>().BanUser(player.GameObject, duration, "No reason given.", "Server");
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

        public static string RunCommand(string command)
        {
            return ServerConsole.EnterCommand(command, out ConsoleColor color, new ConsoleCommandSender());
        }

        public static void DeleteLogs()
        {
            Paths.Delete($"{System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData)}/SCP Secret Laboratory/ServerLogs");
        }
    }
}

using System;
using System.Collections.Generic;
using MEC;
using Vigilance.Enums;
using GameCore;
using UnityEngine;
using Vigilance.Extensions;

namespace Vigilance.API
{
    public static class Server
    {
        public static string Version => CustomNetworkManager.CompatibleVersions[0];
        public static GameObject Host => ReferenceHub.LocalHub.gameObject;
        public static GameObject GameManager => GameObject.Find("GameManager");
        public static ReferenceHub LocalHub => ReferenceHub.LocalHub;
        public static IEnumerable<Player> Players => PlayerList.PlayersDict.Values;
        public static int Port => ServerStatic.ServerPortSet ? ServerStatic.ServerPort : 7777;
        public static int MaxPlayers => ConfigFile.ServerConfig.GetInt("max_players", 20);
        public static bool RoundLock { get => RoundSummary.RoundLock; set => RoundSummary.RoundLock = value; }
        public static bool LobbyLock { get => RoundStart.LobbyLock; set => RoundStart.LobbyLock = value; }
        public static string Name { get => ServerConsole._serverName; set => ServerConsole._serverName = value; }
        public static string IpAddress { get => ServerConsole.Ip; set => ServerConsole.Ip = value; }

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
            yield return Timing.WaitForSeconds(0.5f);
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
            BanHandler.BanType banType;
            if (userId.Contains("@steam") || userId.Contains("@discord"))
                banType = BanHandler.BanType.UserId;
            else
                banType = BanHandler.BanType.IP;
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
                        }, banType);
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
                        }, banType);
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
                        }, banType);
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
                        }, banType);
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
                        }, banType);
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
                        }, banType);
                        return;
                    }
                default:
                    return;
            }
        }

        public static void ReloadConfigs()
        {
            ConfigFile.ReloadGameConfigs(false);
            PluginManager.Config.Reload();
            PluginManager.ReloadPluginConfigs();
            ServerStatic.PermissionsHandler?.RefreshPermissions();
        }

        public static string RunCommand(string command)
        {
            return ServerConsole.EnterCommand(command, out ConsoleColor color, new ConsoleCommandSender());
        }

        public static class PlayerList
        {
            public static Dictionary<GameObject, Player> PlayersDict { get; set; } = new Dictionary<GameObject, Player>();
            public static Dictionary<string, Player> UserIdCache { get; set; } = new Dictionary<string, Player>();
            public static Dictionary<int, Player> PlayerIdCache { get; set; } = new Dictionary<int, Player>();
            public static Dictionary<string, List<int>> TargetGhosts { get; set; } = new Dictionary<string, List<int>>();
            public static Player Local { get; set; } = new Player(ReferenceHub.LocalHub);
            public static Player Host { get; set; } = new Player(ReferenceHub.HostHub);

            public static void Reset()
            {
                Log.Add("PlayerList", "Resetting PlayerList", LogType.Debug);
                PlayersDict.Clear();
                UserIdCache.Clear();
                PlayerIdCache.Clear();
                TargetGhosts.Clear();
            }

            public static void Add(Player player)
            {
                if (player.IsHost || string.IsNullOrEmpty(player.UserId) || player.IpAddress == "localClient" || player == null)
                    return;
                if (Contains(player))
                    return;
                Log.Add("PlayerList", $"{player.Nick} ({player.UserId}) [{player.IpAddress}] has joined, adding to list.", LogType.Debug);
                PlayersDict.Add(player.GameObject, player);
                UserIdCache.Add(player.UserId, player);
                PlayerIdCache.Add(player.PlayerId, player);
                TargetGhosts.Add(player.UserId, new List<int>());
            }

            public static void Remove(Player player)
            {
                if (player.IsHost || string.IsNullOrEmpty(player.UserId) || player.IpAddress == "localClient" || player == null)
                    return;
                if (!Contains(player))
                    return;
                Log.Add("PlayerList", $"{player.Nick} ({player.UserId}) [{player.IpAddress}] has disconnected, removing from list.", LogType.Debug);
                PlayersDict.Remove(player.GameObject);
                UserIdCache.Remove(player.UserId);
                PlayerIdCache.Remove(player.PlayerId);
                TargetGhosts.Remove(player.UserId);
            }

            public static bool Contains(Player player)
            {
                if (player.IsHost || string.IsNullOrEmpty(player.UserId) || player.IpAddress == "localClient" || player == null)
                    return false;
                if (PlayersDict.ContainsValue(player) && UserIdCache.ContainsKey(player.UserId) && PlayerIdCache.ContainsKey(player.PlayerId))
                    return true;
                else
                    return false;
            }

            public static List<Player> GetPlayers(RoleType role)
            {
                List<Player> players = new List<Player>();
                foreach (Player player in PlayersDict.Values)
                {
                    if (player.Role == role)
                        players.Add(player);
                }
                return players;
            }

            public static List<Player> GetPlayers(TeamType team)
            {
                List<Player> players = new List<Player>();
                foreach (Player player in PlayersDict.Values)
                {
                    if (player.Team == team)
                        players.Add(player);
                }
                return players;
            }

            public static Player GetPlayer(GameObject gameObject)
            {
                if (gameObject == null)
                    return Local;
                if (!PlayersDict.TryGetValue(gameObject, out Player player))
                    player = new Player(ReferenceHub.GetHub(gameObject));
                return player;
            }

            public static Player GetPlayer(ReferenceHub hub)
            {
                return GetPlayer(hub.gameObject);
            }

            public static Player GetPlayer(int playerId)
            {
                if (!PlayerIdCache.TryGetValue(playerId, out Player player))
                {
                    foreach (Player ply in PlayersDict.Values)
                        if (ply.PlayerId == playerId)
                            player = ply;
                }
                return player;
            }

            public static Player GetPlayerByUserId(string id)
            {
                if (!UserIdCache.TryGetValue(id, out Player ply))
                {
                    foreach (Player player in PlayersDict.Values)
                        if (player.UserId == id || player.ParsedUserId == id)
                            ply = player;
                }
                return ply;
            }

            public static Player GetPlayer(string args)
            {
                try
                {
                    Player playerFound = null;

                    foreach (string userId in UserIdCache.Keys)
                    {
                        if (userId == args)
                            return UserIdCache[userId];
                    }

                    if (int.TryParse(args, out int id))
                    {
                        return GetPlayer(id);
                    }

                    if (args.EndsWith("@steam") || args.EndsWith("@discord") || args.EndsWith("@northwood") || args.EndsWith("@patreon"))
                    {
                        playerFound = GetPlayerByUserId(args);
                    }
                    else
                    {
                        if (args == "WORLD" || args == "SCP-018" || args == "SCP-575" || args == "SCP-207")
                            return null;
                        int maxNameLength = 31, lastnameDifference = 31;
                        string firstString = args.ToLower();
                        foreach (Player player in PlayersDict.Values)
                        {
                            if (!player.Nick.ToLower().Contains(args.ToLower()))
                                continue;
                            if (firstString.Length < maxNameLength)
                            {
                                int x = maxNameLength - firstString.Length;
                                int y = maxNameLength - player.Nick.Length;
                                string secondString = player.Nick;
                                for (int i = 0; i < x; i++)
                                    firstString += "z";
                                for (int i = 0; i < y; i++)
                                    secondString += "z";
                                int nameDifference = firstString.GetDistance(secondString);
                                if (nameDifference < lastnameDifference)
                                {
                                    lastnameDifference = nameDifference;
                                    playerFound = player;
                                }
                            }
                        }
                    }
                    return playerFound;
                }
                catch (Exception exception)
                {
                    Log.Add("PlayerList", exception);
                    return null;
                }
            }
        }
    }
}

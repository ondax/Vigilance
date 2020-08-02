using System;
using System.IO;
using Vigilance.API;
using Vigilance.API.Extensions;

namespace Vigilance
{
    public static class FileLog
    {
        public static bool Enabled => ConfigManager.GetBool("enable_file_logging");
        public static string LogsPath => $"{PluginManager.Directories.Vigilance}/Logs";
        public static string DebugLogPath => $"{LogsPath}/Debug.txt";
        public static string InfoLogPath => $"{LogsPath}/Info.txt";
        public static string WarnLogPath => $"{LogsPath}/Warn.txt";
        public static string ErrorLogPath => $"{LogsPath}/Error.txt";
        public static string ConsoleLogPath => $"{LogsPath}/Console.txt";
        public static string RemoteAdminLogPath => $"{LogsPath}/RemoteAdmin.txt";
        public static string BanLogPath => $"{LogsPath}/Bans.txt";
        public static string KillsLogPath => $"{LogsPath}/Kills.txt";

        public static void CheckDirectories()
        {
            if (!Directory.Exists(LogsPath))
                Directory.CreateDirectory(LogsPath);
            if (!File.Exists(DebugLogPath))
                File.Create(DebugLogPath);
            if (!File.Exists(InfoLogPath))
                File.Create(InfoLogPath);
            if (!File.Exists(WarnLogPath))
                File.Create(WarnLogPath);
            if (!File.Exists(ErrorLogPath))
                File.Create(ErrorLogPath);
            if (!File.Exists(ConsoleLogPath))
                File.Create(ConsoleLogPath);
            if (!File.Exists(RemoteAdminLogPath))
                File.Create(RemoteAdminLogPath);
            if (!File.Exists(BanLogPath))
                File.Create(BanLogPath);
            if (!File.Exists(KillsLogPath))
                File.Create(KillsLogPath);
        }

        public static void Info(string tag, string message)
        {
            if (!Enabled)
                return;
            WriteLine($"[{tag}]: {message}", InfoLogPath);
        }

        public static void Debug(string tag, string message)
        {
            if (!Enabled)
                return;
            WriteLine($"[{tag}]: {message}", DebugLogPath);
        }

        public static void Warn(string tag, string message)
        {
            if (!Enabled)
                return;
            WriteLine($"[{tag}]: {message}", WarnLogPath);
        }

        public static void Error(string tag, string message)
        {
            if (!Enabled)
                return;
            WriteLine($"[{tag}]: {message}", ErrorLogPath);
        }

        public static void ConsoleLog(string message)
        {
            if (!Enabled)
                return;
            WriteLine(message, ConsoleLogPath);
        }

        public static void RemoteAdminLog(string command, Player issuer)
        {
            if (!Enabled)
                return;
            WriteLine($"{issuer.Nick} ({issuer.UserId}) [{issuer.UserGroup.BadgeText}] executed command \"{command}\"", RemoteAdminLogPath);
        }

        public static void BanLog(Player issuer, Player banned, string reason, int duration)
        {
            if (!Enabled)
                return;
            string banType = banned.UserId.Contains("@steam") || banned.UserId.Contains("@discord") || banned.UserId.Contains("@patreon") || banned.UserId.Contains("@northwood") ? "UserID" : "IP";
            string id = banType == "IP" ? banned.IpAdress : banned.UserId;
            WriteLine($"", BanLogPath);
            WriteLine($"Issuer: {issuer.Nick} ({issuer.UserId})", BanLogPath);
            WriteLine($"Banned Nick: {banned.Nick}", BanLogPath);
            WriteLine($"Banned {banType}: {id}", BanLogPath);
            WriteLine($"Duration: {duration}", BanLogPath);
            WriteLine($"Reason: {reason}", BanLogPath);
            WriteLine("", BanLogPath);
        }

        public static void KillLog(Player killer, Player target, PlayerStats.HitInfo hitInfo)
        {
            if (!Enabled)
                return;
            WriteLine($"[{target.Role}] {target.Nick} ({target.UserId}) has been killed by [{killer.Role}] {killer.Nick} ({killer.UserId}) using {hitInfo.GetDamageType().Convert()} dealing {hitInfo.Amount} damage.", KillsLogPath);
        }

        public static void WriteLine(string line, string filePath)
        {
            if (!Enabled)
                return;
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine($"[{DateTime.UtcNow.ToString("HH:mm:ss")}] {line}");
                }
            }
            catch (Exception e)
            {
                Log.Error("FileLog", e);
            }
        }
    }
}

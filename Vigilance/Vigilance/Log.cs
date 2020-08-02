using System;

namespace Vigilance
{
    public static class Log
    {
        public static bool EnableDebug => ConfigManager.GetBool("debug");
        public static void AddLog(string message, ConsoleColor consoleColor = ConsoleColor.Cyan)
        {
            ServerConsole.AddLog(message, consoleColor);
        }

        public static void Info(string tag, string message)
        {
            AddLog($"[{tag}]: {message}", ConsoleColor.DarkCyan);
            if (FileLog.Enabled)
                FileLog.Info(tag, message);
        }

        public static void Warn(string tag, string message)
        {
            AddLog($"[{tag}]: {message}", ConsoleColor.DarkYellow);
            if (FileLog.Enabled)
                FileLog.Warn(tag, message);
        }

        public static void Error(string tag, string message)
        {
            AddLog($"[{tag}]: {message}", ConsoleColor.DarkRed);
            if (FileLog.Enabled)
                FileLog.Error(tag, message);
        }

        public static void Error(string tag, Exception exception)
        {
            Error(tag, exception.ToString());
            if (FileLog.Enabled)
                FileLog.Error(tag, exception.ToString());
        }

        public static void Debug(string key, string message)
        {
            if (!EnableDebug)
                return;
            AddLog($"[{key}]: {message}", ConsoleColor.Green);
            if (FileLog.Enabled)
                FileLog.Debug(key, message);
        }
    }
}

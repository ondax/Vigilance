using System;

namespace Vigilance
{
    public static class Log
    {
        public static void AddLog(string message, ConsoleColor consoleColor = ConsoleColor.Cyan) => ServerConsole.AddLog(message, consoleColor);
        public static void Info(string tag, string message) => AddLog($"[{tag}]: {message}", ConsoleColor.DarkCyan);
        public static void Warn(string tag, string message) => AddLog($"[{tag}]: {message}", ConsoleColor.DarkYellow);
        public static void Error(string tag, string message) => AddLog($"[{tag}]: {message}", ConsoleColor.DarkRed);
        public static void Error(string tag, Exception exception) => Error(tag, exception.ToString());
        public static void Debug(string key, string message) => AddLog($"[{key}]: {message}", ConsoleColor.Green);
    }
}

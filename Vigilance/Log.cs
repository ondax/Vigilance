using System;
using System.Reflection;
using Vigilance.Extensions;

namespace Vigilance
{
    public static class Log
    {
        public static void Add(string message, LogType type)
        {
            string tag = Assembly.GetExecutingAssembly().GetName().Name;
            Add($"[{type.ToString().ToUpper()}] [{tag}]: {message}", type.GetColor());
        }

        public static void Add(string tag, string message, LogType type)
        {
            Add($"[{type.ToString().ToUpper()}] [{tag}]: {message}", type.GetColor());
        }

        public static void Add(string tag, Exception e)
        {
            Add(tag, e.ToString(), LogType.Error);
        }

        public static void Add(Exception e)
        {
            string tag = Assembly.GetExecutingAssembly().GetName().Name;
            Add($"[ERROR] [{tag}]: {e}", ConsoleColor.DarkRed);
        }

        public static void Add(Assembly assembly, string message, LogType type)
        {
            Add(assembly.GetName().Name, message, type);
        }

        public static void Add(string log, ConsoleColor color = ConsoleColor.White)
        {
            ServerConsole.AddLog(log, color);
        }
    }

    public enum LogType
    {
        Info,
        Warn,
        Error,
        Debug
    }
}

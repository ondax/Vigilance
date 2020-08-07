﻿using System;
using System.Reflection;

namespace Vigilance
{
    public static class Log
    {
        public static void Add(string message, LogType type)
        {
            string tag = Assembly.GetExecutingAssembly().GetName().Name;
            Add($"[{type.ToString().ToUpper()}] [{tag}]: {message}", GetLogColor(type));
        }

        public static void Add(string tag, string message, LogType type)
        {
            Add($"[{type.ToString().ToUpper()}] [{tag}]: {message}", GetLogColor(type));
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

        public static void Add(string log, ConsoleColor color = ConsoleColor.White)
        {
            ServerConsole.AddLog(log, color);
        }

        public static ConsoleColor GetLogColor(LogType type)
        {
            if (type == LogType.Debug)
                return ConsoleColor.Cyan;
            if (type == LogType.Error)
                return ConsoleColor.DarkRed;
            if (type == LogType.Info)
                return ConsoleColor.Green;
            if (type == LogType.Warn)
                return ConsoleColor.DarkYellow;
            else
                return ConsoleColor.White;
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
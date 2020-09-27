using System.Collections.Generic;
using System;
using Vigilance.API;
using Vigilance.Extensions;
using Vigilance.Registered;
using System.Net;

namespace Vigilance
{
    public static class CommandManager
    {
        private static List<CommandHandler> _commands;
        private static List<GameCommandHandler> _gameCommands;
        private static List<ConsoleCommandHandler> _consoleCommands;
        public static List<CommandHandler> Commands => _commands;
        public static List<GameCommandHandler> GameCommands => _gameCommands;
        public static List<ConsoleCommandHandler> ConsoleCommands => _consoleCommands;

        public static void Enable()
        {
            _commands = new List<CommandHandler>();
            _gameCommands = new List<GameCommandHandler>();
            _consoleCommands = new List<ConsoleCommandHandler>();

            RegisterCommand(new CommandClean());
            RegisterCommand(new CommandDropAll());
            RegisterCommand(new CommandGiveAll());
            RegisterCommand(new CommandPlayers());
            RegisterCommand(new CommandTeleport());
            RegisterCommand(new CommandPersonalBroadcast());
            RegisterCommand(new CommandAdminChat());
            RegisterCommand(new CommandClearRagdolls());
            RegisterCommand(new CommandReload());
            RegisterCommand(new CommandRestart());
            RegisterCommand(new CommandGrenade());
            RegisterCommand(new CommandFlash());
            RegisterCommand(new CommandBall());
            RegisterCommand(new CommandRagdoll());
            RegisterCommand(new CommandDummy());
            RegisterCommand(new CommandWorkbench());
            RegisterCommand(new CommandRocket());
            RegisterCommand(new CommandScale());
            RegisterCommand(new CommandChangeUnit());
            RegisterCommand(new CommandDownloadPlugin());
            RegisterCommand(new CommandDownloadDependency());
            RegisterCommand(new CommandAddUnit());
            RegisterCommand(new CommandPos());
            RegisterCommand(new GhostCommand());
            RegisterCommand(new TargetGhostCommand());
            RegisterCommand(new CommandSpawnPrefab());
            RegisterCommand(new CommandExplode());
            RegisterCommand(new CommandDecontaminate());
            RegisterCommand(new CommandForceEnd());
            RegisterCommand(new CommandAddReservedSlot());

            RegisterGameCommand(new UnbanCommand());
            RegisterGameCommand(new OfflineBanCommand());
        }

        public static CommandHandler GetCommandHandler(string command)
        {
            foreach (CommandHandler handler in _commands)
            {
                if (handler.Command.ToUpper() == command.ToUpper())
                    return handler;
                else
                    if (!handler.Aliases.IsEmpty())
                    foreach (string alias in GetAliases(handler))
                        if (alias.ToUpper() == command.ToUpper())
                            return handler;
            }
            return null;
        }

        public static GameCommandHandler GetGameCommandHandler(string command)
        {
            foreach (GameCommandHandler handler in _gameCommands)
            {
                if (handler.Command.ToUpper() == command.ToUpper())
                    return handler;
                else
                    if (!handler.Aliases.IsEmpty())
                    foreach (string alias in GetAliases(handler))
                        if (alias.ToUpper() == command.ToUpper())
                            return handler;
            }
            return null;
        }

        public static ConsoleCommandHandler GetConsoleCommandHandler(string command)
        {
            foreach (ConsoleCommandHandler cch in _consoleCommands)
            {
                if (cch.Command.ToUpper() == command.ToUpper())
                    return cch;
                else
                    if (!cch.Aliases.IsEmpty())
                    foreach (string alias in GetAliases(cch))
                        if (alias.ToUpper() == command.ToUpper())
                            return cch;
            }
            return null;
        }

        public static string[] GetAliases(string str)
        {
            return str.Split(' ');
        }

        public static string[] GetAliases(CommandHandler command)
        {
            return GetAliases(command.Aliases);
        }

        public static string[] GetAliases(GameCommandHandler handler)
        {
            return GetAliases(handler.Aliases);
        }

        public static string[] GetAliases(ConsoleCommandHandler handler)
        {
            return GetAliases(handler.Aliases);
        }

        public static void RegisterCommand(CommandHandler handler) => _commands.Add(handler);
        public static void RegisterGameCommand(GameCommandHandler handler) => _gameCommands.Add(handler);
        public static void RegisterConsoleCommand(ConsoleCommandHandler handler) => _consoleCommands.Add(handler);
    }

    public interface CommandHandler
    {
        string Command { get; }
        string Usage { get; }
        string Aliases { get; }
        string Execute(Player sender, string[] args);
    }

    public interface GameCommandHandler
    {
        string Command { get; }
        string Usage { get; }
        string Aliases { get; }
        string Execute(Player sender, string[] args);
    }

    public interface ConsoleCommandHandler
    {
        string Command { get; }
        string Usage { get; }
        string Aliases { get; }
        string Execute(Player sender, string[] args, out string color);
    }
}

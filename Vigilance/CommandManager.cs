using System.Collections.Generic;
using System;
using Vigilance.API;
using Vigilance.Extensions;
using Vigilance.Registered;

namespace Vigilance
{
    public static class CommandManager
    {
        private static List<CommandHandler> _commands;
        private static List<GameCommandHandler> _gameCommands;
        public static List<CommandHandler> Commands => _commands;
        public static List<GameCommandHandler> GameCommands => _gameCommands;

        public static void Enable()
        {
            _commands = new List<CommandHandler>();
            _gameCommands = new List<GameCommandHandler>();

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
        }

        public static bool CallGameCommand(string query, CommandSender sender, out string reply)
        {
            try
            {
                string command = query.Split(' ')[0].ToUpper();
                GameCommandHandler handler = GetGameCommandHandler(command);
                reply = handler == null ? "SERVER#Unknown command!" : $"SERVER#{handler.Execute(sender.GetPlayer(), query.Split(' ').SkipCommand())}";
                return handler == null ? false : true;
            }
            catch (Exception e)
            {
                Log.Add("CommandManager", e);
                reply = $"An error occured!\nStacktrace:\n {e.StackTrace}";
                return false;
            }
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

        public static string CallCommand(string query, CommandSender sender)
        {
            try
            {
                string command = query.Split(' ')[0].ToUpper();
                CommandHandler handler = GetCommandHandler(command);
                return handler == null ? "SERVER#Unknown command!" : $"SERVER#{handler.Execute(sender.GetPlayer(), query.Split(' ').SkipCommand())}";
            }
            catch (Exception e)
            {
                Log.Add("CommandManager", e);
                return $"SERVER#An error occured!\nStacktrace: \n{e.StackTrace}";
            }
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

        public static void RegisterCommand(CommandHandler handler) => _commands.Add(handler);
        public static void RegisterGameCommand(GameCommandHandler handler) => _gameCommands.Add(handler);
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
}

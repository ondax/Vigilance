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
            RegisterCommand(new CommandReloadConfigs());
            RegisterCommand(new CommandRestart());
            RegisterCommand(new CommandGrenade());
            RegisterCommand(new CommandFlash());
            RegisterCommand(new CommandBall());
            RegisterCommand(new CommandRagdoll());
            RegisterCommand(new CommandDummy());
            RegisterCommand(new CommandWorkbench());
            RegisterCommand(new CommandRocket());
            RegisterCommand(new CommandScale());
        }

        public static bool CallGameCommand(string command, CommandSender sender, string query, out string reply)
        {
            try
            {
                GameCommandHandler handler = GetGameCommandHandler(command);
                reply = handler == null ? "SERVER#Unknown command" : $"SERVER#{handler.Execute(sender.GetPlayer(), query.Split(' ').SkipCommand())}";
                return handler == null ? false : true;
            }
            catch (Exception e)
            {
                Log.Add("CommandManager", e);
                reply = "ER-1";
                return false;
            }
        }

        public static CommandHandler GetCommandHandler(string command)
        {
            foreach (CommandHandler handler in _commands)
            {
                if (handler.Command.ToUpper() == command.ToUpper())
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
            }
            return null;
        }

        public static string CallCommand(string command, CommandSender sender, string query)
        {
            try
            {
                CommandHandler handler = GetCommandHandler(command);
                return handler == null ? "SERVER#Unknown command!" : $"SERVER#{handler.Execute(sender.GetPlayer(), query.Split(' ').SkipCommand())}";
            }
            catch (Exception e)
            {
                Log.Add("CommandManager", e);
                return "SERVER#An exception occured";
            }
        }

        public static void RegisterCommand(CommandHandler handler) => _commands.Add(handler);
        public static void RegisterGameCommand(GameCommandHandler handler) => _gameCommands.Add(handler);
    }

    public interface CommandHandler
    {
        string Command { get; }
        string Usage { get; }
        string Execute(Player sender, string[] args);
    }

    public interface GameCommandHandler : CommandHandler
    {

    }
}

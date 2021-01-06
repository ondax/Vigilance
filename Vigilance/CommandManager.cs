using System.Collections.Generic;
using Vigilance.API;
using Vigilance.Extensions;
using Vigilance.Registered;

namespace Vigilance
{
    public static class CommandManager
    {
        public static Dictionary<string, CommandHandler> Commands { get; set; }
        public static Dictionary<string, GameCommandHandler> GameCommands { get; set; }
        public static Dictionary<string, ConsoleCommandHandler> ConsoleCommands { get; set; }

        public static void Enable()
        {
            Commands = new Dictionary<string, CommandHandler>();
            GameCommands = new Dictionary<string, GameCommandHandler>();
            ConsoleCommands = new Dictionary<string, ConsoleCommandHandler>();

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
            RegisterCommand(new CommandGhost());
            RegisterCommand(new CommandTargetGhost());
            RegisterCommand(new CommandSpawnPrefab());
            RegisterCommand(new CommandExplode());
            RegisterCommand(new CommandDecontaminate());
            RegisterCommand(new CommandForceEnd());
            RegisterCommand(new CommandAddReservedSlot());
            RegisterCommand(new CommandListCommands());
            RegisterCommand(new CommandHelpCommand());
            RegisterCommand(new CommandHint());
            RegisterCommand(new CommandPersonalHint());
            RegisterCommand(new CommandAchieve());
            RegisterCommand(new CommandItemSize());
            RegisterCommand(new CommandList());
            RegisterCommand(new CommandTpRoom());

            RegisterGameCommand(new CommandUnban());
            RegisterGameCommand(new CommandOban());
        }

        public static void UnregisterCommand(string command)
        {
            if (string.IsNullOrEmpty(command))
                return;
            command = command.ToUpper();
            CommandHandler commandHandler = GetCommandHandler(command);
            GameCommandHandler gameCommandHandler = GetGameCommandHandler(command);
            ConsoleCommandHandler consoleCommandHandler = GetConsoleCommandHandler(command);
            if (commandHandler != null)
                Commands.Remove(command);
            if (gameCommandHandler != null)
                GameCommands.Remove(command);
            if (consoleCommandHandler != null)
                ConsoleCommands.Remove(command);
        }

        public static CommandHandler GetCommandHandler(string command)
        {
            foreach (CommandHandler handler in Commands.Values)
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
            foreach (GameCommandHandler handler in GameCommands.Values)
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
            foreach (ConsoleCommandHandler cch in ConsoleCommands.Values)
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

        public static bool CallCommand(Player sender, string[] query, out string reply)
        {
            CommandHandler handler = GetCommandHandler(query[0].ToUpper());
            GameCommandHandler gch = GetGameCommandHandler(query[0].ToUpper());

            if (handler != null)
            {
                reply = $"{query[0].ToUpper()}#{handler.Execute(sender, query.SkipCommand())}";
                return true;
            }

            if (gch != null)
            {
                reply = $"{query[0].ToUpper()}#{gch.Execute(sender, query.SkipCommand())}";
                return true;
            }

            if (HandlerExists(query[0].ToUpper()))
            {
                reply = "SERVER#An error occured while executing this command.";
                return true;
            }

            reply = "SERVER#Unknown command!";
            return false;
        }

        public static bool HandlerExists(string command)
        {
            foreach (CommandHandler handler in Commands.Values)
            {
                if (handler.Command.ToUpper() == command.ToUpper())
                    return true;
                else
                    if (!handler.Aliases.IsEmpty())
                    foreach (string alias in handler.Aliases.Split(' '))
                        if (alias.ToUpper() == command.ToUpper())
                            return true;
            }

            foreach (GameCommandHandler gch in GameCommands.Values)
            {
                if (gch.Command.ToUpper() == command.ToUpper())
                    return true;
                else
                    if (!gch.Aliases.IsEmpty())
                    foreach (string alias in GetAliases(gch))
                        if (alias.ToUpper() == command.ToUpper())
                            return true;
            }
            return false;
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

        public static void RegisterCommand(CommandHandler handler)
        {
            string s = handler.Command.ToUpper();
            if (!Commands.ContainsKey(s))
                Commands.Add(s, handler);
        }

        public static void RegisterGameCommand(GameCommandHandler handler)
        {
            string s = handler.Command.ToUpper();
            if (!GameCommands.ContainsKey(s))
                GameCommands.Add(s, handler);
        }

        public static void RegisterConsoleCommand(ConsoleCommandHandler handler)
        {
            string s = handler.Command.ToUpper();
            if (!ConsoleCommands.ContainsKey(s))
                ConsoleCommands.Add(s, handler);
        }
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

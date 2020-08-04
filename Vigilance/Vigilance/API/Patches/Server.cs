using Harmony;
using Vigilance.Events;
using Vigilance.Handlers;
using System;
using RemoteAdmin;
using Vigilance.API.Extensions;

namespace Vigilance.API.Patches
{
    [HarmonyPatch(typeof(PlayerStats), nameof(PlayerStats.Roundrestart))]
    public static class RoundRestartEventPatch
    {
        private static void Prefix()
        {
            try
            {
                EventController.StartEvent<RoundRestartEventHandler>(new RoundRestartEvent());
                Data.Sitrep.Post(Data.Sitrep.Translation.RoundRestart(), Enums.PostType.Sitrep);
            }
            catch (Exception e)
            {
                Log.Error("PlayerStats", e);
            }
        }
    }

    [HarmonyPatch(typeof(CommandProcessor), nameof(CommandProcessor.ProcessQuery))]
    public static class CommandProcessorPatch
    {
        public static void Postfix(string q, CommandSender sender)
        {
            try
            {
                Player admin = sender.GetPlayer();
                FileLog.RemoteAdminLog(q, admin);
                RACommandEvent ev = new RACommandEvent(admin, sender, q, true);
                Data.Sitrep.Post(Data.Sitrep.Translation.Command(ev), Enums.PostType.RemoteAdmin);
            }
            catch (Exception e)
            {
                Log.Error("CommandProcessor", e);
            }
        }
    }

    [HarmonyPatch(typeof(CharacterClassManager), nameof(CharacterClassManager.CmdStartRound))]
    public static class RoundStartEventPatch
    {
        private static void Postfix()
        {
            try
            {
                EventController.StartEvent<RoundStartEventHandler>(new RoundStartEvent());
                Data.RoundCounter.AddRound();
                Data.Cleanup.Start();
                Data.Sitrep.Post(Data.Sitrep.Translation.RoundStart(), Enums.PostType.Sitrep);
            }
            catch (Exception e)
            {
                Log.Error("CharacterClassManager", e);
            }
        }
    }

    [HarmonyPatch(typeof(ServerConsole), nameof(ServerConsole.AddLog))]
    internal static class RoundEventPatches
    {
        private static void Prefix(string q)
        {
            try
            {
                FileLog.ConsoleLog(q);
                if (q == "Waiting for players...")
                {
                    EventController.StartEvent<WaitingForPlayersEventHandler>(new WaitingForPlayersEvent());
                    Data.Sitrep.Post(Data.Sitrep.Translation.WaitingForPlayers(), Enums.PostType.Sitrep);
                }
                if (q.StartsWith("Round finished! Anomalies:"))
                {
                    EventController.StartEvent<RoundEndEventHandler>(new RoundEndEvent());
                    Data.Sitrep.Post(Data.Sitrep.Translation.RoundEnd(), Enums.PostType.Sitrep);
                    Data.RoundCounter.Restart();
                }
            }
            catch (Exception e)
            {
                Log.Error("ServerConsole", e);
            }
        }
    }
}

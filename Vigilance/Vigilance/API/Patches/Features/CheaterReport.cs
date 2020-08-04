using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Vigilance.API.Extensions;
using Vigilance.Events;
using Vigilance.Handlers;
using Harmony;
using System;

namespace Vigilance.API.Patches
{
    [HarmonyPatch(typeof(CheaterReport), nameof(CheaterReport.CallCmdReport))]
    public static class CheaterReportPatch
    {
        public static bool Prefix(CheaterReport __instance, int playerId, string reason, byte[] signature, bool notifyGm)
        {
			try
			{
				if (!__instance._commandRateLimit.CanExecute(true))
				{
					return false;
				}

				if (string.IsNullOrEmpty(reason))
				{
					return false;
				}

				Player reporter = __instance.gameObject.GetPlayer();
				Player reported = playerId.GetPlayer();
				float num = Time.time - __instance._lastReport;

				if (num < 2f)
				{
					__instance.GetComponent<GameConsoleTransmission>().SendToClient(reporter.Connection, "[REPORTING] Reporting rate limit exceeded (1).", "red");
					return false;
				}

				if (num > 60f)
				{
					__instance._reportedPlayersAmount = 0;
				}

				if (__instance._reportedPlayersAmount > 5)
				{
					__instance.GetComponent<GameConsoleTransmission>().SendToClient(reporter.Connection, "[REPORTING] Reporting rate limit exceeded (2).", "red");
					return false;
				}

				if (notifyGm && (!ServerStatic.GetPermissionsHandler().IsVerified || string.IsNullOrEmpty(ServerConsole.Password)))
				{
					__instance.GetComponent<GameConsoleTransmission>().SendToClient(reporter.Connection, "[REPORTING] Server is not verified - you can't use report feature on this server.", "red");
					return false;
				}

				if (!ReferenceHub.TryGetHub(playerId, out ReferenceHub referenceHub))
				{
					__instance.GetComponent<GameConsoleTransmission>().SendToClient(__instance.connectionToClient, "[REPORTING] Can't find player with that PlayerID.", "red");
					return false;
				}

				if (reporter.Compare(reported))
				{
					reporter.ConsoleMessage("[REPORTING] You can't report yourself!", "red");
					return false;
				}

				ReferenceHub hub = ReferenceHub.GetHub(__instance.gameObject);
				CharacterClassManager reportedCcm = referenceHub.characterClassManager;
				CharacterClassManager reporterCcm = hub.characterClassManager;

				if (!ReferenceHub.TryGetHub(playerId, out referenceHub))
				{
					__instance.GetComponent<GameConsoleTransmission>().SendToClient(__instance.connectionToClient, "[REPORTING] Can't find player with that PlayerID.", "red");
					return false;
				}

				if (__instance._reportedPlayers == null)
				{
					__instance._reportedPlayers = new HashSet<int>();
				}

				if (__instance._reportedPlayers.Contains(playerId))
				{
					__instance.GetComponent<GameConsoleTransmission>().SendToClient(__instance.connectionToClient, "[REPORTING] You have already reported that player.", "red");
					return false;
				}

				if (string.IsNullOrEmpty(reportedCcm.UserId))
				{
					__instance.GetComponent<GameConsoleTransmission>().SendToClient(__instance.connectionToClient, "[REPORTING] Failed: User ID of reported player is null.", "red");
					return false;
				}

				if (string.IsNullOrEmpty(reporterCcm.UserId))
				{
					__instance.GetComponent<GameConsoleTransmission>().SendToClient(__instance.connectionToClient, "[REPORTING] Failed: your User ID of is null.", "red");
					return false;
				}

				string reporterNickname = hub.nicknameSync.MyNick;
				string reportedNickname = referenceHub.nicknameSync.MyNick;

				if (!notifyGm)
				{
					GameCore.Console.AddLog(string.Concat(new string[]
					{
				"Player ",
				hub.LoggedNameFromRefHub(),
				" reported player ",
				referenceHub.LoggedNameFromRefHub(),
				" with reason ",
				reason,
				"."
					}), Color.gray, false);
					__instance.GetComponent<GameConsoleTransmission>().SendToClient(__instance.connectionToClient, "[REPORTING] Player report successfully sent to local administrators.", "green");
					CheaterReportEvent ev = new CheaterReportEvent(reporter, reported, reason, true);
					EventController.StartEvent<CheaterReportEventHandler>(ev);
					Data.Sitrep.Post(Data.Sitrep.Translation.Report(ev), Enums.PostType.Report);
					if (CheaterReport.SendReportsByWebhooks)
					{
						GameConsoleTransmission gct = __instance.GetComponent<GameConsoleTransmission>();
						new Thread(delegate ()
						{
							__instance.LogReport(gct, reporterCcm.UserId, reportedCcm.UserId, ref reason, playerId, false, reporterNickname, reportedNickname);
						})
						{
							Priority = System.Threading.ThreadPriority.Lowest,
							IsBackground = true,
							Name = "Reporting player (locally) - " + reportedCcm.UserId + " by " + reporterCcm.UserId
						}.Start();
					}
					return false;
				}
				return true;
			}
			catch (Exception e)
			{
				Log.Error("CheaterReport", e);
				return true;
			}
		}
	}
}

using System;
using Harmony;
using RemoteAdmin;
using Vigilance.Extensions;
using Vigilance.API;

namespace Vigilance.Patches.CommandProccessing
{
	[HarmonyPatch(typeof(CommandProcessor), nameof(CommandProcessor.ProcessQuery))]
	public static class CommandProcessor_ProcessQuery
	{
		public static bool Prefix(string q, CommandSender sender)
		{
			try
			{
				string[] query = q.Split(' ');
				Player admin = sender.GetPlayer();
				if (admin == null)
					return true;
				Environment.OnRemoteAdminCommand(sender, q, true, out bool allow, out string reply);
				if (!allow)
				{
					sender.RaReply($"SERVER#{reply}", true, true, "");
					return false;
				}

				if (!ConfigManager.EnableGameCommands || ConfigManager.GameCommandsBlacklist.Contains(query[0].ToLower()) || ConfigManager.GameCommandsBlacklist.Contains(admin.UserId))
				{
					sender.RaReply($"SERVER#You are not allowed to use this command ({(ConfigManager.EnableGameCommands ? "Blacklisted command or UserID" : "Command disabled in config")})!", false, true, "");
					return false;
				}

				if (ConfigManager.IsBlacklisted(sender, query))
				{
					sender.RaReply($"SERVER#You are not allowed to use this command!", false, true, "");
					return false;
				}

				if (CommandManager.CallCommand(admin, query, out string response))
                {
					sender.RaReply($"{response}", true, true, "");
					return false;
                }

				return true;
			}
			catch (Exception e)
            {
				Log.Add(nameof(CommandProcessor.ProcessQuery), e, LogType.Error);
				sender.RaReply($"SERVER#An error occured while processing this command!\nError: {e.Message}", false, true, "");
				return false;
            }
		}
	}
}

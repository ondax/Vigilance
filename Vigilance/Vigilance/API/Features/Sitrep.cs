using MEC;
using System;
using System.Collections.Generic;
using Vigilance.API.Discord;
using Vigilance.API.Enums;
using Vigilance.API.Extensions;
using Vigilance.Events;
using System.Linq;

namespace Vigilance.API.Features
{
    public class Sitrep
    {
        private string _sitrepUrl;
        private string _reportUrl;
        private string _banUrl;
        private string _raLogUrl;
        private Translations _translations;

        private Webhook _sitrepWebhook;
        private Webhook _reportWebhook;
        private Webhook _banWebhook;
        private Webhook _raLogWebhook;

        public Translations Translation => _translations;

        public Sitrep()
        {
        }

        public void Start()
        {
            _sitrepUrl = string.IsNullOrEmpty(ConfigManager.GetString("sitrep_url")) ? "none" : ConfigManager.GetString("sitrep_url");
            _reportUrl = string.IsNullOrEmpty(ConfigManager.GetString("report_url")) ? "none" : ConfigManager.GetString("report_url");
            _banUrl = string.IsNullOrEmpty(ConfigManager.GetString("ban_url")) ? "none" : ConfigManager.GetString("ban_url");
            _raLogUrl = string.IsNullOrEmpty(ConfigManager.GetString("remote_admin_log_url")) ? "none" : ConfigManager.GetString("remote_admin_log_url");
            _translations = new Translations();

            if (_sitrepUrl != "none")
                _sitrepWebhook = new Webhook(_sitrepUrl);
            if (_reportUrl != "none")
                _reportWebhook = new Webhook(_reportUrl);
            if (_banUrl != "none")
                _banWebhook = new Webhook(_banUrl);
            if (_raLogUrl != "none")
                _raLogWebhook = new Webhook(_raLogUrl);
        }

        public void Post(string msg, PostType post)
        {
            if (string.IsNullOrEmpty(msg) || msg.ToLower() == "none")
                return;
            if (post == PostType.Sitrep && _sitrepWebhook != null)
                Timing.RunCoroutine(PostSafely(msg));
            if (post == PostType.Ban && _banWebhook != null)
                _banWebhook.Post(msg);
            if (post == PostType.Report && _reportWebhook != null)
                _reportWebhook.Post(msg);
            if (post == PostType.RemoteAdmin && _raLogWebhook != null)
            {
                _raLogWebhook.Post(msg);
            }
        }

       
        private IEnumerator<float> PostSafely(string message)
        {
            yield return Timing.WaitForSeconds(2f);
            _sitrepWebhook.Post($"[{DateTime.Now.ToString("HH:mm:ss")}] {message}");
        }

        public class Translations
        {
            public Translations()
            { }

            public string PlayerDeathEvent(Player killer, Player target, PlayerStats.HitInfo hitInfo)
            {
                RoleType role = target.Role;
                if (role == RoleType.Spectator)
                {
                    Role[] newArray = target.ClassManager.Classes.Where(h => h.roleId != RoleType.Spectator).ToArray();
                    role = newArray[newArray.Count()].roleId;
                }
                return ConfigManager.GetString("sitrep_death_message").Replace("%killerNick%", killer.Nick.DiscordSanitize()).Replace("%killerUserId%", killer.UserId).Replace("%killerRole%", killer.Role.ToString()).Replace("%killerToken%", killer.Token).Replace("%playerNick%", target.Nick.DiscordSanitize()).Replace("%playerUserId%", target.UserId).Replace("%playerRole%", role.ToString()).Replace("%playerToken%", target.Token).Replace("%damageAmount%", hitInfo.Amount.ToString()).Replace("%damagetype%", hitInfo.GetDamageType().Convert().ToString());
            }

            public string LureEvent(Player player)
            {
                return ConfigManager.GetString("sitrep_lure_message").Replace("%nick%", player.Nick.DiscordSanitize()).Replace("%userid%", player.UserId).Replace("%role%", player.Role.ToString()).Replace("%token%", player.Token);
            }

            public string JoinEvent(Player player)
            {
                return ConfigManager.GetString("sitrep_join_message").Replace("%nick%", player.Nick.DiscordSanitize()).Replace("%userid%", player.UserId).Replace("%token%", player.Token);
            }

            public string LeaveEvent(Player player)
            {
                return ConfigManager.GetString("sitrep_leave_message").Replace("%nick%", player.Nick.DiscordSanitize()).Replace("%userid%", player.UserId).Replace("%token%", player.Token);
            }

            public string SpawnEvent(Player player)
            {
                return ConfigManager.GetString("sitrep_spawn_message").Replace("%nick%", player.Nick.DiscordSanitize()).Replace("%userid%", player.UserId).Replace("%token%", player.Token).Replace("%role%", player.Role.ToString());
            }

            public string Ban(BanEvent ev)
            {
                return ConfigManager.GetString("sitrep_ban_message").Replace("%bannedNick%", ev.Player.Nick.DiscordSanitize()).Replace("%bannedUserId%", ev.Player.UserId).Replace("%bannedToken%", ev.Player.Token).Replace("%reason%", ev.Reason).Replace("%adminNick%", ev.Admin.Nick.DiscordSanitize()).Replace("%adminUserId%", ev.Admin.Role.ToString()).Replace("%adminToken%", ev.Admin.Token).Replace("%duration%", ev.Duration.GetDurationString());
            }

            public string Report(CheaterReportEvent ev)
            {
                return ConfigManager.GetString("sitrep_report_message").Replace("%reportedNick%", ev.Reported.Nick.DiscordSanitize()).Replace("%reportedUserId%", ev.Reported.UserId).Replace("%reportedToken%", ev.Reported.Token).Replace("%reporterNick%", ev.Reporter.Nick.DiscordSanitize()).Replace("%reporterUserId%", ev.Reporter.UserId).Replace("%reporterToken%", ev.Reporter.Token).Replace("%reportedRole%", ev.Reported.Role.ToString()).Replace("%reporterRole%", ev.Reporter.Role.ToString()).Replace("%reason%", ev.Reason);
            }

            public string Command(RACommandEvent ev)
            {
                if (ev.Command == "REQUEST_DATA")
                    return string.Empty;
                return ConfigManager.GetString("sitrep_remote_admin_message").Replace("%issuerNick%", ev.Admin.Nick.DiscordSanitize()).Replace("%issuerUserId%", ev.Admin.UserId).Replace("%issuerGroup%", ev.Admin.UserGroup.BadgeText).Replace("%issuerToken%", ev.Admin.Token).Replace("%command%", ev.Query);
            }

            public string RoundEnd() => ConfigManager.GetString("sitrep_round_end_message");
            public string RoundRestart() => ConfigManager.GetString("sitrep_round_restart_message");
            public string RoundStart() => ConfigManager.GetString("sitrep_round_start_message");
            public string WaitingForPlayers() => ConfigManager.GetString("sitrep_waiting_for_players_message");
            public string Decontaminate() => ConfigManager.GetString("sitrep_lcz_decontaminate_message");
            public string TeamRespawn(TeamRespawnEvent ev) => ConfigManager.GetString("sitrep_team_respawn_message").Replace("%isChaos%", ev.IsChaos.ToString());
            public string WarheadDetonate() => ConfigManager.GetString("sitrep_warhead_detonate_message");
            public string WarheadStart(WarheadStartEvent ev) => ConfigManager.GetString("sitrep_warhead_start_message").Replace("%activatorNick%", ev.User.Nick.DiscordSanitize()).Replace("%activatorUserId%", ev.User.UserId);
            public string WarheadStop(WarheadStopEvent ev) => ConfigManager.GetString("sitrep_warhead_stop_message").Replace("%playerNick%", ev.User.Nick.DiscordSanitize()).Replace("%playerUserId%", ev.User.UserId);
        }
    }
}

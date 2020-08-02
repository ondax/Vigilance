using System.Collections.Generic;

namespace Vigilance.API.Features
{
    public class NicknameFilter
    {
        public static List<string> Filters => ConfigManager.GetStringList("nickname_filter_names");
        public static bool Enabled => ConfigManager.GetBool("enable_nickname_filter");
        public static string Reason => string.IsNullOrEmpty(ConfigManager.GetString("nickname_filter_reason")) ? "You were kicked. Reason: Blacklisted name." : ConfigManager.GetString("nickname_filter_reason");

        public void CheckNickname(Player player)
        {
            if (!Enabled)
                return;
            foreach (string filter in Filters)
            {
                if (player.Nick.ToLower().Contains(filter.ToLower()))
                {
                    Kick(player);
                }
            }
        }

        private void Kick(Player player) => player.Kick(Reason);
    }
}

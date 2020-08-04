using MEC;
using System.Collections.Generic;
using UnityEngine;

namespace Vigilance.API.Features
{
    public class ScpHealing
    {
        public static bool Enabled => ConfigManager.GetBool("enable_scp_healing");
        public static int WaitTime => ConfigManager.GetInt("scp_healing_wait_time");
        public static int Health => ConfigManager.GetInt("scp_healing_health");

        public ScpHealing()
        {

        }

        public void StartHealing(Player player)
        {
            if (player.Team == Enums.TeamType.SCP)
            {
                Timing.RunCoroutine(Heal(player));
            }
        }

        private IEnumerator<float> Heal(Player player)
        {
            for (; ; )
            {
                Vector3 pos = player.Position;
                yield return Timing.WaitForSeconds(WaitTime);
                Vector3 newPos = player.Position;
                if (player.Team == Enums.TeamType.SCP)
                {
                    if (Vector3.Distance(pos, newPos) <= 5f)
                    {
                        if (player.Team == Enums.TeamType.SCP)
                        {
                            Log.Debug("SCPHealing", $"Adding {Health} to {player.Nick} ({player.Role})");
                            player.Health += Health;
                            if (player.Health >= player.MaxHealth)
                            {
                                player.Health = player.MaxHealth;
                            }
                        }
                    }
                }
            }
        }
    }
}

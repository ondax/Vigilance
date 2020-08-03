using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Vigilance.API.Features
{
    public class ScpHealing
    {
        public static bool Enabled => ConfigManager.GetBool("enable_scp_healing");
        public static int WaitTime => ConfigManager.GetInt("scp_healing_wait_time");
        public static int Health => ConfigManager.GetInt("scp_healing_health");

        private List<string> _players;

        public ScpHealing()
        {

        }

        public void Start()
        {
            _players = new List<string>();
        }

        public void StartHealing(Player player)
        {
            if (player.Team == Enums.TeamType.SCP)
            {
                _players.Add(player.UserId);
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
                if (player.Team != Enums.TeamType.SCP)
                {
                    _players.Remove(player.UserId);
                }
                if (_players.Contains(player.UserId))
                {
                    if (Vector3.Distance(pos, newPos) <= 5f)
                    {
                        if (player.Team == Enums.TeamType.SCP)
                        {
                            player.Health += Health;
                            if (player.Health >= player.MaxHealth)
                            {
                                player.Health = player.MaxHealth;
                            }
                        }
                    }
                }
                if (_players.Where(h => h == player.UserId).Count() > 1)
                {
                    foreach (string id in _players)
                    {
                        if (id == player.UserId)
                        {
                            _players.Remove(id);
                        }
                    }
                    _players.Add(player.UserId);
                }
            }
        }
    }
}

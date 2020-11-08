using System.Collections.Generic;
using UnityEngine;

namespace Vigilance.API
{
    public static class Ghostmode
    {
        public static readonly Vector3 GhostPosition = Vector3.up * 6000f;
        public static Dictionary<string, List<string>> Targets = new Dictionary<string, List<string>>();
        public static List<string> Ghosts = new List<string>();
        public static List<string> CannotTriggerScp096 = new List<string>();
        public static List<string> CannotBlockScp173 = new List<string>();

        public static void MakeGhost(Player player)
        {
            if (!Ghosts.Contains(player.UserId))
                Ghosts.Add(player.UserId);
        }

        public static void RemoveGhost(Player player)
        {
            if (Ghosts.Contains(player.UserId))
                Ghosts.Remove(player.UserId);
        }

        public static List<string> GetTargets(Player player)
        {
            if (!Targets.ContainsKey(player.UserId))
                Targets.Add(player.UserId, new List<string>());
            return Targets[player.UserId];
        }

        public static void AddTarget(Player player, Player target)
        {
            if (!Targets.ContainsKey(player.UserId))
                Targets.Add(player.UserId, new List<string>());
            Targets[player.UserId].Add(target.UserId);
        }

        public static void RemoveTarget(Player player, Player target)
        {
            if (!Targets.ContainsKey(player.UserId))
                Targets.Add(player.UserId, new List<string>());
            Targets[player.UserId].Remove(target.UserId);
        }

        public static void RemoveAllTargets(Player player)
        {
            if (!Targets.ContainsKey(player.UserId))
                Targets.Add(player.UserId, new List<string>());
            Targets.Remove(player.UserId);
        }

        public static void ClearAll()
        {
            Targets.Clear();
            Ghosts.Clear();
            CannotTriggerScp096.Clear();
            CannotBlockScp173.Clear();
        }

        public static Vector3 FindLookRotation(Vector3 player, Vector3 target) => (target - player).normalized;
        public static bool PlayerCannotSee(Player source, int playerId)
        {
            Player myPlayer = Server.PlayerList.GetPlayer(playerId);
            if (myPlayer == null)
                return false;
            return GetTargets(source).Contains(myPlayer.UserId);
        }

        public static Player GetPlayerOrServer(GameObject gameObject)
        {
            if (gameObject == null)
                return null;
            var refHub = ReferenceHub.GetHub(gameObject);
            return refHub.isLocalPlayer ? Server.PlayerList.Local : Server.PlayerList.GetPlayer(gameObject);
        }

        public static void RotatePlayer(int index, PlayerPositionData[] buff, Vector3 rotation) => buff[index] = new PlayerPositionData(buff[index].position, Quaternion.LookRotation(rotation).eulerAngles.y, buff[index].playerID);
        public static void MakeGhost(int index, PlayerPositionData[] buff) => buff[index] = new PlayerPositionData(GhostPosition, buff[index].rotation, buff[index].playerID);
    }
}

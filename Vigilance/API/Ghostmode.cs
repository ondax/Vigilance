using System.Collections.Generic;
using UnityEngine;

namespace Vigilance.API
{
    public static class Ghostmode
    {
        public static readonly Vector3 GhostPosition = Vector3.up * 6000f;
        public static Dictionary<Player, List<Player>> Targets = new Dictionary<Player, List<Player>>();
        public static List<Player> Ghosts = new List<Player>();
        public static List<Player> CannotTriggerScp096 = new List<Player>();
        public static List<Player> CannotBlockScp173 = new List<Player>();

        public static void MakeGhost(Player player)
        {
            if (!Ghosts.Contains(player))
                Ghosts.Add(player);
        }

        public static void RemoveGhost(Player player)
        {
            if (Ghosts.Contains(player))
                Ghosts.Remove(player);
        }

        public static List<Player> GetTargets(Player player)
        {
            if (!Targets.ContainsKey(player))
                Targets.Add(player, new List<Player>());
            return Targets[player];
        }

        public static void AddTarget(Player player, Player target)
        {
            if (!Targets.ContainsKey(player))
                Targets.Add(player, new List<Player>());
            Targets[player].Add(target);
        }

        public static void RemoveTarget(Player player, Player target)
        {
            if (!Targets.ContainsKey(player))
                Targets.Add(player, new List<Player>());
            Targets[player].Remove(target);
        }

        public static void RemoveAllTargets(Player player)
        {
            if (!Targets.ContainsKey(player))
                Targets.Add(player, new List<Player>());
            Targets.Remove(player);
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
            return GetTargets(source).Contains(myPlayer);
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

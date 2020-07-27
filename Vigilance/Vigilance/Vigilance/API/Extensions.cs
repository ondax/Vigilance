using Mirror;
using UnityEngine;
using Vigilance.API.Enums;
using System.Collections.Generic;
using System;

namespace Vigilance.API.Extensions
{
    public static class GameObjectExtensions
    {
        public static bool IsNull(this GameObject gameObject) => gameObject == null;
        public static void Destroy(this GameObject gameObject)
        {
            if (!gameObject.IsNull())
            {
                NetworkServer.Destroy(gameObject);
            }
        }
    }

    public static class HitInfoExtensions
    {
		public static Player GetAttacker(this PlayerStats.HitInfo hitInfo)
		{
			return hitInfo.RHub.GetPlayer();
		}

		public static Player GetPlayer(this PlayerStats.HitInfo hitInfo)
		{
			return hitInfo.GetPlayerObject().GetPlayer();
		}
	}

	public static class RagdollExtensions
    {
		public static Player GetOwner(this Ragdoll ragdoll) => ragdoll.Networkowner.PlayerId.GetPlayer();
		public static void Delete(this Ragdoll ragdoll) => ragdoll.gameObject.Destroy();
    }

    public static class PlayerExtensions
    {
		public static void SendRemoteAdminMessage(this CommandSender sender, string message, string command)
		{
			sender.RaReply(command.ToUpper() + "#" + message, true, true, string.Empty);
		}

		public static void SendRemoteAdminMessage(this CommandSender sender, string message)
		{
			sender.RaReply("SERVER#" + message, true, true, string.Empty);
		}

		public static void SendRemoteAdminMessage(this Player player, string message)
        {
			player.QueryProcessor._sender.RaReply($"SERVER#{message}", true, true, string.Empty);
        }

		public static Player GetPlayer(this GameObject gameObject)
		{
			return new Player(gameObject);
		}

		public static Player GetPlayer(this ReferenceHub hub)
		{
			return new Player(hub.gameObject);
		}

		public static Player GetPlayer(this CharacterClassManager ccm)
		{
			return new Player(ccm.gameObject);
		}

		public static Player GetPlayer(this PlayerStats stats)
		{
			return new Player(stats.gameObject);
		}

		public static Player GetPlayer(this PlayerInteract interact)
		{
			return interact.gameObject.GetPlayer();
		}

		public static Player GetPlayer(this Inventory inventory)
		{
			return inventory.gameObject.GetPlayer();
		}

		public static Player GetPlayer(this PlayableScpsController controller)
		{
			return controller.gameObject.GetPlayer();
		}

		public static Player GetPlayer(this WeaponManager manager)
		{
			return manager.gameObject.GetPlayer();
		}

		public static Player GetPlayer(this CommandSender sender)
		{
			return sender.SenderId.GetPlayer();
		}

		public static List<Player> GetPlayers(this RoleType role)
		{
			List<Player> players = new List<Player>();
			foreach (Player player in Server.Players)
            {
				if (player.Role == role)
                {
					players.Add(player);
                }
            }
			return players;
		}

		public static List<Player> GetPlayers(this TeamType team)
		{
			List<Player> players = new List<Player>();
			foreach (Player player in Server.Players)
            {
				if (player.Team == team)
                {
					players.Add(player);
                }
            }
			return players;
		}

		public static List<Player> GetPlayers(this List<GameObject> gameObjects)
		{
			List<Player> list = new List<Player>(gameObjects.Count);
			foreach (GameObject @object in gameObjects)
			{
				list.Add(new Player(@object));
			}
			return list;
		}

		public static List<Player> GetPlayers(this List<ReferenceHub> hubs)
		{
			List<Player> list = new List<Player>(hubs.Count);
			foreach (ReferenceHub referenceHub in hubs)
			{
				list.Add(new Player(referenceHub.gameObject));
			}
			return list;
		}

		public static Player GetPlayer(this int playerId)
        {
			foreach (Player player in Server.Players)
            {
				if (player.Id == playerId)
					return player;
            }
			return null;
        }

		public static Player GetPlayer(this string args)
        {
			try
			{
				Player playerFound = null;
				Dictionary<string, Player> userIds = new Dictionary<string, Player>();

				foreach (Player player in Server.Players)
                {
					userIds.Add(player.UserId, player);
                }

				foreach (string userId in userIds.Keys)
                {
					if (userId == args)
						return userIds[userId];
                }

				if (int.TryParse(args, out int id))
                {
					foreach (Player player in Server.Players)
                    {
						if (player.Id == id)
							return player;
                    }
                }

				if (args.EndsWith("@steam") || args.EndsWith("@discord") || args.EndsWith("@northwood") || args.EndsWith("@patreon"))
				{
					foreach (Player player in Server.Players)
					{
						if (player.UserId == args)
						{
							playerFound = player;
						}
					}
				}
				else
				{
					if (args == "WORLD" || args == "SCP-018" || args == "SCP-575" || args == "SCP-207")
						return null;
					int maxNameLength = 31, lastnameDifference = 31;
					string firstString = args.ToLower();
					foreach (Player player in Server.Players)
					{
						if (!player.Nick.ToLower().Contains(args.ToLower()))
							continue;
						if (firstString.Length < maxNameLength)
						{
							int x = maxNameLength - firstString.Length;
							int y = maxNameLength - player.Nick.Length;
							string secondString = player.Nick;
							for (int i = 0; i < x; i++)
								firstString += "z";
							for (int i = 0; i < y; i++)
								secondString += "z";
							int nameDifference = firstString.GetDistance(secondString);
							if (nameDifference < lastnameDifference)
							{
								lastnameDifference = nameDifference;
								playerFound = player;
							}
						}
					}
				}
				return playerFound;
			}
			catch (Exception exception)
			{
				Log.Error("Extensions", exception);
				return null;
			}
		}
	}

	public static class StringExtensions
    {
		public static int GetDistance(this string firstString, string secondString)
		{
			int n = firstString.Length;
			int m = secondString.Length;
			int[,] d = new int[n + 1, m + 1];
			if (n == 0)
				return m;
			if (m == 0)
				return n;
			for (int i = 0; i <= n; d[i, 0] = i++)
			{
			}
			for (int j = 0; j <= m; d[0, j] = j++)
			{
			}
			for (int i = 1; i <= n; i++)
			{
				for (int j = 1; j <= m; j++)
				{
					int cost = (secondString[j - 1] == firstString[i - 1]) ? 0 : 1;

					d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
				}
			}
			return d[n, m];
		}
	}
}

using UnityEngine;
using Mirror;
using Hints;
using System.Collections.Generic;
using MEC;
using Vigilance.Extensions;
using Vigilance.Enums;

namespace Vigilance.API
{
    public class Player
    {
        private ReferenceHub _hub;
        public Player(ReferenceHub hub) => _hub = hub;

        public GameObject GameObject => _hub.gameObject;
        public ReferenceHub Hub => _hub;

        public bool BypassMode => _hub.serverRoles.BypassMode;
        public bool DoNotTrack => _hub.serverRoles.DoNotTrack;
        public bool GodMode { get => _hub.characterClassManager.GodMode; set => _hub.characterClassManager.GodMode = value; }
        public int Health { get => (int)_hub.playerStats.Health; set => _hub.playerStats.SetHPAmount(value); }
        public int PlayerId { get => _hub.queryProcessor.PlayerId; set => _hub.queryProcessor.NetworkPlayerId = value; }
        public string IpAddress => _hub.queryProcessor._ipAddress;
        public bool IsInOverwatch { get => _hub.serverRoles.OverwatchEnabled; set => _hub.serverRoles.SetOverwatchStatus(value); }
        public bool IsIntercomMuted { get => _hub.characterClassManager.NetworkIntercomMuted; set => _hub.characterClassManager.NetworkIntercomMuted = value; }
        public bool IsMuted { get => _hub.characterClassManager.NetworkMuted; set => _hub.characterClassManager.NetworkMuted = value; }
        public ItemType ItemInHand { get => _hub.inventory.curItem; set => _hub.inventory.SetCurItem(value); }
        public int MaxHealth { get => _hub.playerStats.maxHP; set => _hub.playerStats.maxHP = value; }
        public string Nick { get => _hub.nicknameSync.MyNick; set => _hub.nicknameSync.DisplayName = value; }
        public bool NoClip { get => _hub.characterClassManager.NetworkNoclipEnabled; set => _hub.characterClassManager.NetworkNoclipEnabled = value; }
        public Vector3 Position { get => _hub.playerMovementSync.RealModelPosition; set => _hub.playerMovementSync.OverridePosition(value, _hub.PlayerCameraReference.rotation.eulerAngles.magnitude); }
        public RoleType Role { get => _hub.characterClassManager.NetworkCurClass; set => _hub.characterClassManager.SetClassID(value); }
        public string Token { get => _hub.characterClassManager.AuthTokenSerial; }
        public string UserId { get => _hub.characterClassManager.UserId; set => _hub.characterClassManager.UserId = value; }
        public NetworkConnection Connection => _hub.scp079PlayerScript.connectionToClient;
        public TeamType Team
        {
            get
            {
                if (Role == RoleType.ChaosInsurgency)
                {
                    return TeamType.ChaosInsurgency;
                }
                if (Role == RoleType.ClassD)
                {
                    return TeamType.ClassDPersonnel;
                }
                if (Role == RoleType.FacilityGuard || this.Role == RoleType.NtfCadet || this.Role == RoleType.NtfCommander || this.Role == RoleType.NtfLieutenant || this.Role == RoleType.NtfScientist)
                {
                    return TeamType.NineTailedFox;
                }
                if (Role == RoleType.Scientist)
                {
                    return TeamType.Scientist;
                }
                if (Role == RoleType.Scp049 || this.Role == RoleType.Scp0492 || this.Role == RoleType.Scp079 || this.Role == RoleType.Scp096 || this.Role == RoleType.Scp106 || this.Role == RoleType.Scp173 || this.Role == RoleType.Scp93953 || this.Role == RoleType.Scp93989)
                {
                    return TeamType.SCP;
                }
                if (Role == RoleType.Spectator)
                {
                    return TeamType.Spectator;
                }
                if (Role == RoleType.Tutorial)
                {
                    return TeamType.Tutorial;
                }
                return TeamType.Spectator;
            }
        }

        public string ParsedUserId => UserId.Split('@')[0];

        public UserIdType UserIdType
        {
            get
            {
                if (UserId.Contains("@discord"))
                    return UserIdType.Discord;
                if (UserId.Contains("@patreon"))
                    return UserIdType.Patreon;
                if (UserId.Contains("@steam"))
                    return UserIdType.Steam;
                if (UserId.Contains("@northwood"))
                    return UserIdType.Northwood;
                return UserIdType.Unspecified;
            }
        }

        public void Ban(int duration) => Server.Ban(this, duration);
        public void Ban(int duration, string reason) => Server.Ban(this, duration, reason);
        public void Ban(int duration, string reason, string issuer) => Server.Ban(this, duration, reason, issuer);
        public void Kick() => Server.Kick(this);
        public void Kick(string reason) => Server.Kick(this, reason);
        public void IssuePermanentBan() => Server.IssuePermanentBan(this);
        public void IssuePermanentBan(string reason) => Server.IssuePermanentBan(this, reason);
        public void Broadcast(string message, int duration) => Server.Host.GetComponent<Broadcast>().TargetAddElement(Connection, message, (ushort)duration, global::Broadcast.BroadcastFlags.Normal);
        public void Broadcast(string message, int duration, bool monospaced) => Server.Host.GetComponent<Broadcast>().TargetAddElement(Connection, message, (ushort)duration, monospaced ? global::Broadcast.BroadcastFlags.Monospaced : global::Broadcast.BroadcastFlags.Normal);
        public void ClearBroadcasts() => Server.Host.GetComponent<Broadcast>().TargetClearElements(Connection);
        public void ConsoleMessage(string message, string color = "green") => _hub.characterClassManager.TargetConsolePrint(Connection, message, color);
        public void RemoteAdminMessage(string message) => _hub.queryProcessor._sender.SendRemoteAdminMessage(message);
        public void Damage(int amount) => _hub.playerStats.HurtPlayer(new PlayerStats.HitInfo((float)amount, "Server", DamageTypes.None, 0), this.GameObject);
        public void Kill() => Damage(100000);
        public void Teleport(Vector3 pos) => _hub.playerMovementSync.OverridePosition(pos, _hub.PlayerCameraReference.position.magnitude);

        public void ShowHint(string message, float duration = 5f)
        {
            HintParameter[] parameters = new HintParameter[]
            {
                new StringHintParameter(message),
            };
            _hub.hints.Show(new TextHint(message, parameters, null, duration));
        }

        public void SetPlayerScale(float x, float y, float z)
        {
            try
            {
                GameObject target = this.GameObject;
                NetworkIdentity identity = target.GetComponent<NetworkIdentity>();
                target.transform.localScale = new Vector3(1 * x, 1 * y, 1 * z);
                ObjectDestroyMessage destroyMessage = new ObjectDestroyMessage();
                destroyMessage.netId = identity.netId;
                foreach (GameObject player in PlayerManager.players)
                {
                    NetworkConnection playerCon = player.GetComponent<NetworkIdentity>().connectionToClient;
                    if (player != target)
                        playerCon.Send(destroyMessage, 0);
                    object[] parameters = new object[] { identity, playerCon };
                    typeof(NetworkServer).InvokeStaticMethod("SendSpawnMessage", parameters);
                }
            }
            catch (System.Exception e)
            {
                Log.Add("Player", e);
            }
        }

        public void Rocket(float speed) => Timing.RunCoroutine(DoRocket(this, speed));

        private IEnumerator<float> DoRocket(Player player, float speed)
        {
            int maxAmnt = 300000;
            int amnt = 0;
            while (player.Role != RoleType.Spectator)
            {
                player.Position = player.Position + Vector3.up * speed;
                amnt++;
                if (amnt >= maxAmnt)
                {
                    player.GodMode = false;
                    player.Kill();
                }
                yield return Timing.WaitForOneFrame;
            }
        }

        public override string ToString() => $"{this.Nick};{this.UserId};{this.PlayerId};{this.Token};{this.Role}";
    }
}

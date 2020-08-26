using UnityEngine;
using Mirror;
using Hints;
using System.Collections.Generic;
using MEC;
using Vigilance.Extensions;
using Vigilance.Enums;
using RemoteAdmin;
using System.Linq;

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
        public bool RemoteAdmin => _hub.serverRoles.RemoteAdmin;
        public bool GodMode { get => _hub.characterClassManager.GodMode; set => _hub.characterClassManager.GodMode = value; }
        public int Health { get => (int)_hub.playerStats.Health; set => _hub.playerStats.SetHPAmount(value); }
        public int PlayerId { get => _hub.queryProcessor.PlayerId; set => _hub.queryProcessor.NetworkPlayerId = value; }
        public string RankName { get => _hub.serverRoles.Group.BadgeText; set => _hub.serverRoles.SetText(value); }
        public string RankColor { get => _hub.serverRoles.Group.BadgeColor; set => _hub.serverRoles.SetColor(value.ToLower()); }
        public UserGroup UserGroup { get => _hub.serverRoles.Group; set => _hub.serverRoles.SetGroup(value, false); }
        public string IpAddress { get => _hub.queryProcessor._ipAddress; set => _hub.queryProcessor._ipAddress = value; }
        public PlayerCommandSender PlayerCommandSender => _hub.queryProcessor._sender;
        public CommandSender CommandSender => PlayerCommandSender;
        public bool IsHost => _hub.characterClassManager.IsHost;
        public bool IsDead => Team == TeamType.Spectator || Role == RoleType.None;
        public Camera079 Camera { get => _hub.scp079PlayerScript.currentCamera; set => _hub.scp079PlayerScript.RpcSwitchCamera(value.cameraId, false); }
        public int CameraId { get => Camera.cameraId; set => Camera = Map.GetCamera(value); }
        public bool BadgeHidden
        {
            get => string.IsNullOrEmpty(_hub.serverRoles.HiddenBadge);
            set
            {
                if (value)
                    _hub.characterClassManager.CmdRequestHideTag();
                else
                    _hub.characterClassManager.CallCmdRequestShowTag(false);
            }
        }
        public bool IsInOverwatch { get => _hub.serverRoles.OverwatchEnabled; set => _hub.serverRoles.SetOverwatchStatus(value); }
        public bool IsIntercomMuted { get => _hub.characterClassManager.NetworkIntercomMuted; set => _hub.characterClassManager.NetworkIntercomMuted = value; }
        public bool IsMuted { get => _hub.characterClassManager.NetworkMuted; set => _hub.characterClassManager.NetworkMuted = value; }
        public ItemType ItemInHand { get => _hub.inventory.curItem; set => _hub.inventory.SetCurItem(value); }
        public int MaxHealth { get => _hub.playerStats.maxHP; set => _hub.playerStats.maxHP = value; }
        public string Nick { get => _hub.nicknameSync.Network_myNickSync; set => _hub.nicknameSync.Network_myNickSync = value; }
        public string DisplayNick { get => _hub.nicknameSync.Network_displayName; set => _hub.nicknameSync.Network_displayName = value; }
        public bool NoClip { get => _hub.characterClassManager.NetworkNoclipEnabled; set => _hub.characterClassManager.NetworkNoclipEnabled = value; }
        public Vector3 Position { get => _hub.playerMovementSync.RealModelPosition; set => _hub.playerMovementSync.OverridePosition(value, _hub.PlayerCameraReference.rotation.eulerAngles.magnitude); }
        public RoleType Role { get => _hub.characterClassManager.NetworkCurClass; set => _hub.characterClassManager.SetPlayersClass(value, GameObject, false, false);  }
        public string Token { get => _hub.characterClassManager.AuthTokenSerial; set => _hub.characterClassManager.AuthTokenSerial = value; }
        public string UserId { get => _hub.characterClassManager.UserId; set => _hub.characterClassManager.UserId = value; }
        public string CustomUserId { get => _hub.characterClassManager.UserId2; set => _hub.characterClassManager.UserId2 = value; }
        public string NtfUnit { get => _hub.characterClassManager.NetworkCurUnitName; set => _hub.characterClassManager.NetworkCurUnitName = value; }
        public NetworkConnection Connection => _hub.scp079PlayerScript.connectionToClient;
        public Transform PlayerCamera => _hub.PlayerCameraReference;
        public PlayerMovementSync.RotationVector RotationVector { get => _hub.playerMovementSync.NetworkRotationSync; set => _hub.playerMovementSync.NetworkRotationSync = value; }
        public Vector3 Rotation { get => _hub.PlayerCameraReference.forward; set => _hub.PlayerCameraReference.forward = value; }
        public Quaternion RotationQuaternion { get => _hub.PlayerCameraReference.rotation; set => _hub.PlayerCameraReference.rotation = value; }
        public Vector2 Rotations { get => _hub.playerMovementSync.Rotations; set => _hub.playerMovementSync.Rotations = value; }
        public Color RoleColor => Role.GetColor();
        public int CufferId { get => _hub.handcuffs.NetworkCufferId; set => _hub.handcuffs.NetworkCufferId = value; }
        public bool IsCuffed => _hub.handcuffs.NetworkCufferId != -1;
        public bool IsReloading => _hub.weaponManager.IsReloading();
        public bool IsZooming => _hub.weaponManager.ZoomInProgress();
        public bool IsAlive => Role != RoleType.None && Role != RoleType.Spectator;
        public bool IsAnySCP => Team == TeamType.SCP;
        public bool IsSCP => Team == TeamType.SCP && Role != RoleType.Scp0492;
        public bool IsNTF => Team == TeamType.NineTailedFox;
        public TeamType Team => Role.GetTeam();

        public string ParsedUserId => UserId.Substring(0, UserId.LastIndexOf('@'));

        public UserIdType UserIdType
        {
            get
            {
                if (string.IsNullOrEmpty(UserId))
                    return UserIdType.Unspecified;
                int index = UserId.LastIndexOf('@');
                if (index == -1)
                    return UserIdType.Unspecified;
                switch (UserId.Substring(index))
                {
                    case "steam":
                        return UserIdType.Steam;
                    case "discord":
                        return UserIdType.Discord;
                    case "northwood":
                        return UserIdType.Northwood;
                    case "patreon":
                        return UserIdType.Patreon;
                    default:
                        return UserIdType.Unspecified;
                }
            }
        }

        public Room CurrentRoom
        {
            get
            {
                Vector3 end = Position - new Vector3(0f, 10f, 0f);
                bool flag = Physics.Linecast(Position, end, out RaycastHit raycastHit, -84058629);
                if (!flag || raycastHit.transform == null)
                    return null;
                Transform latestParent = raycastHit.transform;
                while (latestParent.parent?.parent != null)
                    latestParent = latestParent.parent;
                foreach (Room room in Map.Rooms)
                {
                    if (room.Transform == latestParent)
                        return room;
                }
                return new Room(latestParent.name, latestParent, latestParent.position);
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
        public void Teleport(RoomType room) => Teleport(Map.GetRoom(room).Position);
        public void Teleport(string roomName) => Teleport(Map.GetRoom(roomName).Position);
        public void BlinkTag() => Timing.RunCoroutine(blinkTag());
        public void RemoveHeldItem() => _hub.inventory.items.Remove(_hub.inventory.GetItemInHand());
        public void RemoveItem(Inventory.SyncItemInfo item) => _hub.inventory.items.Remove(item);
        public void ClearInventory() => _hub.inventory.Clear();
        public void AddItem(ItemType item) => _hub.inventory.AddNewItem(item);
        public void ResetInventory(List<Inventory.SyncItemInfo> newItems) => ResetInventory(newItems.Select(item => item.id).ToList());
        public int GetAmmo(AmmoType ammoType) => (int)_hub.ammoBox[(int)ammoType];

        public void ShowHint(string message, float duration = 10f)
        {
            HintParameter[] parameters = new HintParameter[]
            {
                new StringHintParameter(message),
            };
            _hub.hints.Show(new TextHint(message, parameters, null, duration));
        }

        public void DropItem(Inventory.SyncItemInfo item)
        {
            _hub.inventory.SetPickup(item.id, item.durability, Position, _hub.inventory.camera.transform.rotation, item.modSight, item.modBarrel, item.modOther);
            _hub.inventory.items.Remove(item);
        }

        public void SetRole(RoleType newRole, bool lite = false, bool isEscaped = false)
        {
            _hub.characterClassManager.SetPlayersClass(newRole, GameObject, lite, isEscaped);
        }

        public void Handcuff(Player cuffer)
        {
            if (cuffer?.Hub == null)
                return;
            if (!IsCuffed && cuffer.Hub.inventory.items.Any(item => item.id == ItemType.Disarmer) && Vector3.Distance(Position, cuffer.Position) <= 130f)
            {
                CufferId = cuffer.PlayerId;
            }
        }

        public void ResetInventory(List<ItemType> newItems)
        {
            _hub.inventory.Clear();
            if (newItems.Count > 0)
            {
                foreach (ItemType item in newItems)
                    AddItem(item);
            }
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

        public void SetAmmo(Inventory.SyncItemInfo weapon, int ammo)
        {
            _hub.inventory.items.ModifyDuration(_hub.inventory.items.IndexOf(weapon), (float)ammo);
        }

        public void SetAttachments(Inventory.SyncItemInfo item, int sight, int barrel, int other)
        {
            _hub.inventory.items.ModifyAttachments(_hub.inventory.items.IndexOf(item), sight, barrel, other);
        }

        public void SetAmmo(WeaponType weapon, int ammo)
        {
            int ammoType = (int)weapon.GetWeaponAmmoType();
            Hub.ammoBox[ammoType] = (uint)ammo;
        }

        public void SetAmmo(AmmoType ammo, int amount)
        {
            Hub.ammoBox[(int)ammo] = (uint)amount;
        }

        public float GetAmmo(Inventory.SyncItemInfo weapon)
        {
            int ammoType = (int)weapon.id.GetWeaponType().GetWeaponAmmoType();
            return Hub.ammoBox[ammoType];
        }

        public float GetAmmo(WeaponType weapon)
        {
            AmmoType ammoType = weapon.GetWeaponAmmoType();
            return Hub.ammoBox[(int)ammoType];
        }

        public void Rocket(float speed) => Timing.RunCoroutine(DoRocket(this, speed));

        private IEnumerator<float> DoRocket(Player player, float speed)
        { 
            int amnt = 0;
            while (player.Role != RoleType.Spectator)
            {
                player.Position = player.Position + Vector3.up * speed;
                amnt++;
                if (amnt >= 1000)
                {
                    player.GodMode = false;
                    player.Kill();
                }
                yield return Timing.WaitForOneFrame;
            }
        }

        private IEnumerator<float> blinkTag()
        {
            yield return Timing.WaitForOneFrame;
            BadgeHidden = !BadgeHidden;
            yield return Timing.WaitForOneFrame;
            BadgeHidden = !BadgeHidden;
        }
        public override string ToString() => $"N: {this.Nick};UID: {this.ParsedUserId};PID: {this.PlayerId};R: {this.Role}";
    }
}

using UnityEngine;
using Mirror;
using Hints;
using System.Collections.Generic;
using Vigilance.Extensions;
using Vigilance.Enums;
using RemoteAdmin;
using System.Linq;
using Vigilance.Patches.Events;
using CustomPlayerEffects;

namespace Vigilance.API
{
    public class Player
    {
        private ReferenceHub _hub;
        public Player(ReferenceHub hub)
        {
            _hub = hub;
            IsInvisible = false;
            PlayerLock = false;
        }

        public GameObject GameObject => _hub.gameObject;
        public ReferenceHub Hub => _hub;
        public bool PlayerLock { get; set; }
        public bool BypassMode => _hub.serverRoles.BypassMode;
        public bool DoNotTrack => _hub.serverRoles.DoNotTrack;
        public bool RemoteAdmin => _hub.serverRoles.RemoteAdmin;
        public bool GodMode { get => _hub.characterClassManager.GodMode; set => _hub.characterClassManager.GodMode = value; }
        public int Health { get => (int)_hub.playerStats.Health; set => _hub.playerStats.SetHPAmount(value); }
        public int PlayerId { get => _hub.queryProcessor.PlayerId; set => _hub.queryProcessor.NetworkPlayerId = value; }
        public string RankName
        {
            get
            {
                if (_hub == null)
                    return "Dedicated Server";
                if (_hub.characterClassManager != null && _hub.characterClassManager.IsHost)
                    return "Dedicated Server";
                if (string.IsNullOrEmpty(UserId))
                    return "Dedicated Server";
                if (_hub.nicknameSync == null)
                    return "Dedicated Server";
                if (string.IsNullOrEmpty(_hub.nicknameSync.MyNick))
                    return "Dedicated Server";
                if (_hub.serverRoles == null)
                    return "Dedicated Server";
                if (_hub.serverRoles.Group == null)
                    return "Dedicated Server";
                if (_hub.serverRoles.Group.BadgeText.IsEmpty())
                    return "None";
                return _hub.serverRoles.Group.BadgeText;
            }
            set
            {
                _hub.serverRoles.SetText(value);
            }
        }
        public string RankColor { get => _hub.serverRoles.Group.BadgeColor; set => _hub.serverRoles.SetColor(value.ToLower()); }
        public UserGroup UserGroup { get => _hub.serverRoles.Group; set => _hub.serverRoles.SetGroup(value, false); }
        public string IpAddress { get => _hub.queryProcessor._ipAddress; set => _hub.queryProcessor._ipAddress = value; }
        public bool IsInPocketDimension
        {
            get
            {
                if (Vector3.Distance(Position, Map.PocketDimension) < 100f || Vector3.Distance(new Vector3(0f, Position.y, 0f), Map.PocketDimension) < 100f || Map.PocketDimension.y == Position.y || CurrentRoom.Transform == Map.GetRoom(RoomType.PocketDimension).Transform)
                    return true;
                else
                    return false;
            }
            set
            {
                if (value)
                    Teleport(Map.PocketDimension);
                else
                    Teleport(Map.Rooms[Environment.Random.Next(Map.Rooms.Count)]);
            }
        }

        public List<Ragdoll> Ragdolls
        {
            get
            {
                if (!RagdollManager_SpawnRagdoll.Ragdolls.ContainsKey(this))
                    RagdollManager_SpawnRagdoll.Ragdolls.Add(this, new List<Ragdoll>());
                return RagdollManager_SpawnRagdoll.Ragdolls[this];
            }
        }

        public PlayerCommandSender PlayerCommandSender => _hub.queryProcessor._sender;
        public CommandSender CommandSender => PlayerCommandSender;
        public bool IsHost => _hub.characterClassManager.IsHost;
        public bool IsDead => Team == TeamType.Spectator || Role == RoleType.None;
        public bool Is939 => _hub.characterClassManager.CurClass.Is939();
        public Camera079 Camera { get => _hub.scp079PlayerScript.currentCamera; set => _hub.scp079PlayerScript.RpcSwitchCamera(value.cameraId, false); }
        public MovementLockType MovementLock { get => _hub.fpc.MovementLock; set => _hub.fpc.MovementLock = value; }
        public float Stamina { get => _hub.fpc.GetStamina(); set => _hub.fpc.ModifyStamina(value); }
        public Vector3 MoveDirectory => _hub.fpc.GetMoveDir;
        public bool StopInputs { get => _hub.fpc.NetworkforceStopInputs; set => _hub.fpc.NetworkforceStopInputs = value; }
        public bool IsSprinting => _hub.fpc.IsSprinting;
        public bool IsSneaking => _hub.fpc.IsSneaking;
        public bool IsWalking => _hub.fpc.IsWalking;
        public bool IsJumping => _hub.fpc.isJumping;
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

        public Class CurrentClass
        {
            get
            {
                if (!ClassHelper.ClassesSet || ClassHelper.Classes == null)
                    ClassHelper.SetClasses(_hub.characterClassManager);
                Class c = ClassHelper.Get(Role);
                return c;
            }
        }

        public bool IsInOverwatch { get => _hub.serverRoles.OverwatchEnabled; set => _hub.serverRoles.SetOverwatchStatus(value); }
        public bool IsIntercomMuted { get => _hub.characterClassManager.NetworkIntercomMuted; set => _hub.characterClassManager.NetworkIntercomMuted = value; }
        public bool IsMuted { get => _hub.characterClassManager.NetworkMuted; set => _hub.characterClassManager.NetworkMuted = value; }
        public bool IsFriendlyFireEnabled { get; set; }
        public bool IsUsingStamina { get; set; } = true;
        public bool IsInvisible { get; set; }
        public ItemType ItemInHand { get => _hub.inventory.curItem; set => _hub.inventory.SetCurItem(value); }
        public Inventory.SyncItemInfo CurrentItem { get => _hub.inventory.GetItemInHand(); set => _hub.inventory.items[_hub.inventory.items.IndexOf(_hub.inventory.GetItemInHand())] = value; }
        public int MaxHealth { get => _hub.playerStats.maxHP; set => _hub.playerStats.maxHP = value; }
        public string CustomInfo { get => _hub.nicknameSync.Network_customPlayerInfoString; set => _hub.nicknameSync.Network_customPlayerInfoString = value; }

        public string Nick
        {
            get
            {
                if (_hub == null)
                    return "Dedicated Server";
                if (_hub.characterClassManager != null && _hub.characterClassManager.IsHost)
                    return "Dedicated Server";
                if (string.IsNullOrEmpty(UserId))
                    return "Dedicated Server";
                if (_hub.nicknameSync == null)
                    return "Dedicated Server";
                if (string.IsNullOrEmpty(_hub.nicknameSync.MyNick))
                    return "Dedicated Server";
                return _hub.nicknameSync.Network_myNickSync;
            }
            set => _hub.nicknameSync.Network_myNickSync = value;
        }

        public string DisplayNick { get => _hub.nicknameSync.Network_displayName; set => _hub.nicknameSync.Network_displayName = value; }
        public Vector3 Position { get => _hub.playerMovementSync.RealModelPosition; set => _hub.playerMovementSync.OverridePosition(value, _hub.PlayerCameraReference.rotation.y); }
        public RoleType Role { get => _hub.characterClassManager.NetworkCurClass; set => _hub.characterClassManager.SetPlayersClass(value, GameObject, false, false); }
        public string Token { get => _hub.characterClassManager.AuthTokenSerial; set => _hub.characterClassManager.AuthTokenSerial = value; }
        public string UserId
        {
            get => string.IsNullOrEmpty(_hub.characterClassManager.UserId) ? "Dedicated Server" : _hub.characterClassManager.UserId;
            set => _hub.characterClassManager.UserId = value;
        }
        public string CustomUserId { get => _hub.characterClassManager.UserId2; set => _hub.characterClassManager.UserId2 = value; }
        public string NtfUnit { get => _hub.characterClassManager.NetworkCurUnitName; set => _hub.characterClassManager.NetworkCurUnitName = value; }
        public NetworkConnection Connection => _hub.scp079PlayerScript.connectionToClient;
        public Transform PlayerCamera => _hub.PlayerCameraReference;
        public PlayerMovementSync.RotationVector RotationVector { get => _hub.playerMovementSync.NetworkRotationSync; set => _hub.playerMovementSync.NetworkRotationSync = value; }
        public Vector3 Rotation { get => _hub.PlayerCameraReference.forward; set => _hub.PlayerCameraReference.forward = value; }
        public Quaternion RotationQuaternion { get => _hub.PlayerCameraReference.rotation; set => _hub.PlayerCameraReference.rotation = value; }
        public Vector2 Rotations { get => _hub.playerMovementSync.Rotations; set => _hub.playerMovementSync.Rotations = value; }
        public PlayerInfoArea InfoArea { get => _hub.nicknameSync.Network_playerInfoToShow; set => _hub.nicknameSync.Network_playerInfoToShow = value; }
        public Color RoleColor => Role.GetColor();
        public int CufferId { get => _hub.handcuffs.NetworkCufferId; set => _hub.handcuffs.NetworkCufferId = value; }
        public bool IsCuffed => _hub.handcuffs.NetworkCufferId != -1;
        public bool IsReloading => _hub.weaponManager.IsReloading();
        public bool IsZooming { get => _hub.weaponManager.NetworksyncZoomed; set => _hub.weaponManager.NetworksyncZoomed = value; }
        public bool IsFlashed { get => _hub.weaponManager.NetworksyncFlash; set => _hub.weaponManager.NetworksyncFlash = value; }
        public bool IsAlive => Role != RoleType.None && Role != RoleType.Spectator;
        public bool IsAnySCP => Team == TeamType.SCP;
        public bool IsSCP => Team == TeamType.SCP && Role != RoleType.Scp0492;
        public bool IsNTF => Team == TeamType.NineTailedFox;
        public bool CheckPermission(PlayerPermissions perm) => PermissionsHandler.IsPermitted(UserGroup.Permissions, perm);
        public TeamType Team => Role.GetTeam();

        public string ParsedUserId
        {
            get
            {
                if (_hub == null)
                    return "Dedicated Server";
                if (_hub.characterClassManager != null && _hub.characterClassManager.IsHost)
                    return "Dedicated Server";
                if (string.IsNullOrEmpty(UserId))
                    return "Dedicated Server";
                if (_hub.nicknameSync == null)
                    return "Dedicated Server";
                if (string.IsNullOrEmpty(_hub.nicknameSync.MyNick))
                    return "Dedicated Server";
                if (IsHost || UserId.IsEmpty() || IpAddress.StartsWith("local"))
                    return "Dedicated Server";
                return UserId.Substring(0, UserId.LastIndexOf('@'));
            }
        }

        public UserIdType UserIdType
        {
            get
            {
                if (string.IsNullOrEmpty(UserId))
                    return UserIdType.Unspecified;
                if (UserId.Contains("@steam"))
                    return UserIdType.Steam;
                if (UserId.Contains("@discord"))
                    return UserIdType.Discord;
                if (UserId.Contains("@northwood"))
                    return UserIdType.Northwood;
                if (UserId.Contains("@patreon"))
                    return UserIdType.Patreon;
                return UserIdType.Unspecified;
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
                return new Room(latestParent.name, latestParent.gameObject, latestParent.position);
            }
        }

        public Vector3 Scale
        {
            get
            {
                return GameObject.transform.localScale;
            }
            set
            {
                try
                {
                    NetworkIdentity identity = GameObject.GetComponent<NetworkIdentity>();
                    GameObject.transform.localScale = value;
                    ObjectDestroyMessage destroyMessage = new ObjectDestroyMessage();
                    destroyMessage.netId = identity.netId;
                    foreach (GameObject player in PlayerManager.players)
                    {
                        NetworkConnection playerCon = player.GetComponent<NetworkIdentity>().connectionToClient;
                        if (player != GameObject)
                            playerCon.Send(destroyMessage, 0);
                        object[] parameters = new object[] { identity, playerCon };
                        typeof(NetworkServer).InvokeStaticMethod("SendSpawnMessage", parameters);
                    }
                }
                catch (System.Exception e)
                {
                    Log.Add("Viilance.API.Player.set_Scale", e);
                }
            }
        }

        public void Ban(int duration) => Server.Ban(this, duration);
        public void Ban(int duration, string reason) => Server.Ban(this, duration, reason);
        public void Ban(int duration, string reason, string issuer) => Server.Ban(this, duration, reason, issuer);
        public void Kick() => Server.Kick(this);
        public void Kick(string reason) => Server.Kick(this, reason);
        public void IssuePermanentBan() => Server.IssuePermanentBan(this);
        public void IssuePermanentBan(string reason) => Server.IssuePermanentBan(this, reason);
        public void Broadcast(string message, int duration) => PlayerManager.localPlayer.GetComponent<global::Broadcast>().TargetAddElement(Connection, message, (ushort)duration, global::Broadcast.BroadcastFlags.Normal);
        public void Broadcast(string message, int duration, bool monospaced) => PlayerManager.localPlayer.GetComponent<global::Broadcast>().TargetAddElement(Connection, message, (ushort)duration, monospaced ? global::Broadcast.BroadcastFlags.Monospaced : global::Broadcast.BroadcastFlags.Normal);
        public void Broadcast(Broadcast bc) => Broadcast(bc.Message, bc.Duration, bc.Monospaced);
        public void ClearBroadcasts() => PlayerManager.localPlayer.GetComponent<global::Broadcast>().TargetClearElements(Connection);
        public void ConsoleMessage(string message, string color = "green") => _hub.characterClassManager.TargetConsolePrint(Connection, message, color);
        public void RemoteAdminMessage(string message) => _hub.queryProcessor._sender.SendRemoteAdminMessage(message);
        public void Damage(int amount) => _hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(amount, "WORLD", DamageTypes.Wall, PlayerId), GameObject);
        public void Damage(int amount, DamageTypes.DamageType type) => _hub.playerStats.HurtPlayer(new PlayerStats.HitInfo(amount, "WORLD", type, 0), GameObject, false);
        public void Kill() => Damage(100000);
        public void Teleport(Vector3 pos) => _hub.playerMovementSync.OverridePosition(pos, _hub.PlayerCameraReference.rotation.y);
        public void Teleport(RoomType room) => Teleport(Map.GetRoom(room).Position);
        public void Teleport(string roomName) => Teleport(Map.GetRoom(roomName).Position);
        public void RemoveHeldItem() => _hub.inventory.items.Remove(_hub.inventory.GetItemInHand());
        public void RemoveItem(Inventory.SyncItemInfo item) => _hub.inventory.items.Remove(item);
        public void ClearInventory() => _hub.inventory.Clear();
        public void AddItem(ItemType item)
        {
            if (_hub.inventory.items.Count >= 8)
            {
                Map.SpawnItem(item, Position, RotationQuaternion);
            }
            else
                _hub.inventory.AddNewItem(item);
        }
        public void ResetInventory(List<Inventory.SyncItemInfo> newItems) => ResetInventory(newItems.Select(item => item.id).ToList());
        public void DropAllItems() => Hub.inventory.ServerDropAll();
        public int GetAmmo(AmmoType ammoType) => (int)_hub.ammoBox[(int)ammoType];
        public T GetComponent<T>() where T : Component => _hub.gameObject.GetComponent<T>();
        public T AddComponent<T>() where T : Component => _hub.gameObject.AddComponent<T>();
        public bool HasItem(ItemType item) => _hub.inventory.items.Select(h => h.id).Contains(item);
        public void ResetStamina() => _hub.fpc.ResetStamina();
        public void ShowHint(Broadcast bc) => ShowHint(bc.Message, bc.Duration);

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

        public void SetRole(RoleType newRole, bool keepPos = false, bool isEscaped = false)
        {
            _hub.characterClassManager.SetPlayersClass(newRole, GameObject, keepPos, isEscaped);
        }

        public void Handcuff(Player cuffer)
        {
            if (cuffer == null)
            {
                Hub.handcuffs.NetworkForceCuff = true;
                return;
            }
            if (!IsCuffed && cuffer.Hub.inventory.items.Any(item => item.id == ItemType.Disarmer) && Vector3.Distance(Position, cuffer.Position) <= 130f)
            {
                CufferId = cuffer.PlayerId;
            }
        }

        public void Uncuff()
        {
            Hub.handcuffs.NetworkForceCuff = false;
            Hub.handcuffs.CufferId = -1;
        }

        public void ResetInventory(List<ItemType> newItems)
        {
            _hub.inventory.Clear();
            if (newItems != null && newItems.Count > 0)
            {
                foreach (ItemType item in newItems)
                    AddItem(item);
            }
        }

        public void SetInfo(PlayerInfoArea area, string content)
        {
            _hub.nicknameSync.ShownPlayerInfo = area;
            _hub.nicknameSync.Network_customPlayerInfoString = content;
        }

        public void SetAmmo(Inventory.SyncItemInfo weapon, int ammo)
        {
            _hub.inventory.items.ModifyDuration(_hub.inventory.items.IndexOf(weapon), ammo);
        }

        public void SetAttachments(Inventory.SyncItemInfo item, int sight, int barrel, int other)
        {
            _hub.inventory.items.ModifyAttachments(_hub.inventory.items.IndexOf(item), sight, barrel, other);
        }

        public void SetAmmo(WeaponType weapon, int ammo)
        {
            foreach (Inventory.SyncItemInfo item in GetWeapons(weapon))
            {
                SetAmmo(item, ammo);
            }
        }

        public void SetAmmo(AmmoType ammo, int amount)
        {
            Hub.ammoBox[(int)ammo] = (uint)amount;
        }

        public float GetAmmo(Inventory.SyncItemInfo weapon)
        {
            return weapon.durability;
        }

        public float GetAmmo(WeaponType weapon)
        {
            Inventory.SyncItemInfo item = GetWeapon(weapon);
            return item.durability;
        }

        public void Explode(float force)
        {
            for (int i = 0; i < force; i++)
            {
                Prefab.GrenadeFrag.Spawn(Position, RotationQuaternion, Vector3.one);
            }

            foreach (Player player in Server.Players)
            {
                if (player.Distance(Position) <= 5f && !player.GodMode && player.IsAlive)
                {
                    player.Kill();
                }
            }

            foreach (Door door in Map.Doors)
            {
                if (Distance(door.transform.position) < 10f && !door.Networkdestroyed)
                {
                    door.Networkdestroyed = true;
                }
            }
        }

        public void Achieve(Achievement achievement)
        {
            if (achievement == Achievement.Unknown)
                return;
            if (achievement == Achievement.JustResources)
            {
                _hub.playerStats.TargetStats(Connection, "dboys_killed", "justresources", 50);
                return;
            }
            Environment.Cache.LocalStats?.TargetAchieve(Connection, achievement.ToString().ToLower());
        }

        public float Distance(Vector3 pos) => Vector3.Distance(Position, pos);

        public Inventory.SyncItemInfo GetWeapon(WeaponType weapon)
        {
            foreach (Inventory.SyncItemInfo item in Hub.inventory.items)
            {
                if (item.id == ItemType.GunCOM15 && weapon == WeaponType.Com15)
                    return item;
                if (item.id == ItemType.GunE11SR && weapon == WeaponType.Epsilon11)
                    return item;
                if (item.id == ItemType.GunLogicer && weapon == WeaponType.Logicer)
                    return item;
                if (item.id == ItemType.GunMP7 && weapon == WeaponType.MP7)
                    return item;
                if (item.id == ItemType.GunProject90 && weapon == WeaponType.Project90)
                    return item;
                if (item.id == ItemType.GunUSP && weapon == WeaponType.USP)
                    return item;
                if (item.id == ItemType.MicroHID && weapon == WeaponType.MicroHID)
                    return item;
            }
            return default;
        }

        public List<Inventory.SyncItemInfo> GetWeapons(WeaponType weapon)
        {
            List<Inventory.SyncItemInfo> items = new List<Inventory.SyncItemInfo>();
            foreach (Inventory.SyncItemInfo item in Hub.inventory.items)
            {
                if (item.id == ItemType.GunCOM15 && weapon == WeaponType.Com15)
                    items.Add(item);
                if (item.id == ItemType.GunE11SR && weapon == WeaponType.Epsilon11)
                    items.Add(item);
                if (item.id == ItemType.GunLogicer && weapon == WeaponType.Logicer)
                    items.Add(item);
                if (item.id == ItemType.GunMP7 && weapon == WeaponType.MP7)
                    items.Add(item);
                if (item.id == ItemType.GunProject90 && weapon == WeaponType.Project90)
                    items.Add(item);
                if (item.id == ItemType.GunUSP && weapon == WeaponType.USP)
                    items.Add(item);
                if (item.id == ItemType.MicroHID && weapon == WeaponType.MicroHID)
                    items.Add(item);
            }
            return items;
        }

        public void CreatePortal(Player target = null)
        {
            if (Role != RoleType.Scp106)
                return;
            if (target == null)
            {
                _hub.scp106PlayerScript.CallCmdMakePortal();
            }
            else
            {
                Scp106PlayerScript script = _hub.scp106PlayerScript;
                Transform transform = target.GameObject.transform;
                Debug.DrawRay(transform.position, -transform.up, Color.red, 10f);
                RaycastHit raycastHit;
                if (script.iAm106 && !script.goingViaThePortal && Physics.Raycast(new Ray(script.transform.position, -script.transform.up), out raycastHit, 10f, script.teleportPlacementMask))
                {
                    Vector3 pos = raycastHit.point - Vector3.up;
                    script.SetPortalPosition(pos);
                }
            }
        }

        public void CreatePortal(Vector3 pos)
        {
            if (Role != RoleType.Scp106)
                return;
            if (pos == Vector3.zero)
            {
                _hub.scp106PlayerScript.CallCmdMakePortal();
            }
            else
            {
                Scp106PlayerScript script = _hub.scp106PlayerScript;
                Debug.DrawRay(pos, -Vector3.up, Color.red, 10f);
                RaycastHit raycastHit;
                if (script.iAm106 && !script.goingViaThePortal && Physics.Raycast(new Ray(script.transform.position, -script.transform.up), out raycastHit, 10f, script.teleportPlacementMask))
                {
                    Vector3 position = raycastHit.point - Vector3.up;
                    script.SetPortalPosition(position);
                }
            }
        }

        public void UsePortal()
        {
            if (Role != RoleType.Scp106)
                return;
            _hub.scp106PlayerScript.CallCmdUsePortal();
        }

        public void Teleport(Room room) => Teleport(Environment.FindSafePosition(room.Position));
        public void Teleport(Rid rid) => Teleport(Environment.FindSafePosition(rid.transform.position));

        public void EnableEffect<T>(float duration = 0f, bool addIfActive = false) where T : PlayerEffect => _hub.playerEffectsController.EnableEffect<T>(duration, addIfActive);
        public void DisableEffect<T>() where T : PlayerEffect => _hub.playerEffectsController.DisableEffect<T>();
        public T GetEffect<T>() where T : PlayerEffect => _hub.playerEffectsController.GetEffect<T>();
        public override string ToString() => $"[{PlayerId}]: {Nick} [{UserId}] [{IpAddress}] [{Role.AsString()}]";
    }
}

using Vigilance.API.Enums;
using Vigilance.API.Extensions;
using UnityEngine;
using Grenades;
using RemoteAdmin;
using Mirror;
using Hints;
using Searching;
using System.Collections.Generic;
using CustomPlayerEffects;
using MEC;

namespace Vigilance.API
{
    public class Player
    {
        private GameObject _gameObject;
        public Player(GameObject gobject)
        {
            _gameObject = gobject;
        }

        // Components
        public ReferenceHub Hub => ReferenceHub.GetHub(_gameObject);
        public CharacterClassManager ClassManager => Hub.characterClassManager;
        public AmmoBox AmmoBox => Hub.ammoBox;
        public AnimationController AnimationController => Hub.animationController;
        public CameraEffectsController CameraEffectsController => Hub.gfxController;
        public NetworkConnection Connection => Hub.scp079PlayerScript.connectionToClient;
        public NetworkIdentity Identity => Hub.gameObject.GetComponent<NetworkIdentity>();
        public DisableUselessComponents DisableUselessComponents => Hub.disableUselessComponents;
        public PlayerEffectsController EffectsController => Hub.playerEffectsController;
        public FallDamage FallDamage => Hub.falldamage;
        public FirstPersonController FirstPersonController => Hub.fpc;
        public FootstepSync FootstepSync => Hub.footstepSync;
        public GameObject GameObject => _gameObject;
        public GrenadeManager GrenadeManager => _gameObject.GetComponent<GrenadeManager>();
        public Handcuffs Handcuffs => Hub.handcuffs;
        public HintDisplay HintDisplay => Hub.hints;
        public Inventory Inventory => Hub.inventory;
        public NicknameSync NicknameSync => Hub.nicknameSync;
        public PlayableScpsController PlayableScpsController => Hub.scpsController;
        public PlayerMovementSync PlayerMovement => Hub.playerMovementSync;
        public PlayerStats PlayerStats => Hub.playerStats;
        public QueryProcessor QueryProcessor => Hub.queryProcessor;
        public Scp079PlayerScript Scp079Script => Hub.scp079PlayerScript;
        public SearchCoordinator SearchCoordinator => Hub.searchCoordinator;
        public ServerRoles ServerRoles => Hub.serverRoles;
        public SpectatorManager SpectatorManager => Hub.spectatorManager;
        public UserGroup UserGroup => Hub.serverRoles.Group;
        public WeaponManager WeaponManager => Hub.weaponManager;
        public Transform Camera => Hub.PlayerCameraReference;

        // Other
        public bool BreakDoors
        {
            get => Data.BreakDoors.Contains(UserId);
            set
            {
                if (value)
                    Data.BreakDoors.Add(UserId);
                else
                    Data.BreakDoors.Remove(UserId);
            }
        }

        public string ParsedUserId
        {
            get
            {
                string[] array = UserId.Split('@');
                return array[0];
            }
        }

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

        public bool BypassMode { get => this.ServerRoles.BypassMode; set => this.ServerRoles.BypassMode = value; }
        public bool DoNotTrack { get => this.ServerRoles.DoNotTrack; set => this.ServerRoles.DoNotTrack = value; }
        public bool GodMode { get => this.ClassManager.GodMode; set => this.ClassManager.GodMode = value; }
        public int Health { get => (int)this.PlayerStats.Health; set => this.PlayerStats.SetHPAmount(value); }
        public int Id { get => this.QueryProcessor.PlayerId; set => this.QueryProcessor.NetworkPlayerId = value; }
        public bool InstantKill
        {
            get => Data.InstantKill.Contains(UserId);
            set
            {
                if (value)
                    Data.InstantKill.Add(UserId);
                else
                    Data.InstantKill.Remove(UserId);
            }
        }

        public string IpAdress => this.QueryProcessor._ipAddress;
        public bool IsAlive;
        public bool IsAnySCP;
        public bool IsInOverwatch { get => this.ServerRoles.OverwatchEnabled; set => this.ServerRoles.SetOverwatchStatus(value); }
        public bool IsIntercomMuted { get => this.ClassManager.NetworkIntercomMuted; set => this.ClassManager.NetworkIntercomMuted = value; }
        public bool IsMuted { get => this.ClassManager.NetworkMuted; set => this.ClassManager.NetworkMuted = value; }
        public ItemType ItemInHand { get => this.Inventory.curItem; set => this.Inventory.SetCurItem(value); }
        public int MaxHealth { get => this.PlayerStats.maxHP; set => this.PlayerStats.maxHP = value; }
        public string Nick { get => this.NicknameSync.MyNick; set => this.NicknameSync.DisplayName = value; }
        public bool NoClip { get => this.ClassManager.NetworkNoclipEnabled; set => this.ClassManager.NetworkNoclipEnabled = value; }
        public bool PlayerLock
        {
            get => Data.PlayerLock.Contains(UserId);
            set
            {
                if (value)
                    Data.PlayerLock.Add(UserId);
                else
                    Data.PlayerLock.Remove(UserId);
            }
        }

        public Vector3 Position { get => this.PlayerMovement.RealModelPosition; set => this.PlayerMovement.OverridePosition(value, this.Camera.rotation.eulerAngles.magnitude); }
        public RoleType Role { get => this.ClassManager.NetworkCurClass; set => this.ClassManager.SetClassID(value); }
        public List<Inventory.SyncItemInfo> SyncItems
        {
            get
            {
                List<Inventory.SyncItemInfo> list = new List<Inventory.SyncItemInfo>();
                foreach (Inventory.SyncItemInfo item in this.SyncItems)
                {
                    list.Add(item);
                }
                return list;
            }
        }

        public TeamType Team
        {
            get
            {
                if (this.Role == RoleType.ChaosInsurgency)
                {
                    return TeamType.ChaosInsurgency;
                }
                if (this.Role == RoleType.ClassD)
                {
                    return TeamType.ClassDPersonnel;
                }
                if (this.Role == RoleType.FacilityGuard || this.Role == RoleType.NtfCadet || this.Role == RoleType.NtfCommander || this.Role == RoleType.NtfLieutenant || this.Role == RoleType.NtfScientist)
                {
                    return TeamType.NineTailedFox;
                }
                if (this.Role == RoleType.Scientist)
                {
                    return TeamType.Scientist;
                }
                if (this.Role == RoleType.Scp049 || this.Role == RoleType.Scp0492 || this.Role == RoleType.Scp079 || this.Role == RoleType.Scp096 || this.Role == RoleType.Scp106 || this.Role == RoleType.Scp173 || this.Role == RoleType.Scp93953 || this.Role == RoleType.Scp93989)
                {
                    return TeamType.SCP;
                }
                if (this.Role == RoleType.Spectator)
                {
                    return TeamType.Spectator;
                }
                if (this.Role == RoleType.Tutorial)
                {
                    return TeamType.Tutorial;
                }
                return TeamType.Spectator;
            }
        }

        public string Token { get => this.ClassManager.AuthTokenSerial; }
        public string UserId { get => this.ClassManager.UserId; set => this.ClassManager.UserId = value; }

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
        public void ConsoleMessage(string message, string color = "green") => this.ClassManager.TargetConsolePrint(Connection, message, color);
        public void RemoteAdminMessage(string message) => this.QueryProcessor._sender.SendRemoteAdminMessage(message);
        public void Damage(int amount) => this.PlayerStats.HurtPlayer(new PlayerStats.HitInfo((float)amount, "Server", DamageTypes.None, 0), this.GameObject);
        public void Kill() => Damage(100000);
        public void Disable939Visuals() => this.EffectsController.DisableEffect<Visuals939>();
        public void DropAllItems() => this.Inventory.ServerDropAll();
        public void Enable939Visuals() => this.EffectsController.EnableEffect<Visuals939>(float.MaxValue);
        public void EnableGhost()
        {
            this.Inventory.AddNewItem(ItemType.SCP268);
            this.EffectsController.EnableEffect<Scp268>(float.MaxValue);
        }

        public void DisableGhost() => this.EffectsController.DisableEffect<Scp268>();
        public void Freeze() => this.EffectsController.EnableEffect<Ensnared>(float.MaxValue);
        public void Unfreeze() => this.EffectsController.DisableEffect<Ensnared>();
        public List<ItemType> GetAllItems()
        {
            List<ItemType> list = new List<ItemType>();
            foreach (Inventory.SyncItemInfo syncItemInfo in this.SyncItems)
            {
                list.Add(syncItemInfo.id);
            }
            return list;
        }

        public List<Item> GetItems()
        {
            List<Item> list = new List<Item>();
            foreach (Inventory.SyncItemInfo syncItemInfo in this.SyncItems)
            {
                list.Add(this.Inventory.GetItemByID(syncItemInfo.id));
            }
            return list;
        }

        public Item GetItem(ItemType item)
        {
            return this.Inventory.GetItemByID(item);
        }

        public void Teleport(Vector3 pos) => this.PlayerMovement.OverridePosition(pos, 0f);

        public void ShowHint(string message, float duration = 5f)
        {
            HintParameter[] parameters = new HintParameter[]
            {
                new StringHintParameter(message),
            };
            HintDisplay.Show(new TextHint(message, parameters, null, duration));
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
                Log.Error("Player", e);
            }
        }

        public void Rocket(float speed) => Timing.RunCoroutine(DoRocket(this, speed));

        private IEnumerator<float> DoRocket(Player player, float speed)
        {
            int maxAmnt = ConfigManager.GetInt("rocket_max_amount") == 0 ? 1000 : ConfigManager.GetInt("rocket_max_amount");
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

        public override string ToString() => $"{this.Nick};{this.UserId};{this.Id};{this.Token};{this.Role}";
    }
}

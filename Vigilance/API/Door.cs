using System.Collections.Generic;
using Interactables.Interobjects.DoorUtils;
using Interactables.Interobjects;
using Vigilance.Enums;
using UnityEngine;
using Vigilance.Extensions;
using System.Linq;

namespace Vigilance.API
{
    public class Door
    {
        private DoorVariant _door;

        public Door(DoorVariant d, Room room, DoorType type, int id, string tag)
        {
            _door = d;
            SetInfo(room, type, id, tag);
        }

        public int Id { get; internal set; }
        public string Name { get; internal set; }
        public Room Room { get; internal set; }
        public RoomType RoomType { get; internal set; }
        public DoorType Type { get; internal set; }
        public ZoneType Zone { get; internal set; }
        public DoorProperties[] Properties { get; internal set; }
        public KeycardPermissions Permissions { get; internal set; }
        public Vector3 Position { get; internal set; }
        public GameObject GameObject => _door.gameObject;
        public DoorVariant Variant => _door;
        public List<Player> DisallowedPlayers { get; internal set; }
        public bool IsLocked { get; set; }
        public bool IsDestroyed { get => ((BreakableDoor)_door).IsDestroyed; set => ((BreakableDoor)_door).IsDestroyed = value; }
        public bool IsOpen { get => _door.NetworkTargetState; set => _door.NetworkTargetState = value; }
        public bool RequiredAllPermissions => _door.RequiredPermissions.RequireAll;
        public bool ScpOverride => _door.RequiredPermissions.RequiredPermissions == KeycardPermissions.ScpOverride;
        public bool IsBreakable => Properties.Contains(DoorProperties.IsBreakable);
        public bool IsPryable => Properties.Contains(DoorProperties.IsPryable);

        public bool IsAllowed(Player player, bool allItems) => IsAllowed(player, allItems, 0);

        public bool IsAllowed(Player player, bool allItems, byte collider)
        {
            if (IsLocked) return false;
            if (DisallowedPlayers.Contains(player)) return false;
            if (player.PlayerLock) return false;
            if (!_door.AllowInteracting(player.Hub, collider)) return false;
            if (_door.RequiredPermissions.RequiredPermissions == KeycardPermissions.None) return true;
            if (player.IsAnySCP && (_door.RequiredPermissions.RequiredPermissions == KeycardPermissions.ScpOverride || _door.RequiredPermissions.RequiredPermissions.HasFlagFast(KeycardPermissions.ScpOverride))) return true;
            if (player.ItemInHand == ItemType.None && !ConfigManager.RemoteCard && !player.IsAnySCP && _door.RequiredPermissions.RequiredPermissions != KeycardPermissions.None) return false;
            if (player.Hub.inventory.items.Where(h => h.id.IsKeycard()).Count() < 1 && !player.IsAnySCP) return false;
            return allItems ? CheckAllPermissions(player) : CheckPermission(player.ItemInHand, player);
        }

        public bool CheckAllPermissions(Player player)
        {
            foreach (Inventory.SyncItemInfo item in player.Hub.inventory.items)
            {
                if (item.id == ItemType.None)
                    continue;
                if (!item.id.IsKeycard())
                    continue;
                if (CheckPermission(item.id, player))
                    return true;
            }
            return false;
        }

        public bool CheckPermission(ItemType item, Player player) => CheckPermission(player.Hub.inventory.GetItemByID(item), player);

        public bool CheckPermission(Item item, Player player)
        {
            if (player.PlayerLock || IsLocked)
                return false;
            if (player.BypassMode)
                return true;
            if (item == null)
                return player.IsAnySCP && (_door.RequiredPermissions.RequiredPermissions.HasFlagFast(KeycardPermissions.ScpOverride) || _door.RequiredPermissions.RequiredPermissions == KeycardPermissions.ScpOverride);
            if (_door.RequiredPermissions.RequiredPermissions == KeycardPermissions.None)
                return true;
            KeycardPermissions keycardPermissions = DoorPermissionUtils.TranslateObsoletePermissions(item.permissions);
            if (!_door.RequiredPermissions.RequireAll)
                return (keycardPermissions & _door.RequiredPermissions.RequiredPermissions) > KeycardPermissions.None;
            foreach (KeycardPermissions perm in item.permissions.GetPermissions())
                if (_door.RequiredPermissions.RequiredPermissions.HasFlagFast(perm) || _door.RequiredPermissions.RequiredPermissions == perm)
                    return true;
            return (keycardPermissions & _door.RequiredPermissions.RequiredPermissions) == _door.RequiredPermissions.RequiredPermissions;
        }

        public void ChangeLock(DoorLockReason reason, bool state) => _door.ServerChangeLock(reason, state);

        public void Destroy()
        {
            (_door as IDamageableDoor)?.ServerDamage(65535f, DoorDamageType.ServerCommand);
            IsDestroyed = true;
        }

        public void Delete()
        {
            Mirror.NetworkServer.Destroy(GameObject);
            _door = null;
            Id = -1;
            Name = null;
            Room = null;
            RoomType = RoomType.Unknown;
            Type = DoorType.UnknownDoor;
            Zone = ZoneType.Unspecified;
            Properties = null;
            Permissions = KeycardPermissions.None;
            Position = Vector3.zero;
            DisallowedPlayers = null;
        }

        public void Lock()
        {
            IsLocked = true;
            _door.ServerChangeLock(DoorLockReason.AdminCommand, true);
        }

        public void Unlock()
        {
            IsLocked = false;
            _door.ServerChangeLock(DoorLockReason.AdminCommand, false);
        }

        public void Open()
        {
            if (_door.IsConsideredOpen()) return;
            _door.NetworkTargetState = true;
        }

        public void Close()
        {
            if (!_door.IsConsideredOpen()) return;
            _door.NetworkTargetState = false;
        }

        public void ChangeState()
        {
            _door.NetworkTargetState = !_door.TargetState;
        }

        public void Deny()
        {
            _door.PermissionsDenied(null, 0);
        }

        public void Beep()
        {
            ((BasicDoor)_door).CallRpcPlayBeepSound(true);
        }

        private void SetInfo(Room room, DoorType type, int id, string tag)
        {
            Name = tag;
            Id = id;
            Room = room;
            RoomType = room == null ? RoomType.Unknown : room.Type;
            Type = type;
            Zone = room == null ? ZoneType.Unspecified : room.Zone;
            Permissions = _door.RequiredPermissions.RequiredPermissions;
            Position = _door.transform.position;
            IsLocked = false;
            DisallowedPlayers = new List<Player>();
            SetProperties();
        }

        private void SetProperties()
        {
            List<DoorProperties> props = new List<DoorProperties>();
            DoorType t = Type;
            bool pryable = t == DoorType.GateA || t == DoorType.GateB || t == DoorType.Scp914 || _door.GetType() == typeof(PryableDoor);
            bool breakable = t != DoorType.GateA && t != DoorType.GateB && t != DoorType.SurfaceGate && t != DoorType.UnknownDoor && t != DoorType.Scp372 && t != DoorType.Scp914 && t != DoorType.Scp173 || _door.GetType() == typeof(BreakableDoor);
            bool portalless = _door.name.ToLower().StartsWith("portalless");
            bool keycard = Permissions != KeycardPermissions.None;
            bool unsecured = _door.name.ToLower().StartsWith("unsecured");
            if (pryable) props.Add(DoorProperties.IsPryable);
            if (breakable) props.Add(DoorProperties.IsBreakable);
            if (portalless) props.Add(DoorProperties.Portalless);
            if (keycard) props.Add(DoorProperties.RequiresKeycard);
            if (unsecured) props.Add(DoorProperties.Unsecured);
            Properties = props.ToArray();
        }

        public static Door CheckpointA { get; internal set; }
        public static Door CheckpointB { get; internal set; }
        public static Door CheckpointEZ { get; internal set; }

        public static Door Scp173Gate { get; internal set; }
    }
}

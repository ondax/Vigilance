using System.Collections.Generic;
using Interactables.Interobjects.DoorUtils;
using Vigilance.Enums;
using UnityEngine;

namespace Vigilance.API
{
    public class Door
    {
        private DoorVariant _door;

        public Door(DoorVariant d)
        {
            _door = d;
            SetInfo();
        }

        public string Tag { get; internal set; }
        public RoomType Room { get; internal set; }
        public DoorType Type { get; internal set; }
        public DoorZone Zone { get; internal set; }
        public DoorProperties[] Properties { get; internal set; }
        public KeycardPermissions Permissions { get; internal set; }
        public Vector3 Position { get; internal set; }
        public GameObject GameObject => _door.gameObject;
        public DoorVariant Variant => _door;
        public bool IsLocked { get; set; }
        public bool RequiredAll => _door.RequiredPermissions.RequireAll;
        public bool ScpOverride => _door.RequiredPermissions.RequiredPermissions == KeycardPermissions.ScpOverride;
        public bool IsBreakable => Properties.Contains(DoorProperties.IsBreakable);
        public bool IsPryable => Properties.Contains(DoorProperties.IsPryable);
        public int InstanceId => _door.GetInstanceID();
        public uint NetId => _door.netId;

        public void Destroy()
        {
            IDamageableDoor door = _door as IDamageableDoor;
            if (door == null)
                return;
            door.ServerDamage(65535f, DoorDamageType.ServerCommand);
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

        private void SetInfo()
        {
            IsLocked = false;
            DoorVariant d = _door;
            DoorNametagExtension name = d.GetComponent<DoorNametagExtension>();
            List<DoorProperties> props = new List<DoorProperties>();

            if (d.name.ToLower().Contains("breakable"))
                props.Add(DoorProperties.IsBreakable);
            if (d.name.ToLower().Contains("pryable"))
                props.Add(DoorProperties.IsPryable);
            if (d.name.ToLower().Contains("portalless"))
                props.Add(DoorProperties.Portalless);
            if (d.name.ToLower().Contains("keycard"))
                props.Add(DoorProperties.RequiresKeycard);
            if (d.name.ToLower().Contains("unsecured"))
                props.Add(DoorProperties.Unsecured);
            if (d.name.StartsWith("EZ"))
                Zone = DoorZone.EntranceZone;
            if (d.name.StartsWith("HCZ"))
                Zone = DoorZone.HeavyContainmentZone;
            if (d.name.StartsWith("LCZ"))
                Zone = DoorZone.LightContainmentZone;

            if (d.name.StartsWith("Prison"))
            {
                Zone = DoorZone.LightContainmentZone;
                Type = DoorType.ClassD;
            }

            if (d.name.StartsWith("Intercom"))
            {
                Zone = DoorZone.EntranceZone;
                Type = DoorType.Intercom;
            }

            if (name != null)
            {
                Tag = name.GetName;

                if (Tag == "LCZ_CAFE" || Tag == "173_BOTTOM" || Tag == "012" || Tag == "HID_RIGHT" || Tag == "SERVERS_BOTTOM" || Tag == "HCZ_LEFT" || Tag == "106_BOTTOM" || Tag == "106_SECONDARY" || Tag == "LCZ_WC")
                {
                    Type = DoorType.Basic;
                }

                if (Tag == "INTERCOM")
                {
                    Type = DoorType.Intercom;
                    Zone = DoorZone.EntranceZone;
                    Properties = new DoorProperties[] { DoorProperties.IsBreakable, DoorProperties.RequiresKeycard };
                }

                if (Tag == "914")
                {
                    Type = DoorType.Gate914;
                    Zone = DoorZone.LightContainmentZone;
                    Properties = new DoorProperties[] { DoorProperties.IsPryable, DoorProperties.RequiresKeycard };
                }

                if (Tag == "GATE_B")
                {
                    Type = DoorType.GateB;
                    Zone = DoorZone.SurfaceZone;
                    Properties = new DoorProperties[] { DoorProperties.IsPryable, DoorProperties.RequiresKeycard };
                }

                if (Tag == "GATE_A")
                {
                    Type = DoorType.GateA;
                    Zone = DoorZone.SurfaceZone;
                    Properties = new DoorProperties[] { DoorProperties.IsPryable, DoorProperties.RequiresKeycard };
                }

                if (Tag == "GATE_173")
                {
                    Type = DoorType.Scp173Gate;
                    Zone = DoorZone.LightContainmentZone;
                    Properties = new DoorProperties[] { DoorProperties.IsPryable, DoorProperties.RequiresKeycard };
                    Scp173Gate = this;
                }

                if (Tag == "CHECKPOINT_LCZ_B")
                {
                    CheckpointB = this;
                }

                if (Tag == "CHECKPOINT_LCZ_A")
                {
                    CheckpointA = this;
                }

                if (Tag == "CHECKPOINT_EZ_HCZ")
                {
                    CheckpointEZ = this;
                }

                Position = d.transform.position;
                Properties = props.ToArray();
                Permissions = _door.RequiredPermissions.RequiredPermissions;

                if (!DoorVariants.ContainsKey(d))
                    DoorVariants.Add(d, this);
            }
        }

        public static Door GetDoor(DoorVariant variant)
        {
            if (variant == null)
                return null;
            foreach (KeyValuePair<DoorVariant, Door> pair in DoorVariants)
            {
                if (pair.Key == variant || pair.Key.netId == variant.netId)
                    return pair.Value;
            }

            return null;
        }

        public static bool TryGetDoor(DoorVariant d, out Door door)
        {
            door = GetDoor(d);
            return door != null;
        }

        public static Door CheckpointA { get; internal set; }
        public static Door CheckpointB { get; internal set; }
        public static Door CheckpointEZ { get; internal set; }

        public static Door Scp173Gate { get; internal set; }

        public static Dictionary<DoorVariant, Door> DoorVariants { get; } = new Dictionary<DoorVariant, Door>();
    }
}

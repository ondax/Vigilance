using System.Collections.Generic;
using System.Linq;
using Vigilance.Enums;
using UnityEngine;

namespace Vigilance.API
{

    public class Room
    {
        public Room(string name, GameObject obj, Vector3 position)
        {
            Name = name;
            Transform = obj.transform;
            Position = position;
            Zone = FindZone();
            Type = FindType(name);
            Doors = FindDoors();
            LightController = obj.transform.GetComponentInChildren<FlickerableLightController>();
            RoomInformation = obj.GetComponent<RoomInformation>();
        }
        public string Name { get; }
        public Transform Transform { get; }
        public Vector3 Position { get; }
        public ZoneType Zone { get; }
        public RoomType Type { get; }
        public List<Door> Doors { get; }
        public FlickerableLightController LightController { get; }
        public RoomInformation RoomInformation { get; }
        public IEnumerable<Player> Players => Server.Players.Where(player => player.CurrentRoom.Transform == Transform);

        public void TurnOffLights(float duration)
        {
            LightController?.ServerFlickerLights(duration);
        }

        private ZoneType FindZone()
        {
            if (Name == "PocketDimension")
                return ZoneType.PocketDimension;
            if (Transform.parent == null)
                return ZoneType.Unspecified;
            switch (Transform.parent.name)
            {
                case "HeavyRooms":
                    return ZoneType.HeavyContainment;
                case "LightRooms":
                    return ZoneType.LightContainment;
                case "EntranceRooms":
                    return ZoneType.Entrance;
                default:
                    return Position.y > 900 ? ZoneType.Surface : ZoneType.Unspecified;
            }
        }

        private RoomType FindType(string rawName)
        {
            if (rawName == "PocketDimension")
                return RoomType.PocketDimension;
            var bracketStart = rawName.IndexOf('(') - 1;
            if (bracketStart > 0)
                rawName = rawName.Remove(bracketStart, rawName.Length - bracketStart);
            switch (rawName)
            {
                case "LCZ_Armory":
                    return RoomType.LczArmory;
                case "LCZ_Curve":
                    return RoomType.LczCurve;
                case "LCZ_Straight":
                    return RoomType.LczStraight;
                case "LCZ_012":
                    return RoomType.Lcz012;
                case "LCZ_914":
                    return RoomType.Lcz914;
                case "LCZ_Crossing":
                    return RoomType.LczCrossing;
                case "LCZ_TCross":
                    return RoomType.LczTCross;
                case "LCZ_Cafe":
                    return RoomType.LczCafe;
                case "LCZ_Plants":
                    return RoomType.LczPlants;
                case "LCZ_Toilets":
                    return RoomType.LczToilets;
                case "LCZ_Airlock":
                    return RoomType.LczAirlock;
                case "LCZ_173":
                    return RoomType.Lcz173;
                case "LCZ_ClassDSpawn":
                    return RoomType.LczClassDSpawn;
                case "LCZ_ChkpB":
                    return RoomType.LczCheckpointB;
                case "LCZ_372":
                    return RoomType.LczGlassBox;
                case "LCZ_ChkpA":
                    return RoomType.LczCheckpointB;
                case "HCZ_079":
                    return RoomType.Hcz079;
                case "HCZ_EZ_Checkpoint":
                    return RoomType.HczEzCheckpoint;
                case "HCZ_Room3ar":
                    return RoomType.HczArmory;
                case "HCZ_Testroom":
                    return RoomType.Hcz939;
                case "HCZ_Hid":
                    return RoomType.HczHid;
                case "HCZ_049":
                    return RoomType.Hcz049;
                case "HCZ_ChkpA":
                    return RoomType.HczCheckpointA;
                case "HCZ_Crossing":
                    return RoomType.HczCrossing;
                case "HCZ_106":
                    return RoomType.Hcz106;
                case "HCZ_Nuke":
                    return RoomType.HczNuke;
                case "HCZ_Tesla":
                    return RoomType.HczTesla;
                case "HCZ_Servers":
                    return RoomType.HczServers;
                case "HCZ_ChkpB":
                    return RoomType.HczCheckpointB;
                case "HCZ_Room3":
                    return RoomType.HczTCross;
                case "HCZ_457":
                    return RoomType.Hcz096;
                case "HCZ_Curve":
                    return RoomType.HczCurve;
                case "EZ_Endoof":
                    return RoomType.EzVent;
                case "EZ_Intercom":
                    return RoomType.EzIntercom;
                case "EZ_GateA":
                    return RoomType.EzGateA;
                case "EZ_PCs_small":
                    return RoomType.EzDownstairsPcs;
                case "EZ_Curve":
                    return RoomType.EzCurve;
                case "EZ_PCs":
                    return RoomType.EzPcs;
                case "EZ_Crossing":
                    return RoomType.EzCrossing;
                case "EZ_CollapsedTunnel":
                    return RoomType.EzCollapsedTunnel;
                case "EZ_Smallrooms2":
                    return RoomType.EzConference;
                case "EZ_Straight":
                    return RoomType.EzStraight;
                case "EZ_Cafeteria":
                    return RoomType.EzCafeteria;
                case "EZ_upstairs":
                    return RoomType.EzUpstairsPcs;
                case "EZ_GateB":
                    return RoomType.EzGateB;
                case "EZ_Shelter":
                    return RoomType.EzShelter;
                case "Root_*&*Outside Cams":
                    return RoomType.Surface;
                default:
                    return RoomType.Unknown;
            }
        }

        private List<Door> FindDoors()
        {
            List<Door> doorList = new List<Door>();
            foreach (Scp079Interactable scp079Interactable in Interface079.singleton.allInteractables)
            {
                foreach (Scp079Interactable.ZoneAndRoom zoneAndRoom in scp079Interactable.currentZonesAndRooms)
                {
                    if (zoneAndRoom.currentRoom == Name && zoneAndRoom.currentZone == Transform.parent.name)
                    {
                        if (scp079Interactable.type == Scp079Interactable.InteractableType.Door)
                        {
                            Door door = scp079Interactable.GetComponent<Door>();
                            if (door != null && !doorList.Contains(door))
                            {
                                doorList.Add(door);
                            }
                        }
                    }
                }
            }
            return doorList;
        }
    }
}

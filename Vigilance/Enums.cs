namespace Vigilance.Enums
{
	public enum DamageType
	{
		None,
		Lure,
		Nuke,
		Wall,
		Decontamination,
		Tesla,
		Falldown,
		AntiCheat,
		Containment,
		PocketDimension,
		Ragdolless,
		COM15,
		P90,
		Epsilon11,
		MP7,
		MicroHID,
		Logicer,
		USP,
		FragGrenade,
		Scp049,
		Scp0492,
		Scp096,
		Scp106,
		Scp173,
		Scp939,
		Asphyxiation,
		Bleeding,
		Poison,
		Scp207,
		Recontainment,
		FriendlyFireDetector,
		Disconnect,
		CmdSuicide
	}

	public enum DurationType
	{
		Seconds,
		Minutes,
		Hours,
		Days,
		Months,
		Years
	}

	public enum GrenadeType
	{
		FragGrenade = 25,
		FlashGrenade = 26,
		Scp018 = 31,
		None = 0
	}

	public enum UserIdType
	{
		Steam,
		Discord,
		Northwood,
		Patreon,
		Unspecified
	}

	public enum TeamType
	{
		ChaosInsurgency,
		NineTailedFox,
		ClassDPersonnel,
		Scientist,
		SCP,
		Spectator,
		Tutorial,
		None
	}

	public enum ZoneType
	{
		Surface,
		Entrance,
		HeavyContainment,
		LightContainment,
		PocketDimension,
		Unspecified
	}

    public enum RoomType
    {
        Unknown,
        LczArmory,
        LczCurve,
        LczStraight,
        Lcz012,
        Lcz914,
        LczCrossing,
        LczTCross,
        LczCafe,
        LczPlants,
        LczToilets,
        LczAirlock,
        Lcz173,
        LczClassDSpawn,
        LczCheckpointB,
        LczGlassBox,
        LczCheckpointA,
        Hcz079,
        HczEzCheckpoint,
        HczArmory,
        Hcz939,
        HczHid,
        Hcz049,
        HczCheckpointA,
        HczCrossing,
        Hcz106,
        HczNuke,
        HczTesla,
        HczServers,
        HczCheckpointB,
        HczTCross,
        HczCurve,
        Hcz096,
        EzVent,
        EzIntercom,
        EzGateA,
        EzDownstairsPcs,
        EzCurve,
        EzPcs,
        EzCrossing,
        EzCollapsedTunnel,
        EzConference,
        EzStraight,
        EzCafeteria,
        EzUpstairsPcs,
        EzGateB,
        EzShelter,
        Surface,
		PocketDimension
    }

    public enum RespawnEffectType : byte
	{
		PlayChaosInsurgencyMusic = 0,
		SummonChaosInsurgencyVan = 128,
		SummonNtfChopper = 129,
	}

	public enum WeaponType
    {
		Epsilon11,
		Project90,
		Logicer,
		USP,
		Com15,
		MicroHID,
		MP7,
		None
    }

	public enum AmmoType
	{
		Nato_5mm,
		Nato_7mm,
		Nato_9mm,
		None
	}

	public enum Prefab
    {
		Player,
		PlaybackLobby,
		Pickup,
		WorkStation,
		Ragdoll_0,
		Ragdoll_1,
		Ragdoll_3,
		Ragdoll_4,
		Ragdoll_6,
		Ragdoll_7,
		Ragdoll_8,
		Scp096_Ragdoll,
		Ragdoll_10,
		Ragdoll_14,
		Ragdoll_16,
		Ragdoll_17,
		GrenadeFlash,
		GrenadeFrag,
		GrenadeScp018,
		None
    }

	public enum RoundState
    {
		WaitingForPlayers,
		Started,
		Ended,
		ShowingSummary,
		Restarting,
		Undefined
    }

	public enum Achievement
    {
		LarryIsYourFriend,
		ThatWasClose,
		TablesHaveTurned,
		CrisisAverted,
		DidntEvenFeelThat,
		WowReally,
		UnvoluntaryRageQuit,
		Newb,
		FirstTime,
		IWantToBeARocket,
		Betrayal,
		PewPew,
		JustResources,
		IllPassThanks,
		TimeToDoItMyself,
		Friendship,
		Unknown
    }

	public enum Scp079Interaction : int
    {
		Tesla = 2,
		Door = 1,
		Camera = 0,
		ElevatorTeleport = 5,
		ElevatorUse = 7,
		Light = 3,
		LightController = 8,
		Lockdown = 6,
		Speaker = 4,
    }

	public enum Sight
    {
		None,
		RedDot,
		BlueDot,
		Holo,
		NightVision,
		SniperScope,
		Collimator
    }

	public enum Barrel
    {
		None,
		Suppressor,
		Silencer,
		HeavyBarrel,
		MuzzleBrake,
		MuzzleBooster
    }

	public enum Other
    {
		None,
		Flashlight,
		Laser,
		AmmoCounter,
		GyroscopicStabilizer
	}

	public enum WarheadLeverStatus
    {
		Enabled,
		Disabled
    }

    public enum DoorType
    {
        Scp012,
        Scp012Bottom,
        Scp012Locker,
        Scp049Armory,
        Scp079First,
        Scp079Second,
        Scp096,
        Scp106Bottom,
        Scp106Primary,
        Scp106Secondary,
        Scp173,
        Scp173Armory,
        Scp173Bottom,
        Scp372,
        Scp914,
        Airlocks,
        CheckpointEntrance,
        CheckpointLczA,
        CheckpointLczB,
        ContDoor,
        EntranceDoor,
        Escape,
        EscapeInner,
        GateA,
        GateB,
        HczArmory,
        HeavyContainmentDoor,
        HID,
        HIDLeft,
        HIDRight,
        Intercom,
        LczArmory,
        LczCafe,
        LczWc,
        LightContainmentDoor,
        NukeArmory,
        NukeSurface,
        PrisonDoor,
        SurfaceGate,
        UnknownDoor
    }

    public enum CameraType
    {
        Unknown = 0,
        LczClassDSpawn = 55,
        LczGlassBox = 92,
        Lcz173Hallway = 11,
        Lcz173Armory = 12,
        Lcz173Containment = 13,
        Lcz173Bottom = 14,
        Lcz012 = 54,
        Lcz012Bottom = 53,
        LczCafe = 59,
        LczArmory = 81,
        LczPlants = 82,
        WC = 20,
        Lcz914 = 36,
        Lcz914Hallway = 35,
        LczALight = 75,
        LczBLight = 17,
        LczAChkp = 74,
        LczBChkp = 18,
        HczAChkp = 76,
        HczBChkp = 16,
        LczAHeavy = 60,
        LczBHeavy = 91,
        Hcz079PreHallway = 77,
        Hcz079Hallway = 78,
        Hcz079Interior = 198,
        Hcz096 = 61,
        Hcz049Elevator = 67,
        Hcz049Hall = 69,
        Hcz049Armory = 64,
        HczServerHall = 83,
        HczServerBottom = 84,
        HczServerTop = 85,
        HczHidHall = 71,
        HczHidInterior = 72,
        HczWarheadHall = 90,
        HczWarheadRoom = 89,
        HczWarheadSwitch = 87,
        HczWarheadArmory = 88,
        Hcz939 = 79,
        HczArmory = 38,
        Hcz106First = 46,
        Hcz106Second = 43,
        Hcz106Primary = 45,
        Hcz106Secondary = 44,
        Hcz106Recontainer = 47,
        Hcz106Stairs = 48,
        HczChkpEz = 99,
        EzChkpHcz = 100,
        EzIntercomHall = 50,
        EzIntercomStairs = 52,
        EzIntercomInterior = 49,
        EzGateA = 62,
        EzGateB = 10,
        GateA = 27,
        Bridge = 32,
        Tower = 28,
        Backstreet = 29,
        SurfaceGate = 30,
        Streetcam = 33,
        Helipad = 26,
        EscapeZone = 25,
        Exit = 31,
    }

    public enum DoorProperties
    {
		IsPryable,
		IsBreakable,
		Portalless,
		RequiresKeycard,
		Unsecured
    }
}
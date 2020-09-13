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
		FragGrenade,
		FlashGrenade,
		Scp018,
		None
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
        Surface
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
}
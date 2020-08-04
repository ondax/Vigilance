﻿namespace Vigilance.API.Enums
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
		Scp018
	}

	public enum TeamType
	{
		ChaosInsurgency,
		NineTailedFox,
		ClassDPersonnel,
		Scientist,
		SCP,
		Spectator,
		Tutorial
	}

	public enum ZoneType
	{
		Surface,
		Entrance,
		HeavyContainment,
		LightContainment,
		Unspecified
	}

	public enum PostType
	{
		Sitrep,
		Ban,
		Report,
		RemoteAdmin
	}

	public enum RespawnEffectType : byte
	{
		PlayChaosInsurgencyMusic = 0,
		SummonChaosInsurgencyVan = 128,
		SummonNtfChopper = 129,
	}

	public enum SitrepEventType
	{
		PlayerDeath,
		PlayerLure,
		PlayerJoin,
		PlayerLeave,
		PlayerSpawn,

		ServerBan,
		ServerReport,
		ServerCommand,

		RoundEnd,
		RoundStart,
		RoundWaiting,
		RoundRespawn,


		EnvDecontaminate,
		EnvDetonate,
		EnvStart,
		EnvStop,

		None
	}
}

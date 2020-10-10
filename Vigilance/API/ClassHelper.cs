using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;
using System.Linq;
using Vigilance.Extensions;

namespace Vigilance.API
{
    public static class ClassHelper
    {
        public static Class[] Classes { get; set; }
        public static bool ClassesSet { get; set; }

        public static void SetClasses()
        {
            if (ClassesSet)
                return;
            List<Class> classes = new List<Class>();
            foreach (Role role in CharacterClassManager._staticClasses)
                classes.Add(Build(role.roleId));
            Classes = classes.ToArray();
            ClassesSet = true;
        }

        public static Class Get(RoleType role) => Classes[(int)role];
        public static Class Get(Role role) => Classes[(int)role.roleId];

        public static Class Build(RoleType type)
        {
            Role role = CharacterClassManager._staticClasses.SafeGet(type);
            return new Class(role.roleId, role.fullName, role.description, role.team, role.banClass, role.abilities, role.startItems, role.ammoTypes, role.maxAmmo, role.maxHP, role.bloodType, role.classColor, role.classRecoil, role.forcedCrosshair, role.jumpSpeed, role.runSpeed, role.walkSpeed, role.iconHeightOffset, role.useHeadBob, role.model_offset, role.ragdoll_offset, role.model_player, role.model_ragdoll, role.postprocessingProfile, role.profileSprite, role.stepClips);
        }

        public static RoundSummary.SumInfo_ClassList Build()
        {
            RoundSummary.SumInfo_ClassList classList = new RoundSummary.SumInfo_ClassList()
            {
                chaos_insurgents = Round.Info.ChaosInsurgents,
                class_ds = Round.Info.Class_Ds,
                mtf_and_guards = Round.Info.MTF,
                scientists = Round.Info.Scientists,
                scps_except_zombies = Round.Info.TotalSCPsExceptZombies,
                time = Round.Info.Seconds,
                warhead_kills = Round.Info.WarheadKills,
                zombies = Round.Info.ChangedToZombies
            };
            return classList;
        }
    }

    public class Class
    {
        private Role _role;
        public string Name { get; set; }
        public string Description { get; set; }
        public RoleType RoleId { get; set; }
        public Team TeamId { get; set; }
        public bool Banned { get; set; }
        public List<Ability> Abilities { get; set; }
        public List<ItemType> StartingItems { get; set; }
        public uint[] AmmoTypes { get; set; }
        public uint[] MaxAmmo { get; set; }
        public int MaxHealth { get; set; }
        public int BloodType { get; set; }
        public Color Color { get; set; }
        public float Recoil { get; set; }
        public int Crosshair { get; set; }
        public float JumpSpeed { get; set; }
        public float RunSpeed { get; set; }
        public float WalkSpeed { get; set; }
        public float IconHeight { get; set; }
        public bool Headbob { get; set; }
        public Offset ModelOffset { get; set; }
        public Offset RagdollOffset { get; set; }
        public GameObject PlayerModel { get; set; }
        public GameObject RagdollModel { get; set; }
        public PostProcessingProfile PostProcessing { get; set; }
        public Sprite ProfileSprite { get; set; }
        public AudioClip[] StepClips { get; set; }
        public Role Role => _role;

        public Class(RoleType id, string name, string description, Team team, bool ban, List<Ability> abilities, ItemType[] startingItems, uint[] ammoTypes, uint[] ammo, int maxHp, int bloodType, Color color, float recoil, int crosshair, float jumpSpeed, float runSpeed, float walkSpeed, float iconHeight, bool useHeadbob, Offset modelOffset, Offset ragdollOffset, GameObject playerModel, GameObject ragdollModel, PostProcessingProfile ppp, Sprite sprite, AudioClip[] clips)
        {
            _role = CharacterClassManager._staticClasses.SafeGet(id);
            Name = name;
            Description = description;
            RoleId = id;
            TeamId = team;
            Banned = ban;
            Abilities = abilities;
            StartingItems = startingItems.ToList();
            AmmoTypes = ammoTypes;
            MaxAmmo = ammo;
            MaxHealth = maxHp;
            BloodType = bloodType;
            Color = color;
            Recoil = recoil;
            Crosshair = crosshair;
            JumpSpeed = jumpSpeed;
            RunSpeed = runSpeed;
            WalkSpeed = walkSpeed;
            IconHeight = iconHeight;
            Headbob = useHeadbob;
            ModelOffset = modelOffset;
            RagdollOffset = ragdollOffset;
            PlayerModel = playerModel;
            RagdollModel = ragdollModel;
            PostProcessing = ppp;
            ProfileSprite = sprite;
            StepClips = clips;
        }

        public void SetName(string name)
        {
            Name = name;
            _role.fullName = name;
            _role.nickname = name;
            Replace();
        }

        public void SetDescription(string description)
        {
            Description = description;
            _role.description = description;
            Replace();
        }

        public void SetRoleId(RoleType newRole)
        {
            RoleId = newRole;
            _role.roleId = newRole;
            Replace();
        }

        public void SetBan(bool value)
        {
            Banned = value;
            _role.banClass = value;
            Replace();
        }

        public void SetAbilities(List<Ability> newAbilities)
        {
            Abilities = newAbilities;
            _role.abilities = newAbilities;
            Replace();
        }

        public void SetItems(List<ItemType> newItems)
        {
            StartingItems = newItems;
            _role.startItems = newItems.ToArray();
            Replace();
        }

        public void SetAmmoTypes(uint[] newTypes)
        {
            AmmoTypes = newTypes;
            _role.ammoTypes = newTypes;
            Replace();
        }

        public void SetMaxAmmo(uint[] newMax)
        {
            MaxAmmo = newMax;
            _role.maxAmmo = newMax;
            Replace();
        }

        public void SetMaxHealth(int newMax)
        {
            MaxHealth = newMax;
            _role.maxHP = newMax;
            Replace();
        }

        public void SetBloodType(int newType)
        {
            BloodType = newType;
            _role.bloodType = newType;
            Replace();
        }

        public void SetColor(Color color)
        {
            Color = color;
            _role.classColor = color;
            Replace();
        }

        public void SetRecoil(float recoil)
        {
            Recoil = recoil;
            _role.classRecoil = recoil;
            Replace();
        }

        public void SetCrosshair(int crosshair)
        {
            Crosshair = crosshair;
            _role.forcedCrosshair = crosshair;
            Replace();
        }

        public void SetJumpSpeed(float speed)
        {
            JumpSpeed = speed;
            _role.jumpSpeed = speed;
            Replace();
        }

        public void SetRunSpeed(float speed)
        {
            RunSpeed = speed;
            _role.runSpeed = speed;
            Replace();
        }

        public void SetWalkSpeed(float speed)
        {
            WalkSpeed = speed;
            _role.walkSpeed = speed;
            Replace();
        }

        public void SetIconHeight(float height)
        {
            IconHeight = height;
            _role.iconHeightOffset = height;
            Replace();
        }

        public void SetHeadbob(bool value)
        {
            Headbob = value;
            _role.useHeadBob = value;
            Replace();
        }

        public void SetModelOffset(Offset offset)
        {
            ModelOffset = offset;
            _role.model_offset = offset;
            Replace();
        }

        public void SetRagdollOffset(Offset offset)
        {
            RagdollOffset = offset;
            _role.ragdoll_offset = offset;
            Replace();
        }

        public void SetPlayerModel(GameObject model)
        {
            PlayerModel = model;
            _role.model_player = model;
            Replace();
        }

        public void SetRagdollModel(GameObject model)
        {
            RagdollModel = model;
            _role.model_ragdoll = model;
            Replace();
        }

        public void SetPostProcessingProfile(PostProcessingProfile ppp)
        {
            PostProcessing = ppp;
            _role.postprocessingProfile = ppp;
            Replace();
        }

        public void SetProfileSprite(Sprite sprite)
        {
            ProfileSprite = sprite;
            _role.profileSprite = sprite;
            Replace();
        }

        public void SetStepClips(AudioClip[] clips)
        {
            StepClips = clips;
            _role.stepClips = clips;
            Replace();
        }

        public void Replace()
        {
            try
            {
                Role[] current = CharacterClassManager._staticClasses;
                List<Role> list = new List<Role>();
                for (int i = 0; i < current.Length; i++)
                {
                    Role role = current[i];
                    if (role.roleId == RoleId)
                    {
                        list.Add(_role);
                    }
                    else
                    {
                        list.Add(role);
                    }
                }
                Role[] newRoles = list.ToArray();
                CharacterClassManager._staticClasses = newRoles;
                foreach (CharacterClassManager ccm in Object.FindObjectsOfType<CharacterClassManager>())
                    ccm.Classes = newRoles;
            }
            catch (System.Exception e)
            {
                Log.Add("ClassHelper", e);
            }
        }

        public Role ToRole() => _role;

        public override string ToString()
        {
            return RoleId.AsString();
        }
    }
}

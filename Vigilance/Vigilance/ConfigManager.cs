using GameCore;
using System;
using System.Collections.Generic;
using Vigilance.API.Enums;

namespace Vigilance
{
    public static class ConfigManager
    {
        public static string GetString(string key) => ConfigFile.ServerConfig.GetString(key);
        public static List<string> GetStringList(string key) => ConfigFile.ServerConfig.GetStringList(key);
        public static int GetInt(string key) => ConfigFile.ServerConfig.GetInt(key);
        public static List<int> GetIntList(string key) => ConfigFile.ServerConfig.GetIntList(key);
        public static bool GetBool(string key)
        {
            string value = GetString(key).ToLower();
            if (value == "true" || value == "enabled" || value == "enable" || value == "1" || value == "t" || value == "e")
                return true;
            else
                return false;
        }
        public static List<bool> GetBoolList(string key) => ConfigFile.ServerConfig.GetBoolList(key);
        public static float GetFloat(string key) => ConfigFile.ServerConfig.GetFloat(key);
        public static List<float> GetFloatList(string key) => ConfigFile.ServerConfig.GetFloatList(key);
        public static ItemType GetItem(string key) => (ItemType)Enum.Parse(typeof(ItemType), ConfigManager.GetString(key));
        public static List<ItemType> GetItems(string key)
        {
            List<ItemType> items = new List<ItemType>();
            foreach (int val in GetIntList(key))
            {
                items.Add((ItemType)Enum.Parse(typeof(ItemType), val.ToString()));
            }
            return items;
        }

        public static RoleType GetRole(string key) => (RoleType)Enum.Parse(typeof(RoleType), GetString(key));
        public static List<RoleType> GetRoles(string key)
        {
            List<RoleType> roles = new List<RoleType>();
            foreach (int val in GetIntList(key))
            {
                roles.Add((RoleType)Enum.Parse(typeof(RoleType), val.ToString()));
            }
            return roles;
        }

        public static TeamType GetTeam(string key) => (TeamType)Enum.Parse(typeof(TeamType), GetString(key));
        public static List<TeamType> GetTeams(string key)
        {
            List<TeamType> teams = new List<TeamType>();
            foreach (int val in GetIntList(key))
            {
                teams.Add((TeamType)Enum.Parse(typeof(TeamType), val.ToString()));
            }
            return teams;
        }
    }
}

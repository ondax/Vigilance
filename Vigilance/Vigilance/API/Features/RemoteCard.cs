using System.Collections.Generic;

namespace Vigilance.API.Features
{
    public class RemoteCard
    {
        public bool Enabled => ConfigManager.GetBool("enable_remote_card");

        public RemoteCard()
        { }

        public bool CheckPermission(Player player, Door door)
        {
            if (!Enabled)
                return false;
            foreach (Inventory.SyncItemInfo itemInfo in player.Inventory.items)
            { 
                foreach (string perm in player.Inventory.GetItemByID(itemInfo.id).permissions)
                {
                    if (perm.ToUpper() == door.permissionLevel.ToUpper())
                        return true;
                    if (Door.backwardsCompatPermissions.TryGetValue(perm, out Door.AccessRequirements access) && door.PermissionLevels.HasPermission(access))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool CheckPermission(Player player, Generator079 generator = null)
        {
            if (!Enabled)
                return false;
            foreach (Inventory.SyncItemInfo itemInfo in player.SyncItems)
            {
                foreach (string perm in ConfigManager.GetStringList("generator_permissions"))
                {
                    if (player.GetItem(itemInfo.id).permissions.Contains(perm))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool CheckPermission(Player player, AlphaWarheadOutsitePanel panel = null)
        {
            if (!Enabled)
                return false;
            foreach (Inventory.SyncItemInfo itemInfo in player.SyncItems)
            {
                if (player.GetItem(itemInfo.id).permissions.Contains("CONT_LVL_3"))
                {
                    return true;
                }
            }
            return false;
        }
    }
}

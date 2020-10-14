using Vigilance.Enums;
using Vigilance.Extensions;

namespace Vigilance.API
{
    public class WeaponInfo
    {
        private Player _player;
        private Inventory.SyncItemInfo _item;

        public WeaponType Type { get => _item.id.GetWeaponType(); }
        public int Ammo { get => _player.GetAmmo(Type.GetWeaponAmmoType()); set => _player.SetAmmo(Type.GetWeaponAmmoType(), value); }
        public int Barrel { get => _item.modBarrel; set => _item.modBarrel = value; }
        public int Sight { get => _item.modSight; set => _item.modSight = value; }
        public int Other { get => _item.modOther; set => _item.modOther = value; }

        public void Reload() => _player.Hub.weaponManager.CallCmdReload(false);
        public void TakeAmmo(int ammoToTake) => Ammo -= ammoToTake;
        public void AddAmmo(int ammoToAdd) => Ammo += ammoToAdd;
        public void EmptyClip() => _player.Hub.weaponManager.CallCmdEmptyClip();
 
        public WeaponInfo(Player player, Inventory.SyncItemInfo item)
        {
            _player = player;
            _item = item;
        }
    }
}

using UnityEngine;

namespace Weapons
{
    public interface IWeaponFirerer
    {
        public void Fire(Vector3 direction, WeaponInfo weaponInfo);
    }
}

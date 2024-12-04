using Car;
using Destructors;
using UnityEngine;

// "Gun Weapon" refers to any weapon that shoots a hitscan bullet forward.
// The most basic example would be the gear gun.
// No implementation for aiming

// the way that a weapon fires is dependent upon the "firerer" that it is given.
// WeaponFirerer is an interface that declares one method: "void Fire(Vector3 direction)"

namespace Weapons
{
    public class GunWeapon : MonoBehaviour
    {
        // stats and other information about the weapon
        public WeaponInfo weaponInfo;

        // dependency injection my beloved
        public IWeaponFirerer Firerer;

        // the part of the weapon that determines where the projectile will be shot.
        // whichever direction this object is facing, the projectile will be shot that direction.

        public Transform fireDirector;

        float cooldown = 0;
        // Grabs AudioClip
        private AudioSource gunShotSource;

        private GameObject audioObject;

        [SerializeField] CarStatus _carStatus;

        void Start()
        {
            //TODO: ADD AUDIO SOURCE
            if (!_carStatus)
                _carStatus = transform.parent.GetComponent<CarDestruction>().carSystem.carStatus;
        }

        void Update()
        {
            cooldown += Time.deltaTime;
        }

        public virtual void Fire()
        {
            if (!_carStatus.canFire) return;

            print("it can fire!");

            if (Firerer == null)
                Firerer = GetComponent<IWeaponFirerer>();
            if (cooldown > 1 / weaponInfo.fireRate)
            {
                print("Fire!");
                cooldown = 0;
                print(fireDirector);
                print(weaponInfo);
                print(Firerer);
                Firerer.Fire(fireDirector.forward, weaponInfo);
            }
        }
    }
}
